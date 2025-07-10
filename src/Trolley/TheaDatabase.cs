using System;
using System.Collections.Generic;
using System.Threading;

namespace Trolley;

public class TheaDatabase
{
    private Delegate masterSelector;
    private Delegate slaveSelector;
    public string DbKey { get; internal set; }
    /// <summary>
    /// 主库连接串，可以多个，多个主库连接串时，使用轮询方式选择主库连接串
    /// </summary>
    public List<string> MasterConnectionStrings { get; internal set; }
    /// <summary>
    /// 从库连接串，可以多个，多个从库连接串时，使用轮询方式选择从库连接串
    /// </summary>
    public List<string> SlaveConnectionStrings { get; internal set; }
    public bool IsDefault { get; internal set; }
    public OrmProviderType OrmProviderType { get; internal set; }
    public IOrmProvider OrmProvider { get; internal set; }
    public void UseMasterSelector(Delegate connectionStringSelector)
        => this.masterSelector = connectionStringSelector;
    public void UseSlaveSelector(Delegate connectionStringSelector)
        => this.slaveSelector = connectionStringSelector;
    /// <summary>
    /// 获取主库连接串
    /// </summary>
    /// <returns></returns>
    public string UseMaster()
    {
        var connectionStringSelector = this.masterSelector as Func<string>;
        return connectionStringSelector.Invoke();
    }
    public string UseMasterBy(object shardingParameter)
    {
        var connectionStringSelector = this.masterSelector as Func<object, string>;
        return connectionStringSelector.Invoke(shardingParameter);
    }
    /// <summary>
    /// 获取从库连接串
    /// </summary>
    /// <returns></returns>
    public string UseSlave()
    {
        var connectionStringSelector = this.slaveSelector as Func<string>;
        return connectionStringSelector.Invoke();
    }
    public string UseSlaveBy(object shardingParameter)
    {
        var connectionStringSelector = this.slaveSelector as Func<object, string>;
        return connectionStringSelector.Invoke(shardingParameter);
    }
    public void Build()
    {
        if (string.IsNullOrWhiteSpace(this.DbKey))
            throw new InvalidOperationException("dbKey不能为空");
        if (this.OrmProvider == null)
            throw new InvalidOperationException($"dbKey:{this.DbKey}未设置数据库提供者ormProvider");

        if (this.MasterConnectionStrings == null || this.MasterConnectionStrings.Count == 0)
            throw new InvalidOperationException("没有配置可用的主库连接串");

        // 如果没有设置主库选择器，则使用默认的轮询方式选择器
        if (this.masterSelector == null)
        {
            if (this.MasterConnectionStrings.Count == 1)
                this.masterSelector = () => this.MasterConnectionStrings[0];
            else
            {
                this.masterSelector = () =>
                {
                    lock (this.masterLocker)
                    {
                        var index = Interlocked.Increment(ref this.masterRoundRobin);
                        Interlocked.CompareExchange(ref this.masterRoundRobin, 0, int.MaxValue);
                        index %= this.MasterConnectionStrings.Count;
                        return this.MasterConnectionStrings[index];
                    }
                };
            }
        }
        // 如果已经设置了从库选择器，则不再设置默认的轮询方式选择器
        if (this.slaveSelector != null) return;
        if (this.SlaveConnectionStrings == null || this.SlaveConnectionStrings.Count == 0)
        {
            this.slaveSelector = () => this.UseMaster();
            return;
        }
        if (this.SlaveConnectionStrings.Count == 1)
            this.slaveSelector = () => this.SlaveConnectionStrings[0];
        else
        {
            this.slaveSelector = () =>
            {
                lock (this.slaveLocker)
                {
                    var index = Interlocked.Increment(ref this.slaveRoundRobin);
                    Interlocked.CompareExchange(ref this.slaveRoundRobin, 0, int.MaxValue);
                    index %= this.SlaveConnectionStrings.Count;
                    return this.SlaveConnectionStrings[index];
                }
            };
        }
    }
    public override int GetHashCode() => this.DbKey.GetHashCode();
}