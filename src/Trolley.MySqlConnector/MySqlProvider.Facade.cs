namespace Trolley.MySqlConnector;

partial class MySqlProvider
{

	public override ICreateVisitor NewCreateVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
		=> new MySqlCreateVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
	public override IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
		=> new MySqlUpdateVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
}
