namespace Trolley;

public static class TrolleyExtensions
{
    public static OrmDbFactoryBuilder AddTypeHandler<TTypeHandler>(this OrmDbFactoryBuilder builder) where TTypeHandler : class, ITypeHandler, new()
        => builder.AddTypeHandler(new TTypeHandler());
    public static OrmDbFactoryBuilder Configure<TOrmProvider>(this OrmDbFactoryBuilder builder, IModelConfiguration configuration)
    {
        builder.Configure(typeof(TOrmProvider), configuration);
        return builder;
    }
    public static OrmDbFactoryBuilder Configure<TOrmProvider, TModelConfiguration>(this OrmDbFactoryBuilder builder) where TModelConfiguration : class, IModelConfiguration, new()
    {
        builder.Configure(typeof(TOrmProvider), new TModelConfiguration());
        return builder;
    }
}
