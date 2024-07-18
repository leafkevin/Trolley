using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using Trolley.PostgreSql;
using Xunit;

namespace Trolley.Test.PostgreSql;

public class DateTimeUnitTest : UnitTestBase
{
    public DateTimeUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register<PostgreSqlProvider>("fengling", "Host=localhost;Database=fengling;Username=postgres;Password=123456;SearchPath=public", true, "public")
            .Configure<PostgreSqlProvider, ModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
    }
    [Fact]
    public async void MemberAccess()
    {
        this.Initialize();
        var localDate = DateTime.Parse("2023-05-06").Date;
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
                localDate,
                IsEquals = f.UpdatedAt.Equals(DateTime.Parse("2023-03-25")),
                IsEquals1 = f.UpdatedAt.Equals(localDate)
            })
            .ToSql(out var dbParameters);
        Assert.True(sql == "SELECT CURRENT_TIMESTAMP AS \"Now\",TIMESTAMP '0001-01-01 00:00:00.000' AS \"MinValue\",TIMESTAMP '9999-12-31 23:59:59.999' AS \"MaxValue\",(CURRENT_TIMESTAMP AT TIME ZONE 'UTC') AS \"UtcNow\",CURRENT_DATE AS \"Today\",TIMESTAMP '1970-01-01 00:00:00.000' AS \"UnixEpoch\",TIMESTAMP '2023-05-06 00:00:00.000' AS \"Date\",CAST(CURRENT_TIMESTAMP AS DATE) AS \"CurrentDate\",@p0 AS \"localDate\",(a.\"UpdatedAt\"=TIMESTAMP '2023-03-25 00:00:00.000') AS \"IsEquals\",(a.\"UpdatedAt\"=@p1) AS \"IsEquals1\" FROM \"sys_user\" a WHERE a.\"Id\"=1");
        Assert.True(dbParameters.Count == 2);
        Assert.True(dbParameters[0].Value.GetType() == typeof(DateTime));
        Assert.True(dbParameters[1].Value.GetType() == typeof(DateTime));
        Assert.True((DateTime)dbParameters[0].Value == localDate);
        Assert.True((DateTime)dbParameters[1].Value == localDate);

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
        Assert.True(result.MinValue == DateTime.MinValue);
        //由于精度不同，差一些微秒
        //Assert.True(result.MaxValue == DateTime.MaxValue);
        //取决于时区的设置        
        //Assert.True(result.Today == DateTime.Now.Date);
        Assert.True(result.UnixEpoch == DateTime.UnixEpoch);
        Assert.True(result.Date == DateTime.Parse("2023-05-06").Date);
        Assert.True(result.localDate == localDate);
        Assert.True(result.IsEquals == result.UpdatedAt.Equals(DateTime.Parse("2023-03-25")));
    }
    [Fact]
    public async void AddSubtract()
    {
        this.Initialize();
        var days = 365;
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "SELECT (a.\"CreatedAt\"+ INTERVAL '365D') AS \"Add\",(a.\"CreatedAt\"+INTERVAL '1D'*30) AS \"AddDays\",(a.\"CreatedAt\"+INTERVAL '1S'*300/1000) AS \"AddMilliseconds\",(a.\"CreatedAt\"-INTERVAL '365D 00:00:00.000000') AS \"Subtract1\",(CURRENT_TIMESTAMP-INTERVAL '365D 00:00:00.000000') AS \"Subtract2\",(a.\"UpdatedAt\"-a.\"CreatedAt\") AS \"Subtract3\",EXTRACT(DAYS FROM (MAKE_DATE(EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4,EXTRACT(MONTH FROM CURRENT_TIMESTAMP)::INT4,1)+INTERVAL '1 MONTH'-INTERVAL '1 DAY')) AS \"DayInMonth\",((EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4)%4=0 AND (EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4)%100<>0 OR (EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4)%400=0) AS \"IsLeapYear1\",TRUE AS \"IsLeapYear2\",(TO_CHAR(CURRENT_TIMESTAMP,'YYYY-MM-DD HH24:MI:SS')::TIMESTAMP) AS \"Parse\",TIMESTAMP '2023-05-07 13:08:45.000' AS \"ParseExact\" FROM \"sys_user\" a WHERE a.\"UpdatedAt\">(CURRENT_TIMESTAMP-@p0-INTERVAL '00:25:00.000000')");

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
        Assert.True(result.Add == result.CreatedAt.Add(TimeSpan.FromDays(365)));
        Assert.True(result.AddDays == result.CreatedAt.AddDays(30));
        Assert.True(result.AddMilliseconds == result.CreatedAt.AddMilliseconds(300));
        Assert.True(result.Subtract1 == result.CreatedAt.Subtract(TimeSpan.FromDays(365)));
        Assert.True(result.Subtract2 == result.Now - TimeSpan.FromDays(365));
        Assert.True(result.Subtract3 == result.UpdatedAt - result.CreatedAt);
        Assert.True(result.DayInMonth == DateTime.DaysInMonth(now.Year, now.Month));
        Assert.True(result.IsLeapYear1 == DateTime.IsLeapYear(now.Year));
        Assert.True(result.IsLeapYear2 == DateTime.IsLeapYear(2020));
        Assert.True(result.Parse == DateTime.Parse(now.ToString("yyyy-MM-dd HH:mm:ss")));
        Assert.True(result.ParseExact == DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture));
    }
    [Fact]
    public async void Compare()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "SELECT (CASE WHEN a.\"CreatedAt\"=TIMESTAMP '2023-03-03 00:00:00.000' THEN 0 WHEN a.\"CreatedAt\">TIMESTAMP '2023-03-03 00:00:00.000' THEN 1 ELSE -1 END) AS \"CompareTo\",(a.\"CreatedAt\"-INTERVAL '365D 00:00:00.000000') AS \"OneYearsAgo1\",(CURRENT_TIMESTAMP-TIMESTAMP '2023-03-20 00:00:00.000') AS \"OneYearsAgo2\",EXTRACT(DAYS FROM (MAKE_DATE(EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4,EXTRACT(MONTH FROM CURRENT_TIMESTAMP)::INT4,1)+INTERVAL '1 MONTH'-INTERVAL '1 DAY')) AS \"DayInMonth\",((EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4)%4=0 AND (EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4)%100<>0 OR (EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4)%400=0) AS \"IsLeapYear1\",TRUE AS \"IsLeapYear2\",(TO_CHAR(CURRENT_TIMESTAMP,'YYYY-MM-DD HH24:MI:SS')::TIMESTAMP) AS \"Parse\",TIMESTAMP '2023-05-07 13:08:45.000' AS \"ParseExact\" FROM \"sys_user\" a WHERE (CASE WHEN a.\"UpdatedAt\"=TIMESTAMP '2023-03-20 00:00:00.000' THEN 0 WHEN a.\"UpdatedAt\">TIMESTAMP '2023-03-20 00:00:00.000' THEN 1 ELSE -1 END)>0");

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
        Assert.True(result.Compare == DateTime.Compare(result.UpdatedAt, DateTime.Parse("2023-03-20")));
        Assert.True(result.CompareTo == result.CreatedAt.CompareTo(DateTime.Parse("2023-03-03")));
        Assert.True(result.OneYearsAgo1 == result.CreatedAt.Subtract(TimeSpan.FromDays(365)));
        Assert.True(result.OneYearsAgo2 == result.CreatedAt - DateTime.Parse("2023-03-20"));
        Assert.True(result.Subtract == result.CreatedAt.Subtract(DateTime.Parse("2023-03-01")));
        Assert.True(result.DayInMonth == DateTime.DaysInMonth(now.Year, now.Month));
        Assert.True(result.IsLeapYear1 == DateTime.IsLeapYear(now.Year));
        Assert.True(result.IsLeapYear2 == DateTime.IsLeapYear(2020));
        Assert.True(result.Parse == DateTime.Parse(now.ToString("yyyy-MM-dd HH:mm:ss")));
        Assert.True(result.ParseExact == DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture));
    }
    [Fact]
    public async void Operation()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "SELECT INTERVAL '05:06:07.000000' AS \"DateSub\",(a.\"CreatedAt\"+ INTERVAL '05:00:00.000000') AS \"AddOp\",(a.\"CreatedAt\"-INTERVAL '10:00:00.000000') AS \"SubOp\",(a.\"SomeTimes\"+INTERVAL '00:25:00.000000') AS \"AddOp1\",INTERVAL '1D 05:45:00.000000' AS \"SubOp1\",(a.\"UpdatedAt\"-a.\"CreatedAt\") AS \"SubOp2\",INTERVAL '01:15:00.000000' AS \"MulOp\",INTERVAL '06:00:00.000000' AS \"DivOp1\",10 AS \"DivOp2\" FROM \"sys_user\" a WHERE (CASE WHEN a.\"UpdatedAt\"=TIMESTAMP '2023-03-20 00:00:00.000' THEN 0 WHEN a.\"UpdatedAt\">TIMESTAMP '2023-03-20 00:00:00.000' THEN 1 ELSE -1 END)>0");
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
        Assert.True(result.DateSub == DateTime.Parse("2022-01-01 05:06:07") - DateTime.Parse("2022-01-01"));
        Assert.True(result.AddOp == result.CreatedAt + TimeSpan.FromHours(5));
        Assert.True(result.SubOp == result.CreatedAt - TimeSpan.FromHours(10));
        Assert.True(result.AddOp1 == result.SomeTimes.Value.Add(TimeSpan.FromMinutes(25)));
        Assert.True(result.SubOp1 == TimeSpan.FromHours(30) - TimeSpan.FromMinutes(15));
        Assert.True(result.SubOp2 == result.UpdatedAt - result.CreatedAt);
        Assert.True(result.MulOp == TimeSpan.FromMinutes(25) * 3);
        Assert.True(result.DivOp1 == TimeSpan.FromHours(30) / 5);
        Assert.True(result.DivOp2 == TimeSpan.FromHours(30) / TimeSpan.FromHours(3));
    }
}
