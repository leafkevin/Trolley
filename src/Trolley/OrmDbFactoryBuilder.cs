using System;
using System.Collections.Generic;
using System.Linq;

namespace Trolley;

public class OrmDbFactoryBuilder
{
    private readonly IOrmDbFactory dbFactory = new OrmDbFactory();
    public OrmDbFactoryBuilder Register(string dbKey, bool isDefault, Action<TheaDatabaseBuilder> databaseInitializer)
    {
        this.dbFactory.Register(dbKey, isDefault, databaseInitializer);
        return this;
    }
    public OrmDbFactoryBuilder AddTypeHandler(ITypeHandler typeHandler)
    {
        this.dbFactory.AddTypeHandler(typeHandler);
        return this;
    }
    public OrmDbFactoryBuilder Configure(Type ormProviderType, IModelConfiguration configuration)
    {
        this.dbFactory.Configure(ormProviderType, configuration);
        return this;
    }
    public IOrmDbFactory Build()
    {
        var ormProviderTypes = new List<Type>();
        var dbProviders = this.dbFactory.DatabaseProviders;
        foreach (var dbProvider in dbProviders)
        {
            var types = dbProvider.Databases.Select(f => f.OrmProviderType).Distinct();
            foreach (var ormProviderType in types)
            {
                if (!ormProviderTypes.Contains(ormProviderType))
                    ormProviderTypes.Add(ormProviderType);
            }
        }
        foreach (var ormProviderType in ormProviderTypes)
        {
            if (!this.dbFactory.TryGetOrmProvider(ormProviderType, out var ormProvider))
            {
                ormProvider = Activator.CreateInstance(ormProviderType) as IOrmProvider;
                this.dbFactory.AddOrmProvider(ormProvider);
            }
            if (!this.dbFactory.TryGetEntityMapProvider(ormProviderType, out var mapProvider))
            {
                mapProvider = new EntityMapProvider();
                this.dbFactory.AddEntityMapProvider(ormProviderType, mapProvider);
                foreach (var entityMapper in mapProvider.EntityMaps)
                {
                    entityMapper.Build(ormProvider, this.dbFactory.TypeHandlerProvider);
                }
            }
        }
        return this.dbFactory;
    }
}
