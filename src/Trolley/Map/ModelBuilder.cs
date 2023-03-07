using System;

namespace Trolley;

public class ModelBuilder
{
    private readonly IEntityMapProvider mapProvider;

    public ModelBuilder(IEntityMapProvider mapProvider)
        => this.mapProvider = mapProvider;

    public virtual ModelBuilder Entity<TEntity>(Action<EntityBuilder<TEntity>> initializer) where TEntity : class
    {
        var entityType = typeof(TEntity);
        if (initializer == null)
            throw new ArgumentNullException(nameof(initializer));

        var mapper = new EntityMap(entityType);
        var builder = new EntityBuilder<TEntity>(mapper);
        initializer.Invoke(builder);
        this.mapProvider.AddEntityMap(entityType, mapper);
        return this;
    }
}