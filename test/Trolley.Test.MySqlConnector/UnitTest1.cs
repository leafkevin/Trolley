using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using Trolley.MySqlConnector;
using Xunit;

namespace Trolley.Test.MySqlConnector;

public class UnitTest1 : UnitTestBase
{
    public UnitTest1()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register<MySqlProvider>("fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", true)
            .Configure<MySqlProvider, ModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async void Insert_Parameter()
    {
        using var repository = dbFactory.Create();
        repository.Query<User>(f => f.Id == 4);
        repository.BeginTransaction();
        var count = repository.Delete<User>().Where(f => f.Id == 4).Execute();
        count = await repository.CreateAsync<User>(new
        {
            Id = 4,
            TenantId = "1",
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
        var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES(@Id,@BrandNo,@Name,1,NOW(),@User,NOW(),@User)";
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
        var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES(@Id,@BrandNo,@Name,1,NOW(),@User,NOW(),@User)";
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
    public void Insert_WithBy()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        var count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
        var now = DateTime.Now;
        var sql = repository.Create<User>()
            .WithBy(new
            {
                Id = 1,
                TenantId = "1",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                IsEnabled = true,
                CreatedAt = now,
                CreatedBy = 1,
                UpdatedAt = now,
                UpdatedBy = 1
            })
            .ToSql(out var dbParameters);
        repository.Commit();
        Assert.True(sql == "INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
        Assert.True((int)dbParameters[0].Value == 1);
        Assert.True((string)dbParameters[1].Value == "1");
        Assert.True((string)dbParameters[2].Value == "leafkevin");
        Assert.True((int)dbParameters[3].Value == 25);
        Assert.True((int)dbParameters[4].Value == 1);
        if (dbParameters[5] is MySqlParameter dbParameter)
        {
            Assert.True(dbParameter.MySqlDbType == MySqlDbType.Enum);
            Assert.True((string)dbParameter.Value == Gender.Male.ToString());
        }
        Assert.True((int)dbParameters[6].Value == 1);
        Assert.True((DateTime)dbParameters[7].Value == now);
        Assert.True((int)dbParameters[8].Value == 1);
        Assert.True((DateTime)dbParameters[9].Value == now);
        Assert.True((int)dbParameters[10].Value == 1);
    }
    [Fact]
    public void Insert_WithBy_IgnoreFields()
    {
        using var repository = dbFactory.Create();
        var now = DateTime.Now;
        var sql = repository.Create<User>()
           .WithBy(new
           {
               Id = 1,
               TenantId = "1",
               Name = "leafkevin",
               Age = 25,
               CompanyId = 1,
               Gender = Gender.Male,
               SourceType = UserSourceType.Douyin,
               IsEnabled = true,
               CreatedAt = now,
               CreatedBy = 1,
               UpdatedAt = now,
               UpdatedBy = 1
           })
           .IgnoreFields("CompanyId", "SourceType")
           .ToSql(out var dbParameters);
        Assert.True(sql == "INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@TenantId,@Name,@Age,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");

        repository.BeginTransaction();
        repository.Delete<User>().Where(f => f.Id == 1).Execute();
        repository.Create<User>()
            .WithBy(new
            {
                Id = 1,
                TenantId = "1",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = now,
                CreatedBy = 1,
                UpdatedAt = now,
                UpdatedBy = 1
            })
            .IgnoreFields("CompanyId", "SourceType")
            .Execute();
        var user = repository.Get<User>(1);
        repository.Commit();
        Assert.True(user.Gender == Gender.Male);
        Assert.True(user.CompanyId == 0);
        Assert.True(!user.SourceType.HasValue);

        sql = repository.Create<User>()
           .WithBy(new
           {
               Id = 1,
               TenantId = "1",
               Name = "leafkevin",
               Age = 25,
               CompanyId = 1,
               Gender = Gender.Male,
               IsEnabled = true,
               CreatedAt = now,
               CreatedBy = 1,
               UpdatedAt = now,
               UpdatedBy = 1
           })
           .IgnoreFields(f => new { f.Gender, f.CompanyId })
           .ToSql(out dbParameters);
        Assert.True(sql == "INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@TenantId,@Name,@Age,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");

        repository.BeginTransaction();
        repository.Delete<User>().Where(f => f.Id == 1).Execute();
        repository.Create<User>()
            .WithBy(new
            {
                Id = 1,
                TenantId = "1",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                IsEnabled = true,
                CreatedAt = now,
                CreatedBy = 1,
                UpdatedAt = now,
                UpdatedBy = 1
            })
            .IgnoreFields(f => new { f.Gender, f.CompanyId })
            .Execute();
        user = repository.Get<User>(1);
        repository.Commit();
        Assert.True(user.Gender == Gender.Unknown);
        Assert.True(user.CompanyId == 0);
    }
    [Fact]
    public async void Insert_WithBy_Condition()
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
                TenantId = "1",
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
            .WithBy(user.SomeTimes.HasValue, f => f.SomeTimes, user.SomeTimes)
            .WithBy(guidField.HasValue, new { GuidField = guidField })
            .ToSql(out _);
        repository.Commit();
        Assert.True(sql == "INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`,`SomeTimes`,`GuidField`) VALUES(@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@SomeTimes,@GuidField)");
        repository.BeginTransaction();
        count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
        count = await repository.Create<User>()
            .WithBy(new
            {
                Id = 1,
                TenantId = "1",
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
            .ExecuteAsync();
        repository.Commit();
        Assert.Equal(1, count);
    }
    [Fact]
    public async void Insert_WithBy_AutoIncrement()
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
            })
            .ExecuteIdentity();
        var maxId = repository.From<Company>().Max(f => f.Id);
        Assert.Equal(maxId, id);
    }
    [Fact]
    public async void Insert_WithBulk()
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
        Assert.True(sql == "INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)");

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
            })
            .Execute();
        repository.Commit();
        Assert.Equal(3, count);
    }
    [Fact]
    public async void Insert_WithBulk_Dictionaries()
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
            })
            .Execute();
        repository.Commit();
        Assert.Equal(3, count);
    }
    [Fact]
    public async void Insert_WithBulk_OnlyFields()
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
            .OnlyFields(f => new { f.Id, f.ProductNo, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
            .ToSql(out _);
        Assert.True(sql == "INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)");

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
            })
            .OnlyFields(f => new { f.Id, f.ProductNo, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
            .Execute();
        var products = await repository.QueryAsync<Product>(f => Sql.In(f.Id, new[] { 1, 2, 3 }));
        repository.Commit();
        Assert.Equal(3, count);
        foreach (var product in products)
        {
            Assert.True(product.BrandId == 0);
            Assert.True(product.CategoryId == 0);
        }
    }
    [Fact]
    public async void Insert_Select_From_Table1()
    {
        using var repository = dbFactory.Create();
        var id = 2;
        var brandId = 1;
        var name = "雪中飞羽绒裤";
        int categoryId = 1;
        repository.BeginTransaction();
        repository.Delete<Product>(id);
        var brand = repository.Get<Brand>(brandId);
        var sql = repository.Create<Product>()
            .IgnoreInto()
            .From<Brand>()
            .Where(f => f.Id == brandId)
            .Select(f => new Product
            {
                Id = id,
                ProductNo = "PN_" + id.ToString().PadLeft(3, '0'),
                Name = name,
                Price = 25.85,
                BrandId = f.Id,
                CategoryId = categoryId,
                CompanyId = f.CompanyId,
                IsEnabled = true,
                CreatedBy = 1,
                CreatedAt = DateTime.Now,
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now
            })
            .ToSql(out _);
        Assert.True(sql == "INSERT IGNORE INTO `sys_product` (`Id`,`ProductNo`,`Name`,`Price`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT @p1 AS `Id`,CONCAT('PN_',@p2) AS `ProductNo`,@p3 AS `Name`,25.85 AS `Price`,b.`Id` AS `BrandId`,@p4 AS `CategoryId`,b.`CompanyId`,1 AS `IsEnabled`,1 AS `CreatedBy`,NOW() AS `CreatedAt`,1 AS `UpdatedBy`,NOW() AS `UpdatedAt` FROM `sys_brand` b WHERE b.`Id`=@p0");

        var count = repository.Create<Product>()
            .IgnoreInto()
            .From<Brand>()
            .Where(f => f.Id == brandId)
            .Select(f => new Product
            {
                Id = id,
                ProductNo = "PN_" + id.ToString().PadLeft(3, '0'),
                Name = name,
                Price = 25.85,
                BrandId = f.Id,
                CategoryId = categoryId,
                CompanyId = f.CompanyId,
                IsEnabled = true,
                CreatedBy = 1,
                CreatedAt = DateTime.Now,
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now
            })
           .Execute();
        var product = repository.Get<Product>(id);
        repository.Commit();
        Assert.True(count > 0);
        Assert.NotNull(product);
        Assert.True(product.ProductNo == "PN_" + id.ToString().PadLeft(3, '0'));
        Assert.True(product.Name == name);
        Assert.True(product.BrandId == brandId);

        count = await repository.Create<Product>()
            .IgnoreInto()
            .From<Brand>()
            .Where(f => f.Id == brandId)
            .Select(f => new Product
            {
                Id = id,
                ProductNo = "PN_" + id.ToString().PadLeft(3, '0'),
                Name = name,
                Price = 25.85,
                BrandId = f.Id,
                CategoryId = categoryId,
                CompanyId = f.CompanyId,
                IsEnabled = true,
                CreatedBy = 1,
                CreatedAt = DateTime.Now,
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now
            })
           .ExecuteAsync();
        Assert.True(count == 0);
    }
    [Fact]
    public async void Insert_Select_From_Table2()
    {
        using var repository = dbFactory.Create();
        var sql = repository.Create<OrderDetail>()
            .IgnoreInto()
            .From<Order, Product>()
            .Where((a, b) => a.Id == "3" && b.Id == 1)
            .Select((x, y) => new OrderDetail
            {
                Id = "7",
                TenantId = "1",
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
            .ToSql(out var parameters);
        Assert.True(sql == "INSERT IGNORE INTO `sys_order_detail` (`Id`,`TenantId`,`OrderId`,`ProductId`,`Price`,`Quantity`,`Amount`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT '7' AS `Id`,'1' AS `TenantId`,b.`Id` AS `OrderId`,c.`Id` AS `ProductId`,c.`Price`,3 AS `Quantity`,(c.`Price`*3) AS `Amount`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt` FROM `sys_order` b,`sys_product` c WHERE b.`Id`='3' AND c.`Id`=1");
        await repository.BeginTransactionAsync();
        repository.Delete<OrderDetail>("7");
        var result = await repository.Create<OrderDetail>()
            .IgnoreInto()
            .From<Order, Product>()
            .Where((a, b) => a.Id == "3" && b.Id == 1)
            .Select((x, y) => new OrderDetail
            {
                Id = "7",
                TenantId = "1",
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
           .ExecuteAsync();
        var orderDetail = repository.Get<OrderDetail>("7");
        var product = repository.Get<Product>(1);
        await repository.CommitAsync();
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
        Assert.True(!result.ProductCount.HasValue);
    }
    [Fact]
    public void Insert_Json_Field()
    {
        using var repository = dbFactory.Create();
        var dispute = new Dispute
        {
            Id = 2,
            Content = "无良商家",
            Result = "同意退款",
            Users = "Buyer2,Seller2",
            CreatedAt = DateTime.Parse("2023-03-05")
        };
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
               Disputes = dispute,
               IsEnabled = true,
               CreatedAt = DateTime.Now,
               CreatedBy = 1,
               UpdatedAt = DateTime.Now,
               UpdatedBy = 1
           })
           .ToSql(out var parameters);
        Assert.True(sql == "INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`TotalAmount`,`BuyerId`,`BuyerSource`,`SellerId`,`ProductCount`,`Products`,`Disputes`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) VALUES(@Id,@TenantId,@OrderNo,@TotalAmount,@BuyerId,@BuyerSource,@SellerId,@ProductCount,@Products,@Disputes,@IsEnabled,@CreatedBy,@CreatedAt,@UpdatedBy,@UpdatedAt)");
        Assert.True(parameters[5].ParameterName == "@BuyerSource");
        Assert.True(parameters[5].Value is DBNull);
        Assert.True(parameters[8].ParameterName == "@Products");
        Assert.True((string)parameters[8].Value == new JsonTypeHandler().ToFieldValue(null, null, new List<int> { 1, 2 }).ToString());
        Assert.True(parameters[9].ParameterName == "@Disputes");
        Assert.True((string)parameters[9].Value == new JsonTypeHandler().ToFieldValue(null, null, dispute).ToString());

        repository.BeginTransaction();
        repository.Delete<Order>("4");
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
                Disputes = dispute,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            })
            .Execute();
        var order = repository.Get<Order>("4");
        repository.Commit();
        Assert.NotEmpty(order.Products);
        Assert.NotNull(order.Disputes);
        Assert.True(new JsonTypeHandler().ToFieldValue(null, null, order.Products).ToString() == new JsonTypeHandler().ToFieldValue(null, null, new List<int> { 1, 2 }).ToString());
        Assert.True(new JsonTypeHandler().ToFieldValue(null, null, order.Disputes).ToString() == new JsonTypeHandler().ToFieldValue(null, null, dispute).ToString());
    }
    [Fact]
    public void Insert_Enum_Fields()
    {
        using var repository = dbFactory.Create();
        var sql1 = repository.Create<User>()
            .WithBy(new
            {
                Id = 1,
                TenantId = "1",
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
        Assert.True(sql1 == "INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
        Assert.True(parameters1[5].ParameterName == "@Gender");
        Assert.True(parameters1[5].Value.GetType() == typeof(string));
        Assert.True((string)parameters1[5].Value == Gender.Male.ToString());

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
    [Fact]
    public async void Insert_Ignore()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql1 = repository.Create<User>()
            .IgnoreInto()
            .WithBy(new
            {
                Id = 1,
                TenantId = "1",
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
        Assert.True(sql1 == "INSERT IGNORE INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
        var count = await repository.Create<User>()
            .IgnoreInto()
            .WithBy(new
            {
                Id = 1,
                TenantId = "1",
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
            .ExecuteAsync();
        Assert.True(count == 0);
    }
    [Fact]
    public async void Insert_Ignore_OnlyFields()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Create<User>()
            .IgnoreInto()
            .WithBy(new
            {
                Id = 1,
                TenantId = "1",
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
            .OnlyFields(f => new { f.Id, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
            .ToSql(out var parameters);
        Assert.True(sql == "INSERT IGNORE INTO `sys_user` (`Id`,`Name`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@Name,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
        Assert.True(parameters.Count == 7);
        repository.BeginTransaction();
        repository.Delete<User>(1);
        var count = await repository.Create<User>()
            .IgnoreInto()
            .WithBy(new
            {
                Id = 1,
                TenantId = "1",
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
            .OnlyFields(f => new { f.Id, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
            .ExecuteAsync();
        var user = repository.Get<User>(1);
        Assert.True(count == 1);
        Assert.True(user.CompanyId == 0);
        Assert.True(user.Gender == Gender.Unknown);
        count = await repository.Create<User>()
            .IgnoreInto()
            .WithBy(new
            {
                Id = 1,
                TenantId = "1",
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
            .OnlyFields(f => new { f.Id, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
            .ExecuteAsync();
        Assert.True(count == 0);
    }
    [Fact]
    public async void Insert_OnDuplicateKeyUpdate()
    {
        using var repository = dbFactory.Create();
        UserSourceType? buyerSource = UserSourceType.Douyin;
        var sql1 = repository.Create<Order>()
             .WithBy(new
             {
                 Id = "9",
                 TenantId = "3",
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
             .OnDuplicateKeyUpdate(x => x
                .Set(new
                {
                    TotalAmount = 25,
                    Products = new List<int> { 1, 2 }
                })
                .Set(buyerSource.HasValue, f => f.BuyerSource, buyerSource)
             )
            .ToSql(out _);
        Assert.True(sql1 == "INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`TotalAmount`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@TenantId,@OrderNo,@BuyerId,@SellerId,@TotalAmount,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON DUPLICATE KEY UPDATE `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerSource`=@BuyerSource");

        var sql2 = repository.Create<Order>()
             .WithBy(new
             {
                 Id = "9",
                 TenantId = "3",
                 OrderNo = "ON-001",
                 BuyerId = 1,
                 SellerId = 2,
                 BuyerSource = buyerSource,
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
             .OnDuplicateKeyUpdate(x => x
                .Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
                .Set(f => f.Products, f => x.Values(f.Products)))
            .ToSql(out _);
        Assert.True(sql2 == "INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`TotalAmount`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@TenantId,@OrderNo,@BuyerId,@SellerId,@BuyerSource,@TotalAmount,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON DUPLICATE KEY UPDATE `TotalAmount`=VALUES(`TotalAmount`),`Products`=VALUES(`Products`)");

        await repository.BeginTransactionAsync();
        await repository.DeleteAsync<Order>("9");
        var count = await repository.Create<Order>()
             .WithBy(new
             {
                 Id = "9",
                 TenantId = "3",
                 OrderNo = "ON-001",
                 BuyerId = 1,
                 SellerId = 2,
                 BuyerSource = buyerSource,
                 TotalAmount = 500,
                 //Products = new List<int> { 1, 2 },
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
             .OnDuplicateKeyUpdate(x => x
                .Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
                .Set(true, f => f.Products, f => x.Values(f.Products)))
            .ExecuteAsync();
        var order = await repository.GetAsync<Order>("9");
        await repository.CommitAsync();
        Assert.True(count == 1);
        Assert.True(order.TotalAmount == 500);
        Assert.True(order.Products == null);

        await repository.BeginTransactionAsync();
        count = await repository.Create<Order>()
            .WithBy(new
            {
                Id = "9",
                TenantId = "3",
                OrderNo = "ON-001",
                BuyerId = 1,
                SellerId = 2,
                BuyerSource = buyerSource,
                TotalAmount = 600,
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
            .OnDuplicateKeyUpdate(x => x
                .Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
                .Set(true, f => f.Products, f => x.Values(f.Products)))
            .ExecuteAsync();
        order = await repository.GetAsync<Order>("9");
        await repository.CommitAsync();
        Assert.True(count == 2);
        Assert.True(order.TotalAmount == 600);
        Assert.True(new JsonTypeHandler().ToFieldValue(null, null, order.Products).ToString() == new JsonTypeHandler().ToFieldValue(null, null, new List<int> { 1, 2 }).ToString());
    }
    [Fact]
    public async void Insert_BulkCopy()
    {
        using var repository = dbFactory.Create();
        var orders = new List<Order>();
        for (int i = 1000; i < 2000; i++)
        {
            orders.Add(new Order
            {
                Id = $"ON_{i + 1}",
                TenantId = "3",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 1,
                SellerId = 2,
                TotalAmount = 500,
                Products = new List<int> { 1, 2 },
                Disputes = new Dispute
                {
                    Id = i + 1,
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
            });
        }
        var removeIds = orders.Select(f => f.Id).ToList();
        await repository.BeginTransactionAsync();
        await repository.Delete<Order>()
            .Where(f => removeIds.Contains(f.Id))
            .ExecuteAsync();
        var count = await repository.Create<Order>()
            .WithBulkCopy(orders)
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.True(count == orders.Count);
    }
}
