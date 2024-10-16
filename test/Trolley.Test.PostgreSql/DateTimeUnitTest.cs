using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Trolley.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.PostgreSql;

public class DateTimeUnitTest : UnitTestBase
{
    private readonly ITestOutputHelper output;
    public DateTimeUnitTest(ITestOutputHelper output)
    {
        this.output = output;
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
                .Register(OrmProviderType.PostgreSql, "fengling", "Host=localhost;Database=fengling;Username=postgres;Password=123456;SearchPath=public", true)
                .Configure<ModelConfiguration>(OrmProviderType.PostgreSql)
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
                        this.output.WriteLine($"{evt.SqlType} Begin, TransactionId:{evt.TransactionId} Sql: {evt.Sql}, Parameters: {evt.DbParameters.ToPostgreSqlParametersString()}");
                    };
                    df.OnCommandExecuted += evt =>
                    {
                        this.output.WriteLine($"{evt.SqlType} End, TransactionId:{evt.TransactionId} Elapsed: {evt.Elapsed} ms, Sql: {evt.Sql}, Parameters: {evt.DbParameters.ToPostgreSqlParametersString()}");
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
    [Fact]
    public async Task MemberAccess()
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
                this.UnixEpoch,
                DateTime.Parse("2023-05-06").Date,
                CurrentDate = DateTime.Now.Date,
                localDate,
                IsEquals = f.UpdatedAt.Equals(DateTime.Parse("2023-03-25")),
                IsEquals1 = f.UpdatedAt.Equals(localDate)
            })
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT CURRENT_TIMESTAMP AS \"Now\",TIMESTAMP '0001-01-01 00:00:00.000' AS \"MinValue\",TIMESTAMP '9999-12-31 23:59:59.999' AS \"MaxValue\",(CURRENT_TIMESTAMP AT TIME ZONE 'UTC') AS \"UtcNow\",CURRENT_DATE AS \"Today\",@p0 AS \"UnixEpoch\",TIMESTAMP '2023-05-06 00:00:00.000' AS \"Date\",CURRENT_TIMESTAMP::DATE AS \"CurrentDate\",@p1 AS \"localDate\",(a.\"UpdatedAt\"=TIMESTAMP '2023-03-25 00:00:00.000') AS \"IsEquals\",(a.\"UpdatedAt\"=@p2) AS \"IsEquals1\" FROM \"sys_user\" a WHERE a.\"Id\"=1", sql);
        Assert.Equal(3, dbParameters.Count);
        Assert.Equal(typeof(DateTime), dbParameters[0].Value.GetType());
        Assert.Equal(typeof(DateTime), dbParameters[1].Value.GetType());
        Assert.Equal(typeof(DateTime), dbParameters[2].Value.GetType());
        Assert.Equal(this.UnixEpoch, (DateTime)dbParameters[0].Value);
        Assert.Equal(localDate, (DateTime)dbParameters[1].Value);
        Assert.Equal(localDate, (DateTime)dbParameters[2].Value);

        var lastNow = DateTime.Parse("2024-10-10 05:06:07.123");
        var result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.UpdatedAt,
                DateTime.Now,
                lastNow,
                DateTime.MinValue,
                DateTime.MaxValue,
                DateTime.UtcNow,
                DateTime.Today,
                this.UnixEpoch,
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
        //Assert.Equal(now, result.Now);
        Assert.Equal(lastNow, result.lastNow);
        Assert.Equal(this.UnixEpoch, result.UnixEpoch);
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
        Assert.Equal("SELECT (a.\"CreatedAt\"+ INTERVAL '365D') AS \"Add\",(a.\"CreatedAt\"+INTERVAL '1D'*30) AS \"AddDays\",(a.\"CreatedAt\"+INTERVAL '1S'*300/1000) AS \"AddMilliseconds\",(a.\"CreatedAt\"-INTERVAL '365D 00:00:00.000000') AS \"Subtract1\",(CURRENT_TIMESTAMP-INTERVAL '365D 00:00:00.000000') AS \"Subtract2\",(a.\"UpdatedAt\"-a.\"CreatedAt\") AS \"Subtract3\",EXTRACT(DAYS FROM (MAKE_DATE(EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4,EXTRACT(MONTH FROM CURRENT_TIMESTAMP)::INT4,1)+INTERVAL '1 MONTH'-INTERVAL '1 DAY')) AS \"DayInMonth\",((EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4)%4=0 AND (EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4)%100<>0 OR (EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4)%400=0) AS \"IsLeapYear1\",TRUE AS \"IsLeapYear2\",(TO_CHAR(CURRENT_TIMESTAMP,'YYYY-MM-DD HH24:MI:SS')::TIMESTAMP) AS \"Parse\",TIMESTAMP '2023-05-07 13:08:45.000' AS \"ParseExact\" FROM \"sys_user\" a WHERE a.\"UpdatedAt\">(CURRENT_TIMESTAMP-@p0-INTERVAL '00:25:00.000000')", sql);

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
        Assert.Equal("SELECT (CASE WHEN a.\"CreatedAt\"=TIMESTAMP '2023-03-03 00:00:00.000' THEN 0 WHEN a.\"CreatedAt\">TIMESTAMP '2023-03-03 00:00:00.000' THEN 1 ELSE -1 END) AS \"CompareTo\",(a.\"CreatedAt\"-INTERVAL '365D 00:00:00.000000') AS \"OneYearsAgo1\",(CURRENT_TIMESTAMP-TIMESTAMP '2023-03-20 00:00:00.000') AS \"OneYearsAgo2\",EXTRACT(DAYS FROM (MAKE_DATE(EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4,EXTRACT(MONTH FROM CURRENT_TIMESTAMP)::INT4,1)+INTERVAL '1 MONTH'-INTERVAL '1 DAY')) AS \"DayInMonth\",((EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4)%4=0 AND (EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4)%100<>0 OR (EXTRACT(YEAR FROM CURRENT_TIMESTAMP)::INT4)%400=0) AS \"IsLeapYear1\",TRUE AS \"IsLeapYear2\",(TO_CHAR(CURRENT_TIMESTAMP,'YYYY-MM-DD HH24:MI:SS')::TIMESTAMP) AS \"Parse\",TIMESTAMP '2023-05-07 13:08:45.000' AS \"ParseExact\" FROM \"sys_user\" a WHERE (CASE WHEN a.\"UpdatedAt\"=TIMESTAMP '2023-03-20 00:00:00.000' THEN 0 WHEN a.\"UpdatedAt\">TIMESTAMP '2023-03-20 00:00:00.000' THEN 1 ELSE -1 END)>0", sql);

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
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                MulOp = TimeSpan.FromMinutes(25) * 3,
                DivOp1 = TimeSpan.FromHours(30) / 5,
                DivOp2 = TimeSpan.FromHours(30) / TimeSpan.FromHours(3)
