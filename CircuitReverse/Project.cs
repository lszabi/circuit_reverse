using System;
using System.Drawing;
using System.Windows.Forms;

namespace CircuitReverse
{
	public struct Crosshair
	{
		public RelativePoint location;
		public bool show;
	}
	public partial class MainForm
	{
		// Absolute path for project file
		public string ProjectFilePath = "";

		public Crosshair crosshair = new Crosshair() { location = new RelativePoint(0, 0), show = false };

		// Active command
		public AbstractTool ActiveTool = null;

		// Property list container
		private PropertyList ObjectProperties = new PropertyList();

		// Tool functions
		public void BeginWire(object s, EventArgs e)
		{
			CancelTool();
			toolWire.Checked = true;
			ActiveTool = new WireTool(toolLayerSelect.SelectedIndex, IsMouseOverImgPanel());
		}

		public void BeginPin(object s, EventArgs e)
		{
			CancelTool();
			toolPin.Checked = true;
			ActiveTool = new PinTool(toolLayerSelect.SelectedIndex, IsMouseOverImgPanel());
		}

		public void CancelTool(object s = null, EventArgs e = null)
		{
			ActiveTool = null;
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
