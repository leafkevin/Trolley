using Microsoft.Extensions.DependencyInjection;
using System;
using Trolley.MySqlConnector;
using Xunit;

namespace Trolley.Test.MySql;

public class MySqlWhereUnitTest : UnitTestBase
{
    public MySqlWhereUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register("fengling", true, f =>
            {
                var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
                f.Add<MySqlProvider>(connectionString, true);
            })
            .AddTypeHandler<JsonTypeHandler>()
            .Configure<MySqlProvider, MySqlModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async void WhereBoolean()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => f.IsEnabled);
        Assert.True(result1.Count > 0);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.True(result2.Count > 0);
        Assert.True(result1.Count == result2.Count);
    }
    [Fact]
    public async void WhereMemberVisit()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => !(f.IsEnabled == false) && f.Id > 0);
        Assert.True(result1.Count > 0);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.True(result2.Count > 0);
        Assert.True(result1.Count == result2.Count);
    }
    [Fact]
    public async void WhereStringEnum()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql1 = repository.From<Company>()
          .Where(f => f.Nature == CompanyNature.Internet)
          .ToSql(out _);
        Assert.True(sql1 == "SELECT `Id`,`Name`,`Nature`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy` FROM `sys_company` WHERE `Nature`='Internet'");
        var result1 = await repository.QueryAsync<Company>(f => f.Nature == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);

        var sql2 = repository.From<Company>()
         .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
         .ToSql(out _);
        Assert.True(sql2 == "SELECT `Id`,`Name`,`Nature`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy` FROM `sys_company` WHERE COALESCE(`Nature`,'Internet')='Internet'");
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result2.Count >= 2);

        var localNature = CompanyNature.Internet;
        var sql3 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.True(sql3 == "SELECT `Id`,`Name`,`Nature`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy` FROM `sys_company` WHERE COALESCE(`Nature`,'Internet')=@p0");
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result3 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result3.Count >= 2);
    }
    [Fact]
    public async void WhereIsNull()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
           .Where(f => f.BuyerId.IsNull())
           .ToSql(out _);
        Assert.True(sql == "SELECT `Id`,`OrderNo`,`ProductCount`,`TotalAmount`,`BuyerId`,`SellerId`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy` FROM `sys_order` WHERE `BuyerId` IS NULL");
        repository.BeginTransaction();
        repository.Update<Order>(f => new { BuyerId = DBNull.Value }, new { Id = 1 });
        var result1 = repository.From<Order>()
            .Where(f => f.BuyerId.IsNull())
            .First();
        repository.Commit();

        Assert.True(result1.Id == 1);
        var sql1 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.True(sql1 == "SELECT `Id`,`Name`,`Nature`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy` FROM `sys_company` WHERE COALESCE(`Nature`,'Internet')='Internet'");
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result2.Count >= 2);

        var localNature = CompanyNature.Internet;
        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.True(sql2 == "SELECT `Id`,`Name`,`Nature`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy` FROM `sys_company` WHERE COALESCE(`Nature`,'Internet')=@p0");
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));

        var result3 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result2.Count >= 2);
    }
    [Fact]
    public void WhereAndOr()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<Order, User>()
            .Where((a, b) => a.BuyerId == b.Id)
            .And(true, (a, b) => a.SellerId.IsNull() || !a.ProductCount.HasValue)
            .And(true, (a, b) => a.Products != null)
            .And(true, (a, b) => a.Products == null || a.Disputes == null)
            .ToSql(out _);
        Assert.True(sql == "SELECT * FROM `sys_order` a,`sys_user` b WHERE a.`BuyerId`=b.`Id` AND (a.`SellerId` IS NULL OR a.`ProductCount` IS NULL) AND a.`Products` IS NOT NULL AND (a.`Products` IS NULL OR a.`Disputes` IS NULL)");
    }
}
