using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trolley.SqlServer;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.SqlServer;

public class WhereUnitTest : UnitTestBase
{
    private static int connTotal = 0;
    private static int connOpenTotal = 0;
    private readonly ITestOutputHelper output;
    public WhereUnitTest(ITestOutputHelper output)
    {
        this.output = output;
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
                .Register(OrmProviderType.SqlServer, "fengling", "Server=127.0.0.1;Database=fengling;Uid=sa;password=SQLserverSA123456;TrustServerCertificate=true", true)
                .Configure<ModelConfiguration>(OrmProviderType.SqlServer)
                .UseInterceptors(df =>
                {
                    df.OnConnectionCreated += evt =>
                    {
                        Interlocked.Increment(ref connTotal);
                        this.output.WriteLine($"{evt.ConnectionId} Created, Total:{Volatile.Read(ref connTotal)}");
                    };
                    df.OnConnectionOpened += evt =>
                    {
                        Interlocked.Increment(ref connOpenTotal);
                        this.output.WriteLine($"{evt.ConnectionId} Opened, Total:{Volatile.Read(ref connOpenTotal)}");
                    };
                    df.OnConnectionClosed += evt =>
                    {
                        Interlocked.Decrement(ref connOpenTotal);
                        this.output.WriteLine($"{evt.ConnectionId} Closed, Total:{Volatile.Read(ref connOpenTotal)}");
                    };
                    df.OnCommandExecuting += evt =>
                    {
                        this.output.WriteLine($"{evt.SqlType} Begin, Sql: {evt.Sql}");
                    };
                    df.OnCommandExecuted += evt =>
                    {
                        this.output.WriteLine($"{evt.SqlType} End, Elapsed: {evt.Elapsed} ms, Sql: {evt.Sql}");
                    };
                });
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async Task WhereBoolean()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => f.IsEnabled);
        Assert.True(result1.Count > 0);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.True(result2.Count > 0);
        Assert.True(result1.Count == result2.Count);
    }
    [Fact]
    public async Task WhereMemberVisit()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => !(f.IsEnabled == false) && f.Id > 0);
        Assert.True(result1.Count > 0);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.True(result2.Count > 0);
        Assert.True(result1.Count == result2.Count);
    }
    [Fact]
    public async Task WhereStringEnum()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Company>()
            .Where(f => f.Nature == CompanyNature.Internet)
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[Name],a.[Nature],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_company] a WHERE a.[Nature]=N'Internet'", sql1);
        var result1 = await repository.QueryAsync<Company>(f => f.Nature == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);

        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[Name],a.[Nature],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_company] a WHERE COALESCE(a.[Nature],N'Internet')=N'Internet'", sql2);
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result2.Count >= 2);

        var localNature = CompanyNature.Internet;
        var sql3 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT a.[Id],a.[Name],a.[Nature],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_company] a WHERE COALESCE(a.[Nature],N'Internet')=@p0", sql3);
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result3 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result3.Count >= 2);
    }
    [Fact]
    public async Task WhereCoalesceConditional()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[Name],a.[Nature],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_company] a WHERE COALESCE(a.[Nature],N'Internet')=N'Internet'", sql1);
        var result1 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);
        Assert.Equal(CompanyNature.Internet, (result1[0].Nature ?? CompanyNature.Internet));

        var localNature = CompanyNature.Internet;
        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT a.[Id],a.[Name],a.[Nature],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_company] a WHERE COALESCE(a.[Nature],N'Internet')=@p0", sql2);
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result2.Count >= 2);
        Assert.Equal(localNature, (result2[0].Nature ?? CompanyNature.Internet));

        var sql3 = repository.From<Company>()
            .Where(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature)
            .ToSql(out dbParameters);
        Assert.Equal("SELECT a.[Id],a.[Name],a.[Nature],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_company] a WHERE (CASE WHEN a.[IsEnabled]=1 THEN a.[Nature] ELSE N'Internet' END)=@p0", sql3);
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result3 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.True(result3.Count >= 2);
        Assert.Equal(localNature, result3[0].Nature);

        var sql4 = repository.From<User>()
            .Where(f => (f.IsEnabled ? f.SourceType : UserSourceType.Website) > UserSourceType.Website)
            .Select(f => f.Id)
            .ToSql(out dbParameters);
        Assert.Equal("SELECT a.[Id] FROM [sys_user] a WHERE (CASE WHEN a.[IsEnabled]=1 THEN a.[SourceType] ELSE N'Website' END)>N'Website'", sql4);
        var result5 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.True(result5.Count >= 2);
        Assert.Equal(localNature, result5[0].Nature);
    }
    [Fact]
    public async Task WhereIsNull()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Order>()
           .Where(f => f.BuyerId.IsNull())
           .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_order] a WHERE a.[BuyerId] IS NULL", sql1);
        repository.BeginTransaction();
        repository.Update<Order>(f => new { BuyerId = DBNull.Value }, f => f.Id == "1");
        var result1 = repository.Get<Order>("1");
        repository.Commit();
        Assert.Equal(0, result1.BuyerId);
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result2.Count >= 2);
        var localNature = CompanyNature.Internet;
        var result3 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result2.Count >= 2);
    }
    [Fact]
    public void WhereAndOr()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order, User>()
            .Where((a, b) => a.BuyerId == b.Id)
            .And(true, (a, b) => a.SellerId.IsNull() || !a.ProductCount.HasValue)
            .And(true, (a, b) => a.Products != null)
            .And(true, (a, b) => a.Products == null || a.Disputes == null)
            .Select((a, b) => "*")
            .ToSql(out _);
        Assert.Equal("SELECT * FROM [sys_order] a,[sys_user] b WHERE a.[BuyerId]=b.[Id] AND (a.[SellerId] IS NULL OR a.[ProductCount] IS NULL) AND a.[Products] IS NOT NULL AND (a.[Products] IS NULL OR a.[Disputes] IS NULL)", sql);

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
        Assert.Equal("SELECT * FROM [sys_order] a,[sys_user] b WHERE (a.[BuyerId]=b.[Id] OR b.[SourceType]=N'Douyin') AND (a.[BuyerSource]=N'Taobao' OR (a.[SellerId] IS NULL AND a.[ProductCount] IS NULL) OR a.[ProductCount]>1 OR (a.[TotalAmount]>500 AND a.[BuyerSource]=N'Website')) AND ((a.[BuyerId]<=10 AND a.[ProductCount]>5 AND b.[SourceType]=N'Douyin') OR (a.[BuyerId]>10 AND a.[ProductCount]<=5 AND b.[SourceType]=N'Website') OR a.[BuyerSource]=N'Taobao') AND (a.[Products] IS NULL OR a.[Disputes] IS NULL)", sql);
    }
    [Fact]
    public void Where()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id] FROM [sys_order] a WHERE EXISTS(SELECT * FROM [sys_user] t WHERE t.[Id]=a.[BuyerId] AND t.[IsEnabled]=1) AND (a.[BuyerId] IS NULL OR a.[BuyerId]=2) AND (a.[OrderNo] LIKE N'%ON_%' OR (a.[OrderNo] IS NULL OR a.[OrderNo]=''))", sql1);
        var result1 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo)))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result1);
        Assert.True(result1.Count > 0);

        var sql2 = repository.From<Order>()
            .Where(f => (f.BuyerId.IsNull() || f.BuyerId == 2) && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo))
                && (Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) || f.SellerId.IsNull()))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id] FROM [sys_order] a WHERE (a.[BuyerId] IS NULL OR a.[BuyerId]=2) AND (a.[OrderNo] LIKE N'%ON_%' OR (a.[OrderNo] IS NULL OR a.[OrderNo]='')) AND (EXISTS(SELECT * FROM [sys_user] t WHERE t.[Id]=a.[BuyerId] AND t.[IsEnabled]=1) OR a.[SellerId] IS NULL)", sql2);
        var result2 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo)))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result2);
        Assert.True(result2.Count > 0);

        var sql3 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") && string.IsNullOrEmpty(f.OrderNo)) || DateTime.IsLeapYear(f.CreatedAt.Year))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id] FROM [sys_order] a WHERE EXISTS(SELECT * FROM [sys_user] t WHERE t.[Id]=a.[BuyerId] AND t.[IsEnabled]=1) AND (a.[BuyerId] IS NULL OR a.[BuyerId]=2) AND a.[OrderNo] LIKE N'%ON_%' AND (a.[OrderNo] IS NULL OR a.[OrderNo]='') OR (DATEPART(YEAR,a.[CreatedAt])%4=0 AND DATEPART(YEAR,a.[CreatedAt])%100<>0 OR DATEPART(YEAR,a.[CreatedAt])%400=0)", sql3);
        var result3 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") && string.IsNullOrEmpty(f.OrderNo)) || DateTime.IsLeapYear(f.CreatedAt.Year))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result3);
        Assert.True(result3.Count > 0);
    }
}
