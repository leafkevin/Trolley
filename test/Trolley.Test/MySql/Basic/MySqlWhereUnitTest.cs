using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Trolley.Test.MySql;

public class MySqlWhereUnitTest : UnitTestBase
{
    public MySqlWhereUnitTest()
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
    public async void WhereBoolean()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => f.IsEnabled);
        Assert.True(result1.Count > 0);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.True(result2.Count > 0);
        Assert.True(result1.Count == result2.Count);
    }
    [Fact]
    public async void WhereMemberVisit()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => !(f.IsEnabled == false) && f.Id > 0);
        Assert.True(result1.Count > 0);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.True(result2.Count > 0);
        Assert.True(result1.Count == result2.Count);
    }
    [Fact]
    public async void WhereStringEnum()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var result1 = await repository.QueryAsync<Company>(f => f.Nature == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result2.Count >= 2);
        //Assert.True(result1.Count == result2.Count);
    }
}
