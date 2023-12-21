using MySqlConnector;

namespace Trolley.MySqlConnector;

public static class MySqlExtensions
{
    public static MemberBuilder<TMember> NativeDbType<TMember>(this MemberBuilder<TMember> builder, MySqlDbType nativeDbType)
       => builder.NativeDbType(nativeDbType); 
}
