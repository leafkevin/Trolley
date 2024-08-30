using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trolley.MySqlConnector;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.MySqlConnector;

public class AllUnitTest : UnitTestBase
{
    private static int connTotal = 0;
    private static int connOpenTotal = 0;
    private readonly ITestOutputHelper output;
    public AllUnitTest(ITestOutputHelper output)
    {
        this.output = output;
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
            var builder = new OrmDbFactoryBuilder()
                .Register(OrmProviderType.MySql, "fengling", connectionString, true)
                .Configure<ModelConfiguration>(OrmProviderType.MySql)
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
                        Interlocked.Decrement(ref connTotal);
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
    public async Task MemberAccess()
    {
        this.Initialize();
        var localDate = DateOnly.FromDateTime(DateTime.Parse("2023-05-06"));
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                DateTime.Today,
                Today1 = DateOnly.FromDateTime(DateTime.Now),
                FromDayNumber = DateOnly.FromDayNumber(739081),
                localDate,
                DateOnly.MinValue,
                DateOnly.MaxValue,
                IsEquals = f.UpdatedAt.Equals(DateOnly.Parse("2023-03-25")),
                IsEquals1 = f.UpdatedAt.Equals(localDate),
                DateOnly.FromDateTime(DateTime.Now).DayNumber,
                DateOnly.FromDateTime(DateTime.Now).Day,
                DateOnly.FromDateTime(DateTime.Now).Month,
                DateOnly.FromDateTime(DateTime.Now).Year,
                DateOnly.FromDateTime(DateTime.Now).DayOfWeek
            })
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT CURDATE() AS `Today`,DATE(NOW()) AS `Today1`,'2024-07-15' AS `FromDayNumber`,@p0 AS `localDate`,'0001-01-01' AS `MinValue`,'9999-12-31' AS `MaxValue`,(a.`UpdatedAt`='2023-03-25') AS `IsEquals`,(a.`UpdatedAt`=@p1) AS `IsEquals1`,DATEDIFF(DATE(NOW()),'0001-01-01') AS `DayNumber`,DAYOFMONTH(DATE(NOW())) AS `Day`,MONTH(DATE(NOW())) AS `Month`,YEAR(DATE(NOW())) AS `Year`,(DAYOFWEEK(DATE(NOW()))-1) AS `DayOfWeek` FROM `sys_user` a WHERE a.`Id`=1", sql);
        Assert.Equal(2, dbParameters.Count);
        Assert.True(dbParameters[0].Value.GetType() == typeof(DateOnly));
        Assert.True(dbParameters[1].Value.GetType() == typeof(DateOnly));
        Assert.Equal(localDate, (DateOnly)dbParameters[0].Value);
        Assert.Equal(localDate, (DateOnly)dbParameters[1].Value);

