using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Trolley.AspNetCore;

public static class TrolleyExtensions
{
    public static IServiceCollection AddTrolley(this IServiceCollection services)
    {
        services.AddSingleton<IOrmDbFactory, OrmDbFactory>();
        return services;
    }
    public static IApplicationBuilder UseTrolley(this IApplicationBuilder app, Action<OrmDbFactoryBuilder> initializer)
    {
        var dbFactory = app.ApplicationServices.GetService<IOrmDbFactory>();
        var builder = new OrmDbFactoryBuilder(dbFactory);
        initializer?.Invoke(builder);
        return app;
    }
   
}