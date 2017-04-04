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
        public static DbProviderFactory GetFactory(string assemblyQualifiedName)
        {
            var ft = Type.GetType(assemblyQualifiedName);
            return (DbProviderFactory)ft.GetField("Instance").GetValue(null);
        }
    }
}
