using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class OrmDbFactoryBuilder
{
    private OrmDbFactoryOptions options;
    private TheaDatabase defaultDatabase;
    private readonly List<Type> ormProviderTypes = new();
    private readonly ConcurrentDictionary<string, TheaDatabase> databases = new();
    private readonly ConcurrentDictionary<Type, IEntityMapProvider> mapProviders = new();
    private readonly ConcurrentDictionary<Type, ITypeHandler> typeHandlers = new();

    public OrmDbFactoryBuilder Register<TOrmProvider>(string dbKey, bool isDefault, Action<TheaDatabaseBuilder> databaseInitializer) where TOrmProvider : class, IOrmProvider, new()
    {
        var ormProviderType = typeof(TOrmProvider);
        var database = this.databases.GetOrAdd(dbKey, new TheaDatabase
        {
            DbKey = dbKey,
            IsDefault = isDefault,
            OrmProviderType = ormProviderType
        });
        if (!this.ormProviderTypes.Contains(ormProviderType))
            this.ormProviderTypes.Add(ormProviderType);
        if (isDefault) this.defaultDatabase = database;
        var builder = new TheaDatabaseBuilder(database);
        databaseInitializer?.Invoke(builder);
        return this;
    }
    public OrmDbFactoryBuilder AddTypeHandler(ITypeHandler typeHandler)
    {
        if (typeHandler == null)
            throw new ArgumentNullException(nameof(typeHandler));

        this.typeHandlers.TryAdd(typeHandler.GetType(), typeHandler);
        return this;
    }
    public OrmDbFactoryBuilder Configure(Type ormProviderType, IModelConfiguration configuration)
    {
        if (ormProviderType == null)
            throw new ArgumentNullException(nameof(ormProviderType));

        var mapProvider = this.mapProviders.GetOrAdd(ormProviderType, new EntityMapProvider { OrmProviderType = ormProviderType });
        configuration.OnModelCreating(new ModelBuilder(mapProvider));
        return this;
    }
    public OrmDbFactoryBuilder With(Action<OrmDbFactoryOptions> optionsInitializer)
    {
        if (optionsInitializer == null)
            throw new ArgumentNullException(nameof(optionsInitializer));
        this.options = new OrmDbFactoryOptions();
        optionsInitializer.Invoke(this.options);
        return this;
    }
    public IOrmDbFactory Build()
    {
        var dbFactory = new OrmDbFactory();
        dbFactory.Initialize(this.options, this.defaultDatabase, this.ormProviderTypes, this.databases, this.mapProviders, this.typeHandlers);
        foreach (var ormProviderType in this.ormProviderTypes)
        {
            if (!dbFactory.TryGetMapProvider(ormProviderType, out var mapProvider))
                dbFactory.AddMapProvider(ormProviderType, new EntityMapProvider { OrmProviderType = ormProviderType });
            dbFactory.TryGetOrmProvider(ormProviderType, out var ormProvider);
            mapProvider.Build(ormProvider);
        }
        return dbFactory;
    }
}
