using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Trolley.Tests;

public class SqlServerUnitTest2
{
    private readonly IOrmDbFactory dbFactory;
    public SqlServerUnitTest2()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var connectionString = "Server=.;Database=fengling;Uid=sa;password=Angangyur123456;TrustServerCertificate=true";
            var ormProvider = f.GetService<IOrmProvider>();
            var builder = new OrmDbFactoryBuilder();
            builder.Register("fengling", true, f => f.Add<SqlServerProvider>(connectionString, true))
                .Configure(f => new ModelConfiguration().OnModelCreating(f));
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();

        using var repository = this.dbFactory.Create();
        repository.Delete<User>(new[] { 1, 2 });
        repository.Create<User>(new[]
        {
            new User
            {
                Id = 1,
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new User
            {
                Id = 2,
                Name = "cindy",
                Age = 21,
                CompanyId = 2,
                Gender = Gender.Male,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<Company>(new[] { 1, 2 });
        repository.Create<Company>(new[]
        {
            new Company
            {
                Id = 1,
                Name = "微软",
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Company
            {
                Id = 2,
                Name = "谷歌",
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<Brand>(new[] { 1, 2, 3 });
        repository.Create<Brand>(new[]
        {
            new Brand
            {
                Id = 1,
                BrandNo = "BN-001",
                Name = "波司登",
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Brand
            {
                Id = 2,
                BrandNo = "BN-002",
                Name = "雪中飞",
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Brand
            {
                Id = 3,
                BrandNo = "BN-003",
                Name = "优衣库",
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<Product>(new[] { 1, 2, 3 });
        repository.Create<Product>(new[]
        {
            new Product
            {
                Id = 1,
                ProductNo="PN-001",
                Name = "波司登羽绒服",
                Price =550,
                BrandId = 1,
                CategoryId = 1,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Product
            {
                Id = 2,
                ProductNo="PN-002",
                Name = "雪中飞羽绒裤",
                Price =350,
                BrandId = 2,
                CategoryId = 2,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Product
            {
                Id = 3,
                ProductNo="PN-003",
                Name = "优衣库保暖内衣",
                Price =180,
                BrandId = 3,
                CategoryId = 3,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<Order>(new[] { 1, 2, 3 });
        repository.Create<Order>(new[]
        {
            new Order
            {
                Id = 1,
                OrderNo = "ON-001",
                BuyerId = 1,
                SellerId = 2,
                TotalAmount = 500,
                //Products = new  List<int>{1, 2},
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Order
            {
                Id = 2,
                OrderNo = "ON-002",
                BuyerId = 2,
                SellerId = 1,
                TotalAmount = 350,
                //Products = new  List<int>{1, 3},
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Order
            {
                Id = 3,
                OrderNo = "ON-003",
                BuyerId = 1,
                SellerId = 2,
                TotalAmount = 199,
                //Products = new  List<int>{2},
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<OrderDetail>(new[] { 1, 2, 3, 4, 5, 6 });
        repository.Create<OrderDetail>(new[]
        {
            new OrderDetail
            {
                Id = 1,
                OrderId = 1,
                ProductId = 1,
                Price = 299,
                Quantity = 1,
                Amount = 299,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = 2,
                OrderId = 1,
                ProductId = 2,
                Price = 159,
                Quantity = 1,
                Amount = 159,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = 3,
                OrderId = 1,
                ProductId = 3,
                Price = 69,
                Quantity = 1,
                Amount = 69,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = 4,
                OrderId = 2,
                ProductId = 1,
                Price = 299,
                Quantity = 1,
                Amount = 299,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = 5,
                OrderId = 2,
                ProductId = 3,
                Price = 69,
                Quantity = 1,
                Amount = 69,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = 6,
                OrderId = 3,
                ProductId = 2,
                Price = 199,
                Quantity = 1,
                Amount = 199,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });
    }
    [Fact]
    public async void QueryFirst()
    {
        using var repository = this.dbFactory.Create();
        var result = repository.QueryFirst<User>(f => f.Id == 1);
        Assert.True(result.Name == "leafkevin");
        var user = repository.QueryFirst<User>("SELECT * FROM sys_user where Id=1");
        Assert.True(user.Name == "leafkevin");
        var result1 = await repository.QueryFirstAsync<User>(f => f.Name == "leafkevin");
        var result2 = await repository.QueryFirstAsync<User>(new { Name = "leafkevin" });
        Assert.True(result1.Id == result2.Id);
        Assert.True(result1.Id == 1);
    }
    [Fact]
    public async void Get()
    {
        using var repository = this.dbFactory.Create();
        var result = repository.Get<User>(1);
        Assert.True(result.Name == "leafkevin");
        var user = await repository.GetAsync<User>(new { Id = 1 });
        Assert.True(user.Name == result.Name);
    }
    [Fact]
    public async void Query()
    {
        using var repository = this.dbFactory.Create();
        var result = await repository.QueryAsync<Product>(f => f.ProductNo.Contains("PN-00"));
        Assert.True(result.Count >= 3);
    }
    [Fact]
    public async void QueryPage()
    {
        using var repository = this.dbFactory.Create();
        var result = await repository.QueryPageAsync<OrderDetail>(2, 1, f => f.ProductId == 1);
        var count = await repository.From<OrderDetail>().Where(f => f.ProductId == 1).CountAsync();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.True(result.RecordsTotal == count);
        Assert.True(result.Items.Count == 1);
    }
    [Fact]
    public async void QueryDictionary()
    {
        using var repository = this.dbFactory.Create();
        var result = await repository.QueryDictionaryAsync<Product, int, string>(f => f.ProductNo.Contains("PN-00"), f => f.Id, f => f.Name);
        Assert.True(result.Count >= 3);
    }
    [Fact]
    public async void FromQuery_Include()
    {
        using var repository = this.dbFactory.Create();
        var result = await repository.From<Product>()
            .Include(f => f.Brand)
            .Where(f => f.ProductNo.Contains("PN-00"))
            .ToListAsync();
        Assert.True(result.Count >= 3);
        Assert.NotNull(result[0].Brand);
        Assert.True(result[0].Brand.BrandNo == "BN-001");
        Assert.True(result[1].Brand.BrandNo == "BN-002");
    }
    [Fact]
    public void FromQuery_IncludeMany()
    {
        using var repository = this.dbFactory.Create();
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
        using var repository = this.dbFactory.Create();
        var result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details, f => f.ProductId == 1)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new int[] { 1, 2, 3 }))
            .Select((x, y) => new { Order = x, Buyer = y, Test = x.OrderNo + "_" + (y.Age % 4) })
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
        using var repository = this.dbFactory.Create();
        var result = await repository.From<Order>()
            .InnerJoin<User>((a, b) => a.SellerId == b.Id)
            .Include((x, y) => x.Buyer)
            .ThenInclude(f => f.Company)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new int[] { 1, 2, 3 }))
            .Select((x, y) => new { Order = x, Seller = y })
            .ToListAsync();
        Assert.True(result.Count == 2);
        Assert.NotNull(result[0].Order.Buyer);
        Assert.NotNull(result[0].Order.Buyer.Company);
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
        using var repository = this.dbFactory.Create();
        var result = repository.From<OrderDetail>()
            .Include(f => f.Product)
            .Where(f => f.ProductId == 1)
            .ToPageList(2, 1);
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
    public void FromQuery_Ignore_Include()
    {
        using var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .IncludeMany((a, b) => a.Orders)
            .ThenIncludeMany(f => f.Details)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.Id })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120),COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) ORDER BY a.[Id],b.[Id]");
    }
    [Fact]
    public void FromQuery_Groupby()
    {
        using var repository = this.dbFactory.Create();
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
        Assert.True(result.Count == 2);
        Assert.NotNull(result[0].Grouping);
        Assert.NotNull(result[1].Grouping);
        Assert.NotNull(result[0].Grouping.Name);
        Assert.NotNull(result[1].Grouping.Name);
    }
    [Fact]
    public void FromQuery_Groupby_Fields()
    {
        using var repository = this.dbFactory.Create();
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
        Assert.True(result.Count == 2);
        Assert.NotNull(result[0].Name);
        Assert.NotNull(result[1].Name);
    }
    [Fact]
    public void FromQuery_Groupby_OrderBy()
    {
        using var repository = this.dbFactory.Create();
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
        Assert.True(sql == "SELECT a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120),COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) ORDER BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120)");
    }
    [Fact]
    public void FromQuery_Groupby_OrderBy_Fields()
    {
        using var repository = this.dbFactory.Create();
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
        Assert.True(sql == "SELECT a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120),COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) ORDER BY a.[Id],a.[Name] DESC,CONVERT(CHAR(10),b.[CreatedAt],120)");
    }
    [Fact]
    public void FromQuery_Groupby_Having()
    {
        using var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
                .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
                .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
                .Having((x, a, b) => x.Sum(b.TotalAmount) > 300 && Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && x.CountDistinct(f.ProductId) > 2))
                .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.Id })
                .Select((x, a, b) => new
                {
                    x.Grouping.Id,
                    x.Grouping.Name,
                    x.Grouping.Date,
                    OrderCount = x.Count(b.Id),
                    TotalAmount = x.Sum(b.TotalAmount)
                })
                .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) AS Date,COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] GROUP BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) HAVING SUM(b.[TotalAmount])>300 AND EXISTS(SELECT * FROM [sys_order_detail] f WHERE b.[Id]=f.[OrderId] AND COUNT(DISTINCT f.[ProductId])>2) ORDER BY a.[Id],b.[Id]");
    }
    [Fact]
    public void FromQuery_Groupby_Having_OrderBy_Fields()
    {
        using var repository = this.dbFactory.Create();
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
        using var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, new int[] { 1, 2, 3 }))
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300 && Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && x.CountDistinct(f.ProductId) > 2))
            .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.Id })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120),COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId] WHERE [Id] IN (@p0,@p1,@p2) GROUP BY a.[Id],a.[Name],CONVERT(CHAR(10),b.[CreatedAt],120) HAVING SUM(b.[TotalAmount])>300 AND EXISTS(SELECT * FROM [sys_order_detail] f WHERE b.[Id]=f.[OrderId] AND COUNT(DISTINCT f.[ProductId])>2) ORDER BY a.[Id],b.[Id]");
    }
    [Fact]
    public void FromQuery_In1()
    {
        using var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order, OrderDetail>('b')
                .InnerJoin((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[IsEnabled],a.[CreatedBy],a.[CreatedAt],a.[UpdatedBy],a.[UpdatedAt] FROM [sys_user] a WHERE a.[Id] IN (SELECT b.[BuyerId] FROM [sys_order] b INNER JOIN [sys_order_detail] c ON b.[Id]=c.[OrderId] AND c.[ProductId]=1)");
    }
    [Fact]
    public void FromQuery_In_Exists1()
    {
        using var repository = this.dbFactory.Create();
        bool? isMale = true;
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order, OrderDetail>('b').InnerJoin((a, b) => a.Id == b.OrderId && b.ProductId == 1).Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id],a.[Name],a.[Gender],a.[Age],a.[CompanyId],a.[IsEnabled],a.[CreatedBy],a.[CreatedAt],a.[UpdatedBy],a.[UpdatedAt] FROM [sys_user] a WHERE a.[Id] IN (SELECT b.[BuyerId] FROM [sys_order] b INNER JOIN [sys_order_detail] c ON b.[Id]=c.[OrderId] AND c.[ProductId]=1) AND EXISTS(SELECT * FROM [sys_order] x,[sys_company] y WHERE a.[Id]=x.[SellerId] AND a.[CompanyId]=y.[Id])");
    }
    [Fact]
    public void FromQuery_In_Exists_Group_CountDistinct_Count()
    {
        using var repository = this.dbFactory.Create();
        bool? isMale = true;
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<OrderDetail, Order>('b').InnerJoin((a, b) => a.OrderId == b.Id && a.ProductId == 1).Select((x, y) => y.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Company, Order>((x, y) => f.Id == y.SellerId && f.CompanyId == x.Id))
            .GroupBy(f => new { f.Gender, f.Age })
            .Select((t, a) => new { t.Grouping, CompanyCount = t.CountDistinct(a.CompanyId), UserCount = t.Count(a.Id) })
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Gender],a.[Age],COUNT(DISTINCT a.[CompanyId]) AS CompanyCount,COUNT(a.[Id]) AS UserCount FROM [sys_user] a WHERE a.[Id] IN (SELECT c.[BuyerId] FROM [sys_order_detail] b INNER JOIN [sys_order] c ON b.[OrderId]=c.[Id] AND b.[ProductId]=1) AND EXISTS(SELECT * FROM [sys_company] x,[sys_order] y WHERE a.[Id]=y.[SellerId] AND a.[CompanyId]=x.[Id]) GROUP BY a.[Gender],a.[Age]");
    }
    [Fact]
    public void FromQuery_SelectAggregate()
    {
        using var repository = this.dbFactory.Create();
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
        Assert.True(sql == "SELECT a.[Id] AS UserId,b.[Id] AS OrderId,COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId]ORDER BY a.[Id],b.[Id]");
    }
    [Fact]
    public void FromQuery_Select_Sql_Aggregate()
    {
        using var repository = this.dbFactory.Create();
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
        Assert.True(sql == "SELECT a.[Id] AS UserId,b.[Id] AS OrderId,COUNT(b.[Id]) AS OrderCount,SUM(b.[TotalAmount]) AS TotalAmount FROM [sys_user] a INNER JOIN [sys_order] b ON a.[Id]=b.[BuyerId]ORDER BY a.[Id],b.[Id]");
    }
    [Fact]
    public void Query_Count()
    {
        using var repository = this.dbFactory.Create();
        var count = repository.From<User>().Count();
        var count1 = repository.From<User>().Select(f => Sql.Count()).First();
        var count2 = repository.QueryFirst<int>("SELECT COUNT(1) FROM sys_user");
        Assert.True(count == count1);
        Assert.True(count == count2);
    }
    [Fact]
    public void Query_Max()
    {
        using var repository = this.dbFactory.Create();
        var count = repository.From<Order>().Max(f => f.TotalAmount);
        var count1 = repository.From<Order>().Select(f => Sql.Max(f.TotalAmount)).First();
        var count2 = repository.QueryFirst<double>("SELECT MAX(TotalAmount) FROM sys_order");
        Assert.True(count == count1);
        Assert.True(count == count2);
    }
    [Fact]
    public void Query_Min()
    {
        using var repository = this.dbFactory.Create();
        var count = repository.From<Order>().Min(f => f.TotalAmount);
        var count1 = repository.From<Order>().Select(f => Sql.Min(f.TotalAmount)).First();
        var count2 = repository.QueryFirst<double>("SELECT MIN(TotalAmount) FROM sys_order");
        Assert.True(count == count1);
        Assert.True(count == count2);
    }
    [Fact]
    public void Query_Avg()
    {
        using var repository = this.dbFactory.Create();
        var value1 = repository.From<Order>().Avg(f => f.TotalAmount);
        var value2 = repository.From<Order>().Select(f => Sql.Avg(f.TotalAmount)).First();
        var value3 = repository.QueryFirst<double>("SELECT AVG(TotalAmount) FROM sys_order");
        Assert.True(value1 == value2);
        Assert.True(value1 == value3);
    }
    [Fact]
    public void Query_ValueTuple()
    {
        using var repository = this.dbFactory.Create();
        var sql = "SELECT Id,OrderNo,TotalAmount FROM sys_order";
        var result = repository.Query<(int OrderId, string OrderNo, double TotalAmount)>(sql);
        Assert.NotNull(result);
    }
    [Fact]
    public void Query_Json()
    {
        using var repository = this.dbFactory.Create();
        var result = repository.From<Order>().First();
        Assert.NotNull(result);
    }
}
