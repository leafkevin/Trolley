using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class OrmDbFactory : IOrmDbFactory
{
    private readonly ConcurrentDictionary<Type, IOrmProvider> ormProviders = new();
    private readonly ConcurrentDictionary<string, TheaDatabase> databases = new();
    private readonly ConcurrentDictionary<Type, EntityMap> entityMappers = new();

    private readonly IServiceProvider serviceProvider;
    private TheaDatabase defaultDatabase;
    public OrmDbFactory(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

    public TheaDatabase Register(string dbKey, bool isDefault, Action<TheaDatabaseBuilder> databaseInitializer)
    {
        if (!this.databases.TryGetValue(dbKey, out var database))
        {
            this.databases.TryAdd(dbKey, database = new TheaDatabase
            {
                DbKey = dbKey,
                IsDefault = isDefault,
                ConnectionStrings = new List<TheaConnectionInfo>()
            });
        }
        if (isDefault) this.defaultDatabase = database;
        var builder = new TheaDatabaseBuilder(database);
        databaseInitializer?.Invoke(builder);
        return database;
    }
    //public void LoadFromConfigure(string sectionName)
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
    //}
    public void Configure(Action<ModelBuilder> modelInitializer)
    {
        var builder = new ModelBuilder(this);
        modelInitializer.Invoke(builder);
    }
    public IRepository Create(TheaConnection connection) => new Repository(this, connection);
    public IRepository Create(string dbKey = null, int? tenantId = null)
    {
        var connectionInfo = this.GetConnectionInfo(dbKey, tenantId);
        var connection = new TheaConnection(connectionInfo);
        return new Repository(this, connection);
    }

    //public SqlExpression<T1, T2> From<T1, T2>(string dbKey = null, int? tenantId = null)
    //{
    //    var connectionInfo = this.GetConnectionInfo(dbKey, tenantId);
    //    var connection = new TheaConnection(connectionInfo);
    //    var visitor = new SqlExpressionVisitor(this, connection.OrmProvider);
    //    return new SqlExpression<T1, T2>(this, connection, visitor);
    //}
    //public SqlExpression<T1, T2, T3> From<T1, T2, T3>(string dbKey = null, int? tenantId = null)
    //{
    //    var connectionInfo = this.GetConnectionInfo(dbKey, tenantId);
    //    var connection = new TheaConnection(connectionInfo);
    //    var visitor = new SqlExpressionVisitor(this, connection.OrmProvider);
    //    return new SqlExpression<T>(this, connection, visitor);
    //}
    //public SqlExpression<T> From<T>(string dbKey = null, int? tenantId = null)
    //{
    //    var connectionInfo = this.GetConnectionInfo(dbKey, tenantId);
    //    var connection = new TheaConnection(connectionInfo);
    //    var visitor = new SqlExpressionVisitor(this, connection.OrmProvider);
    //    return new SqlExpression<T>(this, connection, visitor);
    //}
    public TheaDatabase GetDatabase(string dbKey = null)
    {
        TheaDatabase database = null;
        if (string.IsNullOrEmpty(dbKey))
            database = this.defaultDatabase;
        else if (!this.databases.TryGetValue(dbKey, out database))
            throw new Exception($"未配置dbKey:{dbKey}数据库连接串");
        return database;
    }
    public TheaConnectionInfo GetConnectionInfo(string dbKey = null, int? tenantId = null)
    {
        var database = this.GetDatabase(dbKey);
        return database.GetConnectionInfo(tenantId);
    }
    public void AddEntityMap(Type entityType, EntityMap mapper)
        => this.entityMappers.TryAdd(entityType, mapper);
    public bool TryGetEntityMap(Type entityType, out EntityMap mapper)
        => this.entityMappers.TryGetValue(entityType, out mapper);
}
