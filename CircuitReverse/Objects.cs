using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;

namespace CircuitReverse
{
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

	public delegate Point PanelTransform(RelativePoint p);

	public enum LayerEnum
	{
		NONE,
		TOP,
		BOTTOM,
		BOTH
	}

	public enum ObjectType
	{
		NONE,
		LINE
	}

	// Return type to show if the active tool needs to be deleted
	public enum ToolAction
	{
		NONE,
		RESET,
		ABORT,
		EXIT
	}

	// Parent class for objects to organize into list
	public abstract class AbstractObject
	{
		public ObjectType type = ObjectType.NONE;

		public LayerEnum layer = LayerEnum.BOTH;

		public AbstractObject(LayerEnum l)
		{
			layer = l;
		}

		public AbstractObject(AbstractObject o)
		{
			layer = o.layer;
		}

		public abstract void DrawObject(LayerEnum target_layer, PanelTransform tform, Graphics g, bool selected = false);
		public abstract string ExportObject();
		public abstract List<CustomProperty> GetProperties();
		public abstract void ChangeProperty(CustomPropertyDescriptor property);

		public static AbstractObject ImportObject(string descriptor)
		{
			var strarr = descriptor.Split(' ');
			if (strarr.Length > 0)
			{
				if (strarr[0] == "WIRE")
				{
					// Format: WIRE L{layer} '{netname}' #{color} {point1} [{point2}] ...
					var l = (LayerEnum)Enum.Parse(typeof(LayerEnum), strarr[1].Substring(1));
					var ret = new WireObject(l);

					ret.NetName = strarr[2].Substring(1, strarr[2].Length - 2);
					ret.WireColor = Color.FromArgb(int.Parse(strarr[3].Substring(1), NumberStyles.HexNumber));

					for (int i = 4; i < strarr.Length; i++)
					{
						var p = new RelativePoint(strarr[i]);
						ret.WirePoints.Add(p);
					}

					return ret;
				}
				else if (strarr[0] == "PIN")
				{
					// Format: PIN L{layer} {component}:{number} '{netname}' #{color} {location}
					var l = (LayerEnum)Enum.Parse(typeof(LayerEnum), strarr[1].Substring(1));
					var ret = new PinObject(l);

					var properties = strarr[2].Split(':');
					ret.Component = properties[0];
					ret.Number = properties[1];

					ret.NetName = strarr[3].Substring(1, strarr[3].Length - 2);
					ret.PinColor = Color.FromArgb(int.Parse(strarr[4].Substring(1), NumberStyles.HexNumber));
					ret.Location = new RelativePoint(strarr[5]);

					return ret;
				}
			}
			return null;
		}
	}

	public class WireObject : AbstractObject
	{
		public Color WireColor = Color.Red;
		public string NetName = "";

		public List<RelativePoint> WirePoints = new List<RelativePoint>();

		public RelativePoint ActivePoint = new RelativePoint(0, 0);
		public bool ShowActivePoint = false;

		public WireObject(LayerEnum l = LayerEnum.BOTH) : base(l)
		{
			type = ObjectType.LINE;
		}

		public WireObject(WireObject w) : base(w)
		{
			type = ObjectType.LINE;
			WireColor = w.WireColor;
			NetName = w.NetName;
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
				using (var p = new Pen(Color.FromArgb(180, WireColor), 8))
				{
					g.DrawLine(p, tform(p1), tform(p2));
				}
			}

			using (var p = new Pen(WireColor, 4))
			{
				g.DrawLine(p, tform(p1), tform(p2));
			}
		}

		public override void DrawObject(LayerEnum target_layer, PanelTransform tform, Graphics g, bool selected = false)
		{
			if (layer != LayerEnum.BOTH && target_layer != layer)
			{
				// no drawing on this layer
				return;
			}

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
			string exp = string.Format("WIRE L{0} '{1}' #{2}", layer.ToString(), NetName, WireColor.ToArgb().ToString("X8"));
			foreach (var p in WirePoints)
			{
				exp += " " + p.ToString();
			}
			return exp;
		}

		public override string ToString()
		{
			return string.Format("WIRE : Net {0} : {1}", NetName, WireColor.ToKnownColor().ToString());
		}

