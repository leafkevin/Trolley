using System;

namespace Trolley;

public class ModelBuilder
{
    private readonly IOrmDbFactory dbFactory;
    public ModelBuilder(IOrmDbFactory dbFactory)
        => this.dbFactory = dbFactory;

    public virtual ModelBuilder Entity<TEntity>(Action<EntityBuilder<TEntity>> initializer) where TEntity : class
    {
        var entityType = typeof(TEntity);
        if (initializer == null)
            throw new ArgumentNullException(nameof(initializer));

        var mapper = new EntityMap(entityType);
        var builder = new EntityBuilder<TEntity>(this.dbFactory, mapper);
        initializer.Invoke(builder);
        this.dbFactory.AddEntityMap(entityType, builder.Build());
        return this;
    }
}