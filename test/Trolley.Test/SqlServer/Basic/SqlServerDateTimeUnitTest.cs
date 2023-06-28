using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using Trolley.SqlServer;
using Xunit;

namespace Trolley.Test.SqlServer;

public class SqlServerDateTimeUnitTest : UnitTestBase
{
    public SqlServerDateTimeUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register("fengling", true, f =>
            {
                var connectionString = "Server=127.0.0.1;Database=fengling;Uid=sa;password=SQLserverSA123456;TrustServerCertificate=true";
                f.Add<SqlServerProvider>(connectionString, true);
            })
            .AddTypeHandler<JsonTypeHandler>()
            .Configure<SqlServerProvider, SqlServerModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async void MemberAccess()
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
        Assert.True(sql == "SELECT GETDATE() AS [Now],'0001-01-01 00:00:00.0000000' AS [MinValue],'9999-12-31 23:59:59.9999999' AS [MaxValue],GETUTCDATE() AS [UtcNow],CONVERT(DATE,GETDATE()) AS [Today],'1970-01-01 00:00:00.0000000' AS [UnixEpoch],'2023-05-06 00:00:00.000' AS [Date],CONVERT(DATE,GETDATE()) AS [CurrentDate],(CASE WHEN [UpdatedAt]='2023-03-25 00:00:00.000' THEN 1 ELSE 0 END) AS [IsEquals] FROM [sys_user] WHERE [Id]=1");
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
              IsEquals = f.UpdatedAt.Equals(DateTime.Parse("2023-03-25"))
          })
          .FirstAsync();
        Assert.True(result.MinValue == DateTime.MinValue);
        Assert.True(result.MaxValue == DateTime.MaxValue);
        Assert.True(result.Today == DateTime.UtcNow.Date);
        Assert.True(result.UnixEpoch == DateTime.UnixEpoch);
        Assert.True(result.Date == DateTime.Parse("2023-05-06").Date);
        Assert.True(result.IsEquals == result.UpdatedAt.Equals(DateTime.Parse("2023-03-25")));
    }
    [Fact]
    public void AddSubtract()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
          .Where(f => f.UpdatedAt > DateTime.Now - TimeSpan.FromDays(365))
          .Select(f => new
          {
              Add = f.CreatedAt.Add(TimeSpan.FromDays(365)),
              AddDays = f.CreatedAt.AddDays(30),
              AddMilliseconds = f.CreatedAt.AddMilliseconds(300),
              Subtract1 = f.CreatedAt.Subtract(TimeSpan.FromDays(365)),
              Subtract2 = DateTime.Now - TimeSpan.FromDays(365),
              DayInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month),
              IsLeapYear1 = DateTime.IsLeapYear(DateTime.Now.Year),
              IsLeapYear2 = DateTime.IsLeapYear(2020),
              Parse = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
              ParseExact = DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture)
          })
          .ToSql(out _);
        Assert.True(sql == "SELECT DATEADD(DAY,365,[CreatedAt]) AS [Add],DATEADD(DAY,30,[CreatedAt]) AS [AddDays],DATEADD(MILLISECOND,300,[CreatedAt]) AS [AddMilliseconds],DATEADD(DAY,-365,[CreatedAt]) AS [Subtract1],DATEADD(DAY,-365,GETDATE()) AS [Subtract2],30 AS [DayInMonth],0 AS [IsLeapYear1],1 AS [IsLeapYear2],CAST(FORMAT(GETDATE(),'yyyy-MM-dd HH:mm:ss') AS DATETIME) AS [Parse],'2023-05-07 13:08:45.000' AS [ParseExact] FROM [sys_user] WHERE [UpdatedAt]>DATEADD(DAY,-365,GETDATE())");
    }
    [Fact]
    public void Compare()
    {
        Initialize();
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
        Assert.True(sql == "SELECT (CASE WHEN [CreatedAt]='2023-03-20 00:00:00.0000000' THEN 0 WHEN [CreatedAt]>'2023-03-20 00:00:00.0000000' THEN 1 ELSE -1 END) AS [CompareTo],CAST([CreatedAt]-CAST(DATEADD(MILLISECOND,CAST('365 00:00:00.0000000'*86400000 AS BIGINT),'00:00:00') AS TIME) AS DATETIME) AS [OneYearsAgo1],CAST(GETDATE()-'2023-03-20 00:00:00.0000000' AS TIME) AS [OneYearsAgo2],DAY(EOMONTH('DATEPART(YEAR,GETDATE())-DATEPART(MONTH,GETDATE())-01') AS [DayInMonth],((DATEPART(YEAR,GETDATE()))%4=0 AND (DATEPART(YEAR,GETDATE()))%100<>0 OR (DATEPART(YEAR,GETDATE()))%400=0) AS [IsLeapYear1],1 AS [IsLeapYear2],CAST(FORMAT(GETDATE(),'yyyy-MM-dd HH:mm:ss') AS DATETIME) AS [Parse],@p0 AS [ParseExact] FROM [sys_user] WHERE (CASE WHEN [UpdatedAt]='2023-03-20 00:00:00.0000000' THEN 0 WHEN [UpdatedAt]>'2023-03-20 00:00:00.0000000' THEN 1 ELSE -1 END)>0");
    }
    [Fact]
    public void Operation()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
          //.Where(f => DateTime.Compare(f.UpdatedAt, DateTime.Parse("2023-03-20")) > 0)
          .Select(f => new
          {
              AddOp = f.CreatedAt + TimeSpan.FromHours(5),
              SubOp = f.CreatedAt - TimeSpan.FromHours(10),
              AddOp1 = f.SomeTimes.Value.Add(TimeSpan.FromMinutes(25)),
              SubOp1 = TimeSpan.FromHours(30) - TimeSpan.FromMinutes(15),
              MulOp = TimeSpan.FromMinutes(25) * 3,
              DivOp1 = TimeSpan.FromHours(30) / 5,
              DivOp2 = TimeSpan.FromHours(30) / TimeSpan.FromHours(3)
          })
          .ToSql(out _);
        Assert.True(sql == "SELECT DATE_ADD(`CreatedAt`,INTERVAL(5*3600000000) MICROSECOND) AS `AddOp`,DATE_SUB(`CreatedAt`,INTERVAL 10*3600000000 MICROSECOND) AS `SubOp` FROM `sys_user` WHERE (CASE WHEN `UpdatedAt`='2023-03-20 00:00:00' THEN 0 WHEN `UpdatedAt`>'2023-03-20 00:00:00' THEN 1 ELSE -1 END)>0");
    }
}
