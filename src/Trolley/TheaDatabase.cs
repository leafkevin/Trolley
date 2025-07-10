using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;

namespace Trolley;

public class TheaDatabase
{
    private int masterRoundRobin;
    private int slaveRoundRobin;
    private Delegate masterSelector;
    private Delegate slaveSelector;
    public string DbKey { get; internal set; }
    public List<string> MasterConnectionStrings { get; internal set; }
    public List<string> SlaveConnectionStrings { get; internal set; }
    public bool IsDefault { get; internal set; }
    public OrmProviderType OrmProviderType { get; internal set; }
    public IOrmProvider OrmProvider { get; internal set; }
    public void UseMasterSelector(Delegate connectionStringSelector)
        => this.masterSelector = connectionStringSelector;
    public void UseSlaveSelector(Delegate connectionStringSelector)
        => this.slaveSelector = connectionStringSelector;
    public string UseMaster()
    {
        var connectionStringSelector = this.masterSelector as Func<string>;
        if (connectionStringSelector == null)
            throw new InvalidOperationException("主库连接串选择器未设置或参数类型不正确，无法转换为Func<string>类型");
        return connectionStringSelector.Invoke();
    }
    public string UseMasterBy(params object[] fieldValues)
    {
        if (fieldValues == null || fieldValues.Length == 0)
            throw new ArgumentNullException(nameof(fieldValues), "fieldValues参数不能为空");
        string result = null;
        switch (fieldValues.Length)
        {
            case 1:
                var connectionStringSelector1 = this.masterSelector as Func<object, string>;
                if (connectionStringSelector1 == null) throw new InvalidOperationException("主库连接串选择器未设置或或参数类型不正确，无法转换为Func<object, string>类型");
                result = connectionStringSelector1.Invoke(fieldValues[0]);
                break;
            case 2:
                var connectionStringSelector2 = this.masterSelector as Func<object, object, string>;
                if (connectionStringSelector2 == null) throw new InvalidOperationException("主库连接串选择器未设置或或参数类型不正确，无法转换为Func<object, object, string>类型");
                result = connectionStringSelector2.Invoke(fieldValues[0], fieldValues[1]);
                break;
            case 3:
                var connectionStringSelector3 = this.masterSelector as Func<object, object, object, string>;
                if (connectionStringSelector3 == null) throw new InvalidOperationException("主库连接串选择器未设置或或参数类型不正确，无法转换为Func<object, object, object, string>类型");
                result = connectionStringSelector3.Invoke(fieldValues[0], fieldValues[1], fieldValues[2]);
                break;
        }
        return result;
    }
    public string UseSlave()
    {
        var connectionStringSelector = this.slaveSelector as Func<string>;
        if (connectionStringSelector == null)
            throw new InvalidOperationException("从库连接串选择器未设置或参数类型不正确，无法转换为Func<string>类型");
        return connectionStringSelector.Invoke();
    }
    public string UseSlaveBy(params object[] fieldValues)
    {
        if (fieldValues == null || fieldValues.Length == 0)
            throw new ArgumentNullException(nameof(fieldValues), "fieldValues参数不能为空");
        string result = null;
        switch (fieldValues.Length)
        {
            case 1:
                var connectionStringSelector1 = this.masterSelector as Func<object, string>;
                if (connectionStringSelector1 == null) throw new InvalidOperationException("从库连接串选择器未设置或或参数类型不正确，无法转换为Func<object, string>类型");
                result = connectionStringSelector1.Invoke(fieldValues[0]);
                break;
            case 2:
                var connectionStringSelector2 = this.masterSelector as Func<object, object, string>;
                if (connectionStringSelector2 == null) throw new InvalidOperationException("从库连接串选择器未设置或或参数类型不正确，无法转换为Func<object, object, string>类型");
                result = connectionStringSelector2.Invoke(fieldValues[0], fieldValues[1]);
                break;
            case 3:
                var connectionStringSelector3 = this.masterSelector as Func<object, object, object, string>;
                if (connectionStringSelector3 == null) throw new InvalidOperationException("从库连接串选择器未设置或或参数类型不正确，无法转换为Func<object, object, object, string>类型");
                result = connectionStringSelector3.Invoke(fieldValues[0], fieldValues[1], fieldValues[2]);
                break;
        }
        return result;
    }
    public void Build()
    {
        if (this.MasterConnectionStrings == null || this.MasterConnectionStrings.Count == 0)
            throw new InvalidOperationException("没有配置可用的主库连接串");

        // 如果未设置主库连接串选择器，则默认设置为轮询方式选择器
        if (this.MasterConnectionStrings.Count > 0 && this.masterSelector == null)
        {
            if (this.MasterConnectionStrings.Count == 1)
                this.masterSelector = () => this.MasterConnectionStrings[0];
            else
            {
                this.masterSelector = () =>
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
                    index %= this.MasterConnectionStrings.Count;
                    return this.MasterConnectionStrings[index];
                };
            }
        }
        // 如果已经设置了从库选择器，则不再设置默认的轮询方式选择器
        if (this.slaveSelector != null) return;
        if (this.SlaveConnectionStrings == null || this.SlaveConnectionStrings.Count == 0)
        {
            this.slaveSelector = this.masterSelector;
            return;
        }
        if (this.SlaveConnectionStrings.Count == 1)
            this.slaveSelector = () => this.SlaveConnectionStrings[0];
        else
        {
            this.slaveSelector = () =>
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