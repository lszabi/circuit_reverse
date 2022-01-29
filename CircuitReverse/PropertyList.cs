using System;
using System.Collections;
using System.ComponentModel;

// code from https://www.codeproject.com/Articles/9280/Add-Remove-Items-to-from-PropertyGrid-at-Runtime

namespace CircuitReverse
{
	public class PropertyList : CollectionBase, ICustomTypeDescriptor
	{
		public void Add(CustomProperty Value)
		{
			List.Add(Value);
		}

		public void Add(CustomProperty[] Values)
		{
			foreach (var item in Values)
			{
				List.Add(item);
			}
		}
		
		public void Remove(string Name)
		{
			foreach (CustomProperty prop in List)
			{
				if (prop.Name == Name)
				{
					List.Remove(prop);
					return;
				}
			}
		}

		public CustomProperty Get(string Name)
		{
			foreach (CustomProperty prop in List)
			{
				if (prop.Name == Name)
				{
					return prop;
				}
			}
			return null;
		}

		public CustomProperty this[int index]
		{
			get
			{
				return (CustomProperty)List[index];
			}
			set
			{
				List[index] = value;
			}
		}

		public string GetClassName()
		{
			return TypeDescriptor.GetClassName(this, true);
		}

		public AttributeCollection GetAttributes()
		{
			return TypeDescriptor.GetAttributes(this, true);
		}

		public string GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}

		public TypeConverter GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}

		public EventDescriptor GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}

		public PropertyDescriptor GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(this, true);
		}

		public object GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}

		public EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

		public EventDescriptorCollection GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}

		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			PropertyDescriptor[] newProps = new PropertyDescriptor[Count];
			for (int i = 0; i < Count; i++)
			{
				CustomProperty prop = this[i];
				newProps[i] = new CustomPropertyDescriptor(ref prop, attributes);
			}
			return new PropertyDescriptorCollection(newProps);
		}

		public PropertyDescriptorCollection GetProperties()
		{
			return TypeDescriptor.GetProperties(this, true);
		}

		public object GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}
	}

	public class CustomProperty
	{
		public bool ReadOnly { get; private set; }
		public string Name { get; private set; }
		public string Category { get; private set; }
		public bool Visible { get; private set; }
		public object Value { get; set; }

		public CustomProperty(string sName, object value, string sCategory = "Basic", bool bReadOnly = false, bool bVisible = true)
		{
			Name = sName;
			Value = value;
			Category = sCategory;
			ReadOnly = bReadOnly;
			Visible = bVisible;
		}
	}

	public class CustomPropertyDescriptor : PropertyDescriptor
	{
		readonly CustomProperty m_Property;

		public CustomPropertyDescriptor(ref CustomProperty myProperty, Attribute[] attrs) : base(myProperty.Name, attrs)
		{
			m_Property = myProperty;
		}

		public override Type ComponentType => null;
		public override Type PropertyType => m_Property.Value.GetType();

		public override string Description => m_Property.Name;
		public override string Category => m_Property.Category;
		public override string DisplayName => m_Property.Name;
		public override bool IsReadOnly => m_Property.ReadOnly;

		public object Value => m_Property.Value;

		public override bool CanResetValue(object component)
		{
			return false;
		}

		public override object GetValue(object component)
		{
			return m_Property.Value;
		}

		public override void ResetValue(object component)
		{
			// Have to implement
		}

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}

		public override void SetValue(object component, object value)
		{
			m_Property.Value = value;
		}
	}
}
