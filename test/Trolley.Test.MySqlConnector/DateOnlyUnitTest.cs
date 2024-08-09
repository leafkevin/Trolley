using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using Trolley.MySqlConnector;
using Xunit;

namespace Trolley.Test.MySqlConnector;

public class DateOnlyUnitTest : UnitTestBase
{
    public DateOnlyUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register(OrmProviderType.MySql, "fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;", true)
            .Configure<ModelConfiguration>(OrmProviderType.MySql);
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async void MemberAccess()
    {
        this.Initialize();
        var localDate = DateOnly.FromDateTime(DateTime.Parse("2023-05-06"));
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "SELECT CURDATE() AS `Today`,DATE(NOW()) AS `Today1`,'2024-07-15' AS `FromDayNumber`,@p0 AS `localDate`,'0001-01-01' AS `MinValue`,'9999-12-31' AS `MaxValue`,(a.`UpdatedAt`='2023-03-25') AS `IsEquals`,(a.`UpdatedAt`=@p1) AS `IsEquals1`,DATEDIFF(DATE(NOW()),'0001-01-01') AS `DayNumber`,DAYOFMONTH(DATE(NOW())) AS `Day`,MONTH(DATE(NOW())) AS `Month`,YEAR(DATE(NOW())) AS `Year`,(DAYOFWEEK(DATE(NOW()))-1) AS `DayOfWeek` FROM `sys_user` a WHERE a.`Id`=1");
        Assert.True(dbParameters.Count == 2);
        Assert.True(dbParameters[0].Value.GetType() == typeof(DateOnly));
        Assert.True(dbParameters[1].Value.GetType() == typeof(DateOnly));
        Assert.True((DateOnly)dbParameters[0].Value == localDate);
        Assert.True((DateOnly)dbParameters[1].Value == localDate);

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
        Assert.True(result.MinValue == DateOnly.MinValue);
        Assert.True(result.MaxValue == DateOnly.MaxValue);
        Assert.True(result.Today == DateTime.Now.Date);
        Assert.True(result.Today1 == DateOnly.FromDateTime(DateTime.Now));
        Assert.True(result.localDate == localDate);
        Assert.True(result.IsEquals == result.UpdatedAt.Equals(DateTime.Parse("2023-03-25")));
        Assert.True(result.IsEquals1 == result.UpdatedAt.Equals(localDate));
        Assert.True(result.DayNumber == DateOnly.FromDateTime(DateTime.Now).DayNumber);
        Assert.True(result.Day == DateOnly.FromDateTime(DateTime.Now).Day);
        Assert.True(result.Month == DateOnly.FromDateTime(DateTime.Now).Month);
        Assert.True(result.Year == DateOnly.FromDateTime(DateTime.Now).Year);
        Assert.True(result.DayOfWeek == DateOnly.FromDateTime(DateTime.Now).DayOfWeek);
    }
    [Fact]
    public async void AddCompareTo()
    {
        this.Initialize();
        var localDate = DateOnly.FromDateTime(DateTime.Parse("2023-05-06"));
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "SELECT DATE_ADD(a.`DateOnlyField`,INTERVAL 30 DAY) AS `AddDays`,DATE_ADD(a.`DateOnlyField`,INTERVAL 5 MONTH) AS `AddMonths`,DATE_ADD(a.`DateOnlyField`,INTERVAL 2 YEAR) AS `AddYears`,(CASE WHEN a.`DateOnlyField`=@p0 THEN 0 WHEN a.`DateOnlyField`>@p0 THEN 1 ELSE -1 END) AS `CompareTo`,@p1 AS `Parse`,'2023-05-07' AS `ParseExact` FROM `sys_update_entity` a WHERE a.`Id`=1");

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
        Assert.True(result.AddDays == result.DateOnlyField.AddDays(30));
        Assert.True(result.AddMonths == result.DateOnlyField.AddMonths(5));
        Assert.True(result.AddYears == result.DateOnlyField.AddYears(2));
        Assert.True(result.CompareTo == result.DateOnlyField.CompareTo(localDate));
        Assert.True(result.Parse == DateOnly.Parse(localDate.ToString("yyyy-MM-dd")));
        Assert.True(result.ParseExact == DateOnly.ParseExact("05-07/2023", "MM-dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None));
    }
}
