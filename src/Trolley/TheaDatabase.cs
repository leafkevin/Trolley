using System.Collections.Generic;
using System.Threading;

namespace Trolley;

public class TheaDatabase
{
    private int roundRobin = 0;
    private readonly object locker = new object();
    public string DbKey { get; internal set; }
    public string ConnectionString { get; internal set; }
    public List<string> SlaveConnectionStrings { get; internal set; }
    public bool IsDefault { get; internal set; }
    public OrmProviderType OrmProviderType { get; internal set; }
    public IOrmProvider OrmProvider { get; internal set; }
    internal string UseSlave()
    {
        if (this.SlaveConnectionStrings == null)
            return this.ConnectionString;
        lock (this.locker)
        {
            var index = Interlocked.Increment(ref this.roundRobin);
            Interlocked.CompareExchange(ref this.roundRobin, 0, int.MaxValue);
            index %= this.SlaveConnectionStrings.Count;
            return this.SlaveConnectionStrings[index];
        }
    }
    public override int GetHashCode() => this.DbKey.GetHashCode();
}