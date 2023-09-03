using System;
using System.Text;

namespace Trolley.SqlServer;

class SqlServerCreateVisitor : CreateVisitor, ICreateVisitor
{
    public SqlServerCreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix) { }
    public override string BuildTailSql()
    {
        if (this.isUseIgnore && this.ignoreKeysOrUniqueKeys == null)
        {
            if (this.ignoreKeysOrUniqueKeys == null)
                throw new NotSupportedException($"参数keysOrUniqueKeys值为null，SqlServer此参数必须有值");

            var entityMapper = this.tables[0].Mapper;
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(this.whereSql))
            {
                builder.Append(this.whereSql);
                builder.Append($" AND NOT EXISTS(SELECT 1 FROM {this.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
            }
            else builder.Append($" WHERE NOT EXISTS(SELECT 1 FROM {this.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");

            var ignoreKeysType = this.ignoreKeysOrUniqueKeys.GetType();
            if (ignoreKeysType.IsEntityType(out _))
            {
                var commandInitializer = RepositoryHelper.BuildWhereWithKeysCommandInitializer(this, entityMapper.EntityType, this.ignoreKeysOrUniqueKeys);
                commandInitializer.Invoke(this, this.dbParameters, ignoreKeysType);
            }
            else
            {
                var keyMapper = entityMapper.KeyMembers[0];
                var dbParameter = this.OrmProvider.CreateParameter(keyMapper, $"k{keyMapper.MemberName}", this.ignoreKeysOrUniqueKeys);
                builder.Append($"{this.OrmProvider.GetFieldName(keyMapper.FieldName)}=k{keyMapper.MemberName}");
                this.dbParameters.Add(dbParameter);
            }
            builder.Append(')');
            return builder.ToString();
        }
        if (!this.IsBulk && this.tables[0].Mapper.IsAutoIncrement)
            return ";SELECT SCOPE_IDENTITY()";
        return string.Empty;
    }
}
