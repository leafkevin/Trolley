using System;

namespace Trolley.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
        public string FieldName { get; set; }
        public Type FieldType { get; set; }
        public bool AutoIncrement { get; set; }
        public PrimaryKeyAttribute(string fieldName)
        {
            this.FieldName = fieldName;
        }
        public PrimaryKeyAttribute(Type fieldType)
        {
            this.FieldType = fieldType;
        }
        public PrimaryKeyAttribute(string fieldName, Type fieldType, bool autoIncrement = false)
        {
            this.FieldName = fieldName;
            this.FieldType = fieldType;
            this.AutoIncrement = autoIncrement;
        }
    }
}
