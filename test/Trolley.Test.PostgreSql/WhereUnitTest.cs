using Microsoft.Extensions.DependencyInjection;
using System;
using Trolley.PostgreSql;
using Xunit;

namespace Trolley.Test.PostgreSql;

public class WhereUnitTest : UnitTestBase
{
    public WhereUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register<PostgreSqlProvider>("fengling", "Host=localhost;Database=fengling;Username=postgres;Password=123456;SearchPath=public", true, "public")
            .Configure<PostgreSqlProvider, ModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
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
        Assert.True(sql1 == "SELECT a.\"Id\",a.\"Name\",a.\"Nature\",a.\"IsEnabled\",a.\"CreatedAt\",a.\"CreatedBy\",a.\"UpdatedAt\",a.\"UpdatedBy\" FROM \"sys_company\" a WHERE a.\"Nature\"='Internet'");
        var result1 = await repository.QueryAsync<Company>(f => f.Nature == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);

        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.True(sql2 == "SELECT a.\"Id\",a.\"Name\",a.\"Nature\",a.\"IsEnabled\",a.\"CreatedAt\",a.\"CreatedBy\",a.\"UpdatedAt\",a.\"UpdatedBy\" FROM \"sys_company\" a WHERE COALESCE(a.\"Nature\",'Internet')='Internet'");
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result2.Count >= 2);

        var localNature = CompanyNature.Internet;
        var sql3 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.True(sql3 == "SELECT a.\"Id\",a.\"Name\",a.\"Nature\",a.\"IsEnabled\",a.\"CreatedAt\",a.\"CreatedBy\",a.\"UpdatedAt\",a.\"UpdatedBy\" FROM \"sys_company\" a WHERE COALESCE(a.\"Nature\",'Internet')=@p0");
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result3 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result3.Count >= 2);
    }
    [Fact]
    public async void WhereCoalesceConditional()
    {
        using var repository = dbFactory.Create();
        var sql1 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.True(sql1 == "SELECT a.\"Id\",a.\"Name\",a.\"Nature\",a.\"IsEnabled\",a.\"CreatedAt\",a.\"CreatedBy\",a.\"UpdatedAt\",a.\"UpdatedBy\" FROM \"sys_company\" a WHERE COALESCE(a.\"Nature\",'Internet')='Internet'");
        var result1 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);
        Assert.True((result1[0].Nature ?? CompanyNature.Internet) == CompanyNature.Internet);

        var localNature = CompanyNature.Internet;
        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.True(sql2 == "SELECT a.\"Id\",a.\"Name\",a.\"Nature\",a.\"IsEnabled\",a.\"CreatedAt\",a.\"CreatedBy\",a.\"UpdatedAt\",a.\"UpdatedBy\" FROM \"sys_company\" a WHERE COALESCE(a.\"Nature\",'Internet')=@p0");
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result2.Count >= 2);
        Assert.True((result1[0].Nature ?? CompanyNature.Internet) == localNature);

        var sql3 = repository.From<Company>()
            .Where(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature)
            .ToSql(out dbParameters);
        Assert.True(sql3 == "SELECT a.\"Id\",a.\"Name\",a.\"Nature\",a.\"IsEnabled\",a.\"CreatedAt\",a.\"CreatedBy\",a.\"UpdatedAt\",a.\"UpdatedBy\" FROM \"sys_company\" a WHERE (CASE WHEN a.\"IsEnabled\"=TRUE THEN a.\"Nature\" ELSE 'Internet' END)=@p0");
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result3 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.True(result3.Count >= 2);
        Assert.True(result3[0].Nature == localNature);

        var sql4 = repository.From<User>()
            .Where(f => (f.IsEnabled ? f.SourceType : UserSourceType.Website) > UserSourceType.Website)
            .Select(f => f.Id)
            .ToSql(out dbParameters);
        Assert.True(sql4 == "SELECT a.\"Id\" FROM \"sys_user\" a WHERE (CASE WHEN a.\"IsEnabled\"=TRUE THEN a.\"SourceType\" ELSE 'Website' END)>'Website'");
        var result5 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.True(result5.Count >= 2);
        Assert.True(result5[0].Nature == localNature);
    }
    [Fact]
    public async void WhereIsNull()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql1 = repository.From<Order>()
           .Where(f => f.BuyerId.IsNull())
           .ToSql(out _);
        Assert.True(sql1 == "SELECT a.\"Id\",a.\"TenantId\",a.\"OrderNo\",a.\"ProductCount\",a.\"TotalAmount\",a.\"BuyerId\",a.\"BuyerSource\",a.\"SellerId\",a.\"Products\",a.\"Disputes\",a.\"IsEnabled\",a.\"CreatedAt\",a.\"CreatedBy\",a.\"UpdatedAt\",a.\"UpdatedBy\" FROM \"sys_order\" a WHERE a.\"BuyerId\" IS NULL");
        repository.BeginTransaction();
        repository.Update<Order>(f => new { BuyerId = DBNull.Value }, f => f.Id == "1");
        var result1 = repository.Get<Order>("1");
        repository.Commit();
        Assert.True(result1.BuyerId == 0);
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result2.Count >= 2);
        var localNature = CompanyNature.Internet;
        var result3 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result2.Count >= 2);
    }
    [Fact]
    public void WhereAndOr()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<Order, User>()
            .Where((a, b) => a.BuyerId == b.Id)
            .And(true, (a, b) => a.SellerId.IsNull() || !a.ProductCount.HasValue)
            .And(true, (a, b) => a.Products != null)
            .And(true, (a, b) => a.Products == null || a.Disputes == null)
            .Select((a, b) => "*")
            .ToSql(out _);
        Assert.True(sql == "SELECT * FROM \"sys_order\" a,\"sys_user\" b WHERE a.\"BuyerId\"=b.\"Id\" AND (a.\"SellerId\" IS NULL OR a.\"ProductCount\" IS NULL) AND a.\"Products\" IS NOT NULL AND (a.\"Products\" IS NULL OR a.\"Disputes\" IS NULL)");

        var filterExpr = PredicateBuilder.Create<Order, User>()
            .And((x, y) => x.BuyerId <= 10 && x.ProductCount > 5 && y.SourceType == UserSourceType.Douyin)
            .Or((x, y) => x.BuyerId > 10 && x.ProductCount <= 5 && y.SourceType == UserSourceType.Website)
            .Or((x, y) => x.BuyerSource == UserSourceType.Taobao)
            .Build();
        sql = repository.From<Order, User>()
            .Where((a, b) => a.BuyerId == b.Id || b.SourceType == UserSourceType.Douyin)
            .And(true, (a, b) => (a.BuyerSource == UserSourceType.Taobao || a.SellerId.IsNull() && !a.ProductCount.HasValue) || a.ProductCount > 1 || a.TotalAmount > 500 && a.BuyerSource == UserSourceType.Website)
            .And(true, filterExpr)
            .And(true, (a, b) => a.Products == null || a.Disputes == null)
            .Select((a, b) => "*")
        .ToSql(out _);
        Assert.True(sql == "SELECT * FROM \"sys_order\" a,\"sys_user\" b WHERE (a.\"BuyerId\"=b.\"Id\" OR b.\"SourceType\"='Douyin') AND (a.\"BuyerSource\"='Taobao' OR (a.\"SellerId\" IS NULL AND a.\"ProductCount\" IS NULL) OR a.\"ProductCount\">1 OR (a.\"TotalAmount\">500 AND a.\"BuyerSource\"='Website')) AND ((a.\"BuyerId\"<=10 AND a.\"ProductCount\">5 AND b.\"SourceType\"='Douyin') OR (a.\"BuyerId\">10 AND a.\"ProductCount\"<=5 AND b.\"SourceType\"='Website') OR a.\"BuyerSource\"='Taobao') AND (a.\"Products\" IS NULL OR a.\"Disputes\" IS NULL)");
    }
    [Fact]
    public void Where()
    {
        using var repository = dbFactory.Create();
        var sql1 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.True(sql1 == "SELECT a.\"Id\" FROM \"sys_order\" a WHERE EXISTS(SELECT * FROM \"sys_user\" t WHERE t.\"Id\"=a.\"BuyerId\" AND t.\"IsEnabled\"=TRUE) AND (a.\"BuyerId\" IS NULL OR a.\"BuyerId\"=2) AND (a.\"OrderNo\" LIKE '%ON_%' OR (a.\"OrderNo\" IS NULL OR a.\"OrderNo\"=''))");

        var sql2 = repository.From<Order>()
          .Where(f => (f.BuyerId.IsNull() || f.BuyerId == 2) && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo))
              && (Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) || f.SellerId.IsNull()))
          .Select(f => f.Id)
          .ToSql(out _);
        Assert.True(sql2 == "SELECT a.\"Id\" FROM \"sys_order\" a WHERE (a.\"BuyerId\" IS NULL OR a.\"BuyerId\"=2) AND (a.\"OrderNo\" LIKE '%ON_%' OR (a.\"OrderNo\" IS NULL OR a.\"OrderNo\"='')) AND (EXISTS(SELECT * FROM \"sys_user\" t WHERE t.\"Id\"=a.\"BuyerId\" AND t.\"IsEnabled\"=TRUE) OR a.\"SellerId\" IS NULL)");
    }
}
