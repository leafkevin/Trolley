using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Trolley.AspNetCore;

public static class TrolleyExtensions
{
    public static IServiceCollection AddTrolley(this IServiceCollection services, Action<OrmDbFactoryBuilder> initializer)
    {
        var builder = new OrmDbFactoryBuilder();
        initializer.Invoke(builder);
        services.AddSingleton(builder.Build());
        return services;
    }
    public static IOrmDbFactory LoadFromConfiguration(this OrmDbFactoryBuilder builder, IConfiguration configuration, string sectionName)
    {
        var databases = configuration.GetSection(sectionName).GetChildren();
        foreach (var configInfo in databases)
        {
            var database = new TheaDatabase { DbKey = configInfo.Key };
            configInfo.Bind(database);
            var connStrings = configInfo.GetSection("ConnectionStrings").GetChildren();
            foreach (var connString in connStrings)
            {
                var connectionInfo = new TheaConnectionInfo { DbKey = configInfo.Key };
                connString.Bind(connectionInfo);
                var ormProviderTypeName = connString.GetValue<string>("OrmProvider");
                var ormProviderType = typeof(IOrmDbFactory).Assembly.GetType(ormProviderTypeName);
                connectionInfo.OrmProvider = Activator.CreateInstance(ormProviderType) as IOrmProvider;
                builder.Register(database.DbKey, connectionInfo.IsDefault, f => f.Add(connectionInfo));
            }
        }
        return builder.Build();
    }
}