		public override List<CustomProperty> GetProperties()
		{
			var props = new List<CustomProperty>();
			props.Add(new CustomProperty("Net", NetName));
			props.Add(new CustomProperty("Color", WireColor));
			props.Add(new CustomProperty("Layer", layer));
			return props;
		}

		public override void ChangeProperty(CustomPropertyDescriptor property)
		{
			if (property.Description == "Net")
			{
				NetName = property.Value as string;
			}
			if (property.Description == "Color")
			{
				WireColor = (Color)property.Value;
			}
			if (property.Description == "Layer")
			{
				layer = (LayerEnum)property.Value;
			}
		}
	}

	public class PinObject : AbstractObject
	{
		public Color PinColor = Color.Blue;
		public string NetName = "";
		public string Component = "";
		public string Number = "0";
		public RelativePoint Location;

		public PinObject(LayerEnum l) : base(l)
		{
			Location = new RelativePoint();
		}

		public PinObject(PinObject o) : base(o)
		{
			PinColor = o.PinColor;
			NetName = o.NetName;
			Number = o.Number;
			Location = o.Location;
		}

		// Draw pin on panel
		public override void DrawObject(LayerEnum target_layer, PanelTransform tform, Graphics g, bool selected = false)
		{
			if (layer != LayerEnum.BOTH && target_layer != layer)
			{
				// no drawing on this layer
				return;
			}

			var loc = tform(new RelativePoint(Location.X, Location.Y));

			if (selected)
			{
				using (var p = new Pen(Color.FromArgb(180, PinColor), 6))
				{
					const int hs = 7;
					g.DrawRectangle(p, loc.X - hs, loc.Y - hs, hs * 2, hs * 2);
				}
			}

			using (var p = new Pen(PinColor, 2))
			{
				const int hs = 5;
				g.DrawRectangle(p, loc.X - hs, loc.Y - hs, hs * 2, hs * 2);
			}
		}

		public override string ExportObject()
		{
			return string.Format("PIN L{0} {1}:{2} '{3}' #{4} {5}", layer.ToString(), Component, Number, NetName, PinColor.ToArgb().ToString("X8"), Location.ToString());
		}

		public override string ToString()
		{
			return string.Format("PIN : {0}.{1} : Net {2} : {3}", Component, Number, NetName, PinColor.ToKnownColor().ToString());
		}

		public override List<CustomProperty> GetProperties()
		{
			var props = new List<CustomProperty>();
			props.Add(new CustomProperty("Net", NetName));
			props.Add(new CustomProperty("Color", PinColor));
			props.Add(new CustomProperty("Layer", layer));
			props.Add(new CustomProperty("Component", Component, "Component"));
			props.Add(new CustomProperty("Number", Number, "Component"));
			return props;
		}

		public override void ChangeProperty(CustomPropertyDescriptor property)
		{
			if (property.Description == "Net")
			{
				NetName = property.Value as string;
			}
			if (property.Description == "Color")
			{
				PinColor = (Color)property.Value;
			}
			if (property.Description == "Layer")
			{
				layer = (LayerEnum)property.Value;
			}
			if (property.Description == "Component")
			{
				Component = property.Value as string;
			}
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

		public AbstractTool(int layer = 0)
		{
			// interpret layer selection dropdown menu
			if (layer == 0)
			{
				ActiveLayer = LayerEnum.TOP;
			}
			else if (layer == 1)
			{
				ActiveLayer = LayerEnum.BOTTOM;
			}
			else
			{
				ActiveLayer = LayerEnum.BOTH;
			}
		}

		public abstract AbstractObject ResetAndGetObject();
		public abstract ToolAction ClickHandler(MouseEventArgs e);
		public abstract void MoveHandler(RelativePoint p);
		public abstract void PaintHandler(LayerEnum target_layer, PanelTransform tform, Graphics g);
		public abstract void MouseFocusHandler(bool hover);
		public abstract void KeyHandler(Keys key);
	}

	public class WireTool : AbstractTool
	{
		public WireObject wire;

		public WireTool(int layer = 0, bool mouseover = false) : base(layer)
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

		public PinTool(int layer = 0, bool mouseover = false) : base(layer)
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
