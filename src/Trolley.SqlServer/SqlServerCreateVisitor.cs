using System.Text;

namespace Trolley.SqlServer;

class SqlServerCreateVisitor : CreateVisitor, ICreateVisitor
{
    public SqlServerCreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix) { }

    public override string BuildSql()
    {
        //var tableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        //var fieldsBuilder = new StringBuilder($"{this.BuildHeadSql()} {tableName} (");
        //var valuesBuilder = new StringBuilder();
        //if (this.IsUseIfNotExists) valuesBuilder.Append(" SELECT ");
        //else valuesBuilder.Append(" VALUES(");
        //for (int i = 0; i < this.InsertFields.Count; i++)
        //{
        //    var insertField = this.InsertFields[i];
        //    if (i > 0)
        //    {
        //        fieldsBuilder.Append(',');
        //        valuesBuilder.Append(',');
        //    }
        //    fieldsBuilder.Append(insertField.Fields);
        //    valuesBuilder.Append(insertField.Values);
        //}
        //fieldsBuilder.Append(')');
        //if (this.IsUseIfNotExists)
        //{
        //    if (!string.IsNullOrEmpty(this.WhereSql))
        //        valuesBuilder.Append($" WHERE NOT EXISTS(SELECT * FROM {tableName} WHERE {this.WhereSql})");
        //}
        //else valuesBuilder.Append(')');

        //var tailSql = this.BuildTailSql();
        //if (!string.IsNullOrEmpty(tailSql))
        //    valuesBuilder.Append(tailSql);
        //fieldsBuilder.Append(valuesBuilder);
        //return fieldsBuilder.ToString();
        return null;
    }
    public override string BuildTailSql()
    {
        //if (this.IsUseIgnore)
        //{
        //    if (this.IgnoreKeysOrUniqueKeys == null)
        //        throw new NotSupportedException($"参数keysOrUniqueKeys值为null，SqlServer此参数必须有值");

        //    var entityMapper = this.Tables[0].Mapper;
        //    var builder = new StringBuilder();
        //    if (!string.IsNullOrEmpty(this.WhereSql))
        //    {
        //        builder.Append(this.WhereSql);
        //        builder.Append($" AND NOT EXISTS(SELECT * FROM {this.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");
        //    }
        //    else builder.Append($" WHERE NOT EXISTS(SELECT * FROM {this.OrmProvider.GetTableName(entityMapper.TableName)} WHERE ");

        //    var ignoreKeysType = this.IgnoreKeysOrUniqueKeys.GetType();
        //    if (ignoreKeysType.IsEntityType(out _))
        //    {
        //        var commandInitializer = RepositoryHelper.BuildWhereWithKeysCommandInitializer(this, entityMapper.EntityType, this.IgnoreKeysOrUniqueKeys);
        //        builder.Append(commandInitializer.Invoke(this, this.DbParameters, this.IgnoreKeysOrUniqueKeys));
        //    }
        //    else
        //    {
        //        var keyMapper = entityMapper.KeyMembers[0];
        //        var dbParameter = this.OrmProvider.CreateParameter(keyMapper, $"k{keyMapper.MemberName}", this.IgnoreKeysOrUniqueKeys);
        //        builder.Append($"{this.OrmProvider.GetFieldName(keyMapper.FieldName)}=k{keyMapper.MemberName}");
        //        this.DbParameters.Add(dbParameter);
        //    }
        //    builder.Append(')');

        //    if (!this.IsBulk && this.Tables[0].Mapper.IsAutoIncrement)
        //        builder.Append(";SELECT SCOPE_IDENTITY()");
        //    return builder.ToString();
        //}
        if (!this.IsBulk && this.Tables[0].Mapper.IsAutoIncrement)
            return ";SELECT SCOPE_IDENTITY()";
        return string.Empty;
    }
}
