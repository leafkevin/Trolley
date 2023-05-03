using MySqlConnector;

namespace Trolley.SqlServer;

public static class MySqlProviderExtensions
{
    public static MemberBuilder<TMember> NativeDbType<TMember>(this MemberBuilder<TMember> builder, MySqlDbType nativeDbType)
        => builder.NativeDbType(nativeDbType);
}
