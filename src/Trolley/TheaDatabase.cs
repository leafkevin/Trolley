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

    public string UseSelector(params object[] connectionStringSelectorValues)
    {
        if (this.ConnectionStringSelector == null)
            throw new InvalidOperationException("ConnectionStringSelector委托未设置，无法选择主库连接串");
        if (connectionStringSelectorValues != null && connectionStringSelectorValues.Length > 0)
        {
            if (connectionStringSelectorValues.Length != this.ConnectionStringSelector.Method.GetParameters().Length)
                throw new ArgumentException("connectionStringSelectorValues参数个数与ConnectionStringSelector委托参数个数不匹配");
            return this.ConnectionStringSelector.DynamicInvoke(connectionStringSelectorValues) as string;
        }
        return this.ConnectionStringSelector.DynamicInvoke() as string;
    }
    public string UseSlaveSelector(params object[] slaveConnectionStringSelectorValues)
    {
        if (this.SlaveConnectionStringSelector == null)
            throw new InvalidOperationException("SlaveConnectionStringSelector委托未设置，无法选择从库连接串");
        if (slaveConnectionStringSelectorValues != null && slaveConnectionStringSelectorValues.Length > 0)
        {
            if (slaveConnectionStringSelectorValues.Length != this.ConnectionStringSelector.Method.GetParameters().Length)
                throw new ArgumentException("connectionStringSelectorValues参数个数与ConnectionStringSelector委托参数个数不匹配");
            return this.SlaveConnectionStringSelector.DynamicInvoke(slaveConnectionStringSelectorValues) as string;
        }
        return this.SlaveConnectionStringSelector.DynamicInvoke() as string;
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
                    index %= this.ConnectionStrings.Count;
                    return this.ConnectionStrings[index];
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
                index %= this.SlaveConnectionStrings.Count;
                return this.SlaveConnectionStrings[index];
            };
        }
    }
    public override int GetHashCode() => this.DbKey.GetHashCode();
}