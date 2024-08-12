using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Trolley.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.PostgreSql;

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
                .Register(OrmProviderType.PostgreSql, "fengling", "Host=localhost;Database=fengling;Username=postgres;Password=123456;SearchPath=public", true, "public")
                .Configure<ModelConfiguration>(OrmProviderType.PostgreSql)
                .UseDbFilter(df =>
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

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
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
        var rawSql = "INSERT INTO sys_brand(\"Id\",\"BrandNo\",\"Name\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@BrandNo,@Name,'t',NOW(),@User,NOW(),@User)";
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
    public void Insert_WithBy()
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
                IsEnabled = true,
                CreatedAt = now,
                CreatedBy = 1,
                UpdatedAt = now,
                UpdatedBy = 1
            })
            .ToSql(out var dbParameters);
        Assert.True(sql == "INSERT INTO \"sys_user\" (\"Id\",\"TenantId\",\"Name\",\"Age\",\"CompanyId\",\"Gender\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
        Assert.True((int)dbParameters[0].Value == 1);
        Assert.True((string)dbParameters[1].Value == "1");
        Assert.True((string)dbParameters[2].Value == "leafkevin");
        Assert.True((int)dbParameters[3].Value == 25);
        Assert.True((int)dbParameters[4].Value == 1);
        if (dbParameters[5] is NpgsqlParameter dbParameter)
        {
            Assert.True(dbParameter.NpgsqlDbType == NpgsqlDbType.Varchar);
            Assert.True((string)dbParameter.Value == Gender.Male.ToString());
        }
        Assert.True((bool)dbParameters[6].Value == true);
        Assert.True((DateTime)dbParameters[7].Value == now);
        Assert.True((int)dbParameters[8].Value == 1);
        Assert.True((DateTime)dbParameters[9].Value == now);
        Assert.True((int)dbParameters[10].Value == 1);

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
        Assert.True(sql == "INSERT INTO \"sys_user\" (\"Id\",\"TenantId\",\"Name\",\"Age\",\"CompanyId\",\"Gender\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
        Assert.True((int)dbParameters[0].Value == 1);
        Assert.True((string)dbParameters[1].Value == "1");
        Assert.True((string)dbParameters[2].Value == "leafkevin");
        Assert.True((int)dbParameters[3].Value == 25);
        Assert.True((int)dbParameters[4].Value == 1);
        if (dbParameters[5] is NpgsqlParameter dbParameter1)
        {
            Assert.True(dbParameter1.NpgsqlDbType == NpgsqlDbType.Varchar);
            Assert.True((string)dbParameter1.Value == Gender.Male.ToString());
        }
        Assert.True((bool)dbParameters[6].Value == true);
        Assert.True((DateTime)dbParameters[7].Value == now);
        Assert.True((int)dbParameters[8].Value == 1);
        Assert.True((DateTime)dbParameters[9].Value == now);
        Assert.True((int)dbParameters[10].Value == 1);

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
        Assert.True(sql == "INSERT INTO \"sys_user\" (\"Id\",\"TenantId\",\"Name\",\"Age\",\"Gender\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@TenantId,@Name,@Age,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
        Assert.True(dbParameters.Count == 10);

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
        Assert.True(sql == "INSERT INTO \"sys_user\" (\"Id\",\"TenantId\",\"Name\",\"Age\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@TenantId,@Name,@Age,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");

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
        Assert.True(sql == "INSERT INTO \"sys_user\" (\"Id\",\"TenantId\",\"Name\",\"Age\",\"CompanyId\",\"Gender\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\",\"SomeTimes\",\"GuidField\") VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@SomeTimes,@GuidField)");

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
    public async void Insert_WithBy_AnonymousObject_Condition()
    {
        this.Initialize();
        Guid? guidField = Guid.NewGuid();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "INSERT INTO \"sys_user\" (\"Id\",\"Name\",\"Age\",\"CompanyId\",\"Gender\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\",\"GuidField\") VALUES (@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@GuidField)");

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
    public async void Insert_WithBy_AutoIncrement()
    {
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "INSERT INTO \"sys_product\" (\"Id\",\"ProductNo\",\"Name\",\"BrandId\",\"CategoryId\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)");

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
        Assert.True(sql == "INSERT INTO \"sys_product\" (\"Id\",\"ProductNo\",\"Name\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id0,@ProductNo0,@Name0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)");

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
    public void Insert_Select_From_Table1()
    {
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "INSERT INTO \"sys_product\" (\"Id\",\"ProductNo\",\"Name\",\"Price\",\"BrandId\",\"CategoryId\",\"CompanyId\",\"IsEnabled\",\"CreatedBy\",\"CreatedAt\",\"UpdatedBy\",\"UpdatedAt\") SELECT @p1,CONCAT('PN_',@p2),@p3,25.85,b.\"Id\",@p4,b.\"CompanyId\",TRUE,1,CURRENT_TIMESTAMP,1,CURRENT_TIMESTAMP FROM \"sys_brand\" b WHERE b.\"Id\"=@p0");

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
    public async void Insert_Select_From_Table2()
    {
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "INSERT INTO \"sys_order_detail\" (\"Id\",\"TenantId\",\"OrderId\",\"ProductId\",\"Price\",\"Quantity\",\"Amount\",\"IsEnabled\",\"CreatedBy\",\"CreatedAt\",\"UpdatedBy\",\"UpdatedAt\") SELECT '7','1',b.\"Id\",c.\"Id\",c.\"Price\",3,(c.\"Price\"*3),b.\"IsEnabled\",b.\"CreatedBy\",b.\"CreatedAt\",b.\"UpdatedBy\",b.\"UpdatedAt\" FROM \"sys_order\" b,\"sys_product\" c WHERE b.\"Id\"='3' AND c.\"Id\"=1");
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
        Assert.True(orderDetail.OrderId == "3");
        Assert.True(orderDetail.ProductId == 1);
        Assert.True(orderDetail.Amount == product.Price * 3);
    }
    [Fact]
    public async void Insert_Select_From_SubQuery()
    {
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "WITH \"orders\"(\"Id\",\"TenantId\",\"OrderNo\",\"BuyerId\",\"SellerId\",\"BuyerSource\",\"ProductCount\",\"TotalAmount\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") AS \r\n(\r\nSELECT a.\"OrderId\",'1',CONCAT('ON-',a.\"OrderId\"),1,1,'Taobao',2,SUM(a.\"Amount\"),TRUE,CURRENT_TIMESTAMP,1,CURRENT_TIMESTAMP,1 FROM \"sys_order_detail\" a GROUP BY a.\"OrderId\"\r\n)\r\nINSERT INTO \"sys_order\" (\"Id\",\"TenantId\",\"OrderNo\",\"BuyerId\",\"SellerId\",\"BuyerSource\",\"ProductCount\",\"TotalAmount\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") SELECT b.\"Id\",b.\"TenantId\",b.\"OrderNo\",b.\"BuyerId\",b.\"SellerId\",b.\"BuyerSource\",b.\"ProductCount\",b.\"TotalAmount\",b.\"IsEnabled\",b.\"CreatedAt\",b.\"CreatedBy\",b.\"UpdatedAt\",b.\"UpdatedBy\" FROM \"orders\" b");
        var orderIds = ordersQuery.Select(f => f.Id).ToList();
        await repository.BeginTransactionAsync();
        repository.Delete<Order>(orderIds);
        var result = await repository.Create<Order>()
            .From(ordersQuery)
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.True(result == orderIds.Count);

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
        Assert.True(sql == "INSERT INTO \"sys_order\" (\"Id\",\"TenantId\",\"OrderNo\",\"BuyerId\",\"SellerId\",\"BuyerSource\",\"ProductCount\",\"TotalAmount\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") SELECT b.\"OrderId\",'1',CONCAT('ON-',b.\"OrderId\"),1,1,'Taobao',2,SUM(b.\"Amount\"),TRUE,CURRENT_TIMESTAMP,1,CURRENT_TIMESTAMP,1 FROM \"sys_order_detail\" b GROUP BY b.\"OrderId\"");
        await repository.BeginTransactionAsync();
        repository.Delete<Order>(orderIds);
        result = await repository.Create<Order>()
            .From(ordersQuery)
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.True(result == orderIds.Count);
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
        Assert.True(sql == "INSERT INTO \"sys_order\" (\"Id\",\"TenantId\",\"OrderNo\",\"TotalAmount\",\"BuyerId\",\"BuyerSource\",\"SellerId\",\"ProductCount\",\"Products\",\"Disputes\",\"IsEnabled\",\"CreatedBy\",\"CreatedAt\",\"UpdatedBy\",\"UpdatedAt\") VALUES (@Id,@TenantId,@OrderNo,@TotalAmount,@BuyerId,@BuyerSource,@SellerId,@ProductCount,@Products,@Disputes,@IsEnabled,@CreatedBy,@CreatedAt,@UpdatedBy,@UpdatedAt)");
        Assert.True(parameters[5].ParameterName == "@BuyerSource");
        Assert.True(parameters[5].Value is DBNull);
        Assert.True(parameters[8].ParameterName == "@Products");
        Assert.True((string)parameters[8].Value == new JsonTypeHandler().ToFieldValue(null, new List<int> { 1, 2 }).ToString());
        Assert.True(parameters[9].ParameterName == "@Disputes");
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
        Assert.True(sql1 == "INSERT INTO \"sys_user\" (\"Id\",\"TenantId\",\"Name\",\"Age\",\"CompanyId\",\"Gender\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
        Assert.True(parameters1[5].ParameterName == "@Gender");
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
        Assert.True(sql2 == "INSERT INTO \"sys_company\" (\"Name\",\"Nature\",\"IsEnabled\",\"CreatedBy\",\"CreatedAt\",\"UpdatedBy\",\"UpdatedAt\") VALUES (@Name,@Nature,@IsEnabled,@CreatedBy,@CreatedAt,@UpdatedBy,@UpdatedAt)");
        Assert.True(parameters2[1].ParameterName == "@Nature");
        Assert.True(parameters2[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters2[1].Value == CompanyNature.Internet.ToString());
    }
    [Fact]
    public async void Insert_OnConflictDoNothing()
    {
        this.Initialize();
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
            .OnConflict(f => f.DoNothing())
            .ToSql(out var parameters1);
        Assert.True(sql1 == "INSERT INTO \"sys_user\" (\"Id\",\"TenantId\",\"Name\",\"Age\",\"CompanyId\",\"Gender\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON CONFLICT DO NOTHING");
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
            .OnConflict(f => f.DoNothing())
            .ExecuteAsync();
        Assert.True(count == 0);
    }
    [Fact]
    public async void Insert_OnConflictDoNothing_OnlyFields()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
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
            .OnConflict(f => f.DoNothing())
            .ToSql(out var parameters);
        Assert.True(sql == "INSERT INTO \"sys_user\" (\"Id\",\"TenantId\",\"Name\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@TenantId,@Name,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON CONFLICT DO NOTHING");
        Assert.True(parameters.Count == 8);
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
            .OnConflict(f => f.DoNothing())
            .ExecuteAsync();
        var user = repository.Get<User>(1);
        Assert.True(count == 1);
        Assert.True(user.CompanyId == 0);
        Assert.True(user.Gender == Gender.Unknown);
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
            .OnlyFields(f => new { f.Id, f.TenantId, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
            .OnConflict(f => f.DoNothing())
            .ExecuteAsync();
        Assert.True(count == 0);
    }
    [Fact]
    public async void Insert_OnConflictDoUpdate()
    {
        using var repository = dbFactory.Create();
        UserSourceType? buyerSource = UserSourceType.Douyin;
        var products = new List<int> { 1, 2 };
        var sql1 = repository.Create<Order>()
             .WithBy(new
             {
                 Id = "9",
                 TenantId = "3",
                 OrderNo = "ON-001",
                 BuyerId = 1,
                 SellerId = 2,
                 TotalAmount = 500,
                 Products = products,
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
             .OnConflict(x => x.UseKeys()
                .Set(t => new
                {
                    TotalAmount = x.Excluded(t.TotalAmount),
                    Products = x.Excluded(t.Products)
                })
                .Set(buyerSource.HasValue, f => f.BuyerSource, buyerSource))
            .ToSql(out _);
        Assert.True(sql1 == "INSERT INTO \"sys_order\" (\"Id\",\"TenantId\",\"OrderNo\",\"BuyerId\",\"SellerId\",\"TotalAmount\",\"Products\",\"Disputes\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@TenantId,@OrderNo,@BuyerId,@SellerId,@TotalAmount,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON CONFLICT (\"Id\") DO UPDATE SET \"TotalAmount\"=EXCLUDED.\"TotalAmount\",\"Products\"=EXCLUDED.\"Products\",\"BuyerSource\"=@BuyerSource");

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
                 Products = products,
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
             .OnConflict(x => x.UseKeys()
                .Set(t => new
                {
                    TotalAmount = x.Excluded(t.TotalAmount),
                    Products = x.Excluded(t.Products)
                })
                .Set(buyerSource.HasValue, f => f.BuyerSource, buyerSource))
            .ExecuteAsync();
        var order = await repository.GetAsync<Order>("9");
        await repository.CommitAsync();
        Assert.True(count == 1);
        Assert.True(order.TotalAmount == 500);
        Assert.True(new JsonTypeHandler().ToFieldValue(null, order.Products).ToString() == new JsonTypeHandler().ToFieldValue(null, products).ToString());

        buyerSource = UserSourceType.Taobao;
        var sql2 = repository.Create<Order>()
            .WithBy(new
            {
                Id = "9",
                TenantId = "3",
                OrderNo = "ON-001",
                BuyerId = 1,
                SellerId = 2,
                TotalAmount = 500,
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
            .OnConflict(x => x.UseKeys()
                .Set(t => new
                {
                    TotalAmount = t.TotalAmount + x.Excluded(t.TotalAmount),
                    Products = x.Excluded(t.Products)
                })
                .Set(buyerSource.HasValue, f => f.BuyerSource, buyerSource))
            .ToSql(out _);
        Assert.True(sql2 == "INSERT INTO \"sys_order\" AS a (\"Id\",\"TenantId\",\"OrderNo\",\"BuyerId\",\"SellerId\",\"TotalAmount\",\"Disputes\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@TenantId,@OrderNo,@BuyerId,@SellerId,@TotalAmount,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON CONFLICT (\"Id\") DO UPDATE SET \"TotalAmount\"=a.\"TotalAmount\"+EXCLUDED.\"TotalAmount\",\"Products\"=EXCLUDED.\"Products\",\"BuyerSource\"=@BuyerSource");

        await repository.BeginTransactionAsync();
        count = await repository.Create<Order>()
             .WithBy(new
             {
                 Id = "9",
                 TenantId = "3",
                 OrderNo = "ON-001",
                 BuyerId = 1,
                 SellerId = 2,
                 TotalAmount = 500,
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
             .OnConflict(x => x.UseKeys()
                .Set(t => new
                {
                    TotalAmount = t.TotalAmount + x.Excluded(t.TotalAmount),
                    Products = x.Excluded(t.Products)
                })
                .Set(buyerSource.HasValue, f => f.BuyerSource, buyerSource))
            .ExecuteAsync();
        order = await repository.GetAsync<Order>("9");
        await repository.CommitAsync();
        Assert.True(count == 1);
        Assert.True(order.TotalAmount == 1000);
        Assert.True(order.Products == null);
        Assert.True(order.BuyerSource == buyerSource);

        var sql3 = repository.Create<Order>()
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
            .OnConflict(x => x.UseKeys()
                .Set(f => new { TotalAmount = f.TotalAmount + x.Excluded(f.TotalAmount) })
                .Set(f => f.Products, f => x.Excluded(f.Products)))
            .ToSql(out _);
        Assert.True(sql3 == "INSERT INTO \"sys_order\" AS a (\"Id\",\"TenantId\",\"OrderNo\",\"BuyerId\",\"SellerId\",\"BuyerSource\",\"TotalAmount\",\"Products\",\"Disputes\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@TenantId,@OrderNo,@BuyerId,@SellerId,@BuyerSource,@TotalAmount,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON CONFLICT (\"Id\") DO UPDATE SET \"TotalAmount\"=a.\"TotalAmount\"+EXCLUDED.\"TotalAmount\",\"Products\"=EXCLUDED.\"Products\"");

        await repository.BeginTransactionAsync();
        order = await repository.GetAsync<Order>("9");
        count = await repository.Create<Order>()
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
            .OnConflict(x => x.UseKeys()
                .Set(f => new { TotalAmount = f.TotalAmount + x.Excluded(f.TotalAmount) })
                .Set(f => f.Products, f => x.Excluded(f.Products)))
            .ExecuteAsync();
        var updatedOrder = await repository.GetAsync<Order>("9");
        await repository.CommitAsync();
        Assert.True(count == 1);
        Assert.True(updatedOrder.TotalAmount == order.TotalAmount + 500);
    }
    [Fact]
    public async void Insert_Returning()
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
            .Returning(f => new { f.Id, f.TenantId })
            .ToSql(out var parameters1);
        Assert.True(sql1 == "INSERT INTO \"sys_user\" (\"Id\",\"TenantId\",\"Name\",\"Age\",\"CompanyId\",\"Gender\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) RETURNING \"Id\",\"TenantId\"");
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
            .Returning(f => new { f.Id, f.TenantId })
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.True(result1.Id == 1);
        Assert.True(result1.TenantId == "1");

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
            .Returning<User>("*")
            .ToSql(out var parameters2);
        Assert.True(sql2 == "INSERT INTO \"sys_user\" (\"Id\",\"TenantId\",\"Name\",\"Age\",\"CompanyId\",\"Gender\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) RETURNING *");
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
            .Returning<User>("*")
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.True(result2.Id == 2);
        Assert.True(result2.TenantId == "1");
    }
    [Fact]
    public async void Insert_Returnings()
    {
        using var repository = dbFactory.Create();
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
            .Returning(f => new { f.Id, f.ProductNo })
            .ToSql(out var parameters1);
        Assert.True(sql1 == "INSERT INTO \"sys_product\" (\"Id\",\"ProductNo\",\"Name\",\"BrandId\",\"CategoryId\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2) RETURNING \"Id\",\"ProductNo\"");

        await repository.BeginTransactionAsync();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var results1 = await repository.Create<Product>()
            .WithBulk(products)
            .Returning(f => new { f.Id, f.ProductNo })
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.True(results1.Count == 3);
        for (int i = 0; i < results1.Count; i++)
        {
            Assert.True(results1[i].Id == products[i].Id);
            Assert.True(results1[i].ProductNo == products[i].ProductNo);
        }

        var sql2 = repository.Create<Product>()
            .WithBulk(products)
            .Returning<Product>("*")
            .ToSql(out var parameters2);
        Assert.True(sql2 == "INSERT INTO \"sys_product\" (\"Id\",\"ProductNo\",\"Name\",\"BrandId\",\"CategoryId\",\"IsEnabled\",\"CreatedAt\",\"CreatedBy\",\"UpdatedAt\",\"UpdatedBy\") VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2) RETURNING *");

        await repository.BeginTransactionAsync();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var result2 = await repository.Create<Product>()
            .WithBulk(products)
            .Returning<Product>("*")
            .ExecuteAsync();
        await repository.CommitAsync();
        for (int i = 0; i < result2.Count; i++)
        {
            Assert.True(result2[i].Id == products[i].Id);
            Assert.True(result2[i].ProductNo == products[i].ProductNo);
        }
    }
    [Fact]
    public async void Insert_BulkCopy()
    {
        using var repository = dbFactory.Create();
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
        Assert.True(count == orders.Count);
    }
}
