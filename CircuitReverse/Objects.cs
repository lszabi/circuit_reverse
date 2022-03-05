using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;

namespace CircuitReverse
{
	public delegate Point PanelTransform(RelativePoint p);

	public struct Crosshair
	{
		public RelativePoint location;
		public bool show;
	}

	public struct RelativePoint
	{
		public double X, Y;

		public RelativePoint(double x, double y)
		{
			X = x;
			Y = y;
		}

		public RelativePoint(RelativePoint p)
		{
			X = p.X;
			Y = p.Y;
		}

		public RelativePoint(string str)
		{
			var values = str.Substring(1, str.Length - 2).Split(';');
			X = double.Parse(values[0], CultureInfo.InvariantCulture);
			Y = double.Parse(values[1], CultureInfo.InvariantCulture);
		}

		public override string ToString()
		{
			return string.Format("({0};{1})", X.ToString(CultureInfo.InvariantCulture), Y.ToString(CultureInfo.InvariantCulture));
		}
	}

	public enum LayerEnum
	{
		NONE = 0,
		TOP = 1,
		BOTTOM = 2,
		BOTH = TOP | BOTTOM
	}

	// Return type to show if the active tool needs to be deleted
	public enum ToolAction
	{
		NONE,	// Do nothing
		RESET,	// Add the created object to the object list, and restart the tool to create another object
		ABORT,  // Drop the created object and exit the tool
		EXIT    // Add the created object to the object list, and exit the tool
	}

	// Parent class for objects to organize into list
	public abstract class AbstractObject
	{
		public LayerEnum layer = LayerEnum.BOTH;

		public string NetName = "";
		public string Component = "";
		public int Size = 1;
		public Color ObjectColor = Color.White;

		public AbstractObject(LayerEnum l)
		{
			layer = l;
		}

		public AbstractObject(AbstractObject o)
		{
			layer = o.layer;
			NetName = o.NetName;
			Component = o.Component;
			Size = o.Size;
			ObjectColor = o.ObjectColor;
		}

		public void DrawObject(LayerEnum target_layer, PanelTransform tform, Graphics g, bool selected = false)
		{
			if (layer != LayerEnum.BOTH && target_layer != layer)
			{
				// no drawing on this layer
				return;
			}

			DrawObjectGraphics(tform, g, selected);
		}

		// Draw object graphics on the image panel(s)
		public abstract void DrawObjectGraphics(PanelTransform tform, Graphics g, bool selected = false);

		// Save the serialized object text into the objects.txt file
		public abstract string ExportObject();

		// Get the properties of the object for the ObjectPropertyGrid
		public virtual List<CustomProperty> GetProperties()
		{
			var props = new List<CustomProperty>();
			props.Add(new CustomProperty("Net", NetName));
			props.Add(new CustomProperty("Layer", layer));
			props.Add(new CustomProperty("Size", Size));
			props.Add(new CustomProperty("Component", Component));
			props.Add(new CustomProperty("Color", ObjectColor));
			return props;
		}

		// Import the changed properties from the ObjectPropertyGrid
		public virtual void ChangeProperty(CustomPropertyDescriptor property)
		{
			if (property.Description == "Net")
			{
				NetName = property.Value as string;
			}
			if (property.Description == "Layer")
			{
				layer = (LayerEnum)property.Value;
			}
			if (property.Description == "Size")
			{
				Size = (int)property.Value;
			}
			if (property.Description == "Component")
			{
				Component = (string)property.Value;
			}
			if (property.Description == "Color")
			{
				ObjectColor = (Color)property.Value;
			}
		}

		// Load object from the objects.txt file
		public static AbstractObject ImportObject(string descriptor)
		{
			var strarr = descriptor.Split(' ');
			if (strarr.Length < 1)
			{
				return null;
			}

			if (strarr[0] == "WIRE")
			{
				// Format: WIRE L{layer} '{netname}' #{color} {point1} [{point2}] ...
				var l = (LayerEnum)Enum.Parse(typeof(LayerEnum), strarr[1].Substring(1));
				var ret = new WireObject(l);

				ret.NetName = strarr[2].Substring(1, strarr[2].Length - 2);
				ret.ObjectColor = Color.FromArgb(int.Parse(strarr[3].Substring(1), NumberStyles.HexNumber));

				for (int i = 4; i < strarr.Length; i++)
				{
					var p = new RelativePoint(strarr[i]);
					ret.WirePoints.Add(p);
				}

				return ret;
			}
			
			if (strarr[0] == "PIN")
			{
				// Format: PIN L{layer} {component}:{number} '{netname}' #{color} {location}
				var l = (LayerEnum)Enum.Parse(typeof(LayerEnum), strarr[1].Substring(1));
				var ret = new PinObject(l);

				var properties = strarr[2].Split(':');
				ret.Component = properties[0];
				ret.Number = properties[1];

				ret.NetName = strarr[3].Substring(1, strarr[3].Length - 2);
				ret.ObjectColor = Color.FromArgb(int.Parse(strarr[4].Substring(1), NumberStyles.HexNumber));
				ret.Location = new RelativePoint(strarr[5]);

				return ret;
			}

			return null;
		}
	}

