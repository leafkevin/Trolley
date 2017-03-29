using System;

namespace Trolley.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; set; }
        public string FieldPrefix { get; set; }
        public TableAttribute(string tableName, string columnPrefix = "")
        {
            this.TableName = tableName;
            this.FieldPrefix = columnPrefix;
        }
    }
}