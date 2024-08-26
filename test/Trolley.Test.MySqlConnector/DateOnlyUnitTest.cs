using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Trolley.MySqlConnector;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.MySqlConnector;

public class DateOnlyUnitTest : UnitTestBase
{
    private static int connTotal = 0;
    private static int connOpenTotal = 0;
    private readonly ITestOutputHelper output;
    public DateOnlyUnitTest(ITestOutputHelper output)
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
}
