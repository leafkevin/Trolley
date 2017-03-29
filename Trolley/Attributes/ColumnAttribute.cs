using System;

namespace Trolley.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string FieldName { get; set; }
        public Type FieldType { get; set; }
        public bool AutoIncrement { get; set; }
        public ColumnAttribute(string fieldName)
        {
            this.FieldName = fieldName;
        }
        public ColumnAttribute(Type fieldType)
        {
            this.FieldType = fieldType;
        }
        public ColumnAttribute(string fieldName, Type fieldType, bool autoIncrement = false)
        {
            this.FieldName = fieldName;
            this.FieldType = fieldType;
            this.AutoIncrement = autoIncrement;
        }
    }
}
