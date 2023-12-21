namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext)
        => new MySqlCreate<TEntity>(dbContext);
    public override IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlContinuedCreate<TEntity>(dbContext, visitor);
    public override ICreateVisitor NewCreateVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new MySqlCreateVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public override IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new MySqlUpdateVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
}