        var result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.UpdatedAt,
                DateTime.Today,
                Today1 = DateOnly.FromDateTime(DateTime.Now),
                FromDayNumber = DateOnly.FromDayNumber(739081),
                localDate,
                DateOnly.MinValue,
                DateOnly.MaxValue,
                IsEquals = f.UpdatedAt.Equals(DateOnly.Parse("2023-03-25")),
                IsEquals1 = f.UpdatedAt.Equals(localDate),
                DateOnly.FromDateTime(DateTime.Now).DayNumber,
                DateOnly.FromDateTime(DateTime.Now).Day,
                DateOnly.FromDateTime(DateTime.Now).Month,
                DateOnly.FromDateTime(DateTime.Now).Year,
                DateOnly.FromDateTime(DateTime.Now).DayOfWeek
            })
            .FirstAsync();
        Assert.Equal(DateOnly.MinValue, result.MinValue);
        Assert.Equal(DateOnly.MaxValue, result.MaxValue);
        Assert.Equal(DateTime.Now.Date, result.Today);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Now), result.Today1);
        Assert.Equal(localDate, result.localDate);
        Assert.Equal(result.UpdatedAt.Equals(DateTime.Parse("2023-03-25")), result.IsEquals);
        Assert.Equal(result.UpdatedAt.Equals(localDate), result.IsEquals1);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Now).DayNumber, result.DayNumber);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Now).Day, result.Day);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Now).Month, result.Month);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Now).Year, result.Year);
        Assert.Equal(result.DayOfWeek, DateOnly.FromDateTime(DateTime.Now).DayOfWeek);
    }
    [Fact]
    public async Task AddCompareTo()
    {
        this.Initialize();
        var localDate = DateOnly.FromDateTime(DateTime.Parse("2023-05-06"));
        var repository = this.dbFactory.Create();
        var sql = repository.From<UpdateEntity>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                AddDays = f.DateOnlyField.AddDays(30),
                AddMonths = f.DateOnlyField.AddMonths(5),
                AddYears = f.DateOnlyField.AddYears(2),
                CompareTo = f.DateOnlyField.CompareTo(localDate),
                Parse = DateOnly.Parse(localDate.ToString("yyyy-MM-dd")),
                ParseExact = DateOnly.ParseExact("05-07/2023", "MM-dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None)
            })
            .ToSql(out _);
        Assert.Equal("SELECT DATE_ADD(a.`DateOnlyField`,INTERVAL 30 DAY) AS `AddDays`,DATE_ADD(a.`DateOnlyField`,INTERVAL 5 MONTH) AS `AddMonths`,DATE_ADD(a.`DateOnlyField`,INTERVAL 2 YEAR) AS `AddYears`,(CASE WHEN a.`DateOnlyField`=@p0 THEN 0 WHEN a.`DateOnlyField`>@p0 THEN 1 ELSE -1 END) AS `CompareTo`,@p1 AS `Parse`,'2023-05-07' AS `ParseExact` FROM `sys_update_entity` a WHERE a.`Id`=1", sql);

        var now = DateTime.Now;
        var result = await repository.From<UpdateEntity>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.DateOnlyField,
                AddDays = f.DateOnlyField.AddDays(30),
                AddMonths = f.DateOnlyField.AddMonths(5),
                AddYears = f.DateOnlyField.AddYears(2),
                CompareTo = f.DateOnlyField.CompareTo(localDate),
                Parse = DateOnly.Parse(localDate.ToString("yyyy-MM-dd")),
                ParseExact = DateOnly.ParseExact("05-07/2023", "MM-dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None)
            })
            .FirstAsync();
        Assert.Equal(result.DateOnlyField.AddDays(30), result.AddDays);
        Assert.Equal(result.DateOnlyField.AddMonths(5), result.AddMonths);
        Assert.Equal(result.DateOnlyField.AddYears(2), result.AddYears);
        Assert.Equal(result.DateOnlyField.CompareTo(localDate), result.CompareTo);
        Assert.Equal(DateOnly.Parse(localDate.ToString("yyyy-MM-dd")), result.Parse);
        Assert.Equal(DateOnly.ParseExact("05-07/2023", "MM-dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None), result.ParseExact);
    }




    [Fact]
    public async Task MemberAccess1()
    {
        this.Initialize();
        var localDate = DateTime.Parse("2023-05-06").Date;
        var repository = this.dbFactory.Create();
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
                localDate,
                IsEquals = f.UpdatedAt.Equals(DateTime.Parse("2023-03-25")),
                IsEquals1 = f.UpdatedAt.Equals(localDate)
            })
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT NOW() AS `Now`,'0001-01-01 00:00:00.000' AS `MinValue`,'9999-12-31 23:59:59.999' AS `MaxValue`,UTC_TIMESTAMP() AS `UtcNow`,CURDATE() AS `Today`,'1970-01-01 00:00:00.000' AS `UnixEpoch`,'2023-05-06 00:00:00.000' AS `Date`,CONVERT(NOW(),DATE) AS `CurrentDate`,@p0 AS `localDate`,(a.`UpdatedAt`='2023-03-25 00:00:00.000') AS `IsEquals`,(a.`UpdatedAt`=@p1) AS `IsEquals1` FROM `sys_user` a WHERE a.`Id`=1", sql);
        Assert.Equal(2, dbParameters.Count);
        Assert.Equal(typeof(DateTime), dbParameters[0].Value.GetType());
        Assert.Equal(typeof(DateTime), dbParameters[1].Value.GetType());
        Assert.Equal(localDate, (DateTime)dbParameters[0].Value);
        Assert.Equal(localDate, (DateTime)dbParameters[1].Value);

        var result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.UpdatedAt,
                DateTime.Now,
                DateTime.MinValue,
                DateTime.MaxValue,
                DateTime.UtcNow,
                DateTime.Today,
                DateTime.UnixEpoch,
                DateTime.Parse("2023-05-06").Date,
                CurrentDate = DateTime.Now.Date,
                localDate,
                IsEquals = f.UpdatedAt.Equals(DateTime.Parse("2023-03-25")),
                IsEquals1 = f.UpdatedAt.Equals(localDate)
            })
            .FirstAsync();
        Assert.Equal(DateTime.MinValue, result.MinValue);
        //由于精度不同，差一些微秒
        //Assert.True(result.MaxValue == DateTime.MaxValue);
        //取决于时区的设置
        Assert.Equal(DateTime.Now.Date, result.Today);
        Assert.Equal(DateTime.UnixEpoch, result.UnixEpoch);
        Assert.Equal(DateTime.Parse("2023-05-06").Date, result.Date);
        Assert.Equal(localDate, result.localDate);
        Assert.Equal(result.UpdatedAt.Equals(DateTime.Parse("2023-03-25")), result.IsEquals);
    }
    [Fact]
    public async Task AddSubtract()
    {
        this.Initialize();
        var days = 365;
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => f.UpdatedAt > DateTime.Now - TimeSpan.FromDays(days) - TimeSpan.FromMinutes(25))
            .Select(f => new
            {
                Add = f.CreatedAt.Add(TimeSpan.FromDays(365)),
                AddDays = f.CreatedAt.AddDays(30),
                AddMilliseconds = f.CreatedAt.AddMilliseconds(300),
                Subtract1 = f.CreatedAt.Subtract(TimeSpan.FromDays(365)),
                Subtract2 = DateTime.Now - TimeSpan.FromDays(365),
                Subtract3 = f.UpdatedAt.Subtract(f.CreatedAt),
                DayInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month),
                IsLeapYear1 = DateTime.IsLeapYear(DateTime.Now.Year),
                IsLeapYear2 = DateTime.IsLeapYear(2020),
                Parse = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                ParseExact = DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture)
            })
            .ToSql(out _);
        Assert.Equal("SELECT DATE_ADD(a.`CreatedAt`,INTERVAL 365 DAY) AS `Add`,DATE_ADD(a.`CreatedAt`,INTERVAL 30 DAY) AS `AddDays`,DATE_ADD(a.`CreatedAt`,INTERVAL 300*1000 MICROSECOND) AS `AddMilliseconds`,DATE_SUB(a.`CreatedAt`,INTERVAL 365 DAY) AS `Subtract1`,DATE_SUB(NOW(),INTERVAL 365 DAY) AS `Subtract2`,TIMEDIFF(a.`UpdatedAt`,a.`CreatedAt`) AS `Subtract3`,DAYOFMONTH(LAST_DAY(CONCAT(YEAR(NOW()),'-',MONTH(NOW()),'-01'))) AS `DayInMonth`,(YEAR(NOW())%4=0 AND YEAR(NOW())%100<>0 OR YEAR(NOW())%400=0) AS `IsLeapYear1`,1 AS `IsLeapYear2`,CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) AS `Parse`,'2023-05-07 13:08:45.000' AS `ParseExact` FROM `sys_user` a WHERE a.`UpdatedAt`>SUBTIME(DATE_SUB(NOW(),INTERVAL 365 DAY),'00:25:00.000000')", sql);

        var now = DateTime.Now;
        var result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.CreatedAt,
                f.UpdatedAt,
                Now = now,
                Add = f.CreatedAt.Add(TimeSpan.FromDays(365)),
                AddDays = f.CreatedAt.AddDays(30),
                AddMilliseconds = f.CreatedAt.AddMilliseconds(300),
                Subtract1 = f.CreatedAt.Subtract(TimeSpan.FromDays(365)),
                Subtract2 = now - TimeSpan.FromDays(365),
                Subtract3 = f.UpdatedAt.Subtract(f.CreatedAt),
                DayInMonth = DateTime.DaysInMonth(now.Year, now.Month),
                IsLeapYear1 = DateTime.IsLeapYear(now.Year),
                IsLeapYear2 = DateTime.IsLeapYear(2020),
                Parse = DateTime.Parse(now.ToString("yyyy-MM-dd HH:mm:ss")),
                ParseExact = DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture)
            })
            .FirstAsync();
        Assert.Equal(result.CreatedAt.Add(TimeSpan.FromDays(365)), result.Add);
        Assert.Equal(result.CreatedAt.AddDays(30), result.AddDays);
        Assert.Equal(result.CreatedAt.AddMilliseconds(300), result.AddMilliseconds);
        Assert.Equal(result.CreatedAt.Subtract(TimeSpan.FromDays(365)), result.Subtract1);
        Assert.Equal(result.Now - TimeSpan.FromDays(365), result.Subtract2);
        Assert.Equal(result.UpdatedAt - result.CreatedAt, result.Subtract3);
        Assert.Equal(DateTime.DaysInMonth(now.Year, now.Month), result.DayInMonth);
        Assert.Equal(DateTime.IsLeapYear(now.Year), result.IsLeapYear1);
        Assert.Equal(DateTime.IsLeapYear(2020), result.IsLeapYear2);
        Assert.Equal(DateTime.Parse(now.ToString("yyyy-MM-dd HH:mm:ss")), result.Parse);
        Assert.Equal(DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture), result.ParseExact);
    }
    [Fact]
    public async Task Compare()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => DateTime.Compare(f.UpdatedAt, DateTime.Parse("2023-03-20")) > 0)
            .Select(f => new
            {
                CompareTo = f.CreatedAt.CompareTo(DateTime.Parse("2023-03-03")),
                OneYearsAgo1 = f.CreatedAt.Subtract(TimeSpan.FromDays(365)),
                OneYearsAgo2 = DateTime.Now - DateTime.Parse("2023-03-20"),
                DayInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month),
                IsLeapYear1 = DateTime.IsLeapYear(DateTime.Now.Year),
                IsLeapYear2 = DateTime.IsLeapYear(2020),
                Parse = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                ParseExact = DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture)
            })
            .ToSql(out _);
        Assert.Equal("SELECT (CASE WHEN a.`CreatedAt`='2023-03-03 00:00:00.000' THEN 0 WHEN a.`CreatedAt`>'2023-03-03 00:00:00.000' THEN 1 ELSE -1 END) AS `CompareTo`,DATE_SUB(a.`CreatedAt`,INTERVAL 365 DAY) AS `OneYearsAgo1`,TIMEDIFF(NOW(),'2023-03-20 00:00:00.000') AS `OneYearsAgo2`,DAYOFMONTH(LAST_DAY(CONCAT(YEAR(NOW()),'-',MONTH(NOW()),'-01'))) AS `DayInMonth`,(YEAR(NOW())%4=0 AND YEAR(NOW())%100<>0 OR YEAR(NOW())%400=0) AS `IsLeapYear1`,1 AS `IsLeapYear2`,CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) AS `Parse`,'2023-05-07 13:08:45.000' AS `ParseExact` FROM `sys_user` a WHERE (CASE WHEN a.`UpdatedAt`='2023-03-20 00:00:00.000' THEN 0 WHEN a.`UpdatedAt`>'2023-03-20 00:00:00.000' THEN 1 ELSE -1 END)>0", sql);

        var now = DateTime.Now;
        var result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.CreatedAt,
                f.UpdatedAt,
                DateTime.Now,
                Compare = DateTime.Compare(f.UpdatedAt, DateTime.Parse("2023-03-20")),
                CompareTo = f.CreatedAt.CompareTo(DateTime.Parse("2023-03-03")),
                OneYearsAgo1 = f.CreatedAt.Subtract(TimeSpan.FromDays(365)),
                OneYearsAgo2 = f.CreatedAt - DateTime.Parse("2023-03-20"),
                Subtract = f.CreatedAt.Subtract(DateTime.Parse("2023-03-01")),
                DayInMonth = DateTime.DaysInMonth(now.Year, now.Month),
                IsLeapYear1 = DateTime.IsLeapYear(now.Year),
                IsLeapYear2 = DateTime.IsLeapYear(2020),
                Parse = DateTime.Parse(now.ToString("yyyy-MM-dd HH:mm:ss")),
                ParseExact = DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture)
            })
            .FirstAsync();
        Assert.Equal(result.Compare, DateTime.Compare(result.UpdatedAt, DateTime.Parse("2023-03-20")));
        Assert.Equal(result.CompareTo, result.CreatedAt.CompareTo(DateTime.Parse("2023-03-03")));
        Assert.Equal(result.OneYearsAgo1, result.CreatedAt.Subtract(TimeSpan.FromDays(365)));
        Assert.Equal(result.OneYearsAgo2, result.CreatedAt - DateTime.Parse("2023-03-20"));
        Assert.Equal(result.Subtract, result.CreatedAt.Subtract(DateTime.Parse("2023-03-01")));
        Assert.Equal(result.DayInMonth, DateTime.DaysInMonth(now.Year, now.Month));
        Assert.Equal(result.IsLeapYear1, DateTime.IsLeapYear(now.Year));
        Assert.Equal(result.IsLeapYear2, DateTime.IsLeapYear(2020));
        Assert.Equal(result.Parse, DateTime.Parse(now.ToString("yyyy-MM-dd HH:mm:ss")));
        Assert.Equal(result.ParseExact, DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture));
    }
    [Fact]
    public async Task Operation()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => DateTime.Compare(f.UpdatedAt, DateTime.Parse("2023-03-20")) > 0)
            .Select(f => new
            {
                DateSub = DateTime.Parse("2022-01-01 05:06:07") - DateTime.Parse("2022-01-01"),
                AddOp = f.CreatedAt + TimeSpan.FromHours(5),
                SubOp = f.CreatedAt - TimeSpan.FromHours(10),
                AddOp1 = f.SomeTimes.Value.Add(TimeSpan.FromMinutes(25)),
                SubOp1 = TimeSpan.FromHours(30) - TimeSpan.FromMinutes(15),
                SubOp2 = f.UpdatedAt - f.CreatedAt,
                MulOp = TimeSpan.FromMinutes(25) * 3,
                DivOp1 = TimeSpan.FromHours(30) / 5,
                DivOp2 = TimeSpan.FromHours(30) / TimeSpan.FromHours(3)
            })
            .ToSql(out _);
        Assert.Equal("SELECT '05:06:07.000000' AS `DateSub`,ADDTIME(a.`CreatedAt`,'05:00:00.000000') AS `AddOp`,SUBTIME(a.`CreatedAt`,'10:00:00.000000') AS `SubOp`,ADDTIME(a.`SomeTimes`,'00:25:00.000000') AS `AddOp1`,'1.05:45:00.000000' AS `SubOp1`,TIMEDIFF(a.`UpdatedAt`,a.`CreatedAt`) AS `SubOp2`,'01:15:00.000000' AS `MulOp`,'06:00:00.000000' AS `DivOp1`,10 AS `DivOp2` FROM `sys_user` a WHERE (CASE WHEN a.`UpdatedAt`='2023-03-20 00:00:00.000' THEN 0 WHEN a.`UpdatedAt`>'2023-03-20 00:00:00.000' THEN 1 ELSE -1 END)>0", sql);
        var result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.CreatedAt,
                f.UpdatedAt,
                f.SomeTimes,
                DateSub = DateTime.Parse("2022-01-01 05:06:07") - DateTime.Parse("2022-01-01"),
                AddOp = f.CreatedAt + TimeSpan.FromHours(5),
                SubOp = f.CreatedAt - TimeSpan.FromHours(10),
                AddOp1 = f.SomeTimes.Value.Add(TimeSpan.FromMinutes(25)),
                SubOp1 = TimeSpan.FromHours(30) - TimeSpan.FromMinutes(15),
                SubOp2 = f.UpdatedAt - f.CreatedAt,
                MulOp = TimeSpan.FromMinutes(25) * 3,
                DivOp1 = TimeSpan.FromHours(30) / 5,
                DivOp2 = TimeSpan.FromHours(30) / TimeSpan.FromHours(3)
            })
            .FirstAsync();
        Assert.Equal(result.DateSub, DateTime.Parse("2022-01-01 05:06:07") - DateTime.Parse("2022-01-01"));
        Assert.Equal(result.AddOp, result.CreatedAt + TimeSpan.FromHours(5));
        Assert.Equal(result.SubOp, result.CreatedAt - TimeSpan.FromHours(10));
        Assert.Equal(result.AddOp1, result.SomeTimes.Value.Add(TimeSpan.FromMinutes(25)));
        Assert.Equal(result.SubOp1, TimeSpan.FromHours(30) - TimeSpan.FromMinutes(15));
        Assert.Equal(result.SubOp2, result.UpdatedAt - result.CreatedAt);
        Assert.Equal(result.MulOp, TimeSpan.FromMinutes(25) * 3);
        Assert.Equal(result.DivOp1, TimeSpan.FromHours(30) / 5);
        Assert.Equal(result.DivOp2, TimeSpan.FromHours(30) / TimeSpan.FromHours(3));
    }



    [Fact]
    public void Coalesce()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        string firstName = "kevin", lastName = null;
        var sql = repository.From<User>()
            .Where(f => f.Name.Contains(lastName ?? firstName))
            .Select(f => new { HasName = f.Name ?? "NoName" })
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT COALESCE(a.`Name`,'NoName') AS `HasName` FROM `sys_user` a WHERE a.`Name` LIKE CONCAT('%',@p0,'%')", sql);
        Assert.Equal(dbParameters[0].Value.ToString(), firstName);

        sql = repository.From<User>()
            .Where(f => (f.Name ?? f.Id.ToString()) == "leafkevin")
            .Select(f => f.Id)
            .ToSql(out dbParameters);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE COALESCE(a.`Name`,CAST(a.`Id` AS CHAR))='leafkevin'", sql);
    }
    [Fact]
    public void Conditional()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
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
        Assert.Equal("SELECT (CASE WHEN a.`IsEnabled`=1 THEN 'Enabled' ELSE 'Disabled' END) AS `IsEnabled`,(CASE WHEN a.`GuidField` IS NOT NULL THEN 'HasValue' ELSE 'NoValue' END) AS `GuidField`,(CASE WHEN a.`Age`>35 THEN 1 ELSE 0 END) AS `IsOld`,(CASE WHEN INSTR('kevin',a.`Name`)>0 THEN 'Yes' ELSE 'No' END) AS `IsNeedParameter` FROM `sys_user` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN 'Enabled' ELSE 'Disabled' END)='Enabled' AND (CASE WHEN a.`GuidField` IS NOT NULL THEN 'HasValue' ELSE 'NoValue' END)='HasValue'", sql);

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
        Assert.Equal("SELECT (CASE WHEN a.`IsEnabled`=1 THEN @p4 ELSE 'Disabled' END) AS `IsEnabled`,(CASE WHEN a.`GuidField` IS NOT NULL THEN @p5 ELSE 'NoValue' END) AS `GuidField`,(CASE WHEN a.`Age`>35 THEN 1 ELSE 0 END) AS `IsOld`,(CASE WHEN INSTR('kevin',a.`Name`)>0 THEN 'Yes' ELSE 'No' END) AS `IsNeedParameter` FROM `sys_user` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN @p0 ELSE 'Disabled' END)=@p1 AND (CASE WHEN a.`GuidField` IS NOT NULL THEN @p2 ELSE 'NoValue' END)=@p3", sql);
        Assert.Equal(6, dbParameters.Count);
        Assert.True(dbParameters[0].Value.ToString() == enabled);
        Assert.True(dbParameters[1].Value.ToString() == enabled);
        Assert.True(dbParameters[2].Value.ToString() == hasValue);
        Assert.True(dbParameters[3].Value.ToString() == hasValue);
        Assert.True(dbParameters[4].Value.ToString() == enabled);
        Assert.True(dbParameters[5].Value.ToString() == hasValue);

        var result = repository.From<User>()
            .Where(f => (f.IsEnabled ? enabled : "Disabled") == enabled
                && (f.GuidField.HasValue ? hasValue : "NoValue") == hasValue)
            .Select(f => new
            {
                IsEnabled = f.IsEnabled ? enabled : "Disabled",
                GuidField = f.GuidField.HasValue ? hasValue : "NoValue",
                IsOld = f.Age > 35 ? true : false,
                IsNeedParameter = f.Name.Contains("kevin") ? "Yes" : "No",
            })
            .ToList();
        Assert.NotNull(result);
    }
    [Fact]
    public async Task WhereCoalesceConditional()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')='Internet'", sql1);
        var result1 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);
        Assert.Equal(CompanyNature.Internet, (result1[0].Nature ?? CompanyNature.Internet));

        var localNature = CompanyNature.Internet;
        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')=@p0", sql2);
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result2.Count >= 2);
        Assert.Equal(localNature, (result2[0].Nature ?? CompanyNature.Internet));

        var sql3 = repository.From<Company>()
        .Where(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature)
        .ToSql(out dbParameters);
        Assert.Equal("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN a.`Nature` ELSE 'Internet' END)=@p0", sql3);
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result3 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.True(result3.Count >= 2);
        Assert.Equal(localNature, (result3[0].Nature ?? CompanyNature.Internet));
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
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => (f.Name.Contains(dict["1"]) || f.IsEnabled.ToString() == strCollection[0]))
            .Select(f => new
            {
                False = strArray[2],
                Unknown = strCollection[2],
                MyLove = dict["2"] + " and " + dict["3"]
            })
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT @p2 AS `False`,@p3 AS `Unknown`,CONCAT(@p4,' and ',@p5) AS `MyLove` FROM `sys_user` a WHERE a.`Name` LIKE CONCAT('%',@p0,'%') OR CAST(a.`IsEnabled` AS CHAR)=@p1", sql);
        Assert.Equal(6, dbParameters.Count);
        Assert.Equal(dict["1"], (string)dbParameters[0].Value);
        Assert.Equal(strCollection[0], (string)dbParameters[1].Value);
        Assert.Equal(strArray[2], (string)dbParameters[2].Value);
        Assert.Equal(strCollection[2], (string)dbParameters[3].Value);
        Assert.Equal(dict["2"], (string)dbParameters[4].Value);
        Assert.Equal(dict["3"], (string)dbParameters[5].Value);

        var result = repository.From<User>()
            .Where(f => (f.Name.Contains(dict["1"]) || f.IsEnabled.ToString() == strCollection[0]))
            .Select(f => new
            {
                False = strArray[2],
                Unknown = strCollection[2],
                MyLove = dict["2"] + " and " + dict["3"]
            })
            .ToList();
        Assert.NotNull(result);
    }



    [Fact]
    public async Task Contains()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => new int[] { 1, 2 }.Contains(f.Id))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (1,2)", sql);
        var result = repository.From<User>()
            .Where(f => new int[] { 1, 2 }.Contains(f.Id))
            .ToList();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        sql = repository.From<User>()
            .Where(f => f.Name.Contains("kevin"))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Name` LIKE '%kevin%'", sql);
        result = await repository.From<User>()
            .Where(f => f.Name.Contains("kevin"))
            .ToListAsync();
        Assert.NotNull(result);
        Assert.True(result.Count >= 1);

        sql = repository.From<User>()
            .Where(f => new List<string> { "kevin", "cindy" }.Contains(f.Name))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Name` IN ('kevin','cindy')", sql);
        result = await repository.From<User>()
            .Where(f => new List<string> { "kevin", "cindy" }.Contains(f.Name))
            .ToListAsync();
        Assert.NotNull(result);
        Assert.Single(result);

        var ids = new int[] { 1, 2 };
        sql = repository.From<User>()
            .Where(f => ids.Contains(f.Id))
            .Select(f => f.Id)
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (@p0,@p1)", sql);

        result = repository.From<User>()
            .Where(f => ids.Contains(f.Id))
            .ToList();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var names = new List<string> { "kevin", "cindy" };
        sql = repository.From<User>()
            .Where(f => names.Contains(f.Name))
            .Select(f => f.Id)
            .ToSql(out dbParameters);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Name` IN (@p0,@p1)", sql);
        result = await repository.From<User>()
            .Where(f => names.Contains(f.Name))
            .ToListAsync();
        Assert.NotNull(result);
        Assert.Single(result);

        sql = repository.From<Company>()
            .Where(f => f.Name.Contains("微软"))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_company` a WHERE a.`Name` LIKE '%微软%'", sql);
        var result1 = await repository.From<Company>()
            .Where(f => f.Name.Contains("微软"))
            .ToListAsync();
        Assert.NotNull(result1);
        Assert.Single(result);
    }
    [Fact]
    public async Task Concat()
    {
        var repository = this.dbFactory.Create();
        bool isMale = false;
        int count = 10;
        var sql = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => string.Concat(f.Name + "_1_" + isMale, f.Age + 5, isMale) + "_2_" + f.Age + "_3_" + isMale + "_4_" + count)
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT CONCAT(a.`Name`,'_1_',@p0,CAST(a.`Age`+5 AS CHAR),@p1,'_2_',CAST(a.`Age` AS CHAR),'_3_',@p2,'_4_',@p3) FROM `sys_user` a WHERE a.`Id`=1", sql);
        Assert.Equal((string)dbParameters[0].Value, isMale.ToString());
        Assert.Equal(typeof(string), dbParameters[0].Value.GetType());
        Assert.Equal((string)dbParameters[1].Value, isMale.ToString());
        Assert.Equal(typeof(string), dbParameters[1].Value.GetType());
        Assert.Equal((string)dbParameters[2].Value, isMale.ToString());
        Assert.Equal(typeof(string), dbParameters[2].Value.GetType());
        Assert.Equal((string)dbParameters[3].Value, count.ToString());
        Assert.Equal(typeof(string), dbParameters[3].Value.GetType());

        var result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => string.Concat(f.Name + "_1_" + isMale, f.Age + 5, isMale) + "_2_" + f.Age + "_3_" + isMale + "_4_" + count)
            .FirstAsync();
        Assert.NotNull(result);
        Assert.Equal("leafkevin_1_False30False_2_25_3_False_4_10", result);
    }
    [Fact]
    public async Task Format()
    {
        var repository = this.dbFactory.Create();
        bool isMale = false;
        int count = 5;
        var sql = repository.From<User>()
            .Where(f => f.Name.Contains("cindy"))
            .Select(f => $"{f.Name + "222"}_111_{f.Age + isMale.ToString()}_{isMale}_{count}")
               .ToSql(out var dbParameters);
        Assert.Equal("SELECT CONCAT(a.`Name`,'222_111_',CAST(a.`Age` AS CHAR),@p0,'_',@p1,'_',@p2) FROM `sys_user` a WHERE a.`Name` LIKE '%cindy%'", sql);
        Assert.Equal((string)dbParameters[0].Value, isMale.ToString());
        Assert.Equal(typeof(string), dbParameters[0].Value.GetType());
        Assert.Equal((string)dbParameters[1].Value, isMale.ToString());
        Assert.Equal(typeof(string), dbParameters[1].Value.GetType());
        Assert.Equal((string)dbParameters[2].Value, count.ToString());
        Assert.Equal(typeof(string), dbParameters[2].Value.GetType());
        var result = await repository.From<User>()
            .Where(f => f.Name.Contains("cindy"))
            .Select(f => $"{f.Name + "222"}_111_{f.Age + isMale.ToString()}_{isMale}_{count}")
            .FirstAsync();
        Assert.Equal("cindy222_111_21False_False_5", result);
    }
    [Fact]
    public void Compare1()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NameCompare = string.Compare(f.Name, "leafkevin"),
                CreatedAtCompare = DateTime.Compare(f.CreatedAt, DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))),
                CreatedAtCompare1 = DateTime.Compare(f.CreatedAt, DateTime.Now),
                UpdatedAtCompare = DateTime.Compare(f.UpdatedAt, f.UpdatedAt.Subtract(TimeSpan.FromMinutes(2005)))
            })
            .ToSql(out _);
        Assert.Equal("SELECT (CASE WHEN a.`Name`='leafkevin' THEN 0 WHEN a.`Name`>'leafkevin' THEN 1 ELSE -1 END) AS `NameCompare`,(CASE WHEN a.`CreatedAt`=CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) THEN 0 WHEN a.`CreatedAt`>CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) THEN 1 ELSE -1 END) AS `CreatedAtCompare`,(CASE WHEN a.`CreatedAt`=NOW() THEN 0 WHEN a.`CreatedAt`>NOW() THEN 1 ELSE -1 END) AS `CreatedAtCompare1`,(CASE WHEN a.`UpdatedAt`=SUBTIME(DATE_SUB(a.`UpdatedAt`,INTERVAL 1 DAY),'09:25:00.000000') THEN 0 WHEN a.`UpdatedAt`>SUBTIME(DATE_SUB(a.`UpdatedAt`,INTERVAL 1 DAY),'09:25:00.000000') THEN 1 ELSE -1 END) AS `UpdatedAtCompare` FROM `sys_user` a WHERE a.`Id`=1", sql1);

        var result1 = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NameCompare = string.Compare(f.Name, "leafkevin"),
                CreatedAtCompare = DateTime.Compare(f.CreatedAt, DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))),
                CreatedAtCompare1 = DateTime.Compare(f.CreatedAt, DateTime.Now),
                UpdatedAtCompare = DateTime.Compare(f.UpdatedAt, f.UpdatedAt.Subtract(TimeSpan.FromMinutes(2005)))
            })
            .First();
        Assert.NotNull(result1);
        Assert.Equal(0, result1.NameCompare);
        Assert.Equal(-1, result1.CreatedAtCompare);
        Assert.Equal(-1, result1.CreatedAtCompare1);
        Assert.Equal(1, result1.UpdatedAtCompare);

        var sql2 = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NameCompare = string.Compare(f.Name, "leafkevin"),
                CreatedAtCompare = DateTime.Compare(f.CreatedAt, DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))),
                CreatedAtCompare1 = DateTime.Compare(f.CreatedAt, DateTime.Now),
                UpdatedAtCompare = DateTime.Compare(f.UpdatedAt, f.UpdatedAt.Subtract(TimeSpan.FromMinutes(15)))
            })
            .ToSql(out _);
        Assert.Equal("SELECT (CASE WHEN a.`Name`='leafkevin' THEN 0 WHEN a.`Name`>'leafkevin' THEN 1 ELSE -1 END) AS `NameCompare`,(CASE WHEN a.`CreatedAt`=CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) THEN 0 WHEN a.`CreatedAt`>CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) THEN 1 ELSE -1 END) AS `CreatedAtCompare`,(CASE WHEN a.`CreatedAt`=NOW() THEN 0 WHEN a.`CreatedAt`>NOW() THEN 1 ELSE -1 END) AS `CreatedAtCompare1`,(CASE WHEN a.`UpdatedAt`=SUBTIME(a.`UpdatedAt`,'00:15:00.000000') THEN 0 WHEN a.`UpdatedAt`>SUBTIME(a.`UpdatedAt`,'00:15:00.000000') THEN 1 ELSE -1 END) AS `UpdatedAtCompare` FROM `sys_user` a WHERE a.`Id`=1", sql2);

        var result2 = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NameCompare = string.Compare(f.Name, "leafkevin"),
                CreatedAtCompare = DateTime.Compare(f.CreatedAt, DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))),
                CreatedAtCompare1 = DateTime.Compare(f.CreatedAt, DateTime.Now),
                UpdatedAtCompare = DateTime.Compare(f.UpdatedAt, f.UpdatedAt.Subtract(TimeSpan.FromMinutes(15)))
            })
            .First();
        Assert.NotNull(result2);
        Assert.Equal(0, result2.NameCompare);
        Assert.Equal(-1, result2.CreatedAtCompare);
        Assert.Equal(-1, result2.CreatedAtCompare1);
        Assert.Equal(1, result2.UpdatedAtCompare);
    }
    [Fact]
    public void CompareTo()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .Select(f => new
            {
                IntCompare = f.Id.CompareTo("1"),
                StringCompare = f.OrderNo.CompareTo("OrderNo-001"),
                DateTimeCompare = f.CreatedAt.CompareTo(DateTime.Parse("2022-12-20")),
                BooleanCompare = f.IsEnabled.CompareTo(false)
            })
            .ToSql(out _);
        Assert.Equal("SELECT (CASE WHEN a.`Id`='1' THEN 0 WHEN a.`Id`>'1' THEN 1 ELSE -1 END) AS `IntCompare`,(CASE WHEN a.`OrderNo`='OrderNo-001' THEN 0 WHEN a.`OrderNo`>'OrderNo-001' THEN 1 ELSE -1 END) AS `StringCompare`,(CASE WHEN a.`CreatedAt`='2022-12-20 00:00:00.000' THEN 0 WHEN a.`CreatedAt`>'2022-12-20 00:00:00.000' THEN 1 ELSE -1 END) AS `DateTimeCompare`,(CASE WHEN a.`IsEnabled`=0 THEN 0 WHEN a.`IsEnabled`>0 THEN 1 ELSE -1 END) AS `BooleanCompare` FROM `sys_order` a", sql);

        var result = repository.From<Order>()
            .Where(f => f.Id == "1")
            .Select(f => new
            {
                f.Id,
                f.TenantId,
                f.OrderNo,
                f.CreatedAt,
                f.IsEnabled,
                IntCompare = f.Id.CompareTo("1"),
                StringCompare = f.OrderNo.CompareTo("OrderNo-001"),
                DateTimeCompare = f.CreatedAt.CompareTo(DateTime.Parse("2022-12-20")),
                BooleanCompare = f.IsEnabled.CompareTo(false)
            })
            .First();
        Assert.NotNull(result);
        Assert.Equal(result.IntCompare, result.Id.CompareTo("1"));
        Assert.Equal(result.StringCompare, result.OrderNo.CompareTo("OrderNo-001"));
        Assert.Equal(result.DateTimeCompare, result.CreatedAt.CompareTo(DateTime.Parse("2022-12-20")));
        Assert.Equal(result.BooleanCompare, result.IsEnabled.CompareTo(false));
    }
    [Fact]
    public void Trims()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .Select(f => new
            {
                Trim = "Begin_" + f.OrderNo.Trim() + "  123   ".Trim() + "_End",
                TrimStart = "Begin_" + f.OrderNo.TrimStart() + "  123   ".TrimStart() + "_End",
                TrimEnd = "Begin_" + f.OrderNo.TrimEnd() + "  123   ".TrimEnd() + "_End"
            })
            .ToSql(out _);
        Assert.Equal("SELECT CONCAT('Begin_',TRIM(a.`OrderNo`),'123_End') AS `Trim`,CONCAT('Begin_',LTRIM(a.`OrderNo`),'123   _End') AS `TrimStart`,CONCAT('Begin_',RTRIM(a.`OrderNo`),'  123_End') AS `TrimEnd` FROM `sys_order` a", sql);

        var strValue1 = "Begin_";
        var strValue2 = "  123   ";
        var strValue3 = "_End";
        var sql1 = repository.From<Order>()
            .Select(f => new
            {
                Trim = strValue1 + f.OrderNo.Trim() + strValue2.Trim() + strValue3,
                TrimStart = "Begin_" + f.OrderNo.TrimStart() + strValue2.TrimStart() + "_End",
                TrimEnd = "Begin_" + f.OrderNo.TrimEnd() + strValue2.TrimEnd() + "_End"
            })
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT CONCAT(@p0,TRIM(a.`OrderNo`),@p1,@p2) AS `Trim`,CONCAT('Begin_',LTRIM(a.`OrderNo`),@p3,'_End') AS `TrimStart`,CONCAT('Begin_',RTRIM(a.`OrderNo`),@p4,'_End') AS `TrimEnd` FROM `sys_order` a", sql1);
        Assert.Equal(strValue1, (string)dbParameters[0].Value);
        Assert.Equal(typeof(string), dbParameters[0].Value.GetType());
        Assert.Equal((string)dbParameters[1].Value, strValue2.Trim());
        Assert.Equal(typeof(string), dbParameters[1].Value.GetType());
        Assert.Equal(strValue3, (string)dbParameters[2].Value);
        Assert.Equal(typeof(string), dbParameters[2].Value.GetType());
        Assert.Equal((string)dbParameters[3].Value, strValue2.TrimStart());
        Assert.Equal(typeof(string), dbParameters[3].Value.GetType());
        Assert.Equal((string)dbParameters[4].Value, strValue2.TrimEnd());
        Assert.Equal(typeof(string), dbParameters[4].Value.GetType());

        repository.BeginTransaction();
        repository.Delete<Order>(new[] { "1", "2", "3" });
        var count = repository.Create<Order>(new[]
        {
            new Order
            {
                Id =  "1",
                TenantId = "1",
                OrderNo = " ON-001 ",
                BuyerId = 1,
                SellerId = 2,
                TotalAmount = 500,
                Products = new List<int>{1, 2},
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Order
            {
                Id = "2",
                TenantId = "2",
                OrderNo = " ON-002 ",
                BuyerId = 2,
                SellerId = 1,
                TotalAmount = 350,
                Products = new List<int>{1, 3},
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Order
            {
                Id = "3",
                TenantId = "3",
                OrderNo = " ON-003 ",
                BuyerId = 1,
                SellerId = 2,
                TotalAmount = 199,
                Products = new List<int>{2},
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });
        var result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "1", "2", "3" }))
            .OrderBy(f => f.Id)
            .Select(f => new
            {
                Trim = "Begin_" + f.OrderNo.Trim() + "  123   ".Trim() + "_End",
                TrimStart = "Begin_" + f.OrderNo.TrimStart() + "  123   ".TrimStart() + "_End",
                TrimEnd = "Begin_" + f.OrderNo.TrimEnd() + "  123   ".TrimEnd() + "_End"
            })
            .ToList();
        repository.Commit();
        if (result.Count == 3)
        {
            Assert.Equal("Begin_ON-001123_End", result[0].Trim);
            Assert.Equal("Begin_ON-001 123   _End", result[0].TrimStart);
            Assert.Equal("Begin_ ON-001  123_End", result[0].TrimEnd);
        }
    }
    [Fact]
    public void ToUpper_ToLower()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .Select(f => new
            {
                Col1 = f.OrderNo.ToLower() + "_AbCd".ToUpper(),
                Col2 = f.OrderNo.ToUpper() + "_AbCd".ToLower()
            })
            .ToSql(out _);
        Assert.Equal("SELECT CONCAT(LOWER(a.`OrderNo`),'_ABCD') AS `Col1`,CONCAT(UPPER(a.`OrderNo`),'_abcd') AS `Col2` FROM `sys_order` a", sql);

        repository.BeginTransaction();
        repository.Delete<Order>("1");
        var count = repository.Create<Order>(new Order
        {
            Id = "1",
            TenantId = "1",
            OrderNo = "On-ZwYx",
            BuyerId = 1,
            SellerId = 2,
            TotalAmount = 500,
            Products = new List<int> { 1, 2 },
            IsEnabled = true,
            CreatedAt = DateTime.Now,
            CreatedBy = 1,
            UpdatedAt = DateTime.Now,
            UpdatedBy = 1
        });
        var result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "1", "2", "3" }))
            .OrderBy(f => f.Id)
            .Select(f => new
            {
                Col1 = f.OrderNo.ToLower() + "_AbCd".ToUpper(),
                Col2 = f.OrderNo.ToUpper() + "_AbCd".ToLower()
            })
            .ToList();
        repository.Commit();
        if (count > 0)
        {
            Assert.Equal("on-zwyx_ABCD", result[0].Col1);
            Assert.Equal("ON-ZWYX_abcd", result[0].Col2);
        }
    }
    [Fact]
    public void Test_ToString()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .Select(f => new
            {
                Col1 = f.OrderNo.ToLower() + "_AbCd".ToUpper(),
                Col2 = f.OrderNo.ToUpper() + "_AbCd".ToLower()
            })
            .ToSql(out _);
        Assert.Equal("SELECT CONCAT(LOWER(a.`OrderNo`),'_ABCD') AS `Col1`,CONCAT(UPPER(a.`OrderNo`),'_abcd') AS `Col2` FROM `sys_order` a", sql);

        var strValue = "_AbCd";
        var sql1 = repository.From<Order>()
           .Select(f => new
           {
               Col1 = f.OrderNo.ToLower() + strValue.ToUpper(),
               Col2 = f.OrderNo.ToUpper() + strValue.ToLower()
           })
           .ToSql(out var dbParameters);
        Assert.Equal("SELECT CONCAT(LOWER(a.`OrderNo`),@p0) AS `Col1`,CONCAT(UPPER(a.`OrderNo`),@p1) AS `Col2` FROM `sys_order` a", sql1);
        Assert.Equal((string)dbParameters[0].Value, strValue.ToUpper());
        Assert.Equal(typeof(string), dbParameters[0].Value.GetType());
        Assert.Equal((string)dbParameters[1].Value, strValue.ToLower());
        Assert.Equal(typeof(string), dbParameters[1].Value.GetType());

        repository.BeginTransaction();
        repository.Delete<Order>("1");
        repository.Create<Order>(new Order
        {
            Id = "1",
            TenantId = "1",
            OrderNo = "On-ZwYx",
            BuyerId = 1,
            SellerId = 2,
            TotalAmount = 500,
            Products = new List<int> { 1, 2 },
            IsEnabled = true,
            CreatedAt = DateTime.Now,
            CreatedBy = 1,
            UpdatedAt = DateTime.Now,
            UpdatedBy = 1
        });
        repository.Commit();
        var result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "1", "2", "3" }))
            .OrderBy(f => f.Id)
            .Select(f => new
            {
                Col1 = f.OrderNo.ToLower() + "_AbCd".ToUpper(),
                Col2 = f.OrderNo.ToUpper() + "_AbCd".ToLower()
            })
            .ToList();
        Assert.Equal("on-zwyx_ABCD", result[0].Col1);
        Assert.Equal("ON-ZWYX_abcd", result[0].Col2);
    }
    [Fact]
    public void Update_Contains()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        int id = 1;
        var orderNos = new string[] { "ON_001", "ON_002", "ON_003" };
        var sql = repository.Update<Order>()
            .Set(f => new { TotalAmount = 100 })
            .Where(f => f.BuyerId == id || orderNos.Contains(f.OrderNo))
            .ToSql(out _);
        Assert.Equal("UPDATE `sys_order` SET `TotalAmount`=@p0 WHERE `BuyerId`=@p1 OR `OrderNo` IN (@p2,@p3,@p4)", sql);
        var count = repository.Update<Order>()
            .Set(f => new { TotalAmount = 100 })
            .Where(f => f.BuyerId == id || orderNos.Contains(f.OrderNo))
            .Execute();
        Assert.True(count > 0);
    }
    [Fact]
    public void Method_Convert1()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        int age = 23;
        var sql = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                StringAge = "Age-" + Convert.ToString(age),
                StringId1 = "Id-" + Convert.ToString(f.Id),
                DoubleAge = Convert.ToDouble(f.Age) * 2 - 10,
                Gender1 = f.Gender.ToString(),
                Gender2 = Convert.ToString(f.Gender),
                Age = Convert.ToString(f.Age)
            })
            .ToSql(out _);
        Assert.Equal("SELECT CONCAT('Age-',@p0) AS `StringAge`,CONCAT('Id-',CAST(a.`Id` AS CHAR)) AS `StringId1`,((CAST(a.`Age` AS DOUBLE)*2)-10) AS `DoubleAge`,a.`Gender` AS `Gender1`,a.`Gender` AS `Gender2`,CAST(a.`Age` AS CHAR) AS `Age` FROM `sys_user` a WHERE a.`Id`=1", sql);

        var result = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                StringAge = "Age-" + Convert.ToString(age),
                StringId1 = "Id-" + Convert.ToString(f.Id),
                DoubleAge = Convert.ToDouble(f.Age) * 2 - 10,
                Gender1 = f.Gender.ToString(),
                Gender2 = Convert.ToString(f.Gender),
                Age = Convert.ToString(f.Age)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        var sql1 = repository.From<UpdateEntity>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.EnumField,
                EnumField1 = f.EnumField.ToString(),
                EnumField2 = Convert.ToString(f.EnumField)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`EnumField`,(CASE a.`EnumField` WHEN 0 THEN 'Unknown' WHEN 1 THEN 'Female' WHEN 2 THEN 'Male' END) AS `EnumField1`,(CASE a.`EnumField` WHEN 0 THEN 'Unknown' WHEN 1 THEN 'Female' WHEN 2 THEN 'Male' END) AS `EnumField2` FROM `sys_update_entity` a WHERE a.`Id`=1", sql1);
        var result1 = repository.From<UpdateEntity>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.EnumField,
                EnumField1 = f.EnumField.ToString(),
                EnumField2 = Convert.ToString(f.EnumField)
            })
            .First();
        Assert.NotNull(result1);
        Assert.True(result1.EnumField1 == result1.EnumField.ToString());
        Assert.True(result1.EnumField2 == Convert.ToString(result1.EnumField));
    }
    [Fact]
    public async Task Method_Convert2()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        byte id = 1;
        await repository.From<User>()
            .Where(f => f.Id == id)
            .Select(f => (short)f.Age)
            .FirstAsync();
    }
    [Fact]
    public void SqlIn()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, new int[] { 1, 2, 3 }))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (1,2,3)", sql);

        sql = repository.From<User>()
            .Where(f => Sql.In(f.CreatedAt, new DateTime[] { DateTime.Parse("2023-03-03"), DateTime.Parse("2023-03-03 00:00:00"), DateTime.Parse("2023-03-03 06:06:06") }))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`CreatedAt` IN ('2023-03-03 00:00:00.000','2023-03-03 00:00:00.000','2023-03-03 06:06:06.000')", sql);
    }
    [Fact]
    public async Task ComplexDeferredCall()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NewField = $"{f.Age.IsNull(20)}-{f.Gender}"
            })
            .ToSql(out _);
        Assert.Equal("SELECT CONCAT(CAST(IFNULL(a.`Age`,20) AS CHAR),'-',a.`Gender`) AS `NewField` FROM `sys_user` a WHERE a.`Id`=1", sql);

        var result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NewField = $"{f.Age}-{f.Gender}",
                f.Age,
                f.Gender
            })
            .FirstAsync();
        var age = result.Age == 0 ? 20 : result.Age;
        Assert.True(result.NewField == $"{age}-{result.Gender}");

        sql = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NewField = $"{f.Age.IsNull(20)}-{f.Gender}"
            })
            .ToSql(out _);
        Assert.Equal("SELECT CONCAT(CAST(IFNULL(a.`Age`,20) AS CHAR),'-',a.`Gender`) AS `NewField` FROM `sys_user` a WHERE a.`Id`=1", sql);

        result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NewField = $"{f.Age.IsNull(20)}-{f.Gender}",
                f.Age,
                f.Gender
            })
            .FirstAsync();
        age = result.Age == 0 ? 20 : result.Age;
        Assert.Equal(result.NewField, $"{age}-{result.Gender.ToString()}");

        sql = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NewField = $"{f.Age.IsNull(20)}-{f.Gender.ToDescription()}"
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Age`,a.`Gender` FROM `sys_user` a WHERE a.`Id`=1", sql);

        var result1 = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NewField = $"{f.Age.IsNull(20)}-{f.Gender.ToDescription()}",
                f.Age,
                f.Gender
            })
            .FirstAsync();
        age = result1.Age == 0 ? 20 : result.Age;
        Assert.Equal(result1.NewField, $"{age}-{result1.Gender.ToDescription()}");

        sql = repository.From<UpdateEntity>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NewField = $"{f.EnumField}"
            })
            .ToSql(out _);
        Assert.Equal("SELECT CONCAT(CASE a.`EnumField` WHEN 0 THEN 'Unknown' WHEN 1 THEN 'Female' WHEN 2 THEN 'Male' END) AS `NewField` FROM `sys_update_entity` a WHERE a.`Id`=1", sql);

        var result2 = await repository.From<UpdateEntity>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.EnumField,
                NewField = $"{f.EnumField}"
            })
            .FirstAsync();
        Assert.Equal(result2.NewField, $"{result2.EnumField}");
    }
    [Fact]
    public void ContainsEquals()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => f.Name == string.Concat("千", "11"))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Name`='千11'", sql);
        sql = repository.From<User>()
            .Where(f => f.Name.Equals("千11"))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Name`='千11'", sql);
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
        var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES (@Id,@BrandNo,@Name,1,NOW(),@User,NOW(),@User)";
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
        Assert.Equal("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.Equal(1, (int)dbParameters[0].Value);
        Assert.Equal("1", (string)dbParameters[1].Value);
        Assert.Equal("leafkevin", (string)dbParameters[2].Value);
        Assert.Equal(25, (int)dbParameters[3].Value);
        Assert.Equal(1, (int)dbParameters[4].Value);
        if (dbParameters[5] is MySqlParameter dbParameter)
        {
            Assert.Equal(MySqlDbType.Enum, dbParameter.MySqlDbType);
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
        Assert.Equal("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.Equal(1, (int)dbParameters[0].Value);
        Assert.Equal("1", (string)dbParameters[1].Value);
        Assert.Equal("leafkevin", (string)dbParameters[2].Value);
        Assert.Equal(25, (int)dbParameters[3].Value);
        Assert.Equal(1, (int)dbParameters[4].Value);
        if (dbParameters[5] is MySqlParameter dbParameter1)
        {
            Assert.Equal(MySqlDbType.Enum, dbParameter1.MySqlDbType);
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
        Assert.Equal("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Age,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
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
        Assert.Equal("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Age,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);

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
        Assert.Equal("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`,`SomeTimes`,`GuidField`) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@SomeTimes,@GuidField)", sql);

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
        Assert.Equal("INSERT INTO `sys_user` (`Id`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`,`GuidField`) VALUES (@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@GuidField)", sql);

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
        Assert.Equal("INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)", sql);

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
        Assert.Equal("INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)", sql);

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
    public async Task Insert_Select_From_Table1()
    {
        var repository = this.dbFactory.Create();
        var id = 2;
        var brandId = 1;
        var name = "雪中飞羽绒裤";
        int categoryId = 1;
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
        Assert.Equal("INSERT IGNORE INTO `sys_product` (`Id`,`ProductNo`,`Name`,`Price`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT @p1,CONCAT('PN_',@p2),@p3,25.85,b.`Id`,@p4,b.`CompanyId`,1,1,NOW(),1,NOW() FROM `sys_brand` b WHERE b.`Id`=@p0", sql);

        repository.BeginTransaction();
        repository.Delete<Product>(id);
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
        Assert.Equal(0, count);
    }
    [Fact]
    public async Task Insert_Select_From_Table2()
    {
        var repository = this.dbFactory.Create();
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
        Assert.Equal("INSERT IGNORE INTO `sys_order_detail` (`Id`,`TenantId`,`OrderId`,`ProductId`,`Price`,`Quantity`,`Amount`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT '7','1',b.`Id`,c.`Id`,c.`Price`,3,(c.`Price`*3),b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt` FROM `sys_order` b,`sys_product` c WHERE b.`Id`='3' AND c.`Id`=1", sql);
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
        Assert.Equal("INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`ProductCount`,`TotalAmount`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) WITH \r\n`orders`(`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`ProductCount`,`TotalAmount`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) AS \r\n(\r\nSELECT a.`OrderId`,'1',CONCAT('ON-',a.`OrderId`),1,1,'Taobao',2,SUM(a.`Amount`),1,NOW(),1,NOW(),1 FROM `sys_order_detail` a GROUP BY a.`OrderId`\r\n)\r\nSELECT b.`Id`,b.`TenantId`,b.`OrderNo`,b.`BuyerId`,b.`SellerId`,b.`BuyerSource`,b.`ProductCount`,b.`TotalAmount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `orders` b", sql);
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
        Assert.Equal("INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`ProductCount`,`TotalAmount`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) SELECT b.`OrderId`,'1',CONCAT('ON-',b.`OrderId`),1,1,'Taobao',2,SUM(b.`Amount`),1,NOW(),1,NOW(),1 FROM `sys_order_detail` b GROUP BY b.`OrderId`", sql);
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
        Assert.Equal("INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`TotalAmount`,`BuyerId`,`BuyerSource`,`SellerId`,`ProductCount`,`Products`,`Disputes`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) VALUES (@Id,@TenantId,@OrderNo,@TotalAmount,@BuyerId,@BuyerSource,@SellerId,@ProductCount,@Products,@Disputes,@IsEnabled,@CreatedBy,@CreatedAt,@UpdatedBy,@UpdatedAt)", sql);
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
        Assert.Equal("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql1);
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
        Assert.Equal("INSERT INTO `sys_company` (`Name`,`Nature`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) VALUES (@Name,@Nature,@IsEnabled,@CreatedBy,@CreatedAt,@UpdatedBy,@UpdatedAt)", sql2);
        Assert.Equal("@Nature", parameters2[1].ParameterName);
        Assert.True(parameters2[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters2[1].Value == CompanyNature.Internet.ToString());
    }
    [Fact]
    public async Task Insert_Ignore()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
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
        Assert.Equal("INSERT IGNORE INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql1);
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
        Assert.Equal(0, count);
    }
    [Fact]
    public async Task Insert_Ignore_OnlyFields()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
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
            .OnlyFields(f => new { f.Id, f.TenantId, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
            .ToSql(out var parameters);
        Assert.Equal("INSERT IGNORE INTO `sys_user` (`Id`,`TenantId`,`Name`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.Equal(8, parameters.Count);
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
            .OnlyFields(f => new { f.Id, f.TenantId, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
            .ExecuteAsync();
        var user = repository.Get<User>(1);
        repository.Commit();
        Assert.Equal(1, count);
        Assert.Equal(0, user.CompanyId);
        Assert.Equal(Gender.Unknown, user.Gender);
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
        Assert.Equal(0, count);
    }
    [Fact]
    public async Task Insert_OnDuplicateKeyUpdate()
    {
        var repository = this.dbFactory.Create();
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
             .OnDuplicateKeyUpdate(x => x
                .Set(new
                {
                    TotalAmount = 25,
                    Products = new List<int> { 1, 2 }
                })
                .Set(buyerSource.HasValue, f => f.BuyerSource, buyerSource)
             )
            .ToSql(out _);
        Assert.Equal("INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`TotalAmount`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@OrderNo,@BuyerId,@SellerId,@TotalAmount,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON DUPLICATE KEY UPDATE `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerSource`=@BuyerSource", sql1);

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
             .OnDuplicateKeyUpdate(x => x
                .Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
                .Set(f => f.Products, f => x.Values(f.Products)))
            .ToSql(out _);
        Assert.Equal("INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`TotalAmount`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@OrderNo,@BuyerId,@SellerId,@BuyerSource,@TotalAmount,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON DUPLICATE KEY UPDATE `TotalAmount`=VALUES(`TotalAmount`),`Products`=VALUES(`Products`)", sql2);

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
             .OnDuplicateKeyUpdate(x => x.UseAlias()
                .Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
                .Set(f => f.Products, f => x.Values(f.Products)))
            .ToSql(out _);
        Assert.Equal("INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`TotalAmount`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@OrderNo,@BuyerId,@SellerId,@BuyerSource,@TotalAmount,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=newRow.`TotalAmount`,`Products`=newRow.`Products`", sql3);

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
        Assert.Equal(1, count);
        Assert.Equal(500, order.TotalAmount);
        Assert.Null(order.Products);

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
        Assert.Equal(2, count);
        Assert.Equal(600, order.TotalAmount);
        Assert.True(new JsonTypeHandler().ToFieldValue(null, order.Products).ToString() == new JsonTypeHandler().ToFieldValue(null, new List<int> { 1, 2 }).ToString());
    }
    [Fact]
    public async Task Insert_Returning()
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
            .Returning(f => new { f.Id, f.TenantId })
            .ToSql(out var parameters1);
        Assert.Equal("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) RETURNING Id,TenantId", sql1);
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
            .Returning<User>("*")
            .ToSql(out var parameters2);
        Assert.Equal("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) RETURNING *", sql2);
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
        Assert.Equal(2, result2.Id);
        Assert.Equal("1", result2.TenantId);
    }
    [Fact]
    public async Task Insert_Returnings()
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
            .Returning(f => new { f.Id, f.ProductNo })
            .ToSql(out var parameters1);
        Assert.Equal("INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2) RETURNING Id,ProductNo", sql1);

        await repository.BeginTransactionAsync();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var results1 = await repository.Create<Product>()
            .WithBulk(products)
            .Returning(f => new { f.Id, f.ProductNo })
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
            .Returning<Product>("*")
            .ToSql(out var parameters2);
        Assert.Equal("INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2) RETURNING *", sql2);

        await repository.BeginTransactionAsync();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var result2 = await repository.Create<Product>()
            .WithBulk(products)
            .Returning<Product>("*")
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



    [Fact]
    public async Task QueryFirst()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var result = repository.QueryFirst<User>(f => f.Id == 1);
        if (result != null)
        {
            Assert.NotNull(result.Name);
        }
        result = repository.QueryFirst<User>("SELECT * FROM sys_user where Id=1");
        if (result != null)
        {
            Assert.NotNull(result.Name);
        }
        var result1 = await repository.QueryFirstAsync<User>(f => f.Name == "leafkevin");
        var result2 = await repository.QueryFirstAsync<User>(new { Name = "leafkevin" });
        if (result1 != null && result2 != null)
        {
            Assert.True(result1.Id == result2.Id);
        }
    }
    [Fact]
    public async Task Get()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var result = repository.Get<User>(1);
        Assert.Equal("leafkevin", result.Name);
        var user = await repository.GetAsync<User>(new { Id = 1 });
        Assert.True(user.Name == result.Name);
    }
    [Fact]
    public async Task Query()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var result = await repository.QueryAsync<Product>(f => f.ProductNo.Contains("PN-00"));
        Assert.True(result.Count >= 3);
    }
    [Fact]
    public async Task QueryPage()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var result = repository.From<OrderDetail>()
            .Where(f => f.ProductId == 1)
            .OrderByDescending(f => f.CreatedAt)
            .Page(2, 1)
            .ToPageList();
        var count = await repository.From<OrderDetail>().Where(f => f.ProductId == 1).CountAsync();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        Assert.True(result.TotalCount == count);
        Assert.True(result.Data.Count == result.Count);
        Assert.Equal(1, result.Count);
    }
    [Fact]
    public async Task QueryDictionary()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var result = await repository.QueryDictionaryAsync<Product, int, string>(f => f.ProductNo.Contains("PN-00"), f => f.Id, f => f.Name);
        Assert.True(result.Count >= 3);
    }
    class OrderBuyerInfo
    {
        public string OrderId { get; set; }
        public string OrderNo { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int ProductTotal { get; set; }
    }
    [Fact]
    public async Task QueryRawSql()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var result = await repository.QueryAsync<Product>("SELECT * FROM sys_product where Id=@ProductId", new { ProductId = 1 });
        Assert.NotNull(result);
        Assert.Single(result);
    }
    [Fact]
    public void FromQuery_SubQuery()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository
            .From(f => f.From<OrderDetail>()
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                Buyer = y,
                x.ProductCount
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`OrderId`,a.`BuyerId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail` a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1", sql);

        var result = repository
            .From(f => f.From<OrderDetail>()
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                Buyer = y,
                x.ProductCount
            })
            .ToList();
        if (result.Count > 0)
        {
            Assert.NotNull(result[0]);
            Assert.NotNull(result[0].Group);
            Assert.NotNull(result[0].Buyer);
            Assert.True(result[0].ProductCount > 1);
        }
        var sql1 = repository
           .From(f => f.From<Order>()
               .Select(x => new { x.Id, x.OrderNo, x.BuyerId, x.SellerId }))
           .Select(x => new { Order = x })
           .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`OrderNo`,a.`BuyerId`,a.`SellerId` FROM (SELECT a.`Id`,a.`OrderNo`,a.`BuyerId`,a.`SellerId` FROM `sys_order` a) a", sql1);

        var result1 = repository
            .From(f => f.From<Order>()
                .Select(x => new { x.Id, x.OrderNo, x.BuyerId, x.SellerId }))
            .Select(x => new { Order = x })
            .First();
        Assert.NotNull(result1);
        Assert.NotNull(result1.Order);
    }
    [Fact]
    public void FromQuery_SubQuery1()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.From(f => f.From<Page, Menu>('o')
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { MenuId = y.Id, y.ParentId, x.Url }))
            .InnerJoin<Menu>((a, b) => a.MenuId == b.Id)
            .Where((a, b) => a.MenuId == b.Id)
            .Select((a, b) => new { a.MenuId, b.Name, a.ParentId, a.Url })
            .ToSql(out _);
        Assert.Equal("SELECT a.`MenuId`,b.`Name`,a.`ParentId`,a.`Url` FROM (SELECT p.`Id` AS `MenuId`,p.`ParentId`,o.`Url` FROM `sys_page` o,`sys_menu` p WHERE o.`Id`=p.`PageId`) a INNER JOIN `sys_menu` b ON a.`MenuId`=b.`Id` WHERE a.`MenuId`=b.`Id`", sql);

        var result = repository.From(f => f.From<Page, Menu>('o')
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { MenuId = y.Id, y.ParentId, x.Url }))
            .InnerJoin<Menu>((a, b) => a.MenuId == b.Id)
            .Where((a, b) => a.MenuId == b.Id)
            .Select((a, b) => new { a.MenuId, b.Name, a.ParentId, a.Url })
            .ToList();

        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_SubQuery2()
    {
        var repository = this.dbFactory.Create();
        var count = 1;
        var sql = repository
            .From(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => Sql.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = Sql.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .Include((a, b) => b.Details)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT b.`Disputes`,a.`BuyerId`,a.`OrderId`,a.`OrderNo`,a.`ProductTotal`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM (SELECT b.`BuyerId`,b.`Id` AS `OrderId`,b.`OrderNo`,COUNT(DISTINCT c.`ProductId`) AS `ProductTotal` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` LEFT JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` GROUP BY b.`BuyerId`,b.`Id`,b.`OrderNo` HAVING COUNT(DISTINCT c.`ProductId`)>@p0) a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id`", sql);
        Assert.True((int)dbParameters[0].Value == count);

        var result = repository
            .From(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => Sql.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = Sql.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .Include((a, b) => b.Details)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .First();
        if (result != null)
        {
            Assert.NotNull(result.Disputes);
            Assert.NotNull(result.Order);
            Assert.NotNull(result.Order.Details);
            Assert.True(result.Order.Details.Count > 0);
            Assert.True(result.Order.Details[0].Amount > 0);
        }

        var amount = 100;
        sql = repository
            .From(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => Sql.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = Sql.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .IncludeMany((a, b) => b.Details, f => f.Amount > amount)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .ToSql(out dbParameters);
        Assert.Equal("SELECT b.`Disputes`,a.`BuyerId`,a.`OrderId`,a.`OrderNo`,a.`ProductTotal`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM (SELECT b.`BuyerId`,b.`Id` AS `OrderId`,b.`OrderNo`,COUNT(DISTINCT c.`ProductId`) AS `ProductTotal` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` LEFT JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` GROUP BY b.`BuyerId`,b.`Id`,b.`OrderNo` HAVING COUNT(DISTINCT c.`ProductId`)>@p0) a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id`", sql);
        Assert.Single(dbParameters);
        Assert.True((int)dbParameters[0].Value == count);

        result = repository
            .From(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => Sql.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = Sql.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .IncludeMany((a, b) => b.Details, f => f.Amount > amount)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .First();
        if (result != null)
        {
            Assert.NotNull(result.Disputes);
            Assert.NotNull(result.Order);
            Assert.NotNull(result.Order.Details);
            Assert.True(result.Order.Details.Count > 0);
            foreach (var orderDetail in result.Order.Details)
            {
                Assert.True(result.Order.Details[0].Amount > amount);
            }
        }
    }
    [Fact]
    public void FromQuery_SubQuery3()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From(f => f.From<Order, OrderDetail>('a')
                .Where((a, b) => a.Id == b.OrderId)
                .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                .Select((x, a, b) => new { x.Grouping, ProductTotal = Sql.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
            .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
            .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
            .ToSql(out _);
        Assert.Equal("SELECT a.`BuyerId`,a.`OrderId`,a.`BuyerId`,a.`ProductTotal`,b.`Name` AS `BuyerName`,a.`BuyerId1` AS `BuyerId2` FROM (SELECT a.`BuyerId`,a.`Id` AS `OrderId`,COUNT(DISTINCT b.`ProductId`) AS `ProductTotal`,a.`BuyerId` AS `BuyerId1` FROM `sys_order` a,`sys_order_detail` b WHERE a.`Id`=b.`OrderId` GROUP BY a.`BuyerId`,a.`Id` HAVING COUNT(DISTINCT b.`ProductId`)>0) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id`", sql);

        var result = repository.From(f => f.From<Order, OrderDetail>('a')
                .Where((a, b) => a.Id == b.OrderId)
                .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                .Select((x, a, b) => new { x.Grouping, ProductTotal = Sql.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
            .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
            .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
            .First();
        if (result != null)
        {
            Assert.NotNull(result);
            Assert.NotNull(result.Grouping);
            Assert.NotNull(result.BuyerName);
        }
    }
    [Fact]
    public void FromQuery_SubQuery4()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User, Order, OrderDetail>()
            .InnerJoin((a, b, c) => a.Id == b.BuyerId)
            .LeftJoin((a, b, c) => b.Id == c.OrderId)
            .Select((a, b, c) => new { OrderId = b.Id, b.OrderNo, b.Disputes, b.BuyerId, Buyer = a, TotalAmount = Sql.Sum(c.Amount) })
            .ToSql(out _);
        Assert.Equal("SELECT b.`Id` AS `OrderId`,b.`OrderNo`,b.`Disputes`,b.`BuyerId`,a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,SUM(c.`Amount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` LEFT JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId`", sql);

        var result = repository.From<User, Order, OrderDetail>()
                 .InnerJoin((a, b, c) => a.Id == b.BuyerId)
                 .LeftJoin((a, b, c) => b.Id == c.OrderId)
            .Select((a, b, c) => new { OrderId = b.Id, b.OrderNo, b.Disputes, b.BuyerId, Buyer = a, TotalAmount = Sql.Sum(c.Amount) })
            .First();
        if (result != null)
        {
            Assert.NotNull(result);
            Assert.NotNull(result.OrderId);
            Assert.True(result.BuyerId > 0);
            Assert.NotNull(result.OrderNo);
            Assert.NotNull(result.Buyer);
        }
    }
    [Fact]
    public async Task WithTable_SubQuery()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Menu>()
             .WithTable(f => f.From<Page, Menu>('c')
                 .Where((a, b) => a.Id == b.PageId)
                 .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
             .Where((a, b) => a.Id == b.Id)
             .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
             .ToSql(out _);
        Assert.Equal(@"SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `sys_menu` a,(SELECT d.`Id`,d.`ParentId`,c.`Url` FROM `sys_page` c,`sys_menu` d WHERE c.`Id`=d.`PageId`) b WHERE a.`Id`=b.`Id`", sql);
        var result = repository.From<Menu>()
             .WithTable(f => f.From<Page, Menu>('c')
                 .Where((a, b) => a.Id == b.PageId)
                 .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
             .Where((a, b) => a.Id == b.Id)
             .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
             .First();
        Assert.NotNull(result);

        var sql1 = repository.From<User>()
            .WithTable(f => f.From<Order>()
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .GroupBy((a, b) => new { OrderId = a.Id, a.BuyerId })
                .Select((x, a, b) => new { x.Grouping, ProductCount = x.CountDistinct(b.ProductId) }))
            .InnerJoin((x, y) => x.Id == y.Grouping.BuyerId)
            .Where((a, b) => b.ProductCount > 1)
            .Select((x, y) => new
            {
                y.Grouping,
                Buyer = x,
                y.ProductCount
            })
            .ToSql(out _);
        Assert.Equal("SELECT b.`OrderId`,b.`BuyerId`,a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`ProductCount` FROM `sys_user` a INNER JOIN (SELECT a.`Id` AS `OrderId`,a.`BuyerId`,COUNT(DISTINCT b.`ProductId`) AS `ProductCount` FROM `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` GROUP BY a.`Id`,a.`BuyerId`) b ON a.`Id`=b.`BuyerId` WHERE b.`ProductCount`>1", sql1);

        var result1 = repository.From<User>()
            .WithTable(f => f.From<Order>()
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .GroupBy((a, b) => new { OrderId = a.Id, a.BuyerId })
                .Select((x, a, b) => new { x.Grouping, ProductCount = x.CountDistinct(b.ProductId) }))
            .InnerJoin((x, y) => x.Id == y.Grouping.BuyerId)
            .Where((a, b) => b.ProductCount > 1)
            .Select((x, y) => new
            {
                y.Grouping,
                Buyer = x,
                y.ProductCount
            })
            .ToList();
        Assert.True(result1.Count > 0);

        var sql2 = repository
             .From<Order, User>()
             .WithTable(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, TotalAmount = x.Sum(b.Amount) }))
            .Where((a, b, c) => a.BuyerId == b.Id && a.Id == c.OrderId)
            .Select((a, b, c) => new { Order = a, Buyer = b, OrderId = a.Id, a.BuyerId, c.TotalAmount })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`Id` AS `OrderId`,a.`BuyerId`,c.`TotalAmount` FROM `sys_order` a,`sys_user` b,(SELECT a.`Id` AS `OrderId`,SUM(b.`Amount`) AS `TotalAmount` FROM `sys_order` a,`sys_order_detail` b,`sys_user` c WHERE a.`Id`=b.`OrderId` AND a.`BuyerId`=c.`Id` AND c.`Age`>20 GROUP BY a.`Id`,a.`BuyerId` HAVING SUM(b.`Amount`)>500) c WHERE a.`BuyerId`=b.`Id` AND a.`Id`=c.`OrderId`", sql2);

        var result2 = await repository
             .From<Order, User>()
             .WithTable(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, TotalAmount = x.Sum(b.Amount) }))
            .Where((a, b, c) => a.BuyerId == b.Id && a.Id == c.OrderId)
            .Select((a, b, c) => new { Order = a, Buyer = b, OrderId = a.Id, a.BuyerId, c.TotalAmount })
            .ToListAsync();
        Assert.True(result2.Count > 0);
    }
    [Fact]
    public void FromQuery_InnerJoin()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
           .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
           .Where((a, b) => b.ProductCount > 1)
           .Select((x, y) => new
           {
               User = x,
               Order = y
           })
           .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE b.`ProductCount`>1", sql);

        var result = repository.From<User>()
            .Include(x => x.Orders)
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => b.ProductCount > 1)
            .Select((x, y) => new
            {
                User = x,
                Order = y
            })
            .ToList();
        if (result.Count > 0)
        {
            Assert.NotNull(result[0]);
            Assert.NotNull(result[0].User);
            Assert.NotNull(result[0].Order);
            Assert.True(result[0].Order.ProductCount > 1);
        }
    }
    [Fact]
    public async Task FromQuery_InnerJoin1()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
          .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
          .InnerJoin(f => f.From<OrderDetail>()
              .GroupBy(x => x.OrderId)
              .Select((x, y) => new
              {
                  y.OrderId,
                  ProductCount = x.CountDistinct(y.ProductId)
              }), (a, b, c) => b.Id == c.OrderId)
          .Where((a, b, c) => c.ProductCount > 2)
          .Select((a, b, c) => new
          {
              User = a,
              Order = b,
              c.ProductCount
          })
          .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,c.`ProductCount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail` a GROUP BY a.`OrderId`) c ON b.`Id`=c.`OrderId` WHERE c.`ProductCount`>2", sql);

        var result = await repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .InnerJoin(f => f.From<OrderDetail>()
                .GroupBy(x => x.OrderId)
                .Select((x, y) => new
                {
                    y.OrderId,
                    ProductCount = x.CountDistinct(y.ProductId)
                }), (a, b, c) => b.Id == c.OrderId)
            .Where((a, b, c) => c.ProductCount > 2)
            .Select((a, b, c) => new
            {
                User = a,
                Order = b,
                c.ProductCount
            })
            .ToListAsync();
        if (result.Count > 0)
        {
            Assert.NotNull(result[0]);
            Assert.NotNull(result[0].User);
            Assert.NotNull(result[0].Order);
            Assert.True(result[0].ProductCount > 2);
        }
    }
    [Fact]
    public void Join_Cte()
    {
        this.Initialize();
        var menuId = 1;
        var pageId = 1;
        var repository = this.dbFactory.Create();
        var menuPageList = repository.From<Page, Menu>()
            .Where((a, b) => a.Id == b.PageId && b.Id > menuId.ToParameter("@MenuId"))
            .Select((x, y) => new { MenuId = y.Id, y.ParentId, x.Url })
            .AsCteTable("menuPageList");
        var sql = repository.From<Menu>()
            .InnerJoin(menuPageList, (a, b) => a.Id == b.MenuId && a.PageId > pageId)
            .Select((a, b) => new { b.MenuId, a.Name, b.ParentId, a.PageId, b.Url })
            .ToSql(out var dbParameters);
        Assert.Equal(@"WITH `menuPageList`(`MenuId`,`ParentId`,`Url`) AS 
(
SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE a.`Id`=b.`PageId` AND b.`Id`>@MenuId
)
SELECT b.`MenuId`,a.`Name`,b.`ParentId`,a.`PageId`,b.`Url` FROM `sys_menu` a INNER JOIN `menuPageList` b ON a.`Id`=b.`MenuId` AND a.`PageId`>@p1", sql);
        Assert.Equal(2, dbParameters.Count);
        Assert.Equal("@MenuId", dbParameters[0].ParameterName);
        Assert.True((int)dbParameters[0].Value == menuId);
        Assert.True((int)dbParameters[1].Value == pageId);

        var result = repository.From<Menu>()
            .InnerJoin(menuPageList, (a, b) => a.Id == b.MenuId && a.PageId > pageId)
            .Select((a, b) => new { b.MenuId, a.Name, b.ParentId, a.PageId, b.Url })
            .ToList();
        Assert.True(result.Count > 0);
        foreach (var item in result)
        {
            Assert.True(item.MenuId > menuId);
            Assert.True(item.PageId > pageId);
        }
        int parentId = 10;
        sql = repository.From<Menu>()
            .InnerJoin<Page>((a, b) => a.PageId == b.Id && b.Id > pageId)
            .Select((a, b) => new { MenuId = a.Id, a.ParentId, b.Url })
            .Union(f => f.From(menuPageList)
                .Where(f => f.ParentId < parentId)
                .Select())
            .ToSql(out dbParameters);
        Assert.Equal(@"WITH `menuPageList`(`MenuId`,`ParentId`,`Url`) AS 
(
SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE a.`Id`=b.`PageId` AND b.`Id`>@MenuId
)
SELECT a.`Id` AS `MenuId`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN `sys_page` b ON a.`PageId`=b.`Id` AND b.`Id`>@p0 UNION
SELECT a.`MenuId`,a.`ParentId`,a.`Url` FROM `menuPageList` a WHERE a.`ParentId`<@p2", sql);
        Assert.Equal(3, dbParameters.Count);
        Assert.Equal("@p0", dbParameters[0].ParameterName);
        Assert.Equal("@MenuId", dbParameters[1].ParameterName);
        Assert.Equal("@p2", dbParameters[2].ParameterName);
        Assert.True((int)dbParameters[0].Value == menuId);
        Assert.True((int)dbParameters[1].Value == pageId);
        Assert.True((int)dbParameters[2].Value == parentId);

        var result1 = repository.From<Menu>()
            .InnerJoin<Page>((a, b) => a.PageId == b.Id && b.Id > pageId)
            .Select((a, b) => new { MenuId = a.Id, a.ParentId, b.Url })
            .Union(f => f.From(menuPageList)
                .Where(f => f.ParentId < parentId)
                .Select())
            .ToList();
        Assert.True(result1.Count > 0);
        foreach (var item in result1)
        {
            Assert.True(item.MenuId > menuId);
            Assert.True(item.ParentId < parentId);
        }
    }
    [Fact]
    public async Task FromQuery_Include()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.From<Product>()
            .Include(f => f.Brand)
            .Where(f => f.ProductNo.Contains("PN-00"))
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'", sql);

        var result = await repository.From<Product>()
            .Include(f => f.Brand)
            .Where(f => f.ProductNo.Contains("PN-00"))
            .OrderBy(f => f.Id)
            .ToListAsync();

        if (result.Count > 0)
        {
            Assert.NotNull(result[0].Brand);
            Assert.Equal("BN-001", result[0].Brand.BrandNo);
        }
        if (result.Count > 1)
        {
            Assert.NotNull(result[1].Brand);
            Assert.Equal("BN-002", result[1].Brand.BrandNo);
        }
    }
    [Fact]
    public void FromQuery_IncludeMany()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Include((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.Equal(2, result.Count);
        Assert.NotNull(result[0].Order);
        Assert.NotNull(result[0].Order.Details);
        Assert.NotEmpty(result[0].Order.Details);
        Assert.Equal(3, result[0].Order.Details.Count);
        result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Include((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, "1", "2", "3"))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.Equal(2, result.Count);
        Assert.NotNull(result[0].Order);
        Assert.NotNull(result[0].Order.Details);
        Assert.NotEmpty(result[0].Order.Details);
        Assert.Equal(3, result[0].Order.Details.Count);

        result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.Equal(2, result.Count);
        Assert.NotNull(result[0].Order);
        Assert.NotNull(result[0].Order.Details);
        Assert.NotEmpty(result[0].Order.Details);
        Assert.Equal(3, result[0].Order.Details.Count);
        result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, "1", "2", "3"))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.Equal(2, result.Count);
        Assert.NotNull(result[0].Order);
        Assert.NotNull(result[0].Order.Details);
        Assert.NotEmpty(result[0].Order.Details);
        Assert.Equal(3, result[0].Order.Details.Count);
    }
    [Fact]
    public void FromQuery_IncludeMany_Filter()
    {
        Initialize();
        int productId = 1;
        var repository = this.dbFactory.Create();
        var result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details, f => f.ProductId == productId)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Buyer = y, Test = x.OrderNo + "_" + y.Age % 4 })
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.NotNull(result[0].Order);
        Assert.NotNull(result[0].Order.Details);
        Assert.NotEmpty(result[0].Order.Details);
        Assert.Single(result[0].Order.Details);
        Assert.True(result[0].Order.Details[0].ProductId == productId);
        Assert.Single(result[1].Order.Details);
        Assert.True(result[1].Order.Details[0].ProductId == productId);
    }
    [Fact]
    public async Task FromQuery_Include_ThenInclude()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var result = await repository.From<Order>()
            .InnerJoin<User>((a, b) => a.SellerId == b.Id)
            .Include((x, y) => x.Buyer)
            .ThenInclude(f => f.Company)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Seller = y })
            .ToListAsync();

        if (result.Count > 0)
        {
            Assert.NotNull(result[0].Order.Buyer);
            Assert.NotNull(result[0].Order.Buyer.Company);
            Assert.NotNull(result[0].Order.Buyer.SomeTimes.ToString());
        }
    }
    //[Fact]
    //public async Task FromQuery_IncludeMany_ThenInclude()
    //{
    //    var repository = this.dbFactory.Create();
    //    var result = await repository.From<Order>()
    //        .IncludeMany(f => f.Details)
    //        .ThenInclude(f => f.Product)
    //        .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
    //        .Where((a, b) => a.TotalAmount > 300)
    //        .Select((x, y) => new { Order = x, Buyer = y })
    //        .ToListAsync();
    //    Assert.True(result.Count == 2);
    //    Assert.NotNull(result[0].Order.Details);
    //    Assert.NotEmpty(result[0].Order.Details);
    //    Assert.True(result[0].Order.Details.Count == 3);
    //    Assert.NotNull(result[0].Order.Details[0].Product);
    //    Assert.NotNull(result[0].Order.Details[1].Product);
    //    Assert.NotNull(result[0].Order.Details[2].Product);
    //}
    [Fact]
    public void QueryPage_Include()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var result = repository.From<OrderDetail>()
            .Include(f => f.Product)
            .Where(f => f.ProductId == 1)
            .OrderBy(f => f.OrderId)
            .Page(2, 1)
            .ToPageList();
        var count = repository.From<OrderDetail>()
            .Where(f => f.ProductId == 1)
            .Count();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        Assert.True(result.TotalCount == count);
        Assert.True(result.Data.Count == result.Count);
        Assert.Equal(1, result.Count);
        Assert.NotEmpty(result.Data);
        Assert.NotNull(result.Data[0].Product);
        Assert.Equal(1, result.Data[0].Product.Id);
    }
    [Fact]
    public void FromQuery_Ignore_Include()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .IncludeMany((a, b) => a.Orders)
            .ThenIncludeMany(f => f.Details)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.CreatedAt.Date })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,CONVERT(b.`CreatedAt`,DATE)", sql);
        var result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            //.IncludeMany((a, b) => a.Orders)
            //.ThenIncludeMany(f => f.Details)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.CreatedAt.Date })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_Groupby()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
           .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
           .IncludeMany((x, y) => x.Details)
           .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
           .Select((x, y) => new { Order = x, Buyer = y })
           .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`TotalAmount`>300 AND a.`Id` IN ('1','2','3')", sql);

        var result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.Equal(2, result.Count);
        Assert.NotNull(result[0].Order);
        Assert.NotNull(result[0].Order.Details);
        Assert.NotEmpty(result[0].Order.Details);
        Assert.Equal(3, result[0].Order.Details.Count);

        var sql1 = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`", sql1);
        var result1 = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        if (result1.Count > 0)
        {
            Assert.NotNull(result1[0].Grouping);
            Assert.NotNull(result1[0].Grouping.Name);
        }
        if (result1.Count > 1)
        {
            Assert.NotNull(result1[1].Grouping);
            Assert.NotNull(result1[1].Grouping.Name);
        }
    }
    [Fact]
    public void FromQuery_Groupby_Fields()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { UserId = a.Id, a.Name, CreatedDate = b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id })
            .Select((x, a, b) => new
            {
                UserId1 = x.Grouping.UserId,
                UserName = x.Grouping.Name,
                CreatedDate1 = x.Grouping.CreatedDate,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
           .ToSql(out _);
        Assert.Equal("SELECT a.`Id` AS `UserId1`,a.`Name` AS `UserName`,CONVERT(b.`CreatedAt`,DATE) AS `CreatedDate1`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`", sql);
        var result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { UserId = a.Id, a.Name, CreatedDate = b.CreatedAt.Date })
            .OrderBy((x, a, b) => new { UserId = a.Id })
            .Select((x, a, b) => new
            {
                UserId1 = x.Grouping.UserId,
                UserName = x.Grouping.Name,
                CreatedDate1 = x.Grouping.CreatedDate,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.True(result.Count >= 2);
        Assert.NotNull(result[0].UserName);
        Assert.NotNull(result[1].UserName);
    }
    [Fact]
    public void FromQuery_Groupby_OrderBy()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
           .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
           .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
           .OrderBy((x, a, b) => x.Grouping)
           .Select((x, a, b) => new
           {
               x.Grouping,
               OrderCount = x.Count(b.Id),
               TotalAmount = x.Sum(b.TotalAmount)
           })
           .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)", sql);
        var result = repository.From<User>()
          .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
          .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
          .OrderBy((x, a, b) => x.Grouping)
          .Select((x, a, b) => new
          {
              x.Grouping,
              OrderCount = x.Count(b.Id),
              TotalAmount = x.Sum(b.TotalAmount)
          })
          .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        var sql1 = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => x.Grouping)
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                CreatedAt = x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `CreatedAt`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)", sql1);
        var result1 = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .OrderBy((x, a, b) => x.Grouping)
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                CreatedAt = x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result1);
        Assert.True(result1.Count > 0);
    }
    [Fact]
    public async Task FromQuery_Groupby_OrderBy_Fields()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
           .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
           .GroupBy((a, b) => new { UserId = a.Id, a.Name, CreatedDate = b.CreatedAt.Date })
           .OrderBy((x, a, b) => x.Grouping.UserId)
           .OrderByDescending((x, a, b) => x.Grouping.Name)
           .OrderBy((x, a, b) => x.Grouping.CreatedDate)
           .Select((x, a, b) => new
           {
               x.Grouping,
               OrderCount = x.Count(b.Id),
               TotalAmount = x.Sum(b.TotalAmount)
           })
           .ToSql(out _);
        Assert.Equal("SELECT a.`Id` AS `UserId`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `CreatedDate`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name` DESC,CONVERT(b.`CreatedAt`,DATE)", sql);

        await repository.From<User>()
           .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
           .GroupBy((a, b) => new { UserId = a.Id, a.Name, CreatedDate = b.CreatedAt.Date })
           .OrderBy((x, a, b) => x.Grouping.UserId)
           .OrderByDescending((x, a, b) => x.Grouping.Name)
           .OrderBy((x, a, b) => x.Grouping.CreatedDate)
           .Select((x, a, b) => new
           {
               x.Grouping,
               OrderCount = x.Count(b.Id),
               TotalAmount = x.Sum(b.TotalAmount)
           })
           .FirstAsync();
    }
    [Fact]
    public async Task FromQuery_Groupby_Having()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From(f => f.From<Order, OrderDetail>()
                .Where((x, y) => x.Id == y.OrderId)
                .GroupBy((x, y) => new { x.BuyerId, x.CreatedAt.Date })
                .Select((x, a, b) => new
                {
                    a.BuyerId,
                    a.CreatedAt.Date,
                    OrderCount = x.Count(a.Id),
                    ProductCount = x.CountDistinct(b.ProductId),
                    TotalAmount = x.Sum(a.TotalAmount)
                }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Where((a, b) => a.ProductCount > 2 && a.TotalAmount > 300)
            .OrderBy((a, b) => b.Id)
            .Select((a, b) => new
            {
                a.BuyerId,
                BuyerName = b.Name,
                BuyDate = a.Date,
                a.ProductCount,
                a.OrderCount,
                a.TotalAmount
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`BuyerId`,b.`Name` AS `BuyerName`,a.`Date` AS `BuyDate`,a.`ProductCount`,a.`OrderCount`,a.`TotalAmount` FROM (SELECT a.`BuyerId`,CONVERT(a.`CreatedAt`,DATE) AS `Date`,COUNT(a.`Id`) AS `OrderCount`,COUNT(DISTINCT b.`ProductId`) AS `ProductCount`,SUM(a.`TotalAmount`) AS `TotalAmount` FROM `sys_order` a,`sys_order_detail` b WHERE a.`Id`=b.`OrderId` GROUP BY a.`BuyerId`,CONVERT(a.`CreatedAt`,DATE)) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>2 AND a.`TotalAmount`>300 ORDER BY b.`Id`", sql);

        var result = await repository.From(f => f
            .From<Order, OrderDetail>()
                .Where((x, y) => x.Id == y.OrderId)
                .GroupBy((x, y) => new { x.BuyerId, x.CreatedAt.Date })
                .Select((x, a, b) => new
                {
                    a.BuyerId,
                    a.CreatedAt.Date,
                    OrderCount = x.Count(a.Id),
                    ProductCount = x.CountDistinct(b.ProductId),
                    TotalAmount = x.Sum(a.TotalAmount)
                }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Where((a, b) => a.ProductCount > 2 && a.TotalAmount > 300)
            .OrderBy((a, b) => b.Id)
            .Select((a, b) => new
            {
                a.BuyerId,
                BuyerName = b.Name,
                BuyDate = a.Date,
                a.ProductCount,
                a.OrderCount,
                a.TotalAmount
            })
            .ToListAsync();
        Assert.True(result.Count > 0);

        var sql1 = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .InnerJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
            .GroupBy((a, b, c) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b, c) => x.Sum(b.TotalAmount) > 300 && x.CountDistinct(c.ProductId) > 2)
            .OrderBy((x, a, b, c) => new { x.Grouping })
            .Select((x, a, b, c) => new
            {
                BuyerId = x.Grouping.Id,
                BuyerName = x.Grouping.Name,
                BuyDate = x.Grouping.Date,
                ProductCount = x.CountDistinct(c.ProductId),
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` AS `BuyerId`,a.`Name` AS `BuyerName`,CONVERT(b.`CreatedAt`,DATE) AS `BuyDate`,COUNT(DISTINCT c.`ProductId`) AS `ProductCount`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 AND COUNT(DISTINCT c.`ProductId`)>2 ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)", sql1);
        var result1 = await repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .InnerJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
            .GroupBy((a, b, c) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b, c) => x.Sum(b.TotalAmount) > 300 && x.CountDistinct(c.ProductId) > 2)
            .OrderBy((x, a, b, c) => new { x.Grouping })
            .Select((x, a, b, c) => new
            {
                BuyerId = x.Grouping.Id,
                BuyerName = x.Grouping.Name,
                BuyDate = x.Grouping.Date,
                ProductCount = x.CountDistinct(c.ProductId),
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToListAsync();
        Assert.True(result1.Count > 0);
    }
    [Fact]
    public void FromQuery_Groupby_Having_OrderBy()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => x.Grouping)
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND f.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)", sql);
        var result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => x.Grouping)
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_Groupby_Having_OrderBy_Fields()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => x.Grouping.Id)
            .OrderByDescending((x, a, b) => x.Grouping.Name)
            .OrderBy((x, a, b) => x.Grouping.Date)
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND f.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`,a.`Name` DESC,CONVERT(b.`CreatedAt`,DATE)", sql);
        var result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => x.Grouping.Id)
            .OrderByDescending((x, a, b) => x.Grouping.Name)
            .OrderBy((x, a, b) => x.Grouping.Date)
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void Where_Exists()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => repository.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_company` t WHERE t.`Name` LIKE '%谷歌%' AND a.`CompanyId`=t.`Id`)", sql);
        var result = repository.From<User>()
            .Where(f => repository.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => repository.From<Company>('b').Exists(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_company` b WHERE b.`Name` LIKE '%谷歌%' AND a.`CompanyId`=b.`Id`)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Company>('b').Exists(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => repository.From<Order>('b')
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .Exists((x, y) => x.BuyerId == f.Id && y.Price > 200))
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_order` b INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` WHERE b.`BuyerId`=a.`Id` AND c.`Price`>200)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Order>('b')
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .Exists((x, y) => x.BuyerId == f.Id && y.Price > 200))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
               .InnerJoin((x, y) => x.Id == y.OrderId)
               .Exists((x, y) => x.BuyerId == f.Id && y.Price > 200))
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_order` b INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` WHERE b.`BuyerId`=a.`Id` AND c.`Price`>200)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
               .InnerJoin((x, y) => x.Id == y.OrderId)
               .Exists((x, y) => x.BuyerId == f.Id && y.Price > 200))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
               .Exists((x, y) => x.Id == y.OrderId && x.BuyerId == f.Id && y.Price > 200))
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND b.`BuyerId`=a.`Id` AND c.`Price`>200)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
               .Exists((x, y) => x.Id == y.OrderId && x.BuyerId == f.Id && y.Price > 200))
           .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
                .Where((x, y) => x.Id == y.OrderId)
                .Exists((x, y) => x.BuyerId == f.Id && y.Price > 200))
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND b.`BuyerId`=a.`Id` AND c.`Price`>200)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
                .Where((x, y) => x.Id == y.OrderId)
                .Exists((x, y) => x.BuyerId == f.Id && y.Price > 200))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => Sql.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_company` t WHERE t.`Name` LIKE '%谷歌%' AND a.`CompanyId`=t.`Id`)", sql);
        result = repository.From<User>()
            .Where(f => Sql.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        var sql1 = repository.From<User>()
            .Where(f => Sql.Exists(t => t
                .From<Order, OrderDetail>('b')
                .Where((x, y) => x.Id == y.OrderId && f.Id == x.BuyerId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 1)
                .Select()))
            .GroupBy(f => new { f.Gender, f.CompanyId })
            .Select((t, a) => new { t.Grouping, UserTotal = t.CountDistinct(a.Id) })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Gender`,a.`CompanyId`,COUNT(DISTINCT a.`Id`) AS `UserTotal` FROM `sys_user` a WHERE EXISTS(SELECT b.`Id` FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND a.`Id`=b.`BuyerId` GROUP BY b.`Id` HAVING COUNT(DISTINCT c.`ProductId`)>1) GROUP BY a.`Gender`,a.`CompanyId`", sql1);
        var result1 = repository.From<User>()
            .Where(f => Sql.Exists(t => t
                .From<Order, OrderDetail>('b')
                .Where((x, y) => x.Id == y.OrderId && f.Id == x.BuyerId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 1)
                .Select()))
            .GroupBy(f => new { f.Gender, f.CompanyId })
            .Select((t, a) => new { t.Grouping, UserTotal = t.CountDistinct(a.Id) })
            .ToList();
        Assert.NotNull(result1);
        Assert.True(result1.Count > 0);
    }
    [Fact]
    public async Task FromQuery_Exists()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.Exists(f => f
                .From<Order, OrderDetail>('c')
                .Where((a, b) => a.BuyerId == x.Id && a.Id == b.OrderId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                .Select()))
            .GroupBy((x, y) => new { x.Gender, x.CompanyId })
            .Select((x, a, b) => new { x.Grouping, UserTotal = x.CountDistinct(a.Id) })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Gender`,a.`CompanyId`,COUNT(DISTINCT a.`Id`) AS `UserTotal` FROM `sys_user` a INNER JOIN `sys_company` b ON a.`CompanyId`=b.`Id` WHERE EXISTS(SELECT c.`Id` FROM `sys_order` c,`sys_order_detail` d WHERE c.`BuyerId`=a.`Id` AND c.`Id`=d.`OrderId` GROUP BY c.`Id` HAVING COUNT(DISTINCT d.`ProductId`)>0) GROUP BY a.`Gender`,a.`CompanyId`", sql);
        var result = await repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.Exists(f => f
                .From<Order, OrderDetail>('c')
                .Where((a, b) => a.BuyerId == x.Id && a.Id == b.OrderId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                .Select()))
            .GroupBy((x, y) => new { x.Gender, x.CompanyId })
            .Select((x, a, b) => new { x.Grouping, UserTotal = x.CountDistinct(a.Id) })
            .ToListAsync();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void CteTable_Exists()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var myOrders = repository.From<OrderDetail, Order>()
            .Where((a, b) => a.OrderId == b.Id)
            .GroupBy((a, b) => new { a.OrderId, b.BuyerId })
            .Having((x, a, b) => x.CountDistinct(a.ProductId) > 1)
            .Select((x, a, b) => x.Grouping)
            .AsCteTable("myOrders");

        var sql = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.Exists(myOrders, f => f.BuyerId == x.Id))
            .Select((a, b) => new { a.Id, a.Name, CompanyName = b.Name })
            .ToSql(out _);
        Assert.Equal(@"WITH `myOrders`(`OrderId`,`BuyerId`) AS 
(
SELECT a.`OrderId`,b.`BuyerId` FROM `sys_order_detail` a,`sys_order` b WHERE a.`OrderId`=b.`Id` GROUP BY a.`OrderId`,b.`BuyerId` HAVING COUNT(DISTINCT a.`ProductId`)>1
)
SELECT a.`Id`,a.`Name`,b.`Name` AS `CompanyName` FROM `sys_user` a INNER JOIN `sys_company` b ON a.`CompanyId`=b.`Id` WHERE EXISTS(SELECT * FROM `myOrders` f WHERE f.`BuyerId`=a.`Id`)", sql);

        var result = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.Exists(myOrders, f => f.BuyerId == x.Id))
            .Select((a, b) => new { a.Id, a.Name, CompanyName = b.Name })
            .First();
        Assert.NotNull(result);
    }
    [Fact]
    public void FromQuery_In_Exists()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((x, y) => Sql.In(x.Id, new int[] { 1, 2, 3 }) && Sql.Exists<OrderDetail>(f => y.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => x.Grouping.Id)
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE a.`Id` IN (1,2,3) AND EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND f.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`", sql);
        var result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((x, y) => Sql.In(x.Id, new int[] { 1, 2, 3 }) && Sql.Exists<OrderDetail>(f => y.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => x.Grouping.Id)
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => Sql.In(a.Id, new int[] { 1, 2, 3 }) && Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => new { UserId = a.Id, b.CreatedAt.Date })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE a.`Id` IN (1,2,3) AND EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND f.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`,CONVERT(b.`CreatedAt`,DATE)", sql);
        result = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .Where((a, b) => Sql.In(a.Id, new int[] { 1, 2, 3 }) && Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
            .OrderBy((x, a, b) => new { UserId = a.Id, b.CreatedAt.Date })
            .Select((x, a, b) => new
            {
                x.Grouping,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_In1()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order>('b')
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` AND c.`ProductId`=1)", sql);
        var result = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order>('b')
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order, OrderDetail>('b')
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND c.`ProductId`=1)", sql);
        result = repository.From<User>()
           .Where(f => Sql.In(f.Id, t => t.From<Order, OrderDetail>('b')
               .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
               .Select((x, y) => x.BuyerId)))
           .Select(f => f.Id)
           .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        var subQuery = repository.From<Order>('b')
              .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
              .Select((x, y) => x.BuyerId);
        sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, subQuery))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` AND c.`ProductId`=1)", sql);
        result = repository.From<User>()
            .Where(f => Sql.In(f.Id, subQuery))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        subQuery = repository.From<Order, OrderDetail>('b')
            .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
            .Select((x, y) => x.BuyerId);
        sql = repository.From<User>()
           .Where(f => Sql.In(f.Id, subQuery))
           .Select(f => f.Id)
           .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND c.`ProductId`=1)", sql);
        result = repository.From<User>()
           .Where(f => Sql.In(f.Id, subQuery))
           .Select(f => f.Id)
           .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_In_Exists1()
    {
        var repository = this.dbFactory.Create();
        bool? isMale = true;
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order>('b')
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` AND c.`ProductId`=1) AND EXISTS(SELECT * FROM `sys_order` x,`sys_company` y WHERE a.`Id`=x.`SellerId` AND a.`CompanyId`=y.`Id`)", sql);
        var result = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order>('b')
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order, OrderDetail>('b')
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND c.`ProductId`=1) AND EXISTS(SELECT * FROM `sys_order` x,`sys_company` y WHERE a.`Id`=x.`SellerId` AND a.`CompanyId`=y.`Id`)", sql);
        result = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<Order, OrderDetail>('b')
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_In_Exists_Group_CountDistinct_Count()
    {
        var repository = this.dbFactory.Create();
        bool? isMale = true;
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<OrderDetail>('b')
                .InnerJoin<Order>((a, b) => a.OrderId == b.Id && a.ProductId == 1)
                .Select((x, y) => y.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Company, Order>((x, y) => f.Id == y.SellerId && f.CompanyId == x.Id))
            .GroupBy(f => new { f.Gender, f.Age })
            .Select((t, a) => new { t.Grouping, CompanyCount = t.CountDistinct(a.CompanyId), UserCount = t.Count(a.Id) })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Gender`,a.`Age`,COUNT(DISTINCT a.`CompanyId`) AS `CompanyCount`,COUNT(a.`Id`) AS `UserCount` FROM `sys_user` a WHERE a.`Id` IN (SELECT c.`BuyerId` FROM `sys_order_detail` b INNER JOIN `sys_order` c ON b.`OrderId`=c.`Id` AND b.`ProductId`=1) AND EXISTS(SELECT * FROM `sys_company` x,`sys_order` y WHERE a.`Id`=y.`SellerId` AND a.`CompanyId`=x.`Id`) GROUP BY a.`Gender`,a.`Age`", sql);

        var result = repository.From<User>()
            .Where(f => Sql.In(f.Id, t => t.From<OrderDetail>('b')
                .InnerJoin<Order>((a, b) => a.OrderId == b.Id && a.ProductId == 1)
                .Select((x, y) => y.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Company, Order>((x, y) => f.Id == y.SellerId && f.CompanyId == x.Id))
            .GroupBy(f => new { f.Gender, f.Age })
            .Select((t, a) => new { t.Grouping, CompanyCount = t.CountDistinct(a.CompanyId), UserCount = t.Count(a.Id) })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public void FromQuery_Aggregate()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .SelectAggregate((x, a) => new
            {
                OrderCount = x.Count(a.Id),
                TotalAmount = x.Sum(a.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT COUNT(a.`Id`) AS `OrderCount`,SUM(a.`TotalAmount`) AS `TotalAmount` FROM `sys_order` a", sql);
        var result = repository.From<Order>()
            .SelectAggregate((x, a) => new
            {
                OrderCount = x.Count(a.Id),
                TotalAmount = x.Sum(a.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        sql = repository.From<Order>()
            .Select(a => new
            {
                OrderCount = Sql.Count(a.Id),
                TotalAmount = Sql.Sum(a.TotalAmount)
            })
        .ToSql(out _);
        Assert.Equal("SELECT COUNT(a.`Id`) AS `OrderCount`,SUM(a.`TotalAmount`) AS `TotalAmount` FROM `sys_order` a", sql);
        result = repository.From<Order>()
            .Select(a => new
            {
                OrderCount = Sql.Count(a.Id),
                TotalAmount = Sql.Sum(a.TotalAmount)
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        var sql1 = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .OrderBy((a, b) => new { UserId = a.Id, OrderId = b.Id })
            .Select((a, b) => new
            {
                UserId = a.Id,
                OrderId = b.Id,
                OrderCount = Sql.Count(b.Id),
                TotalAmount = Sql.Sum(b.TotalAmount)
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` AS `UserId`,b.`Id` AS `OrderId`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` ORDER BY a.`Id`,b.`Id`", sql1);
    }
    [Fact]
    public void Query_Count()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var count = repository.From<User>().Count();
        var count1 = repository.From<User>().Select(f => Sql.Count()).First();
        var count2 = repository.QueryFirst<int>("SELECT COUNT(1) FROM sys_user");
        Assert.True(count == count1);
        Assert.True(count == count2);
    }
    [Fact]
    public void Query_Where_Count()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var result = repository.From<User>()
            .Where(t => Sql.Exists(f =>
                f.From<Order, OrderDetail>('o')
                    .Where((a, b) => a.BuyerId == t.Id && a.Id == b.OrderId)
                    .GroupBy((a, b) => a.Id)
                    .Having((x, a, b) => Sql.Count(b.Id) > 0)
                    .Select()))
            .GroupBy(f => new { f.Gender, f.CompanyId })
            .Select((x, y) => new { x.Grouping, UserTotal = x.CountDistinct(y.Id) })
            .ToList();
        if (result.Count > 0)
        {
            Assert.NotNull(result[0]);
            Assert.NotNull(result[0].Grouping);
            Assert.True(result[0].UserTotal > 0);
        }
    }
    [Fact]
    public void Query_Max()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var count = repository.From<Order>().Max(f => f.TotalAmount);
        var count1 = repository.From<Order>().Select(f => Sql.Max(f.TotalAmount)).First();
        var count2 = repository.QueryFirst<double>("SELECT MAX(TotalAmount) FROM sys_order");
        Assert.True(count == count1);
        Assert.True(count == count2);
    }
    [Fact]
    public void Query_Min()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var count = repository.From<Order>().Min(f => f.TotalAmount);
        var count1 = repository.From<Order>().Select(f => Sql.Min(f.TotalAmount)).First();
        var count2 = repository.QueryFirst<double>("SELECT MIN(TotalAmount) FROM sys_order");
        Assert.True(count == count1);
        Assert.True(count == count2);
    }
    [Fact]
    public void Query_Avg()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var value1 = repository.From<Order>().Avg(f => f.TotalAmount);
        var value2 = repository.From<Order>().Select(f => Sql.Avg(f.TotalAmount)).First();
        var value3 = repository.QueryFirst<double>("SELECT AVG(TotalAmount) FROM sys_order");
        Assert.True(value1 == value2);
        Assert.True(value1 == value3);
    }
    [Fact]
    public void Query_ValueTuple()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var sql = "SELECT Id,OrderNo,TotalAmount FROM sys_order";
        var result = repository.Query<(string OrderId, string OrderNo, double TotalAmount)>(sql);
        Assert.NotNull(result);
    }
    [Fact]
    public void Query_Json()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var result = repository.Get<Order>("1");
        Assert.NotNull(result);
        Assert.NotNull(result.Products);
        Assert.NotNull(result.Disputes);
    }
    [Fact]
    public void Query_SelectNull_WhereNull()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .Where(x => x.ProductCount == null)
            .And(true, f => !f.ProductCount.HasValue)
            .Select(x => new
            {
                NoOrderNo = x.OrderNo == null,
                HasProduct = x.ProductCount.HasValue
            })
            .ToSql(out _);
        Assert.Equal("SELECT (CASE WHEN a.`OrderNo` IS NULL THEN 1 ELSE 0 END) AS `NoOrderNo`,(CASE WHEN a.`ProductCount` IS NOT NULL THEN 1 ELSE 0 END) AS `HasProduct` FROM `sys_order` a WHERE a.`ProductCount` IS NULL AND a.`ProductCount` IS NULL", sql);
        var result = repository.From<Order>()
            .Where(x => x.ProductCount == null)
            .And(true, f => !f.ProductCount.HasValue)
            .Select(x => new
            {
                NoOrderNo = x.OrderNo == null,
                HasProduct = x.ProductCount.HasValue
            })
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public async Task Query_Where_IsNull()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .Where(x => x.ProductCount == null || x.BuyerId.IsNull())
            .And(true, f => !f.ProductCount.HasValue)
            .Select(x => x.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_order` a WHERE (a.`ProductCount` IS NULL OR a.`BuyerId` IS NULL) AND a.`ProductCount` IS NULL", sql);
        var result = repository.From<Order>()
            .Where(x => x.ProductCount == null || x.BuyerId.IsNull())
            .And(true, f => !f.ProductCount.HasValue)
            .Select(x => x.Id)
            .ToList();
        Assert.NotNull(result);

        var sql1 = repository.From<Order>()
           .Where(x => x.ProductCount.IsNull(0) > 0 || x.BuyerId.IsNull(0) >= 0)
           .Select(f => new
           {
               f.Id,
               f.OrderNo,
               ProductCount = f.ProductCount.IsNull(0),
               BuyerId = f.BuyerId.IsNull(0),
               TotalAmount = f.TotalAmount.IsNull(0)
           })
           .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`OrderNo`,IFNULL(a.`ProductCount`,0) AS `ProductCount`,IFNULL(a.`BuyerId`,0) AS `BuyerId`,IFNULL(a.`TotalAmount`,0) AS `TotalAmount` FROM `sys_order` a WHERE IFNULL(a.`ProductCount`,0)>0 OR IFNULL(a.`BuyerId`,0)>=0", sql1);

        await repository.BeginTransactionAsync();
        await repository.UpdateAsync<Order>(new { Id = "1", BuyerId = DBNull.Value });
        await repository.UpdateAsync<Order>(new { Id = "2", ProductCount = DBNull.Value });
        await repository.UpdateAsync<Order>(new { Id = "3", TotalAmount = DBNull.Value });
        var result1 = repository.From<Order>()
            .Where(x => x.ProductCount.IsNull(0) > 0 || x.BuyerId.IsNull(0) >= 0)
            .Select(f => new
            {
                f.Id,
                f.OrderNo,
                ProductCount = f.ProductCount.IsNull(0),
                BuyerId = f.BuyerId.IsNull(0),
                f.TotalAmount
            })
            .ToList();
        await repository.CommitAsync();
        var myOrders = result1.FindAll(f => "1,2,3".Contains(f.Id)).OrderBy(f => f.Id).ToList();
        Assert.True(result1.Count >= 3);
        Assert.Equal(0, myOrders[0].BuyerId);
        Assert.Equal(0, myOrders[1].ProductCount);
        Assert.Equal(0, myOrders[2].TotalAmount);
    }
    [Fact]
    public async Task Query_Union()
    {
        var id1 = "1";
        var id2 = "2";
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .Where(x => x.Id == id1)
            .Select(x => new
            {
                x.Id,
                x.OrderNo,
                x.SellerId,
                x.BuyerId
            })
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                }))
            .ToSql(out _);
        Assert.Equal(@"SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`=@p0 UNION ALL
SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`<>@p1", sql);

        var result = await repository.From<Order>()
           .Where(x => x.Id == id1)
           .Select(x => new
           {
               x.Id,
               x.OrderNo,
               x.SellerId,
               x.BuyerId
           })
           .UnionAll(f => f.From<Order>()
               .Where(x => x.Id != id2)
               .Select(x => new
               {
                   x.Id,
                   x.OrderNo,
                   x.SellerId,
                   x.BuyerId
               }))
           .ToListAsync();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public async Task Query_Union_Take()
    {
        this.Initialize();
        string id1 = "3", id2 = "2";
        var repository = this.dbFactory.Create();
        var sql = repository
            .From<Order>('b')
                .Where(x => x.Id == id1)
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
               .Take(1)
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                }))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .Select((x, y) => new { x.Id, x.OrderNo, x.SellerId, x.BuyerId, BuyerName = y.Name })
            .ToSql(out var dbParameters);
        Assert.Equal(@"SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId`,b.`Name` AS `BuyerName` FROM (SELECT * FROM (SELECT b.`Id`,b.`OrderNo`,b.`SellerId`,b.`BuyerId` FROM `sys_order` b WHERE b.`Id`=@p0 ORDER BY b.`Id` LIMIT 1) a UNION ALL
SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`<>@p1) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id`", sql);
        Assert.Equal("@p0", dbParameters[0].ParameterName);
        Assert.Equal("@p1", dbParameters[1].ParameterName);
        var result = await repository
            .From<Order>('b')
                .Where(x => x.Id == id1)
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
               .Take(1)
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                }))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .Select((x, y) => new { x.Id, x.OrderNo, x.SellerId, x.BuyerId, BuyerName = y.Name })
            .ToListAsync();
        Assert.True(result.Count > 0);

        var sql1 = repository
            .From<User>()
            .WithTable(t => t
                .From<Order>()
                    .InnerJoin<User>((a, b) => a.SellerId == b.Id)
                    .Where((x, y) => x.Id == id1)
                    .OrderBy((a, b) => a.Id)
                    .Select((x, y) => new
                    {
                        x.Id,
                        x.OrderNo,
                        x.SellerId,
                        x.BuyerId
                    })
                    .Take(1)
                .UnionAll(f => f.From<Order>()
                    .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
                    .Where((x, y) => x.Id != id2)
                    .Select((x, y) => new
                    {
                        x.Id,
                        x.OrderNo,
                        x.SellerId,
                        x.BuyerId
                    })))
          .InnerJoin<User>((a, b, c) => a.Id == b.SellerId)
          .InnerJoin((a, b, c) => b.BuyerId == c.Id)
          .Select((x, y, z) => new { y.Id, y.OrderNo, y.SellerId, SellerName = x.Name, y.BuyerId, BuyerName = z.Name })
          .ToSql(out var dbParameters1);
        Assert.Equal(@"SELECT b.`Id`,b.`OrderNo`,b.`SellerId`,a.`Name` AS `SellerName`,b.`BuyerId`,c.`Name` AS `BuyerName` FROM `sys_user` a INNER JOIN (SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`SellerId`=b.`Id` WHERE a.`Id`=@p0 ORDER BY a.`Id` LIMIT 1) a UNION ALL
SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`Id`<>@p1) b ON a.`Id`=b.`SellerId` INNER JOIN `sys_user` c ON b.`BuyerId`=c.`Id`", sql1);
        Assert.Equal("@p0", dbParameters1[0].ParameterName);
        Assert.Equal("@p1", dbParameters1[1].ParameterName);

        var result1 = repository
            .From<User>()
            .WithTable(t => t
                .From<Order>()
                    .InnerJoin<User>((a, b) => a.SellerId == b.Id)
                    .Where((x, y) => x.Id == id1)
                    .OrderBy((a, b) => a.Id)
                    .Select((x, y) => new
                    {
                        x.Id,
                        x.OrderNo,
                        x.SellerId,
                        x.BuyerId
                    })
                    .Take(1)
                .UnionAll(f => f.From<Order>()
                    .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
                    .Where((x, y) => x.Id != id2)
                    .Select((x, y) => new
                    {
                        x.Id,
                        x.OrderNo,
                        x.SellerId,
                        x.BuyerId
                    })))
            .InnerJoin<User>((a, b, c) => a.Id == b.SellerId)
            .InnerJoin((a, b, c) => b.BuyerId == c.Id)
            .Select((x, y, z) => new { y.Id, y.OrderNo, y.SellerId, SellerName = x.Name, y.BuyerId, BuyerName = z.Name })
            .ToList();
        Assert.True(result1.Count > 0);
    }
    [Fact]
    public async Task FromQuery_Union_Limit()
    {
        this.Initialize();
        string id1 = "4", id2 = "2";
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
                .Where(x => x.Id == id1)
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1)
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1))
            .ToSql(out var dbParameters);
        Assert.Equal(@"SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`=@p0 ORDER BY a.`Id` LIMIT 1) a UNION ALL
SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`<>@p1 LIMIT 1) a", sql);

        Assert.Equal(2, dbParameters.Count);
        Assert.True((string)dbParameters[0].Value == id1);
        Assert.True((string)dbParameters[1].Value == id2);

        var result = await repository.From<Order>()
                .Where(x => x.Id == id1)
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1)
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                }).Take(1))
            .ToListAsync();
        Assert.True(result.Count > 0);
        foreach (var item in result)
        {
            Assert.True(item.Id == id1 || item.Id != id2);
        }
    }
    [Fact]
    public void FromQuery_Union_SubQuery_Limit()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.From(f => f.From<Order>()
                .Where(x => x.Id != "3")
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
               .Take(1))
            .Select()
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id == "3")
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1))
            .ToSql(out _);
        Assert.Equal(@"SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`<>'3' ORDER BY a.`Id` LIMIT 1) a UNION ALL
SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`='3' LIMIT 1) a", sql);
        var result = repository.From(f => f.From<Order>()
                .Where(x => x.Id != "3")
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
               .Take(1))
            .Select()
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id == "3")
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public async Task FromQuery_Union_SubQuery_OrderBy()
    {
        this.Initialize();
        string id1 = "4", id2 = "2";
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
                .Where(x => x.Id == id1)
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
               .Take(1)
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .OrderByDescending(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1))
            .ToSql(out var dbParameters);
        Assert.Equal(@"SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`=@p0 ORDER BY a.`Id` LIMIT 1) a UNION ALL
SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`<>@p1 ORDER BY a.`Id` DESC LIMIT 1) a", sql);
        Assert.Equal(2, dbParameters.Count);
        Assert.True((string)dbParameters[0].Value == id1);
        Assert.True((string)dbParameters[1].Value == id2);

        var result = await repository.From<Order>()
                .Where(x => x.Id == id1)
                .OrderBy(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
               .Take(1)
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id != id2)
                .OrderByDescending(f => f.Id)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNo,
                    x.SellerId,
                    x.BuyerId
                })
                .Take(1))
           .ToListAsync();
        Assert.True(result.Count > 0);
        foreach (var item in result)
        {
            Assert.True(item.Id == id1 || item.Id != id2);
        }
    }
    [Fact]
    public void Union_Take()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository
            .From(f => f.From<Menu>()
                .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId }))
            .InnerJoin<Page>((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id == "2")
                .OrderByDescending(f => f.Id)
                .Select(x => new
                {
                    Id = x.BuyerId,
                    Name = x.OrderNo,
                    ParentId = x.SellerId,
                    Url = x.BuyerId.ToString()
                })
                .Take(1))
            .ToSql(out _);
        Assert.Equal(@"SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM (SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a) a INNER JOIN `sys_page` b ON a.`Id`=b.`Id` UNION ALL
SELECT * FROM (SELECT a.`BuyerId`,a.`OrderNo`,a.`SellerId`,CAST(a.`BuyerId` AS CHAR) FROM `sys_order` a WHERE a.`Id`='2' ORDER BY a.`Id` DESC LIMIT 1) a", sql);

        var result = repository
            .From(f => f.From<Menu>()
                .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId }))
            .InnerJoin<Page>((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .UnionAll(f => f.From<Order>()
                .Where(x => x.Id == "2")
                .OrderByDescending(f => f.Id)
                .Select(x => new
                {
                    Id = x.BuyerId,
                    Name = x.OrderNo,
                    ParentId = x.SellerId,
                    Url = x.BuyerId.ToString()
                })
                .Take(1))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public async Task Query_WithCte_SelfRef()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        int menuId = 2;
        int pageId = 1;
        var sql = repository
            .From(f => f.From<Menu>()
                .Where(t => t.Id >= menuId)
                .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId })
                .AsCteTable("MenuList"))
            .InnerJoin<Page>((a, b) => a.Id == b.Id)
            .Where((x, y) => y.Id >= pageId)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out var dbParameters);

        Assert.Equal(@"WITH `MenuList`(`Id`,`Name`,`ParentId`,`PageId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a WHERE a.`Id`>=@p0
)
SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `MenuList` a INNER JOIN `sys_page` b ON a.`Id`=b.`Id` WHERE b.`Id`>=@p1", sql);
        Assert.Equal(2, dbParameters.Count);
        Assert.True((int)dbParameters[0].Value == menuId);
        Assert.True((int)dbParameters[1].Value == pageId);

        var result = await repository
            .From(f => f.From<Menu>()
                .Where(t => t.Id >= menuId)
                .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId })
                .AsCteTable("MenuList"))
            .InnerJoin<Page>((a, b) => a.Id == b.Id)
            .Where((x, y) => y.Id >= pageId)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToListAsync();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    [Fact]
    public async Task Query_WithNextCte()
    {
        this.Initialize();
        int rootId = 1;
        var repository = this.dbFactory.Create();
        var myCteTable1 = repository
            .From<Menu>()
                .Where(x => x.Id == rootId)
                .Select(x => new { x.Id, x.Name, x.ParentId })
            .UnionAllRecursive((x, self) => x.From<Menu>()
                .InnerJoin(self, (a, b) => a.ParentId == b.Id)
                .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
            .AsCteTable("myCteTable1");
        var myCteTable2 = repository
            .From<Page, Menu>()
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { y.Id, y.ParentId, x.Url })
            .UnionAll(x => x.From<Menu>()
                .InnerJoin<Page>((a, b) => a.PageId == b.Id)
                .Select((x, y) => new { x.Id, x.ParentId, y.Url }))
            .AsCteTable("myCteTable2");

        var sql = repository
            .From(myCteTable1)
            .InnerJoin(myCteTable2, (a, b) => a.Id == b.Id)
            .Select((a, b) => new { b.Id, a.Name, b.ParentId, b.Url })
            .ToSql(out _);
        Assert.Equal(@"WITH RECURSIVE `myCteTable1`(`Id`,`Name`,`ParentId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a WHERE a.`Id`=@p0 UNION ALL
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a INNER JOIN `myCteTable1` b ON a.`ParentId`=b.`Id`
),
`myCteTable2`(`Id`,`ParentId`,`Url`) AS 
(
SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE a.`Id`=b.`PageId` UNION ALL
SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN `sys_page` b ON a.`PageId`=b.`Id`
)
SELECT b.`Id`,a.`Name`,b.`ParentId`,b.`Url` FROM `myCteTable1` a INNER JOIN `myCteTable2` b ON a.`Id`=b.`Id`", sql);

        var menuList = repository
            .From<Menu>()
                .Where(x => x.Id == rootId.ToParameter("@RootId"))
                .Select(x => new { x.Id, x.Name, x.ParentId })
            .UnionAllRecursive((x, y) => x.From<Menu>()
                .InnerJoin(y, (a, b) => a.ParentId == b.Id)
                .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
            .AsCteTable("MenuList");

        var result1 = repository
            .From(myCteTable2)
            .InnerJoin(myCteTable1, (a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, b.Name, a.ParentId, a.Url })
            .ToList();
        Assert.NotNull(result1);
        Assert.True(result1.Count > 0);

        int pageId = 1;
        sql = repository
            .From(menuList)
            .WithTable(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()//.WithTable(self)
                    .InnerJoin(menuList, (a, b) => a.Id == b.Id)
                    .Where((a, b) => a.Id > pageId)
                    .Select((x, y) => new { y.Id, x.Url })))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out _);
        Assert.Equal(@"WITH RECURSIVE `MenuList`(`Id`,`Name`,`ParentId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a WHERE a.`Id`=@RootId UNION ALL
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a INNER JOIN `MenuList` b ON a.`ParentId`=b.`Id`
)
SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `MenuList` a INNER JOIN (SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `sys_menu` b ON a.`Id`=b.`PageId` WHERE a.`Id`=@p1 UNION ALL
SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `MenuList` b ON a.`Id`=b.`Id` WHERE a.`Id`>@p2) b ON a.`Id`=b.`Id`", sql);
        var result2 = repository
            .From(menuList)
            .WithTable(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()//.WithTable(self)
                    .InnerJoin(menuList, (a, b) => a.Id == b.Id)
                    .Where((a, b) => a.Id > pageId)
                    .Select((x, y) => new { y.Id, x.Url })))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToList();
        Assert.NotNull(result2);
        Assert.True(result2.Count > 0);

        sql = repository
            .From(menuList)
            .WithTable(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()//.WithTable(self)
                    .InnerJoin(menuList, (a, b) => a.Id == b.Id)
                    .Where((a, b) => a.Id > pageId)
                    .Select((x, y) => new { y.Id, x.Url }))
                .AsCteTable("MenuPageList"))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out _);
        Assert.Equal(@"WITH RECURSIVE `MenuList`(`Id`,`Name`,`ParentId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a WHERE a.`Id`=@RootId UNION ALL
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a INNER JOIN `MenuList` b ON a.`ParentId`=b.`Id`
),
`MenuPageList`(`Id`,`Url`) AS 
(
SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `sys_menu` b ON a.`Id`=b.`PageId` WHERE a.`Id`=@p1 UNION ALL
SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `MenuList` b ON a.`Id`=b.`Id` WHERE a.`Id`>@p2
)
SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `MenuList` a INNER JOIN `MenuPageList` b ON a.`Id`=b.`Id`", sql);

        var result3 = await repository
            .From(menuList)
            .WithTable(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()//.WithTable(self)
                    .InnerJoin(menuList, (a, b) => a.Id == b.Id)
                    .Where((a, b) => a.Id > pageId)
                    .Select((x, y) => new { y.Id, x.Url }))
                .AsCteTable("MenuPageList"))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToListAsync();
        Assert.NotNull(result3);
        Assert.True(result3.Count > 0);
    }
    [Fact]
    public async Task Query_WithTable()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Menu>()
            .WithTable(f => f.From<Page, Menu>('c')
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
            .Where((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out _);
        Assert.Equal(@"SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `sys_menu` a,(SELECT d.`Id`,d.`ParentId`,c.`Url` FROM `sys_page` c,`sys_menu` d WHERE c.`Id`=d.`PageId`) b WHERE a.`Id`=b.`Id`", sql);

        var result = repository.From<Menu>()
            .WithTable(f => f.From<Page, Menu>('c')
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
            .Where((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToList();
        Assert.True(result.Count > 0);

        int menuId = 1;
        int pageId = 1;
        int pageId2 = 1;
        var sql1 = repository
            .From(f => f.From<Menu>()
                    .Where(x => x.Id == menuId)
                    .Select(x => new { x.Id, x.Name, x.ParentId })
                .UnionAllRecursive((x, y) => x.From<Menu>()
                    .InnerJoin(y, (a, b) => a.ParentId == b.Id)
                    .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
                .AsCteTable("myCteTable1"))
            .WithTable(f => f.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id > pageId2)
                    .Select((x, y) => new { y.Id, x.Url }))
                .AsCteTable("myCteTable2"))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out var dbParameters);
        Assert.Equal(@"WITH RECURSIVE `myCteTable1`(`Id`,`Name`,`ParentId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a WHERE a.`Id`=@p0 UNION ALL
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a INNER JOIN `myCteTable1` b ON a.`ParentId`=b.`Id`
),
`myCteTable2`(`Id`,`Url`) AS 
(
SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `sys_menu` b ON a.`Id`=b.`PageId` WHERE a.`Id`=@p1 UNION ALL
SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `sys_menu` b ON a.`Id`=b.`PageId` WHERE a.`Id`>@p2
)
SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `myCteTable1` a INNER JOIN `myCteTable2` b ON a.`Id`=b.`Id`", sql1);

        var result1 = await repository
            .From(f => f.From<Menu>()
                    .Where(x => x.Id == menuId)
                    .Select(x => new { x.Id, x.Name, x.ParentId })
                .UnionAllRecursive((x, y) => x.From<Menu>()
                    .InnerJoin(y, (a, b) => a.ParentId == b.Id)
                    .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
                .AsCteTable("myCteTable1"))
            .WithTable(f => f.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id > pageId2)
                    .Select((x, y) => new { y.Id, x.Url }))
                .AsCteTable("myCteTable2"))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToListAsync();
        Assert.NotNull(result1);
        Assert.True(result1.Count > 0);

        var sql2 = repository.From<Order, OrderDetail>()
            .InnerJoin((x, y) => x.Id == y.OrderId)
            .Include((x, y) => x.Buyer)
            .Where((a, b) => a.Id == b.OrderId)
            .Select((a, b) => new { Order = a, a.BuyerId, DetailId = b.Id, b.Price, b.Quantity, b.Amount })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,c.`Id`,c.`TenantId`,c.`Name`,c.`Gender`,c.`Age`,c.`CompanyId`,c.`GuidField`,c.`SomeTimes`,c.`SourceType`,c.`IsEnabled`,c.`CreatedAt`,c.`CreatedBy`,c.`UpdatedAt`,c.`UpdatedBy`,a.`BuyerId`,b.`Id` AS `DetailId`,b.`Price`,b.`Quantity`,b.`Amount` FROM `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` LEFT JOIN `sys_user` c ON a.`BuyerId`=c.`Id` WHERE a.`Id`=b.`OrderId`", sql2);

        var result2 = repository.From<Order, OrderDetail>()
            .InnerJoin((x, y) => x.Id == y.OrderId)
            .Include((x, y) => x.Buyer)
            .Where((a, b) => a.Id == b.OrderId)
            .Select((a, b) => new { Order = a, a.BuyerId, DetailId = b.Id, b.Price, b.Quantity, b.Amount })
            .ToList();
        Assert.True(result2.Count > 0);
        Assert.NotNull(result2[0].Order);
        Assert.NotNull(result2[0].Order.Buyer);

        var sql3 = repository.From(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .InnerJoin<Order>((a, b, c) => a.OrderId == c.Id)
            .Select((a, b, c) => new { a.OrderId, a.BuyerId, Buyer = b, Order = c, a.TotalAmount })
            .ToSql(out _);
        Assert.Equal("SELECT a.`OrderId`,a.`BuyerId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,c.`Id`,c.`TenantId`,c.`OrderNo`,c.`ProductCount`,c.`TotalAmount`,c.`BuyerId`,c.`BuyerSource`,c.`SellerId`,c.`Products`,c.`Disputes`,c.`IsEnabled`,c.`CreatedAt`,c.`CreatedBy`,c.`UpdatedAt`,c.`UpdatedBy`,a.`TotalAmount` FROM (SELECT a.`Id` AS `OrderId`,a.`BuyerId`,SUM(b.`Amount`) AS `TotalAmount` FROM `sys_order` a,`sys_order_detail` b,`sys_user` c WHERE a.`Id`=b.`OrderId` AND a.`BuyerId`=c.`Id` AND c.`Age`>20 GROUP BY a.`Id`,a.`BuyerId` HAVING SUM(b.`Amount`)>500) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` INNER JOIN `sys_order` c ON a.`OrderId`=c.`Id`", sql3);

        var result3 = repository.From(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .InnerJoin<Order>((a, b, c) => a.OrderId == c.Id)
            .Select((a, b, c) => new { a.OrderId, a.BuyerId, Buyer = b, Order = c, a.TotalAmount })
            .ToList();
        Assert.True(result3.Count > 0);
    }
    [Fact]
    public void SelectFlattenTo()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Order>("8");
        repository.Create<Order>(new Order
        {
            Id = "8",
            TenantId = "2",
            OrderNo = "On-ZwYx",
            BuyerId = 1,
            SellerId = 2,
            TotalAmount = 500,
            Products = new List<int> { 1, 2 },
            IsEnabled = true,
            CreatedAt = DateTime.Now,
            CreatedBy = 1,
            UpdatedAt = DateTime.Now,
            UpdatedBy = 1
        });
        repository.Commit();

        var result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectFlattenTo<OrderInfo>()
            .ToList();
        Assert.Equal("8", result[0].Id);
        Assert.Equal(1, result[0].BuyerId);
        Assert.Equal("On-ZwYx", result[0].OrderNo);
        Assert.Null(result[0].Description);

        result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectFlattenTo(f => new OrderInfo
            {
                Description = "TotalAmount:" + f.TotalAmount
            })
            .ToList();
        Assert.Equal("8", result[0].Id);
        Assert.Equal(1, result[0].BuyerId);
        Assert.Equal("On-ZwYx", result[0].OrderNo);
        Assert.NotNull(result[0].Description);
        Assert.Equal("TotalAmount:500", result[0].Description);

        result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectFlattenTo(f => new OrderInfo
            {
                Description = this.DeferInvoke().Deferred()
            })
            .ToList();
        Assert.Equal("8", result[0].Id);
        Assert.Equal(1, result[0].BuyerId);
        Assert.Equal("On-ZwYx", result[0].OrderNo);
        Assert.NotNull(result[0].Description);
        Assert.True(result[0].Description == this.DeferInvoke());

        var result1 = repository.From(f =>
               f.From<Order, OrderDetail>('a')
                .Where((a, b) => a.Id == b.OrderId)
                .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                .Select((x, a, b) => new { x.Grouping.BuyerId, x.Grouping.OrderId, ProductTotal = Sql.CountDistinct(b.ProductId) }))
           .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
           .SelectFlattenTo((x, y) => new OrderBuyerInfo { BuyerName = y.Name })
           .First();
        if (result1 != null)
        {
            Assert.NotNull(result1);
            Assert.False(string.IsNullOrEmpty(result1.OrderId));
            Assert.True(result1.BuyerId > 0);
            Assert.Null(result1.OrderNo);
            Assert.NotNull(result1.BuyerName);
        }
    }
    [Fact]
    public void SelectAfterOrderBy()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .LeftJoin<OrderDetail>((a, b, c) => a.Id == c.OrderId)
            .GroupBy((a, b, c) => new { a.BuyerId, OrderId = a.Id, BuyerName = b.Name, BuyerAge = b.Age })
            .Select((x, a, b, c) => new
            {
                x.Grouping.BuyerId,
                x.Grouping.OrderId,
                x.Grouping.BuyerName,
                x.Grouping.BuyerAge,
                ProductCount = x.CountDistinct(c.ProductId),
                LastBuyAt = x.Max(b.CreatedAt).IsNull(a.CreatedAt)
            })
            .OrderByDescending(f => f.LastBuyAt)
            .ToSql(out _);
        Assert.Equal("SELECT a.`BuyerId`,a.`Id` AS `OrderId`,b.`Name` AS `BuyerName`,b.`Age` AS `BuyerAge`,COUNT(DISTINCT c.`ProductId`) AS `ProductCount`,IFNULL(MAX(b.`CreatedAt`),a.`CreatedAt`) AS `LastBuyAt` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` LEFT JOIN `sys_order_detail` c ON a.`Id`=c.`OrderId` GROUP BY a.`BuyerId`,a.`Id`,b.`Name`,b.`Age` ORDER BY IFNULL(MAX(b.`CreatedAt`),a.`CreatedAt`) DESC", sql);

        var result = repository.From<Order>()
           .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
           .LeftJoin<OrderDetail>((a, b, c) => a.Id == c.OrderId)
           .GroupBy((a, b, c) => new { a.BuyerId, OrderId = a.Id, BuyerName = b.Name, BuyerAge = b.Age })
           .Select((x, a, b, c) => new
           {
               x.Grouping.BuyerId,
               x.Grouping.OrderId,
               x.Grouping.BuyerName,
               x.Grouping.BuyerAge,
               ProductCount = x.CountDistinct(c.ProductId),
               LastBuyAt = x.Max(b.CreatedAt).IsNull(a.CreatedAt)
           })
           .OrderByDescending(f => f.LastBuyAt)
           .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
        if (result.Count > 1)
        {
            Assert.True(result[0].LastBuyAt >= result[1].LastBuyAt);
        }
    }
    private string DeferInvoke() => "DeferInvoke";



    [Fact]
    public void Update_AnonymousObject()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var user = repository.Get<User>(1);
        user.Name = "kevin";
        user.Gender = Gender.Female;
        user.SourceType = null;
        var count = repository.Update<User>(user);
        var changedUser = repository.Get<User>(1);
        Assert.True(count > 0);
        Assert.NotNull(changedUser);
        Assert.True(changedUser.Name == user.Name);
        Assert.True(changedUser.SourceType == changedUser.SourceType);

        count = repository.Update<User>(new
        {
            Id = 1,
            Name = (string)null,
            Gender = Gender.Male,
            SourceType = UserSourceType.Douyin
        });
        var result = repository.Get<User>(1);
        Assert.True(count > 0);
        Assert.NotNull(result);
        Assert.Null(result.Name);
        Assert.Equal(UserSourceType.Douyin, result.SourceType);
    }
    [Fact]
    public async Task Update_AnonymousObjects()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var parameters = await repository.From<OrderDetail>()
            .GroupBy(f => f.OrderId)
            .OrderBy((x, f) => f.OrderId)
            .Select((x, f) => new
            {
                Id = x.Grouping,
                TotalAmount = x.Sum(f.Amount) + 50
            })
            .ToListAsync();
        var count = await repository.UpdateAsync<Order>(parameters);
        var ids = parameters.Select(f => f.Id).ToList();
        var orders = await repository.QueryAsync<Order>(f => ids.Contains(f.Id));
        repository.Commit();
        Assert.True(count > 0);
        Assert.True(parameters.Count == orders.Count);
        orders.Sort((x, y) => x.Id.CompareTo(y.Id));
        for (int i = 0; i < orders.Count; i++)
        {
            Assert.True(orders[i].TotalAmount == parameters[i].TotalAmount);
        }
    }
    [Fact]
    public async Task Update_SetBulk()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var parameters = await repository.From<OrderDetail>()
           .GroupBy(f => f.OrderId)
           .Select((x, f) => new
           {
               Id = x.Grouping,
               Amount = x.Sum(f.Amount) + 50
           })
           .ToListAsync();
        var sql = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_order_detail` SET `Amount`=@Amount0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Amount`=@Amount1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Amount`=@Amount2 WHERE `Id`=@kId2", sql);
        Assert.True(dbParameters.Count == parameters.Count * 2);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.True(dbParameters[i * 2].ParameterName == $"@Amount{i}");
            Assert.True(dbParameters[i * 2 + 1].ParameterName == $"@kId{i}");
        }
    }
    [Fact]
    public async Task Update_SetBulk_OnlyFields()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var orderDetails = await repository.From<OrderDetail>()
           .OrderBy(f => f.Id)
           .Take(5)
           .ToListAsync();
        var parameters = orderDetails.Select(f => new
        {
            f.Id,
            Price = f.Price + 80,
            Quantity = f.Quantity + 1,
            Amount = f.Amount + 50,
            UpdatedAt = f.UpdatedAt.AddDays(1)
        })
        .ToList();
        var sql = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .OnlyFields(f => new
            {
                f.Price,
                f.Quantity
            })
            .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_order_detail` SET `Price`=@Price0,`Quantity`=@Quantity0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Price`=@Price1,`Quantity`=@Quantity1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Price`=@Price2,`Quantity`=@Quantity2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Price`=@Price3,`Quantity`=@Quantity3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Price`=@Price4,`Quantity`=@Quantity4 WHERE `Id`=@kId4", sql);
        Assert.Equal(parameters.Count * 3, dbParameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.Equal($"@Price{i}", dbParameters[i * 3].ParameterName);
            Assert.Equal($"@Quantity{i}", dbParameters[i * 3 + 1].ParameterName);
            Assert.Equal($"@kId{i}", dbParameters[i * 3 + 2].ParameterName);
        }

        var ids = parameters.Select(f => f.Id).ToList();
        repository.BeginTransaction();
        var result = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .OnlyFields(f => new
            {
                f.Price,
                f.Quantity
            })
            .Execute();
        var updatedDetails = await repository.From<OrderDetail>()
            .Where(f => ids.Contains(f.Id))
            .OrderBy(f => f.Id)
            .ToListAsync();
        repository.Commit();
        Assert.True(result == parameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.True(updatedDetails[i].Price == parameters[i].Price);
            Assert.True(updatedDetails[i].Quantity == parameters[i].Quantity);
            Assert.True(updatedDetails[i].Amount != parameters[i].Amount);
        }
    }
    [Fact]
    public async Task Update_SetBulk_IgnoreFields()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var orderDetails = await repository.From<OrderDetail>()
            .OrderBy(f => f.Id)
            .Take(5)
            .ToListAsync();
        var parameters = orderDetails.Select(f => new
        {
            f.Id,
            Price = f.Price + 80,
            Quantity = f.Quantity + 1,
            Amount = f.Amount + 50,
            UpdatedAt = f.UpdatedAt.AddDays(1)
        })
        .ToList();
        var sql = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .IgnoreFields(f => f.Price)
            .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_order_detail` SET `Quantity`=@Quantity0,`Amount`=@Amount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Quantity`=@Quantity1,`Amount`=@Amount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Quantity`=@Quantity2,`Amount`=@Amount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Quantity`=@Quantity3,`Amount`=@Amount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Quantity`=@Quantity4,`Amount`=@Amount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4", sql);
        Assert.True(dbParameters.Count == parameters.Count * 4);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.True(dbParameters[i * 4].ParameterName == $"@Quantity{i}");
            Assert.True(dbParameters[i * 4 + 1].ParameterName == $"@Amount{i}");
            Assert.True(dbParameters[i * 4 + 2].ParameterName == $"@UpdatedAt{i}");
            Assert.True(dbParameters[i * 4 + 3].ParameterName == $"@kId{i}");
        }
        var ids = parameters.Select(f => f.Id).ToList();
        repository.BeginTransaction();
        var result = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .IgnoreFields(f => f.Price)
            .Execute();
        var updatedDetails = await repository.QueryAsync<OrderDetail>(f => ids.Contains(f.Id));
        repository.Commit();
        Assert.True(result == parameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.True(updatedDetails[i].Price != parameters[i].Price);
            Assert.True(updatedDetails[i].Quantity == parameters[i].Quantity);
            Assert.True(updatedDetails[i].Amount == parameters[i].Amount);
            Assert.True(updatedDetails[i].UpdatedAt == parameters[i].UpdatedAt);
            Assert.True(updatedDetails[i].Id == parameters[i].Id);
        }
    }
    [Fact]
    public async Task Update_SetBulk_SetFields()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var orderDetails = await repository.From<OrderDetail>()
            .OrderBy(f => f.Id)
            .Take(5)
            .ToListAsync();
        var parameters = orderDetails.Select(f => new
        {
            f.Id,
            Amount = f.Amount + 50,
            UpdatedAt = f.UpdatedAt.AddDays(1)
        })
        .ToList();
        var sql = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .Set(f => f.ProductId, 3)
            .Set(new { Quantity = 5 })
            .Set(f => new { Price = f.Price + 10 })
            .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4", sql);
        Assert.True(dbParameters.Count == parameters.Count * 3 + 2);

        var ids = parameters.Select(f => f.Id).ToList();
        repository.BeginTransaction();
        var result = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .Set(f => f.ProductId, 3)
            .Set(new { Price = 200, Quantity = 5 })
            .IgnoreFields(f => f.Price)
            .Execute();
        var updatedDetails = await repository.QueryAsync<OrderDetail>(f => ids.Contains(f.Id));
        repository.Commit();
        Assert.True(result == parameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.Equal(3, updatedDetails[i].ProductId);
            Assert.Equal(5, updatedDetails[i].Quantity);
            Assert.True(updatedDetails[i].Amount == parameters[i].Amount);
            Assert.True(updatedDetails[i].UpdatedAt == parameters[i].UpdatedAt);
        }
    }
    [Fact]
    public void Update_Fields_Where()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var result = repository.Update<User>(f => new
        {
            Name = f.Name + "_1",
            Gender = Gender.Female,
            SourceType = DBNull.Value
        }, t => t.Id == 1);
        var result1 = repository.Get<User>(1);
        Assert.True(result > 0);
        Assert.NotNull(result1);
        Assert.Equal("leafkevin_1", result1.Name);
        Assert.False(result1.SourceType.HasValue);

        var result2 = repository.Update<User>(new
        {
            Id = 1,
            Name = "kevin",
            Gender = Gender.Female,
            SourceType = DBNull.Value
        });
        var result3 = repository.Get<User>(1);
        Assert.True(result2 > 0);
        Assert.NotNull(result3);
        Assert.Equal("kevin", result3.Name);
        Assert.False(result3.SourceType.HasValue);
    }
    [Fact]
    public void Update_Set_Fields_Where()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var result = repository.Update<User>()
            .Set(f => new
            {
                Age = 25,
                Name = "leafkevin22",
                CompanyId = DBNull.Value
            })
            .Where(f => f.Id == 1)
            .Execute();
        var result2 = repository.Get<User>(1);
        Assert.True(result > 0);
        Assert.NotNull(result2);
        Assert.Equal("leafkevin22", result2.Name);
        Assert.Equal(25, result2.Age);
        Assert.Equal(0, result2.CompanyId);
    }
    [Fact]
    public void Update_Fields_Parameters()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var result = repository.Update<User>(new { Id = 1, Name = "leafkevin11" });
        var result1 = repository.Get<User>(1);
        Assert.True(result > 0);
        Assert.NotNull(result1);
        Assert.Equal("leafkevin11", result1.Name);
    }
    [Fact]
    public void Update_Set_AnonymousObject_Where()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.Update<User>()
            .Set(new
            {
                Age = 25,
                Name = "leafkevin22",
                CompanyId = DBNull.Value
            })
            .Where(f => f.Id == 1)
            .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_user` SET `Age`=@Age,`Name`=@Name,`CompanyId`=@CompanyId WHERE `Id`=1", sql);
        Assert.Equal(3, dbParameters.Count);
        Assert.Equal(25, (int)dbParameters[0].Value);
        Assert.Equal("leafkevin22", (string)dbParameters[1].Value);
        Assert.True(dbParameters[2].Value == DBNull.Value);

        repository.Update<User>()
           .Set(f => new
           {
               Age = 25,
               Name = "leafkevin22",
               CompanyId = DBNull.Value
           })
           .Where(f => f.Id == 1)
           .Execute();
        var result = repository.Get<User>(1);
        Assert.Equal("leafkevin22", result.Name);
        Assert.Equal(25, result.Age);
        Assert.Equal(0, result.CompanyId);
    }
    [Fact]
    public void Update_Set_AnonymousObject_Where_OnlyFields()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var user = repository.Get<User>(1);
        var sql = repository.Update<User>()
            .Set(new
            {
                Age = 30,
                Name = "leafkevinabc",
                CompanyId = 1
            })
            .OnlyFields(f => f.Name)
            .Where(f => f.Id == 1)
            .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=1", sql);
        Assert.Single(dbParameters);
        Assert.Equal("leafkevinabc", (string)dbParameters[0].Value);

        repository.Update<User>()
            .Set(new
            {
                Age = 30,
                Name = "leafkevinabc",
                CompanyId = 1
            })
            .OnlyFields(f => f.Name)
            .Where(f => f.Id == 1)
            .Execute();
        var result = repository.Get<User>(1);
        Assert.Equal("leafkevinabc", result.Name);
        Assert.True(result.Age == user.Age);
        Assert.True(result.CompanyId == user.CompanyId);
    }
    [Fact]
    public void Update_Set_AnonymousObject_Where_IgnoreFields()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.Update<User>()
            .Set(new
            {
                Age = 25,
                Name = "leafkevin22",
                CompanyId = DBNull.Value
            })
            .IgnoreFields(f => f.Name)
            .Where(f => f.Id == 1)
            .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_user` SET `Age`=@Age,`CompanyId`=@CompanyId WHERE `Id`=1", sql);
        Assert.Equal(2, dbParameters.Count);
        Assert.Equal(25, (int)dbParameters[0].Value);
        Assert.True(dbParameters[1].Value == DBNull.Value);

        repository.Update<User>()
            .Set(new
            {
                Age = 25,
                Name = "leafkevin22",
                CompanyId = DBNull.Value
            })
            .IgnoreFields(f => f.Name)
            .Where(f => f.Id == 1)
            .Execute();
        var result = repository.Get<User>(1);
        Assert.NotEqual("leafkevin22", result.Name);
        Assert.Equal(25, result.Age);
        Assert.Equal(0, result.CompanyId);
    }
    [Fact]
    public void Update_SetWith_Parameters()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var result = repository.Update<Order>(new
        {
            ProductCount = 10,
            Id = 1
        });
        var result1 = repository.Get<Order>(new { Id = "1" });
        repository.Commit();
        if (result > 0)
        {
            Assert.NotNull(result1);
            Assert.Equal("1", result1.Id);
            Assert.Equal(10, result1.ProductCount);
        }
        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(new { ProductCount = 11 })
            .Where(new { Id = "1" })
            .Execute();
        var result2 = repository.Get<Order>(new { Id = "1" });
        repository.Commit();
        if (result > 0)
        {
            Assert.NotNull(result2);
            Assert.Equal("1", result2.Id);
            Assert.Equal(11, result2.ProductCount);
        }
        var updateObj = new Dictionary<string, object>
        {
            { "ProductCount", result2.ProductCount + 1 },
            { "TotalAmount", result2.TotalAmount + 100 }
        };
        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(updateObj)
            .Where(new { Id = "1" })
            .Execute();
        var result3 = repository.Get<Order>(new { Id = "1" });
        repository.Commit();
        if (result > 0)
        {
            Assert.NotNull(result3);
            Assert.Equal("1", result3.Id);
            Assert.True(result3.ProductCount == result2.ProductCount + 1);
            Assert.True(result3.TotalAmount == result2.TotalAmount + 100);
        }
    }
    [Fact]
    public async Task Update_MultiParameters()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var parameters = await repository.From<OrderDetail>()
            .Where(f => new[] { "1", "2", "3", "4", "5", "6" }.Contains(f.Id))
            .OrderBy(f => f.Id)
            .Select(f => new { f.Id, Price = f.Price + 80, Quantity = f.Quantity + 2, Amount = f.Amount + 100 })
            .ToListAsync();
        var count = repository.Update<OrderDetail>(parameters);
        var orderDetails = await repository.From<OrderDetail>()
            .Where(f => new[] { "1", "2", "3", "4", "5", "6" }.Contains(f.Id))
            .OrderBy(f => f.Id)
            .Select()
            .ToListAsync();
        repository.Commit();
        Assert.True(count > 0);
        for (int i = 0; i < orderDetails.Count; i++)
        {
            Assert.True(orderDetails[i].Price == parameters[i].Price);
            Assert.True(orderDetails[i].Quantity == parameters[i].Quantity);
            Assert.True(orderDetails[i].Amount == parameters[i].Amount);
        }
    }
    [Fact]
    public void Update_Set_MethodCall()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var parameter = repository.Get<Order>("1");
        parameter.TotalAmount += 50;
        var result = repository.Update<Order>()
            .Set(f => new
            {
                TotalAmount = this.CalcAmount(parameter.TotalAmount, 3),
                Products = this.GetProducts(),
                OrderNo = string.Concat("Order", "111").Substring(0, 7) + "_123"
            })
            .Where(x => x.Id == "2")
            .Execute();
        var order = repository.Get<Order>("2");
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(order.Products);
            Assert.Equal(3, order.Products.Count);
            Assert.Equal(1, order.Products[0]);
            Assert.Equal(2, order.Products[1]);
            Assert.Equal(3, order.Products[2]);
            Assert.True(order.TotalAmount == this.CalcAmount(parameter.TotalAmount, 3));
        }

        var updateObj = repository.Get<Order>("1");
        updateObj.Disputes = new Dispute
        {
            Id = 2,
            Content = "无良商家",
            Result = "同意退款",
            Users = "Buyer2,Seller2",
            CreatedAt = DateTime.Now
        };
        updateObj.UpdatedAt = DateTime.Now;
        int increasedAmount = 50;
        var sql = repository.Update<Order>()
            .Set(f => new
            {
                TotalAmount = this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3),
                Products = this.GetProducts(),
                updateObj.Disputes,
                UpdatedAt = DateTime.Now
            })
            .Where(new { updateObj.Id })
            .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_order` SET `TotalAmount`=@p0,`Products`=@p1,`Disputes`=@p2,`UpdatedAt`=NOW() WHERE `Id`=@kId", sql);
        Assert.Equal(4, dbParameters.Count);
        Assert.Equal("@p0", dbParameters[0].ParameterName);
        Assert.True((double)dbParameters[0].Value == this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3));
        Assert.Equal("@p1", dbParameters[1].ParameterName);
        Assert.True((string)dbParameters[1].Value == new JsonTypeHandler().ToFieldValue(null, this.GetProducts()).ToString());
        Assert.Equal("@p2", dbParameters[2].ParameterName);
        Assert.True((string)dbParameters[2].Value == new JsonTypeHandler().ToFieldValue(null, updateObj.Disputes).ToString());
        Assert.Equal("@kId", dbParameters[3].ParameterName);
        Assert.True((string)dbParameters[3].Value == updateObj.Id);

        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(f => new
            {
                TotalAmount = this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3).Deferred(),
                Products = this.GetProducts(),
                updateObj.Disputes,
                updateObj.UpdatedAt
            })
            .Where(new { updateObj.Id })
            .Execute();

        var updatedOrder = repository.Get<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(updatedOrder.Products);
            Assert.Equal(3, updatedOrder.Products.Count);
            Assert.Equal(1, updatedOrder.Products[0]);
            Assert.Equal(2, updatedOrder.Products[1]);
            Assert.Equal(3, updatedOrder.Products[2]);
            Assert.True(updatedOrder.TotalAmount == this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3));
            //TODO:两个对象的hash值是不同的，各属性值都是一样
            Assert.True(JsonSerializer.Serialize(updatedOrder.Disputes) == JsonSerializer.Serialize(updateObj.Disputes));
            //TODO:两个日期的ticks是不同的，MySqlConnector驱动保存时间就到秒
            //Assert.True(updatedOrder.UpdatedAt == updateObj.UpdatedAt);
        }
    }
    [Fact]
    public void Update_Set_FromQuery_Multi()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Set(true, (x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .Set(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == 1)
          .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1", sql);
        Assert.NotNull(dbParameters);
        Assert.Equal("@p0", dbParameters[0].ParameterName);
        Assert.Equal(200.56, (double)dbParameters[0].Value);
        Assert.Equal("@Products", dbParameters[1].ParameterName);
        Assert.True((string)dbParameters[1].Value == JsonSerializer.Serialize(new List<int> { 1, 2, 3 }));

        var count = repository.Update<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Set(true, (x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .Set(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == 1)
            .Execute();
        Assert.True(count > 0);

        sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.Equal("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        count = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .Execute();
        Assert.True(count > 0);
    }
    [Fact]
    public async Task Update_SetFrom()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.Equal("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        var count = await repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ExecuteAsync();
        Assert.True(count > 0);

        var orderAmounts = repository.From<OrderDetail>()
            .GroupBy(x => x.OrderId)
            .Select((f, a) => new
            {
                OrderId = f.Grouping,
                TotalAmount = f.Sum(a.Amount)
            })
            .ToList();
    }
    [Fact]
    public void Update_Set_Join()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Set(true, (x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .Set(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == 1)
            .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1", sql);
        Assert.NotNull(dbParameters);
        Assert.Equal("@p0", dbParameters[0].ParameterName);
        Assert.Equal(200.56, (double)dbParameters[0].Value);
        Assert.Equal("@Products", dbParameters[1].ParameterName);
        Assert.True((string)dbParameters[1].Value == JsonSerializer.Serialize(new List<int> { 1, 2, 3 }));

        var result = repository.Update<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Set(true, (x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .Set(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == 1)
            .Execute();
        Assert.True(result > 0);

        sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.Equal("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        result = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .Execute();
        Assert.True(result > 0);
    }
    [Fact]
    public async Task Update_Set_FromQuery_One()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        var order = repository.Get<Order>("1");
        var totalAmount = await repository.From<OrderDetail>()
            .Where(f => f.OrderId == "1")
            .SumAsync(f => f.Amount);
        var sql = repository.Update<Order>()
            .SetFrom(f => f.TotalAmount, (x, y) => x
                .From<OrderDetail>('b')
                .Where(t => t.OrderId == y.Id)
                .Select(f => Sql.Sum(f.Amount)))
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.Id == "1")
            .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`Id`='1'", sql);
        Assert.Single(dbParameters);
        Assert.Equal("ON_111", (string)dbParameters[0].Value);

        var count = await repository.Update<Order>()
            .SetFrom(f => f.TotalAmount, (x, y) => x
                .From<OrderDetail>('b')
                .Where(t => t.OrderId == y.Id)
                .Select(f => Sql.Sum(f.Amount)))
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.Id == "1")
            .ExecuteAsync();
        var reult = repository.Get<Order>("1");
        Assert.True(count > 0);
        Assert.True(reult.TotalAmount == totalAmount);
        Assert.True(reult.TotalAmount != order.TotalAmount);

        sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.Equal("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        count = await repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ExecuteAsync();
        Assert.True(count > 0);
    }
    [Fact]
    public void Update_Set_FromQuery_One_Enum()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Company>()
            .SetFrom((a, b) => new
            {
                Nature = a.From<Company>('b')
                    .Where(f => f.Id == 1)
                    .Select(t => t.Nature)
            })
            .Where(f => f.Nature != CompanyNature.Internet)
            .ToSql(out _);
        Assert.Equal("UPDATE `sys_company` a SET a.`Nature`=(SELECT b.`Nature` FROM `sys_company` b WHERE b.`Id`=1) WHERE a.`Nature`<>'Internet'", sql);
        repository.BeginTransaction();
        repository.Update<Company>()
            .Set(f => f.Nature, CompanyNature.Industry)
            .Where(f => f.Id > 1)
            .Execute();
        var result = repository.Update<Company>()
            .SetFrom((a, b) => new
            {
                Nature = a.From<Company>('b')
                    .Where(f => f.Id == 1)
                    .Select(t => t.Nature)
            })
            .Where(f => f.Nature != CompanyNature.Internet)
            .Execute();
        var microCompany = repository.From<Company>('b')
            .Where(f => f.Id == 1)
            .First();
        var companies = repository.Query<Company>(f => f.Id > 1);
        repository.Commit();
        Assert.True(result > 0);
        foreach (var company in companies)
        {
            Assert.True(company.Nature == microCompany.Nature);
        }
    }
    [Fact]
    public async Task Update_Set_FromQuery_Fields()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .SetFrom((x, y) => new
            {
                TotalAmount = x.From<OrderDetail>('b')
                    .Where(f => f.OrderId == y.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.Equal("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        var origValues = await repository.From<Order, OrderDetail>()
            .InnerJoin((x, y) => x.Id == y.OrderId)
            .Where((a, b) => a.BuyerId == 1)
            .GroupBy((a, b) => new { a.Id, a.OrderNo, a.BuyerId })
            .Select((x, a, b) => new { x.Grouping, TotalAmount = x.Sum(b.Amount) })
            .ToListAsync();

        await repository.BeginTransactionAsync();
        var result = repository.Update<Order>()
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .Set((x, y) => new
            {
                TotalAmount = y.Amount,
                OrderNo = x.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where((a, b) => a.BuyerId == 1)
            .Execute();
        var updatedValues = await repository.From<Order, OrderDetail>()
           .InnerJoin((x, y) => x.Id == y.OrderId)
           .Where((a, b) => a.BuyerId == 1)
           .GroupBy((a, b) => new { a.Id, a.OrderNo, a.BuyerId })
           .Select((x, a, b) => new { x.Grouping.Id, x.Grouping.OrderNo, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) })
           .ToListAsync();
        await repository.CommitAsync();
        Assert.True(result > 0);
        foreach (var updatedValue in updatedValues)
        {
            var origValue = origValues.Find(f => f.Grouping.Id == updatedValue.Id);
            Assert.True(updatedValue.TotalAmount == origValue.TotalAmount);
            Assert.True(updatedValue.OrderNo == origValue.Grouping.OrderNo + "_111");
            Assert.True(updatedValue.BuyerId == default);
        }
    }
    [Fact]
    public void Update_InnerJoin_One()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .Set(x => x.TotalAmount, 200.56)
            .Set((a, b) => new
            {
                OrderNo = a.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where((a, b) => a.BuyerId == 1)
            .ToSql(out _);
        Assert.Equal("UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);
        var result = repository.Update<Order>()
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .Set(x => x.TotalAmount, 200.56)
            .Set((a, b) => new
            {
                OrderNo = a.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where((a, b) => a.BuyerId == 1)
            .Execute();
        Assert.True(result > 0);
    }
    [Fact]
    public async Task Update_InnerJoin_Multi()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .Set((x, y) => new
            {
                TotalAmount = y.Amount,
                OrderNo = x.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where((a, b) => a.BuyerId == 1)
            .ToSql(out _);
        Assert.Equal("UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=b.`Amount`,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        var origValues = await repository.From<Order, OrderDetail>()
           .InnerJoin((x, y) => x.Id == y.OrderId)
           .Where((a, b) => a.BuyerId == 1)
           .GroupBy((a, b) => new { a.Id, a.OrderNo, a.BuyerId })
           .Select((x, a, b) => new { x.Grouping, TotalAmount = x.Sum(b.Amount) })
           .ToListAsync();

        await repository.BeginTransactionAsync();
        var result = repository.Update<Order>()
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .Set((x, y) => new
            {
                TotalAmount = y.Amount,
                OrderNo = x.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where((a, b) => a.BuyerId == 1)
            .Execute();
        var updatedValues = await repository.From<Order, OrderDetail>()
           .InnerJoin((x, y) => x.Id == y.OrderId)
           .Where((a, b) => a.BuyerId == 1)
           .GroupBy((a, b) => new { a.Id, a.OrderNo, a.BuyerId })
           .Select((x, a, b) => new { x.Grouping.Id, x.Grouping.OrderNo, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) })
           .ToListAsync();
        await repository.CommitAsync();
        Assert.True(result > 0);
        foreach (var updatedValue in updatedValues)
        {
            var origValue = origValues.Find(f => f.Grouping.Id == updatedValue.Id);
            Assert.True(updatedValue.TotalAmount == origValue.TotalAmount);
            Assert.True(updatedValue.OrderNo == origValue.Grouping.OrderNo + "_111");
            Assert.True(updatedValue.BuyerId == default);
        }
    }
    [Fact]
    public async Task Update_InnerJoin_Fields()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SetFrom((x, y) => new
            {
                TotalAmount = x.From<OrderDetail>('c')
                    .Where(f => f.OrderId == y.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((a, b) => a.Id == "1")
            .ToSql(out _);
        Assert.Equal("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,' - ',CAST(b.`Id` AS CHAR)),a.`BuyerId`=NULL WHERE a.`Id`='1'", sql);

        var origValues = await repository.From<Order, User>()
            .InnerJoin((x, y) => x.BuyerId == y.Id)
            .Where((a, b) => a.Id == "1")
            .Select((a, b) => new { a.OrderNo, b.Id })
            .FirstAsync();
        await repository.BeginTransactionAsync();
        var result = repository.Update<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SetFrom((x, y) => new
            {
                TotalAmount = x.From<OrderDetail>('c')
                    .Where(f => f.OrderId == y.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((a, b) => a.Id == "1")
            .Execute();
        var order = await repository.GetAsync<Order>("1");
        var orderDetails = await repository.QueryAsync<OrderDetail>(f => f.OrderId == "1");
        await repository.CommitAsync();
        Assert.True(result > 0);
        Assert.True(order.TotalAmount == orderDetails.Where(f => f.OrderId == order.Id).Sum(f => f.Amount));
        Assert.True(order.OrderNo == origValues.OrderNo + " - " + origValues.Id.ToString());
        Assert.True(order.BuyerId == default);

        var sql1 = repository.Update<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SetFrom(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
                .Where(f => f.OrderId == y.Id)
                .Select(t => Sql.Sum(t.Amount)))
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((x, y) => x.Id == "2")
            .ToSql(out _);
        Assert.Equal("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,' - ',CAST(b.`Id` AS CHAR)),a.`BuyerId`=NULL WHERE a.`Id`='2'", sql1);

        origValues = await repository.From<Order, User>()
            .InnerJoin((x, y) => x.BuyerId == y.Id)
            .Where((x, y) => x.Id == "2")
            .Select((a, b) => new { a.OrderNo, b.Id })
            .FirstAsync();
        await repository.BeginTransactionAsync();
        result = repository.Update<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SetFrom(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
                .Where(f => f.OrderId == y.Id)
                .Select(t => Sql.Sum(t.Amount)))
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((x, y) => x.Id == "2")
            .Execute();
        order = await repository.GetAsync<Order>("2");
        orderDetails = await repository.QueryAsync<OrderDetail>(f => f.OrderId == "2");
        await repository.CommitAsync();

        Assert.True(result > 0);
        Assert.True(order.TotalAmount == orderDetails.Where(f => f.OrderId == order.Id).Sum(f => f.Amount));
        Assert.True(order.OrderNo == origValues.OrderNo + " - " + origValues.Id.ToString());
        Assert.True(order.BuyerId == default);
    }
    [Fact]
    public void Update_SetNull_WhereNull()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .Set(x => new
            {
                BuyerId = DBNull.Value,
                Seller = (int?)null
            })
            .Where(x => x.OrderNo == null)
            .ToSql(out _);
        Assert.Equal("UPDATE `sys_order` SET `BuyerId`=NULL,`Seller`=NULL WHERE `OrderNo` IS NULL", sql);
    }
    [Fact]
    public void Update_Set()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var parameter = repository.Get<Order>("1");
        parameter.TotalAmount += 50;
        var result = repository.Update<Order>()
            .Set(f => new
            {
                parameter.TotalAmount,
                Products = new List<int> { 1, 2, 3 },
                Disputes = new Dispute
                {
                    Id = 1,
                    Content = "43dss",
                    Users = "1,2",
                    Result = "OK",
                    CreatedAt = DateTime.Now
                }
            })
            .Where(x => x.Id == "1")
            .Execute();
        var order = repository.Get<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(order.Products);
            Assert.Equal(3, order.Products.Count);
            Assert.Equal(1, order.Products[0]);
            Assert.Equal(2, order.Products[1]);
            Assert.Equal(3, order.Products[2]);
            Assert.True(order.TotalAmount == parameter.TotalAmount);
        }

        repository.BeginTransaction();
        parameter = repository.Get<Order>("1");
        parameter.TotalAmount += 50;
        result = repository.Update<Order>()
            .Set(new
            {
                parameter.TotalAmount,
                Products = new List<int> { 1, 2, 3 },
                Disputes = new Dispute
                {
                    Id = 1,
                    Content = "43dss",
                    Users = "1,2",
                    Result = "OK",
                    CreatedAt = DateTime.Now
                }
            })
          .Where(x => x.Id == "1")
          .Execute();
        order = repository.Get<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(order.Products);
            Assert.Equal(3, order.Products.Count);
            Assert.Equal(1, order.Products[0]);
            Assert.Equal(2, order.Products[1]);
            Assert.Equal(3, order.Products[2]);
            Assert.True(order.TotalAmount == parameter.TotalAmount);
        }
    }
    [Fact]
    public void Update_SetJson()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var result = repository.Update<Order>()
            .Set(f => new
            {
                Products = new List<int> { 1, 2, 3 },
                Disputes = new Dispute
                {
                    Id = 1,
                    Content = "43dss",
                    Users = "1,2",
                    Result = "OK",
                    CreatedAt = DateTime.Now
                }
            })
            .Where(x => x.Id == "1")
            .Execute();
        var order = repository.Get<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(order.Products);
            Assert.Equal(3, order.Products.Count);
            Assert.Equal(1, order.Products[0]);
            Assert.Equal(2, order.Products[1]);
            Assert.Equal(3, order.Products[2]);
        }
    }
    [Fact]
    public void Update_SetJson1()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var result = repository.Update<Order>()
            .Set(f => new
            {
                OrderNo = f.OrderNo + "111",
                Products = new List<int> { 1, 2, 3 },
                BuyerId = DBNull.Value,
                UpdatedAt = DateTime.UtcNow
            })
            .Where(x => x.Id == "1")
            .Execute();
        repository.Update<Order>()
            .Set(f => new
            {
                UpdatedAt = DateTime.Now
            })
            .Where(x => x.Id == "2")
            .Execute();
        var order = repository.Get<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(order.Products);
            Assert.Equal(3, order.Products.Count);
            Assert.Equal(1, order.Products[0]);
            Assert.Equal(2, order.Products[1]);
            Assert.Equal(3, order.Products[2]);
        }
    }
    [Fact]
    public void Update_Enum_Fields()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Set((x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .Set(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == 1)
            .ToSql(out var parameters);
        Assert.Equal("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1", sql);
        Assert.Equal("@p0", parameters[0].ParameterName);
        Assert.True(parameters[0].Value.GetType() == typeof(double));
        Assert.Equal(200.56, (double)parameters[0].Value);
        Assert.Equal("@Products", parameters[1].ParameterName);
        Assert.True(parameters[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters[1].Value == new JsonTypeHandler().ToFieldValue(null, new List<int> { 1, 2, 3 }).ToString());

        var sql1 = repository.Update<User>()
            .Set(new { Gender = Gender.Male })
            .Where(new { Id = 1 })
            .ToSql(out var parameters1);
        Assert.Equal("UPDATE `sys_user` SET `Gender`=@Gender WHERE `Id`=@kId", sql1);
        Assert.Equal("@Gender", parameters1[0].ParameterName);
        Assert.True(parameters1[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters1[0].Value == Gender.Male.ToString());

        var sql2 = repository.Update<User>()
            .Set(f => new { Gender = Gender.Male })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters2);
        Assert.Equal("UPDATE `sys_user` SET `Gender`=@p0 WHERE `Id`=1", sql2);
        Assert.Equal("@p0", parameters2[0].ParameterName);
        Assert.True(parameters2[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters2[0].Value == Gender.Male.ToString());

        var user = new User { Gender = Gender.Female };
        var sql3 = repository.Update<User>()
            .Set(f => f.Age, 20)
            .Set(new { user.Gender })
            .Where(new { Id = 1 })
            .ToSql(out var parameters3);
        Assert.Equal("UPDATE `sys_user` SET `Age`=@Age,`Gender`=@Gender WHERE `Id`=@kId", sql3);
        Assert.Equal("@Gender", parameters3[1].ParameterName);
        Assert.True(parameters3[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters3[1].Value == Gender.Female.ToString());

        int age = 20;
        var sql7 = repository.Update<User>()
            .Set(f => f.Age, age)
            .Set(new { Gender = Gender.Male })
            .Where(new { Id = 1 })
            .ToSql(out var parameters7);
        Assert.Equal("UPDATE `sys_user` SET `Age`=@Age,`Gender`=@Gender WHERE `Id`=@kId", sql7);
        Assert.Equal("@Gender", parameters7[1].ParameterName);
        Assert.True(parameters7[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters7[1].Value == Gender.Male.ToString());

        var sql4 = repository.Update<Company>()
            .Set(new { Nature = CompanyNature.Internet })
            .Where(new { Id = 1 })
            .ToSql(out var parameters4);
        Assert.Equal("UPDATE `sys_company` SET `Nature`=@Nature WHERE `Id`=@kId", sql4);
        Assert.Equal("@Nature", parameters4[0].ParameterName);
        Assert.True(parameters4[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters4[0].Value == CompanyNature.Internet.ToString());

        var sql5 = repository.Update<Company>()
            .Set(f => new { Nature = CompanyNature.Internet })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters5);
        Assert.Equal("UPDATE `sys_company` SET `Nature`=@p0 WHERE `Id`=1", sql5);
        Assert.Equal("@p0", parameters5[0].ParameterName);
        Assert.True(parameters5[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters5[0].Value == CompanyNature.Internet.ToString());

        var sql6 = repository.Update<Company>()
            .Set(f => f.Nature, CompanyNature.Internet)
            .Where(new { Id = 1 })
            .ToSql(out var parameters6);
        Assert.Equal("UPDATE `sys_company` SET `Nature`=@Nature WHERE `Id`=@kId", sql6);
        Assert.Equal("@Nature", parameters6[0].ParameterName);
        Assert.True(parameters6[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters6[0].Value == CompanyNature.Internet.ToString());

        var company = new Company { Name = "facebook", Nature = CompanyNature.Internet };
        var sql8 = repository.Update<Company>()
            .Set(f => new { Name = f.Name + "_New", company.Nature })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters8);
        Assert.Equal("UPDATE `sys_company` SET `Name`=CONCAT(`Name`,'_New'),`Nature`=@p0 WHERE `Id`=1", sql8);
        Assert.Equal("@p0", parameters8[0].ParameterName);
        Assert.True(parameters8[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters8[0].Value == CompanyNature.Internet.ToString());

        //批量表达式部分栏位更新
        var sql9 = repository.Update<Company>()
            .SetBulk(new[] { new { Id = 1, Name = "google" }, new { Id = 2, Name = "facebook" } })
            .Set(new { company.Nature })
            .ToSql(out var parameters9);
        Assert.Equal("UPDATE `sys_company` SET `Nature`=@Nature,`Name`=@Name0 WHERE `Id`=@kId0;UPDATE `sys_company` SET `Nature`=@Nature,`Name`=@Name1 WHERE `Id`=@kId1", sql9);
        Assert.Equal(5, parameters9.Count);
        Assert.Equal("@Nature", parameters9[0].ParameterName);
        Assert.True(parameters9[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters9[0].Value == CompanyNature.Internet.ToString());

        CompanyNature? nature = CompanyNature.Production;
        var sql10 = repository.Update<Company>()
            .SetBulk(new[] { new { Id = 1, company.Nature }, new { Id = 2, Nature = nature } })
            .Set(f => new { company.Name })
            .OnlyFields(f => f.Nature)
            .ToSql(out var parameters10);
        Assert.Equal("UPDATE `sys_company` SET `Name`=@p0,`Nature`=@Nature0 WHERE `Id`=@kId0;UPDATE `sys_company` SET `Name`=@p0,`Nature`=@Nature1 WHERE `Id`=@kId1", sql10);
        Assert.Equal("@Nature0", parameters10[1].ParameterName);
        Assert.True(parameters10[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters10[1].Value == company.Nature.ToString());
        Assert.Equal("@Nature1", parameters10[3].ParameterName);
        Assert.True(parameters10[3].Value.GetType() == typeof(string));
        Assert.True((string)parameters10[3].Value == CompanyNature.Production.ToString());
        Assert.Equal("@p0", parameters10[0].ParameterName);
        Assert.True(parameters10[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters10[0].Value == company.Name);
    }
    [Fact]
    public async Task Update_TimeSpan_Fields()
    {
        var repository = this.dbFactory.Create();
        var timeSpan = TimeSpan.FromMinutes(455);
        await repository.DeleteAsync<UpdateEntity>(1);
        await repository.CreateAsync<UpdateEntity>(new UpdateEntity
        {
            Id = 1,
            BooleanField = true,
            TimeSpanField = TimeSpan.FromSeconds(456),
            DateOnlyField = new DateOnly(2022, 05, 06),
            DateTimeField = DateTime.Now,
            DateTimeOffsetField = new DateTimeOffset(DateTime.Parse("2022-01-02 03:04:05")),
            EnumField = Gender.Male,
            GuidField = Guid.NewGuid(),
            TimeOnlyField = new TimeOnly(3, 5, 7)
        });
        var sql1 = repository.Update<User>()
            .Set(new { SomeTimes = timeSpan })
            .Where(new { Id = 1 })
            .ToSql(out var parameters1);
        Assert.Equal("UPDATE `sys_user` SET `SomeTimes`=@SomeTimes WHERE `Id`=@kId", sql1);
        Assert.Equal("@SomeTimes", parameters1[0].ParameterName);
        Assert.True(parameters1[0].Value.GetType() == typeof(TimeOnly));
        Assert.True((TimeOnly)parameters1[0].Value == TimeOnly.FromTimeSpan(timeSpan));

        var sql2 = repository.Update<User>()
            .Set(f => new { SomeTimes = timeSpan })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters2);
        Assert.Equal("UPDATE `sys_user` SET `SomeTimes`=@p0 WHERE `Id`=1", sql2);
        Assert.Equal("@p0", parameters2[0].ParameterName);
        Assert.True(parameters2[0].Value.GetType() == typeof(TimeOnly));
        Assert.True((TimeOnly)parameters2[0].Value == TimeOnly.FromTimeSpan(timeSpan));

        repository.BeginTransaction();
        await repository.Update<User>()
            .Set(new { SomeTimes = timeSpan })
            .Where(new { Id = 1 })
            .ExecuteAsync();
        var userInfo = repository.Get<User>(1);
        repository.Commit();
        Assert.True(userInfo.SomeTimes.Value == TimeOnly.FromTimeSpan(timeSpan));
    }
    [Fact]
    public async Task Update_BulkCopy()
    {
        var repository = this.dbFactory.Create();
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

        var updateObjs = orders.Select(f => new
        {
            f.Id,
            TenantId = "1",
            TotalAmount = f.TotalAmount + 20,
            Products = new List<int> { 1, 2, 3 },
            Disputes = new Dispute
            {
                Id = 1,
                Content = "无良商家",
                Result = "同意退款",
                Users = "Buyer1,Seller1",
                CreatedAt = DateTime.Now
            }
        });
        count = await repository.Update<Order>()
            .SetBulkCopy(updateObjs)
            .ExecuteAsync();

        Assert.True(count == orders.Count);
    }
    private double CalcAmount(double price, double amount) => price * amount - 150;
    private int[] GetProducts() => new int[] { 1, 2, 3 };



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
        var repository = this.dbFactory.Create();
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
        Assert.Equal("DELETE FROM `sys_user` WHERE `Id`=1", sql);
    }
    [Fact]
    public async Task Delete_Multi()
    {
        var repository = this.dbFactory.Create();
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
        Assert.Equal("DELETE FROM `sys_user` WHERE `Id` IN (@Id0,@Id1)", sql);
        Assert.Equal(1, (int)parameters[0].Value);
        Assert.Equal(2, (int)parameters[1].Value);

        var sql1 = repository.Delete<Function>()
            .Where(new[] { new { MenuId = 1, PageId = 1 }, new { MenuId = 2, PageId = 2 } })
            .ToSql(out parameters);
        Assert.Equal("DELETE FROM `sys_function` WHERE `MenuId`=@MenuId0 AND `PageId`=@PageId0 OR `MenuId`=@MenuId1 AND `PageId`=@PageId1", sql1);
        Assert.Equal(4, parameters.Count);
        Assert.Equal(1, (int)parameters[0].Value);
        Assert.Equal(1, (int)parameters[1].Value);
        Assert.Equal(2, (int)parameters[2].Value);
        Assert.Equal(2, (int)parameters[3].Value);
    }
    [Fact]
    public async Task Delete_Multi1()
    {
        var repository = this.dbFactory.Create();
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
        Assert.Equal("DELETE FROM `sys_user` WHERE `Id` IN (@Id0,@Id1)", sql);
        Assert.Equal(1, (int)parameters[0].Value);
        Assert.Equal(2, (int)parameters[1].Value);

        var orderNos = new string[] { "ON_001", "ON_002", "ON_003" };
        sql = repository.Delete<Order>()
            .Where(f => f.BuyerId == 1 && orderNos.Contains(f.OrderNo))
            .ToSql(out parameters);
        Assert.Equal("DELETE FROM `sys_order` WHERE `BuyerId`=1 AND `OrderNo` IN (@p0,@p1,@p2)", sql);
        Assert.Equal(orderNos[0], (string)parameters[0].Value);
        Assert.Equal(orderNos[1], (string)parameters[1].Value);
        Assert.Equal(orderNos[2], (string)parameters[2].Value);
    }
    [Fact]
    public async Task Delete_Multi_Where()
    {
        var repository = this.dbFactory.Create();
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
        Assert.Equal("DELETE FROM `sys_user` WHERE `Id` IN (1,2)", sql);
        //Assert.True((int)parameters[0].Value == 1);
        //Assert.True((int)parameters[1].Value == 2);
    }
    [Fact]
    public void Delete_Where_And()
    {
        var repository = this.dbFactory.Create();
        bool? isMale = true;
        var sql = repository.Delete<User>()
            .Where(f => f.Name.Contains("kevin"))
            .And(isMale.HasValue, f => f.Age > 25)
            .ToSql(out _);
        Assert.Equal("DELETE FROM `sys_user` WHERE `Name` LIKE '%kevin%' AND `Age`>25", sql);
    }
    [Fact]
    public void Delete_Enum_Fields()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.Delete<User>()
            .Where(f => f.Gender == Gender.Male)
            .ToSql(out _);
        Assert.Equal("DELETE FROM `sys_user` WHERE `Gender`='Male'", sql1);

        var gender = Gender.Male;
        var sql2 = repository.Delete<User>()
            .Where(f => f.Gender == gender)
            .ToSql(out var parameters1);
        Assert.Equal("DELETE FROM `sys_user` WHERE `Gender`=@p0", sql2);
        Assert.Equal("@p0", parameters1[0].ParameterName);
        Assert.True(parameters1[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters1[0].Value == gender.ToString());

        var sql3 = repository.Delete<Company>()
             .Where(f => f.Nature == CompanyNature.Internet)
             .ToSql(out _);
        Assert.Equal("DELETE FROM `sys_company` WHERE `Nature`='Internet'", sql3);

        var nature = CompanyNature.Internet;
        var sql4 = repository.Delete<Company>()
             .Where(f => f.Nature == nature)
             .ToSql(out var parameters2);
        Assert.Equal("DELETE FROM `sys_company` WHERE `Nature`=@p0", sql4);
        Assert.Equal("@p0", parameters2[0].ParameterName);
        Assert.True(parameters2[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters2[0].Value == CompanyNature.Internet.ToString());
    }
    [Fact]
    public void Transation()
    {
        var repository = this.dbFactory.Create();
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



    [Fact]
    public async Task MultipleQuery()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        using var reader = await repository.QueryMultipleAsync(f => f
            .Get<User>(new { Id = 1 })
            .Exists<Order>(f => f.BuyerId.IsNull())
            .From<Order>()
                .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
                .Where((x, y) => x.Id == "1")
                .Select((x, y) => new { x.Id, x.OrderNo, x.BuyerId, BuyerName = y.Name, x.TotalAmount })
                .First()
            .QueryFirst<User>(new { Id = 2 })
            .From<Product>()
                .Include(f => f.Brand)
                .Where(f => f.ProductNo.Contains("PN-00"))
                .ToList()
            .From(f => f.From<Order, OrderDetail>('a')
                    .Where((a, b) => a.Id == b.OrderId && a.Id == "1")
                    .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                    .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                    .Select((x, a, b) => new { a.Id, x.Grouping, ProductTotal = Sql.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
                .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
                .Select((x, y) => new { x.Id, x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
                .First());
        var sql = reader.ToSql(out var dbParameters);
        var userInfo = await reader.ReadFirstAsync<User>();
        var isExists = await reader.ReadFirstAsync<bool>();
        var orderInfo = await reader.ReadFirstAsync<dynamic>();
        var userInfo2 = await reader.ReadFirstAsync<User>();
        //var products = await reader.ReadAsync<Product>();
        //var groupedOrderInfo = await reader.ReadFirstAsync<dynamic>();
        //Assert.Null(userInfo);
        //Assert.False(isExists);
        //Assert.Null(orderInfo);
        //Assert.Null(userInfo2);
        //Assert.Empty(products);
        //Assert.Null(groupedOrderInfo);
    }
    [Fact]
    public async Task MultipleQuery_UseMaster()
    {
        Initialize();
        var repository = this.dbFactory.Create();
        using var reader = await repository.QueryMultipleAsync(f => f
            .UseMaster()
            .Get<User>(new { Id = 1 })
            .Exists<Order>(f => f.BuyerId.IsNull())
            .From<Order>()
                .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
                .Where((x, y) => x.Id == "1")
                .Select((x, y) => new { x.Id, x.OrderNo, x.BuyerId, BuyerName = y.Name, x.TotalAmount })
                .First()
            .QueryFirst<User>(new { Id = 2 })
            .From<Product>()
                .Include(f => f.Brand)
                .Where(f => f.ProductNo.Contains("PN-00"))
                .ToList()
            .From(f => f.From<Order, OrderDetail>('a')
                    .Where((a, b) => a.Id == b.OrderId && a.Id == "1")
                    .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                    .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                    .Select((x, a, b) => new { a.Id, x.Grouping, ProductTotal = Sql.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
                .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
                .Select((x, y) => new { x.Id, x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
                .First());
        var sql = reader.ToSql(out var dbParameters);
        var userInfo = await reader.ReadFirstAsync<User>();
        var isExists = await reader.ReadFirstAsync<bool>();
        var orderInfo = await reader.ReadFirstAsync<dynamic>();
        var userInfo2 = await reader.ReadFirstAsync<User>();
        var products = await reader.ReadAsync<Product>();
        var groupedOrderInfo = await reader.ReadFirstAsync<dynamic>();
        Assert.NotNull(userInfo);
        Assert.Equal(1, userInfo.Id);
        Assert.Equal("1", orderInfo.Id);
        Assert.Equal("1", groupedOrderInfo.Id);
        Assert.Equal("1", groupedOrderInfo.Grouping.OrderId);
    }
    [Fact]
    public async Task MultipleCommand()
    {
        var repository = this.dbFactory.Create();
        int[] productIds = new int[] { 2, 4, 5, 6 };
        int category = 1;
        var commands = new List<MultipleCommand>();
        var deleteCommand = repository.Delete<Product>()
            .Where(f => productIds.Contains(f.Id))
            .ToMultipleCommand();

        var insertCommand = repository.Create<Product>()
           .WithBy(new
           {
               Id = 2,
               ProductNo = "PN_111",
               Name = "PName_111",
               BrandId = 1,
               CategoryId = category,
               CompanyId = 1,
               IsEnabled = true,
               CreatedBy = 1,
               CreatedAt = DateTime.Now,
               UpdatedBy = 1,
               UpdatedAt = DateTime.Now
           })
           .ToMultipleCommand();

        var insertCommand2 = repository.Create<Product>()
            .WithBulk(new[]
            {
                new
                {
                    Id = 4,
                    ProductNo="PN-004",
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
                    Id = 5,
                    ProductNo="PN-005",
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
                    Id = 6,
                    ProductNo="PN-006",
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
            .ToMultipleCommand();

        var updateCommand = repository.Update<Order>()
           .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
           .Set(true, (x, y) => new
           {
               TotalAmount = 200.56,
               OrderNo = x.OrderNo + "-111",
               BuyerSource = y.SourceType
           })
           .Set(x => x.Products, new List<int> { 1, 2, 3 })
           .Where((a, b) => a.Id == "1")
           .ToMultipleCommand();

        var orderDetails = await repository.From<OrderDetail>().ToListAsync();
        var parameters = orderDetails.Select(f => new
        {
            f.Id,
            Amount = f.Amount + 50,
            UpdatedAt = f.UpdatedAt.AddDays(1)
        })
        .ToList();
        var bulkUpdateCommand = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .Set(f => f.ProductId, 3)
            .Set(new { Quantity = 5 })
            .Set(f => new { Price = f.Price + 10 })
            .ToMultipleCommand();

        commands.AddRange(new[] { deleteCommand, insertCommand, insertCommand2, updateCommand, bulkUpdateCommand });
        var count = repository.MultipleExecute(commands);
        Assert.True(count > 0);
    }



    [Fact]
    public async Task WhereBoolean()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => f.IsEnabled);
        Assert.True(result1.Count > 0);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.True(result2.Count > 0);
        Assert.True(result1.Count == result2.Count);
    }
    [Fact]
    public async Task WhereMemberVisit()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => !(f.IsEnabled == false) && f.Id > 0);
        Assert.True(result1.Count > 0);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.True(result2.Count > 0);
        Assert.True(result1.Count == result2.Count);
    }
    [Fact]
    public async Task WhereStringEnum()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Company>()
            .Where(f => f.Nature == CompanyNature.Internet)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE a.`Nature`='Internet'", sql1);
        var result1 = await repository.QueryAsync<Company>(f => f.Nature == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);

        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')='Internet'", sql2);
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result2.Count >= 2);

        var localNature = CompanyNature.Internet;
        var sql3 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')=@p0", sql3);
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result3 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result3.Count >= 2);
    }
    [Fact]
    public async Task WhereCoalesceConditional2()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')='Internet'", sql1);
        var result1 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);
        Assert.Equal(CompanyNature.Internet, (result1[0].Nature ?? CompanyNature.Internet));

        var localNature = CompanyNature.Internet;
        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')=@p0", sql2);
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result2.Count >= 2);
        Assert.Equal(localNature, (result2[0].Nature ?? CompanyNature.Internet));

        var sql3 = repository.From<Company>()
            .Where(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature)
            .ToSql(out dbParameters);
        Assert.Equal("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN a.`Nature` ELSE 'Internet' END)=@p0", sql3);
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result3 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.True(result3.Count >= 2);
        Assert.Equal(localNature, result3[0].Nature);

        var sql4 = repository.From<User>()
            .Where(f => (f.IsEnabled ? f.SourceType : UserSourceType.Website) > UserSourceType.Website)
            .Select(f => f.Id)
            .ToSql(out dbParameters);
        Assert.Equal("SELECT a.`Id` FROM `sys_user` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN a.`SourceType` ELSE 'Website' END)>'Website'", sql4);
        var result5 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.True(result5.Count >= 2);
        Assert.Equal(localNature, result5[0].Nature);
    }
    [Fact]
    public async Task WhereIsNull()
    {
        this.Initialize();
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Order>()
           .Where(f => f.BuyerId.IsNull())
           .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order` a WHERE a.`BuyerId` IS NULL", sql1);
        repository.BeginTransaction();
        repository.Update<Order>(f => new { BuyerId = DBNull.Value }, f => f.Id == "1");
        var result1 = repository.Get<Order>("1");
        repository.Commit();
        Assert.Equal(0, result1.BuyerId);
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result2.Count >= 2);
        var localNature = CompanyNature.Internet;
        var result3 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result2.Count >= 2);
    }
    [Fact]
    public void WhereAndOr()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order, User>()
            .Where((a, b) => a.BuyerId == b.Id)
            .And(true, (a, b) => a.SellerId.IsNull() || !a.ProductCount.HasValue)
            .And(true, (a, b) => a.Products != null)
            .And(true, (a, b) => a.Products == null || a.Disputes == null)
            .Select((a, b) => "*")
            .ToSql(out _);
        Assert.Equal("SELECT * FROM `sys_order` a,`sys_user` b WHERE a.`BuyerId`=b.`Id` AND (a.`SellerId` IS NULL OR a.`ProductCount` IS NULL) AND a.`Products` IS NOT NULL AND (a.`Products` IS NULL OR a.`Disputes` IS NULL)", sql);

        var filterExpr = PredicateBuilder.Create<Order, User>()
            .And((x, y) => x.BuyerId <= 10 && x.ProductCount > 5 && y.SourceType == UserSourceType.Douyin)
            .Or((x, y) => x.BuyerId > 10 && x.ProductCount <= 5 && y.SourceType == UserSourceType.Website)
            .Or((x, y) => x.BuyerSource == UserSourceType.Taobao)
            .Build();
        sql = repository.From<Order, User>()
            .Where((a, b) => a.BuyerId == b.Id || b.SourceType == UserSourceType.Douyin)
            .And(true, (a, b) => (a.BuyerSource == UserSourceType.Taobao || a.SellerId.IsNull() && !a.ProductCount.HasValue) || a.ProductCount > 1 || a.TotalAmount > 500 && a.BuyerSource == UserSourceType.Website)
            .And(true, filterExpr)
            .And(true, (a, b) => a.Products == null || a.Disputes == null)
            .Select((a, b) => "*")
        .ToSql(out _);
        Assert.Equal("SELECT * FROM `sys_order` a,`sys_user` b WHERE (a.`BuyerId`=b.`Id` OR b.`SourceType`='Douyin') AND (a.`BuyerSource`='Taobao' OR (a.`SellerId` IS NULL AND a.`ProductCount` IS NULL) OR a.`ProductCount`>1 OR (a.`TotalAmount`>500 AND a.`BuyerSource`='Website')) AND ((a.`BuyerId`<=10 AND a.`ProductCount`>5 AND b.`SourceType`='Douyin') OR (a.`BuyerId`>10 AND a.`ProductCount`<=5 AND b.`SourceType`='Website') OR a.`BuyerSource`='Taobao') AND (a.`Products` IS NULL OR a.`Disputes` IS NULL)", sql);
    }
    [Fact]
    public void Where()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") && string.IsNullOrEmpty(f.OrderNo)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_order` a WHERE EXISTS(SELECT * FROM `sys_user` t WHERE t.`Id`=a.`BuyerId` AND t.`IsEnabled`=1) AND (a.`BuyerId` IS NULL OR a.`BuyerId`=2) AND a.`OrderNo` LIKE '%ON_%' AND (a.`OrderNo` IS NULL OR a.`OrderNo`='')", sql1);
        var result1 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo)))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result1);
        Assert.True(result1.Count > 0);

        var sql2 = repository.From<Order>()
            .Where(f => (f.BuyerId.IsNull() || f.BuyerId == 2) && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo))
                && (Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) || f.SellerId.IsNull()))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_order` a WHERE (a.`BuyerId` IS NULL OR a.`BuyerId`=2) AND (a.`OrderNo` LIKE '%ON_%' OR (a.`OrderNo` IS NULL OR a.`OrderNo`='')) AND (EXISTS(SELECT * FROM `sys_user` t WHERE t.`Id`=a.`BuyerId` AND t.`IsEnabled`=1) OR a.`SellerId` IS NULL)", sql2);
        var result2 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo)))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result2);
        Assert.True(result2.Count > 0);

        var sql3 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") && string.IsNullOrEmpty(f.OrderNo)) || DateTime.IsLeapYear(f.CreatedAt.Year))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id` FROM `sys_order` a WHERE EXISTS(SELECT * FROM `sys_user` t WHERE t.`Id`=a.`BuyerId` AND t.`IsEnabled`=1) AND (a.`BuyerId` IS NULL OR a.`BuyerId`=2) AND a.`OrderNo` LIKE '%ON_%' AND (a.`OrderNo` IS NULL OR a.`OrderNo`='') OR (YEAR(a.`CreatedAt`)%4=0 AND YEAR(a.`CreatedAt`)%100<>0 OR YEAR(a.`CreatedAt`)%400=0)", sql3);
        var result3 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") && string.IsNullOrEmpty(f.OrderNo)) || DateTime.IsLeapYear(f.CreatedAt.Year))
            .Select(f => f.Id)
            .ToList();
        Assert.NotNull(result3);
        Assert.True(result3.Count > 0);
    }
}