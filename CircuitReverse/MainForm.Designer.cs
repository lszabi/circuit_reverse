namespace CircuitReverse
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.RefreshTimer = new System.Windows.Forms.Timer(this.components);
			this.statusStripMain = new System.Windows.Forms.StatusStrip();
			this.StatusLabelDefault = new System.Windows.Forms.ToolStripStatusLabel();
			this.ProjectSaveDialog = new System.Windows.Forms.SaveFileDialog();
			this.ProjectOpenDialog = new System.Windows.Forms.OpenFileDialog();
			this.objectPropertyGrid = new System.Windows.Forms.PropertyGrid();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadTopImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadBottomImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.newProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveProjectAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStripMain = new System.Windows.Forms.MenuStrip();
			this.toolStripMain = new System.Windows.Forms.ToolStrip();
			this.buttonSave = new System.Windows.Forms.ToolStripButton();
			this.buttonOpen = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolCancel = new System.Windows.Forms.ToolStripButton();
			this.toolWire = new System.Windows.Forms.ToolStripButton();
			this.toolPin = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.layerSelectTop = new System.Windows.Forms.ToolStripButton();
			this.layerSelectBottom = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.objDelete = new System.Windows.Forms.ToolStripButton();
			this.BottomPanel = new CircuitReverse.BufferedPanel();
			this.TopPanel = new CircuitReverse.BufferedPanel();
			this.objectTreeView = new CircuitReverse.MultiSelectTreeview();
			this.statusStripMain.SuspendLayout();
			this.menuStripMain.SuspendLayout();
			this.toolStripMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// RefreshTimer
			// 
			this.RefreshTimer.Enabled = true;
			this.RefreshTimer.Interval = 30;
			this.RefreshTimer.Tick += new System.EventHandler(this.RefreshTimer_Tick);
			// 
			// statusStripMain
			// 
			this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabelDefault});
			this.statusStripMain.Location = new System.Drawing.Point(0, 439);
			this.statusStripMain.Name = "statusStripMain";
			this.statusStripMain.Size = new System.Drawing.Size(884, 22);
			this.statusStripMain.TabIndex = 5;
			// 
			// StatusLabelDefault
			// 
			this.StatusLabelDefault.Name = "StatusLabelDefault";
			this.StatusLabelDefault.Size = new System.Drawing.Size(60, 17);
			this.StatusLabelDefault.Text = "Welcome!";
			// 
			// ProjectSaveDialog
			// 
			this.ProjectSaveDialog.FileName = "project.zip";
			this.ProjectSaveDialog.Filter = "ZIP file|*.zip";
			// 
			// ProjectOpenDialog
			// 
			this.ProjectOpenDialog.Filter = "ZIP file|*.zip";
			// 
			// objectPropertyGrid
			// 
			this.objectPropertyGrid.HelpVisible = false;
			this.objectPropertyGrid.Location = new System.Drawing.Point(588, 52);
			this.objectPropertyGrid.Name = "objectPropertyGrid";
			this.objectPropertyGrid.Size = new System.Drawing.Size(284, 189);
			this.objectPropertyGrid.TabIndex = 6;
			this.objectPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.objectPropertyGrid_PropertyValueChanged);
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadTopImageToolStripMenuItem,
            this.loadBottomImageToolStripMenuItem,
            this.toolStripSeparator1,
            this.newProjectToolStripMenuItem,
            this.saveProjectToolStripMenuItem,
            this.saveProjectAsToolStripMenuItem,
            this.openProjectToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// loadTopImageToolStripMenuItem
			// 
			this.loadTopImageToolStripMenuItem.Name = "loadTopImageToolStripMenuItem";
			this.loadTopImageToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
			this.loadTopImageToolStripMenuItem.Text = "Load Top Image";
			this.loadTopImageToolStripMenuItem.Click += new System.EventHandler(this.LoadTopMenu_Click);
			// 
			// loadBottomImageToolStripMenuItem
			// 
			this.loadBottomImageToolStripMenuItem.Name = "loadBottomImageToolStripMenuItem";
			this.loadBottomImageToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
			this.loadBottomImageToolStripMenuItem.Text = "Load Bottom Image";
			this.loadBottomImageToolStripMenuItem.Click += new System.EventHandler(this.LoadBottomMenu_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(176, 6);
			// 
			// newProjectToolStripMenuItem
			// 
			this.newProjectToolStripMenuItem.Image = global::CircuitReverse.Properties.Resources.NewFile_16x;
			this.newProjectToolStripMenuItem.Name = "newProjectToolStripMenuItem";
			this.newProjectToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
			this.newProjectToolStripMenuItem.Text = "New project";
			this.newProjectToolStripMenuItem.Click += new System.EventHandler(this.newProjectToolStripMenuItem_Click);
			// 
			// saveProjectToolStripMenuItem
			// 
			this.saveProjectToolStripMenuItem.Image = global::CircuitReverse.Properties.Resources.Save_16x;
			this.saveProjectToolStripMenuItem.Name = "saveProjectToolStripMenuItem";
			this.saveProjectToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveProjectToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
			this.saveProjectToolStripMenuItem.Text = "Save project";
			this.saveProjectToolStripMenuItem.Click += new System.EventHandler(this.saveProjectMenu_Click);
			// 
			// saveProjectAsToolStripMenuItem
			// 
			this.saveProjectAsToolStripMenuItem.Image = global::CircuitReverse.Properties.Resources.SaveAs_16x;
			this.saveProjectAsToolStripMenuItem.Name = "saveProjectAsToolStripMenuItem";
			this.saveProjectAsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
			this.saveProjectAsToolStripMenuItem.Text = "Save project as...";
			this.saveProjectAsToolStripMenuItem.Click += new System.EventHandler(this.saveProjectAsMenu_Click);
			// 
			// openProjectToolStripMenuItem
			// 
			this.openProjectToolStripMenuItem.Image = global::CircuitReverse.Properties.Resources.OpenProject_16x;
			this.openProjectToolStripMenuItem.Name = "openProjectToolStripMenuItem";
			this.openProjectToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
			this.openProjectToolStripMenuItem.Text = "Open project";
			this.openProjectToolStripMenuItem.Click += new System.EventHandler(this.openProjectMenu_Click);
			// 
			// menuStripMain
			// 
			this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
			this.menuStripMain.Location = new System.Drawing.Point(0, 0);
			this.menuStripMain.Name = "menuStripMain";
			this.menuStripMain.Size = new System.Drawing.Size(884, 24);
			this.menuStripMain.TabIndex = 4;
			this.menuStripMain.Text = "menuStrip1";
			// 
			// toolStripMain
			// 
			this.toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonSave,
            this.buttonOpen,
            this.toolStripSeparator4,
            this.toolCancel,
            this.toolWire,
            this.toolPin,
            this.toolStripSeparator2,
            this.layerSelectTop,
            this.layerSelectBottom,
            this.toolStripSeparator3,
            this.objDelete});
			this.toolStripMain.Location = new System.Drawing.Point(0, 24);
			this.toolStripMain.Name = "toolStripMain";
			this.toolStripMain.Size = new System.Drawing.Size(884, 25);
			this.toolStripMain.TabIndex = 7;
			this.toolStripMain.Text = "toolStrip1";
			// 
			// buttonSave
			// 
			this.buttonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonSave.Image = global::CircuitReverse.Properties.Resources.Save_16x;
			this.buttonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(23, 22);
			this.buttonSave.ToolTipText = "Save (Ctrl+S)";
			this.buttonSave.Click += new System.EventHandler(this.saveProjectMenu_Click);
			// 
			// buttonOpen
			// 
			this.buttonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonOpen.Image = global::CircuitReverse.Properties.Resources.OpenProject_16x;
			this.buttonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonOpen.Name = "buttonOpen";
			this.buttonOpen.Size = new System.Drawing.Size(23, 22);
			this.buttonOpen.ToolTipText = "Open";
			this.buttonOpen.Click += new System.EventHandler(this.openProjectMenu_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
			// 
			// toolCancel
			// 
			this.toolCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolCancel.Image = global::CircuitReverse.Properties.Resources.Cancel_16x;
			this.toolCancel.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolCancel.Name = "toolCancel";
			this.toolCancel.Size = new System.Drawing.Size(23, 22);
			this.toolCancel.ToolTipText = "Cancel action (Esc)";
			this.toolCancel.Click += new System.EventHandler(this.CancelTool);
			// 
			// toolWire
			// 
			this.toolWire.CheckOnClick = true;
			this.toolWire.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolWire.Image = global::CircuitReverse.Properties.Resources.AssociationRelationship_16x;
			this.toolWire.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolWire.Name = "toolWire";
			this.toolWire.Size = new System.Drawing.Size(23, 22);
			this.toolWire.ToolTipText = "Draw line (W)";
			this.toolWire.Click += new System.EventHandler(this.BeginWire);
			// 
			// toolPin
			// 
			this.toolPin.CheckOnClick = true;
			this.toolPin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolPin.Image = global::CircuitReverse.Properties.Resources.Pin_16x;
			this.toolPin.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolPin.Name = "toolPin";
			this.toolPin.Size = new System.Drawing.Size(23, 22);
			this.toolPin.ToolTipText = "Draw pin (P)";
			this.toolPin.Click += new System.EventHandler(this.BeginPin);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// layerSelectTop
			// 
			this.layerSelectTop.Checked = true;
			this.layerSelectTop.CheckOnClick = true;
			this.layerSelectTop.CheckState = System.Windows.Forms.CheckState.Checked;
			this.layerSelectTop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.layerSelectTop.Image = global::CircuitReverse.Properties.Resources.LayerTop_16x;
			this.layerSelectTop.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.layerSelectTop.Name = "layerSelectTop";
			this.layerSelectTop.Size = new System.Drawing.Size(23, 22);
			this.layerSelectTop.ToolTipText = "Use Top layer (1)";
			// 
			// layerSelectBottom
			// 
			this.layerSelectBottom.Checked = true;
			this.layerSelectBottom.CheckOnClick = true;
			this.layerSelectBottom.CheckState = System.Windows.Forms.CheckState.Checked;
			this.layerSelectBottom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.layerSelectBottom.Image = global::CircuitReverse.Properties.Resources.LayerBottom_16x;
			this.layerSelectBottom.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.layerSelectBottom.Name = "layerSelectBottom";
			this.layerSelectBottom.Size = new System.Drawing.Size(23, 22);
			this.layerSelectBottom.ToolTipText = "Use Bottom layer (2)";
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// objDelete
			// 
			this.objDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.objDelete.Image = global::CircuitReverse.Properties.Resources.RecycleBin_16x;
			this.objDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.objDelete.Name = "objDelete";
			this.objDelete.Size = new System.Drawing.Size(23, 22);
			this.objDelete.Text = "toolStripButton1";
			this.objDelete.ToolTipText = "Delete object (Del)";
			this.objDelete.Click += new System.EventHandler(this.DeleteObject);
			// 
			// BottomPanel
			// 
			this.BottomPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.BottomPanel.Location = new System.Drawing.Point(300, 52);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = new System.Drawing.Size(282, 384);
			this.BottomPanel.TabIndex = 2;
			this.BottomPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.ImgPanelPaint);
			this.BottomPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ImgPanelMouseClick);
			this.BottomPanel.MouseEnter += new System.EventHandler(this.ImgPanelMouseEnter);
			this.BottomPanel.MouseLeave += new System.EventHandler(this.ImgPanelMouseLeave);
			this.BottomPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImgPanelMouseMove);
			// 
			// TopPanel
			// 
			this.TopPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.TopPanel.Location = new System.Drawing.Point(12, 52);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = new System.Drawing.Size(282, 384);
			this.TopPanel.TabIndex = 1;
			this.TopPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.ImgPanelPaint);
			this.TopPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ImgPanelMouseClick);
			this.TopPanel.MouseEnter += new System.EventHandler(this.ImgPanelMouseEnter);
			this.TopPanel.MouseLeave += new System.EventHandler(this.ImgPanelMouseLeave);
			this.TopPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImgPanelMouseMove);
			// 
			// objectTreeView
			// 
			this.objectTreeView.Location = new System.Drawing.Point(588, 247);
			this.objectTreeView.Name = "objectTreeView";
			this.objectTreeView.SelectedNodes = ((System.Collections.Generic.List<System.Windows.Forms.TreeNode>)(resources.GetObject("objectTreeView.SelectedNodes")));
			this.objectTreeView.Size = new System.Drawing.Size(284, 189);
			this.objectTreeView.TabIndex = 8;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(884, 461);
			this.Controls.Add(this.objectTreeView);
			this.Controls.Add(this.toolStripMain);
			this.Controls.Add(this.objectPropertyGrid);
			this.Controls.Add(this.statusStripMain);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.menuStripMain);
			this.DoubleBuffered = true;
			this.KeyPreview = true;
			this.MainMenuStrip = this.menuStripMain;
			this.Name = "MainForm";
			this.Text = "CircuitReverse";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			this.statusStripMain.ResumeLayout(false);
			this.statusStripMain.PerformLayout();
			this.menuStripMain.ResumeLayout(false);
			this.menuStripMain.PerformLayout();
			this.toolStripMain.ResumeLayout(false);
			this.toolStripMain.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private BufferedPanel TopPanel;
		private BufferedPanel BottomPanel;
		private MultiSelectTreeview objectTreeView;
		private System.Windows.Forms.Timer RefreshTimer;
		private System.Windows.Forms.StatusStrip statusStripMain;
		private System.Windows.Forms.ToolStripStatusLabel StatusLabelDefault;
		private System.Windows.Forms.SaveFileDialog ProjectSaveDialog;
		private System.Windows.Forms.OpenFileDialog ProjectOpenDialog;
		private System.Windows.Forms.PropertyGrid objectPropertyGrid;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadTopImageToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadBottomImageToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem saveProjectToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveProjectAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openProjectToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStripMain;
		private System.Windows.Forms.ToolStrip toolStripMain;
		private System.Windows.Forms.ToolStripButton toolWire;
		private System.Windows.Forms.ToolStripButton toolPin;
		private System.Windows.Forms.ToolStripButton toolCancel;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripButton objDelete;
		private System.Windows.Forms.ToolStripButton buttonSave;
		private System.Windows.Forms.ToolStripButton buttonOpen;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem newProjectToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton layerSelectTop;
		private System.Windows.Forms.ToolStripButton layerSelectBottom;
	}
}

