using System;
using System.Data;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new MySqlRepository(dbContext);

    public override IIdentitiedCreated NewIdentitiedCreated(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlIdentitiedCreated(dbContext, visitor);
    public override ICreated NewCreated(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlCreated(dbContext, visitor);

    public override IUpdated NewUpdated(DbContext dbContext, IUpdateVisitor visitor)
        => new MySqlUpdated(dbContext, visitor);

    public override IQueryVisitor NewQueryVisitor(DbContext dbContext, char tableAsStart = 'a', IDataParameterCollection dbParameters = null)
        => new MySqlQueryVisitor(dbContext, tableAsStart, dbParameters);

    public override ICreateVisitor NewCreateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        => new MySqlCreateVisitor(entityType, dbContext, tableAsStart);

    public override IUpdateVisitor NewUpdateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        => new MySqlUpdateVisitor(entityType, dbContext, tableAsStart);

    public override IDeleteVisitor NewDeleteVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        => new MySqlDeleteVisitor(entityType, dbContext, tableAsStart);
}