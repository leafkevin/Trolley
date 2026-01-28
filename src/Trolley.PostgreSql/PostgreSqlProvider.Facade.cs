using System;
using System.Data;

namespace Trolley.PostgreSql;

partial class PostgreSqlProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new PostgreSqlRepository(dbContext);


    public override IIdentitiedCreated NewIdentitiedCreated(DbContext dbContext, ICreateVisitor visitor)
        => new PostgreSqlIdentitiedCreated(dbContext, visitor);
    public override ICreated NewCreated(DbContext dbContext, ICreateVisitor visitor)
        => new PostgreSqlCreated(dbContext, visitor);

    public override IUpdated NewUpdated(DbContext dbContext, IUpdateVisitor visitor)
        => new PostgreSqlUpdated(dbContext, visitor);

    public override IBulkResultCommand<TResult> NewResultUpdated<TResult>(DbContext dbContext, IUpdateVisitor visitor)
        => new PostgreSqlResultUpdated<TResult>(dbContext, visitor);

    public override IQueryVisitor NewQueryVisitor(DbContext dbContext, char tableAsStart = 'a', IDataParameterCollection dbParameters = null)
        => new PostgreSqlQueryVisitor(dbContext, tableAsStart, dbParameters);

    public override ICreateVisitor NewCreateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        => new PostgreSqlCreateVisitor(entityType, dbContext, tableAsStart);

    public override IUpdateVisitor NewUpdateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        => new PostgreSqlUpdateVisitor(entityType, dbContext, tableAsStart);

    public override IDeleteVisitor NewDeleteVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        => new PostgreSqlDeleteVisitor(entityType, dbContext, tableAsStart);
}