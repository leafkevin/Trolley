using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Trolley.Tests;

public class MySqlMethodCallUnitTest
{
    private readonly IOrmDbFactory dbFactory;
    public MySqlMethodCallUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
            var ormProvider = f.GetService<IOrmProvider>();
            var builder = new OrmDbFactoryBuilder();
            builder.Register("fengling", true, f => f.Add<MySqlProvider>(connectionString, true))
                .Configure(f => new ModelConfiguration().OnModelCreating(f));
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public void Contains()
    {
        using var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => new int[] { 1, 2, 3 }.Contains(f.Id))
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id` IN (1,2,3)");
        sql = repository.From<User>()
            .Where(f => f.Name.Contains("kevin"))
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Name` LIKE '%kevin%'");
        sql = repository.From<User>()
            .Where(f => new List<string> { "keivn", "cindy" }.Contains(f.Name))
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Name` IN ('keivn','cindy')");
    }
    [Fact]
    public void Concat()
    {
        using var repository = this.dbFactory.Create();
        bool isMale = false;
        int count = 10;
        var sql = repository.From<User>()
            .Select(f => string.Concat(f.Name + "_1_" + isMale, f.Age, isMale) + "_2_" + f.Age + "_3_" + isMale + "_4_" + count)
            .ToSql(out _);
        Assert.True(sql == "SELECT CONCAT(`Name`,'_1_','False',`Age`,'False','_2_',`Age`,'_3_','False','_4_','10') FROM `sys_user`");
    }
    [Fact]
    public void Format()
    {
        using var repository = this.dbFactory.Create();
        bool isMale = false;
        int count = 5;
        var sql = repository.From<User>()
            .Select(f => $"{f.Name + "222"}_111_{f.Age + isMale.ToString()}_{isMale}_{count}")
            .ToSql(out _);
        Assert.True(sql == "SELECT CONCAT(CONCAT(`Name`,'222'),'_111_',CONCAT(`Age`,'False'),'_False_5') FROM `sys_user`");
    }
}
