using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace Trolley
{
    public class OrmProviderFactory
    {
        private static Dictionary<string, IOrmProvider> providers = new Dictionary<string, IOrmProvider>();
        public static string DefaultConnString { get; private set; }
        public static IOrmProvider DefaultProvider { get; private set; }
        public static void RegisterProvider(string connString, IOrmProvider provider, bool isDefaultProvider = true)
        {
            if (isDefaultProvider)
            {
                DefaultConnString = connString;
                DefaultProvider = provider;
            }
            providers.Add(connString, provider);
        }
        public static IOrmProvider GetProvider(string connString)
        {
            return OrmProviderFactory.providers[connString];
        }
        public static DbProviderFactory PostgreSqlFactory()
        {
            return GetFactory("Npgsql.NpgsqlFactory, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
        }
        public static DbProviderFactory MySqlFactory()
        {
            return GetFactory("MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Culture=neutral, PublicKeyToken=c5687fc88969c44d");
        }
        public static DbProviderFactory SqlServerFactory()
        {
            return GetFactory("System.Data.SqlClient.SqlClientFactory, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        }
        public static DbProviderFactory SQLiteFactory()
        {
            return GetFactory("System.Data.SQLite.SQLiteFactory, System.Data.SQLite, Culture=neutral, PublicKeyToken=db937bc2d44ff139");
        }
        public static DbProviderFactory GetFactory(string assemblyQualifiedName)
        {
            var ft = Type.GetType(assemblyQualifiedName);
            return (DbProviderFactory)ft.GetField("Instance").GetValue(null);
        }
    }
}
