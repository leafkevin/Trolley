using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Trolley;

[TypeDescriptionProvider(typeof(TheaRowTypeDescriptionProvider))]
internal sealed partial class TheaRow
{
    private sealed class TheaRowTypeDescriptionProvider : TypeDescriptionProvider
    {
        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            => new TheaRowTypeDescriptor(instance);
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            => new TheaRowTypeDescriptor(instance);
    }
    internal sealed class TheaRowTypeDescriptor : ICustomTypeDescriptor
    {
        private readonly TheaRow _row;
        public TheaRowTypeDescriptor(object instance) => _row = (TheaRow)instance;
        AttributeCollection ICustomTypeDescriptor.GetAttributes() => AttributeCollection.Empty;
        string ICustomTypeDescriptor.GetClassName() => typeof(TheaRow).FullName;
        string ICustomTypeDescriptor.GetComponentName() => null;
        private static readonly TypeConverter s_converter = new ExpandableObjectConverter();
        TypeConverter ICustomTypeDescriptor.GetConverter() => s_converter;
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() => null;
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() => null;
        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) => null;
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() => EventDescriptorCollection.Empty;
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) => EventDescriptorCollection.Empty;
        internal static PropertyDescriptorCollection GetProperties(TheaRow row) => GetProperties(row?.table, row);
        internal static PropertyDescriptorCollection GetProperties(TheaTable table, IDictionary<string, object> row = null)
        {
            string[] names = table?.FieldNames;
            if (names == null || names.Length == 0) return PropertyDescriptorCollection.Empty;
            var arr = new PropertyDescriptor[names.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                var type = row != null && row.TryGetValue(names[i], out var value) && value != null
                    ? value.GetType() : typeof(object);
                arr[i] = new RowBoundPropertyDescriptor(type, names[i], i);
            }
            return new PropertyDescriptorCollection(arr, true);
        }
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() => GetProperties(_row);
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) => GetProperties(_row);
        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) => _row;
    }
    internal sealed class RowBoundPropertyDescriptor : PropertyDescriptor
    {
        private readonly Type _type;
        private readonly int _index;
        public RowBoundPropertyDescriptor(Type type, string name, int index) : base(name, null)
        {
            _type = type;
            _index = index;
        }
        public override bool CanResetValue(object component) => true;
        public override void ResetValue(object component) => ((TheaRow)component).Remove(_index);
        public override bool IsReadOnly => false;
        public override bool ShouldSerializeValue(object component) => ((TheaRow)component).TryGetValue(_index, out _);
        public override Type ComponentType => typeof(TheaRow);
        public override Type PropertyType => _type;
        public override object GetValue(object component)
            => ((TheaRow)component).TryGetValue(_index, out var val) ? (val ?? DBNull.Value) : DBNull.Value;
        public override void SetValue(object component, object value)
            => ((TheaRow)component).SetValue(_index, value is DBNull ? null : value);
    }
}
