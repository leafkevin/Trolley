﻿using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Trolley.MySqlConnector;
using Xunit;

namespace Trolley.Test.MySql;

public class MySqlExpressionUnitTest : UnitTestBase
{
    public MySqlExpressionUnitTest()
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
    public void Coalesce()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        string firstName = "kevin", lastName = null;
        var sql = repository.From<User>()
            .Where(f => f.Name.Contains(lastName ?? firstName))
            .Select(f => new { HasName = f.Name ?? "NoName" })
            .ToSql(out var dbParameters);
        Assert.True(sql == "SELECT COALESCE(`Name`,'NoName') AS `HasName` FROM `sys_user` WHERE `Name` LIKE CONCAT('%',@p0,'%')");
        Assert.True(dbParameters[0].Value.ToString() == firstName);
    }
    [Fact]
    public void Conditional()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
             .Where(f => (f.IsEnabled ? "Enabled" : "Disabled") == "Enabled"
                 && (f.GuidField.HasValue ? "HasValue" : "NoValue") == "HasValue")
             .Select(f => new
             {
                 IsEnabled = f.IsEnabled ? "Enabled" : "Disabled",
                 GuidField = f.GuidField.HasValue ? "HasValue" : "NoValue",
                 IsOld = f.Age > 35 ? true : false,
                 IsNeedParameter = f.Name.Contains("kevin") ? "Yes" : "No",
             })
             .ToSql(out _);
        Assert.True(sql == "SELECT (CASE WHEN `IsEnabled`=1 THEN 'Enabled' ELSE 'Disabled' END) AS `IsEnabled`,(CASE WHEN `GuidField` IS NOT NULL THEN 'HasValue' ELSE 'NoValue' END) AS `GuidField`,(CASE WHEN `Age`>35 THEN 1 ELSE 0 END) AS `IsOld`,(CASE WHEN `Name` LIKE '%kevin%' THEN 'Yes' ELSE 'No' END) AS `IsNeedParameter` FROM `sys_user` WHERE (CASE WHEN `IsEnabled`=1 THEN 'Enabled' ELSE 'Disabled' END)='Enabled' AND (CASE WHEN `GuidField` IS NOT NULL THEN 'HasValue' ELSE 'NoValue' END)='HasValue'");

        var enabled = "Enabled";
        var hasValue = "HasValue";
        sql = repository.From<User>()
            .Where(f => (f.IsEnabled ? enabled : "Disabled") == enabled
                && (f.GuidField.HasValue ? hasValue : "NoValue") == hasValue)
            .Select(f => new
            {
                IsEnabled = f.IsEnabled ? enabled : "Disabled",
                GuidField = f.GuidField.HasValue ? hasValue : "NoValue",
                IsOld = f.Age > 35 ? true : false,
                IsNeedParameter = f.Name.Contains("kevin") ? "Yes" : "No",
            })
            .ToSql(out var dbParameters);
        Assert.True(sql == "SELECT (CASE WHEN `IsEnabled`=1 THEN @p4 ELSE 'Disabled' END) AS `IsEnabled`,(CASE WHEN `GuidField` IS NOT NULL THEN @p5 ELSE 'NoValue' END) AS `GuidField`,(CASE WHEN `Age`>35 THEN 1 ELSE 0 END) AS `IsOld`,(CASE WHEN `Name` LIKE '%kevin%' THEN 'Yes' ELSE 'No' END) AS `IsNeedParameter` FROM `sys_user` WHERE (CASE WHEN `IsEnabled`=1 THEN @p0 ELSE 'Disabled' END)=@p1 AND (CASE WHEN `GuidField` IS NOT NULL THEN @p2 ELSE 'NoValue' END)=@p3");
        Assert.True(dbParameters[0].Value.ToString() == enabled);
        Assert.True(dbParameters[1].Value.ToString() == enabled);
        Assert.True(dbParameters[2].Value.ToString() == hasValue);
        Assert.True(dbParameters[3].Value.ToString() == hasValue);
        Assert.True(dbParameters[4].Value.ToString() == enabled);
        Assert.True(dbParameters[5].Value.ToString() == hasValue);
    }
    [Fact]
    public void Index()
    {
        this.Initialize();
        string[] strArray = { "True", "False", "Unknown" };
        var strCollection = new ReadOnlyCollection<string>(strArray);
        var dict = new Dictionary<string, string>
        {
            {"1","leafkevin" },
            {"2","cindy" },
            {"3","xiyuan" }
        };
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => (f.Name.Contains(dict["1"]) || f.IsEnabled.ToString() == strCollection[0]))
            .Select(f => new
            {
                False = strArray[2],
                Unknown = strCollection[2],
                MyLove = dict["2"] + " and " + dict["3"]
            })
            .ToSql(out var dbParameters);
        Assert.True(sql == "SELECT @p2 AS `False`,@p3 AS `Unknown`,CONCAT(@p4,' and ',@p5) AS `MyLove` FROM `sys_user` WHERE `Name` LIKE CONCAT('%',@p0,'%') OR CAST(`IsEnabled` AS CHAR)=@p1");
        Assert.True((string)dbParameters[0].Value == dict["1"]);
        Assert.True((string)dbParameters[1].Value == strCollection[0]);
        Assert.True((string)dbParameters[2].Value == strArray[2]);
        Assert.True((string)dbParameters[3].Value == strCollection[2]);
        Assert.True((string)dbParameters[4].Value == dict["2"]);
        Assert.True((string)dbParameters[5].Value == dict["3"]);
    }
}

