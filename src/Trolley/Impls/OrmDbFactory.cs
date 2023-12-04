using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class OrmDbFactory : IOrmDbFactory
{
    private OrmDbFactoryOptions options;
    private TheaDatabase defaultDatabase;
    private ConcurrentDictionary<Type, IOrmProvider> ormProviders;
    private ConcurrentDictionary<string, TheaDatabase> databases;
    private ConcurrentDictionary<Type, IEntityMapProvider> mapProviders;
    private ConcurrentDictionary<Type, ITypeHandler> typeHandlers;

    public virtual ICollection<TheaDatabase> Databases => this.databases?.Values;
    public virtual ICollection<ITypeHandler> TypeHandlers => this.typeHandlers?.Values;

    public virtual void AddDatabase(TheaDatabase database)
    {
        if (database == null) throw new ArgumentNullException(nameof(database));
        if (!this.databases.TryAdd(database.DbKey, database))
            throw new Exception($"dbKey:{database.DbKey}数据库已经添加");
        if (database.IsDefault) this.defaultDatabase = database;
    }
    public virtual void AddTypeHandler(ITypeHandler typeHandler)
    {
        if (typeHandler == null)
            throw new ArgumentNullException(nameof(typeHandler));

        this.typeHandlers.TryAdd(typeHandler.GetType(), typeHandler);
    }
    public virtual bool TryGetTypeHandler(Type handlerType, out ITypeHandler typeHandler)
    {
        if (handlerType == null)
            throw new ArgumentNullException(nameof(handlerType));

        return this.typeHandlers.TryGetValue(handlerType, out typeHandler);
    }

    public virtual void AddOrmProvider(IOrmProvider ormProvider)
    {
        if (ormProvider == null)
            throw new ArgumentNullException(nameof(ormProvider));

        var ormProviderType = ormProvider.GetType();
        this.ormProviders.TryAdd(ormProviderType, ormProvider);
    }
    public virtual bool TryGetOrmProvider(Type ormProviderType, out IOrmProvider ormProvider)
    {
        if (ormProviderType == null)
            throw new ArgumentNullException(nameof(ormProviderType));

        return this.ormProviders.TryGetValue(ormProviderType, out ormProvider);
    }
    public virtual void AddMapProvider(Type ormProviderType, IEntityMapProvider entityMapProvider)
    {
        if (ormProviderType == null)
            throw new ArgumentNullException(nameof(ormProviderType));
        if (entityMapProvider == null)
            throw new ArgumentNullException(nameof(entityMapProvider));

        this.mapProviders.TryAdd(ormProviderType, entityMapProvider);
    }
    public virtual bool TryGetMapProvider(Type ormProviderType, out IEntityMapProvider entityMapProvider)
    {
        if (ormProviderType == null)
            throw new ArgumentNullException(nameof(ormProviderType));

        return this.mapProviders.TryGetValue(ormProviderType, out entityMapProvider);
    }
    public virtual TheaDatabase GetDatabase(string dbKey = null)
    {
        if (string.IsNullOrEmpty(dbKey))
        {
            if (this.defaultDatabase == null)
                throw new Exception($"未配置默认数据库连接串");
            return this.defaultDatabase;
        }
        if (!this.databases.TryGetValue(dbKey, out var database))
            throw new Exception($"dbKey:{dbKey}未配置任何数据库连接串");
        return database;
    }
    public virtual IRepository Create(string dbKey = null, string tenantId = null)
    {
        var database = this.GetDatabase(dbKey);
        if (!this.TryGetOrmProvider(database.OrmProviderType, out var ormProvider))
            throw new Exception($"未注册类型为{database.OrmProviderType.FullName}的OrmProvider");
        var tenantDatabase = database.GetTenantDatabase(tenantId);
        if (!this.TryGetMapProvider(database.OrmProviderType, out var mapProvider))
            throw new Exception($"未注册Key为{database.OrmProviderType.FullName}的EntityMapProvider");
        var connection = new TheaConnection
        {
            DbKey = dbKey,
            BaseConnection = ormProvider.CreateConnection(tenantDatabase.ConnectionString),
            OrmProvider = ormProvider
        };
        return new Repository(dbKey, connection, ormProvider, mapProvider).With(this.options);
    }
    public void With(OrmDbFactoryOptions options) => this.options = options;
    public void Build(Type ormProviderType)
    {
        if (!this.TryGetOrmProvider(ormProviderType, out var ormProvider))
            throw new Exception($"未注册类型为{ormProviderType.FullName}的OrmProvider");
        if (!this.TryGetMapProvider(ormProviderType, out var mapProvider))
            throw new Exception($"请调用IOrmDbFactory.Configure(Type ormProviderType, IModelConfiguration configuration)后，再Build实体映射");
        mapProvider.Build(ormProvider);
    }
    internal void Initialize(OrmDbFactoryOptions options, TheaDatabase defaultDatabase, List<Type> ormProviderTypes, ConcurrentDictionary<string, TheaDatabase> databases,
        ConcurrentDictionary<Type, IEntityMapProvider> mapProviders, ConcurrentDictionary<Type, ITypeHandler> typeHandlers)
    {
        this.options = options;
        this.defaultDatabase = defaultDatabase;
        this.ormProviders = new();
        ormProviderTypes.ForEach(f =>
        {
            var ormProvider = Activator.CreateInstance(f) as IOrmProvider;
            this.ormProviders.TryAdd(f, ormProvider);
        });
        this.databases = databases;
        this.mapProviders = mapProviders;
        this.typeHandlers = typeHandlers;
    }
}
