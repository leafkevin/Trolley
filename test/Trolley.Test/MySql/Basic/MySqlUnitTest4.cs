using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Trolley.MySqlConnector;
using Xunit;

namespace Trolley.Test.MySql;

public class MySqlUnitTest4 : UnitTestBase
{
    enum Sex { Male, Female }
    struct Studuent
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public MySqlUnitTest4()
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
    public void IsEntityType()
    {
        Assert.False(typeof(Sex).IsEntityType());
        Assert.False(typeof(Sex?).IsEntityType());
        Assert.True(typeof(Studuent).IsEntityType());
        Assert.False(typeof(string).IsEntityType());
        Assert.False(typeof(int).IsEntityType());
        Assert.False(typeof(int?).IsEntityType());
        Assert.False(typeof(Guid).IsEntityType());
        Assert.False(typeof(Guid?).IsEntityType());
        Assert.False(typeof(DateTime).IsEntityType());
        Assert.False(typeof(DateTime?).IsEntityType());
        Assert.False(typeof(byte[]).IsEntityType());
        Assert.False(typeof(int[]).IsEntityType());
    }
    [Fact]
    public async void Delete()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(f => f.Id == 1);
        var count = repository.Create<User>(new User
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
        Assert.Equal(1, count);
        count = await repository.DeleteAsync<User>(f => f.Id == 1);
        repository.Commit();
        Assert.Equal(1, count);

        var sql = repository.Delete<User>()
            .Where(f => f.Id == 1)
            .ToSql(out var parameters);
        Assert.True(sql == "DELETE FROM `sys_user` WHERE `Id`=1");
        //Assert.True((int)parameters[0].Value == 1);
    }
    [Fact]
    public async void Delete_Multi()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(new[] { new { Id = 1 }, new { Id = 2 } });
        var count = repository.Create<User>(new[]
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
        Assert.Equal(2, count);
        count = await repository.DeleteAsync<User>(new[] { new { Id = 1 }, new { Id = 2 } });
        repository.Commit();
        Assert.Equal(2, count);

        var sql = repository.Delete<User>()
            .Where(new[] { new { Id = 1 }, new { Id = 2 } })
            .ToSql(out var parameters);
        Assert.True(sql == "DELETE FROM `sys_user` WHERE `Id`=@Id0;DELETE FROM `sys_user` WHERE `Id`=@Id1");
        Assert.True((int)parameters[0].Value == 1);
        Assert.True((int)parameters[1].Value == 2);
    }
    [Fact]
    public async void Delete_Multi1()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(new[] { 1, 2 });
        var count = repository.Create<User>(new[]
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
        Assert.Equal(2, count);
        count = await repository.DeleteAsync<User>(new int[] { 1, 2 });
        repository.Commit();
        Assert.Equal(2, count);

        var sql = repository.Delete<User>()
            .Where(new int[] { 1, 2 })
            .ToSql(out var parameters);
        Assert.True(sql == "DELETE FROM `sys_user` WHERE `Id`=@Id0;DELETE FROM `sys_user` WHERE `Id`=@Id1");
        Assert.True((int)parameters[0].Value == 1);
        Assert.True((int)parameters[1].Value == 2);

        var orderNos = new string[] { "ON_001", "ON_002", "ON_003" };
        sql = repository.Delete<Order>()
            .Where(f => f.BuyerId == 1 && orderNos.Contains(f.OrderNo))
            .ToSql(out parameters);
        Assert.True(sql == "DELETE FROM `sys_order` WHERE `BuyerId`=1 OR `OrderNo` IN (@p0,@p1,@p2)");
        Assert.True((string)parameters[0].Value == orderNos[0]);
        Assert.True((string)parameters[1].Value == orderNos[1]);
        Assert.True((string)parameters[2].Value == orderNos[2]);
    }
    [Fact]
    public async void Delete_Multi_Where()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(f => new int[] { 1, 2 }.Contains(f.Id));
        var count = repository.Create<User>(new[]
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
        Assert.Equal(2, count);
        count = await repository.DeleteAsync<User>(f => new int[] { 1, 2 }.Contains(f.Id));
        repository.Commit();
        Assert.Equal(2, count);

        var sql = repository.Delete<User>()
           .Where(f => new int[] { 1, 2 }.Contains(f.Id))
           .ToSql(out var parameters);
        Assert.True(sql == "DELETE FROM `sys_user` WHERE `Id` IN (1,2)");
        //Assert.True((int)parameters[0].Value == 1);
        //Assert.True((int)parameters[1].Value == 2);
    }
    [Fact]
    public void Delete_Enum_Fields()
    {
        using var repository = dbFactory.Create();
        var sql1 = repository.Delete<User>()
            .Where(f => f.Gender == Gender.Male)
            .ToSql(out _);
        Assert.True(sql1 == "DELETE FROM `sys_user` WHERE `Gender`=2");

        var gender = Gender.Male;
        var sql2 = repository.Delete<User>()
            .Where(f => f.Gender == gender)
            .ToSql(out var parameters1);
        Assert.True(sql2 == "DELETE FROM `sys_user` WHERE `Gender`=@p0");
        Assert.True(parameters1[0].ParameterName == "@p0");
        Assert.True(parameters1[0].Value.GetType() == typeof(sbyte));
        Assert.True((sbyte)parameters1[0].Value == (sbyte)gender);

        var sql3 = repository.Delete<Company>()
             .Where(f => f.Nature == CompanyNature.Internet)
             .ToSql(out _);
        Assert.True(sql3 == "DELETE FROM `sys_company` WHERE `Nature`='Internet'");

        var nature = CompanyNature.Internet;
        var sql4 = repository.Delete<Company>()
             .Where(f => f.Nature == nature)
             .ToSql(out var parameters2);
        Assert.True(sql4 == "DELETE FROM `sys_company` WHERE `Nature`=@p0");
        Assert.True(parameters2[0].ParameterName == "@p0");
        Assert.True(parameters2[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters2[0].Value == CompanyNature.Internet.ToString());
    }
}
