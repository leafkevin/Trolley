using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Trolley.Test.MySql;

public class MySqlDateTimeUnitTest : UnitTestBase
{
    public MySqlDateTimeUnitTest()
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
    public void MemberAccess()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
          .Where(f => f.Id == 1)
          .Select(f => new
          {
              DateTime.Now,
              DateTime.MinValue,
              DateTime.MaxValue,
              DateTime.UtcNow,
              DateTime.Today,
              DateTime.UnixEpoch,
              DateTime.Parse("2023-05-06").Date,
              CurrentDate = DateTime.Now.Date,
              IsEquals = f.UpdatedAt.Equals(DateTime.Parse("2023-03-25"))
          })
          .ToSql(out _);
        Assert.True(sql == "SELECT '2023-04-02 22:44:07' AS `Now`,'0001-01-01 00:00:00' AS `MinValue`,'9999-12-31 23:59:59' AS `MaxValue`,'2023-04-02 14:44:07' AS `UtcNow`,'2023-04-02 00:00:00' AS `Today`,'1970-01-01 00:00:00' AS `UnixEpoch`,'2023-05-06 00:00:00' AS `Date`,'2023-04-02 00:00:00' AS `CurrentDate`,`UpdatedAt`='2023-03-25 00:00:00' AS `IsEquals` FROM `sys_user` WHERE `Id`=1");
    }


    [Fact]
    public void Subtract()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
          .Where(f => f.UpdatedAt > DateTime.Now - TimeSpan.FromDays(365))
          .Select(f => new
          {
              OneYearsAgo1 = f.CreatedAt.Subtract(TimeSpan.FromDays(365)),
              OneYearsAgo2 = DateTime.Now - TimeSpan.FromDays(365),
              DayInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month),
              IsLeapYear1 = DateTime.IsLeapYear(DateTime.Now.Year),
              IsLeapYear2 = DateTime.IsLeapYear(2020),
              Parse = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
              ParseExact = DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture)
          })
          .ToSql(out _);
        Assert.True(sql == "");
    }
    [Fact]
    public void Compare()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
          .Where(f => DateTime.Compare(f.UpdatedAt, DateTime.Parse("2023-03-20")) > 0)
          .Select(f => new
          {
              CompareTo = f.CreatedAt.CompareTo(DateTime.Parse("2023-03-20")),
              OneYearsAgo1 = f.CreatedAt.Subtract(TimeSpan.FromDays(365)),
              OneYearsAgo2 = DateTime.Now - DateTime.Parse("2023-03-20"),
              DayInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month),
              IsLeapYear1 = DateTime.IsLeapYear(DateTime.Now.Year),
              IsLeapYear2 = DateTime.IsLeapYear(2020),
              Parse = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
              ParseExact = DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture)
          })
          .ToSql(out _);
        Assert.True(sql == "SELECT (CASE WHEN `CreatedAt`='2023-03-20 00:00:00' THEN 0 WHEN `CreatedAt`>'2023-03-20 00:00:00' THEN 1 ELSE -1 END) AS `CompareTo`,DATE_SUB(`CreatedAt`,INTERVAL 365*86400000000 MICROSECOND) AS `OneYearsAgo1`,5977302042426 AS `OneYearsAgo2`,31 AS `DayInMonth`,0 AS `IsLeapYear1`,1 AS `IsLeapYear2`,'2023-03-26 22:02:10' AS `Parse`,'2023-05-07 13:08:45' AS `ParseExact` FROM `sys_user` WHERE (CASE WHEN `UpdatedAt`='2023-03-20 00:00:00' THEN 0 WHEN `UpdatedAt`>'2023-03-20 00:00:00' THEN 1 ELSE -1 END)>0");
    }
    [Fact]
    public void Operation()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
          //.Where(f => DateTime.Compare(f.UpdatedAt, DateTime.Parse("2023-03-20")) > 0)
          .Select(f => new
          {
              //AddOp = f.CreatedAt + TimeSpan.FromHours(5),
              //SubOp = f.CreatedAt - TimeSpan.FromHours(10),
              //AddOp1 = f.SomeTimes.Add(TimeSpan.FromMinutes(25)),
              //SubOp1 = TimeSpan.FromHours(30) - TimeSpan.FromMinutes(15),
              //MulOp = TimeSpan.FromMinutes(25) * 3,
              //DivOp1 = TimeSpan.FromHours(30) / 5,
              //DivOp2 = TimeSpan.FromHours(30) / TimeSpan.FromHours(3)
          })
          .ToSql(out _);
        Assert.True(sql == "SELECT DATE_ADD(`CreatedAt`,INTERVAL(5*3600000000) MICROSECOND) AS `AddOp`,DATE_SUB(`CreatedAt`,INTERVAL 10*3600000000 MICROSECOND) AS `SubOp` FROM `sys_user` WHERE (CASE WHEN `UpdatedAt`='2023-03-20 00:00:00' THEN 0 WHEN `UpdatedAt`>'2023-03-20 00:00:00' THEN 1 ELSE -1 END)>0");
    }
}
