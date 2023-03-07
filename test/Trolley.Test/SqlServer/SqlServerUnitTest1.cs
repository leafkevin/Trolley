using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Trolley.Test;

public class SqlServerUnitTest1
{
    private readonly IOrmDbFactory dbFactory;
    public SqlServerUnitTest1()
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
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async void Insert_Parameter()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
        count = await repository.CreateAsync<User>(new
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
        });
        repository.Commit();
        Assert.Equal(1, count);
    }
    [Fact]
    public async void Insert_RawSql()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Brand>().Where(new { Id = 1 }).Execute();
        var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES(@Id,@BrandNo,@Name,1,GETDATE(),@User,GETDATE(),@User)";
        var count = await repository.CreateAsync<Brand>(rawSql, new
        {
            Id = 1,
            BrandNo = "BN-001",
            Name = "波司登",
            User = 1
        });
        repository.Commit();
        Assert.Equal(1, count);
    }
    [Fact]
    public async void Insert_Parameters()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var count = repository.Create<Product>(new[]
        {
            new
            {
                Id = 1,
                ProductNo="PN-001",
                Name = "波司登羽绒服",
                BrandId = 1,
                CategoryId = 1,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new
            {
                Id = 2,
                ProductNo="PN-002",
                Name = "雪中飞羽绒裤",
                BrandId = 2,
                CategoryId = 2,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new
            {
                Id = 3,
                ProductNo="PN-003",
                Name = "优衣库保暖内衣",
                BrandId = 3,
                CategoryId = 3,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });
        repository.Commit();
        Assert.Equal(3, count);
    }
    [Fact]
    public async void Insert_RawSql1()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Brand>().Where(new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } }).Execute();
        var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES(@Id,@BrandNo,@Name,1,GETDATE(),@User,GETDATE(),@User)";
        var count = await repository.Create<Brand>().RawSql(rawSql, new
        {
            Id = 1,
            BrandNo = "BN-001",
            Name = "波司登",
            User = 1
        }).ExecuteAsync();
        Assert.Equal(1, count);
        count = await repository.Create<Brand>().RawSql(rawSql, new
        {
            Id = 2,
            BrandNo = "BN-002",
            Name = "雪中飞",
            User = 1
        }).ExecuteAsync();
        Assert.Equal(1, count);
        count = await repository.Create<Brand>().RawSql(rawSql, new
        {
            Id = 3,
            BrandNo = "BN-003",
            Name = "优衣库",
            User = 1
        }).ExecuteAsync();
        Assert.Equal(1, count);
        repository.Commit();
    }
    [Fact]
    public void Insert_WithBy_AnonymousObject()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
        var sql = repository.Create<User>()
            .WithBy(new
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
            }).ToSql(out _);
        repository.Commit();
        Assert.Equal("INSERT INTO [sys_user] ([Id],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES(@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
    }
    //[Fact]
    //public async void Insert_WithBy_Dictionary_AutoIncrement()
    //{
    //    using var repository = this.dbFactory.Create();
    //    await repository.Delete<Company>().Where(f => f.Id == 1).ExecuteAsync();
    //    var id = repository.Create<Company>()
    //        .WithBy(new Dictionary<string, object>()
    //        {
    //                //{ "Id", 1},
    //                { "Name","微软11"},
    //                { "IsEnabled", true},
    //                { "CreatedAt", DateTime.Now},
    //                { "CreatedBy", 1},
    //                { "UpdatedAt", DateTime.Now},
    //                { "UpdatedBy", 1}
    //        }).Execute();
    //    var maxId = repository.From<Company>().Max(f => f.Id);
    //    Assert.Equal(maxId, id);
    //}
    [Fact]
    public async void Insert_WithBy_Batch_AnonymousObjects()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var count = repository.Create<Product>()
            .WithBy(new[]
            {
                new
                {
                    Id = 1,
                    ProductNo="PN-001",
                    Name = "波司登羽绒服",
                    BrandId = 1,
                    CategoryId = 1,
                    IsEnabled = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = 1,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = 1
                },
                new
                {
                    Id = 2,
                    ProductNo="PN-002",
                    Name = "雪中飞羽绒裤",
                    BrandId = 2,
                    CategoryId = 2,
                    IsEnabled = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = 1,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = 1
                },
                new
                {
                    Id = 3,
                    ProductNo="PN-003",
                    Name = "优衣库保暖内衣",
                    BrandId = 3,
                    CategoryId = 3,
                    IsEnabled = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = 1,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = 1
                }
            }).Execute();
        repository.Commit();
        Assert.Equal(3, count);
    }
    [Fact]
    public async void Insert_WithBy_Batch_Dictionaries()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        await repository.Delete<Product>().Where(new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } }).ExecuteAsync();
        var count = repository.Create<Product>()
            .WithBy(new[]
            {
                new Dictionary<string,object>
                {
                    { "Id",1 },
                    { "ProductNo","PN-001"},
                    { "Name","波司登羽绒服"},
                    { "BrandId",1},
                    { "CategoryId",1},
                    { "IsEnabled",true},
                    { "CreatedAt",DateTime.Now},
                    { "CreatedBy",1},
                    { "UpdatedAt",DateTime.Now},
                    { "UpdatedBy",1}
                },
                new Dictionary<string,object>
                {
                    { "Id",2},
                    { "ProductNo","PN-002"},
                    { "Name","雪中飞羽绒裤"},
                    { "BrandId",2},
                    { "CategoryId",2},
                    { "IsEnabled",true},
                    { "CreatedAt",DateTime.Now},
                    { "CreatedBy",1},
                    { "UpdatedAt",DateTime.Now},
                    { "UpdatedBy",1}
                },
                new Dictionary<string,object>
                {
                    { "Id",3},
                    { "ProductNo","PN-003"},
                    { "Name","优衣库保暖内衣"},
                    { "BrandId",3},
                    { "CategoryId",3},
                    { "IsEnabled",true},
                    { "CreatedAt",DateTime.Now},
                    { "CreatedBy",1},
                    { "UpdatedAt",DateTime.Now},
                    { "UpdatedBy",1}
    }
            }).Execute();
        repository.Commit();
        Assert.Equal(3, count);
    }
    [Fact]
    public void Insert_Select_From_Table1()
    {
        using var repository = this.dbFactory.Create();
        var sql = repository.Create<Product>()
            .From<Brand>(f => new
            {
                Id = f.Id + 1,
                ProductNo = "PN_" + f.BrandNo,
                Name = "PName_" + f.Name,
                BrandId = f.Id,
                CategoryId = 1,
                f.CompanyId,
                f.IsEnabled,
                f.CreatedBy,
                f.CreatedAt,
                f.UpdatedBy,
                f.UpdatedAt
            })
            .Where(f => f.Id == 1)
            .ToSql(out _);
        Assert.True(sql == "INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[BrandId],[CategoryId],[CompanyId],[IsEnabled],[CreatedBy],[CreatedAt],[UpdatedBy],[UpdatedAt]) SELECT a.[Id]+1,'PN_'+a.[BrandNo],'PName_'+a.[Name],a.[Id],@CategoryId,a.[CompanyId],a.[IsEnabled],a.[CreatedBy],a.[CreatedAt],a.[UpdatedBy],a.[UpdatedAt] FROM [sys_brand] a WHERE a.[Id]=1");
    }
    [Fact]
    public void Insert_Select_From_Table2()
    {
        using var repository = this.dbFactory.Create();
        var sql = repository.Create<OrderDetail>()
            .From<Order, Product>((x, y) => new OrderDetail
            {
                Id = 7,
                OrderId = x.Id,
                ProductId = y.Id,
                Price = y.Price,
                Quantity = 3,
                Amount = y.Price * 3,
                IsEnabled = x.IsEnabled,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt
            })
            .Where((a, b) => a.Id == 3 && b.Id == 1)
            .ToSql(out _);
        Assert.True(sql == "INSERT INTO [sys_order_detail] ([Id],[OrderId],[ProductId],[Price],[Quantity],[Amount],[IsEnabled],[CreatedBy],[CreatedAt],[UpdatedBy],[UpdatedAt]) SELECT @Id,a.[Id],b.[Id],b.[Price],@Quantity,b.[Price]*3,a.[IsEnabled],a.[CreatedBy],a.[CreatedAt],a.[UpdatedBy],a.[UpdatedAt] FROM [sys_order] a,[sys_product] b WHERE a.[Id]=3 AND b.[Id]=1");
    }
    [Fact]
    public void Insert_Null_Field()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Order>(1);
        var count = repository.Create<Order>(new Order
        {
            Id = 1,
            OrderNo = "ON-001",
            BuyerId = 1,
            SellerId = 2,
            TotalAmount = 500,
            //此字段可为空，但不赋值
            //ProductCount = 3,
            IsEnabled = true,
            CreatedAt = DateTime.Now,
            CreatedBy = 1,
            UpdatedAt = DateTime.Now,
            UpdatedBy = 1
        });
        var result = repository.Get<Order>(1);
        repository.Commit();
        if (count > 0)
        {
            Assert.True(!result.ProductCount.HasValue);
        }
    }
    [Fact]
    public void Insert_Json_Field()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Order>(4);
        var count = repository.Create<Order>(new Order
        {
            Id = 4,
            OrderNo = "ON-001",
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
        var order = repository.Get<Order>(4);
        repository.Commit();
        if (count > 0)
        {
            Assert.NotEmpty(order.Products);
            Assert.True(order.Products.Count == 2);
            Assert.True(order.Products[0] == 1);
            Assert.True(order.Products[1] == 2);
        }
    }
}