#else
                MulOp = TimeSpan.FromMinutes(25 * 3),
                DivOp1 = TimeSpan.FromHours(30 / 5),
                DivOp2 = 10
#endif
            })
            .ToSql(out _);
        Assert.Equal("SELECT INTERVAL '05:06:07.000000' AS \"DateSub\",(a.\"CreatedAt\"+ INTERVAL '05:00:00.000000') AS \"AddOp\",(a.\"CreatedAt\"-INTERVAL '10:00:00.000000') AS \"SubOp\",(a.\"SomeTimes\"+INTERVAL '00:25:00.000000') AS \"AddOp1\",INTERVAL '1D 05:45:00.000000' AS \"SubOp1\",(a.\"UpdatedAt\"-a.\"CreatedAt\") AS \"SubOp2\",INTERVAL '01:15:00.000000' AS \"MulOp\",INTERVAL '06:00:00.000000' AS \"DivOp1\",10 AS \"DivOp2\" FROM \"sys_user\" a WHERE (CASE WHEN a.\"UpdatedAt\"=TIMESTAMP '2023-03-20 00:00:00.000' THEN 0 WHEN a.\"UpdatedAt\">TIMESTAMP '2023-03-20 00:00:00.000' THEN 1 ELSE -1 END)>0", sql);
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
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                MulOp = TimeSpan.FromMinutes(25) * 3,
                DivOp1 = TimeSpan.FromHours(30) / 5,
                DivOp2 = TimeSpan.FromHours(30) / TimeSpan.FromHours(3)
#else
                MulOp = TimeSpan.FromMinutes(25 * 3),
                DivOp1 = TimeSpan.FromHours(30 / 5),
                DivOp2 = 10
#endif
            })
            .FirstAsync();
        Assert.Equal(result.DateSub, DateTime.Parse("2022-01-01 05:06:07") - DateTime.Parse("2022-01-01"));
        Assert.Equal(result.AddOp, result.CreatedAt + TimeSpan.FromHours(5));
        Assert.Equal(result.SubOp, result.CreatedAt - TimeSpan.FromHours(10));
        Assert.Equal(result.AddOp1, result.SomeTimes.Value.Add(TimeSpan.FromMinutes(25)));
        Assert.Equal(result.SubOp1, TimeSpan.FromHours(30) - TimeSpan.FromMinutes(15));
        Assert.Equal(result.SubOp2, result.UpdatedAt - result.CreatedAt);
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        Assert.Equal(result.MulOp, TimeSpan.FromMinutes(25) * 3);
        Assert.Equal(result.DivOp1, TimeSpan.FromHours(30) / 5);
        Assert.Equal(result.DivOp2, TimeSpan.FromHours(30) / TimeSpan.FromHours(3));
#else
        Assert.Equal(result.MulOp, TimeSpan.FromMinutes(25 * 3));
        Assert.Equal(result.DivOp1, TimeSpan.FromHours(30 / 5));
        Assert.Equal(10, result.DivOp2);
#endif
    }
}
