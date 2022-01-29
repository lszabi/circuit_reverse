using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace CircuitReverse
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();

			TopPanel.Layer = LayerEnum.TOP;
			BottomPanel.Layer = LayerEnum.BOTTOM;
		}

		// Resize event to make form responsive
		private void MainForm_Resize(object sender, EventArgs e)
		{
			// Resize TopPanel
			TopPanel.Size = new Size((Size.Width - 52) / 3, Size.Height - 116);

			// Align BottomPanel to TopPanel
			BottomPanel.Size = new Size(TopPanel.Size.Width, TopPanel.Size.Height);
			BottomPanel.Location = new Point(TopPanel.Location.X + TopPanel.Size.Width + 6, BottomPanel.Location.Y);

			// Align objectPropertyGrid to BottomPanel
			objectPropertyGrid.Size = new Size(TopPanel.Size.Width, (TopPanel.Size.Height - 6) / 2);
			objectPropertyGrid.Location = new Point(BottomPanel.Location.X + BottomPanel.Size.Width + 6, objectPropertyGrid.Location.Y);

			// Align objectList to objectPropertyGrid
			objectList.Size = objectPropertyGrid.Size;
			objectList.Location = new Point(objectPropertyGrid.Location.X, objectPropertyGrid.Location.Y + objectPropertyGrid.Size.Height + 6);
		}

		// Load images by using LoadImageForm as a dialog
		private void LoadTopMenu_Click(object sender, EventArgs e)
		{
			using (var f = new LoadImageForm())
			{
				if (f.ShowDialog() == DialogResult.OK)
				{
					TopPanel.img = f.getImage();
				}
			}
		}

		private void LoadBottomMenu_Click(object sender, EventArgs e)
		{
			using (var f = new LoadImageForm())
			{
				if (f.ShowDialog() == DialogResult.OK)
				{
					BottomPanel.img = f.getImage();
				}
			}
		}

		// Invalidate panels in fix intervals instead of on every change to unload CPU
		private void RefreshTimer_Tick(object sender, EventArgs e)
		{
			TopPanel.Invalidate();
			BottomPanel.Invalidate();
		}

		private void saveProjectMenu_Click(object sender, EventArgs e)
		{
			SaveProject(false);
		}

		private void saveProjectAsMenu_Click(object sender, EventArgs e)
		{
			SaveProject(true);
		}

		private void SavePanelImg(BufferedPanel panel, ZipArchive archive, string entryName)
		{
			if (!(panel.img is null))
			{
				var entry = archive.CreateEntry(entryName);
				using (var s = entry.Open())
				{
					panel.img.Save(s, ImageFormat.Png);
				}
			}
		}

		private void SaveObjectList(ZipArchive archive)
		{
			var entry = archive.CreateEntry("objects.txt");
			using (var e = entry.Open())
			{
				using (var s = new StreamWriter(e))
				{
					foreach (AbstractObject item in objectList.Items)
					{
						s.WriteLine(item.ExportObject());
					}
				}
			}
		}

		private void SaveProject(bool saveas)
		{
			if (ProjectFilePath == "" || saveas)
			{
				if (ProjectSaveDialog.ShowDialog() == DialogResult.OK)
				{
					ProjectFilePath = ProjectSaveDialog.FileName;
					Text = "CircuitReverse - " + ProjectFilePath;
				}
				else
				{
					return;
				}
			}

			using (var zip = new ZipArchive(new FileStream(ProjectFilePath, FileMode.Create), ZipArchiveMode.Create))
			{
				SavePanelImg(TopPanel, zip, "top.png");
				SavePanelImg(BottomPanel, zip, "bottom.png");
				SaveObjectList(zip);
			}
			statusStripMain.Items["statusLabelDefault"].Text = "Project saved";
		}

		private void OpenPanelImg(BufferedPanel panel, ZipArchive archive, string entryName)
		{
			var entry = archive.GetEntry(entryName);
			if (!(entry is null))
			{
				using (var s = entry.Open())
				{
					panel.img = new Bitmap(s);
				}
			}
		}

		private void OpenObjectList(ZipArchive archive)
		{
			objectList.Items.Clear();

			var entry = archive.GetEntry("objects.txt");
			if (!(entry is null))
			{
				using (var e = entry.Open())
				{
					using (var s = new StreamReader(e))
					{
						while (!s.EndOfStream)
						{
							var str = s.ReadLine().Trim();
							var obj = AbstractObject.ImportObject(str);
							if (!(obj is null))
							{
								objectList.Items.Add(obj);
							}
						}
					}
				}
			}
		}

		private void openProjectMenu_Click(object sender, EventArgs e)
		{
			if (ProjectOpenDialog.ShowDialog() == DialogResult.OK)
			{
				ProjectFilePath = ProjectOpenDialog.FileName;
				Text = "CircuitReverse - " + ProjectFilePath;
			}
			else
			{
				return;
			}

			using (var zip = new ZipArchive(new FileStream(ProjectFilePath, FileMode.Open), ZipArchiveMode.Read))
			{
				OpenPanelImg(TopPanel, zip, "top.png");
				OpenPanelImg(BottomPanel, zip, "bottom.png");
				OpenObjectList(zip);
			}
			statusStripMain.Items["statusLabelDefault"].Text = "Project loaded";
		}

		// Handle form key presses and mouse events, and forward them to the respective objects
		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				CancelTool(sender, e);
			}
			else if (e.KeyCode == Keys.D0)
			{
				toolLayerSelect.SelectedIndex = 0;
			}
			else if (e.KeyCode == Keys.D1)
			{
				toolLayerSelect.SelectedIndex = 1;
			}
			else if (e.KeyCode == Keys.D2)
			{
				toolLayerSelect.SelectedIndex = 2;
			}
			else if (e.KeyCode == Keys.W)
			{
				BeginWire(sender, e);
			}
			else if (e.KeyCode == Keys.P)
			{
				BeginPin(sender, e);
			}
			else
			{
				ActiveTool?.KeyHandler(e.KeyCode);
			}
		}

		private void ImgPanelPaint(object sender, PaintEventArgs e)
		{
			var p = sender as BufferedPanel;
			var g = e.Graphics;

			if (p.ImageLoaded())
			{
				p.DrawPanelImage(g);

				ActiveTool?.PaintHandler(p.Layer, p.RelativeToPanel, g);

				var selectedIndexes = objectList.SelectedIndices;
				for (int i = 0; i < objectList.Items.Count; i++)
				{
					AbstractObject obj = objectList.Items[i] as AbstractObject;
					obj.DrawObject(p.Layer, p.RelativeToPanel, g, selectedIndexes.Contains(i));
				}

				p.DrawPanelCrosshair(g, crosshair);
			}
		}

		private void ImgPanelMouseMove(object sender, MouseEventArgs e)
		{
			var p = sender as BufferedPanel;
			if (!(p.img is null))
			{
				crosshair.location = p.PanelToRelative(e.Location);
			}

			ActiveTool?.MoveHandler(crosshair.location);
		}

		private void ImgPanelMouseEnter(object sender, EventArgs e)
		{
			crosshair.show = true;
			ActiveTool?.MouseFocusHandler(true);
		}

		private void ImgPanelMouseLeave(object sender, EventArgs e)
		{
			crosshair.show = false;
			ActiveTool?.MouseFocusHandler(false);
		}

		private void ImgPanelMouseClick(object sender, MouseEventArgs e)
		{
			if (!(ActiveTool is null))
			{
				var action = ActiveTool.ClickHandler(e);

				// if the tool is resetting or exiting, save the object
				if (action == ToolAction.RESET || action == ToolAction.EXIT)
				{
					objectList.Items.Add(ActiveTool.ResetAndGetObject());
				}

				// if the tool is aborting or exiting, delete the object
				if (action == ToolAction.ABORT || action == ToolAction.EXIT)
				{
					CancelTool();
				}
			}
		}

		private bool IsMouseOverImgPanel()
		{
			if (TopPanel.ClientRectangle.Contains(PointToClient(MousePosition)) || BottomPanel.ClientRectangle.Contains(PointToClient(MousePosition)))
			{
				return true;
			}
			return false;
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			objectPropertyGrid.SelectedObject = ObjectProperties;
		}

	}
}
