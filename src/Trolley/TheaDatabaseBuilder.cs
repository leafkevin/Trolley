using System;

namespace Trolley;

public class TheaDatabaseBuilder
{
    private readonly TheaDatabaseProvider database;

    public TheaDatabaseBuilder(TheaDatabaseProvider database) => this.database = database;
    public TenantDatabaseBuilder Add(TheaDatabase connectionInfo)
    {
        if (connectionInfo == null)
            throw new ArgumentNullException(nameof(connectionInfo));

        this.database.AddDatabase(connectionInfo);
        return new TenantDatabaseBuilder(this.database, connectionInfo);
    }
}