	public class WireObject : AbstractObject
	{
		public List<RelativePoint> WirePoints = new List<RelativePoint>();

		public RelativePoint ActivePoint = new RelativePoint(0, 0);
		public bool ShowActivePoint = false;

		public WireObject(LayerEnum l = LayerEnum.BOTH) : base(l)
		{
			ObjectColor = Color.Red;
			Size = 4;
		}

		public WireObject(WireObject w) : base(w)
		{
			WirePoints = new List<RelativePoint>(w.WirePoints);
		}

		public void AddActivePoint()
		{
			WirePoints.Add(ActivePoint);
		}

		private void DrawLine(RelativePoint p1, RelativePoint p2, PanelTransform tform, Graphics g, bool selected)
		{
			if (selected)
			{
				// Highlight line
				using (var p = new Pen(Color.FromArgb(180, ObjectColor), 3 * Size / 2))
				{
					g.DrawLine(p, tform(p1), tform(p2));
				}
			}

			using (var p = new Pen(ObjectColor, Size))
			{
				g.DrawLine(p, tform(p1), tform(p2));
			}
		}

		public override void DrawObjectGraphics(PanelTransform tform, Graphics g, bool selected = false)
		{
			for (int i = 0; i < WirePoints.Count - 1; i++)
			{
				DrawLine(WirePoints[i], WirePoints[i + 1], tform, g, selected);
			}

			if (ShowActivePoint && WirePoints.Count > 0)
			{
				DrawLine(WirePoints[WirePoints.Count - 1], ActivePoint, tform, g, selected);
			}
		}

		public override string ExportObject()
		{
			string exp = string.Format("WIRE L{0} '{1}' #{2}", layer.ToString(), NetName, ObjectColor.ToArgb().ToString("X8"));
			foreach (var p in WirePoints)
			{
				exp += " " + p.ToString();
			}
			return exp;
		}

		public override string ToString()
		{
			return string.Format("WIRE : Net {0} : {1}", NetName, ObjectColor.ToKnownColor().ToString());
		}

		public override List<CustomProperty> GetProperties()
		{
			return base.GetProperties();
		}

		public override void ChangeProperty(CustomPropertyDescriptor property)
		{
			base.ChangeProperty(property);
		}
	}

	public class PinObject : AbstractObject
	{
		public string Number = "0";
		public RelativePoint Location = new RelativePoint();

		public PinObject(LayerEnum l) : base(l)
		{
			Size = 2;
			ObjectColor = Color.Blue;
		}

		public PinObject(PinObject o) : base(o)
		{
			Number = o.Number;
			Location = o.Location;
		}

		// Draw pin on panel
		public override void DrawObjectGraphics(PanelTransform tform, Graphics g, bool selected = false)
		{
			var loc = tform(new RelativePoint(Location.X, Location.Y));

			if (selected)
			{
				using (var p = new Pen(Color.FromArgb(180, ObjectColor), 3 * Size / 2))
				{
					const int hs = 7;
					g.DrawRectangle(p, loc.X - hs, loc.Y - hs, hs * 2, hs * 2);
				}
			}

			using (var p = new Pen(ObjectColor, Size))
			{
				const int hs = 5;
				g.DrawRectangle(p, loc.X - hs, loc.Y - hs, hs * 2, hs * 2);
			}
		}

		public override string ExportObject()
		{
			return string.Format("PIN L{0} {1}:{2} '{3}' #{4} {5}", layer.ToString(), Component, Number, NetName, ObjectColor.ToArgb().ToString("X8"), Location.ToString());
		}

		public override string ToString()
		{
			return string.Format("PIN : {0}.{1} : Net {2} : {3}", Component, Number, NetName, ObjectColor.ToKnownColor().ToString());
		}

		public override List<CustomProperty> GetProperties()
		{
			var props = base.GetProperties();
			props.Add(new CustomProperty("Number", Number, "Component"));
			return props;
		}

		public override void ChangeProperty(CustomPropertyDescriptor property)
		{
			base.ChangeProperty(property);
			if (property.Description == "Number")
			{
				Number = property.Value as string;
			}
		}
	}

