using System;

namespace Trolley.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// 获取或设置数据库字段名字
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 获取或设置数据库字段的类型
        /// </summary>
        public Type FieldType { get; set; }
        /// <summary>
        /// 获取或设置数据库是否是自增字段
        /// </summary>
        public bool AutoIncrement { get; set; }
        /// <summary>
        /// 获取或设置数据库字段名字
        /// </summary>
        /// <param name="fieldName">数据库字段名字</param>
        public ColumnAttribute(string fieldName)
        {
            this.FieldName = fieldName;
        }
        /// <summary>
        /// 数据库类型为fieldType所对应的类型
        /// </summary>
        /// <param name="fieldType"></param>
        public ColumnAttribute(Type fieldType)
        {
            this.FieldType = fieldType;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fieldName">数据库字段的类型</param>
        /// <param name="fieldType">数据库字段类型</param>
        /// <param name="autoIncrement">是否自增字段</param>
        public ColumnAttribute(string fieldName, Type fieldType, bool autoIncrement = false)
        {
            this.FieldName = fieldName;
            this.FieldType = fieldType;
            this.AutoIncrement = autoIncrement;
        }
    }
}
