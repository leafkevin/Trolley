using NpgsqlTypes;

namespace Trolley.PostgreSql;

public static class PostgreSqlExtensions
{
    public static IPostgreSqlRepository Create(this IOrmDbFactory dbFactory, string dbKey = null)
        => dbFactory.CreateRepository(dbKey) as IPostgreSqlRepository;
    public static MemberBuilder<TMember> NativeDbType<TMember>(this MemberBuilder<TMember> builder, NpgsqlDbType nativeDbType)
        => builder.NativeDbType(nativeDbType);
}
