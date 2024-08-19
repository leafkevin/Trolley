namespace Trolley;

public class TheaDatabase
{
    public string DbKey { get; internal set; }
    public string ConnectionString { get; internal set; }
    public bool IsDefault { get; internal set; }
    public OrmProviderType OrmProviderType { get; internal set; }
    public IOrmProvider OrmProvider { get; internal set; }
}
