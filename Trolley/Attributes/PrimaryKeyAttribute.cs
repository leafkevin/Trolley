using System;

namespace Trolley.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// 数据库字段名称
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 数据库字段类型，如：typeof(stirng),typeof(DateTime)
        /// </summary>
        public Type FieldType { get; set; }
        /// <summary>
        /// 数据库字段是否为自增长，是：true，否则：false
        /// </summary>
        public bool AutoIncrement { get; set; }
        public PrimaryKeyAttribute()
        {
        }
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
