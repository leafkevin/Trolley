using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Trolley;

public class OrmDbFactoryBuilder
{
    private readonly IOrmDbFactory dbFactory;
    internal OrmDbFactoryBuilder(IOrmDbFactory dbFactory) => this.dbFactory = dbFactory;
    public OrmDbFactoryBuilder Register(string dbKey, bool isDefault, Action<TheaDatabaseBuilder> databaseInitializer)
    {
        this.dbFactory.Register(dbKey, isDefault, databaseInitializer);
        return this;
    }
    public OrmDbFactoryBuilder Configure(IModelConfiguration configuration)
    {
        var builder = new ModelBuilder(this.dbFactory);
        configuration.OnModelCreating(builder);
        return this;
    }
    public OrmDbFactoryBuilder Configure<TModelConfiguration>() where TModelConfiguration : class, IModelConfiguration, new()
    {
        var builder = new ModelBuilder(this.dbFactory);
        var configuration = new TModelConfiguration();
        configuration.OnModelCreating(builder);
        return this;
    }
    //public OrmDbFactoryBuilder LoadFromConfigure(string sectionName)
    //{
    //    var configuration = this.serviceProvider.GetService<IConfiguration>();
    //    var databases = configuration.GetSection(sectionName).GetChildren();
    //    foreach (var configInfo in databases)
    //    {
    //        var isDefault = configInfo.GetValue<bool>("IsDefault");
    //        var database = new TheaDatabase
    //        {
    //            DbKey = configInfo.Key,
    //            IsDefault = isDefault,
    //            ConnectionStrings = new List<TheaConnectionInfo>()
    //        };
    //        if (isDefault) this.defaultDatabase = database;
    //        this.databases.TryAdd(configInfo.Key, database);

    //        var connStrings = configInfo.GetSection("ConnectionStrings").GetChildren();
    //        foreach (var connString in connStrings)
    //        {
    //            var theaConnString = new TheaConnectionInfo { DbKey = configInfo.Key };
    //            connString.Bind(theaConnString);
    //            var ormProviderTypeName = connString.GetValue<string>("OrmProvider");
    //            var ormProviderType = Assembly.GetExecutingAssembly().GetType(ormProviderTypeName);
    //            if (!ormProviders.TryGetValue(ormProviderType, out var ormProvider))
    //            {
    //                var instance = TheaActivator.CreateInstance(this.serviceProvider, ormProviderType);
    //                ormProviders.TryAdd(ormProviderType, ormProvider = instance as IOrmProvider);
    //            }
    //            theaConnString.OrmProvider = ormProvider;
    //            database.ConnectionStrings.Add(theaConnString);
    //        }
    //    }
    //    return this;
    //}
}
