using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Trolley.SqlServer;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.SqlServer;

public class DateTimeUnitTest : UnitTestBase
{
    private static int connTotal = 0;
    private static int connOpenTotal = 0;
    private readonly ITestOutputHelper output;
    public DateTimeUnitTest(ITestOutputHelper output)
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
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();
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
                DateTime.UnixEpoch,
                DateTime.Parse("2023-05-06").Date,
                CurrentDate = DateTime.Now.Date,
                localDate,
                IsEquals = f.UpdatedAt.Equals(DateTime.Parse("2023-03-25")),
                IsEquals1 = f.UpdatedAt.Equals(localDate)
            })
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT GETDATE() AS [Now],'0001-01-01 00:00:00.000' AS [MinValue],'9999-12-31 23:59:59.999' AS [MaxValue],GETUTCDATE() AS [UtcNow],CONVERT(DATE,GETDATE()) AS [Today],'1970-01-01 00:00:00.000' AS [UnixEpoch],'2023-05-06 00:00:00.000' AS [Date],CONVERT(DATE,GETDATE()) AS [CurrentDate],@p0 AS [localDate],(CASE WHEN a.[UpdatedAt]='2023-03-25 00:00:00.000' THEN 1 ELSE 0 END) AS [IsEquals],(CASE WHEN a.[UpdatedAt]=@p1 THEN 1 ELSE 0 END) AS [IsEquals1] FROM [sys_user] a WHERE a.[Id]=1", sql);
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
        //Assert.True(result.Today == DateTime.Now.Date);
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
        Assert.Equal("SELECT DATEADD(DAY,365,a.[CreatedAt]) AS [Add],DATEADD(DAY,30,a.[CreatedAt]) AS [AddDays],DATEADD(MILLISECOND,300,a.[CreatedAt]) AS [AddMilliseconds],(DATEADD(DAY,-365,a.[CreatedAt])) AS [Subtract1],(DATEADD(DAY,-365,GETDATE())) AS [Subtract2],(CASE WHEN DATEDIFF(SECOND,a.[CreatedAt],a.[UpdatedAt])>0 THEN CAST(DATEDIFF(DAY,a.[CreatedAt],a.[UpdatedAt]) AS VARCHAR)+'.'+CAST(CONVERT(TIME,DATEADD(SECOND,DATEDIFF(SECOND,a.[CreatedAt],a.[UpdatedAt])%86400,'00:00:00')) AS VARCHAR) ELSE CAST(DATEDIFF(DAY,a.[CreatedAt],a.[UpdatedAt])+1 AS VARCHAR)+'.'+CAST(CONVERT(TIME,DATEADD(SECOND,86400-DATEDIFF(SECOND,a.[CreatedAt],a.[UpdatedAt])%86400,'00:00:00')) AS VARCHAR) END) AS [Subtract3],DAY(EOMONTH(CAST(DATEPART(YEAR,GETDATE()) AS NVARCHAR(4))+'-'+CAST(DATEPART(MONTH,GETDATE()) AS NVARCHAR(2))+'-01')) AS [DayInMonth],(CASE WHEN DATEPART(YEAR,GETDATE())%4=0 AND DATEPART(YEAR,GETDATE())%100<>0 OR DATEPART(YEAR,GETDATE())%400=0 THEN 1 ELSE 0 END) AS [IsLeapYear1],1 AS [IsLeapYear2],CAST(FORMAT(GETDATE(),N'yyyy-MM-dd HH:mm:ss') AS DATETIME) AS [Parse],'2023-05-07 13:08:45.000' AS [ParseExact] FROM [sys_user] a WHERE a.[UpdatedAt]>(DATEADD(MILLISECOND,-1500000,DATEADD(DAY,-365,GETDATE())))", sql);

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
                OneYearsAgo2 = f.CreatedAt - DateTime.Parse("2023-03-20"),
                DayInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month),
                IsLeapYear1 = DateTime.IsLeapYear(DateTime.Now.Year),
                IsLeapYear2 = DateTime.IsLeapYear(2020),
                Parse = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                ParseExact = DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture)
            })
            .ToSql(out _);
        Assert.Equal("SELECT (CASE WHEN a.[CreatedAt]='2023-03-03 00:00:00.000' THEN 0 WHEN a.[CreatedAt]>'2023-03-03 00:00:00.000' THEN 1 ELSE -1 END) AS [CompareTo],(DATEADD(DAY,-365,a.[CreatedAt])) AS [OneYearsAgo1],(CASE WHEN DATEDIFF(SECOND,'2023-03-20 00:00:00.000',a.[CreatedAt])>0 THEN CAST(DATEDIFF(DAY,'2023-03-20 00:00:00.000',a.[CreatedAt]) AS VARCHAR)+'.'+CAST(CONVERT(TIME,DATEADD(SECOND,DATEDIFF(SECOND,'2023-03-20 00:00:00.000',a.[CreatedAt])%86400,'00:00:00')) AS VARCHAR) ELSE CAST(DATEDIFF(DAY,'2023-03-20 00:00:00.000',a.[CreatedAt])+1 AS VARCHAR)+'.'+CAST(CONVERT(TIME,DATEADD(SECOND,86400-DATEDIFF(SECOND,'2023-03-20 00:00:00.000',a.[CreatedAt])%86400,'00:00:00')) AS VARCHAR) END) AS [OneYearsAgo2],DAY(EOMONTH(CAST(DATEPART(YEAR,GETDATE()) AS NVARCHAR(4))+'-'+CAST(DATEPART(MONTH,GETDATE()) AS NVARCHAR(2))+'-01')) AS [DayInMonth],(CASE WHEN DATEPART(YEAR,GETDATE())%4=0 AND DATEPART(YEAR,GETDATE())%100<>0 OR DATEPART(YEAR,GETDATE())%400=0 THEN 1 ELSE 0 END) AS [IsLeapYear1],1 AS [IsLeapYear2],CAST(FORMAT(GETDATE(),N'yyyy-MM-dd HH:mm:ss') AS DATETIME) AS [Parse],'2023-05-07 13:08:45.000' AS [ParseExact] FROM [sys_user] a WHERE (CASE WHEN a.[UpdatedAt]='2023-03-20 00:00:00.000' THEN 0 WHEN a.[UpdatedAt]>'2023-03-20 00:00:00.000' THEN 1 ELSE -1 END)>0", sql);
		
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
                 //TODO:这里有个issue，当返回值是负值时，值不对
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
        Assert.Equal("SELECT '05:06:07.000000' AS [DateSub],DATEADD(MILLISECOND,18000000,a.[CreatedAt]) AS [AddOp],(DATEADD(MILLISECOND,-36000000,a.[CreatedAt])) AS [SubOp],CAST(DATEADD(SECOND,DATEDIFF(SECOND,'00:00:00',a.[SomeTimes])+DATEDIFF(SECOND,'00:00:00','00:25:00.000000'),'00:00:00') AS TIME) AS [AddOp1],'1.05:45:00.000000' AS [SubOp1],(CASE WHEN DATEDIFF(SECOND,a.[CreatedAt],a.[UpdatedAt])>0 THEN CAST(DATEDIFF(DAY,a.[CreatedAt],a.[UpdatedAt]) AS VARCHAR)+'.'+CAST(CONVERT(TIME,DATEADD(SECOND,DATEDIFF(SECOND,a.[CreatedAt],a.[UpdatedAt])%86400,'00:00:00')) AS VARCHAR) ELSE CAST(DATEDIFF(DAY,a.[CreatedAt],a.[UpdatedAt])+1 AS VARCHAR)+'.'+CAST(CONVERT(TIME,DATEADD(SECOND,86400-DATEDIFF(SECOND,a.[CreatedAt],a.[UpdatedAt])%86400,'00:00:00')) AS VARCHAR) END) AS [SubOp2],'01:15:00.000000' AS [MulOp],'06:00:00.000000' AS [DivOp1],10 AS [DivOp2] FROM [sys_user] a WHERE (CASE WHEN a.[UpdatedAt]='2023-03-20 00:00:00.000' THEN 0 WHEN a.[UpdatedAt]>'2023-03-20 00:00:00.000' THEN 1 ELSE -1 END)>0", sql);
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
}
