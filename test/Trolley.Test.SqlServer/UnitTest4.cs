using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trolley.SqlServer;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.SqlServer;

public class UnitTest4 : UnitTestBase
{
    private static int connTotal = 0;
    private static int connOpenTotal = 0;
    private readonly ITestOutputHelper output;
    public UnitTest4(ITestOutputHelper output)
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
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public void IsEntityType()
    {
        Assert.False(typeof(Sex).IsEntityType(out _));
        Assert.False(typeof(Sex?).IsEntityType(out _));
        Assert.True(typeof(Studuent).IsEntityType(out _));
        Assert.False(typeof(string).IsEntityType(out _));
        Assert.False(typeof(int).IsEntityType(out _));
        Assert.False(typeof(int?).IsEntityType(out _));
        Assert.False(typeof(Guid).IsEntityType(out _));
        Assert.False(typeof(Guid?).IsEntityType(out _));
        Assert.False(typeof(DateTime).IsEntityType(out _));
        Assert.False(typeof(DateTime?).IsEntityType(out _));
        Assert.False(typeof(byte[]).IsEntityType(out _));
        Assert.False(typeof(int[]).IsEntityType(out _));
        Assert.False(typeof(List<int>).IsEntityType(out _));
        Assert.False(typeof(List<int[]>).IsEntityType(out _));
        Assert.False(typeof(Collection<string>).IsEntityType(out _));
        Assert.False(typeof(DBNull).IsEntityType(out _));

        var vt1 = ValueTuple.Create("kevin");
        Assert.False(vt1.GetType().IsEntityType(out _));
        var vt2 = ValueTuple.Create(1, "kevin", 25, 30000.00d);
        Assert.True(vt2.GetType().IsEntityType(out _));
        Assert.True(typeof((string Name, int Age)).IsEntityType(out _));
        Assert.True(typeof(Dictionary<string, int>).IsEntityType(out _));
        Assert.True(typeof(Studuent).IsEntityType(out _));
        Assert.True(typeof(Teacher).IsEntityType(out _));

        Assert.True(typeof(Dictionary<string, int>[]).IsEntityType(out _));
        Assert.True(typeof(List<Dictionary<string, int>>).IsEntityType(out _));
        Assert.True(typeof(List<Dictionary<string, int>[]>).IsEntityType(out _));
        Assert.True(typeof(Collection<Dictionary<string, int>>).IsEntityType(out _));
        Assert.True(typeof(Dictionary<string, Dictionary<string, int>>).IsEntityType(out _));

        Assert.True(typeof(Teacher[]).IsEntityType(out _));
        Assert.True(typeof(List<Teacher>).IsEntityType(out _));
        Assert.True(typeof(List<Teacher[]>).IsEntityType(out _));
        Assert.True(typeof(Collection<Teacher>).IsEntityType(out _));
        Assert.True(typeof(Dictionary<string, Teacher>).IsEntityType(out _));

        Assert.True(typeof(Studuent[]).IsEntityType(out _));
        Assert.True(typeof(List<Studuent>).IsEntityType(out _));
        Assert.True(typeof(List<Studuent[]>).IsEntityType(out _));
        Assert.True(typeof(Collection<Studuent>).IsEntityType(out _));
        Assert.True(typeof(Dictionary<string, Studuent>).IsEntityType(out _));
    }
    [Fact]
    public async Task Delete()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(f => f.Id == 1);
        var count = repository.Create<User>(new User
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
        });
        Assert.Equal(1, count);
        count = await repository.DeleteAsync<User>(f => f.Id == 1);
        repository.Commit();
        Assert.Equal(1, count);

        var sql = repository.Delete<User>()
            .Where(f => f.Id == 1)
            .ToSql(out var parameters);
        Assert.Equal("DELETE FROM [sys_user] WHERE [Id]=1", sql);
        //Assert.True((int)parameters[0].Value == 1);
    }
    [Fact]
    public async Task Delete_Multi()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(new[] { new { Id = 1 }, new { Id = 2 } });
        var count = repository.Create<User>(new[]
        {
            new User
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
            },
            new User
            {
                Id = 2,
                TenantId = "2",
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
        Assert.Equal(2, count);
        count = await repository.DeleteAsync<User>(new[] { new { Id = 1 }, new { Id = 2 } });
        repository.Commit();
        Assert.Equal(2, count);

        var sql = repository.Delete<User>()
            .Where(new[] { new { Id = 1 }, new { Id = 2 } })
            .ToSql(out var parameters);
        Assert.Equal("DELETE FROM [sys_user] WHERE [Id] IN (@Id0,@Id1)", sql);
        Assert.Equal(1, (int)parameters[0].Value);
        Assert.Equal(2, (int)parameters[1].Value);

        var sql1 = repository.Delete<Function>()
            .Where(new[] { new { MenuId = 1, PageId = 1 }, new { MenuId = 2, PageId = 2 } })
            .ToSql(out parameters);
        Assert.Equal("DELETE FROM [sys_function] WHERE [MenuId]=@MenuId0 AND [PageId]=@PageId0 OR [MenuId]=@MenuId1 AND [PageId]=@PageId1", sql1);
        Assert.Equal(4, parameters.Count);
        Assert.Equal(1, (int)parameters[0].Value);
        Assert.Equal(1, (int)parameters[1].Value);
        Assert.Equal(2, (int)parameters[2].Value);
        Assert.Equal(2, (int)parameters[3].Value);
    }
    [Fact]
    public async Task Delete_Multi1()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(new[] { 1, 2 });
        var count = repository.Create<User>(new[]
        {
            new User
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
            },
            new User
            {
                Id = 2,
                TenantId = "2",
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
        Assert.Equal(2, count);
        count = await repository.DeleteAsync<User>(new int[] { 1, 2 });
        repository.Commit();
        Assert.Equal(2, count);

        var sql = repository.Delete<User>()
            .Where(new int[] { 1, 2 })
            .ToSql(out var parameters);
        Assert.Equal("DELETE FROM [sys_user] WHERE [Id] IN (@Id0,@Id1)", sql);
        Assert.Equal(1, (int)parameters[0].Value);
        Assert.Equal(2, (int)parameters[1].Value);

        var orderNos = new string[] { "ON_001", "ON_002", "ON_003" };
        sql = repository.Delete<Order>()
            .Where(f => f.BuyerId == 1 && orderNos.Contains(f.OrderNo))
            .ToSql(out parameters);
        Assert.Equal("DELETE FROM [sys_order] WHERE [BuyerId]=1 AND [OrderNo] IN (@p0,@p1,@p2)", sql);
        Assert.Equal(orderNos[0], (string)parameters[0].Value);
        Assert.Equal(orderNos[1], (string)parameters[1].Value);
        Assert.Equal(orderNos[2], (string)parameters[2].Value);
    }
    [Fact]
    public async Task Delete_Multi_Where()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(f => new int[] { 1, 2 }.Contains(f.Id));
        var count = repository.Create<User>(new[]
        {
            new User
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
            },
            new User
            {
                Id = 2,
                TenantId = "2",
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
        Assert.Equal(2, count);
        count = await repository.DeleteAsync<User>(f => new int[] { 1, 2 }.Contains(f.Id));
        repository.Commit();
        Assert.Equal(2, count);

        var sql = repository.Delete<User>()
           .Where(f => new int[] { 1, 2 }.Contains(f.Id))
           .ToSql(out var parameters);
        Assert.Equal("DELETE FROM [sys_user] WHERE [Id] IN (1,2)", sql);
        //Assert.True((int)parameters[0].Value == 1);
        //Assert.True((int)parameters[1].Value == 2);
    }
    [Fact]
    public void Delete_Where_And()
    {
        using var repository = dbFactory.Create();
        bool? isMale = true;
        var sql = repository.Delete<User>()
            .Where(f => f.Name.Contains("kevin"))
            .And(isMale.HasValue, f => f.Age > 25)
            .ToSql(out _);
        Assert.Equal("DELETE FROM [sys_user] WHERE [Name] LIKE N'%kevin%' AND [Age]>25", sql);
    }
    [Fact]
    public void Delete_Enum_Fields()
    {
        using var repository = dbFactory.Create();
        var sql1 = repository.Delete<User>()
            .Where(f => f.Gender == Gender.Male)
            .ToSql(out _);
        Assert.Equal("DELETE FROM [sys_user] WHERE [Gender]=N'Male'", sql1);

        var gender = Gender.Male;
        var sql2 = repository.Delete<User>()
            .Where(f => f.Gender == gender)
            .ToSql(out var parameters1);
        Assert.Equal("DELETE FROM [sys_user] WHERE [Gender]=@p0", sql2);
        Assert.Equal("@p0", parameters1[0].ParameterName);
        Assert.True(parameters1[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters1[0].Value == gender.ToString());

        var sql3 = repository.Delete<Company>()
             .Where(f => f.Nature == CompanyNature.Internet)
             .ToSql(out _);
        Assert.Equal("DELETE FROM [sys_company] WHERE [Nature]=N'Internet'", sql3);

        var nature = CompanyNature.Internet;
        var sql4 = repository.Delete<Company>()
             .Where(f => f.Nature == nature)
             .ToSql(out var parameters2);
        Assert.Equal("DELETE FROM [sys_company] WHERE [Nature]=@p0", sql4);
        Assert.Equal("@p0", parameters2[0].ParameterName);
        Assert.True(parameters2[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters2[0].Value == CompanyNature.Internet.ToString());
    }
    [Fact]
    public void Transation()
    {
        using var repository = dbFactory.Create();
        bool? isMale = true;
        repository.Timeout(60);
        repository.BeginTransaction();
        repository.Update<User>()
            .Set(new { Name = "leafkevin1" })
            .Where(new { Id = 1 })
            .Execute();
        repository.Update<User>(new { Name = "leafkevin1", Id = 1 });
        repository.Delete<User>()
            .Where(f => f.Name.Contains("kevin"))
            .And(isMale.HasValue, f => f.Age > 25)
            .Execute();
        repository.Commit();
    }
}
