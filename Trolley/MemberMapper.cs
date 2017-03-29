using System;
using System.Data;
using System.Reflection;

namespace Trolley
{
    public class MemberMapper
    {
        public string MemberName { get; set; }
        public Type MemberType { get; set; }
        public Type BoxType { get; set; }
        public bool IsValueType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsString { get; set; }
        public bool IsEnum { get; set; }
        public bool IsLinqBinary { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public string FieldName { get; set; }
        public DbType DbType { get; set; }
        public MethodInfo GetMethodInfo { get; set; }
    }
}
