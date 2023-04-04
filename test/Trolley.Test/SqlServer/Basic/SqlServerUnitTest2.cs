using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Trolley.Test.SqlServer;

public class SqlServerUnitTest2 : UnitTestBase
{
    public SqlServerUnitTest2()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register("fengling", true, f =>
            {
                var connectionString = "Server=.;Database=fengling;Uid=sa;password=Angangyur123456;TrustServerCertificate=true";
                f.Add<SqlServerProvider>(connectionString, true);
            })
            .AddTypeHandler<JsonTypeHandler>()
            .Configure<SqlServerProvider, SqlServerModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async void QueryFirst()
    {
        //this.Initialize();
        using var repository = dbFactory.Create();
        var sss = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => f.SomeTimes)
            .First();
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
            Assert.True(result1.Id == 1);
        }
    }
    [Fact]
    public async void Get()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.Get<User>(1);
        Assert.True(result.Name == "leafkevin");
        var user = await repository.GetAsync<User>(new { Id = 1 });
        Assert.True(user.Name == result.Name);
    }
    [Fact]
    public async void Query()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = await repository.QueryAsync<Product>(f => f.ProductNo.Contains("PN-00"));
        Assert.True(result.Count >= 3);
    }
    [Fact]
    public async void QueryPage()
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
        Assert.NotEmpty(result.Items);
        Assert.True(result.RecordsTotal == count);
        Assert.True(result.Items.Count == 1);
    }
    [Fact]
    public async void QueryDictionary()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = await repository.QueryDictionaryAsync<Product, int, string>(f => f.ProductNo.Contains("PN-00"), f => f.Id, f => f.Name);
        Assert.True(result.Count >= 3);
    }
    [Fact]
    public async void QueryRawSql()
    {
        using var repository = dbFactory.Create();
        var result = await repository.QueryAsync<Product>("SELECT * FROM sys_product where Id=@ProductId", new { ProductId = 1 });
        Assert.NotNull(result);
        Assert.True(result.Count == 1);
    }
    [Fact]
    public void FromQuery_SubQuery()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository
            .From(f => f.From<Order>()
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .GroupBy((a, b) => new { OrderId = a.Id, a.BuyerId })
                .Select((x, a, b) => new { x.Grouping, ProductCount = x.CountDistinct(b.ProductId) }))
            .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Grouping,
                Buyer = y,
                x.ProductCount
            })
            .ToList();
        if (result.Count > 0)
        {
            Assert.NotNull(result[0]);
            Assert.NotNull(result[0].Grouping);
            Assert.NotNull(result[0].Buyer);
            Assert.True(result[0].ProductCount > 1);
        }
    }
    [Fact]
    public void FromQuery_InnerJoin()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.From<User>()
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
    public async void FromQuery_InnerJoin1()
    {
        using var repository = dbFactory.Create();
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
    public async void FromQuery_Include()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = await repository.From<Product>(suffixRawSql: "WITH(NOLOCK)")
            .Include(f => f.Brand)
            .Where(f => f.ProductNo.Contains("PN-00"))
            .ToListAsync();

        if (result.Count > 0)
        {
            Assert.NotNull(result[0].Brand);
            Assert.True(result[0].Brand.BrandNo == "BN-001");
        }
        if (result.Count > 1)
        {
            Assert.NotNull(result[1].Brand);
            Assert.True(result[1].Brand.BrandNo == "BN-002");
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
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new int[] { 1, 2, 3 }))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.True(result.Count == 2);
        Assert.NotNull(result[0].Order);
        Assert.NotNull(result[0].Order.Details);
        Assert.NotEmpty(result[0].Order.Details);
        Assert.True(result[0].Order.Details.Count == 3);
    }
    [Fact]
    public void FromQuery_IncludeMany_Filter()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.From<Order>(suffixRawSql: "WITH(NOLOCK)")
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details, f => f.ProductId == 1)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new int[] { 1, 2, 3 }))
            .Select((x, y) => new { Order = x, Buyer = y, Test = x.OrderNo + "_" + y.Age % 4 })
            .ToList();

        Assert.True(result.Count == 2);
        Assert.NotNull(result[0].Order);
        Assert.NotNull(result[0].Order.Details);
        Assert.NotEmpty(result[0].Order.Details);
        Assert.True(result[0].Order.Details.Count == 1);
        Assert.True(result[0].Order.Details[0].ProductId == 1);
        Assert.True(result[1].Order.Details.Count == 1);
        Assert.True(result[1].Order.Details[0].ProductId == 1);
    }
    [Fact]
    public async void FromQuery_Include_ThenInclude()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = await repository.From<Order>(suffixRawSql: "WITH(NOLOCK)")
            .InnerJoin<User>((a, b) => a.SellerId == b.Id)
            .Include((x, y) => x.Buyer)
            .ThenInclude(f => f.Company)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new int[] { 1, 2, 3 }))
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
    //public async void FromQuery_IncludeMany_ThenInclude()
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
        Assert.NotEmpty(result.Items);
        Assert.True(result.RecordsTotal == count);
        Assert.True(result.Items.Count == 1);
        Assert.NotNull(result.Items[0].Product);
        Assert.True(result.Items[0].Product.Id == 1);
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
        Assert.True(sql == "SELECT a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) AS Date,COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) ORDER BY a.[Id],CONVERT(CHAR(10),b.[CreatedAt],120)");
    }
    [Fact]
    public void FromQuery_Groupby()
    {
        Initialize();
        using var repository = dbFactory.Create();
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
        if (result.Count > 0)
        {
            Assert.NotNull(result[0].Grouping);
            Assert.NotNull(result[0].Grouping.Name);
        }
        if (result.Count > 1)
        {
            Assert.NotNull(result[1].Grouping);
            Assert.NotNull(result[1].Grouping.Name);
        }
    }
    [Fact]
    public void FromQuery_Groupby_Fields()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id })
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.True(result.Count >= 2);
        Assert.NotNull(result[0].Name);
        Assert.NotNull(result[1].Name);
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
        Assert.True(sql == "SELECT a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) AS Date,COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) ORDER BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120)");
    }
    [Fact]
    public void FromQuery_Groupby_OrderBy_Fields()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
           .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
           .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
           .OrderBy((x, a, b) => x.Grouping.Id)
           .OrderByDescending((x, a, b) => x.Grouping.Name)
           .OrderBy((x, a, b) => x.Grouping.Date)
           .Select((x, a, b) => new
           {
               x.Grouping,
               OrderCount = x.Count(b.Id),
               TotalAmount = x.Sum(b.TotalAmount)
           })
           .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) AS Date,COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) ORDER BY a.[Id],a.[Name] DESC,CONVERT(CHAR(10),b.[CreatedAt],120)");
    }
    [Fact]
    public void FromQuery_Groupby_Having()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
                .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
                .InnerJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                .GroupBy((a, b, c) => new { a.Id, a.Name, b.CreatedAt.Date })
                .Having((x, a, b, c) => x.Sum(b.TotalAmount) > 300 && x.CountDistinct(c.ProductId) > 2)
                .OrderBy((x, a, b, c) => new { x.Grouping })
                .Select((x, a, b, c) => new
                {
                    x.Grouping.Id,
                    x.Grouping.Name,
                    x.Grouping.Date,
                    OrderCount = x.Count(b.Id),
                    TotalAmount = x.Sum(b.TotalAmount)
                })
                .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) AS Date,COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] INNER JOIN [sys_order_detail] c ON b.[Id]=c.[OrderId] GROUP BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) HAVING SUM(b.[TotalAmount])>300 AND COUNT(DISTINCT c.[ProductId])>2 ORDER BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120)");
    }
    [Fact]
    public void FromQuery_Groupby_Having_OrderBy_Fields()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
                .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
                .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
                .Having((x, a, b) => x.Sum(b.TotalAmount) > 300 && Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && x.CountDistinct(f.ProductId) > 2))
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
        Assert.True(sql == "SELECT a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) AS Date,COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) HAVING SUM(b.[TotalAmount])>300 AND EXISTS(SELECT * FROM [sys_order_detail] f WHERE b.[Id]=f.[OrderId] AND COUNT(DISTINCT f.[ProductId])>2) ORDER BY a.[Id],a.[Name] DESC,CONVERT(CHAR(10),b.[CreatedAt],120)");
    }
    [Fact]
    public void FromQuery_In_Exists()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
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
        Assert.True(sql == "SELECT a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) AS Date,COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] WHERE a.[Id] IN (1,2,3) AND EXISTS(SELECT * FROM [sys_order_detail] f WHERE b.[Id]=f.[OrderId] AND f.[ProductId]=2) GROUP BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) HAVING SUM(b.[TotalAmount])>300 ORDER BY a.[Id],CONVERT(CHAR(10),b.[CreatedAt],120)");
    }
    [Fact]
    public void FromQuery_In1()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order, OrderDetail>('b')
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id] FROM [sys_user] a WHERE a.[Id] IN (SELECT b.[BuyerId] FROM [sys_order] b,[sys_order_detail] c WHERE b.[Id]=c.[OrderId] AND c.[ProductId]=1)");
    }
    [Fact]
    public void FromQuery_In_Exists1()
    {
        using var repository = dbFactory.Create();
        bool? isMale = true;
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order, OrderDetail>('b')
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id] FROM [sys_user] a WHERE a.[Id] IN (SELECT b.[BuyerId] FROM [sys_order] b,[sys_order_detail] c WHERE b.[Id]=c.[OrderId] AND c.[ProductId]=1) AND EXISTS(SELECT * FROM [sys_order] x,[sys_company] y WHERE a.[Id]=x.[SellerId] AND a.[CompanyId]=y.[Id])");
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
        Assert.True(sql == "SELECT a.[Gender],a.[Age],COUNT(DISTINCT a.[CompanyId]) AS CompanyCount,COUNT(a.[Id]) AS UserCount FROM [sys_user] a WHERE a.[Id] IN (SELECT c.[BuyerId] FROM [sys_order_detail] b INNER JOIN [sys_order] c ON b.[OrderId]=c.[Id] AND b.[ProductId]=1) AND EXISTS(SELECT * FROM [sys_company] x,[sys_order] y WHERE a.[Id]=y.[SellerId] AND a.[CompanyId]=x.[Id]) GROUP BY a.[Gender],a.[Age]");
    }
    [Fact]
    public void FromQuery_SelectAggregate()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .IncludeMany((a, b) => a.Orders)
            .OrderBy((a, b) => new { UserId = a.Id, OrderId = b.Id })
            .SelectAggregate((x, a, b) => new
            {
                UserId = a.Id,
                OrderId = b.Id,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id] AS UserId,b.[Id] AS OrderId,COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] ORDER BY a.[Id],b.[Id]");
    }
    [Fact]
    public void FromQuery_Select_Sql_Aggregate()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .IncludeMany((a, b) => a.Orders)
            .OrderBy((a, b) => new { UserId = a.Id, OrderId = b.Id })
            .Select((a, b) => new
            {
                UserId = a.Id,
                OrderId = b.Id,
                OrderCount = Sql.Count(b.Id),
                TotalAmount = Sql.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id] AS UserId,b.[Id] AS OrderId,COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] ORDER BY a.[Id],b.[Id]");
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
        var result = repository.Query<(int OrderId, string OrderNo, double TotalAmount)>(sql);
        Assert.NotNull(result);
    }
    [Fact]
    public void Query_Json()
    {
        using var repository = dbFactory.Create();
        var result = repository.From<Order>().First();
        Assert.NotNull(result);
        Assert.NotNull(result.Products);
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
        Assert.True(sql == "SELECT ([OrderNo] IS NULL) AS NoOrderNo,([ProductCount] IS NOT NULL) AS HasProduct FROM [sys_order] WHERE [ProductCount] IS NULL AND [ProductCount] IS NULL");
    }
    [Fact]
    public async void Query_Union()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .Where(x => x.Id == 1)
            .Select(x => new
            {
                x.Id,
                x.OrderNo,
                x.SellerId,
                x.BuyerId
            })
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id > 1)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                }))
            .ToSql(out _);
        Assert.True(sql == @"SELECT [Id],[OrderNo],[SellerId],[BuyerId] FROM [sys_order] WHERE [Id]=1 UNION ALL
SELECT [Id],[OrderNo],[SellerId],[BuyerId] FROM [sys_order] WHERE [Id]>1");

        var result = await repository.From<Order>()
           .Where(x => x.Id == 1)
           .Select(x => new
           {
               x.Id,
               x.OrderNo,
               x.SellerId,
               x.BuyerId
           })
           .UnionAll(f => f.From<Order>()
               .Where(x => x.Id > 1)
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
    public async void Query_WithCte()
    {
        using var repository = dbFactory.Create();
        var sql = repository
            .FromWithRecursive((f, cte) => f.From<Menu>()
                    .Where(x => x.Id == 1)
                    .Select(x => new { x.Id, x.Name, x.ParentId })
                .UnionAllRecursive((x, y) => x.From<Menu>()
                    .InnerJoinRecursive(y, cte, (a, b) => a.ParentId == b.Id)
                    .Select((a, b) => new { a.Id, a.Name, a.ParentId })), "MenuList")
            .NextWithRecursive((f, cte) => f.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == 1)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id > 1)
                    .Select((x, y) => new { y.Id, x.Url })), "MenuPageList")
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out _);

        Assert.True(sql == @"WITH MenuList(Id,Name,ParentId) AS 
(
SELECT [Id],[Name],[ParentId] FROM [sys_menu] WHERE [Id]=1 UNION ALL
SELECT a.[Id],a.[Name],a.[ParentId] FROM [sys_menu] a INNER JOIN MenuList b ON a.[ParentId]=b.[Id]
),
MenuPageList(Id,Url) AS 
(
SELECT b.[Id],a.[Url] FROM [sys_page] a INNER JOIN [sys_menu] b ON a.[Id]=b.[PageId] WHERE a.[Id]=1 UNION ALL
SELECT b.[Id],a.[Url] FROM [sys_page] a INNER JOIN [sys_menu] b ON a.[Id]=b.[PageId] WHERE a.[Id]>1
)
SELECT a.[Id],a.[Name],a.[ParentId],b.[Url] FROM MenuList a INNER JOIN MenuPageList b ON a.[Id]=b.[Id]");

        var result = await repository.FromWithRecursive((f, cte) => f.From<Menu>()
                    .Where(x => x.Id == 1)
                    .Select(x => new { x.Id, x.Name, x.ParentId })
                .UnionAllRecursive((x, y) => x.From<Menu>()
                    .InnerJoinRecursive(y, cte, (a, b) => a.ParentId == b.Id)
                    .Select((a, b) => new { a.Id, a.Name, a.ParentId })), "MenuList")
            .NextWithRecursive((f, cte) => f.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == 1)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id > 1)
                    .Select((x, y) => new { y.Id, x.Url })), "MenuPageList")
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
           .ToListAsync();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
}
