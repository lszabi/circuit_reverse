﻿using System;
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
		// Absolute path for project file
		public string ProjectFilePath = "";

		public Crosshair crosshair = new Crosshair() { location = new RelativePoint(0, 0), show = false };

		// Active command
		public AbstractTool ActiveTool = new SelectTool();

		// Property list container
		private PropertyList ObjectProperties = new PropertyList();

		public MainForm()
		{
			InitializeComponent();

			TopPanel.Layer = LayerEnum.TOP;
			BottomPanel.Layer = LayerEnum.BOTTOM;
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			objectPropertyGrid.SelectedObject = ObjectProperties;
		}

		// Resize event to make form responsive
		private void MainForm_Resize(object sender, EventArgs e)
		{
			// Calculate positions and widths
			int thirdwidth = (Size.Width - 52) / 3;
			int fullheight = Size.Height - 116;
			int halfheight = (fullheight - 6) / 2;
			int left = 12;

			// Resize TopPanel
			TopPanel.Size = new Size(thirdwidth, fullheight);
			left += thirdwidth + 6;

			// Align BottomPanel to TopPanel
			BottomPanel.Size = new Size(thirdwidth, fullheight);
			BottomPanel.Location = new Point(left, BottomPanel.Location.Y);
			left += thirdwidth + 6;

			// Align objectPropertyGrid to BottomPanel
			objectPropertyGrid.Size = new Size(thirdwidth, halfheight);
			objectPropertyGrid.Location = new Point(left, objectPropertyGrid.Location.Y);

			// Align objectList to objectPropertyGrid
			objectList.Size = new Size(thirdwidth, halfheight);
			objectList.Location = new Point(left, 52 + 6 + halfheight);
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

		private void ImgPanelPaint(object sender, PaintEventArgs e)
		{
			var p = sender as BufferedPanel;
			var g = e.Graphics;

			if (!p.ImageLoaded())
			{
				return;
			}

			p.DrawPanelImage(g);

			ActiveTool.PaintHandler(p.Layer, p.RelativeToPanel, g);

			var selectedIndexes = objectList.SelectedIndices;
			for (int i = 0; i < objectList.Items.Count; i++)
			{
				AbstractObject obj = objectList.Items[i] as AbstractObject;
				obj.DrawObject(p.Layer, p.RelativeToPanel, g, selectedIndexes.Contains(i));
			}

			p.DrawPanelCrosshair(g, crosshair);
		}

		private void SetStatusText(string value)
		{
			statusStripMain.Items["statusLabelDefault"].Text = value;
		}

		// Create new project
		private void ResetProject()
		{
			CancelTool();
			ProjectFilePath = "";
			objectList.Items.Clear();
			TopPanel.img = null;
			BottomPanel.img = null;
			Text = "CircuitReverse";
		}

		private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Are you sure?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				ResetProject();
				SetStatusText("Project created.");
			}
		}

		private void saveProjectMenu_Click(object sender, EventArgs e)
		{
			SaveProject(false);
		}

		private void saveProjectAsMenu_Click(object sender, EventArgs e)
		{
			SaveProject(true);
		}

		// Save images and objects into ZIP archive
		private void SavePanelImg(BufferedPanel panel, ZipArchive archive, string entryName)
		{
			if (panel.img is null)
			{
				return;
			}

			var entry = archive.CreateEntry(entryName);
			using (var s = entry.Open())
			{
				panel.img.Save(s, ImageFormat.Png);
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
				if (ProjectSaveDialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				ProjectFilePath = ProjectSaveDialog.FileName;
				Text = "CircuitReverse - " + ProjectFilePath;
			}

			using (var zip = new ZipArchive(new FileStream(ProjectFilePath, FileMode.Create), ZipArchiveMode.Create))
			{
				SavePanelImg(TopPanel, zip, "top.png");
				SavePanelImg(BottomPanel, zip, "bottom.png");
				SaveObjectList(zip);
			}
			SetStatusText("Project saved");
		}

		private void OpenPanelImg(BufferedPanel panel, ZipArchive archive, string entryName)
		{
			var entry = archive.GetEntry(entryName);
			if (entry is null)
			{
				return;
			}

			using (var s = entry.Open())
			{
				panel.img = new Bitmap(s);
			}
		}

		private void OpenObjectList(ZipArchive archive)
		{
			var entry = archive.GetEntry("objects.txt");
			if (entry is null)
			{
				return;
			}

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

		private void openProjectMenu_Click(object sender, EventArgs e)
		{
			if (ProjectOpenDialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			ResetProject();
			ProjectFilePath = ProjectOpenDialog.FileName;

			using (var zip = new ZipArchive(new FileStream(ProjectFilePath, FileMode.Open), ZipArchiveMode.Read))
			{
				OpenPanelImg(TopPanel, zip, "top.png");
				OpenPanelImg(BottomPanel, zip, "bottom.png");
				OpenObjectList(zip);
			}

			Text = "CircuitReverse - " + ProjectFilePath;
			SetStatusText("Project loaded");
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
				// Select both layers
				layerSelectBottom.Checked = true;
				layerSelectTop.Checked = true;
			}
			else if (e.KeyCode == Keys.D1)
			{
				layerSelectTop.Checked = !layerSelectTop.Checked;
			}
			else if (e.KeyCode == Keys.D2)
			{
				layerSelectBottom.Checked = !layerSelectBottom.Checked;
			}
			else if (e.KeyCode == Keys.W)
			{
				BeginWire(sender, e);
			}
			else if (e.KeyCode == Keys.P)
			{
				BeginPin(sender, e);
			}
			else if (e.KeyCode == Keys.Delete)
			{
				DeleteObject(sender, e);
			}
			else
			{
				ActiveTool.KeyHandler(e.KeyCode);
			}
		}

		private void ImgPanelMouseMove(object sender, MouseEventArgs e)
		{
			var p = sender as BufferedPanel;
			if (!(p.img is null))
			{
				crosshair.location = p.PanelToRelative(e.Location);
			}

			ActiveTool.MoveHandler(crosshair.location);
		}

		private void ImgPanelMouseEnter(object sender, EventArgs e)
		{
			crosshair.show = true;
			ActiveTool.MouseFocusHandler(true);
		}

		private void ImgPanelMouseLeave(object sender, EventArgs e)
		{
			crosshair.show = false;
			ActiveTool.MouseFocusHandler(false);
		}

		private void ImgPanelMouseClick(object sender, MouseEventArgs e)
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

		// See whether mouse is over either image panel
		// this is used to show the object when starting a tool
		private bool IsMouseOverImgPanel()
		{
			if (TopPanel.ClientRectangle.Contains(PointToClient(MousePosition)) || BottomPanel.ClientRectangle.Contains(PointToClient(MousePosition)))
			{
				return true;
			}
			return false;
		}

		// Tool functions
		public LayerEnum GetSelectedLayers()
		{
			LayerEnum ret = LayerEnum.NONE;
			if (layerSelectBottom.Checked)
			{
				ret |= LayerEnum.BOTTOM;
			}
			if (layerSelectTop.Checked)
			{
				ret |= LayerEnum.TOP;
			}
			return ret;
		}

		public void BeginWire(object s, EventArgs e)
		{
			CancelTool();
			toolWire.Checked = true;
			ActiveTool = new WireTool(GetSelectedLayers(), IsMouseOverImgPanel());
		}

		public void BeginPin(object s, EventArgs e)
		{
			CancelTool();
			toolPin.Checked = true;
			ActiveTool = new PinTool(GetSelectedLayers(), IsMouseOverImgPanel());
		}

		public void CancelTool(object s = null, EventArgs e = null)
		{
			ActiveTool = new SelectTool();
			toolWire.Checked = false;
			toolPin.Checked = false;
			objectList.ClearSelected();
		}

		public void DeleteObject(object s, EventArgs e)
		{
			var selected = objectList.SelectedIndices;
			for (int i = objectList.Items.Count - 1; i >= 0; i--)
			{
				if (selected.Contains(i))
				{
					objectList.Items.RemoveAt(i);
				}
			}
		}

		private void objectList_SelectedValueChanged(object sender, EventArgs e)
		{
			ObjectProperties.Clear();

			// get properties from first item
			var selected = objectList.SelectedItems;
			if (selected.Count < 1)
			{
				objectPropertyGrid.Refresh();
				return;
			}

			var first = selected[0] as AbstractObject;
			var properties = first.GetProperties();

			// remove all properties which are not in all items
			for (int i = 1; i < selected.Count; i++)
			{
				var obj = selected[i] as AbstractObject;
				var props = obj.GetProperties();
				for (int k = properties.Count - 1; k >= 0; k--)
				{
					if (!props.Exists(x => x.Name == properties[k].Name))
					{
						properties.RemoveAt(k);
					}
				}
			}

			ObjectProperties.Add(properties.ToArray());
			objectPropertyGrid.Refresh();
		}

		private void objectPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			var change = e.ChangedItem.PropertyDescriptor as CustomPropertyDescriptor;
			foreach (var item in objectList.SelectedItems)
			{
				var p = item as AbstractObject;
				p.ChangeProperty(change);
			}

			// refresh objectList texts
			objectList.DrawMode = DrawMode.OwnerDrawVariable;
			objectList.DrawMode = DrawMode.Normal;
		}
	}
}
