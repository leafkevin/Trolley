using Trolley.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Trolley
{
    public struct EntityMapper
    {
        public bool IsEmpty { get { return this.EntityType == null; } }
        public Type EntityType { get; private set; }
        public string TableName { get; private set; }
        public string FieldPrefix { get; private set; }
        public Dictionary<string, MemberMapper> MemberMappers { get; private set; }
        public List<MemberMapper> PrimaryKeys { get; private set; }
        public EntityMapper(Type entityType)
        {
            this.EntityType = entityType;
            this.TableName = entityType.Name();
            this.FieldPrefix = String.Empty;
            this.MemberMappers = new Dictionary<string, MemberMapper>();
            this.PrimaryKeys = new List<MemberMapper>();
            var tableAttr = entityType.GetCustomAttribute<TableAttribute>();
            if (tableAttr != null)
            {
                if (!String.IsNullOrEmpty(tableAttr.TableName)) this.TableName = tableAttr.TableName;
                this.FieldPrefix = tableAttr.FieldPrefix;
            }
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(p => p.GetIndexParameters().Length == 0 && p.GetSetMethod(true) != null);
            foreach (var prop in properties)
            {
                MemberMapper colMapper = new MemberMapper();
                if (prop.GetCustomAttribute<IgnoreAttribute>() != null)
                {
                    continue;
                }
                colMapper.FieldName = this.FieldPrefix + prop.Name;
                colMapper.GetMethodInfo = prop.GetGetMethod(true);
                colMapper.MemberName = prop.Name;
                colMapper.MemberType = prop.PropertyType;
                colMapper.DbType = DbTypeMap.LookupDbType(prop.PropertyType);
                colMapper.BoxType = prop.PropertyType;
                colMapper.IsValueType = prop.PropertyType.IsValueType();
                colMapper.IsEnum = prop.PropertyType.IsEnum();
                colMapper.IsNullable = Nullable.GetUnderlyingType(colMapper.MemberType) != null;
                if (colMapper.IsNullable)
                {
                    var underlyingType = Nullable.GetUnderlyingType(colMapper.MemberType);
                    colMapper.IsEnum = underlyingType.IsEnum();
                    if (colMapper.IsEnum)
                    {
                        colMapper.BoxType = underlyingType;
                    }
                }
                var keyAttr = prop.GetCustomAttribute<PrimaryKeyAttribute>();
                if (keyAttr != null)
                {
                    if (!String.IsNullOrEmpty(keyAttr.FieldName)) colMapper.FieldName = keyAttr.FieldName;
                    if (keyAttr.FieldType != null)
                    {
                        colMapper.DbType = DbTypeMap.LookupDbType(keyAttr.FieldType);
                        colMapper.BoxType = keyAttr.FieldType;
                    }
                    colMapper.IsPrimaryKey = true;
                    colMapper.IsAutoIncrement = keyAttr.AutoIncrement;
                    this.PrimaryKeys.Add(colMapper);
                }
                else colMapper.IsPrimaryKey = false;
                var colAttr = prop.GetCustomAttribute<ColumnAttribute>();
                if (colAttr != null)
                {
                    if (!String.IsNullOrEmpty(colAttr.FieldName)) colMapper.FieldName = colAttr.FieldName;
                    if (colAttr.FieldType != null)
                    {
                        colMapper.DbType = DbTypeMap.LookupDbType(colAttr.FieldType);
                        colMapper.BoxType = colAttr.FieldType;
                        colMapper.IsAutoIncrement = keyAttr.AutoIncrement;
                    }
                }
                colMapper.IsString = colMapper.BoxType == typeof(string);
                colMapper.IsLinqBinary = colMapper.BoxType.FullName == DbTypeMap.LinqBinary;
                this.MemberMappers.Add(prop.Name, colMapper);
            }
        }
    }
}
