using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trolley.SqlServer;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.SqlServer;

public class UnitTest1 : UnitTestBase
{
    private static int connTotal = 0;
    private static int connOpenTotal = 0;
    private readonly ITestOutputHelper output;
    public UnitTest1(ITestOutputHelper output)
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
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async Task Insert_Parameter()
    {
        var repository = this.dbFactory.Create();
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
    public async Task Insert_RawSql()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Brand>().Where(new { Id = 1 }).Execute();
        var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES (@Id,@BrandNo,@Name,1,GETDATE(),@User,GETDATE(),@User)";
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
    public async Task Insert_Parameters()
    {
        var repository = this.dbFactory.Create();
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
    public void Insert_WithBy()
    {
        var repository = this.dbFactory.Create();
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
        Assert.Equal("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.Equal(1, (int)dbParameters[0].Value);
        Assert.Equal("1", (string)dbParameters[1].Value);
        Assert.Equal("leafkevin", (string)dbParameters[2].Value);
        Assert.Equal(25, (int)dbParameters[3].Value);
        Assert.Equal(1, (int)dbParameters[4].Value);
        if (dbParameters[5] is SqlParameter dbParameter)
        {
            Assert.Equal(SqlDbType.NVarChar, dbParameter.SqlDbType);
            Assert.True((string)dbParameter.Value == Gender.Male.ToString());
        }
        Assert.True((bool)dbParameters[6].Value);
        Assert.True((DateTime)dbParameters[7].Value == now);
        Assert.Equal(1, (int)dbParameters[8].Value);
        Assert.True((DateTime)dbParameters[9].Value == now);
        Assert.Equal(1, (int)dbParameters[10].Value);

        sql = repository.Create<User>()
            .WithBy(new Dictionary<string, object>
            {
                { "Id", 1 },
                { "TenantId", "1"},
                { "Name", "leafkevin"},
                { "Age", 25},
                { "CompanyId", 1},
                { "Gender", Gender.Male},
                { "IsEnabled", true},
                { "CreatedAt", now},
                { "CreatedBy", 1},
                { "UpdatedAt", now},
                { "UpdatedBy", 1}
            })
          .ToSql(out dbParameters);
        Assert.Equal("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.Equal(1, (int)dbParameters[0].Value);
        Assert.Equal("1", (string)dbParameters[1].Value);
        Assert.Equal("leafkevin", (string)dbParameters[2].Value);
        Assert.Equal(25, (int)dbParameters[3].Value);
        Assert.Equal(1, (int)dbParameters[4].Value);
        if (dbParameters[5] is SqlParameter dbParameter1)
        {
            Assert.Equal(SqlDbType.NVarChar, dbParameter1.SqlDbType);
            Assert.True((string)dbParameter1.Value == Gender.Male.ToString());
        }
        Assert.True((bool)dbParameters[6].Value);
        Assert.Equal(now, (DateTime)dbParameters[7].Value);
        Assert.Equal(1, (int)dbParameters[8].Value);
        Assert.Equal(now, (DateTime)dbParameters[9].Value);
        Assert.Equal(1, (int)dbParameters[10].Value);

        repository.BeginTransaction();
        var count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
        var result = repository.Create<User>()
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
            .Execute();
        repository.Commit();
        Assert.Equal(1, result);

        repository.BeginTransaction();
        count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
        result = repository.Create<User>()
            .WithBy(new Dictionary<string, object>
            {
                { "Id", 1 },
                { "TenantId", "1"},
                { "Name", "leafkevin"},
                { "Age", 25},
                { "CompanyId", 1},
                { "Gender", Gender.Male},
                { "IsEnabled", true},
                { "CreatedAt", now},
                { "CreatedBy", 1},
                { "UpdatedAt", now},
                { "UpdatedBy", 1}
            })
            .Execute();
        repository.Commit();
        Assert.Equal(1, result);
    }
    [Fact]
    public void Insert_WithBy_IgnoreFields()
    {
        var repository = this.dbFactory.Create();
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
        Assert.Equal("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Age],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@Name,@Age,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.Equal(10, dbParameters.Count);

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
        Assert.Equal(Gender.Male, user.Gender);
        Assert.Equal(0, user.CompanyId);
        Assert.False(user.SourceType.HasValue);

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
        Assert.Equal("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Age],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@Name,@Age,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);

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
        Assert.Equal(Gender.Unknown, user.Gender);
        Assert.Equal(0, user.CompanyId);
    }
    [Fact]
    public async Task Insert_WithBy_Condition()
    {
        this.Initialize();
        Guid? guidField = Guid.NewGuid();
        var repository = this.dbFactory.Create();
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
        Assert.Equal("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy],[SomeTimes],[GuidField]) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@SomeTimes,@GuidField)", sql);

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
    public async Task Insert_WithBy_AnonymousObject_Condition()
    {
        this.Initialize();
        Guid? guidField = Guid.NewGuid();
        var repository = this.dbFactory.Create();
        var user = repository.Get<User>(1);
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
        Assert.Equal("INSERT INTO [sys_user] ([Id],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy],[GuidField]) VALUES (@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@GuidField)", sql);

        repository.BeginTransaction();
        var count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
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
            }).ExecuteAsync();
        repository.Commit();
        Assert.Equal(1, count);
    }
    [Fact]
    public async Task Insert_WithBy_AutoIncrement()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var id = repository.CreateIdentity<Company>(new
        {
            Name = "微软",
            IsEnabled = true,
            CreatedAt = DateTime.Now,
            CreatedBy = 1,
            UpdatedAt = DateTime.Now,
            UpdatedBy = 1
        });
        var maxId = repository.From<Company>().Max(f => f.Id);
        repository.Commit();
        Assert.Equal(maxId, id);

        repository.BeginTransaction();
        await repository.Delete<Company>().Where(f => f.Id == id).ExecuteAsync();
        id = repository.Create<Company>()
            .WithBy(new Dictionary<string, object>()
            {
                    { "Name","谷歌"},
                    { "IsEnabled", true},
                    { "CreatedAt", DateTime.Now},
                    { "CreatedBy", 1},
                    { "UpdatedAt", DateTime.Now},
                    { "UpdatedBy", 1}
            })
            .ExecuteIdentity();
        maxId = repository.From<Company>().Max(f => f.Id);
        repository.Commit();
        Assert.Equal(maxId, id);
    }
    [Fact]
    public async Task Insert_WithBulk()
    {
        var repository = this.dbFactory.Create();
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
        Assert.Equal("INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[BrandId],[CategoryId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)", sql);

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
    public async Task Insert_WithBulk_Dictionaries()
    {
        var repository = this.dbFactory.Create();
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
    public async Task Insert_WithBulk_OnlyFields()
    {
        var repository = this.dbFactory.Create();
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
        Assert.Equal("INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id0,@ProductNo0,@Name0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)", sql);

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
            Assert.Equal(0, product.BrandId);
            Assert.Equal(0, product.CategoryId);
        }
    }
    [Fact]
    public void Insert_Select_From_Table1()
    {
        var repository = this.dbFactory.Create();
        var id = 2;
        var brandId = 1;
        var name = "雪中飞羽绒裤";
        int categoryId = 1;
        var brand = repository.Get<Brand>(brandId);
        var sql = repository.Create<Product>()
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
        Assert.Equal("INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[Price],[BrandId],[CategoryId],[CompanyId],[IsEnabled],[CreatedBy],[CreatedAt],[UpdatedBy],[UpdatedAt]) SELECT @p1,('PN_'+@p2),@p3,25.85,b.[Id],@p4,b.[CompanyId],1,1,GETDATE(),1,GETDATE() FROM [sys_brand] b WHERE b.[Id]=@p0", sql);

        repository.BeginTransaction();
        repository.Delete<Product>(id);
        var count = repository.Create<Product>()
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
    }
    [Fact]
    public async Task Insert_Select_From_Table2()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.Create<OrderDetail>()
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
        Assert.Equal("INSERT INTO [sys_order_detail] ([Id],[TenantId],[OrderId],[ProductId],[Price],[Quantity],[Amount],[IsEnabled],[CreatedBy],[CreatedAt],[UpdatedBy],[UpdatedAt]) SELECT N'7',N'1',b.[Id],c.[Id],c.[Price],3,(c.[Price]*3),b.[IsEnabled],b.[CreatedBy],b.[CreatedAt],b.[UpdatedBy],b.[UpdatedAt] FROM [sys_order] b,[sys_product] c WHERE b.[Id]=N'3' AND c.[Id]=1", sql);
        await repository.BeginTransactionAsync();
        repository.Delete<OrderDetail>("7");
        var result = await repository.Create<OrderDetail>()
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
        Assert.True(result > 0);
        Assert.NotNull(orderDetail);
        Assert.Equal("3", orderDetail.OrderId);
        Assert.Equal(1, orderDetail.ProductId);
        Assert.Equal(product.Price * 3, orderDetail.Amount);
    }
    [Fact]
    public async Task Insert_Select_From_SubQuery()
    {
        var repository = this.dbFactory.Create();
        var ordersQuery = repository.From<OrderDetail>()
            .GroupBy(f => f.OrderId)
            .Select((x, f) => new
            {
                Id = f.OrderId,
                TenantId = "1",
                OrderNo = $"ON-{f.OrderId}",
                BuyerId = 1,
                SellerId = 1,
                BuyerSource = UserSourceType.Taobao.ToString(),
                ProductCount = 2,
                TotalAmount = x.Sum(f.Amount),
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            })
            .AsCteTable("orders");
        var sql = repository.Create<Order>()
            .From(ordersQuery)
            .ToSql(out var parameters);
        Assert.Equal("WITH [orders]([Id],[TenantId],[OrderNo],[BuyerId],[SellerId],[BuyerSource],[ProductCount],[TotalAmount],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) AS \r\n(\r\nSELECT a.[OrderId],N'1',('ON-'+a.[OrderId]),1,1,N'Taobao',2,SUM(a.[Amount]),1,GETDATE(),1,GETDATE(),1 FROM [sys_order_detail] a GROUP BY a.[OrderId]\r\n)\r\nINSERT INTO [sys_order] ([Id],[TenantId],[OrderNo],[BuyerId],[SellerId],[BuyerSource],[ProductCount],[TotalAmount],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) SELECT b.[Id],b.[TenantId],b.[OrderNo],b.[BuyerId],b.[SellerId],b.[BuyerSource],b.[ProductCount],b.[TotalAmount],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy] FROM [orders] b", sql);
        var orderIds = ordersQuery.Select(f => f.Id).ToList();
        await repository.BeginTransactionAsync();
        repository.Delete<Order>(orderIds);
        var result = await repository.Create<Order>()
            .From(ordersQuery)
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.Equal(orderIds.Count, result);

        sql = repository.Create<Order>()
            .From<OrderDetail>()
            .GroupBy(f => f.OrderId)
            .Select((x, f) => new
            {
                Id = f.OrderId,
                TenantId = "1",
                OrderNo = $"ON-{f.OrderId}",
                BuyerId = 1,
                SellerId = 1,
                BuyerSource = UserSourceType.Taobao.ToString(),
                ProductCount = 2,
                TotalAmount = x.Sum(f.Amount),
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            })
            .ToSql(out parameters);
        Assert.Equal("INSERT INTO [sys_order] ([Id],[TenantId],[OrderNo],[BuyerId],[SellerId],[BuyerSource],[ProductCount],[TotalAmount],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) SELECT b.[OrderId],N'1',('ON-'+b.[OrderId]),1,1,N'Taobao',2,SUM(b.[Amount]),1,GETDATE(),1,GETDATE(),1 FROM [sys_order_detail] b GROUP BY b.[OrderId]", sql);
        await repository.BeginTransactionAsync();
        repository.Delete<Order>(orderIds);
        result = await repository.Create<Order>()
            .From(ordersQuery)
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.Equal(orderIds.Count, result);
    }
    [Fact]
    public void Insert_Null_Field()
    {
        var repository = this.dbFactory.Create();
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
            Assert.False(result.ProductCount.HasValue);
        }
    }
    [Fact]
    public void Insert_Json_Field()
    {
        var repository = this.dbFactory.Create();
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
        Assert.Equal("INSERT INTO [sys_order] ([Id],[TenantId],[OrderNo],[TotalAmount],[BuyerId],[BuyerSource],[SellerId],[ProductCount],[Products],[Disputes],[IsEnabled],[CreatedBy],[CreatedAt],[UpdatedBy],[UpdatedAt]) VALUES (@Id,@TenantId,@OrderNo,@TotalAmount,@BuyerId,@BuyerSource,@SellerId,@ProductCount,@Products,@Disputes,@IsEnabled,@CreatedBy,@CreatedAt,@UpdatedBy,@UpdatedAt)", sql);
        Assert.Equal("@BuyerSource", parameters[5].ParameterName);
        Assert.True(parameters[5].Value is DBNull);
        Assert.Equal("@Products", parameters[8].ParameterName);
        Assert.True((string)parameters[8].Value == new JsonTypeHandler().ToFieldValue(null, new List<int> { 1, 2 }).ToString());
        Assert.Equal("@Disputes", parameters[9].ParameterName);
        Assert.True((string)parameters[9].Value == new JsonTypeHandler().ToFieldValue(null, dispute).ToString());

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
        Assert.True(new JsonTypeHandler().ToFieldValue(null, order.Products).ToString() == new JsonTypeHandler().ToFieldValue(null, new List<int> { 1, 2 }).ToString());
        Assert.True(new JsonTypeHandler().ToFieldValue(null, order.Disputes).ToString() == new JsonTypeHandler().ToFieldValue(null, dispute).ToString());
    }
    [Fact]
    public void Insert_Enum_Fields()
    {
        var repository = this.dbFactory.Create();
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
        Assert.Equal("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql1);
        Assert.Equal("@Gender", parameters1[5].ParameterName);
        Assert.True(parameters1[5].Value.GetType() == typeof(string));
        Assert.True((string)parameters1[5].Value == Gender.Male.ToString());

        var sql2 = repository.Create<Company>()
             .WithBy(new Company
             {
                 Name = "leafkevin",
                 Nature = CompanyNature.Internet,
                 IsEnabled = true,
                 CreatedAt = DateTime.Now,
                 CreatedBy = 1,
                 UpdatedAt = DateTime.Now,
                 UpdatedBy = 1
             })
             .ToSql(out var parameters2);
        Assert.Equal("INSERT INTO [sys_company] ([Name],[Nature],[IsEnabled],[CreatedBy],[CreatedAt],[UpdatedBy],[UpdatedAt]) VALUES (@Name,@Nature,@IsEnabled,@CreatedBy,@CreatedAt,@UpdatedBy,@UpdatedAt)", sql2);
        Assert.Equal("@Nature", parameters2[1].ParameterName);
        Assert.True(parameters2[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters2[1].Value == CompanyNature.Internet.ToString());
    }
    [Fact]
    public async Task Insert_OnlyFields()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
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
            .OnlyFields(f => new { f.Id, f.TenantId, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
            .ToSql(out var parameters);
        Assert.Equal("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@Name,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.Equal(8, parameters.Count);
        repository.BeginTransaction();
        repository.Delete<User>(1);
        var count = await repository.Create<User>()
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
            .OnlyFields(f => new { f.Id, f.TenantId, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
            .ExecuteAsync();
        var user = repository.Get<User>(1);
        repository.Commit();
        Assert.Equal(1, count);
        Assert.Equal(0, user.CompanyId);
        Assert.Equal(Gender.Unknown, user.Gender);
    }
    [Fact]
    public async Task Insert_Output()
    {
        var repository = this.dbFactory.Create();
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
            .Output(f => new { f.Id, f.TenantId })
            .ToSql(out var parameters1);
        Assert.Equal("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.Id,INSERTED.TenantId VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql1);
        await repository.BeginTransactionAsync();
        await repository.DeleteAsync<User>(1);
        var result1 = await repository.Create<User>()
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
            .Output(f => new { f.Id, f.TenantId })
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.Equal(1, result1.Id);
        Assert.Equal("1", result1.TenantId);

        var sql2 = repository.Create<User>()
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
            .Output<User>("*")
            .ToSql(out var parameters2);
        Assert.Equal("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.* VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql2);
        await repository.BeginTransactionAsync();
        await repository.DeleteAsync<User>(2);
        var result2 = await repository.Create<User>()
            .WithBy(new
            {
                Id = 2,
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
            .Output<User>("*")
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.Equal(2, result2.Id);
        Assert.Equal("1", result2.TenantId);
    }
    [Fact]
    public async Task Insert_Outputs()
    {
        var repository = this.dbFactory.Create();
        var products = new[]
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
        };

        var sql1 = repository.Create<Product>()
            .WithBulk(products)
            .Output(f => new { f.Id, f.ProductNo })
            .ToSql(out var parameters1);
        Assert.Equal("INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[BrandId],[CategoryId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.Id,INSERTED.ProductNo VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)", sql1);

        await repository.BeginTransactionAsync();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var results1 = await repository.Create<Product>()
            .WithBulk(products)
            .Output(f => new { f.Id, f.ProductNo })
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.Equal(3, results1.Count);
        for (int i = 0; i < results1.Count; i++)
        {
            Assert.Equal(products[i].Id, results1[i].Id);
            Assert.Equal(products[i].ProductNo, results1[i].ProductNo);
        }

        var sql2 = repository.Create<Product>()
            .WithBulk(products)
            .Output<Product>("*")
            .ToSql(out var parameters2);
        Assert.Equal("INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[BrandId],[CategoryId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.* VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)", sql2);

        await repository.BeginTransactionAsync();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var result2 = await repository.Create<Product>()
            .WithBulk(products)
            .Output<Product>("*")
            .ExecuteAsync();
        await repository.CommitAsync();
        for (int i = 0; i < result2.Count; i++)
        {
            Assert.Equal(products[i].Id, result2[i].Id);
            Assert.Equal(products[i].ProductNo, result2[i].ProductNo);
        }
    }
    [Fact]
    public async Task Insert_BulkCopy()
    {
        var repository = this.dbFactory.Create();
        var orders = new List<dynamic>();
        for (int i = 1000; i < 2000; i++)
        {
            orders.Add(new
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
        Assert.Equal(orders.Count, count);
    }
}