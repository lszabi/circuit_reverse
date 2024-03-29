﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace CircuitReverse
{
	public partial class MainForm : Form
	{
		// Absolute path for project file
		public string ProjectFilePath = "";

		public Crosshair crosshair = new Crosshair() { location = new PointF(0, 0), show = false };

		// Active command
		public AbstractTool ActiveTool = new SelectTool();

		// List containing all objects
		// made static, so the Select Tool can see it
		public List<AbstractObject> ObjectList = new List<AbstractObject>();

		// Property list container
		private PropertyList ObjectProperties = new PropertyList();

		// Set this to true to reorder the form to vertical layout
		bool VerticalLayout = false;

		public void SetStatusText(string value)
		{
			statusStripMain.Items["statusLabelDefault"].Text = value;
		}

		public MainForm()
		{
			InitializeComponent();

			TopPanel.Layer = LayerEnum.TOP;
			BottomPanel.Layer = LayerEnum.BOTTOM;
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			objectPropertyGrid.SelectedObject = ObjectProperties;
			MainForm_Resize(sender, e);
		}

		// Resize event to make form responsive
		private void MainForm_Resize(object sender, EventArgs e)
		{
			// Define positions and sizes
			const int spacing = 6;
			const int left = 6;
			const int top = 52;

			int thirdwidth = (Size.Width - 4 * spacing - 18) / 3;

			int fullheight = Size.Height - 120;
			int halfheight = (fullheight - spacing) / 2;

			if (VerticalLayout)
			{
				// Resize and align TopPanel
				TopPanel.Location = new Point(
					left,
					top
				);
				TopPanel.Size = new Size(
					2 * thirdwidth + spacing,
					halfheight
				);

				// Resize and align BottomPanel
				BottomPanel.Location = new Point(
					left,
					top + halfheight + spacing
				);
				BottomPanel.Size = new Size(
					2 * thirdwidth + spacing,
					halfheight
				);
			}
			else
			{
				// Resize and align TopPanel
				TopPanel.Location = new Point(
					left,
					top
				);
				TopPanel.Size = new Size(
					thirdwidth,
					fullheight
				);

				// Resize and align BottomPanel
				BottomPanel.Location = new Point(
					left + thirdwidth + spacing,
					top
				);
				BottomPanel.Size = new Size(
					thirdwidth,
					fullheight
				);
			}

			// Resize and align objectPropertyGrid
			objectPropertyGrid.Location = new Point(
				left + thirdwidth + spacing + thirdwidth + spacing,
				top
			);
			objectPropertyGrid.Size = new Size(
				thirdwidth,
				halfheight
			);

			// Resize and align bjectTreeView
			objectTreeView.Location = new Point(
				left + thirdwidth + spacing + thirdwidth + spacing,
				top + halfheight + spacing
			);
			objectTreeView.Size = new Size(
				thirdwidth,
				halfheight
			);
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

			g.InterpolationMode = InterpolationMode.High;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			g.CompositingQuality = CompositingQuality.HighQuality;

			if (!p.ImageLoaded())
			{
				// Nothing to paint if no image is loaded
				return;
			}

			p.DrawPanelImage(g);

			ActiveTool.PaintHandler(p.Layer, p.RelativeToPanel, g);

			// Draw all objects, highlight the selected ones
			for (int i = 0; i < ObjectList.Count; i++)
			{
				AbstractObject obj = ObjectList[i];
				obj.DrawObject(p.Layer, p.RelativeToPanel, g, obj.Selected);
			}

			p.DrawPanelCrosshair(g, crosshair);
		}

		// Create new project (reset all project related variables)
		private void ResetProject()
		{
			CancelTool();
			ObjectList.Clear();
			RefreshObjectTreeView();

			TopPanel.img = null;
			BottomPanel.img = null;
			ProjectFilePath = "";
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
					foreach (AbstractObject item in ObjectList)
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
				// Save project into a new file
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

		// Load images and objects from project files
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
							ObjectList.Add(obj);
						}
					}
				}
			}

			RefreshObjectTreeView();
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
				ObjectList.Add(ActiveTool.ResetAndGetObject());
				RefreshObjectTreeView();
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

		private void BeginText(object s, EventArgs e)
		{
			CancelTool();
			toolText.Checked = true;
			ActiveTool = new TextTool(GetSelectedLayers(), IsMouseOverImgPanel());
		}

		public void CancelTool(object s = null, EventArgs e = null)
		{
			ActiveTool = new SelectTool();
			toolWire.Checked = false;
			toolPin.Checked = false;
			toolText.Checked = false;
			objectTreeView.SelectedNodes = null;
		}

		public void DeleteObject(object s, EventArgs e)
		{
			for (int i = ObjectList.Count - 1; i >= 0; i--)
			{
				if (ObjectList[i].Selected)
				{
					ObjectList.RemoveAt(i);
				}
			}
			RefreshObjectTreeView();
		}

		public void RefreshObjectTreeView()
		{
			objectTreeView.Nodes.Clear();
			for (int i = 0; i < ObjectList.Count; i++)
			{
				var t = new TreeNode(ObjectList[i].ToString())
				{
					Tag = i
				};
				objectTreeView.Nodes.Add(t);
				if (ObjectList[i].Selected)
				{
					objectTreeView.ToggleNode(t, true);
				}
			}
		}

		private void objectTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			ObjectProperties.Clear();

			// set selected objects
			foreach (var item in ObjectList)
			{
				item.Selected = false;
			}
			foreach (var item in objectTreeView.SelectedNodes)
			{
				ObjectList[(int)item.Tag].Selected = true;
			}

			if (objectTreeView.SelectedNodes.Count < 1)
			{
				// nothing is selected, no properties to show
				objectPropertyGrid.Refresh();
				return;
			}

			var properties = new List<CustomProperty>();
			bool first = true;

			foreach (var item in ObjectList)
			{
				if (item.Selected)
				{
					if (first)
					{
						// add the properties of the first item
						properties = item.GetProperties();
						first = false;
					}
					else
					{
						// remove all properties which are not in all items
						var props = item.GetProperties();
						for (int k = properties.Count - 1; k >= 0; k--)
						{
							if (!props.Exists(x => x.Name == properties[k].Name))
							{
								properties.RemoveAt(k);
							}
						}
					}
				}
			}

			ObjectProperties.Add(properties.ToArray());
			objectPropertyGrid.Refresh();
		}

		private void objectPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			// Refresh changed properties in objects
			var change = e.ChangedItem.PropertyDescriptor as CustomPropertyDescriptor;
			foreach (var item in objectTreeView.SelectedNodes)
			{
				ObjectList[(int)item.Tag].ChangeProperty(change);
			}

			// refresh objectTreeView texts
			RefreshObjectTreeView();
		}

		private void horizontalLayoutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			horizontalLayoutToolStripMenuItem.Checked = true;
			verticalLayoutToolStripMenuItem.Checked = false;
			VerticalLayout = false;
			MainForm_Resize(sender, e);
		}

		private void verticalLayoutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			horizontalLayoutToolStripMenuItem.Checked = false;
			verticalLayoutToolStripMenuItem.Checked = true;
			VerticalLayout = true;
			MainForm_Resize(sender, e);
		}
	}
}
