using System.Text;

namespace Trolley.MySqlConnector;

class MySqlCreateVisitor : CreateVisitor, ICreateVisitor
{
    public MySqlCreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix) { }

    public override string BuildHeadSql()
    {
        if (this.IsUseIgnore) return "INSERT IGNORE INTO";
        return "INSERT INTO";
    }
    public override string BuildTailSql()
    {
        if (this.IsUseOrUpdate)
        {
            var index = 0;
            var builder = new StringBuilder(" ON DUPLICATE KEY UPDATE ");
            foreach (var updateField in this.UpdateFields)
            {
                if (index > 0) builder.Append(',');
                var fieldName = this.OrmProvider.GetFieldName(updateField.MemberMapper.FieldName);
                builder.Append($"{fieldName}={updateField.Value}");
                index++;
            }
            return builder.ToString();
        }
        if (this.Tables[0].Mapper.IsAutoIncrement && !this.IsBulk && !this.IsUseOrUpdate)
            return ";SELECT LAST_INSERT_ID()";
        if (this.IsUseIfNotExists)
        {

        }
            return string.Empty;
    }
}