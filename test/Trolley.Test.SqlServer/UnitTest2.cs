using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trolley.SqlServer;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.SqlServer;

public class UnitTest2 : UnitTestBase
{
    private static int connTotal = 0;
    private static int connOpenTotal = 0;
    private readonly ITestOutputHelper output;
    public UnitTest2(ITestOutputHelper output)
    {
        this.output = output;
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
                .Register(OrmProviderType.SqlServer, "fengling", "Server=172.16.30.190;Database=fengling;Uid=sa;password=SQLserverSA123456;TrustServerCertificate=true", true)
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
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async Task QueryFirst()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.QueryFirst<User>(f => f.Id == 1);
        if (result != null)
        {
            Assert.NotNull(result.Name);
        }
        result = repository.QueryFirst<User>("SELECT * FROM sys_user where Id=1");
        if (result != null)
        {
            Assert.NotNull(result.Name);
        }
        var result1 = await repository.QueryFirstAsync<User>(f => f.Name == "leafkevin");
        var result2 = await repository.QueryFirstAsync<User>(new { Name = "leafkevin" });
        if (result1 != null && result2 != null)
        {
            Assert.True(result1.Id == result2.Id);
            Assert.Equal(1, result1.Id);
        }
    }
    [Fact]
    public async Task Get()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.Get<User>(1);
        Assert.Equal("leafkevin", result.Name);
        var user = await repository.GetAsync<User>(new { Id = 1 });
        Assert.True(user.Name == result.Name);
    }
    [Fact]
    public async Task Query()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = await repository.QueryAsync<Product>(f => f.ProductNo.Contains("PN-00"));
        Assert.True(result.Count >= 3);
    }
    [Fact]
    public async Task QueryPage()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.From<OrderDetail>()
            .Where(f => f.ProductId == 1)
            .OrderByDescending(f => f.CreatedAt)
            .Page(2, 1)
            .ToPageList();
        var count = await repository.From<OrderDetail>().Where(f => f.ProductId == 1).CountAsync();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        Assert.True(result.TotalCount == count);
        Assert.True(result.Data.Count == result.Count);
        Assert.Equal(1, result.Count);
    }
    [Fact]
    public async Task QueryDictionary()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = await repository.QueryDictionaryAsync<Product, int, string>(f => f.ProductNo.Contains("PN-00"), f => f.Id, f => f.Name);
        Assert.True(result.Count >= 3);
    }
    class OrderBuyerInfo
    {
        public string OrderId { get; set; }
        public string OrderNo { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int ProductTotal { get; set; }
    }
    [Fact]
    public async Task QueryRawSql()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var result = await repository.QueryAsync<Product>("SELECT * FROM sys_product where Id=@ProductId", new { ProductId = 1 });
        Assert.NotNull(result);
        Assert.Single(result);
    }
    [Fact]
    public void FromQuery_SubQuery()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var sql = repository
            .From(f => f.From<OrderDetail>()
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                Buyer = y,
                x.ProductCount
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.[OrderId],a.[BuyerId],b.[Id],b.[TenantId],b.[Name],b.[Gender],b.[Age],b.[CompanyId],b.[GuidField],b.[SomeTimes],b.[SourceType],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy],a.[ProductCount] FROM (SELECT b.[Id] AS [OrderId],b.[BuyerId],COUNT(DISTINCT a.[ProductId]) AS [ProductCount] FROM [sys_order_detail] a INNER JOIN [sys_order] b ON a.[OrderId]=b.[Id] GROUP BY b.[Id],b.[BuyerId]) a INNER JOIN [sys_user] b ON a.[BuyerId]=b.[Id] WHERE a.[ProductCount]>1", sql);

        var result = repository
            .From(f => f.From<OrderDetail>()
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                Buyer = y,
                x.ProductCount
            })
            .ToList();
        if (result.Count > 0)
        {
            Assert.NotNull(result[0]);
            Assert.NotNull(result[0].Group);
            Assert.NotNull(result[0].Buyer);
            Assert.True(result[0].ProductCount > 1);
        }
        var sql1 = repository
           .From(f => f.From<Order>()
               .Select(x => new { x.Id, x.OrderNo, x.BuyerId, x.SellerId }))
           .Select(x => new { Order = x })
           .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[OrderNo],a.[BuyerId],a.[SellerId] FROM (SELECT a.[Id],a.[OrderNo],a.[BuyerId],a.[SellerId] FROM [sys_order] a) a", sql1);

        var result1 = repository
            .From(f => f.From<Order>()
                .Select(x => new { x.Id, x.OrderNo, x.BuyerId, x.SellerId }))
            .Select(x => new { Order = x })
            .First();
        Assert.NotNull(result1);
        Assert.NotNull(result1.Order);
    }
    [Fact]
    public void FromQuery_SubQuery1()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From(f => f.From<Page, Menu>('o')
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { MenuId = y.Id, y.ParentId, x.Url }))
            .InnerJoin<Menu>((a, b) => a.MenuId == b.Id)
            .Where((a, b) => a.MenuId == b.Id)
            .Select((a, b) => new { a.MenuId, b.Name, a.ParentId, a.Url })
            .ToSql(out _);
        Assert.Equal("SELECT a.[MenuId],b.[Name],a.[ParentId],a.[Url] FROM (SELECT p.[Id] AS [MenuId],p.[ParentId],o.[Url] FROM [sys_page] o,[sys_menu] p WHERE o.[Id]=p.[PageId]) a INNER JOIN [sys_menu] b ON a.[MenuId]=b.[Id] WHERE a.[MenuId]=b.[Id]", sql);

        var result = repository.From(f => f.From<Page, Menu>('o')
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { MenuId = y.Id, y.ParentId, x.Url }))
            .InnerJoin<Menu>((a, b) => a.MenuId == b.Id)
            .Where((a, b) => a.MenuId == b.Id)
            .Select((a, b) => new { a.MenuId, b.Name, a.ParentId, a.Url })
            .ToList();

        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_SubQuery2()
    {
        using var repository = dbFactory.Create();
        var count = 1;
        var sql = repository
            .From(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => Sql.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = Sql.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .Include((a, b) => b.Details)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT b.[Disputes],a.[BuyerId],a.[OrderId],a.[OrderNo],a.[ProductTotal],b.[Id],b.[TenantId],b.[OrderNo],b.[ProductCount],b.[TotalAmount],b.[BuyerId],b.[BuyerSource],b.[SellerId],b.[Products],b.[Disputes],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy] FROM (SELECT b.[BuyerId],b.[Id] AS [OrderId],b.[OrderNo],COUNT(DISTINCT c.[ProductId]) AS [ProductTotal] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] LEFT JOIN [sys_order_detail] c ON b.[Id]=c.[OrderId] GROUP BY b.[BuyerId],b.[Id],b.[OrderNo] HAVING COUNT(DISTINCT c.[ProductId])>@p0) a INNER JOIN [sys_order] b ON a.[OrderId]=b.[Id]", sql);
        Assert.True((int)dbParameters[0].Value == count);

        var result = repository
            .From(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => Sql.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = Sql.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .Include((a, b) => b.Details)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .First();
        if (result != null)
        {
            Assert.NotNull(result.Disputes);
            Assert.NotNull(result.Order);
            Assert.NotNull(result.Order.Details);
            Assert.True(result.Order.Details.Count > 0);
            Assert.True(result.Order.Details[0].Amount > 0);
        }

        var amount = 100;
        sql = repository
            .From(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => Sql.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = Sql.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .IncludeMany((a, b) => b.Details, f => f.Amount > amount)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .ToSql(out dbParameters);
        Assert.Equal("SELECT b.[Disputes],a.[BuyerId],a.[OrderId],a.[OrderNo],a.[ProductTotal],b.[Id],b.[TenantId],b.[OrderNo],b.[ProductCount],b.[TotalAmount],b.[BuyerId],b.[BuyerSource],b.[SellerId],b.[Products],b.[Disputes],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy] FROM (SELECT b.[BuyerId],b.[Id] AS [OrderId],b.[OrderNo],COUNT(DISTINCT c.[ProductId]) AS [ProductTotal] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] LEFT JOIN [sys_order_detail] c ON b.[Id]=c.[OrderId] GROUP BY b.[BuyerId],b.[Id],b.[OrderNo] HAVING COUNT(DISTINCT c.[ProductId])>@p0) a INNER JOIN [sys_order] b ON a.[OrderId]=b.[Id]", sql);
        Assert.Single(dbParameters);
        Assert.True((int)dbParameters[0].Value == count);

        result = repository
            .From(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => Sql.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = Sql.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .IncludeMany((a, b) => b.Details, f => f.Amount > amount)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .First();
        if (result != null)
        {
            Assert.NotNull(result.Disputes);
            Assert.NotNull(result.Order);
            Assert.NotNull(result.Order.Details);
            Assert.True(result.Order.Details.Count > 0);
            foreach (var orderDetail in result.Order.Details)
            {
                Assert.True(result.Order.Details[0].Amount > amount);
            }
        }
    }
    [Fact]
    public void FromQuery_SubQuery3()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From(f => f.From<Order, OrderDetail>('a')
                .Where((a, b) => a.Id == b.OrderId)
                .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                .Select((x, a, b) => new { x.Grouping, ProductTotal = Sql.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
            .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
            .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
            .ToSql(out _);
        Assert.Equal("SELECT a.[BuyerId],a.[OrderId],a.[BuyerId],a.[ProductTotal],b.[Name] AS [BuyerName],a.[BuyerId1] AS [BuyerId2] FROM (SELECT a.[BuyerId],a.[Id] AS [OrderId],COUNT(DISTINCT b.[ProductId]) AS [ProductTotal],a.[BuyerId] AS [BuyerId1] FROM [sys_order] a,[sys_order_detail] b WHERE a.[Id]=b.[OrderId] GROUP BY a.[BuyerId],a.[Id] HAVING COUNT(DISTINCT b.[ProductId])>0) a INNER JOIN [sys_user] b ON a.[BuyerId]=b.[Id]", sql);

        var result = repository.From(f => f.From<Order, OrderDetail>('a')
                .Where((a, b) => a.Id == b.OrderId)
                .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                .Select((x, a, b) => new { x.Grouping, ProductTotal = Sql.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
            .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
            .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
            .First();
        if (result != null)
        {
            Assert.NotNull(result);
            Assert.NotNull(result.Grouping);
            Assert.NotNull(result.BuyerName);
        }
    }
    [Fact]
    public void FromQuery_SubQuery4()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User, Order, OrderDetail>()
            .InnerJoin((a, b, c) => a.Id == b.BuyerId)
            .LeftJoin((a, b, c) => b.Id == c.OrderId)
            .Select((a, b, c) => new { OrderId = b.Id, b.OrderNo, b.Disputes, b.BuyerId, Buyer = a })
            .ToSql(out _);
        Assert.Equal("SELECT b.[Id] AS [OrderId],b.[OrderNo],b.[Disputes],b.[BuyerId],a.[Id],a.[TenantId],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[GuidField],a.[SomeTimes],a.[SourceType],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] LEFT JOIN [sys_order_detail] c ON b.[Id]=c.[OrderId]", sql);

        var result = repository.From<User, Order, OrderDetail>()
                 .InnerJoin((a, b, c) => a.Id == b.BuyerId)
                 .LeftJoin((a, b, c) => b.Id == c.OrderId)
            .Select((a, b, c) => new { OrderId = b.Id, b.OrderNo, b.Disputes, b.BuyerId, Buyer = a })
            .First();
        if (result != null)
        {
            Assert.NotNull(result);
            Assert.NotNull(result.OrderId);
            Assert.True(result.BuyerId > 0);
            Assert.NotNull(result.OrderNo);
            Assert.NotNull(result.Buyer);
        }
    }
    [Fact]
    public async Task WithTable_SubQuery()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<Menu>()
             .WithTable(f => f.From<Page, Menu>('c')
                 .Where((a, b) => a.Id == b.PageId)
                 .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
             .Where((a, b) => a.Id == b.Id)
             .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
             .ToSql(out _);
        Assert.Equal(@"SELECT a.[Id],a.[Name],a.[ParentId],b.[Url] FROM [sys_menu] a,(SELECT d.[Id],d.[ParentId],c.[Url] FROM [sys_page] c,[sys_menu] d WHERE c.[Id]=d.[PageId]) b WHERE a.[Id]=b.[Id]", sql);
        var result = repository.From<Menu>()
             .WithTable(f => f.From<Page, Menu>('c')
                 .Where((a, b) => a.Id == b.PageId)
                 .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
             .Where((a, b) => a.Id == b.Id)
             .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
             .First();
        Assert.NotNull(result);

        var sql1 = repository.From<User>()
            .WithTable(f => f.From<Order>()
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .GroupBy((a, b) => new { OrderId = a.Id, a.BuyerId })
                .Select((x, a, b) => new { x.Grouping, ProductCount = x.CountDistinct(b.ProductId) }))
            .InnerJoin((x, y) => x.Id == y.Grouping.BuyerId)
            .Where((a, b) => b.ProductCount > 1)
            .Select((x, y) => new
            {
                y.Grouping,
                Buyer = x,
                y.ProductCount
            })
            .ToSql(out _);
        Assert.Equal("SELECT b.[OrderId],b.[BuyerId],a.[Id],a.[TenantId],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[GuidField],a.[SomeTimes],a.[SourceType],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],b.[ProductCount] FROM [sys_user] a INNER JOIN (SELECT a.[Id] AS [OrderId],a.[BuyerId],COUNT(DISTINCT b.[ProductId]) AS [ProductCount] FROM [sys_order] a INNER JOIN [sys_order_detail] b ON a.[Id]=b.[OrderId] GROUP BY a.[Id],a.[BuyerId]) b ON a.[Id]=b.[BuyerId] WHERE b.[ProductCount]>1", sql1);

        var result1 = repository.From<User>()
            .WithTable(f => f.From<Order>()
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .GroupBy((a, b) => new { OrderId = a.Id, a.BuyerId })
                .Select((x, a, b) => new { x.Grouping, ProductCount = x.CountDistinct(b.ProductId) }))
            .InnerJoin((x, y) => x.Id == y.Grouping.BuyerId)
            .Where((a, b) => b.ProductCount > 1)
            .Select((x, y) => new
            {
                y.Grouping,
                Buyer = x,
                y.ProductCount
            })
            .ToList();
        Assert.True(result1.Count > 0);

        var sql2 = repository
             .From<Order, User>()
             .WithTable(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, TotalAmount = x.Sum(b.Amount) }))
            .Where((a, b, c) => a.BuyerId == b.Id && a.Id == c.OrderId)
            .Select((a, b, c) => new { Order = a, Buyer = b, OrderId = a.Id, a.BuyerId, c.TotalAmount })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],b.[Id],b.[TenantId],b.[Name],b.[Gender],b.[Age],b.[CompanyId],b.[GuidField],b.[SomeTimes],b.[SourceType],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy],a.[Id] AS [OrderId],a.[BuyerId],c.[TotalAmount] FROM [sys_order] a,[sys_user] b,(SELECT a.[Id] AS [OrderId],SUM(b.[Amount]) AS [TotalAmount] FROM [sys_order] a,[sys_order_detail] b,[sys_user] c WHERE a.[Id]=b.[OrderId] AND a.[BuyerId]=c.[Id] AND c.[Age]>20 GROUP BY a.[Id],a.[BuyerId] HAVING SUM(b.[Amount])>500) c WHERE a.[BuyerId]=b.[Id] AND a.[Id]=c.[OrderId]", sql2);

        var result2 = await repository
             .From<Order, User>()
             .WithTable(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, TotalAmount = x.Sum(b.Amount) }))
            .Where((a, b, c) => a.BuyerId == b.Id && a.Id == c.OrderId)
            .Select((a, b, c) => new { Order = a, Buyer = b, OrderId = a.Id, a.BuyerId, c.TotalAmount })
            .ToListAsync();
        Assert.True(result2.Count > 0);
    }
    [Fact]
    public void FromQuery_InnerJoin()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
           .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
           .Where((a, b) => b.ProductCount > 1)
           .Select((x, y) => new
           {
               User = x,
               Order = y
           })
           .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[GuidField],a.[SomeTimes],a.[SourceType],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],b.[Id],b.[TenantId],b.[OrderNo],b.[ProductCount],b.[TotalAmount],b.[BuyerId],b.[BuyerSource],b.[SellerId],b.[Products],b.[Disputes],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] WHERE b.[ProductCount]>1", sql);

        var result = repository.From<User>()
            .Include(x => x.Orders)
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => b.ProductCount > 1)
            .Select((x, y) => new
            {
                User = x,
                Order = y
            })
            .ToList();
        if (result.Count > 0)
        {
            Assert.NotNull(result[0]);
            Assert.NotNull(result[0].User);
            Assert.NotNull(result[0].Order);
            Assert.True(result[0].Order.ProductCount > 1);
        }
    }
    [Fact]
    public async Task FromQuery_InnerJoin1()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
          .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
          .InnerJoin(f => f.From<OrderDetail>()
              .GroupBy(x => x.OrderId)
              .Select((x, y) => new
              {
                  y.OrderId,
                  ProductCount = x.CountDistinct(y.ProductId)
              }), (a, b, c) => b.Id == c.OrderId)
          .Where((a, b, c) => c.ProductCount > 2)
          .Select((a, b, c) => new
          {
              User = a,
              Order = b,
              c.ProductCount
          })
          .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[GuidField],a.[SomeTimes],a.[SourceType],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],b.[Id],b.[TenantId],b.[OrderNo],b.[ProductCount],b.[TotalAmount],b.[BuyerId],b.[BuyerSource],b.[SellerId],b.[Products],b.[Disputes],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy],c.[ProductCount] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] INNER JOIN (SELECT a.[OrderId],COUNT(DISTINCT a.[ProductId]) AS [ProductCount] FROM [sys_order_detail] a GROUP BY a.[OrderId]) c ON b.[Id]=c.[OrderId] WHERE c.[ProductCount]>2", sql);

        var result = await repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .InnerJoin(f => f.From<OrderDetail>()
                .GroupBy(x => x.OrderId)
                .Select((x, y) => new
                {
                    y.OrderId,
                    ProductCount = x.CountDistinct(y.ProductId)
                }), (a, b, c) => b.Id == c.OrderId)
            .Where((a, b, c) => c.ProductCount > 2)
            .Select((a, b, c) => new
            {
                User = a,
                Order = b,
                c.ProductCount
            })
            .ToListAsync();
        if (result.Count > 0)
        {
            Assert.NotNull(result[0]);
            Assert.NotNull(result[0].User);
            Assert.NotNull(result[0].Order);
            Assert.True(result[0].ProductCount > 2);
        }
    }
    [Fact]
    public void Join_Cte()
    {
        this.Initialize();
        var menuId = 1;
        var pageId = 1;
        using var repository = dbFactory.Create();
        var menuPageList = repository.From<Page, Menu>()
            .Where((a, b) => a.Id == b.PageId && b.Id > menuId.ToParameter("@MenuId"))
            .Select((x, y) => new { MenuId = y.Id, y.ParentId, x.Url })
            .AsCteTable("menuPageList");
        var sql = repository.From<Menu>()
            .InnerJoin(menuPageList, (a, b) => a.Id == b.MenuId && a.PageId > pageId)
            .Select((a, b) => new { b.MenuId, a.Name, b.ParentId, a.PageId, b.Url })
            .ToSql(out var dbParameters);
        Assert.Equal(@"WITH [menuPageList]([MenuId],[ParentId],[Url]) AS 
(
SELECT b.[Id],b.[ParentId],a.[Url] FROM [sys_page] a,[sys_menu] b WHERE a.[Id]=b.[PageId] AND b.[Id]>@MenuId
)
SELECT b.[MenuId],a.[Name],b.[ParentId],a.[PageId],b.[Url] FROM [sys_menu] a INNER JOIN [menuPageList] b ON a.[Id]=b.[MenuId] AND a.[PageId]>@p1", sql);
        Assert.Equal(2, dbParameters.Count);
        Assert.Equal("@MenuId", dbParameters[0].ParameterName);
        Assert.True((int)dbParameters[0].Value == menuId);
        Assert.True((int)dbParameters[1].Value == pageId);

        var result = repository.From<Menu>()
            .InnerJoin(menuPageList, (a, b) => a.Id == b.MenuId && a.PageId > pageId)
            .Select((a, b) => new { b.MenuId, a.Name, b.ParentId, a.PageId, b.Url })
            .ToList();
        Assert.True(result.Count > 0);
        foreach (var item in result)
        {
            Assert.True(item.MenuId > menuId);
            Assert.True(item.PageId > pageId);
        }
        int parentId = 10;
        sql = repository.From<Menu>()
            .InnerJoin<Page>((a, b) => a.PageId == b.Id && b.Id > pageId)
            .Select((a, b) => new { MenuId = a.Id, a.ParentId, b.Url })
            .Union(f => f.From(menuPageList)
                .Where(f => f.ParentId < parentId)
                .Select())
            .ToSql(out dbParameters);
        Assert.Equal(@"WITH [menuPageList]([MenuId],[ParentId],[Url]) AS 
(
SELECT b.[Id],b.[ParentId],a.[Url] FROM [sys_page] a,[sys_menu] b WHERE a.[Id]=b.[PageId] AND b.[Id]>@MenuId
)
SELECT a.[Id] AS [MenuId],a.[ParentId],b.[Url] FROM [sys_menu] a INNER JOIN [sys_page] b ON a.[PageId]=b.[Id] AND b.[Id]>@p0 UNION
SELECT a.[MenuId],a.[ParentId],a.[Url] FROM [menuPageList] a WHERE a.[ParentId]<@p2", sql);
        Assert.Equal(3, dbParameters.Count);
        Assert.Equal("@p0", dbParameters[0].ParameterName);
        Assert.Equal("@MenuId", dbParameters[1].ParameterName);
        Assert.Equal("@p2", dbParameters[2].ParameterName);
        Assert.True((int)dbParameters[0].Value == menuId);
        Assert.True((int)dbParameters[1].Value == pageId);
        Assert.True((int)dbParameters[2].Value == parentId);

        var result1 = repository.From<Menu>()
            .InnerJoin<Page>((a, b) => a.PageId == b.Id && b.Id > pageId)
            .Select((a, b) => new { MenuId = a.Id, a.ParentId, b.Url })
            .Union(f => f.From(menuPageList)
                .Where(f => f.ParentId < parentId)
                .Select())
            .ToList();
        Assert.True(result1.Count > 0);
        foreach (var item in result1)
        {
            Assert.True(item.MenuId > menuId);
            Assert.True(item.ParentId < parentId);
        }
    }
    [Fact]
    public async Task FromQuery_Include()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<Product>()
            .Include(f => f.Brand)
            .Where(f => f.ProductNo.Contains("PN-00"))
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[ProductNo],a.[Name],a.[BrandId],a.[CategoryId],a.[Price],a.[CompanyId],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],b.[Id],b.[BrandNo],b.[Name] FROM [sys_product] a LEFT JOIN [sys_brand] b ON a.[BrandId]=b.[Id] WHERE a.[ProductNo] LIKE N'%PN-00%'", sql);

        var result = await repository.From<Product>()
            .Include(f => f.Brand)
            .Where(f => f.ProductNo.Contains("PN-00"))
            .OrderBy(f => f.Id)
            .ToListAsync();

        if (result.Count > 0)
        {
            Assert.NotNull(result[0].Brand);
            Assert.Equal("BN-001", result[0].Brand.BrandNo);
        }
        if (result.Count > 1)
        {
            Assert.NotNull(result[1].Brand);
            Assert.Equal("BN-002", result[1].Brand.BrandNo);
        }
    }
    [Fact]
    public void FromQuery_IncludeMany()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.Equal(2, result.Count);
        Assert.NotNull(result[0].Order);
        Assert.NotNull(result[0].Order.Details);
        Assert.NotEmpty(result[0].Order.Details);
        Assert.Equal(3, result[0].Order.Details.Count);
        result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, "1", "2", "3"))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.Equal(2, result.Count);
        Assert.NotNull(result[0].Order);
        Assert.NotNull(result[0].Order.Details);
        Assert.NotEmpty(result[0].Order.Details);
        Assert.Equal(3, result[0].Order.Details.Count);
    }
    [Fact]
    public void FromQuery_IncludeMany_Filter()
    {
        Initialize();
        int productId = 1;
        using var repository = dbFactory.Create();
        var result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details, f => f.ProductId == productId)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Buyer = y, Test = x.OrderNo + "_" + y.Age % 4 })
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.NotNull(result[0].Order);
        Assert.NotNull(result[0].Order.Details);
        Assert.NotEmpty(result[0].Order.Details);
        Assert.Single(result[0].Order.Details);
        Assert.True(result[0].Order.Details[0].ProductId == productId);
        Assert.Single(result[1].Order.Details);
        Assert.True(result[1].Order.Details[0].ProductId == productId);
    }
    [Fact]
    public async Task FromQuery_Include_ThenInclude()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = await repository.From<Order>()
            .InnerJoin<User>((a, b) => a.SellerId == b.Id)
            .Include((x, y) => x.Buyer)
            .ThenInclude(f => f.Company)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Seller = y })
            .ToListAsync();

        if (result.Count > 0)
        {
            Assert.NotNull(result[0].Order.Buyer);
            Assert.NotNull(result[0].Order.Buyer.Company);
            Assert.NotNull(result[0].Order.Buyer.SomeTimes.ToString());
        }
    }
    //[Fact]
    //public async Task FromQuery_IncludeMany_ThenInclude()
    //{
    //    using var repository = this.dbFactory.Create();
    //    var result = await repository.From<Order>()
    //        .IncludeMany(f => f.Details)
    //        .ThenInclude(f => f.Product)
    //        .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
    //        .Where((a, b) => a.TotalAmount > 300)
    //        .Select((x, y) => new { Order = x, Buyer = y })
    //        .ToListAsync();
    //    Assert.True(result.Count == 2);
    //    Assert.NotNull(result[0].Order.Details);
    //    Assert.NotEmpty(result[0].Order.Details);
    //    Assert.True(result[0].Order.Details.Count == 3);
    //    Assert.NotNull(result[0].Order.Details[0].Product);
    //    Assert.NotNull(result[0].Order.Details[1].Product);
    //    Assert.NotNull(result[0].Order.Details[2].Product);
    //}
    [Fact]
    public void QueryPage_Include()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.From<OrderDetail>()
            .Include(f => f.Product)
            .Where(f => f.ProductId == 1)
            .OrderBy(f => f.OrderId)
            .Page(2, 1)
            .ToPageList();
        var count = repository.From<OrderDetail>()
            .Where(f => f.ProductId == 1)
            .Count();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        Assert.True(result.TotalCount == count);
        Assert.True(result.Data.Count == result.Count);
        Assert.Equal(1, result.Count);
        Assert.NotEmpty(result.Data);
        Assert.NotNull(result.Data[0].Product);
        Assert.Equal(1, result.Data[0].Product.Id);
    }
    [Fact]
    public void QueryPage_No_OrderBy_ThrowException()
    {
        using var repository = dbFactory.Create();
        Assert.Throws<ArgumentNullException>(() =>
        {
            repository.From<OrderDetail>()
                .Include(f => f.Product)
                .Where(f => f.ProductId == 1)
                .Page(2, 10)
                .ToPageList();
        });
    }
    [Fact]
    public void FromQuery_Ignore_Include()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .IncludeMany((a, b) => a.Orders)
            .ThenIncludeMany(f => f.Details)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.CreatedAt.Date })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) AS [Date],COUNT(b.[Id]) AS [OrderCount],SUM(b.[TotalAmount]) AS [TotalAmount] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) ORDER BY a.[Id],CONVERT(DATE,b.[CreatedAt])", sql);
        var result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            //.IncludeMany((a, b) => a.Orders)
            //.ThenIncludeMany(f => f.Details)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.CreatedAt.Date })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_Groupby()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
           .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
           .IncludeMany((x, y) => x.Details)
           .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
           .Select((x, y) => new { Order = x, Buyer = y })
           .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],b.[Id],b.[TenantId],b.[Name],b.[Gender],b.[Age],b.[CompanyId],b.[GuidField],b.[SomeTimes],b.[SourceType],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy] FROM [sys_order] a INNER JOIN [sys_user] b ON a.[BuyerId]=b.[Id] WHERE a.[TotalAmount]>300 AND a.[Id] IN (N'1',N'2',N'3')", sql);

        var result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.Equal(2, result.Count);
        Assert.NotNull(result[0].Order);
        Assert.NotNull(result[0].Order.Details);
        Assert.NotEmpty(result[0].Order.Details);
        Assert.Equal(3, result[0].Order.Details.Count);

        var sql1 = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) AS [Date],COUNT(b.[Id]) AS [OrderCount],SUM(b.[TotalAmount]) AS [TotalAmount] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) ORDER BY a.[Id]", sql1);
        var result1 = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        if (result1.Count > 0)
        {
            Assert.NotNull(result1[0].Grouping);
            Assert.NotNull(result1[0].Grouping.Name);
        }
        if (result1.Count > 1)
        {
            Assert.NotNull(result1[1].Grouping);
            Assert.NotNull(result1[1].Grouping.Name);
        }
    }
    [Fact]
    public void FromQuery_Groupby_Fields()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { UserId = a.Id, a.Name, CreatedDate = b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id })
            .Select((x, a, b) => new
            {
                UserId1 = x.Grouping.UserId,
                UserName = x.Grouping.Name,
                CreatedDate1 = x.Grouping.CreatedDate,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
           .ToSql(out _);
        Assert.Equal("SELECT a.[Id] AS [UserId1],a.[Name] AS [UserName],CONVERT(DATE,b.[CreatedAt]) AS [CreatedDate1],COUNT(b.[Id]) AS [OrderCount],SUM(b.[TotalAmount]) AS [TotalAmount] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) ORDER BY a.[Id]", sql);
        var result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { UserId = a.Id, a.Name, CreatedDate = b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id })
            .Select((x, a, b) => new
            {
                UserId1 = x.Grouping.UserId,
                UserName = x.Grouping.Name,
                CreatedDate1 = x.Grouping.CreatedDate,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.True(result.Count >= 2);
        Assert.NotNull(result[0].UserName);
        Assert.NotNull(result[1].UserName);
    }
    [Fact]
    public void FromQuery_Groupby_OrderBy()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
           .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
           .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
           .OrderBy((x, a, b) => x.Grouping)
           .Select((x, a, b) => new
           {
               x.Grouping,
               OrderCount = x.Count(b.Id),
               TotalAmount = x.Sum(b.TotalAmount)
           })
           .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) AS [Date],COUNT(b.[Id]) AS [OrderCount],SUM(b.[TotalAmount]) AS [TotalAmount] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) ORDER BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt])", sql);
        var result = repository.From<User>()
          .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
          .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
          .OrderBy((x, a, b) => x.Grouping)
          .Select((x, a, b) => new
          {
              x.Grouping,
              OrderCount = x.Count(b.Id),
              TotalAmount = x.Sum(b.TotalAmount)
          })
          .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        var sql1 = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => x.Grouping)
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                CreatedAt = x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) AS [CreatedAt],COUNT(b.[Id]) AS [OrderCount],SUM(b.[TotalAmount]) AS [TotalAmount] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) ORDER BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt])", sql1);
        var result1 = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => x.Grouping)
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                CreatedAt = x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result1);
        Assert.True(result1.Count > 0);
    }
    [Fact]
    public async Task FromQuery_Groupby_OrderBy_Fields()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
           .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
           .GroupBy((a, b) => new { UserId = a.Id, a.Name, CreatedDate = b.CreatedAt.Date })
           .OrderBy((x, a, b) => x.Grouping.UserId)
           .OrderByDescending((x, a, b) => x.Grouping.Name)
           .OrderBy((x, a, b) => x.Grouping.CreatedDate)
           .Select((x, a, b) => new
           {
               x.Grouping,
               OrderCount = x.Count(b.Id),
               TotalAmount = x.Sum(b.TotalAmount)
           })
           .ToSql(out _);
        Assert.Equal("SELECT a.[Id] AS [UserId],a.[Name],CONVERT(DATE,b.[CreatedAt]) AS [CreatedDate],COUNT(b.[Id]) AS [OrderCount],SUM(b.[TotalAmount]) AS [TotalAmount] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) ORDER BY a.[Id],a.[Name] DESC,CONVERT(DATE,b.[CreatedAt])", sql);

        await repository.From<User>()
           .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
           .GroupBy((a, b) => new { UserId = a.Id, a.Name, CreatedDate = b.CreatedAt.Date })
           .OrderBy((x, a, b) => x.Grouping.UserId)
           .OrderByDescending((x, a, b) => x.Grouping.Name)
           .OrderBy((x, a, b) => x.Grouping.CreatedDate)
           .Select((x, a, b) => new
           {
               x.Grouping,
               OrderCount = x.Count(b.Id),
               TotalAmount = x.Sum(b.TotalAmount)
           })
           .FirstAsync();
    }
    [Fact]
    public async Task FromQuery_Groupby_Having()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From(f => f.From<Order, OrderDetail>()
                .Where((x, y) => x.Id == y.OrderId)
                .GroupBy((x, y) => new { x.BuyerId, x.CreatedAt.Date })
                .Select((x, a, b) => new
                {
                    a.BuyerId,
                    a.CreatedAt.Date,
                    OrderCount = x.Count(a.Id),
                    ProductCount = x.CountDistinct(b.ProductId),
                    TotalAmount = x.Sum(a.TotalAmount)
                }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Where((a, b) => a.ProductCount > 2 && a.TotalAmount > 300)
            .OrderBy((a, b) => b.Id)
            .Select((a, b) => new
            {
                a.BuyerId,
                BuyerName = b.Name,
                BuyDate = a.Date,
                a.ProductCount,
                a.OrderCount,
                a.TotalAmount
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.[BuyerId],b.[Name] AS [BuyerName],a.[Date] AS [BuyDate],a.[ProductCount],a.[OrderCount],a.[TotalAmount] FROM (SELECT a.[BuyerId],CONVERT(DATE,a.[CreatedAt]) AS [Date],COUNT(a.[Id]) AS [OrderCount],COUNT(DISTINCT b.[ProductId]) AS [ProductCount],SUM(a.[TotalAmount]) AS [TotalAmount] FROM [sys_order] a,[sys_order_detail] b WHERE a.[Id]=b.[OrderId] GROUP BY a.[BuyerId],CONVERT(DATE,a.[CreatedAt])) a INNER JOIN [sys_user] b ON a.[BuyerId]=b.[Id] WHERE a.[ProductCount]>2 AND a.[TotalAmount]>300 ORDER BY b.[Id]", sql);

        var result = await repository.From(f => f
            .From<Order, OrderDetail>()
                .Where((x, y) => x.Id == y.OrderId)
                .GroupBy((x, y) => new { x.BuyerId, x.CreatedAt.Date })
                .Select((x, a, b) => new
                {
                    a.BuyerId,
                    a.CreatedAt.Date,
                    OrderCount = x.Count(a.Id),
                    ProductCount = x.CountDistinct(b.ProductId),
                    TotalAmount = x.Sum(a.TotalAmount)
                }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Where((a, b) => a.ProductCount > 2 && a.TotalAmount > 300)
            .OrderBy((a, b) => b.Id)
            .Select((a, b) => new
            {
                a.BuyerId,
                BuyerName = b.Name,
                BuyDate = a.Date,
                a.ProductCount,
                a.OrderCount,
                a.TotalAmount
            })
            .ToListAsync();
        Assert.True(result.Count > 0);

        var sql1 = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .InnerJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
            .GroupBy((a, b, c) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b, c) => x.Sum(b.TotalAmount) > 300 && x.CountDistinct(c.ProductId) > 2)
            .OrderBy((x, a, b, c) => new { x.Grouping })
            .Select((x, a, b, c) => new
            {
                BuyerId = x.Grouping.Id,
                BuyerName = x.Grouping.Name,
                BuyDate = x.Grouping.Date,
                ProductCount = x.CountDistinct(c.ProductId),
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id] AS [BuyerId],a.[Name] AS [BuyerName],CONVERT(DATE,b.[CreatedAt]) AS [BuyDate],COUNT(DISTINCT c.[ProductId]) AS [ProductCount],COUNT(b.[Id]) AS [OrderCount],SUM(b.[TotalAmount]) AS [TotalAmount] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] INNER JOIN [sys_order_detail] c ON b.[Id]=c.[OrderId] GROUP BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) HAVING SUM(b.[TotalAmount])>300 AND COUNT(DISTINCT c.[ProductId])>2 ORDER BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt])", sql1);
        var result1 = await repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .InnerJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
            .GroupBy((a, b, c) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b, c) => x.Sum(b.TotalAmount) > 300 && x.CountDistinct(c.ProductId) > 2)
            .OrderBy((x, a, b, c) => new { x.Grouping })
            .Select((x, a, b, c) => new
            {
                BuyerId = x.Grouping.Id,
                BuyerName = x.Grouping.Name,
                BuyDate = x.Grouping.Date,
                ProductCount = x.CountDistinct(c.ProductId),
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToListAsync();
        Assert.True(result1.Count > 0);
    }
    [Fact]
    public void FromQuery_Groupby_Having_OrderBy()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => x.Grouping)
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) AS [Date],COUNT(b.[Id]) AS [OrderCount],SUM(b.[TotalAmount]) AS [TotalAmount] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] WHERE EXISTS(SELECT * FROM [sys_order_detail] f WHERE b.[Id]=f.[OrderId] AND f.[ProductId]=2) GROUP BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) HAVING SUM(b.[TotalAmount])>300 ORDER BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt])", sql);
        var result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => x.Grouping)
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_Groupby_Having_OrderBy_Fields()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => x.Grouping.Id)
            .OrderByDescending((x, a, b) => x.Grouping.Name)
            .OrderBy((x, a, b) => x.Grouping.Date)
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) AS [Date],COUNT(b.[Id]) AS [OrderCount],SUM(b.[TotalAmount]) AS [TotalAmount] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] WHERE EXISTS(SELECT * FROM [sys_order_detail] f WHERE b.[Id]=f.[OrderId] AND f.[ProductId]=2) GROUP BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) HAVING SUM(b.[TotalAmount])>300 ORDER BY a.[Id],a.[Name] DESC,CONVERT(DATE,b.[CreatedAt])", sql);
        var result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => x.Grouping.Id)
            .OrderByDescending((x, a, b) => x.Grouping.Name)
            .OrderBy((x, a, b) => x.Grouping.Date)
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void Where_Exists()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => repository.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[GuidField],a.[SomeTimes],a.[SourceType],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_user] a WHERE EXISTS(SELECT * FROM [sys_company] t WHERE t.[Name] LIKE N'%谷歌%' AND a.[CompanyId]=t.[Id])", sql);
        var result = repository.From<User>()
            .Where(f => repository.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => repository.From<Company>('b').Exists(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[GuidField],a.[SomeTimes],a.[SourceType],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_user] a WHERE EXISTS(SELECT * FROM [sys_company] b WHERE b.[Name] LIKE N'%谷歌%' AND a.[CompanyId]=b.[Id])", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Company>('b').Exists(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => repository.From<Order>('b')
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .Exists((x, y) => x.BuyerId == f.Id && y.Price > 200))
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[GuidField],a.[SomeTimes],a.[SourceType],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_user] a WHERE EXISTS(SELECT * FROM [sys_order] b INNER JOIN [sys_order_detail] c ON b.[Id]=c.[OrderId] WHERE b.[BuyerId]=a.[Id] AND c.[Price]>200)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Order>('b')
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .Exists((x, y) => x.BuyerId == f.Id && y.Price > 200))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
               .InnerJoin((x, y) => x.Id == y.OrderId)
               .Exists((x, y) => x.BuyerId == f.Id && y.Price > 200))
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[GuidField],a.[SomeTimes],a.[SourceType],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_user] a WHERE EXISTS(SELECT * FROM [sys_order] b INNER JOIN [sys_order_detail] c ON b.[Id]=c.[OrderId] WHERE b.[BuyerId]=a.[Id] AND c.[Price]>200)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
               .InnerJoin((x, y) => x.Id == y.OrderId)
               .Exists((x, y) => x.BuyerId == f.Id && y.Price > 200))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
               .Exists((x, y) => x.Id == y.OrderId && x.BuyerId == f.Id && y.Price > 200))
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[GuidField],a.[SomeTimes],a.[SourceType],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_user] a WHERE EXISTS(SELECT * FROM [sys_order] b,[sys_order_detail] c WHERE b.[Id]=c.[OrderId] AND b.[BuyerId]=a.[Id] AND c.[Price]>200)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
               .Exists((x, y) => x.Id == y.OrderId && x.BuyerId == f.Id && y.Price > 200))
           .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
                .Where((x, y) => x.Id == y.OrderId)
                .Exists((x, y) => x.BuyerId == f.Id && y.Price > 200))
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[GuidField],a.[SomeTimes],a.[SourceType],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_user] a WHERE EXISTS(SELECT * FROM [sys_order] b,[sys_order_detail] c WHERE b.[Id]=c.[OrderId] AND b.[BuyerId]=a.[Id] AND c.[Price]>200)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
                .Where((x, y) => x.Id == y.OrderId)
                .Exists((x, y) => x.BuyerId == f.Id && y.Price > 200))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => Sql.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[GuidField],a.[SomeTimes],a.[SourceType],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_user] a WHERE EXISTS(SELECT * FROM [sys_company] t WHERE t.[Name] LIKE N'%谷歌%' AND a.[CompanyId]=t.[Id])", sql);
        result = repository.From<User>()
            .Where(f => Sql.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        var sql1 = repository.From<User>()
            .Where(f => Sql.Exists(t => t
                .From<Order, OrderDetail>('b')
                .Where((x, y) => x.Id == y.OrderId && f.Id == x.BuyerId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 1)
                .Select()))
            .GroupBy(f => new { f.Gender, f.CompanyId })
            .Select((t, a) => new { t.Grouping, UserTotal = t.CountDistinct(a.Id) })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Gender],a.[CompanyId],COUNT(DISTINCT a.[Id]) AS [UserTotal] FROM [sys_user] a WHERE EXISTS(SELECT b.[Id] FROM [sys_order] b,[sys_order_detail] c WHERE b.[Id]=c.[OrderId] AND a.[Id]=b.[BuyerId] GROUP BY b.[Id] HAVING COUNT(DISTINCT c.[ProductId])>1) GROUP BY a.[Gender],a.[CompanyId]", sql1);
        var result1 = repository.From<User>()
            .Where(f => Sql.Exists(t => t
                .From<Order, OrderDetail>('b')
                .Where((x, y) => x.Id == y.OrderId && f.Id == x.BuyerId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 1)
                .Select()))
            .GroupBy(f => new { f.Gender, f.CompanyId })
            .Select((t, a) => new { t.Grouping, UserTotal = t.CountDistinct(a.Id) })
            .ToList();
        Assert.NotNull(result1);
        Assert.True(result1.Count > 0);
    }
    [Fact]
    public async Task FromQuery_Exists()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.Exists(f => f
                .From<Order, OrderDetail>('c')
                .Where((a, b) => a.BuyerId == x.Id && a.Id == b.OrderId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                .Select()))
            .GroupBy((x, y) => new { x.Gender, x.CompanyId })
            .Select((x, a, b) => new { x.Grouping, UserTotal = x.CountDistinct(a.Id) })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Gender],a.[CompanyId],COUNT(DISTINCT a.[Id]) AS [UserTotal] FROM [sys_user] a INNER JOIN [sys_company] b ON a.[CompanyId]=b.[Id] WHERE EXISTS(SELECT c.[Id] FROM [sys_order] c,[sys_order_detail] d WHERE c.[BuyerId]=a.[Id] AND c.[Id]=d.[OrderId] GROUP BY c.[Id] HAVING COUNT(DISTINCT d.[ProductId])>0) GROUP BY a.[Gender],a.[CompanyId]", sql);
        var result = await repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.Exists(f => f
                .From<Order, OrderDetail>('c')
                .Where((a, b) => a.BuyerId == x.Id && a.Id == b.OrderId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                .Select()))
            .GroupBy((x, y) => new { x.Gender, x.CompanyId })
            .Select((x, a, b) => new { x.Grouping, UserTotal = x.CountDistinct(a.Id) })
            .ToListAsync();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void CteTable_Exists()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var myOrders = repository.From<OrderDetail, Order>()
            .Where((a, b) => a.OrderId == b.Id)
            .GroupBy((a, b) => new { a.OrderId, b.BuyerId })
            .Having((x, a, b) => x.CountDistinct(a.ProductId) > 1)
            .Select((x, a, b) => x.Grouping)
            .AsCteTable("myOrders");

        var sql = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.Exists(myOrders, f => f.BuyerId == x.Id))
            .Select((a, b) => new { a.Id, a.Name, CompanyName = b.Name })
            .ToSql(out _);
        Assert.Equal(@"WITH [myOrders]([OrderId],[BuyerId]) AS 
(
SELECT a.[OrderId],b.[BuyerId] FROM [sys_order_detail] a,[sys_order] b WHERE a.[OrderId]=b.[Id] GROUP BY a.[OrderId],b.[BuyerId] HAVING COUNT(DISTINCT a.[ProductId])>1
)
SELECT a.[Id],a.[Name],b.[Name] AS [CompanyName] FROM [sys_user] a INNER JOIN [sys_company] b ON a.[CompanyId]=b.[Id] WHERE EXISTS(SELECT * FROM [myOrders] f WHERE f.[BuyerId]=a.[Id])", sql);

        var result = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.Exists(myOrders, f => f.BuyerId == x.Id))
            .Select((a, b) => new { a.Id, a.Name, CompanyName = b.Name })
            .First();
        Assert.NotNull(result);
    }
    [Fact]
    public void FromQuery_In_Exists()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((x, y) => Sql.In(x.Id, new int[] { 1, 2, 3 }) && Sql.Exists<OrderDetail>(f => y.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => x.Grouping.Id)
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) AS [Date],COUNT(b.[Id]) AS [OrderCount],SUM(b.[TotalAmount]) AS [TotalAmount] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] WHERE a.[Id] IN (1,2,3) AND EXISTS(SELECT * FROM [sys_order_detail] f WHERE b.[Id]=f.[OrderId] AND f.[ProductId]=2) GROUP BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) HAVING SUM(b.[TotalAmount])>300 ORDER BY a.[Id]", sql);
        var result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((x, y) => Sql.In(x.Id, new int[] { 1, 2, 3 }) && Sql.Exists<OrderDetail>(f => y.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => x.Grouping.Id)
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => Sql.In(a.Id, new int[] { 1, 2, 3 }) && Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => new { UserId = a.Id, b.CreatedAt.Date })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) AS [Date],COUNT(b.[Id]) AS [OrderCount],SUM(b.[TotalAmount]) AS [TotalAmount] FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] WHERE a.[Id] IN (1,2,3) AND EXISTS(SELECT * FROM [sys_order_detail] f WHERE b.[Id]=f.[OrderId] AND f.[ProductId]=2) GROUP BY a.[Id],a.[Name],CONVERT(DATE,b.[CreatedAt]) HAVING SUM(b.[TotalAmount])>300 ORDER BY a.[Id],CONVERT(DATE,b.[CreatedAt])", sql);
        result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => Sql.In(a.Id, new int[] { 1, 2, 3 }) && Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => new { UserId = a.Id, b.CreatedAt.Date })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_In1()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order>('b')
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id] FROM [sys_user] a WHERE a.[Id] IN (SELECT b.[BuyerId] FROM [sys_order] b INNER JOIN [sys_order_detail] c ON b.[Id]=c.[OrderId] AND c.[ProductId]=1)", sql);
        var result = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order>('b')
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order, OrderDetail>('b')
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id] FROM [sys_user] a WHERE a.[Id] IN (SELECT b.[BuyerId] FROM [sys_order] b,[sys_order_detail] c WHERE b.[Id]=c.[OrderId] AND c.[ProductId]=1)", sql);
        result = repository.From<User>()
           .Where(f => Sql.In(f.Id, t => t.From<Order, OrderDetail>('b')
               .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
               .Select((x, y) => x.BuyerId)))
           .Select(f => f.Id)
           .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        var subQuery = repository.From<Order>('b')
              .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
              .Select((x, y) => x.BuyerId);
        sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, subQuery))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id] FROM [sys_user] a WHERE a.[Id] IN (SELECT b.[BuyerId] FROM [sys_order] b INNER JOIN [sys_order_detail] c ON b.[Id]=c.[OrderId] AND c.[ProductId]=1)", sql);
        result = repository.From<User>()
            .Where(f => Sql.In(f.Id, subQuery))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        subQuery = repository.From<Order, OrderDetail>('b')
            .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
            .Select((x, y) => x.BuyerId);
        sql = repository.From<User>()
           .Where(f => Sql.In(f.Id, subQuery))
           .Select(f => f.Id)
           .ToSql(out _);
        Assert.Equal("SELECT a.[Id] FROM [sys_user] a WHERE a.[Id] IN (SELECT b.[BuyerId] FROM [sys_order] b,[sys_order_detail] c WHERE b.[Id]=c.[OrderId] AND c.[ProductId]=1)", sql);
        result = repository.From<User>()
           .Where(f => Sql.In(f.Id, subQuery))
           .Select(f => f.Id)
           .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_In_Exists1()
    {
        using var repository = dbFactory.Create();
        bool? isMale = true;
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order>('b')
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id] FROM [sys_user] a WHERE a.[Id] IN (SELECT b.[BuyerId] FROM [sys_order] b INNER JOIN [sys_order_detail] c ON b.[Id]=c.[OrderId] AND c.[ProductId]=1) AND EXISTS(SELECT * FROM [sys_order] x,[sys_company] y WHERE a.[Id]=x.[SellerId] AND a.[CompanyId]=y.[Id])", sql);
        var result = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order>('b')
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order, OrderDetail>('b')
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id] FROM [sys_user] a WHERE a.[Id] IN (SELECT b.[BuyerId] FROM [sys_order] b,[sys_order_detail] c WHERE b.[Id]=c.[OrderId] AND c.[ProductId]=1) AND EXISTS(SELECT * FROM [sys_order] x,[sys_company] y WHERE a.[Id]=x.[SellerId] AND a.[CompanyId]=y.[Id])", sql);
        result = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order, OrderDetail>('b')
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_In_Exists_Group_CountDistinct_Count()
    {
        using var repository = dbFactory.Create();
        bool? isMale = true;
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<OrderDetail>('b')
                .InnerJoin<Order>((a, b) => a.OrderId == b.Id && a.ProductId == 1)
                .Select((x, y) => y.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Company, Order>((x, y) => f.Id == y.SellerId && f.CompanyId == x.Id))
            .GroupBy(f => new { f.Gender, f.Age })
            .Select((t, a) => new { t.Grouping, CompanyCount = t.CountDistinct(a.CompanyId), UserCount = t.Count(a.Id) })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Gender],a.[Age],COUNT(DISTINCT a.[CompanyId]) AS [CompanyCount],COUNT(a.[Id]) AS [UserCount] FROM [sys_user] a WHERE a.[Id] IN (SELECT c.[BuyerId] FROM [sys_order_detail] b INNER JOIN [sys_order] c ON b.[OrderId]=c.[Id] AND b.[ProductId]=1) AND EXISTS(SELECT * FROM [sys_company] x,[sys_order] y WHERE a.[Id]=y.[SellerId] AND a.[CompanyId]=x.[Id]) GROUP BY a.[Gender],a.[Age]", sql);

        var result = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<OrderDetail>('b')
                .InnerJoin<Order>((a, b) => a.OrderId == b.Id && a.ProductId == 1)
                .Select((x, y) => y.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Company, Order>((x, y) => f.Id == y.SellerId && f.CompanyId == x.Id))
            .GroupBy(f => new { f.Gender, f.Age })
            .Select((t, a) => new { t.Grouping, CompanyCount = t.CountDistinct(a.CompanyId), UserCount = t.Count(a.Id) })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_Aggregate()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .SelectAggregate((x, a) => new
            {
                OrderCount = x.Count(a.Id),
                TotalAmount = x.Sum(a.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT COUNT(a.[Id]) AS [OrderCount],SUM(a.[TotalAmount]) AS [TotalAmount] FROM [sys_order] a", sql);
        var result = repository.From<Order>()
            .SelectAggregate((x, a) => new
            {
                OrderCount = x.Count(a.Id),
                TotalAmount = x.Sum(a.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<Order>()
            .Select(a => new
            {
                OrderCount = Sql.Count(a.Id),
                TotalAmount = Sql.Sum(a.TotalAmount)
            })
        .ToSql(out _);
        Assert.Equal("SELECT COUNT(a.[Id]) AS [OrderCount],SUM(a.[TotalAmount]) AS [TotalAmount] FROM [sys_order] a", sql);
        result = repository.From<Order>()
            .Select(a => new
            {
                OrderCount = Sql.Count(a.Id),
                TotalAmount = Sql.Sum(a.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        //SqlServer不支持多表join聚合查询
    }
    [Fact]
    public void Query_Count()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var count = repository.From<User>().Count();
        var count1 = repository.From<User>().Select(f => Sql.Count()).First();
        var count2 = repository.QueryFirst<int>("SELECT COUNT(1) FROM sys_user");
        Assert.True(count == count1);
        Assert.True(count == count2);
    }
    [Fact]
    public void Query_Where_Count()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.From<User>()
            .Where(t => Sql.Exists(f =>
                f.From<Order, OrderDetail>('o')
                    .Where((a, b) => a.BuyerId == t.Id && a.Id == b.OrderId)
                    .GroupBy((a, b) => a.Id)
                    .Having((x, a, b) => Sql.Count(b.Id) > 0)
                    .Select()))
            .GroupBy(f => new { f.Gender, f.CompanyId })
            .Select((x, y) => new { x.Grouping, UserTotal = x.CountDistinct(y.Id) })
            .ToList();
        if (result.Count > 0)
        {
            Assert.NotNull(result[0]);
            Assert.NotNull(result[0].Grouping);
            Assert.True(result[0].UserTotal > 0);
        }
    }
    [Fact]
    public void Query_Max()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var count = repository.From<Order>().Max(f => f.TotalAmount);
        var count1 = repository.From<Order>().Select(f => Sql.Max(f.TotalAmount)).First();
        var count2 = repository.QueryFirst<double>("SELECT MAX(TotalAmount) FROM sys_order");
        Assert.True(count == count1);
        Assert.True(count == count2);
    }
    [Fact]
    public void Query_Min()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var count = repository.From<Order>().Min(f => f.TotalAmount);
        var count1 = repository.From<Order>().Select(f => Sql.Min(f.TotalAmount)).First();
        var count2 = repository.QueryFirst<double>("SELECT MIN(TotalAmount) FROM sys_order");
        Assert.True(count == count1);
        Assert.True(count == count2);
    }
    [Fact]
    public void Query_Avg()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var value1 = repository.From<Order>().Avg(f => f.TotalAmount);
        var value2 = repository.From<Order>().Select(f => Sql.Avg(f.TotalAmount)).First();
        var value3 = repository.QueryFirst<double>("SELECT AVG(TotalAmount) FROM sys_order");
        Assert.True(value1 == value2);
        Assert.True(value1 == value3);
    }
    [Fact]
    public void Query_ValueTuple()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var sql = "SELECT Id,OrderNo,TotalAmount FROM sys_order";
        var result = repository.Query<(string OrderId, string OrderNo, double TotalAmount)>(sql);
        Assert.NotNull(result);
    }
    [Fact]
    public void Query_Json()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var result = repository.Get<Order>("1");
        Assert.NotNull(result);
        Assert.NotNull(result.Products);
        Assert.NotNull(result.Disputes);
    }
    [Fact]
    public void Query_SelectNull_WhereNull()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .Where(x => x.ProductCount == null)
            .And(true, f => !f.ProductCount.HasValue)
            .Select(x => new
            {
                NoOrderNo = x.OrderNo == null,
                HasProduct = x.ProductCount.HasValue
            })
            .ToSql(out _);
        Assert.Equal("SELECT (CASE WHEN a.[OrderNo] IS NULL THEN 1 ELSE 0 END) AS [NoOrderNo],(CASE WHEN a.[ProductCount] IS NOT NULL THEN 1 ELSE 0 END) AS [HasProduct] FROM [sys_order] a WHERE a.[ProductCount] IS NULL AND a.[ProductCount] IS NULL", sql);
        var result = repository.From<Order>()
            .Where(x => x.ProductCount == null)
            .And(true, f => !f.ProductCount.HasValue)
            .Select(x => new
            {
                NoOrderNo = x.OrderNo == null,
                HasProduct = x.ProductCount.HasValue
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public async Task Query_Where_IsNull()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .Where(x => x.ProductCount == null || x.BuyerId.IsNull())
            .And(true, f => !f.ProductCount.HasValue)
            .Select(x => x.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id] FROM [sys_order] a WHERE (a.[ProductCount] IS NULL OR a.[BuyerId] IS NULL) AND a.[ProductCount] IS NULL", sql);
        var result = repository.From<Order>()
            .Where(x => x.ProductCount == null || x.BuyerId.IsNull())
            .And(true, f => !f.ProductCount.HasValue)
            .Select(x => x.Id)
            .ToList();
        Assert.NotNull(result);

        var sql1 = repository.From<Order>()
           .Where(x => x.ProductCount.IsNull(0) > 0 || x.BuyerId.IsNull(0) >= 0)
           .Select(f => new
           {
               f.Id,
               f.OrderNo,
               ProductCount = f.ProductCount.IsNull(0),
               BuyerId = f.BuyerId.IsNull(0),
               TotalAmount = f.TotalAmount.IsNull(0)
           })
           .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[OrderNo],ISNULL(a.[ProductCount],0) AS [ProductCount],ISNULL(a.[BuyerId],0) AS [BuyerId],ISNULL(a.[TotalAmount],0) AS [TotalAmount] FROM [sys_order] a WHERE ISNULL(a.[ProductCount],0)>0 OR ISNULL(a.[BuyerId],0)>=0", sql1);

        await repository.BeginTransactionAsync();
        await repository.UpdateAsync<Order>(new { Id = "1", BuyerId = DBNull.Value });
        await repository.UpdateAsync<Order>(new { Id = "2", ProductCount = DBNull.Value });
        await repository.UpdateAsync<Order>(new { Id = "3", TotalAmount = DBNull.Value });
        var result1 = repository.From<Order>()
            .Where(x => x.ProductCount.IsNull(0) > 0 || x.BuyerId.IsNull(0) >= 0)
            .Select(f => new
            {
                f.Id,
                f.OrderNo,
                ProductCount = f.ProductCount.IsNull(0),
                BuyerId = f.BuyerId.IsNull(0),
                f.TotalAmount
            })
            .ToList();
        await repository.CommitAsync();
        var myOrders = result1.FindAll(f => "1,2,3".Contains(f.Id)).OrderBy(f => f.Id).ToList();
        Assert.True(result1.Count >= 3);
        Assert.Equal(0, myOrders[0].BuyerId);
        Assert.Equal(0, myOrders[1].ProductCount);
        Assert.Equal(0, myOrders[2].TotalAmount);
    }
    [Fact]
    public async Task Query_Union()
    {
        var id1 = "1";
        var id2 = "2";
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .Where(x => x.Id == id1)
            .Select(x => new
            {
                x.Id,
                x.OrderNo,
                x.SellerId,
                x.BuyerId
            })
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                }))
            .ToSql(out _);
        Assert.Equal(@"SELECT a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId] FROM [sys_order] a WHERE a.[Id]=@p0 UNION ALL
SELECT a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId] FROM [sys_order] a WHERE a.[Id]<>@p1", sql);

        var result = await repository.From<Order>()
           .Where(x => x.Id == id1)
           .Select(x => new
           {
               x.Id,
               x.OrderNo,
               x.SellerId,
               x.BuyerId
           })
           .UnionAll(f => f.From<Order>()
               .Where(x => x.Id != id2)
               .Select(x => new
               {
                   x.Id,
                   x.OrderNo,
                   x.SellerId,
                   x.BuyerId
               }))
           .ToListAsync();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public async Task Query_Union_Take()
    {
        this.Initialize();
        string id1 = "3", id2 = "2";
        using var repository = this.dbFactory.Create();
        var sql = repository
            .From<Order>('b')
                .Where(x => x.Id == id1)
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
               .Take(1)
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                }))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .Select((x, y) => new { x.Id, x.OrderNo, x.SellerId, x.BuyerId, BuyerName = y.Name })
            .ToSql(out var dbParameters);
        Assert.Equal(@"SELECT a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId],b.[Name] AS [BuyerName] FROM (SELECT * FROM (SELECT TOP 1 b.[Id],b.[OrderNo],b.[SellerId],b.[BuyerId] FROM [sys_order] b WHERE b.[Id]=@p0 ORDER BY b.[Id]) a UNION ALL
SELECT a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId] FROM [sys_order] a WHERE a.[Id]<>@p1) a INNER JOIN [sys_user] b ON a.[BuyerId]=b.[Id]", sql);
        Assert.Equal("@p0", dbParameters[0].ParameterName);
        Assert.Equal("@p1", dbParameters[1].ParameterName);
        var result = await repository
            .From<Order>('b')
                .Where(x => x.Id == id1)
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
               .Take(1)
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                }))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .Select((x, y) => new { x.Id, x.OrderNo, x.SellerId, x.BuyerId, BuyerName = y.Name })
            .ToListAsync();
        Assert.True(result.Count > 0);

        var sql1 = repository
            .From<User>()
            .WithTable(t => t
                .From<Order>()
                    .InnerJoin<User>((a, b) => a.SellerId == b.Id)
                    .Where((x, y) => x.Id == id1)
                    .OrderBy((a, b) => a.Id)
                    .Select((x, y) => new
                    {
                        x.Id,
                        x.OrderNo,
                        x.SellerId,
                        x.BuyerId
                    })
                    .Take(1)
                .UnionAll(f => f.From<Order>()
                    .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
                    .Where((x, y) => x.Id != id2)
                    .Select((x, y) => new
                    {
                        x.Id,
                        x.OrderNo,
                        x.SellerId,
                        x.BuyerId
                    })))
          .InnerJoin<User>((a, b, c) => a.Id == b.SellerId)
          .InnerJoin((a, b, c) => b.BuyerId == c.Id)
          .Select((x, y, z) => new { y.Id, y.OrderNo, y.SellerId, SellerName = x.Name, y.BuyerId, BuyerName = z.Name })
          .ToSql(out var dbParameters1);
        Assert.Equal(@"SELECT b.[Id],b.[OrderNo],b.[SellerId],a.[Name] AS [SellerName],b.[BuyerId],c.[Name] AS [BuyerName] FROM [sys_user] a INNER JOIN (SELECT * FROM (SELECT TOP 1 a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId] FROM [sys_order] a INNER JOIN [sys_user] b ON a.[SellerId]=b.[Id] WHERE a.[Id]=@p0 ORDER BY a.[Id]) a UNION ALL
SELECT a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId] FROM [sys_order] a INNER JOIN [sys_user] b ON a.[BuyerId]=b.[Id] WHERE a.[Id]<>@p1) b ON a.[Id]=b.[SellerId] INNER JOIN [sys_user] c ON b.[BuyerId]=c.[Id]", sql1);
        Assert.Equal("@p0", dbParameters1[0].ParameterName);
        Assert.Equal("@p1", dbParameters1[1].ParameterName);

        var result1 = repository
            .From<User>()
            .WithTable(t => t
                .From<Order>()
                    .InnerJoin<User>((a, b) => a.SellerId == b.Id)
                    .Where((x, y) => x.Id == id1)
                    .OrderBy((a, b) => a.Id)
                    .Select((x, y) => new
                    {
                        x.Id,
                        x.OrderNo,
                        x.SellerId,
                        x.BuyerId
                    })
                    .Take(1)
                .UnionAll(f => f.From<Order>()
                    .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
                    .Where((x, y) => x.Id != id2)
                    .Select((x, y) => new
                    {
                        x.Id,
                        x.OrderNo,
                        x.SellerId,
                        x.BuyerId
                    })))
            .InnerJoin<User>((a, b, c) => a.Id == b.SellerId)
            .InnerJoin((a, b, c) => b.BuyerId == c.Id)
            .Select((x, y, z) => new { y.Id, y.OrderNo, y.SellerId, SellerName = x.Name, y.BuyerId, BuyerName = z.Name })
            .ToList();
        Assert.True(result1.Count > 0);
    }
    [Fact]
    public async Task FromQuery_Union_Limit()
    {
        this.Initialize();
        string id1 = "4", id2 = "2";
        using var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
                .Where(x => x.Id == id1)
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1)
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1))
            .ToSql(out var dbParameters);
        Assert.Equal(@"SELECT * FROM (SELECT TOP 1 a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId] FROM [sys_order] a WHERE a.[Id]=@p0 ORDER BY a.[Id]) a UNION ALL
SELECT * FROM (SELECT TOP 1 a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId] FROM [sys_order] a WHERE a.[Id]<>@p1) a", sql);

        Assert.Equal(2, dbParameters.Count);
        Assert.True((string)dbParameters[0].Value == id1);
        Assert.True((string)dbParameters[1].Value == id2);

        var result = await repository.From<Order>()
                .Where(x => x.Id == id1)
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1)
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                }).Take(1))
            .ToListAsync();
        Assert.True(result.Count > 0);
        foreach (var item in result)
        {
            Assert.True(item.Id == id1 || item.Id != id2);
        }
    }
    [Fact]
    public void FromQuery_Union_SubQuery_Limit()
    {
        this.Initialize();
        using var repository = this.dbFactory.Create();
        var sql = repository.From(f => f.From<Order>()
                .Where(x => x.Id != "3")
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
               .Take(1))
            .Select()
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id == "3")
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1))
            .ToSql(out _);
        Assert.Equal(@"SELECT a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId] FROM (SELECT TOP 1 a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId] FROM [sys_order] a WHERE a.[Id]<>N'3' ORDER BY a.[Id]) a UNION ALL
SELECT * FROM (SELECT TOP 1 a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId] FROM [sys_order] a WHERE a.[Id]=N'3') a", sql);
        var result = repository.From(f => f.From<Order>()
                .Where(x => x.Id != "3")
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
               .Take(1))
            .Select()
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id == "3")
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public async Task FromQuery_Union_SubQuery_OrderBy()
    {
        this.Initialize();
        string id1 = "4", id2 = "2";
        using var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
                .Where(x => x.Id == id1)
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
               .Take(1)
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .OrderByDescending(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1))
            .ToSql(out var dbParameters);
        Assert.Equal(@"SELECT * FROM (SELECT TOP 1 a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId] FROM [sys_order] a WHERE a.[Id]=@p0 ORDER BY a.[Id]) a UNION ALL
SELECT * FROM (SELECT TOP 1 a.[Id],a.[OrderNo],a.[SellerId],a.[BuyerId] FROM [sys_order] a WHERE a.[Id]<>@p1 ORDER BY a.[Id] DESC) a", sql);
        Assert.Equal(2, dbParameters.Count);
        Assert.True((string)dbParameters[0].Value == id1);
        Assert.True((string)dbParameters[1].Value == id2);

        var result = await repository.From<Order>()
                .Where(x => x.Id == id1)
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
               .Take(1)
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .OrderByDescending(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1))
           .ToListAsync();
        Assert.True(result.Count > 0);
        foreach (var item in result)
        {
            Assert.True(item.Id == id1 || item.Id != id2);
        }
    }
    [Fact]
    public void Union_Take()
    {
        this.Initialize();
        using var repository = this.dbFactory.Create();
        var sql = repository
            .From(f => f.From<Menu>()
                .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId }))
            .InnerJoin<Page>((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id == "2")
                .OrderByDescending(f => f.Id)
                .Select(x => new
                {
                    Id = x.BuyerId,
                    Name = x.OrderNo,
                    ParentId = x.SellerId,
                    Url = x.BuyerId.ToString()
                })
                .Take(1))
            .ToSql(out _);
        Assert.Equal(@"SELECT a.[Id],a.[Name],a.[ParentId],b.[Url] FROM (SELECT a.[Id],a.[Name],a.[ParentId],a.[PageId] FROM [sys_menu] a) a INNER JOIN [sys_page] b ON a.[Id]=b.[Id] UNION ALL
SELECT * FROM (SELECT TOP 1 a.[BuyerId] AS [Id],a.[OrderNo] AS [Name],a.[SellerId] AS [ParentId],CAST(a.[BuyerId] AS NVARCHAR(MAX)) AS [Url] FROM [sys_order] a WHERE a.[Id]=N'2' ORDER BY a.[Id] DESC) a", sql);

        var result = repository
            .From(f => f.From<Menu>()
                .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId }))
            .InnerJoin<Page>((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id == "2")
                .OrderByDescending(f => f.Id)
                .Select(x => new
                {
                    Id = x.BuyerId,
                    Name = x.OrderNo,
                    ParentId = x.SellerId,
                    Url = x.BuyerId.ToString()
                })
                .Take(1))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public async Task Query_WithCte_SelfRef()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        int menuId = 2;
        int pageId = 1;
        var sql = repository
            .From(f => f.From<Menu>()
                .Where(t => t.Id >= menuId)
                .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId })
                .AsCteTable("MenuList"))
            .InnerJoin<Page>((a, b) => a.Id == b.Id)
            .Where((x, y) => y.Id >= pageId)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out var dbParameters);

        Assert.Equal(@"WITH [MenuList]([Id],[Name],[ParentId],[PageId]) AS 
(
SELECT a.[Id],a.[Name],a.[ParentId],a.[PageId] FROM [sys_menu] a WHERE a.[Id]>=@p0
)
SELECT a.[Id],a.[Name],a.[ParentId],b.[Url] FROM [MenuList] a INNER JOIN [sys_page] b ON a.[Id]=b.[Id] WHERE b.[Id]>=@p1", sql);
        Assert.Equal(2, dbParameters.Count);
        Assert.True((int)dbParameters[0].Value == menuId);
        Assert.True((int)dbParameters[1].Value == pageId);

        var result = await repository
            .From(f => f.From<Menu>()
                .Where(t => t.Id >= menuId)
                .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId })
                .AsCteTable("MenuList"))
            .InnerJoin<Page>((a, b) => a.Id == b.Id)
            .Where((x, y) => y.Id >= pageId)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToListAsync();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public async Task Query_WithNextCte()
    {
        this.Initialize();
        int rootId = 1;
        using var repository = dbFactory.Create();
        var myCteTable1 = repository
            .From<Menu>()
                .Where(x => x.Id == rootId)
                .Select(x => new { x.Id, x.Name, x.ParentId })
            .UnionAllRecursive((x, self) => x.From<Menu>()
                .InnerJoin(self, (a, b) => a.ParentId == b.Id)
                .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
            .AsCteTable("myCteTable1");
        var myCteTable2 = repository
            .From<Page, Menu>()
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { y.Id, y.ParentId, x.Url })
            .UnionAll(x => x.From<Menu>()
                .InnerJoin<Page>((a, b) => a.PageId == b.Id)
                .Select((x, y) => new { x.Id, x.ParentId, y.Url }))
            .AsCteTable("myCteTable2");

        var sql = repository
            .From(myCteTable1)
            .InnerJoin(myCteTable2, (a, b) => a.Id == b.Id)
            .Select((a, b) => new { b.Id, a.Name, b.ParentId, b.Url })
            .ToSql(out _);

        Assert.Equal(@"WITH [myCteTable1]([Id],[Name],[ParentId]) AS 
(
SELECT a.[Id],a.[Name],a.[ParentId] FROM [sys_menu] a WHERE a.[Id]=@p0 UNION ALL
SELECT a.[Id],a.[Name],a.[ParentId] FROM [sys_menu] a INNER JOIN [myCteTable1] b ON a.[ParentId]=b.[Id]
),
[myCteTable2]([Id],[ParentId],[Url]) AS 
(
SELECT b.[Id],b.[ParentId],a.[Url] FROM [sys_page] a,[sys_menu] b WHERE a.[Id]=b.[PageId] UNION ALL
SELECT a.[Id],a.[ParentId],b.[Url] FROM [sys_menu] a INNER JOIN [sys_page] b ON a.[PageId]=b.[Id]
)
SELECT b.[Id],a.[Name],b.[ParentId],b.[Url] FROM [myCteTable1] a INNER JOIN [myCteTable2] b ON a.[Id]=b.[Id]", sql);

        var menuList = repository
            .From<Menu>()
                .Where(x => x.Id == rootId.ToParameter("@RootId"))
                .Select(x => new { x.Id, x.Name, x.ParentId })
            .UnionAllRecursive((x, y) => x.From<Menu>()
                .InnerJoin(y, (a, b) => a.ParentId == b.Id)
                .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
            .AsCteTable("MenuList");

        var result1 = repository
            .From(myCteTable2)
            .InnerJoin(myCteTable1, (a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, b.Name, a.ParentId, a.Url })
            .ToList();
        Assert.NotNull(result1);
        Assert.True(result1.Count > 0);

        int pageId = 1;
        sql = repository
            .From(menuList)
            .WithTable(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()//.WithTable(self)
                    .InnerJoin(menuList, (a, b) => a.Id == b.Id)
                    .Where((a, b) => a.Id > pageId)
                    .Select((x, y) => new { y.Id, x.Url })))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out _);
        Assert.Equal(@"WITH [MenuList]([Id],[Name],[ParentId]) AS 
(
SELECT a.[Id],a.[Name],a.[ParentId] FROM [sys_menu] a WHERE a.[Id]=@RootId UNION ALL
SELECT a.[Id],a.[Name],a.[ParentId] FROM [sys_menu] a INNER JOIN [MenuList] b ON a.[ParentId]=b.[Id]
)
SELECT a.[Id],a.[Name],a.[ParentId],b.[Url] FROM [MenuList] a INNER JOIN (SELECT b.[Id],a.[Url] FROM [sys_page] a INNER JOIN [sys_menu] b ON a.[Id]=b.[PageId] WHERE a.[Id]=@p1 UNION ALL
SELECT b.[Id],a.[Url] FROM [sys_page] a INNER JOIN [MenuList] b ON a.[Id]=b.[Id] WHERE a.[Id]>@p2) b ON a.[Id]=b.[Id]", sql);

        var result2 = repository
            .From(menuList)
            .WithTable(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()//.WithTable(self)
                    .InnerJoin(menuList, (a, b) => a.Id == b.Id)
                    .Where((a, b) => a.Id > pageId)
                    .Select((x, y) => new { y.Id, x.Url })))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToList();
        Assert.NotNull(result2);
        Assert.True(result2.Count > 0);

        sql = repository
            .From(menuList)
            .WithTable(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()//.WithTable(self)
                    .InnerJoin(menuList, (a, b) => a.Id == b.Id)
                    .Where((a, b) => a.Id > pageId)
                    .Select((x, y) => new { y.Id, x.Url }))
                .AsCteTable("MenuPageList"))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out _);
        Assert.Equal(@"WITH [MenuList]([Id],[Name],[ParentId]) AS 
(
SELECT a.[Id],a.[Name],a.[ParentId] FROM [sys_menu] a WHERE a.[Id]=@RootId UNION ALL
SELECT a.[Id],a.[Name],a.[ParentId] FROM [sys_menu] a INNER JOIN [MenuList] b ON a.[ParentId]=b.[Id]
),
[MenuPageList]([Id],[Url]) AS 
(
SELECT b.[Id],a.[Url] FROM [sys_page] a INNER JOIN [sys_menu] b ON a.[Id]=b.[PageId] WHERE a.[Id]=@p1 UNION ALL
SELECT b.[Id],a.[Url] FROM [sys_page] a INNER JOIN [MenuList] b ON a.[Id]=b.[Id] WHERE a.[Id]>@p2
)
SELECT a.[Id],a.[Name],a.[ParentId],b.[Url] FROM [MenuList] a INNER JOIN [MenuPageList] b ON a.[Id]=b.[Id]", sql);
        var result3 = await repository
            .From(menuList)
            .WithTable(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()
                    .InnerJoin(menuList, (a, b) => a.Id == b.Id)
                    .Where((a, b) => a.Id > pageId)
                    .Select((x, y) => new { y.Id, x.Url }))
                .AsCteTable("MenuPageList"))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToListAsync();
        Assert.NotNull(result3);
        Assert.True(result3.Count > 0);
    }
    [Fact]
    public async Task Query_WithTable()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<Menu>()
            .WithTable(f => f.From<Page, Menu>('c')
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
            .Where((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out _);
        Assert.Equal(@"SELECT a.[Id],a.[Name],a.[ParentId],b.[Url] FROM [sys_menu] a,(SELECT d.[Id],d.[ParentId],c.[Url] FROM [sys_page] c,[sys_menu] d WHERE c.[Id]=d.[PageId]) b WHERE a.[Id]=b.[Id]", sql);

        var result = repository.From<Menu>()
            .WithTable(f => f.From<Page, Menu>('c')
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
            .Where((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToList();
        Assert.True(result.Count > 0);

        int menuId = 1;
        int pageId = 1;
        int pageId2 = 1;
        var sql1 = repository
            .From(f => f.From<Menu>()
                    .Where(x => x.Id == menuId)
                    .Select(x => new { x.Id, x.Name, x.ParentId })
                .UnionAllRecursive((x, y) => x.From<Menu>()
                    .InnerJoin(y, (a, b) => a.ParentId == b.Id)
                    .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
                .AsCteTable("myCteTable1"))
            .WithTable(f => f.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id > pageId2)
                    .Select((x, y) => new { y.Id, x.Url }))
                .AsCteTable("myCteTable2"))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out var dbParameters);
        Assert.Equal(@"WITH [myCteTable1]([Id],[Name],[ParentId]) AS 
(
SELECT a.[Id],a.[Name],a.[ParentId] FROM [sys_menu] a WHERE a.[Id]=@p0 UNION ALL
SELECT a.[Id],a.[Name],a.[ParentId] FROM [sys_menu] a INNER JOIN [myCteTable1] b ON a.[ParentId]=b.[Id]
),
[myCteTable2]([Id],[Url]) AS 
(
SELECT b.[Id],a.[Url] FROM [sys_page] a INNER JOIN [sys_menu] b ON a.[Id]=b.[PageId] WHERE a.[Id]=@p1 UNION ALL
SELECT b.[Id],a.[Url] FROM [sys_page] a INNER JOIN [sys_menu] b ON a.[Id]=b.[PageId] WHERE a.[Id]>@p2
)
SELECT a.[Id],a.[Name],a.[ParentId],b.[Url] FROM [myCteTable1] a INNER JOIN [myCteTable2] b ON a.[Id]=b.[Id]", sql1);

        var result1 = await repository
            .From(f => f.From<Menu>()
                    .Where(x => x.Id == menuId)
                    .Select(x => new { x.Id, x.Name, x.ParentId })
                .UnionAllRecursive((x, y) => x.From<Menu>()
                    .InnerJoin(y, (a, b) => a.ParentId == b.Id)
                    .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
                .AsCteTable("myCteTable1"))
            .WithTable(f => f.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id > pageId2)
                    .Select((x, y) => new { y.Id, x.Url }))
                .AsCteTable("myCteTable2"))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToListAsync();
        Assert.NotNull(result1);
        Assert.True(result1.Count > 0);

        var sql2 = repository.From<Order, OrderDetail>()
            .InnerJoin((x, y) => x.Id == y.OrderId)
            .Include((x, y) => x.Buyer)
            .Where((a, b) => a.Id == b.OrderId)
            .Select((a, b) => new { Order = a, a.BuyerId, DetailId = b.Id, b.Price, b.Quantity, b.Amount })
            .ToSql(out _);
        Assert.Equal("SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],c.[Id],c.[TenantId],c.[Name],c.[Gender],c.[Age],c.[CompanyId],c.[GuidField],c.[SomeTimes],c.[SourceType],c.[IsEnabled],c.[CreatedAt],c.[CreatedBy],c.[UpdatedAt],c.[UpdatedBy],a.[BuyerId],b.[Id] AS [DetailId],b.[Price],b.[Quantity],b.[Amount] FROM [sys_order] a INNER JOIN [sys_order_detail] b ON a.[Id]=b.[OrderId] LEFT JOIN [sys_user] c ON a.[BuyerId]=c.[Id] WHERE a.[Id]=b.[OrderId]", sql2);

        var result2 = repository.From<Order, OrderDetail>()
            .InnerJoin((x, y) => x.Id == y.OrderId)
            .Include((x, y) => x.Buyer)
            .Where((a, b) => a.Id == b.OrderId)
            .Select((a, b) => new { Order = a, a.BuyerId, DetailId = b.Id, b.Price, b.Quantity, b.Amount })
            .ToList();
        Assert.True(result2.Count > 0);
        Assert.NotNull(result2[0].Order);
        Assert.NotNull(result2[0].Order.Buyer);

        var sql3 = repository.From(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .InnerJoin<Order>((a, b, c) => a.OrderId == c.Id)
            .Select((a, b, c) => new { a.OrderId, a.BuyerId, Buyer = b, Order = c, a.TotalAmount })
            .ToSql(out _);
        Assert.Equal("SELECT a.[OrderId],a.[BuyerId],b.[Id],b.[TenantId],b.[Name],b.[Gender],b.[Age],b.[CompanyId],b.[GuidField],b.[SomeTimes],b.[SourceType],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy],c.[Id],c.[TenantId],c.[OrderNo],c.[ProductCount],c.[TotalAmount],c.[BuyerId],c.[BuyerSource],c.[SellerId],c.[Products],c.[Disputes],c.[IsEnabled],c.[CreatedAt],c.[CreatedBy],c.[UpdatedAt],c.[UpdatedBy],a.[TotalAmount] FROM (SELECT a.[Id] AS [OrderId],a.[BuyerId],SUM(b.[Amount]) AS [TotalAmount] FROM [sys_order] a,[sys_order_detail] b,[sys_user] c WHERE a.[Id]=b.[OrderId] AND a.[BuyerId]=c.[Id] AND c.[Age]>20 GROUP BY a.[Id],a.[BuyerId] HAVING SUM(b.[Amount])>500) a INNER JOIN [sys_user] b ON a.[BuyerId]=b.[Id] INNER JOIN [sys_order] c ON a.[OrderId]=c.[Id]", sql3);

        var result3 = repository.From(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .InnerJoin<Order>((a, b, c) => a.OrderId == c.Id)
            .Select((a, b, c) => new { a.OrderId, a.BuyerId, Buyer = b, Order = c, a.TotalAmount })
            .ToList();
        Assert.True(result3.Count > 0);
    }
    [Fact]
    public void SelectFlattenTo()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Order>("8");
        repository.Create<Order>(new Order
        {
            Id = "8",
            TenantId = "2",
            OrderNo = "On-ZwYx",
            BuyerId = 1,
            SellerId = 2,
            TotalAmount = 500,
            Products = new List<int> { 1, 2 },
            IsEnabled = true,
            CreatedAt = DateTime.Now,
            CreatedBy = 1,
            UpdatedAt = DateTime.Now,
            UpdatedBy = 1
        });
        repository.Commit();

        var result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectFlattenTo<OrderInfo>()
            .ToList();
        Assert.Equal("8", result[0].Id);
        Assert.Equal(1, result[0].BuyerId);
        Assert.Equal("On-ZwYx", result[0].OrderNo);
        Assert.Null(result[0].Description);

        result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectFlattenTo(f => new OrderInfo
            {
                Description = "TotalAmount:" + f.TotalAmount
            })
            .ToList();
        Assert.Equal("8", result[0].Id);
        Assert.Equal(1, result[0].BuyerId);
        Assert.Equal("On-ZwYx", result[0].OrderNo);
        Assert.NotNull(result[0].Description);
        Assert.Equal("TotalAmount:500", result[0].Description);

        result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectFlattenTo(f => new OrderInfo
            {
                Description = this.DeferInvoke().Deferred()
            })
            .ToList();
        Assert.Equal("8", result[0].Id);
        Assert.Equal(1, result[0].BuyerId);
        Assert.Equal("On-ZwYx", result[0].OrderNo);
        Assert.NotNull(result[0].Description);
        Assert.True(result[0].Description == this.DeferInvoke());

        var result1 = repository.From(f =>
               f.From<Order, OrderDetail>('a')
                .Where((a, b) => a.Id == b.OrderId)
                .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                .Select((x, a, b) => new { x.Grouping.BuyerId, x.Grouping.OrderId, ProductTotal = Sql.CountDistinct(b.ProductId) }))
           .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
           .SelectFlattenTo((x, y) => new OrderBuyerInfo { BuyerName = y.Name })
           .First();
        if (result1 != null)
        {
            Assert.NotNull(result1);
            Assert.False(string.IsNullOrEmpty(result1.OrderId));
            Assert.True(result1.BuyerId > 0);
            Assert.Null(result1.OrderNo);
            Assert.NotNull(result1.BuyerName);
        }
    }
    [Fact]
    public void SelectAfterOrderBy()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .LeftJoin<OrderDetail>((a, b, c) => a.Id == c.OrderId)
            .GroupBy((a, b, c) => new { a.BuyerId, OrderId = a.Id, a.CreatedAt, BuyerName = b.Name, BuyerAge = b.Age })
            .Select((x, a, b, c) => new
            {
                x.Grouping.BuyerId,
                x.Grouping.OrderId,
                x.Grouping.BuyerName,
                x.Grouping.BuyerAge,
                ProductCount = x.CountDistinct(c.ProductId),
                LastBuyAt = x.Max(b.CreatedAt).IsNull(a.CreatedAt)
            })
            .OrderByDescending(f => f.LastBuyAt)
            .ToSql(out _);
        Assert.Equal("SELECT a.[BuyerId],a.[Id] AS [OrderId],b.[Name] AS [BuyerName],b.[Age] AS [BuyerAge],COUNT(DISTINCT c.[ProductId]) AS [ProductCount],ISNULL(MAX(b.[CreatedAt]),a.[CreatedAt]) AS [LastBuyAt] FROM [sys_order] a INNER JOIN [sys_user] b ON a.[BuyerId]=b.[Id] LEFT JOIN [sys_order_detail] c ON a.[Id]=c.[OrderId] GROUP BY a.[BuyerId],a.[Id],a.[CreatedAt],b.[Name],b.[Age] ORDER BY ISNULL(MAX(b.[CreatedAt]),a.[CreatedAt]) DESC", sql);

        var result = repository.From<Order>()
           .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
           .LeftJoin<OrderDetail>((a, b, c) => a.Id == c.OrderId)
           .GroupBy((a, b, c) => new { a.BuyerId, OrderId = a.Id, a.CreatedAt, BuyerName = b.Name, BuyerAge = b.Age })
           .Select((x, a, b, c) => new
           {
               x.Grouping.BuyerId,
               x.Grouping.OrderId,
               x.Grouping.BuyerName,
               x.Grouping.BuyerAge,
               ProductCount = x.CountDistinct(c.ProductId),
               LastBuyAt = x.Max(b.CreatedAt).IsNull(a.CreatedAt)
           })
           .OrderByDescending(f => f.LastBuyAt)
           .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
        if (result.Count > 1)
        {
            Assert.True(result[0].LastBuyAt >= result[1].LastBuyAt);
        }
    }
    private string DeferInvoke() => "DeferInvoke";
}
