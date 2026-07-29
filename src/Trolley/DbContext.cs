namespace Trolley;

public sealed class DbContext
{
    //默认配置的副本，方便单次设置有效
    private OrmDbFactoryOptions options = new OrmDbFactoryOptions();

    #region Properties
    public string DbKey { get; internal set; }
    public string ConnectionString { get; internal set; }
    public TheaDatabase Database { get; internal set; }
    public ITheaConnection Connection { get; set; }
    public ITheaTransaction Transaction { get; set; }
    public string DefaultTableSchema { get; internal set; }
    public IOrmProvider OrmProvider => this.Database.OrmProvider;
    public IEntityMapProvider EntityMapProvider => this.Database.EntityMapProvider;
    public ITableShardingProvider TableShardingProvider => this.Database.TableShardingProvider;
    public IDbInterceptor Interceptor { get; internal set; }
    public OrmDbFactoryOptions Options => this.options;
    #endregion
}