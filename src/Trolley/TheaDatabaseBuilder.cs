namespace Trolley;

public class TheaDatabaseBuilder
{
    private readonly TheaDatabase database;

    public TheaDatabaseBuilder(TheaDatabase database) => this.database = database;
    public TheaDatabaseBuilder Add(string connectionString, bool isDefault, params string[] tenantIds)
    {
        this.database.AddTenantDatabase(new TenantDatabase
        {
            ConnectionString = connectionString,
            IsDefault = isDefault,
            TenantIds = tenantIds
        }); 
        return this;
    } 
}
