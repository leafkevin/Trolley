using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Trolley.SqlServer;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.SqlServer;

public class UnitTest1 : UnitTestBase
{
    private readonly ITestOutputHelper output;
    public UnitTest1()
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
                        this.output.WriteLine($"Connection {evt.ConnectionId} Created, Total:{Volatile.Read(ref connTotal)}");
                    };
                    df.OnConnectionOpened += evt =>
                    {
                        Interlocked.Increment(ref connOpenTotal);
                        this.output.WriteLine($"Connection {evt.ConnectionId} Opened, Total:{Volatile.Read(ref connOpenTotal)}");
                    };
                    df.OnConnectionClosed += evt =>
                    {
                        Interlocked.Decrement(ref connOpenTotal);
                        Interlocked.Decrement(ref connTotal);
                        this.output.WriteLine($"Connection {evt.ConnectionId} Closed, Total:{Volatile.Read(ref connOpenTotal)}");
                    };
                    df.OnCommandExecuting += evt =>
                    {
                        this.output.WriteLine($"{evt.SqlType} Begin, TransactionId:{evt.TransactionId} Sql: {evt.Sql}, Parameters: {evt.DbParameters.ToSqlServerParametersString()}");
                    };
                    df.OnCommandExecuted += evt =>
                    {
                        this.output.WriteLine($"{evt.SqlType} End, TransactionId:{evt.TransactionId} Elapsed: {evt.Elapsed} ms, Sql: {evt.Sql}, Parameters: {evt.DbParameters.ToSqlServerParametersString()}");
                    };
                    df.OnTransactionCreated += evt =>
                    {
                        Interlocked.Increment(ref tranTotal);
                        this.output.WriteLine($"Transaction {evt.TransactionId} Created, Total:{Volatile.Read(ref tranTotal)}");
                    };
                    df.OnTransactionCompleted += evt =>
                    {
                        Interlocked.Decrement(ref tranTotal);
                        this.output.WriteLine($"Transaction {evt.TransactionId} {evt.Action} Completed, Transaction Total:{Volatile.Read(ref tranTotal)}");
                    };
                });
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Test]
    public async Task Insert_Parameter()
    {
        var repository = this.dbFactory.Create();
        repository.Query<User>(f => f.Id == 4);
        repository.BeginTransaction();
        var count = repository.Delete<User>().Where(f => f.Id == 4).Execute();
        count = await repository.CreateAsync<User>(new
        {
            id = 4,
            tenantId = "1",
            name = "leafkevin",
            age = 25,
            companyId = 1,
            gender = Gender.Male,
            isEnabled = true,
            createdAt = DateTime.Now,
            createdBy = 1,
            updatedAt = DateTime.Now,
            updatedBy = 1,
#if NET6_0_OR_GREATER
            someTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(35)),
#else
            someTimes = TimeSpan.FromSeconds(35),
#endif
            guidField = Guid.NewGuid()
        });
        repository.Commit();
        Assert.AreEqual(1, count);
        repository.Delete<User>().Where(f => f.Id == 5).Execute();
        count = await repository.CreateAsync<User>(new Dictionary<string, object>()
        {
            {"id" , 5},
            {"tenantId" , "1"},
            {"name" , "leafkevin"},
            {"age" , 25},
            {"companyId" , 1},
            {"gender" , Gender.Male},
            {"isEnabled" , true},
            {"createdAt" , DateTime.Now},
            {"createdBy" , 1},
            {"updatedAt" , DateTime.Now},
            {"updatedBy" , 1},
#if NET6_0_OR_GREATER
            {"someTimes" , TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(35))},
#else
            {"someTimes" , TimeSpan.FromMinutes(35)},
#endif
            {"guidField" , Guid.NewGuid()}
        });
        Assert.AreEqual(1, count);
    }
    [Test]
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
        Assert.AreEqual(1, count);
    }
    [Test]
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
        Assert.AreEqual(3, count);
    }
    [Test]
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
        Assert.AreEqual("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Gender],[Age],[CompanyId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.AreEqual(1, (int)dbParameters[0].Value);
        Assert.AreEqual("1", (string)dbParameters[1].Value);
        Assert.AreEqual("leafkevin", (string)dbParameters[2].Value);
        if (dbParameters[3] is SqlParameter dbParameter)
        {
            Assert.AreEqual(SqlDbType.NVarChar, dbParameter.SqlDbType);
            Assert.IsTrue((string)dbParameter.Value == Gender.Male.ToString());
        }
        Assert.AreEqual(25, (int)dbParameters[4].Value);
        Assert.AreEqual(1, (int)dbParameters[5].Value);
        Assert.IsTrue((bool)dbParameters[6].Value);
        Assert.IsTrue((DateTime)dbParameters[7].Value == now);
        Assert.AreEqual(1, (int)dbParameters[8].Value);
        Assert.IsTrue((DateTime)dbParameters[9].Value == now);
        Assert.AreEqual(1, (int)dbParameters[10].Value);

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
        Assert.AreEqual("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.AreEqual(1, (int)dbParameters[0].Value);
        Assert.AreEqual("1", (string)dbParameters[1].Value);
        Assert.AreEqual("leafkevin", (string)dbParameters[2].Value);
        Assert.AreEqual(25, (int)dbParameters[3].Value);
        Assert.AreEqual(1, (int)dbParameters[4].Value);
        if (dbParameters[5] is SqlParameter dbParameter1)
        {
            Assert.AreEqual(SqlDbType.NVarChar, dbParameter1.SqlDbType);
            Assert.IsTrue((string)dbParameter1.Value == Gender.Male.ToString());
        }
        Assert.IsTrue((bool)dbParameters[6].Value);
        Assert.AreEqual(now, (DateTime)dbParameters[7].Value);
        Assert.AreEqual(1, (int)dbParameters[8].Value);
        Assert.AreEqual(now, (DateTime)dbParameters[9].Value);
        Assert.AreEqual(1, (int)dbParameters[10].Value);

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
        Assert.AreEqual(1, result);

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
        Assert.AreEqual(1, result);
    }
    [Test]
    public void Insert_WithBy_IgnoreFields()
    {
        var repository = this.dbFactory.Create();
        var now = DateTime.Now;
        var sql = repository.Create<User>()
           .WithBy(new
           {
               id = 1,
               tenantId = "1",
               name = "leafkevin",
               age = 25,
               companyId = 1,
               gender = Gender.Male,
               sourceType = UserSourceType.Douyin,
               isEnabled = true,
               createdAt = now,
               createdBy = 1,
               updatedAt = now,
               updatedBy = 1
           })
           .IgnoreFields("CompanyId", "sourceType")
           .ToSql(out var dbParameters);
        Assert.AreEqual("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Gender],[Age],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@Name,@Gender,@Age,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.AreEqual(10, dbParameters.Count);

        repository.BeginTransaction();
        repository.Delete<User>().Where(f => f.Id == 1).Execute();
        repository.Create<User>()
            .WithBy(new
            {
                id = 1,
                tenantId = "1",
                name = "leafkevin",
                age = 25,
                companyId = 1,
                gender = Gender.Male,
                sourceType = UserSourceType.Douyin,
                isEnabled = true,
                createdAt = now,
                createdBy = 1,
                updatedAt = now,
                updatedBy = 1
            })
            .IgnoreFields("CompanyId", "sourceType")
            .Execute();
        var user = repository.QueryById<User>(1);
        repository.Commit();
        Assert.AreEqual(Gender.Male, user.Gender);
        Assert.AreEqual(0, user.CompanyId);
        Assert.IsFalse(user.SourceType.HasValue);

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
        Assert.AreEqual("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Age],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@Name,@Age,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);

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
        user = repository.QueryById<User>(1);
        repository.Commit();
        Assert.AreEqual(Gender.Unknown, user.Gender);
        Assert.AreEqual(0, user.CompanyId);
    }
    [Test]
    public async Task Insert_WithBy_Condition()
    {
        this.Initialize(3);
        Guid? guidField = Guid.NewGuid();
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var user = repository.QueryById<User>(1);
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
        Assert.AreEqual("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Gender],[Age],[CompanyId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy],[SomeTimes],[GuidField]) VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@SomeTimes,@GuidField)", sql);

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
        Assert.AreEqual(1, count);
    }
    [Test]
    public async Task Insert_WithBy_AnonymousObject_Condition()
    {
        this.Initialize(3);
        Guid? guidField = Guid.NewGuid();
        var repository = this.dbFactory.Create();
        var user = repository.QueryById<User>(1);
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
        Assert.AreEqual("INSERT INTO [sys_user] ([Id],[Name],[Gender],[Age],[CompanyId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy],[GuidField]) VALUES (@Id,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@GuidField)", sql);

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
        Assert.AreEqual(1, count);
    }
    [Test]
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
        Assert.AreEqual(maxId, id);

        repository.BeginTransaction();
        await repository.Delete<Company>().Where(f => f.Id == id).ExecuteAsync();
        id = repository.Create<Company>()
            .WithBy(new Dictionary<string, object>()
            {
                    { "name","谷歌"},
                    { "isEnabled", true},
                    { "createdAt", DateTime.Now},
                    { "createdBy", 1},
                    { "updatedAt", DateTime.Now},
                    { "updatedBy", 1}
            })
            .ExecuteIdentity();
        maxId = repository.From<Company>().Max(f => f.Id);
        repository.Commit();
        Assert.AreEqual(maxId, id);
    }
    [Test]
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
        Assert.AreEqual("INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[BrandId],[CategoryId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)", sql);

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
        Assert.AreEqual(3, count);
    }
    [Test]
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
                    { "id",1 },
                    { "productNo","PN-001"},
                    { "name","波司登羽绒服"},
                    { "brandId",1},
                    { "categoryId",1},
                    { "isEnabled",true},
                    { "createdAt",DateTime.Now},
                    { "createdBy",1},
                    { "updatedAt",DateTime.Now},
                    { "updatedBy",1}
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
                    { "id",3},
                    { "productNo","PN-003"},
                    { "name","优衣库保暖内衣"},
                    { "brandId",3},
                    { "categoryId",3},
                    { "isEnabled",true},
                    { "createdAt",DateTime.Now},
                    { "createdBy",1},
                    { "updatedAt",DateTime.Now},
                    { "updatedBy",1}
                }
            })
            .Execute();
        repository.Commit();
        Assert.AreEqual(3, count);
    }
    [Test]
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
        Assert.AreEqual("INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id0,@ProductNo0,@Name0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)", sql);

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
        Assert.AreEqual(3, count);
        foreach (var product in products)
        {
            Assert.AreEqual(0, product.BrandId);
            Assert.AreEqual(0, product.CategoryId);
        }
    }
    [Test]
    public void Insert_Select_From_Table1()
    {
        var repository = this.dbFactory.Create();
        var id = 2;
        var brandId = 1;
        var name = "雪中飞羽绒裤";
        int categoryId = 1;
        var brand = repository.QueryById<Brand>(brandId);
        var sql = repository.Create<Product>()
            .From<Brand>()
            .Where(f => f.Id == brandId)
            .Select(f => new
            {
                Id = id,
                ProductNo = "PN_" + id.ToString().PadLeft(3, '0'),
                Name = name,
                Price = 25.85,
                BrandId = f.Id,
                CategoryId = categoryId,
                f.CompanyId,
                IsEnabled = true,
                CreatedBy = 1,
                CreatedAt = DateTime.Now,
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now
            })
            .ToSql(out _);
        Assert.AreEqual("INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[Price],[BrandId],[CategoryId],[CompanyId],[IsEnabled],[CreatedBy],[CreatedAt],[UpdatedBy],[UpdatedAt]) SELECT @p1,('PN_'+@p2),@p3,25.85,b.[Id],@p4,b.[CompanyId],1,1,GETDATE(),1,GETDATE() FROM [sys_brand] b WHERE b.[Id]=@p0", sql);

        repository.BeginTransaction();
        repository.Delete<Product>(id);
        var count = repository.Create<Product>()
            .From<Brand>()
            .Where(f => f.Id == brandId)
            .Select(f => new
            {
                Id = id,
                ProductNo = "PN_" + id.ToString().PadLeft(3, '0'),
                Name = name,
                Price = 25.85,
                BrandId = f.Id,
                CategoryId = categoryId,
                f.CompanyId,
                IsEnabled = true,
                CreatedBy = 1,
                CreatedAt = DateTime.Now,
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now
            })
           .Execute();
        var product = repository.QueryById<Product>(id);
        repository.Commit();
        Assert.IsTrue(count > 0);
        Assert.NotNull(product);
        Assert.IsTrue(product.ProductNo == "PN_" + id.ToString().PadLeft(3, '0'));
        Assert.IsTrue(product.Name == name);
        Assert.IsTrue(product.BrandId == brandId);
    }
    [Test]
    public async Task Insert_Select_From_Table2()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.Create<OrderDetail>()
            .From<Order, Product>()
            .Where((a, b) => a.Id == "3" && b.Id == 1)
            .Select((x, y) => new
            {
                id = "7",
                tenantId = "1",
                orderId = x.Id,
                productId = y.Id,
                price = y.Price,
                quantity = 3,
                amount = y.Price * 3,
                isEnabled = x.IsEnabled,
                createdBy = x.CreatedBy,
                createdAt = x.CreatedAt,
                updatedBy = x.UpdatedBy,
                updatedAt = x.UpdatedAt
            })
            .ToSql(out var parameters);
        Assert.AreEqual("INSERT INTO [sys_order_detail] ([Id],[TenantId],[OrderId],[ProductId],[Price],[Quantity],[Amount],[IsEnabled],[CreatedBy],[CreatedAt],[UpdatedBy],[UpdatedAt]) SELECT N'7',N'1',b.[Id],c.[Id],c.[Price],3,(c.[Price]*3),b.[IsEnabled],b.[CreatedBy],b.[CreatedAt],b.[UpdatedBy],b.[UpdatedAt] FROM [sys_order] b,[sys_product] c WHERE b.[Id]=N'3' AND c.[Id]=1", sql);
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
        var orderDetail = repository.QueryById<OrderDetail>("7");
        var product = repository.QueryById<Product>(1);
        await repository.CommitAsync();
        Assert.IsTrue(result > 0);
        Assert.NotNull(orderDetail);
        Assert.AreEqual("3", orderDetail.OrderId);
        Assert.AreEqual(1, orderDetail.ProductId);
        Assert.AreEqual(product.Price * 3, orderDetail.Amount);
    }
    [Test]
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
        Assert.AreEqual("WITH [orders]([Id],[TenantId],[OrderNo],[BuyerId],[SellerId],[BuyerSource],[ProductCount],[TotalAmount],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) AS \r\n(\r\nSELECT a.[OrderId],N'1',('ON-'+a.[OrderId]),1,1,N'Taobao',2,SUM(a.[Amount]),1,GETDATE(),1,GETDATE(),1 FROM [sys_order_detail] a GROUP BY a.[OrderId]\r\n)\r\nINSERT INTO [sys_order] ([Id],[TenantId],[OrderNo],[BuyerId],[SellerId],[BuyerSource],[ProductCount],[TotalAmount],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) SELECT b.[Id],b.[TenantId],b.[OrderNo],b.[BuyerId],b.[SellerId],b.[BuyerSource],b.[ProductCount],b.[TotalAmount],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy] FROM [orders] b", sql);
        var orderIds = ordersQuery.Select(f => f.Id).ToList();
        await repository.BeginTransactionAsync();
        repository.Delete<Order>(orderIds);
        var result = await repository.Create<Order>()
            .From(ordersQuery)
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(orderIds.Count, result);

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
        Assert.AreEqual("INSERT INTO [sys_order] ([Id],[TenantId],[OrderNo],[BuyerId],[SellerId],[BuyerSource],[ProductCount],[TotalAmount],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) SELECT b.[OrderId],N'1',('ON-'+b.[OrderId]),1,1,N'Taobao',2,SUM(b.[Amount]),1,GETDATE(),1,GETDATE(),1 FROM [sys_order_detail] b GROUP BY b.[OrderId]", sql);
        await repository.BeginTransactionAsync();
        repository.Delete<Order>(orderIds);
        result = await repository.Create<Order>()
            .From(ordersQuery)
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(orderIds.Count, result);
    }
    [Test]
    public async Task Insert_Select_From_SubQuery_Output()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.Create<Order>()
            .From<OrderDetail>()
            .Where(f => f.Id.Length < 10)
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
            .Output<OrderInfo>("INSERTED.[BuyerId],INSERTED.[TotalAmount]")
            .ToSql(out var parameters);
        Assert.AreEqual("INSERT INTO [sys_order] ([Id],[TenantId],[OrderNo],[BuyerId],[SellerId],[BuyerSource],[ProductCount],[TotalAmount],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.[BuyerId],INSERTED.[TotalAmount] SELECT b.[OrderId],N'1',('ON-'+b.[OrderId]),1,1,N'Taobao',2,SUM(b.[Amount]),1,GETDATE(),1,GETDATE(),1 FROM [sys_order_detail] b WHERE LEN(b.[Id])<10 GROUP BY b.[OrderId]", sql);
        await repository.BeginTransactionAsync();
        repository.Delete<Order>(f => f.Id.Length < 10);
        var result = await repository.Create<Order>()
            .From<OrderDetail>()
            .Where(f => f.Id.Length < 10)
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
            .Output<OrderInfo>("INSERTED.[BuyerId],INSERTED.[TotalAmount]")
            .ExecuteAsync();
        await repository.CommitAsync();
       Assert.Greater(result);
        Assert.Null(result[0].OrderNo);
    }
    [Test]
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
        var result = repository.QueryById<Order>("1");
        repository.Commit();
        if (count > 0)
        {
            Assert.IsFalse(result.ProductCount.HasValue);
        }
    }
    [Test]
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
        Assert.AreEqual("INSERT INTO [sys_order] ([Id],[TenantId],[OrderNo],[ProductCount],[TotalAmount],[BuyerId],[BuyerSource],[SellerId],[Products],[Disputes],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@OrderNo,@ProductCount,@TotalAmount,@BuyerId,@BuyerSource,@SellerId,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.AreEqual("@BuyerSource", parameters[6].ParameterName);
        Assert.IsTrue(parameters[3].Value is DBNull);
        Assert.IsTrue(parameters[6].Value is DBNull);
        Assert.AreEqual("@Products", parameters[8].ParameterName);
        Assert.IsTrue((string)parameters[8].Value == new JsonTypeHandler().ToFieldValue(new List<int> { 1, 2 }).ToString());
        Assert.AreEqual("@Disputes", parameters[9].ParameterName);
        Assert.IsTrue((string)parameters[9].Value == new JsonTypeHandler().ToFieldValue(dispute).ToString());

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
        var order = repository.QueryById<Order>("4");
        repository.Commit();
       Assert.Greater(order.Products);
        Assert.NotNull(order.Disputes);
        Assert.IsTrue(new JsonTypeHandler().ToFieldValue(order.Products).ToString() == new JsonTypeHandler().ToFieldValue(new List<int> { 1, 2 }).ToString());
        Assert.IsTrue(new JsonTypeHandler().ToFieldValue(order.Disputes).ToString() == new JsonTypeHandler().ToFieldValue(dispute).ToString());
    }
    [Test]
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
        Assert.AreEqual("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Gender],[Age],[CompanyId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql1);
        Assert.AreEqual("@Gender", parameters1[3].ParameterName);
        Assert.IsTrue(parameters1[3].Value.GetType() == typeof(string));
        Assert.IsTrue((string)parameters1[3].Value == Gender.Male.ToString());

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
        Assert.AreEqual("INSERT INTO [sys_company] ([Name],[Nature],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Name,@Nature,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql2);
        Assert.AreEqual("@Nature", parameters2[1].ParameterName);
        Assert.IsTrue(parameters2[1].Value.GetType() == typeof(string));
        Assert.IsTrue((string)parameters2[1].Value == CompanyNature.Internet.ToString());
    }
    [Test]
    public async Task Insert_OnlyFields()
    {
        this.Initialize(3);
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
        Assert.AreEqual("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@Id,@TenantId,@Name,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.AreEqual(8, parameters.Count);
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
        var user = repository.QueryById<User>(1);
        repository.Commit();
        Assert.AreEqual(1, count);
        Assert.AreEqual(0, user.CompanyId);
        Assert.AreEqual(Gender.Unknown, user.Gender);
    }
    [Test]
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
            .Output(f => new { f.Id, f.TenantId, Info = $"{f.Gender}-{f.Age}-{f.Name.ToUpper()}" })
            .ToSql(out var parameters1);
        Assert.AreEqual("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Gender],[Age],[CompanyId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.[Id],INSERTED.[TenantId],(INSERTED.[Gender]+'-'+CAST(INSERTED.[Age] AS NVARCHAR(MAX))+'-'+UPPER(INSERTED.[Name])) AS [Info] VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql1);
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
            .Output(f => new { f.Id, f.TenantId, Info = $"{f.Gender}-{f.Age}-{f.Name.ToUpper()}" })
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(1, result1.Id);
        Assert.AreEqual("1", result1.TenantId);
        Assert.AreEqual($"{Gender.Male}-{25}-{"leafkevin".ToUpper()}", result1.Info);

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
        Assert.AreEqual("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Gender],[Age],[CompanyId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.* VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql2);
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
        Assert.AreEqual(2, result2.Id);
        Assert.AreEqual("1", result2.TenantId);

        var sql3 = repository.Create<User>()
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
            .Output<User>("INSERTED.[Id],INSERTED.[TenantId],CONCAT(INSERTED.[Gender],'-',CAST(INSERTED.[Age] AS NVARCHAR(MAX)),'-',UPPER(INSERTED.[Name])) AS Name")
            .ToSql(out _);
        Assert.AreEqual("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Gender],[Age],[CompanyId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.[Id],INSERTED.[TenantId],CONCAT(INSERTED.[Gender],'-',CAST(INSERTED.[Age] AS NVARCHAR(MAX)),'-',UPPER(INSERTED.[Name])) AS Name VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql3);
        await repository.BeginTransactionAsync();
        await repository.DeleteAsync<User>(1);
        var result3 = await repository.Create<User>()
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
            .Output<User>("INSERTED.[Id],INSERTED.[TenantId],CONCAT(INSERTED.[Gender],'-',CAST(INSERTED.[Age] AS NVARCHAR(MAX)),'-',UPPER(INSERTED.[Name])) AS Name")
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(1, result3.Id);
        Assert.AreEqual("1", result3.TenantId);
        Assert.AreEqual($"{Gender.Male}-{25}-{"leafkevin".ToUpper()}", result3.Name);
        Assert.Null(result3.SourceType);
        await repository.BeginTransactionAsync();
        await repository.DeleteAsync<User>(1);
        var result4 = await repository.Create<User>()
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
            .Output(f => f)
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(1, result4.Id);
        Assert.AreEqual("1", result4.TenantId);
        Assert.AreEqual("leafkevin", result4.Name);
        Assert.Null(result4.SourceType);
    }
    [Test]
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
        Assert.AreEqual("INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[BrandId],[CategoryId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.[Id],INSERTED.[ProductNo] VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)", sql1);

        await repository.BeginTransactionAsync();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var results1 = await repository.Create<Product>()
            .WithBulk(products)
            .Output(f => new { f.Id, f.ProductNo })
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(3, results1.Count);
        for (int i = 0; i < results1.Count; i++)
        {
            Assert.AreEqual(products[i].Id, results1[i].Id);
            Assert.AreEqual(products[i].ProductNo, results1[i].ProductNo);
        }

        var sql2 = repository.Create<Product>()
            .WithBulk(products)
            .Output<Product>("*")
            .ToSql(out var parameters2);
        Assert.AreEqual("INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[BrandId],[CategoryId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.* VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)", sql2);

        await repository.BeginTransactionAsync();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var result2 = await repository.Create<Product>()
            .WithBulk(products)
            .Output<Product>("*")
            .ExecuteAsync();
        await repository.CommitAsync();
        for (int i = 0; i < result2.Count; i++)
        {
            Assert.AreEqual(products[i].Id, result2[i].Id);
            Assert.AreEqual(products[i].ProductNo, result2[i].ProductNo);
        }
    }
    [Test]
    public async Task Insert_Output_RawSql()
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
            .Output<UserInfo>("INSERTED.[Age],INSERTED.[Id]")
            .ToSql(out _);
        Assert.AreEqual("INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Gender],[Age],[CompanyId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.[Age],INSERTED.[Id] VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql1);
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
            .Output<UserInfo>("INSERTED.[Age],INSERTED.[Id]")
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(1, result1.Id);
        Assert.Null(result1.Name);
        Assert.AreEqual(25, result1.Age);
        Assert.AreEqual(Gender.Unknown, result1.Gender);
    }
    [Test]
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
        Assert.AreEqual(orders.Count, count);
    }
    [Test]
    public async Task BitArrayTest()
    {
        var repository = this.dbFactory.Create();
        var timeSpan = TimeSpan.FromMinutes(455);
        await repository.BeginTransactionAsync();
        await repository.DeleteAsync<UpdateEntity3>(1);
        byte[] bytes = [68];
        await repository.CreateAsync<UpdateEntity3>(new UpdateEntity3
        {
            Id = 1,
            BooleanField = true,
            TimeSpanField = TimeSpan.FromSeconds(456),
#if NET6_0_OR_GREATER
            DateOnlyField = new DateOnly(2022, 05, 06),
#else
            DateOnlyField = new DateTime(2022, 05, 06),
#endif
            DateTimeField = DateTime.Now,
            DateTimeOffsetField = new DateTimeOffset(DateTime.Parse("2022-01-02 03:04:05")).ToUniversalTime(),
            EnumField = Gender.Male,
            GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
            TimeOnlyField = new TimeOnly(3, 5, 7),
#else
            TimeOnlyField = new TimeSpan(3, 5, 7),
#endif
            ByteArrayField = Encoding.ASCII.GetBytes("ByteArry"),
            BitArrayField = Encoding.ASCII.GetBytes("BitArray")
        });
        var entity = await repository.QueryByIdAsync<UpdateEntity3>(1);
        await repository.CommitAsync();
        Assert.AreEqual(entity.ByteArrayField, Encoding.ASCII.GetBytes("ByteArry"));
        Assert.AreEqual(entity.BitArrayField, Encoding.ASCII.GetBytes("BitArray"));
    }
}