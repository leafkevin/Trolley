using System;
using System.Collections.Generic;
using System.Threading;

namespace Trolley;

public class TheaDatabase
{
    private int masterRoundRobin;
    private int slaveRoundRobin;
    public string DbKey { get; internal set; }
    public List<string> ConnectionStrings { get; internal set; }
    public Delegate ConnectionStringSelector { get; internal set; }
    public List<string> SlaveConnectionStrings { get; internal set; }
    public Delegate SlaveConnectionStringSelector { get; internal set; }
    public bool IsDefault { get; internal set; }
    public OrmProviderType OrmProviderType { get; internal set; }
    public IOrmProvider OrmProvider { get; internal set; }
    public IEntityMapProvider EntityMapProvider { get; internal set; }
    public ITableShardingProvider TableShardingProvider { get; internal set; }


    public void Use(params string[] connectionStrings)
    {
        if (connectionStrings == null || connectionStrings.Length == 0)
            throw new ArgumentNullException(nameof(connectionStrings));
        this.ConnectionStrings ??= new();
        this.ConnectionStrings.AddRange(connectionStrings);
    }
    public void Use(string[] connectionStrings, Func<object[], string> connectionStringSelector)
    {
        this.Use(connectionStrings);
        if (connectionStringSelector == null)
            throw new ArgumentNullException(nameof(connectionStringSelector));
        this.ConnectionStringSelector = connectionStringSelector;
    }
    public string Select(params object[] selectorValues)
    {
        if (this.ConnectionStringSelector == null)
            throw new InvalidOperationException("主库连接串选择器未设置");
        return this.ConnectionStringSelector.DynamicInvoke(selectorValues) as string;
    }
    public void UseSlave(params string[] connectionStrings)
    {
        if (connectionStrings == null || connectionStrings.Length == 0)
            throw new ArgumentNullException(nameof(connectionStrings));
        this.SlaveConnectionStrings ??= new();
        this.SlaveConnectionStrings.AddRange(connectionStrings);
    }
    public void UseSlave(string[] connectionStrings, Func<object[], string> connectionStringSelector)
    {
        this.UseSlave(connectionStrings);
        if (connectionStringSelector == null)
            throw new ArgumentNullException(nameof(connectionStringSelector));
        this.SlaveConnectionStringSelector = connectionStringSelector;
    }
    public string SelectSlave(params object[] selectorValues)
    {
        if (this.SlaveConnectionStringSelector == null)
            throw new InvalidOperationException("从库连接串选择器未设置");
        return this.SlaveConnectionStringSelector.DynamicInvoke(selectorValues) as string;
    }
    public void UseOrmProvider(IOrmProvider ormProvider)
    {
        if (ormProvider == null)
            throw new ArgumentNullException(nameof(ormProvider));
        this.OrmProvider = ormProvider;
        this.OrmProviderType = ormProvider.OrmProviderType;
    }
    public void UseEntityMapProvider(IEntityMapProvider entityMapProvider)
    {
        if (entityMapProvider == null)
            throw new ArgumentNullException(nameof(entityMapProvider));
        this.EntityMapProvider = entityMapProvider;
    }
    public void UseTableShardingProvider(ITableShardingProvider tableShardingProvider)
    {
        if (tableShardingProvider == null)
            throw new ArgumentNullException(nameof(tableShardingProvider));
        this.TableShardingProvider = tableShardingProvider;
    }

    public void Build()
    {
        if (this.ConnectionStrings == null || this.ConnectionStrings.Count == 0)
            throw new InvalidOperationException("没有配置可用的主库连接串");

        // 如果未设置主库连接串选择器，则默认设置为轮询方式选择器
        if (this.ConnectionStrings.Count > 0 && this.ConnectionStringSelector == null)
        {
            if (this.ConnectionStrings.Count == 1)
                this.ConnectionStringSelector = () => this.ConnectionStrings[0];
            else
            {
                this.ConnectionStringSelector = () =>
                {
                    int index = 0;
                    unchecked
                    {
                        index = Interlocked.Increment(ref this.masterRoundRobin);
                        // 溢出时为负数，重置为0
                        if (index < 0)
                        {
                            Interlocked.Exchange(ref this.masterRoundRobin, 0);
                            index = 0;
                        }
                    }
                    return this.ConnectionStrings[index %= this.ConnectionStrings.Count];
                };
            }
        }
        // 如果已经设置了从库选择器，则不再设置默认的轮询方式选择器
        if (this.SlaveConnectionStringSelector != null) return;
        if (this.SlaveConnectionStrings == null || this.SlaveConnectionStrings.Count == 0)
        {
            this.SlaveConnectionStringSelector = this.ConnectionStringSelector;
            return;
        }
        if (this.SlaveConnectionStrings.Count == 1)
            this.SlaveConnectionStringSelector = () => this.SlaveConnectionStrings[0];
        else
        {
            this.SlaveConnectionStringSelector = () =>
            {
                int index = 0;
                unchecked
                {
                    index = Interlocked.Increment(ref this.slaveRoundRobin);
                    // 溢出时为负数，重置为0
                    if (index < 0)
                    {
                        Interlocked.Exchange(ref this.slaveRoundRobin, 0);
                        index = 0;
                    }
                }
                return this.SlaveConnectionStrings[index %= this.SlaveConnectionStrings.Count];
            };
        }
    }
    public override int GetHashCode() => this.DbKey.GetHashCode();
}