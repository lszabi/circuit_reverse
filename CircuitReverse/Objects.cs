﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;
using System.Drawing.Drawing2D;

namespace CircuitReverse
{
	public delegate Point PanelTransform(PointF p);

	public struct Crosshair
	{
		public PointF location;
		public bool show;
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

		public readonly string ObjectType;

		public string NetName = "";
		public string Component = "";
		public int Size = 1;
		public Color ObjectColor = Color.White;

		public bool Selected = false;

		public AbstractObject(LayerEnum l, string t)
		{
			ObjectType = t;
			layer = l;
		}

		public AbstractObject(AbstractObject o)
		{
			ObjectType = o.ObjectType;
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

		public static PointF ParsePoint(string str)
		{
			var values = str.Substring(1, str.Length - 2).Split(';');
			float x = float.Parse(values[0], CultureInfo.InvariantCulture);
			float y = float.Parse(values[1], CultureInfo.InvariantCulture);
			return new PointF(x, y);
		}

		// Load object from the objects.txt file
		public static AbstractObject ImportObject(string descriptor)
		{
			// TODO rewrite to use CustomProperties
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
					var p = ParsePoint(strarr[i]);
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
				ret.Location = ParsePoint(strarr[5]);

				return ret;
			}

			if (strarr[0] == "TEXT")
			{
				// Format: TEXT L{layer} '{text}' {component} '{netname}' #{color} {location} {angle}
				var l = (LayerEnum)Enum.Parse(typeof(LayerEnum), strarr[1].Substring(1));
				var ret = new TextObject(l);

				ret.Text = strarr[2].Substring(1, strarr[2].Length - 2);
				ret.Component = strarr[3];

				ret.NetName = strarr[4].Substring(1, strarr[4].Length - 2);
				ret.ObjectColor = Color.FromArgb(int.Parse(strarr[5].Substring(1), NumberStyles.HexNumber));
				ret.Location = ParsePoint(strarr[6]);

				return ret;
			}
			return null;
		}
	}

	public class WireObject : AbstractObject
	{
		public List<PointF> WirePoints = new List<PointF>();

		public PointF ActivePoint = new PointF(0, 0);
		public bool ShowActivePoint = false;

		public WireObject(LayerEnum l = LayerEnum.BOTH) : base(l, "WIRE")
		{
			ObjectColor = Color.Red;
			Size = 4;
		}

		public WireObject(WireObject w) : base(w)
		{
			WirePoints = new List<PointF>(w.WirePoints);
		}

		public void AddActivePoint()
		{
			WirePoints.Add(ActivePoint);
		}

		private void DrawLine(PointF p1, PointF p2, PanelTransform tform, Graphics g, bool selected)
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
			return string.Format("WIRE : Net {0}", NetName);
		}

		public override List<CustomProperty> GetProperties()
		{
			var ret = base.GetProperties();
			ret.Add(new CustomProperty("Points", WirePoints, "Line"));
			return ret;
		}

		public override void ChangeProperty(CustomPropertyDescriptor property)
		{
			base.ChangeProperty(property);
		}
	}

	public class PinObject : AbstractObject
	{
		public string Number = "0";
		public PointF Location = new PointF();

		public PinObject(LayerEnum l) : base(l, "PIN")
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
			var loc = tform(Location);

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
			return string.Format("PIN : {0}.{1} : Net {2}", Component, Number, NetName);
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

	public class TextObject : AbstractObject
	{
		public string Text = "Text";
		public Font TextFont = new Font("Arial", 12);
		public float Angle = 0;
		public PointF Location = new PointF();

		public TextObject(LayerEnum l) : base(l, "TEXT")
		{
			ObjectColor = Color.Gold;
			Size = 12;
		}

		public TextObject(TextObject o) : base(o)
		{
			Text = o.Text;
			Location = o.Location;
		}

		public void SetAngle(float a)
		{
			while (a < 0)
			{
				a += 360;
			}
			while (a > 360)
			{
				a -= 360;
			}
			Angle = a;
		}

		public override void DrawObjectGraphics(PanelTransform tform, Graphics g, bool selected = false)
		{
			var loc = tform(Location);

			g.TranslateTransform(loc.X, loc.Y);
			g.RotateTransform(Angle);
			
			using (var p = new GraphicsPath())
			{
				p.AddString(Text, TextFont.FontFamily, (int)TextFont.Style, g.DpiY * TextFont.SizeInPoints / 72, new Point(0, 0), new StringFormat());
				g.FillPath(new SolidBrush(ObjectColor), p);
				if (selected)
				{
					p.Widen(new Pen(ObjectColor, 3));
					g.FillPath(new SolidBrush(Color.FromArgb(180, ObjectColor)), p);
				}
			}

			g.RotateTransform(-Angle);
			g.TranslateTransform(-loc.X, -loc.Y);
		}

		public override string ExportObject()
		{
			return string.Format("TEXT L{0} '{1}' {2} '{3}' #{4} {5} {6}", layer.ToString(), Text, Component, NetName, ObjectColor.ToArgb().ToString("X8"), Location.ToString(), 0);
		}

		public override string ToString()
		{
			return string.Format("TEXT : '{0}'", Text);
		}

		public override List<CustomProperty> GetProperties()
		{
			var props = base.GetProperties();
			props.Add(new CustomProperty("Text", Text, "Visual"));
			props.Add(new CustomProperty("Font", TextFont, "Visual"));
			props.Add(new CustomProperty("Angle", Angle, "Visual"));
			return props;
		}

		public override void ChangeProperty(CustomPropertyDescriptor property)
		{
			base.ChangeProperty(property);
			if (property.Description == "Text")
			{
				Text = property.Value as string;
			}
			if (property.Description == "Font")
			{
				TextFont = property.Value as Font;
			}
			if (property.Description == "Angle")
			{
				SetAngle((float)property.Value);
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
		public abstract void MoveHandler(PointF p);

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

		public override void MoveHandler(PointF p)
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

		public override void MoveHandler(PointF p)
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

		public override void MoveHandler(PointF p)
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

	public class TextTool : AbstractTool
	{
		TextObject TextObj;
		bool show = false;

		public TextTool(LayerEnum layer, bool mouseover = false) : base(layer)
		{
			TextObj = new TextObject(ActiveLayer);
			show = mouseover;
		}

		public override AbstractObject ResetAndGetObject()
		{
			return new TextObject(TextObj);
		}

		public override ToolAction ClickHandler(MouseEventArgs e)
		{
			// left click places the text
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

		public override void MoveHandler(PointF p)
		{
			TextObj.Location = p;
		}

		public override void PaintHandler(LayerEnum target_layer, PanelTransform tform, Graphics g)
		{
			if (show)
			{
				TextObj.DrawObject(target_layer, tform, g);
			}
		}

		public override void MouseFocusHandler(bool hover)
		{
			show = hover;
		}

		public override void KeyHandler(Keys key)
		{
			if (key == Keys.Space)
			{
				TextObj.SetAngle(TextObj.Angle + 90);
			}
		}
	}
}
