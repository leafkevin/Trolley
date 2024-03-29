﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Trolley.SqlServer;
using Xunit;

namespace Trolley.Test.SqlServer;

public class UnitTest1 : UnitTestBase
{
    public UnitTest1()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
             .Register<SqlServerProvider>("fengling", "Server=127.0.0.1;Database=fengling;Uid=sa;password=SQLserverSA123456;TrustServerCertificate=true", true)
             .Configure<SqlServerProvider, ModelConfiguration>();
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
        var count = repository.Delete<User>().Where(f => f.Id == 4).Execute();
        count = await repository.CreateAsync<User>(new
        {
            Id = 4,
            Name = "leafkevin",
            Age = 25,
            CompanyId = 1,
            Gender = Gender.Male,
            IsEnabled = true,
            CreatedAt = DateTime.Now,
            CreatedBy = 1,
            UpdatedAt = DateTime.Now,
            UpdatedBy = 1,
            SomeTimes = TimeSpan.FromMinutes(35),
            GuidField = Guid.NewGuid()
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
        var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES(@Id,@BrandNo,@Name,1,GETDATE(),@User,GETDATE(),@User)";
        var count = await repository.ExecuteAsync(rawSql, new
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
        using var repository = dbFactory.Create();
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
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Brand>().Where(new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } }).Execute();
        var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES(@Id,@BrandNo,@Name,1,GETDATE(),@User,GETDATE(),@User)";
        var count = await repository.ExecuteAsync(rawSql, new
        {
            Id = 1,
            BrandNo = "BN-001",
            Name = "波司登",
            User = 1
        });
        Assert.Equal(1, count);
        repository.Commit();
    }
    [Fact]
    public void Insert_WithBy_AnonymousObject()
    {
        using var repository = dbFactory.Create();
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
            })
            .ToSql(out _);
        repository.Commit();
        Assert.True(sql == "INSERT INTO [sys_user] ([Id],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES(@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
    }
    [Fact]
    public async void Insert_WithBy_AnonymousObject_Condition()
    {
        this.Initialize();
        Guid? guidField = Guid.NewGuid();
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        var user = repository.Get<User>(1);
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
            })
            .WithBy(false, new { user.SomeTimes })
            .WithBy(guidField.HasValue, new { GuidField = guidField })
            .ToSql(out _);
        repository.Commit();
        Assert.True(sql == "INSERT INTO [sys_user] ([Id],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy],[GuidField]) VALUES(@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@GuidField)");
        repository.BeginTransaction();
        count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
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
    public async void Insert_WithBy_Dictionary_AutoIncrement()
    {
        using var repository = dbFactory.Create();
        await repository.Delete<Company>().Where(f => f.Id == 1).ExecuteAsync();
        var id = repository.Create<Company>()
            .WithBy(new Dictionary<string, object>()
            {
                    //{ "Id", 1},
                    { "Name","微软11"},
                    { "IsEnabled", true},
                    { "CreatedAt", DateTime.Now},
                    { "CreatedBy", 1},
                    { "UpdatedAt", DateTime.Now},
                    { "UpdatedBy", 1}
            }).Execute();
        var maxId = repository.From<Company>().Max(f => f.Id);
        Assert.Equal(maxId, id);
    }
    [Fact]
    public async void Insert_WithBy_Batch_AnonymousObjects()
    {
        using var repository = dbFactory.Create();
        var sql = repository.Create<Product>()
            .WithBulk(new[]
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
            }, 50)
            .ToSql(out _);
        Assert.True(sql == "INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[BrandId],[CategoryId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)");

        repository.BeginTransaction();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var count = repository.Create<Product>()
            .WithBulk(new[]
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
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        await repository.Delete<Product>().Where(new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } }).ExecuteAsync();
        var count = repository.Create<Product>()
            .WithBulk(new[]
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
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Product>(2);
        var brand = repository.Get<Brand>(1);
        //var sql = repository.Create<Product>()
        //    .From<Brand>()
        //    .Select(f => new
        //    {
        //        Id = f.Id + 1,
        //        ProductNo = "PN_" + f.BrandNo,
        //        Name = "PName_" + f.Name,
        //        BrandId = f.Id,
        //        CategoryId = 1,
        //        f.CompanyId,
        //        f.IsEnabled,
        //        f.CreatedBy,
        //        f.CreatedAt,
        //        f.UpdatedBy,
        //        f.UpdatedAt
        //    })
        //    .Where(f => f.Id == 1)
        //    .ToSql(out var parameters);
        //Assert.True(sql == "INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[BrandId],[CategoryId],[CompanyId],[IsEnabled],[CreatedBy],[CreatedAt],[UpdatedBy],[UpdatedAt]) SELECT a.[Id]+1,('PN_'+a.[BrandNo]),('PName_'+a.[Name]),a.[Id],@CategoryId,a.[CompanyId],a.[IsEnabled],a.[CreatedBy],a.[CreatedAt],a.[UpdatedBy],a.[UpdatedAt] FROM [sys_brand] a WHERE a.[Id]=1");

        //var count = repository.Create<Product>()
        //   .From<Brand>()
        //   .Select(f => new
        //   {
        //       Id = f.Id + 1,
        //       ProductNo = "PN_" + f.BrandNo,
        //       Name = "PName_" + f.Name,
        //       BrandId = f.Id,
        //       CategoryId = 1,
        //       f.CompanyId,
        //       f.IsEnabled,
        //       f.CreatedBy,
        //       f.CreatedAt,
        //       f.UpdatedBy,
        //       f.UpdatedAt
        //   })
        //   .Where(f => f.Id == 1)
        //   .Execute();
        //var product = repository.Get<Product>(2);
        repository.Commit();
        //Assert.True(count > 0);
        //Assert.NotNull(product);
        //Assert.True(product.ProductNo == "PN_" + brand.BrandNo);
        //Assert.True(product.Name == "PName_" + brand.Name);
        //Assert.NotNull(parameters);
        //Assert.True(parameters.Count == 1);
    }
    [Fact]
    public void Insert_Select_From_Table2()
    {
        using var repository = dbFactory.Create();
        //var sql = repository.Create<OrderDetail>()
        //    .From<Order, Product>()
        //    .Where((a, b) => a.Id == 3 && b.Id == 1)
        //    .Select((x, y) => new OrderDetail
        //    {
        //        Id = 7,
        //        OrderId = x.Id,
        //        ProductId = y.Id,
        //        Price = y.Price,
        //        Quantity = 3,
        //        Amount = y.Price * 3,
        //        IsEnabled = x.IsEnabled,
        //        CreatedBy = x.CreatedBy,
        //        CreatedAt = x.CreatedAt,
        //        UpdatedBy = x.UpdatedBy,
        //        UpdatedAt = x.UpdatedAt
        //    })
        //    .ToSql(out var parameters);
        //Assert.True(sql == "INSERT INTO [sys_order_detail] ([Id],[OrderId],[ProductId],[Price],[Quantity],[Amount],[IsEnabled],[CreatedBy],[CreatedAt],[UpdatedBy],[UpdatedAt]) SELECT @Id,a.[Id],b.[Id],b.[Price],@Quantity,b.[Price]*3,a.[IsEnabled],a.[CreatedBy],a.[CreatedAt],a.[UpdatedBy],a.[UpdatedAt] FROM [sys_order] a,[sys_product] b WHERE a.[Id]=3 AND b.[Id]=1");
        //Assert.True(parameters.Count == 2);
        //Assert.True((int)parameters[0].Value == 7);
        //Assert.True((int)parameters[1].Value == 3);

        repository.Delete<OrderDetail>(7);
        //var result = repository.Create<OrderDetail>()
        //   .From<Order, Product>()
        //   .Where((a, b) => a.Id == 3 && b.Id == 1)
        //   .Select((x, y) => new OrderDetail
        //   {
        //       Id = 7,
        //       OrderId = x.Id,
        //       ProductId = y.Id,
        //       Price = y.Price,
        //       Quantity = 3,
        //       Amount = y.Price * 3,
        //       IsEnabled = x.IsEnabled,
        //       CreatedBy = x.CreatedBy,
        //       CreatedAt = x.CreatedAt,
        //       UpdatedBy = x.UpdatedBy,
        //       UpdatedAt = x.UpdatedAt
        //   })
        //   .Execute();
        var orderDetail = repository.Get<OrderDetail>(7);
        var product = repository.Get<Product>(1);
        Assert.NotNull(orderDetail);
        Assert.True(orderDetail.OrderId == "3");
        Assert.True(orderDetail.ProductId == 1);
        Assert.True(orderDetail.Amount == product.Price * 3);
    }
    [Fact]
    public void Insert_Null_Field()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Order>("1");
        var count = repository.Create<Order>(new Order
        {
            Id = "1",
            TenantId = "1",
            OrderNo = "ON-001",
            BuyerId = 1,
            SellerId = 2,
            TotalAmount = 500,
            //此字段可为空，但不赋值
            //ProductCount = 3,
            Products = new List<int> { 1, 2 },
            IsEnabled = true,
            CreatedAt = DateTime.Now,
            CreatedBy = 1,
            UpdatedAt = DateTime.Now,
            UpdatedBy = 1
        });
        var result = repository.Get<Order>("1");
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
        repository.Delete<Order>("4");
        var sql = repository.Create<Order>()
            .WithBy(new Order
            {
                Id = "4",
                TenantId = "1",
                OrderNo = "ON-001",
                BuyerId = 1,
                SellerId = 2,
                TotalAmount = 500,
                Products = new List<int> { 1, 2 },
                Disputes = new Dispute
                {
                    Id = 2,
                    Content = "无良商家",
                    Result = "同意退款",
                    Users = "Buyer2,Seller2",
                    CreatedAt = DateTime.Parse("2023-03-05")
                },
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            })
            .ToSql(out var parameters);
        Assert.True(sql == "INSERT INTO [sys_order] ([Id],[OrderNo],[TotalAmount],[BuyerId],[BuyerSource],[SellerId],[ProductCount],[Products],[Disputes],[IsEnabled],[CreatedBy],[CreatedAt],[UpdatedBy],[UpdatedAt]) VALUES(@Id,@OrderNo,@TotalAmount,@BuyerId,@BuyerSource,@SellerId,@ProductCount,@Products,@Disputes,@IsEnabled,@CreatedBy,@CreatedAt,@UpdatedBy,@UpdatedAt)");
        Assert.True(parameters[4].ParameterName == "@BuyerSource");
        Assert.True(parameters[4].Value is DBNull);
        Assert.True(parameters[7].ParameterName == "@Products");
        Assert.True((string)parameters[7].Value == "[1,2]");
        Assert.True(parameters[8].ParameterName == "@Disputes");
        Assert.True((string)parameters[8].Value == "{\"Id\":2,\"Users\":\"Buyer2,Seller2\",\"Content\":\"\\u65E0\\u826F\\u5546\\u5BB6\",\"Result\":\"\\u540C\\u610F\\u9000\\u6B3E\",\"CreatedAt\":\"2023-03-05T00:00:00\"}");
        var count = repository.Create<Order>()
            .WithBy(new Order
            {
                Id = "4",
                TenantId = "1",
                OrderNo = "ON-001",
                BuyerId = 1,
                SellerId = 2,
                TotalAmount = 500,
                Products = new List<int> { 1, 2 },
                Disputes = new Dispute
                {
                    Id = 2,
                    Content = "无良商家",
                    Result = "同意退款",
                    Users = "Buyer2,Seller2",
                    CreatedAt = DateTime.Now
                },
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            })
            .Execute();
        var order = repository.Get<Order>("4");
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
        Assert.True(sql1 == "INSERT INTO [sys_user] ([Id],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES(@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
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
        Assert.True(sql2 == "INSERT INTO [sys_company] ([Id],[Name],[Nature],[IsEnabled],[CreatedBy],[CreatedAt],[UpdatedBy],[UpdatedAt]) VALUES(@Id,@Name,@Nature,@IsEnabled,@CreatedBy,@CreatedAt,@UpdatedBy,@UpdatedAt)");
        Assert.True(parameters2[2].ParameterName == "@Nature");
        Assert.True(parameters2[2].Value.GetType() == typeof(string));
        Assert.True((string)parameters2[2].Value == CompanyNature.Internet.ToString());
    }
    //[Fact]
    //public async void Insert_Ignore()
    //{
    //    this.Initialize();
    //    using var repository = dbFactory.Create();
    //    var sql1 = repository.Create<User>()
    //        .WithBy(new
    //        {
    //            Id = 1,
    //            Name = "leafkevin",
    //            Age = 25,
    //            CompanyId = 1,
    //            Gender = Gender.Male,
    //            IsEnabled = true,
    //            CreatedAt = DateTime.Now,
    //            CreatedBy = 1,
    //            UpdatedAt = DateTime.Now,
    //            UpdatedBy = 1
    //        })
    //        .UseIgnore()
    //        .ToSql(out var parameters1);
    //    Assert.True(sql1 == "INSERT INTO [sys_user] ([Id],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) SELECT(@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) WHERE NOT EXISTS(SELECT * FROM [sys_user] b WHERE b.[Id]=@kId)");
    //    var count = await repository.Create<User>()
    //        .WithBy(new
    //        {
    //            Id = 1,
    //            Name = "leafkevin",
    //            Age = 25,
    //            CompanyId = 1,
    //            Gender = Gender.Male,
    //            IsEnabled = true,
    //            CreatedAt = DateTime.Now,
    //            CreatedBy = 1,
    //            UpdatedAt = DateTime.Now,
    //            UpdatedBy = 1
    //        })
    //        .UseIgnore()
    //        .ExecuteAsync();
    //    Assert.True(count == 0);
    //}
}