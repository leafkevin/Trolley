using MySqlConnector;

namespace Trolley.MySqlConnector;

public static class MySqlExtensions
{
    public static IMySqlRepository Create(this IOrmDbFactory dbFactory, string dbKey = null)
        => dbFactory.CreateRepository(dbKey) as IMySqlRepository;
    public static MemberBuilder<TMember> NativeDbType<TMember>(this MemberBuilder<TMember> builder, MySqlDbType nativeDbType)
       => builder.NativeDbType(nativeDbType); 
}