	// Parent class for drawing commands to generalize in a variable
	public abstract class AbstractTool
	{
		public LayerEnum ActiveLayer = LayerEnum.NONE;

		public AbstractTool(LayerEnum layer = LayerEnum.NONE)
		{
			ActiveLayer = layer;
		}

		// Return created object and restart tool
		public abstract AbstractObject ResetAndGetObject();

		// Handle mouse clicks on the ImagePanel
		public abstract ToolAction ClickHandler(MouseEventArgs e);

		// Handle mouse movement on the ImagePanel
		public abstract void MoveHandler(RelativePoint p);

		// Handle mouse going in and out of the ImagePanel
		public abstract void MouseFocusHandler(bool hover);

		// Handle keyboard keypresses
		public abstract void KeyHandler(Keys key);

		// Paint the object in creation
		public abstract void PaintHandler(LayerEnum target_layer, PanelTransform tform, Graphics g);
	}

	public class SelectTool : AbstractTool
	{
		public SelectTool()
		{
		}

		public override AbstractObject ResetAndGetObject()
		{
			return null;
		}

		public override ToolAction ClickHandler(MouseEventArgs e)
		{
			return ToolAction.NONE;
		}

		public override void MoveHandler(RelativePoint p)
		{
		}

		public override void PaintHandler(LayerEnum target_layer, PanelTransform tform, Graphics g)
		{
		}

		public override void MouseFocusHandler(bool hover)
		{
		}

		public override void KeyHandler(Keys key)
		{
		}
	}

	public class WireTool : AbstractTool
	{
		public WireObject wire;

		public WireTool(LayerEnum layer, bool mouseover = false) : base(layer)
		{
			ResetAndGetObject();
			wire.ShowActivePoint = mouseover;
		}

		public override AbstractObject ResetAndGetObject()
		{
			// return this wire and create a new
			var tmp = wire;
			wire = new WireObject(ActiveLayer);

			if (!(tmp is null))
			{
				wire.ShowActivePoint = tmp.ShowActivePoint;
				tmp.ShowActivePoint = false;
			}

			return tmp;
		}

		public override ToolAction ClickHandler(MouseEventArgs e)
		{
			// left click adds node to wire
			if (e.Button == MouseButtons.Left)
			{
				wire.AddActivePoint();
			}

			// right click ends wire and begins a new one
			if (e.Button == MouseButtons.Right)
			{
				// exit tool if it has no points
				if (wire.WirePoints.Count == 0)
				{
					return ToolAction.ABORT;
				}

				// save wire if it has at least 2 points
				if (wire.WirePoints.Count >= 2)
				{
					return ToolAction.RESET;
				}

				// reset now if not, and return with no action
				ResetAndGetObject();
			}

			return ToolAction.NONE;
		}

		public override void MoveHandler(RelativePoint p)
		{
			wire.ActivePoint = p;
		}

		public override void PaintHandler(LayerEnum target_layer, PanelTransform tform, Graphics g)
		{
			wire.DrawObject(target_layer, tform, g);
		}

		public override void MouseFocusHandler(bool hover)
		{
			wire.ShowActivePoint = hover;
		}

		public override void KeyHandler(Keys key)
		{
			if (key == Keys.Back)
			{
				if (wire.WirePoints.Count > 0)
				{
					wire.WirePoints.RemoveAt(wire.WirePoints.Count - 1);
				}
			}
		}
	}

	public class PinTool : AbstractTool
	{
		PinObject pin;
		bool show = false;

		public PinTool(LayerEnum layer, bool mouseover = false) : base(layer)
		{
			pin = new PinObject(ActiveLayer);
			show = mouseover;
		}

		public override AbstractObject ResetAndGetObject()
		{
			return new PinObject(pin);
		}

		public override ToolAction ClickHandler(MouseEventArgs e)
		{
			// left click places pin
			if (e.Button == MouseButtons.Left)
			{
				return ToolAction.RESET;
			}

			// right click exits tool
			if (e.Button == MouseButtons.Right)
			{
				return ToolAction.ABORT;
			}

			return ToolAction.NONE;
		}

		public override void MoveHandler(RelativePoint p)
		{
			pin.Location = p;
		}

		public override void PaintHandler(LayerEnum target_layer, PanelTransform tform, Graphics g)
		{
			if (show)
			{
				pin.DrawObject(target_layer, tform, g);
			}
		}

		public override void MouseFocusHandler(bool hover)
		{
			show = hover;
		}

		public override void KeyHandler(Keys key)
		{
		}
	}
}
