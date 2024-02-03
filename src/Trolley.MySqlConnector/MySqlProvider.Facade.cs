using System.Data;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new MySqlRepository(dbContext);
    public override IQueryVisitor NewQueryVisitor(IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null)
        => new MySqlQueryVisitor(this, mapProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters);
    public override ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext) => new MySqlCreate<TEntity>(dbContext);
    public override IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlContinuedCreate<TEntity>(dbContext, visitor);
    public override ICreateVisitor NewCreateVisitor(IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new MySqlCreateVisitor(this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public override IUpdateVisitor NewUpdateVisitor(IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new MySqlUpdateVisitor(this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
}
