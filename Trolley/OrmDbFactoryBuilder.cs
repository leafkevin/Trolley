using System;

namespace Trolley;

public class OrmDbFactoryBuilder
{
    private readonly OrmDbFactory dbFactory;
    public OrmDbFactoryBuilder() => this.dbFactory = new OrmDbFactory();
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
    public IOrmDbFactory Build() => this.dbFactory.Build();
}
