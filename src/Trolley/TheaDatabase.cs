using System;
using System.Collections.Generic;
using System.Threading;

namespace Trolley;

public class TheaDatabase
{
    private int masterRoundRobin = 0;
    private int slaveRoundRobin = 0;
    private readonly object masterLocker = new object();
    private readonly object slaveLocker = new object();
    private object masterSelector;
    private object slaveSelector;
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
    /// <summary>
    /// 设置主库连接串选择器，可自主设置选择主库规则，未设置则默认采用轮询方式，也可作为主库的分库规则，如：主库有2个数据库，分别为tenant1_master和tenant2_master，保存不同租户的数据，可使用此规则获取不同租户的数据，进行写操作
    /// </summary>
    /// <param name="connectionStringSelector"></param>
    public void UseMasterSelector(Func<string> connectionStringSelector)
        => this.masterSelector = connectionStringSelector;
    /// <summary>
    /// 设置从库连接串选择器，可自主设置选择从库规则，未设置则默认采用轮询方式，也可作为从库的分库规则，如：从库有2个数据库，分别为tenant1_slave和tenant2_slave，保存不同租户的数据，可使用此规则获取不同租户的数据，进行读操作
    /// </summary>
    /// <param name="connectionStringSelector"></param>
    public void UseSlaveSelector(Func<string> connectionStringSelector)
        => this.masterSelector = connectionStringSelector;
    /// <summary>
    /// 获取主库连接串
    /// </summary>
    /// <returns></returns>
    public string UseMaster()
    {
        var connectionStringSelector = this.masterSelector as Func<string>;
        return connectionStringSelector.Invoke();
    }
    /// <summary>
    /// 手动指定参数值获取主库连接串
    /// </summary>
    /// <param name="shardingBy">设置依赖的参数值</param>
    /// <returns>返回主库连接串</returns>
    public string UseMasterBy(object shardingBy)
    {
        var connectionStringSelector = this.masterSelector as Func<object, string>;
        return connectionStringSelector.Invoke(shardingBy);
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
    /// <summary>
    /// 手动指定参数值获取从库连接串，也可以作为从库的分库规则，如：.UseSlaveBy(tenant1) .UseSlaveBy("tenant2")，指定不同的租户ID，获取不同租户的数据，进行读操作
    /// </summary>
    /// <param name="shardingBy"></param>
    /// <returns></returns>
    public string UseSlaveBy(object shardingBy)
    {
        var connectionStringSelector = this.slaveSelector as Func<object, string>;
        return connectionStringSelector.Invoke(shardingBy);
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