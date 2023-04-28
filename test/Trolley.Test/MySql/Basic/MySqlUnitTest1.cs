using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Trolley.Test.MySql;

public class MySqlUnitTest1 : UnitTestBase
{
    public MySqlUnitTest1()
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
    public async void Insert_Parameter()
    {
        using var repository = dbFactory.Create();
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
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Brand>().Where(new { Id = 1 }).Execute();
        var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES(@Id,@BrandNo,@Name,1,NOW(),@User,NOW(),@User)";
        var count = await repository.CreateAsync<Brand>(rawSql, new
        {
            Id = 1,
            BrandNo = "BN-001",
            Name = "²¨Ë¾µÇ",
            User = 1
        });
        repository.Commit();
        Assert.Equal(1, count);
    }
    [Fact]
    public async void Insert_Parameters()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var count = repository.Create<Product>(new[]
        {
            new
            {
                Id = 1,
                ProductNo="PN-001",
                Name = "²¨Ë¾µÇÓðÈÞ·þ",
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
                Name = "Ñ©ÖÐ·ÉÓðÈÞ¿ã",
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
                Name = "ÓÅÒÂ¿â±£Å¯ÄÚÒÂ",
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
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Brand>().Where(new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } }).Execute();
        var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES(@Id,@BrandNo,@Name,1,NOW(),@User,NOW(),@User)";
        var count = await repository.Create<Brand>().RawSql(rawSql, new
        {
            Id = 1,
            BrandNo = "BN-001",
            Name = "²¨Ë¾µÇ",
            User = 1
        }).ExecuteAsync();
        Assert.Equal(1, count);
        count = await repository.Create<Brand>().RawSql(rawSql, new
        {
            Id = 2,
            BrandNo = "BN-002",
            Name = "Ñ©ÖÐ·É",
            User = 1
        }).ExecuteAsync();
        Assert.Equal(1, count);
        count = await repository.Create<Brand>().RawSql(rawSql, new
        {
            Id = 3,
            BrandNo = "BN-003",
            Name = "ÓÅÒÂ¿â",
            User = 1
        }).ExecuteAsync();
        Assert.Equal(1, count);
        repository.Commit();
    }
    [Fact]
    public async void Insert_WithBy_AnonymousObject()
    {
        using var repository = dbFactory.Create();
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
             })
             .ToSql(out _);

        Assert.True(sql == "INSERT INTO `sys_user` (`Id`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");

        repository.BeginTransaction();
        var count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
        count = await repository.Create<User>()
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
            }).ExecuteAsync();
        repository.Commit();
        Assert.Equal(1, count);
    }
    [Fact]
    public async void Insert_WithBy_AnonymousObject_Condition()
    {
        using var repository = dbFactory.Create();
        Dispute disputes = null;
        int[] products = new int[] { 1, 2, 3 };
        var sql = repository.Create<Order>()
             .WithBy(new
             {
                 Id = 9,
                 OrderNo = "ON_009",
                 BuyerId = 1,
                 ProductCount = 2,
                 SellerId = 2,
                 TotalAmount = 500,
                 IsEnabled = true,
                 CreatedAt = DateTime.Now,
                 CreatedBy = 1,
                 UpdatedAt = DateTime.Now,
                 UpdatedBy = 1
             })
             .WithBy(disputes != null, new { Disputes = disputes })
             .WithBy(products != null, new { Products = products })
             .ToSql(out var dbParameters);

        Assert.True(sql == "INSERT INTO `sys_order` (`Id`,`OrderNo`,`BuyerId`,`ProductCount`,`SellerId`,`TotalAmount`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`,`Products`) VALUES(@Id,@OrderNo,@BuyerId,@ProductCount,@SellerId,@TotalAmount,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@Products)");
        Assert.True(dbParameters.Count == 12);
        Assert.True(dbParameters[11].Value.ToString() == "[1,2,3]");

        repository.BeginTransaction();
        var count = repository.Delete<Order>().Where(f => f.Id == 9).Execute();
        count = await repository.Create<Order>()
            .WithBy(new
            {
                Id = 9,
                OrderNo = "ON_009",
                BuyerId = 1,
                ProductCount = 2,
                SellerId = 2,
                TotalAmount = 500,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            })
            .WithBy(disputes != null, new { Disputes = disputes })
            .WithBy(products != null, new { Products = products })
            .ExecuteAsync();
        repository.Commit();
        Assert.Equal(1, count);
    }
    //[Fact]
    //public async void Insert_WithBy_Dictionary_AutoIncrement()
    //{
    //    using var repository = this.dbFactory.Create();
    //    await repository.Delete<Company>().Where(f => f.Id == 1).ExecuteAsync();
    //    var id = repository.Create<Company>()
    //        .WithBy(new Dictionary<string, object>()
    //        {
    //                { "Id", 1},
    //                { "Name","Î¢Èí11"},
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
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var count = repository.Create<Product>()
            .WithByBulk(new[]
            {
                new
                {
                    Id = 1,
                    ProductNo="PN-001",
                    Name = "²¨Ë¾µÇÓðÈÞ·þ",
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
                    Name = "Ñ©ÖÐ·ÉÓðÈÞ¿ã",
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
                    Name = "ÓÅÒÂ¿â±£Å¯ÄÚÒÂ",
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
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        await repository.Delete<Product>().Where(new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } }).ExecuteAsync();
        var count = repository.Create<Product>()
            .WithByBulk(new[]
            {
                new Dictionary<string,object>
                {
                    { "Id",1 },
                    { "ProductNo","PN-001"},
                    { "Name","²¨Ë¾µÇÓðÈÞ·þ"},
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
                    { "Name","Ñ©ÖÐ·ÉÓðÈÞ¿ã"},
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
                    { "Name","ÓÅÒÂ¿â±£Å¯ÄÚÒÂ"},
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
        using var repository = dbFactory.Create();
        repository.Delete<Product>(2);
        var brand = repository.Get<Brand>(1);
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
        Assert.True(sql == "INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT a.`Id`+1,CONCAT('PN_',a.`BrandNo`),CONCAT('PName_',a.`Name`),a.`Id`,@p0,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt` FROM `sys_brand` a WHERE a.`Id`=1");

        var count = repository.Create<Product>()
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
           .Execute();
        var product = repository.Get<Product>(2);
        Assert.True(count > 0);
        Assert.NotNull(product);
        Assert.True(product.ProductNo == "PN_" + brand.BrandNo);
        Assert.True(product.Name == "PName_" + brand.Name);
    }
    [Fact]
    public async void Insert_Select_From_Table2()
    {
        using var repository = dbFactory.Create();
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
            .ToSql(out var dbParameters);
        Assert.True(sql == "INSERT INTO `sys_order_detail` (`Id`,`OrderId`,`ProductId`,`Price`,`Quantity`,`Amount`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT @p0,a.`Id`,b.`Id`,b.`Price`,@p1,b.`Price`*3,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt` FROM `sys_order` a,`sys_product` b WHERE a.`Id`=3 AND b.`Id`=1");
        Assert.True(dbParameters.Count == 2);
        Assert.True((int)dbParameters[0].Value == 7);
        Assert.True((int)dbParameters[1].Value == 3);

        repository.Delete<OrderDetail>(7);
        var result = await repository.Create<OrderDetail>()
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
           .ExecuteAsync();
        var orderDetail = repository.Get<OrderDetail>(7);
        var product = repository.Get<Product>(1);
        Assert.NotNull(orderDetail);
        Assert.True(orderDetail.OrderId == 3);
        Assert.True(orderDetail.ProductId == 1);
        Assert.True(orderDetail.Amount == product.Price * 3);

    }
    [Fact]
    public void Insert_Null_Field()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Order>(1);
        var count = repository.Create<Order>(new Order
        {
            Id = 1,
            OrderNo = "ON-001",
            BuyerId = 1,
            SellerId = 2,
            TotalAmount = 500,
            //´Ë×Ö¶Î¿ÉÎª¿Õ£¬µ«²»¸³Öµ
            //ProductCount = 3,
            Products = new List<int> { 1, 2 },
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
        using var repository = dbFactory.Create();
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
    [Fact]
    public void Insert_Enum_Fields()
    {
        using var repository = dbFactory.Create();
        var sql1 = repository.Create<User>()
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
            })
            .ToSql(out var parameters1);
        Assert.True(sql1 == "INSERT INTO `sys_user` (`Id`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
        Assert.True(parameters1[4].ParameterName == "@Gender");
        Assert.True(parameters1[4].Value.GetType() == typeof(byte));
        Assert.True((byte)parameters1[4].Value == (byte)Gender.Male);

        var sql2 = repository.Create<Company>()
             .WithBy(new Company
             {
                 Id = 1,
                 Name = "leafkevin",
                 Nature = CompanyNature.Internet,
                 IsEnabled = true,
                 CreatedAt = DateTime.Now,
                 CreatedBy = 1,
                 UpdatedAt = DateTime.Now,
                 UpdatedBy = 1
             })
             .ToSql(out var parameters2);
        Assert.True(sql2 == "INSERT INTO `sys_company` (`Id`,`Name`,`Nature`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) VALUES(@Id,@Name,@Nature,@IsEnabled,@CreatedBy,@CreatedAt,@UpdatedBy,@UpdatedAt)");
        Assert.True(parameters2[2].ParameterName == "@Nature");
        Assert.True(parameters2[2].Value.GetType() == typeof(string));
        Assert.True((string)parameters2[2].Value == CompanyNature.Internet.ToString());
    }
}