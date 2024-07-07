using System.Data;

namespace Trolley.SqlServer;

public static class SqlServerExtensions
{
    public static ISqlServerRepository Create(this IOrmDbFactory dbFactory, string dbKey = null)
        => dbFactory.CreateRepository(dbKey) as ISqlServerRepository;
    public static MemberBuilder<TMember> NativeDbType<TMember>(this MemberBuilder<TMember> builder, SqlDbType nativeDbType)
        => builder.NativeDbType(nativeDbType);
}
