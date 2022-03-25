using System;
using System.Drawing;
using System.Windows.Forms;

namespace CircuitReverse
{
	// This is a double buffered Panel for visualizing the PCB layers
	// Used on MainForm and LoadImageForm
	public class BufferedPanel : Panel
	{
		// PCB image to show
		public Image img = null;
		public float ImageScale = 1.0f;
		public float ImageWidthScale = 1; // for load image scaling preview

		// Which layer shown by this panel
		public LayerEnum Layer = LayerEnum.NONE;

		public BufferedPanel()
		{
			DoubleBuffered = true;
			ResizeRedraw = true;
		}

		public bool ImageLoaded()
		{
			return !(img is null);
		}

		public void DrawPanelImage(Graphics g, float whscale = 0)
		{
			if (whscale != 0)
			{
				// scaling width for load preview
				ImageWidthScale = (float)img.Size.Height / img.Size.Width / whscale;
			}

			// determine scale and draw image to best fit
			float imgw = img.Width * ImageWidthScale;
			float imgh = img.Height;

			var scale_x = Size.Width / imgw;
			var scale_y = Size.Height / imgh;

			if (scale_x > scale_y)
			{
				// center horizontally
				var w = imgw * scale_y;
				var h = imgh * scale_y;
				var fw = Size.Width;
				g.DrawImage(img, (fw - w) / 2, 0, w, h);
				ImageScale = scale_y;
			}
			else
			{
				// center vertically
				var w = imgw * scale_x;
				var h = imgh * scale_x;
				var fh = Size.Height;
				g.DrawImage(img, 0, (fh - h) / 2, w, h);
				ImageScale = scale_x;
			}
		}

		// Draw crosshair on panel
		// Called from Paint event
		public void DrawPanelCrosshair(Graphics g, Crosshair c)
		{
			if (c.show && !(img is null))
			{
				Pen pn = new Pen(Color.Black, 1);
				Point p = RelativeToPanel(c.location);
				g.DrawLine(pn, p.X, 0, p.X, Size.Height);
				g.DrawLine(pn, 0, p.Y, Size.Width, p.Y);
			}
		}

		// Get relative corrdinates from panel coordinates (transform)
		// Panel coordinates: X: [0, width], Y: [0, height], centered on top left
		// Relative coordinates: X: [-1, 1], Y: [-1, 1], centered on image center
		public PointF PanelToRelative(PointF p)
		{
			float x = (p.X - Size.Width / 2.0f) * 2 / ImageScale / img.Width;
			float y = (p.Y - Size.Height / 2.0f) * 2 / ImageScale / img.Height;
			return new PointF(x, y);
		}

		// Get panel coordinates from relative coordinates (transform)
		public Point RelativeToPanel(PointF p)
		{
			var x = p.X * img.Width * ImageScale * 0.5 + Size.Width / 2.0;
			var y = p.Y * img.Height * ImageScale * 0.5 + Size.Height / 2.0;
			return new Point(ImageClipper.dtoi(x), ImageClipper.dtoi(y));
		}

		// Get image corrdinates from panel coordinates (transform)
		public Point PanelToImage(Point p)
		{
			var x = (p.X - Size.Width / 2.0) / ImageScale + img.Size.Width / 2.0;
			var y = (p.Y - Size.Height / 2.0) / ImageScale + img.Size.Height / 2.0;
			return new Point(ImageClipper.dtoi(x), ImageClipper.dtoi(y));
		}

		// Get panel coordinates from image coordinates (transform)
		public Point ImageToPanel(Point p)
		{
			var x = (p.X - img.Size.Width / 2.0) * ImageScale + Size.Width / 2.0;
			var y = (p.Y - img.Size.Height / 2.0) * ImageScale + Size.Height / 2.0;
			return new Point(ImageClipper.dtoi(x), ImageClipper.dtoi(y));
		}
	}
}
