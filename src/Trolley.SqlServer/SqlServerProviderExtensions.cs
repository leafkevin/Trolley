using System.Data;

namespace Trolley.SqlServer;

public static class SqlServerProviderExtensions
{
    public static MemberBuilder<TMember> NativeDbType<TMember>(this MemberBuilder<TMember> builder, SqlDbType nativeDbType)
        => builder.NativeDbType(nativeDbType);
}
