using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trolley.MySqlConnector;

namespace Trolley.Test.MySqlConnector;

public class AllUnitTest : UnitTestBase
{
    [SetUp]
    public void Setup()
    {
        var connectionString = "Server=192.168.61.67;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
        var connectionString1 = "Server=192.168.61.67;Database=fengling1;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
        var connectionString2 = "Server=192.168.61.67;Database=fengling2;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
        var builder = new OrmDbFactoryBuilder()
            .Register(OrmProviderType.MySql, "fengling", f => f.Use(connectionString)
                .UseSlave(connectionString1, connectionString2), true)
            .Register(OrmProviderType.MySql, "fengling1", f => f.Use(connectionString1))
            .Register(OrmProviderType.MySql, "fengling2", f => f.Use(connectionString2))
            .UseMapping<ModelMappingConfiguration>(OrmProviderType.MySql)
            .UseTableSharding<TableShardingConfiguration>(OrmProviderType.MySql)
            .UseInterceptor(new MyDbInterceptor());
        this.dbFactory = builder.Build();
        this.Initialize(1);
    }
    private async Task InitSharding()
    {
        var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .UseTableBy("105")
            .WhereByIds(new[] { 101, 102, 103 })
            .ExecuteAsync();
        repository.Create<User>(new[]
        {
            new User
            {
                Id = 101,
                TenantId ="104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                UpdatedBy = 1
            },
            new User
            {
                Id = 102,
                TenantId ="105",
                Name = "cindy",
                Age = 21,
                CompanyId = 2,
                Gender = Gender.Female,
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
#else
                SomeTimes = TimeSpan.FromSeconds(5730),
#endif
                SourceType = UserSourceType.Taobao,
                IsEnabled = true,
                CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new User
            {
                Id = 103,
                TenantId ="105",
                Name = "xiyuan",
                Age = 17,
                CompanyId = 3,
                Gender = Gender.Female,
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
#else
                SomeTimes = TimeSpan.FromSeconds(5730),
#endif
                SourceType = UserSourceType.Taobao,
                IsEnabled = true,
                CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        var createdAt = DateTime.Parse("2024-05-24");
        var orders = new List<Order>();
        var orderDetails = new List<OrderDetail>();
        for (int i = 1000; i < 2000; i++)
        {
            var orderId = $"ON_{i + 1}";
            orders.Add(new Order
            {
                Id = orderId,
                TenantId = "104",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 101,
                SellerId = 2,
                TotalAmount = 420,
                ProductCount = 2,
                Products = new List<int> { 1, 2 },
                Disputes = new Dispute
                {
                    Id = i + 1,
                    Content = "无良商家",
                    Result = "同意退款",
                    Users = "Buyer2,Seller2",
                    CreatedAt = createdAt
                },
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{1000 + (i - 1000) * 2 + 1}",
                TenantId = "104",
                Amount = 240,
                OrderId = orderId,
                Price = 120,
                ProductId = 11,
                Quantity = 2,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{1000 + (i - 1000) * 2 + 2}",
                TenantId = "104",
                Amount = 180,
                OrderId = orderId,
                Price = 180,
                ProductId = 12,
                Quantity = 1,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
        }
        for (int i = 2000; i < 3000; i++)
        {
            var orderId = $"ON_{i + 1}";
            orders.Add(new Order
            {
                Id = orderId,
                TenantId = "105",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 102,
                SellerId = 2,
                TotalAmount = 630,
                ProductCount = 2,
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
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{2000 + (i - 2000) * 2 + 1}",
                TenantId = "105",
                Amount = 230,
                OrderId = orderId,
                Price = 230,
                ProductId = 13,
                Quantity = 1,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{2000 + (i - 2000) * 2 + 2}",
                TenantId = "105",
                Amount = 400,
                OrderId = orderId,
                Price = 200,
                ProductId = 14,
                Quantity = 2,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
        }
        var removeIds = orders.Select(f => f.Id).ToList();

        await repository.BeginTransactionAsync();
        var count = await repository.Delete<Order>()
            .UseTableBy("104", createdAt)
            .UseTableBy("105", createdAt)
            .Where(f => removeIds.Contains(f.Id))
            .ExecuteAsync();
        count = await repository.Delete<OrderDetail>()
            .UseTableBy("104", createdAt)
            .UseTableBy("105", createdAt)
            .Where(f => removeIds.Contains(f.OrderId))
            .ExecuteAsync();

        var count1 = await repository.Create<Order>()
            .WithBulkCopy(orders)
            .ExecuteAsync();
        var count2 = await repository.Create<OrderDetail>()
            .WithBulkCopy(orderDetails)
            .ExecuteAsync();
        await repository.CommitAsync();
    }
#if NET6_0_OR_GREATER
    [Test]
    public async Task MemberAccess()
    {
        this.Initialize(1);
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
        Assert.AreEqual("SELECT CURDATE() AS `Today`,DATE(NOW()) AS `Today1`,'2024-07-15' AS `FromDayNumber`,@p0 AS `localDate`,'0001-01-01' AS `MinValue`,'9999-12-31' AS `MaxValue`,(a.`UpdatedAt`='2023-03-25') AS `IsEquals`,(a.`UpdatedAt`=@p1) AS `IsEquals1`,DATEDIFF(DATE(NOW()),'0001-01-01') AS `DayNumber`,DAYOFMONTH(DATE(NOW())) AS `Day`,MONTH(DATE(NOW())) AS `Month`,YEAR(DATE(NOW())) AS `Year`,(DAYOFWEEK(DATE(NOW()))-1) AS `DayOfWeek` FROM `sys_user` a WHERE a.`Id`=1", sql);
        Assert.AreEqual(2, dbParameters.Count);
        Assert.That(dbParameters[0].Value, Is.TypeOf<DateOnly>());
        Assert.That(dbParameters[1].Value, Is.TypeOf<DateOnly>());
        Assert.AreEqual(localDate, (DateOnly)dbParameters[0].Value);
        Assert.AreEqual(localDate, (DateOnly)dbParameters[1].Value);

        var now = DateTime.Now;
        var result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.UpdatedAt,
                DateTime.Today,
                Now = now,
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
        Assert.AreEqual(DateOnly.MinValue, result.MinValue);
        Assert.AreEqual(DateOnly.MaxValue, result.MaxValue);
        Assert.AreEqual(DateTime.Now.Date, result.Today);
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.Now), result.Today1);
        Assert.AreEqual(localDate, result.localDate);
        Assert.AreEqual(result.UpdatedAt.Equals(DateTime.Parse("2023-03-25")), result.IsEquals);
        Assert.AreEqual(result.UpdatedAt.Equals(localDate), result.IsEquals1);
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.Now).DayNumber, result.DayNumber);
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.Now).Day, result.Day);
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.Now).Month, result.Month);
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.Now).Year, result.Year);
        Assert.AreEqual(result.DayOfWeek, DateOnly.FromDateTime(DateTime.Now).DayOfWeek);
    }
    [Test]
    public async Task AddCompareTo()
    {
        this.Initialize(1);
        var localDate = DateOnly.FromDateTime(DateTime.Parse("2023-05-06"));
        var repository = this.dbFactory.Create();
        var sql = repository.From<UpdateEntity1>()
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
        Assert.AreEqual("SELECT DATE_ADD(a.`DateOnlyField`,INTERVAL 30 DAY) AS `AddDays`,DATE_ADD(a.`DateOnlyField`,INTERVAL 5 MONTH) AS `AddMonths`,DATE_ADD(a.`DateOnlyField`,INTERVAL 2 YEAR) AS `AddYears`,(CASE WHEN a.`DateOnlyField`=@p0 THEN 0 WHEN a.`DateOnlyField`>@p0 THEN 1 ELSE -1 END) AS `CompareTo`,@p1 AS `Parse`,'2023-05-07' AS `ParseExact` FROM `sys_update_entity` a WHERE a.`Id`=1", sql);

        var now = DateTime.Now;
        var result = await repository.From<UpdateEntity1>()
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
        Assert.AreEqual(result.DateOnlyField.AddDays(30), result.AddDays);
        Assert.AreEqual(result.DateOnlyField.AddMonths(5), result.AddMonths);
        Assert.AreEqual(result.DateOnlyField.AddYears(2), result.AddYears);
        Assert.AreEqual(result.DateOnlyField.CompareTo(localDate), result.CompareTo);
        Assert.AreEqual(DateOnly.Parse(localDate.ToString("yyyy-MM-dd")), result.Parse);
        Assert.AreEqual(DateOnly.ParseExact("05-07/2023", "MM-dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None), result.ParseExact);
    }
#endif



    [Test]
    public async Task MemberAccess1()
    {
        this.Initialize(1);
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
        Assert.AreEqual("SELECT NOW() AS `Now`,'0001-01-01 00:00:00.000' AS `MinValue`,'9999-12-31 23:59:59.999' AS `MaxValue`,UTC_TIMESTAMP() AS `UtcNow`,CURDATE() AS `Today`,@p0 AS `UnixEpoch`,'2023-05-06 00:00:00.000' AS `Date`,CONVERT(NOW(),DATE) AS `CurrentDate`,@p1 AS `localDate`,(a.`UpdatedAt`='2023-03-25 00:00:00.000') AS `IsEquals`,(a.`UpdatedAt`=@p2) AS `IsEquals1` FROM `sys_user` a WHERE a.`Id`=1", sql);
        Assert.AreEqual(3, dbParameters.Count);
        Assert.That(dbParameters[0].Value, Is.TypeOf<DateTime>());
        Assert.That(dbParameters[1].Value, Is.TypeOf<DateTime>());
        Assert.That(dbParameters[2].Value, Is.TypeOf<DateTime>());
        Assert.AreEqual(this.UnixEpoch, (DateTime)dbParameters[0].Value);
        Assert.AreEqual(localDate, (DateTime)dbParameters[1].Value);
        Assert.AreEqual(localDate, (DateTime)dbParameters[2].Value);

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
                this.UnixEpoch,
                DateTime.Parse("2023-05-06").Date,
                CurrentDate = DateTime.Now.Date,
                localDate,
                IsEquals = f.UpdatedAt.Equals(DateTime.Parse("2023-03-25")),
                IsEquals1 = f.UpdatedAt.Equals(localDate)
            })
            .FirstAsync();
        Assert.AreEqual(DateTime.MinValue, result.MinValue);
        //由于精度不同，差一些微秒
        //Assert.IsTrue(result.MaxValue == DateTime.MaxValue);
        //取决于时区的设置
        Assert.AreEqual(DateTime.Now.Date, result.Today);
        Assert.AreEqual(this.UnixEpoch, result.UnixEpoch);
        Assert.AreEqual(DateTime.Parse("2023-05-06").Date, result.Date);
        Assert.AreEqual(localDate, result.localDate);
        Assert.AreEqual(result.UpdatedAt.Equals(DateTime.Parse("2023-03-25")), result.IsEquals);
    }
    [Test]
    public async Task AddSubtract()
    {
        this.Initialize(1);
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
        Assert.AreEqual("SELECT DATE_ADD(a.`CreatedAt`,INTERVAL 365 DAY) AS `Add`,DATE_ADD(a.`CreatedAt`,INTERVAL 30 DAY) AS `AddDays`,DATE_ADD(a.`CreatedAt`,INTERVAL 300*1000 MICROSECOND) AS `AddMilliseconds`,DATE_SUB(a.`CreatedAt`,INTERVAL 365 DAY) AS `Subtract1`,DATE_SUB(NOW(),INTERVAL 365 DAY) AS `Subtract2`,TIMEDIFF(a.`UpdatedAt`,a.`CreatedAt`) AS `Subtract3`,DAYOFMONTH(LAST_DAY(CONCAT(YEAR(NOW()),'-',MONTH(NOW()),'-01'))) AS `DayInMonth`,(YEAR(NOW())%4=0 AND YEAR(NOW())%100<>0 OR YEAR(NOW())%400=0) AS `IsLeapYear1`,1 AS `IsLeapYear2`,CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) AS `Parse`,'2023-05-07 13:08:45.000' AS `ParseExact` FROM `sys_user` a WHERE a.`UpdatedAt`>SUBTIME(DATE_SUB(NOW(),INTERVAL 365 DAY),'00:25:00.000000')", sql);

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
        Assert.AreEqual(result.CreatedAt.Add(TimeSpan.FromDays(365)), result.Add);
        Assert.AreEqual(result.CreatedAt.AddDays(30), result.AddDays);
        Assert.AreEqual(result.CreatedAt.AddMilliseconds(300), result.AddMilliseconds);
        Assert.AreEqual(result.CreatedAt.Subtract(TimeSpan.FromDays(365)), result.Subtract1);
        Assert.AreEqual(result.Now - TimeSpan.FromDays(365), result.Subtract2);
        Assert.AreEqual(result.UpdatedAt - result.CreatedAt, result.Subtract3);
        Assert.AreEqual(DateTime.DaysInMonth(now.Year, now.Month), result.DayInMonth);
        Assert.AreEqual(DateTime.IsLeapYear(now.Year), result.IsLeapYear1);
        Assert.AreEqual(DateTime.IsLeapYear(2020), result.IsLeapYear2);
        Assert.AreEqual(DateTime.Parse(now.ToString("yyyy-MM-dd HH:mm:ss")), result.Parse);
        Assert.AreEqual(DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture), result.ParseExact);
    }
    [Test]
    public async Task Compare()
    {
        this.Initialize(1);
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
        Assert.AreEqual("SELECT (CASE WHEN a.`CreatedAt`='2023-03-03 00:00:00.000' THEN 0 WHEN a.`CreatedAt`>'2023-03-03 00:00:00.000' THEN 1 ELSE -1 END) AS `CompareTo`,DATE_SUB(a.`CreatedAt`,INTERVAL 365 DAY) AS `OneYearsAgo1`,TIMEDIFF(NOW(),'2023-03-20 00:00:00.000') AS `OneYearsAgo2`,DAYOFMONTH(LAST_DAY(CONCAT(YEAR(NOW()),'-',MONTH(NOW()),'-01'))) AS `DayInMonth`,(YEAR(NOW())%4=0 AND YEAR(NOW())%100<>0 OR YEAR(NOW())%400=0) AS `IsLeapYear1`,1 AS `IsLeapYear2`,CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) AS `Parse`,'2023-05-07 13:08:45.000' AS `ParseExact` FROM `sys_user` a WHERE (CASE WHEN a.`UpdatedAt`='2023-03-20 00:00:00.000' THEN 0 WHEN a.`UpdatedAt`>'2023-03-20 00:00:00.000' THEN 1 ELSE -1 END)>0", sql);

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
        Assert.AreEqual(result.Compare, DateTime.Compare(result.UpdatedAt, DateTime.Parse("2023-03-20")));
        Assert.AreEqual(result.CompareTo, result.CreatedAt.CompareTo(DateTime.Parse("2023-03-03")));
        Assert.AreEqual(result.OneYearsAgo1, result.CreatedAt.Subtract(TimeSpan.FromDays(365)));
        Assert.AreEqual(result.OneYearsAgo2, result.CreatedAt - DateTime.Parse("2023-03-20"));
        Assert.AreEqual(result.Subtract, result.CreatedAt.Subtract(DateTime.Parse("2023-03-01")));
        Assert.AreEqual(result.DayInMonth, DateTime.DaysInMonth(now.Year, now.Month));
        Assert.AreEqual(result.IsLeapYear1, DateTime.IsLeapYear(now.Year));
        Assert.AreEqual(result.IsLeapYear2, DateTime.IsLeapYear(2020));
        Assert.AreEqual(result.Parse, DateTime.Parse(now.ToString("yyyy-MM-dd HH:mm:ss")));
        Assert.AreEqual(result.ParseExact, DateTime.ParseExact("05-07/2023 13-08-45", "MM-dd/yyyy HH-mm-ss", CultureInfo.InvariantCulture));
    }
    [Test]
    public async Task Operation()
    {
        this.Initialize(1);
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
        Assert.AreEqual("SELECT '05:06:07.000000' AS `DateSub`,ADDTIME(a.`CreatedAt`,'05:00:00.000000') AS `AddOp`,SUBTIME(a.`CreatedAt`,'10:00:00.000000') AS `SubOp`,ADDTIME(a.`SomeTimes`,'00:25:00.000000') AS `AddOp1`,'1.05:45:00.000000' AS `SubOp1`,TIMEDIFF(a.`UpdatedAt`,a.`CreatedAt`) AS `SubOp2`,'01:15:00.000000' AS `MulOp`,'06:00:00.000000' AS `DivOp1`,10 AS `DivOp2` FROM `sys_user` a WHERE (CASE WHEN a.`UpdatedAt`='2023-03-20 00:00:00.000' THEN 0 WHEN a.`UpdatedAt`>'2023-03-20 00:00:00.000' THEN 1 ELSE -1 END)>0", sql);
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
        Assert.AreEqual(result.DateSub, DateTime.Parse("2022-01-01 05:06:07") - DateTime.Parse("2022-01-01"));
        Assert.AreEqual(result.AddOp, result.CreatedAt + TimeSpan.FromHours(5));
        Assert.AreEqual(result.SubOp, result.CreatedAt - TimeSpan.FromHours(10));
        Assert.AreEqual(result.AddOp1, result.SomeTimes.Value.Add(TimeSpan.FromMinutes(25)));
        Assert.AreEqual(result.SubOp1, TimeSpan.FromHours(30) - TimeSpan.FromMinutes(15));
        Assert.AreEqual(result.SubOp2, result.UpdatedAt - result.CreatedAt);
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        Assert.AreEqual(result.MulOp, TimeSpan.FromMinutes(25) * 3);
        Assert.AreEqual(result.DivOp1, TimeSpan.FromHours(30) / 5);
        Assert.AreEqual(result.DivOp2, TimeSpan.FromHours(30) / TimeSpan.FromHours(3));
#else
        Assert.AreEqual(result.MulOp, TimeSpan.FromMinutes(25 * 3));
        Assert.AreEqual(result.DivOp1, TimeSpan.FromHours(30 / 5));
        Assert.AreEqual(10, result.DivOp2);
#endif
    }



    [Test]
    public void Coalesce()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        string firstName = "kevin", lastName = null;
        var sql = repository.From<User>()
            .Where(f => f.Name.Contains(lastName ?? firstName))
            .Select(f => new { HasName = f.Name ?? "NoName" })
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT COALESCE(a.`Name`,'NoName') AS `HasName` FROM `sys_user` a WHERE a.`Name` LIKE CONCAT('%',@p0,'%')", sql);
        Assert.AreEqual(dbParameters[0].Value.ToString(), firstName);

        repository.BeginTransaction();
        var count = repository.Update<User>(new { Id = 1, Name = "千叶111" });
        var result = repository.From<User>()
            .Where(f => f.Name.Contains(lastName ?? firstName))
            .Select(f => new { f.Id, HasName = f.Name ?? "NoName" })
            .ToList();
        repository.Commit();
        Assert.IsNotEmpty(result);
        //Assert.IsTrue(result.Exists(f => f.Id == 1));

        sql = repository.From<User>()
            .Where(f => (f.Name ?? f.Id.ToString()) == "leafkevin")
            .Select(f => f.Id)
            .ToSql(out dbParameters);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE COALESCE(a.`Name`,CAST(a.`Id` AS CHAR))='leafkevin'", sql);
        repository.BeginTransaction();
        count = repository.Update<User>(new { Id = 1, Name = "leafkevin" });
        var result1 = repository.From<User>()
            .Where(f => (f.Name ?? f.Id.ToString()) == "leafkevin")
            .Select(f => f.Id)
            .ToList();
        repository.Commit();
        Assert.IsNotNull(result);
        //Assert.IsTrue(result.Exists(f => f.Id == 1));
    }
    [Test]
    public void Conditional()
    {
        this.Initialize(1);
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
        Assert.AreEqual("SELECT (CASE WHEN a.`IsEnabled`=1 THEN 'Enabled' ELSE 'Disabled' END) AS `IsEnabled`,(CASE WHEN a.`GuidField` IS NOT NULL THEN 'HasValue' ELSE 'NoValue' END) AS `GuidField`,(CASE WHEN a.`Age`>35 THEN 1 ELSE 0 END) AS `IsOld`,(CASE WHEN INSTR('kevin',a.`Name`)>0 THEN 'Yes' ELSE 'No' END) AS `IsNeedParameter` FROM `sys_user` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN 'Enabled' ELSE 'Disabled' END)='Enabled' AND (CASE WHEN a.`GuidField` IS NOT NULL THEN 'HasValue' ELSE 'NoValue' END)='HasValue'", sql);

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
        Assert.AreEqual("SELECT (CASE WHEN a.`IsEnabled`=1 THEN @p4 ELSE 'Disabled' END) AS `IsEnabled`,(CASE WHEN a.`GuidField` IS NOT NULL THEN @p5 ELSE 'NoValue' END) AS `GuidField`,(CASE WHEN a.`Age`>35 THEN 1 ELSE 0 END) AS `IsOld`,(CASE WHEN INSTR('kevin',a.`Name`)>0 THEN 'Yes' ELSE 'No' END) AS `IsNeedParameter` FROM `sys_user` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN @p0 ELSE 'Disabled' END)=@p1 AND (CASE WHEN a.`GuidField` IS NOT NULL THEN @p2 ELSE 'NoValue' END)=@p3", sql);
        Assert.AreEqual(6, dbParameters.Count);
        Assert.AreEqual(enabled, dbParameters[0].Value.ToString());
        Assert.AreEqual(enabled, dbParameters[1].Value.ToString());
        Assert.AreEqual(hasValue, dbParameters[2].Value.ToString());
        Assert.AreEqual(hasValue, dbParameters[3].Value.ToString());
        Assert.AreEqual(enabled, dbParameters[4].Value.ToString());
        Assert.AreEqual(hasValue, dbParameters[5].Value.ToString());

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
        Assert.IsNotNull(result);
    }
    [Test]
    public async Task WhereCoalesceConditional()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')='Internet'", sql1);
        var result1 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.GreaterOrEqual(result1.Count, 2);
        Assert.AreEqual(CompanyNature.Internet, (result1[0].Nature ?? CompanyNature.Internet));

        var localNature = CompanyNature.Internet;
        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')=@p0", sql2);
        Assert.AreEqual(localNature.ToString(), (string)dbParameters[0].Value);
        Assert.That(dbParameters[0].Value, Is.TypeOf<string>());
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.GreaterOrEqual(result2.Count, 2);
        Assert.AreEqual(localNature, (result2[0].Nature ?? CompanyNature.Internet));

        var sql3 = repository.From<Company>()
            .Where(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature)
            .ToSql(out dbParameters);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN a.`Nature` ELSE 'Internet' END)=@p0", sql3);
        Assert.AreEqual(localNature.ToString(), (string)dbParameters[0].Value);
        Assert.That(dbParameters[0].Value, Is.TypeOf<string>());
        var result3 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.GreaterOrEqual(result3.Count, 2);
        Assert.AreEqual(localNature, (result3[0].Nature ?? CompanyNature.Internet));
    }
    [Test]
    public void Index()
    {
        this.Initialize(1);
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
        Assert.AreEqual("SELECT @p2 AS `False`,@p3 AS `Unknown`,CONCAT(@p4,' and ',@p5) AS `MyLove` FROM `sys_user` a WHERE a.`Name` LIKE CONCAT('%',@p0,'%') OR CAST(a.`IsEnabled` AS CHAR)=@p1", sql);
        Assert.AreEqual(6, dbParameters.Count);
        Assert.AreEqual(dict["1"], (string)dbParameters[0].Value);
        Assert.AreEqual(strCollection[0], (string)dbParameters[1].Value);
        Assert.AreEqual(strArray[2], (string)dbParameters[2].Value);
        Assert.AreEqual(strCollection[2], (string)dbParameters[3].Value);
        Assert.AreEqual(dict["2"], (string)dbParameters[4].Value);
        Assert.AreEqual(dict["3"], (string)dbParameters[5].Value);

        var result = repository.From<User>()
            .Where(f => (f.Name.Contains(dict["1"]) || f.IsEnabled.ToString() == strCollection[0]))
            .Select(f => new
            {
                False = strArray[2],
                Unknown = strCollection[2],
                MyLove = dict["2"] + " and " + dict["3"]
            })
            .ToList();
        Assert.IsNotNull(result);
    }



    [Test]
    public async Task Contains()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => new int[] { 1, 2 }.Contains(f.Id))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (1,2)", sql);
        var result = repository.From<User>()
            .Where(f => new int[] { 1, 2 }.Contains(f.Id))
            .ToList();
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);

        sql = repository.From<User>()
            .Where(f => f.Name.Contains("kevin"))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Name` LIKE '%kevin%'", sql);
        result = await repository.From<User>()
            .Where(f => f.Name.Contains("kevin"))
            .ToListAsync();
        Assert.IsNotNull(result);
        Assert.GreaterOrEqual(result.Count, 1);

        sql = repository.From<User>()
            .Where(f => new List<string> { "kevin", "cindy" }.Contains(f.Name))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Name` IN ('kevin','cindy')", sql);
        result = await repository.From<User>()
            .Where(f => new List<string> { "kevin", "cindy" }.Contains(f.Name))
            .ToListAsync();
        Assert.IsNotEmpty(result);

        var ids = new int[] { 1, 2 };
        sql = repository.From<User>()
            .Where(f => ids.Contains(f.Id))
            .Select(f => f.Id)
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (@p0,@p1)", sql);

        result = repository.From<User>()
            .Where(f => ids.Contains(f.Id))
            .ToList();
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);

        var names = new List<string> { "kevin", "cindy" };
        sql = repository.From<User>()
            .Where(f => names.Contains(f.Name))
            .Select(f => f.Id)
            .ToSql(out dbParameters);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Name` IN (@p0,@p1)", sql);
        result = await repository.From<User>()
            .Where(f => names.Contains(f.Name))
            .ToListAsync();
        Assert.IsNotEmpty(result);

        sql = repository.From<Company>()
            .Where(f => f.Name.Contains("微软"))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_company` a WHERE a.`Name` LIKE '%微软%'", sql);
        var result1 = await repository.From<Company>()
            .Where(f => f.Name.Contains("微软"))
            .ToListAsync();
        Assert.IsNotNull(result1);
        Assert.IsNotEmpty(result);
    }
    [Test]
    public async Task Concat()
    {
        var repository = this.dbFactory.Create();
        bool isMale = false;
        int count = 10;
        var sql = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => string.Concat(f.Name + "_1_" + isMale, f.Age + 5, isMale) + "_2_" + f.Age + "_3_" + isMale + "_4_" + count)
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT CONCAT(a.`Name`,'_1_',@p0,CAST(a.`Age`+5 AS CHAR),@p1,'_2_',CAST(a.`Age` AS CHAR),'_3_',@p2,'_4_',@p3) FROM `sys_user` a WHERE a.`Id`=1", sql);
        Assert.AreEqual((string)dbParameters[0].Value, isMale.ToString());
        Assert.That(dbParameters[0].Value, Is.TypeOf<string>());
        Assert.AreEqual((string)dbParameters[1].Value, isMale.ToString());
        Assert.That(dbParameters[1].Value, Is.TypeOf<string>());
        Assert.AreEqual((string)dbParameters[2].Value, isMale.ToString());
        Assert.That(dbParameters[2].Value, Is.TypeOf<string>());
        Assert.AreEqual((string)dbParameters[3].Value, count.ToString());
        Assert.That(dbParameters[3].Value, Is.TypeOf<string>());

        var result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => string.Concat(f.Name + "_1_" + isMale, f.Age + 5, isMale) + "_2_" + f.Age + "_3_" + isMale + "_4_" + count)
            .FirstAsync();
        Assert.IsNotNull(result);
        Assert.AreEqual("leafkevin_1_False30False_2_25_3_False_4_10", result);
    }
    [Test]
    public async Task Format()
    {
        var repository = this.dbFactory.Create();
        bool isMale = false;
        int count = 5;
        var sql = repository.From<User>()
            .Where(f => f.Name.Contains("cindy"))
            .Select(f => $"{f.Name + "222"}_111_{f.Age + isMale.ToString()}_{isMale}_{count}")
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT CONCAT(a.`Name`,'222_111_',CAST(a.`Age` AS CHAR),@p0,'_',@p1,'_',@p2) FROM `sys_user` a WHERE a.`Name` LIKE '%cindy%'", sql);
        Assert.AreEqual((string)dbParameters[0].Value, isMale.ToString());
        Assert.That(dbParameters[0].Value, Is.TypeOf<string>());
        Assert.AreEqual((string)dbParameters[1].Value, isMale.ToString());
        Assert.That(dbParameters[1].Value, Is.TypeOf<string>());
        Assert.AreEqual((string)dbParameters[2].Value, count.ToString());
        Assert.That(dbParameters[2].Value, Is.TypeOf<string>());
        var result = await repository.From<User>()
            .Where(f => f.Name.Contains("cindy"))
            .Select(f => $"{f.Name + "222"}_111_{f.Age + isMale.ToString()}_{isMale}_{count}")
            .FirstAsync();
        Assert.AreEqual("cindy222_111_21False_False_5", result);
    }
    [Test]
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
        Assert.AreEqual("SELECT (CASE WHEN a.`Name`='leafkevin' THEN 0 WHEN a.`Name`>'leafkevin' THEN 1 ELSE -1 END) AS `NameCompare`,(CASE WHEN a.`CreatedAt`=CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) THEN 0 WHEN a.`CreatedAt`>CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) THEN 1 ELSE -1 END) AS `CreatedAtCompare`,(CASE WHEN a.`CreatedAt`=NOW() THEN 0 WHEN a.`CreatedAt`>NOW() THEN 1 ELSE -1 END) AS `CreatedAtCompare1`,(CASE WHEN a.`UpdatedAt`=SUBTIME(DATE_SUB(a.`UpdatedAt`,INTERVAL 1 DAY),'09:25:00.000000') THEN 0 WHEN a.`UpdatedAt`>SUBTIME(DATE_SUB(a.`UpdatedAt`,INTERVAL 1 DAY),'09:25:00.000000') THEN 1 ELSE -1 END) AS `UpdatedAtCompare` FROM `sys_user` a WHERE a.`Id`=1", sql1);

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
        Assert.IsNotNull(result1);
        Assert.AreEqual(0, result1.NameCompare);
        Assert.AreEqual(-1, result1.CreatedAtCompare);
        Assert.AreEqual(-1, result1.CreatedAtCompare1);
        Assert.AreEqual(1, result1.UpdatedAtCompare);

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
        Assert.AreEqual("SELECT (CASE WHEN a.`Name`='leafkevin' THEN 0 WHEN a.`Name`>'leafkevin' THEN 1 ELSE -1 END) AS `NameCompare`,(CASE WHEN a.`CreatedAt`=CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) THEN 0 WHEN a.`CreatedAt`>CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) THEN 1 ELSE -1 END) AS `CreatedAtCompare`,(CASE WHEN a.`CreatedAt`=NOW() THEN 0 WHEN a.`CreatedAt`>NOW() THEN 1 ELSE -1 END) AS `CreatedAtCompare1`,(CASE WHEN a.`UpdatedAt`=SUBTIME(a.`UpdatedAt`,'00:15:00.000000') THEN 0 WHEN a.`UpdatedAt`>SUBTIME(a.`UpdatedAt`,'00:15:00.000000') THEN 1 ELSE -1 END) AS `UpdatedAtCompare` FROM `sys_user` a WHERE a.`Id`=1", sql2);

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
        Assert.IsNotNull(result2);
        Assert.AreEqual(0, result2.NameCompare);
        Assert.AreEqual(-1, result2.CreatedAtCompare);
        Assert.AreEqual(-1, result2.CreatedAtCompare1);
        Assert.AreEqual(1, result2.UpdatedAtCompare);
    }
    [Test]
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
        Assert.AreEqual("SELECT (CASE WHEN a.`Id`='1' THEN 0 WHEN a.`Id`>'1' THEN 1 ELSE -1 END) AS `IntCompare`,(CASE WHEN a.`OrderNo`='OrderNo-001' THEN 0 WHEN a.`OrderNo`>'OrderNo-001' THEN 1 ELSE -1 END) AS `StringCompare`,(CASE WHEN a.`CreatedAt`='2022-12-20 00:00:00.000' THEN 0 WHEN a.`CreatedAt`>'2022-12-20 00:00:00.000' THEN 1 ELSE -1 END) AS `DateTimeCompare`,(CASE WHEN a.`IsEnabled`=0 THEN 0 WHEN a.`IsEnabled`>0 THEN 1 ELSE -1 END) AS `BooleanCompare` FROM `sys_order` a", sql);

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
        Assert.IsNotNull(result);
        Assert.AreEqual(result.IntCompare, result.Id.CompareTo("1"));
        Assert.AreEqual(result.StringCompare, result.OrderNo.CompareTo("OrderNo-001"));
        Assert.AreEqual(result.DateTimeCompare, result.CreatedAt.CompareTo(DateTime.Parse("2022-12-20")));
        Assert.AreEqual(result.BooleanCompare, result.IsEnabled.CompareTo(false));
    }
    [Test]
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
        Assert.AreEqual("SELECT CONCAT('Begin_',TRIM(a.`OrderNo`),'123_End') AS `Trim`,CONCAT('Begin_',LTRIM(a.`OrderNo`),'123   _End') AS `TrimStart`,CONCAT('Begin_',RTRIM(a.`OrderNo`),'  123_End') AS `TrimEnd` FROM `sys_order` a", sql);

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
        Assert.AreEqual("SELECT CONCAT(@p0,TRIM(a.`OrderNo`),@p1,@p2) AS `Trim`,CONCAT('Begin_',LTRIM(a.`OrderNo`),@p3,'_End') AS `TrimStart`,CONCAT('Begin_',RTRIM(a.`OrderNo`),@p4,'_End') AS `TrimEnd` FROM `sys_order` a", sql1);
        Assert.AreEqual(strValue1, (string)dbParameters[0].Value);
        Assert.That(dbParameters[0].Value, Is.TypeOf<string>());
        Assert.AreEqual((string)dbParameters[1].Value, strValue2.Trim());
        Assert.That(dbParameters[1].Value, Is.TypeOf<string>());
        Assert.AreEqual(strValue3, (string)dbParameters[2].Value);
        Assert.That(dbParameters[2].Value, Is.TypeOf<string>());
        Assert.AreEqual((string)dbParameters[3].Value, strValue2.TrimStart());
        Assert.That(dbParameters[3].Value, Is.TypeOf<string>());
        Assert.AreEqual((string)dbParameters[4].Value, strValue2.TrimEnd());
        Assert.That(dbParameters[4].Value, Is.TypeOf<string>());

        repository.BeginTransaction();
        repository.DeleteByIds<Order>(new[] { "1", "2", "3" });
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
            Assert.AreEqual("Begin_ON-001123_End", result[0].Trim);
            Assert.AreEqual("Begin_ON-001 123   _End", result[0].TrimStart);
            Assert.AreEqual("Begin_ ON-001  123_End", result[0].TrimEnd);
        }
    }
    [Test]
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
        Assert.AreEqual("SELECT CONCAT(LOWER(a.`OrderNo`),'_ABCD') AS `Col1`,CONCAT(UPPER(a.`OrderNo`),'_abcd') AS `Col2` FROM `sys_order` a", sql);

        repository.BeginTransaction();
        repository.DeleteById<Order>("1");
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
        Assert.AreEqual(1, count);
        Assert.IsNotEmpty(result);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("on-zwyx_ABCD", result[0].Col1);
            Assert.AreEqual("ON-ZWYX_abcd", result[0].Col2);
        });
    }
    [Test]
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
        Assert.AreEqual("SELECT CONCAT(LOWER(a.`OrderNo`),'_ABCD') AS `Col1`,CONCAT(UPPER(a.`OrderNo`),'_abcd') AS `Col2` FROM `sys_order` a", sql);

        var strValue = "_AbCd";
        var sql1 = repository.From<Order>()
           .Select(f => new
           {
               Col1 = f.OrderNo.ToLower() + strValue.ToUpper(),
               Col2 = f.OrderNo.ToUpper() + strValue.ToLower()
           })
           .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT CONCAT(LOWER(a.`OrderNo`),@p0) AS `Col1`,CONCAT(UPPER(a.`OrderNo`),@p1) AS `Col2` FROM `sys_order` a", sql1);
        Assert.AreEqual((string)dbParameters[0].Value, strValue.ToUpper());
        Assert.That(dbParameters[0].Value, Is.TypeOf<string>());
        Assert.AreEqual((string)dbParameters[1].Value, strValue.ToLower());
        Assert.That(dbParameters[1].Value, Is.TypeOf<string>());

        repository.BeginTransaction();
        repository.DeleteById<Order>("1");
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
        var result = repository.UseMaster()
            .From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "1", "2", "3" }))
            .OrderBy(f => f.Id)
            .Select(f => new
            {
                Col1 = f.OrderNo.ToLower() + "_AbCd".ToUpper(),
                Col2 = f.OrderNo.ToUpper() + "_AbCd".ToLower()
            })
            .ToList();
        Assert.AreEqual("on-zwyx_ABCD", result[0].Col1);
        Assert.AreEqual("ON-ZWYX_abcd", result[0].Col2);
    }
    [Test]
    public void Update_Contains()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        int id = 1;
        var orderNos = new string[] { "ON_001", "ON_002", "ON_003" };
        var sql = repository.Update<Order>()
            .Set(f => new { TotalAmount = 100 })
            .Where(f => f.BuyerId == id || orderNos.Contains(f.OrderNo))
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` SET `TotalAmount`=@p0 WHERE `BuyerId`=@p1 OR `OrderNo` IN (@p2,@p3,@p4)", sql);
        var count = repository.Update<Order>()
            .Set(f => new { TotalAmount = 100 })
            .Where(f => f.BuyerId == id || orderNos.Contains(f.OrderNo))
            .Execute();
        Assert.Greater(count, 0);
    }
    [Test]
    public void Method_Convert1()
    {
        this.Initialize(1);
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
        Assert.AreEqual("SELECT CONCAT('Age-',@p0) AS `StringAge`,CONCAT('Id-',CAST(a.`Id` AS CHAR)) AS `StringId1`,((CAST(a.`Age` AS DOUBLE)*2)-10) AS `DoubleAge`,a.`Gender` AS `Gender1`,a.`Gender` AS `Gender2`,CAST(a.`Age` AS CHAR) AS `Age` FROM `sys_user` a WHERE a.`Id`=1", sql);

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
        Assert.IsNotEmpty(result);

        var sql1 = repository.From<UpdateEntity1>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.EnumField,
                EnumField1 = f.EnumField.ToString(),
                EnumField2 = Convert.ToString(f.EnumField)
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`EnumField`,(CASE a.`EnumField` WHEN 0 THEN 'Unknown' WHEN 1 THEN 'Female' WHEN 2 THEN 'Male' END) AS `EnumField1`,(CASE a.`EnumField` WHEN 0 THEN 'Unknown' WHEN 1 THEN 'Female' WHEN 2 THEN 'Male' END) AS `EnumField2` FROM `sys_update_entity` a WHERE a.`Id`=1", sql1);
        var result1 = repository.From<UpdateEntity1>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.EnumField,
                EnumField1 = f.EnumField.ToString(),
                EnumField2 = Convert.ToString(f.EnumField)
            })
            .First();
        Assert.IsNotNull(result1);
        Assert.AreEqual(result1.EnumField.ToString(), result1.EnumField1);
        Assert.AreEqual(Convert.ToString(result1.EnumField), result1.EnumField2);
    }
    [Test]
    public async Task Method_Convert2()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        byte id = 1;
        await repository.From<User>()
            .Where(f => f.Id == id)
            .Select(f => (short)f.Age)
            .FirstAsync();
    }
    [Test]
    public void SqlIn()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, new int[] { 1, 2, 3 }))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (1,2,3)", sql);

        sql = repository.From<User>()
            .Where(f => Sql.In(f.CreatedAt, new DateTime[] { DateTime.Parse("2023-03-03"), DateTime.Parse("2023-03-03 00:00:00"), DateTime.Parse("2023-03-03 06:06:06") }))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`CreatedAt` IN ('2023-03-03 00:00:00.000','2023-03-03 00:00:00.000','2023-03-03 06:06:06.000')", sql);
    }
    [Test]
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
        Assert.AreEqual("SELECT CONCAT(CAST(IFNULL(a.`Age`,20) AS CHAR),'-',a.`Gender`) AS `NewField` FROM `sys_user` a WHERE a.`Id`=1", sql);

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
        Assert.AreEqual($"{age}-{result.Gender}", result.NewField);

        sql = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NewField = $"{f.Age.IsNull(20)}-{f.Gender}"
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT CONCAT(CAST(IFNULL(a.`Age`,20) AS CHAR),'-',a.`Gender`) AS `NewField` FROM `sys_user` a WHERE a.`Id`=1", sql);

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
        Assert.AreEqual(result.NewField, $"{age}-{result.Gender.ToString()}");

        sql = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NewField = $"{f.Age.IsNull(20)}-{f.Gender.ToDescription()}"
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Age`,a.`Gender` FROM `sys_user` a WHERE a.`Id`=1", sql);

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
        Assert.AreEqual(result1.NewField, $"{age}-{result1.Gender.ToDescription()}");

        sql = repository.From<UpdateEntity1>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NewField = $"{f.EnumField}"
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT CONCAT(CASE a.`EnumField` WHEN 0 THEN 'Unknown' WHEN 1 THEN 'Female' WHEN 2 THEN 'Male' END) AS `NewField` FROM `sys_update_entity` a WHERE a.`Id`=1", sql);

        var result2 = await repository.From<UpdateEntity1>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.EnumField,
                NewField = $"{f.EnumField}"
            })
            .FirstAsync();
        Assert.AreEqual(result2.NewField, $"{result2.EnumField}");
    }
    [Test]
    public void ContainsEquals()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => f.Name == string.Concat("千", "11"))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Name`='千11'", sql);
        sql = repository.From<User>()
            .Where(f => f.Name.Equals("千11"))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Name`='千11'", sql);
    }




    [Test]
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
        Assert.AreEqual(1, count);
    }
    [Test]
    public async Task Insert_RawSql()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<Brand>().WhereById(new { Id = 1 }).Execute();
        var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES (@Id,@BrandNo,@Name,1,NOW(),@User,NOW(),@User)";
        var count = await repository.ExecuteAsync(rawSql, new
        {
            Id = 1,
            BrandNo = "BN-001",
            Name = "波司登",
            User = 1
        });
        repository.Commit();
        Assert.AreEqual(1, count);
    }
    [Test]
    public async Task Insert_Parameters()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        await repository.Delete<Product>().WhereByIds(new int[] { 1, 2, 3 }).ExecuteAsync();
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
        Assert.AreEqual(3, count);
    }
    [Test]
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
        Assert.AreEqual("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.AreEqual(1, (int)dbParameters[0].Value);
        Assert.AreEqual("1", (string)dbParameters[1].Value);
        Assert.AreEqual("leafkevin", (string)dbParameters[2].Value);
        if (dbParameters[3] is MySqlParameter dbParameter)
        {
            Assert.AreEqual(MySqlDbType.Enum, dbParameter.MySqlDbType);
            Assert.AreEqual(Gender.Male.ToString(), (string)dbParameter.Value);
        }
        Assert.AreEqual(25, (int)dbParameters[4].Value);
        Assert.AreEqual(1, (int)dbParameters[5].Value);
        Assert.IsTrue((bool)dbParameters[6].Value);
        Assert.AreEqual(now, (DateTime)dbParameters[7].Value);
        Assert.AreEqual(1, (int)dbParameters[8].Value);
        Assert.AreEqual(now, (DateTime)dbParameters[9].Value);
        Assert.AreEqual(1, (int)dbParameters[10].Value);

        sql = repository.Create<User>()
            .WithBy(new Dictionary<string, object>
            {
                { "id", 1 },
                { "tenantId", "1"},
                { "name", "leafkevin"},
                { "age", 25},
                { "companyId", 1},
                { "gender", Gender.Male},
                { "isEnabled", true},
                { "createdAt", now},
                { "createdBy", 1},
                { "updatedAt", now},
                { "updatedBy", 1}
            })
          .ToSql(out dbParameters);
        Assert.AreEqual("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.AreEqual(1, (int)dbParameters[0].Value);
        Assert.AreEqual("1", (string)dbParameters[1].Value);
        Assert.AreEqual("leafkevin", (string)dbParameters[2].Value);
        Assert.AreEqual(25, (int)dbParameters[3].Value);
        Assert.AreEqual(1, (int)dbParameters[4].Value);
        if (dbParameters[5] is MySqlParameter dbParameter1)
        {
            Assert.AreEqual(MySqlDbType.Enum, dbParameter1.MySqlDbType);
            Assert.AreEqual(Gender.Male.ToString(), (string)dbParameter1.Value);
        }
        Assert.IsTrue((bool)dbParameters[6].Value);
        Assert.AreEqual(now, (DateTime)dbParameters[7].Value);
        Assert.AreEqual(1, (int)dbParameters[8].Value);
        Assert.AreEqual(now, (DateTime)dbParameters[9].Value);
        Assert.AreEqual(1, (int)dbParameters[10].Value);

        repository.BeginTransaction();
        var count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
        var result = repository.Create<User>()
            .WithBy(new
            {
                id = 1,
                tenantId = "1",
                name = "leafkevin",
                age = 25,
                companyId = 1,
                gender = Gender.Male,
                isEnabled = true,
                createdAt = now,
                createdBy = 1,
                updatedAt = now,
                updatedBy = 1
            })
            .Execute();
        repository.Commit();
        Assert.AreEqual(1, result);

        repository.BeginTransaction();
        count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
        result = repository.Create<User>()
            .WithBy(new Dictionary<string, object>
            {
                { "id", 1 },
                { "tenantId", "1"},
                { "name", "leafkevin"},
                { "age", 25},
                { "companyId", 1},
                { "gender", Gender.Male},
                { "isEnabled", true},
                { "createdAt", now},
                { "createdBy", 1},
                { "updatedAt", now},
                { "updatedBy", 1}
            })
            .Execute();
        repository.Commit();
        Assert.AreEqual(1, result);
    }
    [Test]
    public async Task Insert_WithBy_Condition()
    {
        this.Initialize(1);
        Guid? guidField = Guid.NewGuid();
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var user = repository.QueryById<User>(1);
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
            .WithBy(guidField.HasValue, f => f.GuidField, guidField)
            .ToSql(out _);
        repository.Commit();
        Assert.AreEqual("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`,`SomeTimes`,`GuidField`) VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@SomeTimes,@GuidField)", sql);

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
        Assert.AreEqual(1, count);
    }
    [Test]
    public async Task Insert_WithBy_AnonymousObject_Condition()
    {
        this.Initialize(1);
        Guid? guidField = Guid.NewGuid();
        var repository = this.dbFactory.Create();
        var user = repository.QueryById<User>(1);
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
            .WithBy(false, f => f.SomeTimes, user.SomeTimes)
            .WithBy(f => f.TenantId, "1")
            .WithBy(guidField.HasValue, "GuidField", guidField)
            .ToSql(out _);
        Assert.AreEqual("INSERT INTO `sys_user` (`Id`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`,`TenantId`,`GuidField`) VALUES (@Id,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@TenantId,@GuidField)", sql);

        repository.BeginTransaction();
        var count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
        count = await repository.Create<User>()
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
            .WithBy(false, "SomeTimes", user.SomeTimes)
            .WithBy(f => f.TenantId, "1")
            .WithBy(guidField.HasValue, "GuidField", guidField)
            .ExecuteAsync();
        repository.Commit();
        Assert.AreEqual(1, count);
    }
    [Test]
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
        Assert.AreEqual(maxId, id);

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
        Assert.AreEqual(maxId, id);
    }
    [Test]
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
        Assert.AreEqual("INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)", sql);

        repository.BeginTransaction();
        await repository.Delete<Product>().WhereByIds(new int[] { 1, 2, 3 }).ExecuteAsync();
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
        Assert.AreEqual(3, count);
    }
    [Test]
    public async Task Insert_WithBulk_Dictionaries()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        await repository.DeleteByIdsAsync<Product>(new[] { new { id = 1 }, new { id = 2 }, new { id = 3 } });
        var count = repository.Create<Product>()
            .WithBulk(new[]
            {
                new Dictionary<string,object>
                {
                    { "id",1 },
                    { "productNo","PN-001"},
                    { "name","波司登羽绒服"},
                    { "brandId",1},
                    { "categoryId",1},
                    { "isEnabled",true},
                    { "createdAt",DateTime.Now},
                    { "createdBy",1},
                    { "updatedAt",DateTime.Now},
                    { "updatedBy",1}
                },
                new Dictionary<string,object>
                {
                    { "id",2},
                    { "productNo","PN-002"},
                    { "name","雪中飞羽绒裤"},
                    { "brandId",2},
                    { "categoryId",2},
                    { "isEnabled",true},
                    { "createdAt",DateTime.Now},
                    { "createdBy",1},
                    { "updatedAt",DateTime.Now},
                    { "updatedBy",1}
                },
                new Dictionary<string,object>
                {
                    { "id",3},
                    { "productNo","PN-003"},
                    { "name","优衣库保暖内衣"},
                    { "brandId",3},
                    { "categoryId",3},
                    { "isEnabled",true},
                    { "createdAt",DateTime.Now},
                    { "createdBy",1},
                    { "updatedAt",DateTime.Now},
                    { "updatedBy",1}
                }
            })
            .Execute();
        repository.Commit();
        Assert.AreEqual(3, count);
    }
    [Test]
    public async Task Insert_Select_From_Table1()
    {
        var repository = this.dbFactory.Create();
        var id = 2;
        var brandId = 1;
        var name = "雪中飞羽绒裤";
        int categoryId = 1;
        var brand = repository.QueryById<Brand>(brandId);
        var sql = repository.From<Brand>()
            .Where(f => f.Id == brandId)
            .Select(f => new
            {
                Id = id,
                ProductNo = "PN_" + id.ToString().PadLeft(3, '0'),
                Name = name,
                Price = 25.85,
                BrandId = f.Id,
                CategoryId = categoryId,
                f.CompanyId,
                IsEnabled = true,
                CreatedBy = 1,
                CreatedAt = DateTime.Now,
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now
            })
            .ToCreate<Product>()
            .IgnoreInto()
            .ToSql(out _);
        Assert.AreEqual("INSERT IGNORE INTO `sys_product` (`Id`,`ProductNo`,`Name`,`Price`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT @p1,CONCAT('PN_',@p2),@p3,25.85,b.`Id`,@p4,b.`CompanyId`,1,1,NOW(),1,NOW() FROM `sys_brand` b WHERE b.`Id`=@p0", sql);

        repository.BeginTransaction();
        repository.DeleteById<Product>(id);
        var count = repository.From<Brand>()
            .Where(f => f.Id == brandId)
            .Select(f => new
            {
                Id = id,
                ProductNo = "PN_" + id.ToString().PadLeft(3, '0'),
                Name = name,
                Price = 25.85,
                BrandId = f.Id,
                CategoryId = categoryId,
                f.CompanyId,
                IsEnabled = true,
                CreatedBy = 1,
                CreatedAt = DateTime.Now,
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now
            })
            .ToCreate<Product>()
            .IgnoreInto()
            .Execute();
        var product = repository.QueryById<Product>(id);
        repository.Commit();
        Assert.Greater(count, 0);
        Assert.IsNotNull(product);
        Assert.AreEqual("PN_" + id.ToString().PadLeft(3, '0'), product.ProductNo);
        Assert.AreEqual(name, product.Name);
        Assert.AreEqual(brandId, product.BrandId);

        count = await repository.From<Brand>()
            .Where(f => f.Id == brandId)
            .Select(f => new Product
            {
                Id = id,
                ProductNo = "PN_" + id.ToString().PadLeft(3, '0'),
                Name = name,
                Price = 25.85,
                BrandId = f.Id,
                CategoryId = categoryId,
                //CompanyId = f.CompanyId,
                IsEnabled = true,
                CreatedBy = 1,
                CreatedAt = DateTime.Now,
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now
            })
            .ToCreate<Product>()
            .IgnoreInto()
            .ExecuteAsync();
        Assert.AreEqual(0, count);
    }
    [Test]
    public async Task Insert_Select_From_Table2()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order, Product>()
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
            .ToCreate<OrderDetail>()
            .IgnoreInto()
            .ToSql(out var parameters);
        Assert.AreEqual("INSERT IGNORE INTO `sys_order_detail` (`Id`,`TenantId`,`OrderId`,`ProductId`,`Price`,`Quantity`,`Amount`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT '7','1',b.`Id`,c.`Id`,c.`Price`,3,(c.`Price`*3),b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt` FROM `sys_order` b,`sys_product` c WHERE b.`Id`='3' AND c.`Id`=1", sql);
        await repository.BeginTransactionAsync();
        repository.DeleteById<OrderDetail>("7");
        var result = await repository.From<Order, Product>()
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
            .ToCreate<OrderDetail>()
            .IgnoreInto()
            .ExecuteAsync();
        var orderDetail = repository.QueryById<OrderDetail>("7");
        var product = repository.QueryById<Product>(1);
        await repository.CommitAsync();
        Assert.Greater(result, 0);
        Assert.IsNotNull(orderDetail);
        Assert.AreEqual("3", orderDetail.OrderId);
        Assert.AreEqual(1, orderDetail.ProductId);
        Assert.AreEqual(product.Price * 3, orderDetail.Amount);
    }
    [Test]
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
        var sql = repository.FromQuery(ordersQuery)
            .ToCreate<Order>()
            .ToSql(out var parameters);
        Assert.AreEqual("INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`ProductCount`,`TotalAmount`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) WITH \r\n`orders`(`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`ProductCount`,`TotalAmount`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) AS \r\n(\r\nSELECT a.`OrderId`,'1',CONCAT('ON-',a.`OrderId`),1,1,'Taobao',2,SUM(a.`Amount`),1,NOW(),1,NOW(),1 FROM `sys_order_detail` a GROUP BY a.`OrderId`\r\n)\r\nSELECT b.`Id`,b.`TenantId`,b.`OrderNo`,b.`BuyerId`,b.`SellerId`,b.`BuyerSource`,b.`ProductCount`,b.`TotalAmount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `orders` b", sql);
        var orderIds = ordersQuery.Select(f => f.Id).ToList();
        await repository.BeginTransactionAsync();
        repository.DeleteByIds<Order>(orderIds);
        var result = await repository.FromQuery(ordersQuery)
            .ToCreate<Order>()
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(orderIds.Count, result);

        sql = repository.From<OrderDetail>()
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
            .ToCreate<Order>()
            .ToSql(out parameters);
        Assert.AreEqual("INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`ProductCount`,`TotalAmount`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) SELECT b.`OrderId`,'1',CONCAT('ON-',b.`OrderId`),1,1,'Taobao',2,SUM(b.`Amount`),1,NOW(),1,NOW(),1 FROM `sys_order_detail` b GROUP BY b.`OrderId`", sql);
        await repository.BeginTransactionAsync();
        repository.DeleteByIds<Order>(orderIds);
        result = await repository.FromQuery(ordersQuery)
            .ToCreate<Order>()
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(orderIds.Count, result);
    }
    [Test]
    public void Insert_Null_Field()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.DeleteById<Order>("1");
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
        var result = repository.QueryById<Order>("1");
        repository.Commit();
        Assert.AreEqual(1, count);
        Assert.IsNotNull(result);
        Assert.IsNull(result.ProductCount);
    }
    [Test]
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
        Assert.AreEqual("INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`ProductCount`,`TotalAmount`,`BuyerId`,`BuyerSource`,`SellerId`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@OrderNo,@ProductCount,@TotalAmount,@BuyerId,@BuyerSource,@SellerId,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.IsTrue(parameters[3].Value is DBNull);
        Assert.AreEqual("@BuyerSource", parameters[6].ParameterName);
        Assert.IsTrue(parameters[6].Value is DBNull);
        Assert.AreEqual("@Products", parameters[8].ParameterName);
        Assert.AreEqual(new JsonTypeHandler().ToFieldValue(new List<int> { 1, 2 }).ToString(), (string)parameters[8].Value);
        Assert.AreEqual("@Disputes", parameters[9].ParameterName);
        Assert.AreEqual(new JsonTypeHandler().ToFieldValue(dispute).ToString(), (string)parameters[9].Value);

        repository.BeginTransaction();
        repository.DeleteById<Order>("4");
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
        var order = repository.QueryById<Order>("4");
        repository.Commit();
        Assert.IsNotNull(order.Products);
        Assert.IsNotEmpty(order.Products);
        Assert.IsNotNull(order.Disputes);
        Assert.AreEqual(new JsonTypeHandler().ToFieldValue(new List<int> { 1, 2 }).ToString(), new JsonTypeHandler().ToFieldValue(order.Products).ToString());
        Assert.AreEqual(new JsonTypeHandler().ToFieldValue(dispute).ToString(), new JsonTypeHandler().ToFieldValue(order.Disputes).ToString());
    }
    [Test]
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
        Assert.AreEqual("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql1);
        Assert.AreEqual("@Gender", parameters1[3].ParameterName);
        Assert.That(parameters1[3].Value, Is.TypeOf<string>());
        Assert.AreEqual(Gender.Male.ToString(), (string)parameters1[3].Value);

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
        Assert.AreEqual("INSERT INTO `sys_company` (`Name`,`Nature`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Name,@Nature,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql2);
        Assert.AreEqual("@Nature", parameters2[1].ParameterName);
        Assert.That(parameters2[1].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters2[1].Value);
    }
    [Test]
    public async Task Insert_Ignore()
    {
        this.Initialize(1);
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
        Assert.AreEqual("INSERT IGNORE INTO `sys_user` (`Id`,`TenantId`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql1);
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
        Assert.AreEqual(0, count);
    }
    [Test]
    public async Task Insert_Ignore_OnlyFields()
    {
        this.Initialize(1);
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
              .ToSql(out var parameters);
        Assert.AreEqual("INSERT IGNORE INTO `sys_user` (`Id`,`TenantId`,`Name`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)", sql);
        Assert.AreEqual(8, parameters.Count);
        repository.BeginTransaction();
        repository.DeleteById<User>(1);
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
        var user = repository.QueryById<User>(1);
        repository.Commit();
        Assert.AreEqual(1, count);
        Assert.AreEqual(0, user.CompanyId);
        Assert.AreEqual(Gender.Unknown, user.Gender);
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
             .ExecuteAsync();
        Assert.AreEqual(0, count);
    }
    [Test]
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
            .OnDuplicateKeyUpdate()
            .Set(new
            {
                TotalAmount = 25,
                Products = new List<int> { 1, 2 }
            })
            .Set(buyerSource.HasValue, f => f.BuyerSource, buyerSource)
            .ToSql(out _);
        Assert.AreEqual("INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`TotalAmount`,`BuyerId`,`SellerId`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@OrderNo,@TotalAmount,@BuyerId,@SellerId,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON DUPLICATE KEY UPDATE `TotalAmount`=@pTotalAmount,`Products`=@pProducts,`BuyerSource`=@BuyerSource", sql1);

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
            .OnDuplicateKeyUpdate()
            .Set(f => new { TotalAmount = f.Values(f.TotalAmount) })
            .Set(f => f.Products, f => f.Values(f.Products))
            .ToSql(out _);
        Assert.AreEqual("INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`TotalAmount`,`BuyerId`,`BuyerSource`,`SellerId`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@OrderNo,@TotalAmount,@BuyerId,@BuyerSource,@SellerId,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON DUPLICATE KEY UPDATE `TotalAmount`=VALUES(`TotalAmount`),`Products`=VALUES(`Products`)", sql2);

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
            .OnDuplicateKeyUpdate()
            .UseAlias()
            .Set(f => new { TotalAmount = f.TotalAmount + f.Values(f.TotalAmount) })
            .Set(f => f.Products, f => f.Values(f.Products))
            .ToSql(out _);
        Assert.AreEqual("INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`TotalAmount`,`BuyerId`,`BuyerSource`,`SellerId`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@OrderNo,@TotalAmount,@BuyerId,@BuyerSource,@SellerId,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=`TotalAmount`+newRow.`TotalAmount`,`Products`=newRow.`Products`", sql3);

        await repository.BeginTransactionAsync();
        await repository.DeleteByIdAsync<Order>("9");
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
            .OnDuplicateKeyUpdate()
            .Set(f => new { TotalAmount = f.Values(f.TotalAmount) })
            .Set(true, f => f.Products, f => f.Values(f.Products))
            .ExecuteAsync();
        var order = await repository.QueryByIdAsync<Order>("9");
        await repository.CommitAsync();
        Assert.AreEqual(1, count);
        Assert.AreEqual(500, order.TotalAmount);
        Assert.IsNull(order.Products);

        await repository.BeginTransactionAsync();
        var oldOrder = await repository.QueryByIdAsync<Order>("9");
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
            .OnDuplicateKeyUpdate()
            .Set(f => new { TotalAmount = f.TotalAmount + f.Values(f.TotalAmount) })
            .Set(true, f => f.Products, f => f.Values(f.Products))
            .ExecuteAsync();
        order = await repository.QueryByIdAsync<Order>("9");
        await repository.CommitAsync();
        Assert.AreEqual(2, count);
        Assert.AreEqual(order.TotalAmount, oldOrder.TotalAmount + 600);
        Assert.AreEqual(new JsonTypeHandler().ToFieldValue(new List<int> { 1, 2 }).ToString(), new JsonTypeHandler().ToFieldValue(order.Products).ToString());
    }
    [Test]
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
        Assert.AreEqual("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) RETURNING `Id`,`TenantId`", sql1);
        await repository.BeginTransactionAsync();
        await repository.DeleteByIdAsync<User>(1);
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
        Assert.AreEqual(1, result1.Id);
        Assert.AreEqual("1", result1.TenantId);

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
        Assert.AreEqual("INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Gender,@Age,@CompanyId,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) RETURNING *", sql2);
        await repository.BeginTransactionAsync();
        await repository.DeleteByIdAsync<User>(2);
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
        Assert.AreEqual(2, result2.Id);
        Assert.AreEqual("1", result2.TenantId);
    }
    [Test]
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
        Assert.AreEqual("INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2) RETURNING `Id`,`ProductNo`", sql1);

        await repository.BeginTransactionAsync();
        await repository.Delete<Product>().WhereByIds(new int[] { 1, 2, 3 }).ExecuteAsync();
        var results1 = await repository.Create<Product>()
            .WithBulk(products)
            .Returning(f => new { f.Id, f.ProductNo })
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(3, results1.Count);
        for (int i = 0; i < results1.Count; i++)
        {
            Assert.AreEqual(products[i].Id, results1[i].Id);
            Assert.AreEqual(products[i].ProductNo, results1[i].ProductNo);
        }

        var sql2 = repository.Create<Product>()
            .WithBulk(products)
            .Returning<Product>("*")
            .ToSql(out var parameters2);
        Assert.AreEqual("INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2) RETURNING *", sql2);

        await repository.BeginTransactionAsync();
        await repository.DeleteByIdsAsync<Product>(new int[] { 1, 2, 3 });
        var result2 = await repository.Create<Product>()
            .WithBulk(products)
            .Returning<Product>("*")
            .ExecuteAsync();
        await repository.CommitAsync();
        for (int i = 0; i < result2.Count; i++)
        {
            Assert.AreEqual(products[i].Id, result2[i].Id);
            Assert.AreEqual(products[i].ProductNo, result2[i].ProductNo);
        }
    }
    [Test]
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
        Assert.AreEqual(orders.Count, count);
    }




    [Test]
    public async Task QueryFirst()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = repository.QueryFirst<User>(f => f.Id == 1);
        if (result != null)
        {
            Assert.IsNotNull(result.Name);
        }
        result = repository.QueryFirst<User>("SELECT * FROM sys_user where Id=1");
        if (result != null)
        {
            Assert.IsNotNull(result.Name);
        }
        var result1 = await repository.QueryFirstAsync<User>(f => f.Name == "leafkevin");
        var result2 = await repository.QueryFirstAsync<User>(new { Name = "leafkevin" });
        if (result1 != null && result2 != null)
        {
            Assert.AreEqual(result2.Id, result1.Id);
            Assert.AreEqual(1, result1.Id);
        }
    }
    [Test]
    public async Task Get()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = repository.QueryById<User>(1);
        Assert.AreEqual("leafkevin", result.Name);
        var user = await repository.QueryByIdAsync<User>(new { Id = 1 });
        Assert.AreEqual(result.Name, user.Name);
    }
    [Test]
    public async Task Query()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = await repository.QueryAsync<Product>(f => f.ProductNo.Contains("PN-00"));
        Assert.GreaterOrEqual(result.Count, 3);
    }
    [Test]
    public async Task QueryPage()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = repository.From<OrderDetail>()
            .Where(f => f.ProductId == 1)
            .OrderByDescending(f => f.CreatedAt)
            .Page(2, 1)
            .ToPageList();
        var count = await repository.From<OrderDetail>().Where(f => f.ProductId == 1).CountAsync();
        Assert.IsNotNull(result);
        Assert.IsNotEmpty(result.Data);
        Assert.AreEqual(count, result.TotalCount);
        Assert.AreEqual(result.Count, result.Data.Count);
        Assert.AreEqual(1, result.Count);
    }
    //[Test]
    //public async Task QueryDictionary()
    //{
    //    this.Initialize(1);
    //    var repository = this.dbFactory.Create();
    //    var result = await repository.QueryDictionaryAsync<Product, int, string>(f => f.ProductNo.Contains("PN-00"), f => f.Id, f => f.Name);
    //    Assert.IsTrue(result.Count >= 3);
    //}
    class OrderBuyerInfo
    {
        public string OrderId { get; set; }
        public string OrderNo { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int ProductTotal { get; set; }
    }
    [Test]
    public async Task QueryRawSql()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = await repository.QueryAsync<Product>("SELECT * FROM sys_product where Id=@ProductId", new { ProductId = 1 });
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
    }
    [Test]
    public void FromQuery_SubQuery()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository
            .FromQuery(f => f.From<OrderDetail>()
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
        Assert.AreEqual("SELECT a.`OrderId`,a.`BuyerId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail` a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1", sql);

        var result = repository
            .FromQuery(f => f.From<OrderDetail>()
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
        Assert.IsNotEmpty(result);
        {
            Assert.IsNotNull(result[0]);
            Assert.IsNotNull(result[0].Group);
            Assert.IsNotNull(result[0].Buyer);
            Assert.Greater(result[0].ProductCount, 1);
        }
        var sql1 = repository
           .FromQuery(f => f.From<Order>()
               .Select(x => new { x.Id, x.OrderNo, x.BuyerId, x.SellerId }))
           .Select(x => new { Order = x })
           .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`OrderNo`,a.`BuyerId`,a.`SellerId` FROM (SELECT a.`Id`,a.`OrderNo`,a.`BuyerId`,a.`SellerId` FROM `sys_order` a) a", sql1);

        var result1 = repository
            .FromQuery(f => f.From<Order>()
                .Select(x => new { x.Id, x.OrderNo, x.BuyerId, x.SellerId }))
            .Select(x => new { Order = x })
            .First();
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result1.Order);
    }
    [Test]
    public void FromQuery_SubQuery1()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.FromQuery(f => f.From<Page, Menu>('o')
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { MenuId = y.Id, y.ParentId, x.Url }))
            .InnerJoin<Menu>((a, b) => a.MenuId == b.Id)
            .Where((a, b) => a.MenuId == b.Id)
            .Select((a, b) => new { a.MenuId, b.Name, a.ParentId, a.Url })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`MenuId`,b.`Name`,a.`ParentId`,a.`Url` FROM (SELECT p.`Id` AS `MenuId`,p.`ParentId`,o.`Url` FROM `sys_page` o,`sys_menu` p WHERE o.`Id`=p.`PageId`) a INNER JOIN `sys_menu` b ON a.`MenuId`=b.`Id` WHERE a.`MenuId`=b.`Id`", sql);

        var result = repository.FromQuery(f => f.From<Page, Menu>('o')
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { MenuId = y.Id, y.ParentId, x.Url }))
            .InnerJoin<Menu>((a, b) => a.MenuId == b.Id)
            .Where((a, b) => a.MenuId == b.Id)
            .Select((a, b) => new { a.MenuId, b.Name, a.ParentId, a.Url })
            .ToList();

        Assert.IsNotEmpty(result);
    }
    [Test]
    public void FromQuery_SubQuery2()
    {
        var repository = this.dbFactory.Create();
        var count = 1;
        var sql = repository
            .FromQuery(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => x.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = a.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .Include((a, b) => b.Details)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT b.`Disputes`,a.`BuyerId`,a.`OrderId`,a.`OrderNo`,a.`ProductTotal`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM (SELECT b.`BuyerId`,b.`Id` AS `OrderId`,b.`OrderNo`,COUNT(DISTINCT c.`ProductId`) AS `ProductTotal` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` LEFT JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` GROUP BY b.`BuyerId`,b.`Id`,b.`OrderNo` HAVING COUNT(DISTINCT c.`ProductId`)>@p0) a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id`", sql);
        Assert.AreEqual(count, (int)dbParameters[0].Value);

        var result = repository
            .FromQuery(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => x.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = a.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .Include((a, b) => b.Details)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .First();
        if (result != null)
        {
            Assert.IsNotNull(result.Order);
            Assert.IsNotNull(result.Order.Details);
            Assert.IsNotEmpty(result.Order.Details);
            Assert.Greater(result.Order.Details[0].Amount, 0);
        }

        var amount = 100;
        sql = repository
            .FromQuery(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => x.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = a.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .IncludeMany((a, b) => b.Details, f => f.Amount > amount)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .ToSql(out dbParameters);
        Assert.AreEqual("SELECT b.`Disputes`,a.`BuyerId`,a.`OrderId`,a.`OrderNo`,a.`ProductTotal`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM (SELECT b.`BuyerId`,b.`Id` AS `OrderId`,b.`OrderNo`,COUNT(DISTINCT c.`ProductId`) AS `ProductTotal` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` LEFT JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` GROUP BY b.`BuyerId`,b.`Id`,b.`OrderNo` HAVING COUNT(DISTINCT c.`ProductId`)>@p0) a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id`", sql);
        Assert.AreEqual(1, dbParameters.Count);
        Assert.AreEqual(count, (int)dbParameters[0].Value);

        result = repository
            .FromQuery(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => x.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = a.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .IncludeMany((a, b) => b.Details, f => f.Amount > amount)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .First();
        if (result != null)
        {
            Assert.IsNotNull(result.Order);
            Assert.IsNotNull(result.Order.Details);
            Assert.IsNotEmpty(result.Order.Details);
            foreach (var orderDetail in result.Order.Details)
            {
                Assert.Greater(result.Order.Details[0].Amount, amount);
            }
        }
    }
    [Test]
    public void FromQuery_SubQuery3()
    {
        var repository = this.dbFactory.Create();
        var sql = repository
            .FromQuery(f => f.From<Order, OrderDetail>('a')
                .Where((a, b) => a.Id == b.OrderId)
                .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                .Having((x, a, b) => x.CountDistinct(b.ProductId) > 0)
                .Select((x, a, b) => new { x.Grouping, ProductTotal = x.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
            .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
            .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`BuyerId`,a.`OrderId`,a.`BuyerId`,a.`ProductTotal`,b.`Name` AS `BuyerName`,a.`BuyerId1` AS `BuyerId2` FROM (SELECT a.`BuyerId`,a.`Id` AS `OrderId`,COUNT(DISTINCT b.`ProductId`) AS `ProductTotal`,a.`BuyerId` AS `BuyerId1` FROM `sys_order` a,`sys_order_detail` b WHERE a.`Id`=b.`OrderId` GROUP BY a.`BuyerId`,a.`Id` HAVING COUNT(DISTINCT b.`ProductId`)>0) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id`", sql);

        var result = repository
            .FromQuery(f => f.From<Order, OrderDetail>('a')
                .Where((a, b) => a.Id == b.OrderId)
                .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                .Having((x, a, b) => x.CountDistinct(b.ProductId) > 0)
                .Select((x, a, b) => new { x.Grouping, ProductTotal = x.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
            .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
            .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
            .First();
        if (result != null)
        {
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Grouping);
            Assert.IsNotNull(result.BuyerName);
        }
    }
    [Test]
    public void FromQuery_SubQuery4()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User, Order, OrderDetail>()
            .InnerJoin((a, b, c) => a.Id == b.BuyerId)
            .LeftJoin((a, b, c) => b.Id == c.OrderId)
            .Select((a, b, c) => new { OrderId = b.Id, b.OrderNo, b.Disputes, b.BuyerId, Buyer = a })
            .ToSql(out _);
        Assert.AreEqual("SELECT b.`Id` AS `OrderId`,b.`OrderNo`,b.`Disputes`,b.`BuyerId`,a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` LEFT JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId`", sql);

        var result = repository.From<User, Order, OrderDetail>()
                 .InnerJoin((a, b, c) => a.Id == b.BuyerId)
                 .LeftJoin((a, b, c) => b.Id == c.OrderId)
            .Select((a, b, c) => new { OrderId = b.Id, b.OrderNo, b.Disputes, b.BuyerId, Buyer = a })
            .First();
        if (result != null)
        {
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.OrderId);
            Assert.Greater(result.BuyerId, 0);
            Assert.IsNotNull(result.OrderNo);
            Assert.IsNotNull(result.Buyer);
        }
    }
    [Test]
    public async Task WithTable_SubQuery()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Menu>()
             .WithQuery(f => f.From<Page, Menu>('c')
                 .Where((a, b) => a.Id == b.PageId)
                 .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
             .Where((a, b) => a.Id == b.Id)
             .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
             .ToSql(out _);
        Assert.AreEqual(@"SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `sys_menu` a,(SELECT d.`Id`,d.`ParentId`,c.`Url` FROM `sys_page` c,`sys_menu` d WHERE c.`Id`=d.`PageId`) b WHERE a.`Id`=b.`Id`", sql);
        var result = repository.From<Menu>()
             .WithQuery(f => f.From<Page, Menu>('c')
                 .Where((a, b) => a.Id == b.PageId)
                 .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
             .Where((a, b) => a.Id == b.Id)
             .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
             .First();
        Assert.IsNotNull(result);

        var sql1 = repository.From<User>()
            .WithQuery(f => f.From<Order>()
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
        Assert.AreEqual("SELECT b.`OrderId`,b.`BuyerId`,a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`ProductCount` FROM `sys_user` a INNER JOIN (SELECT a.`Id` AS `OrderId`,a.`BuyerId`,COUNT(DISTINCT b.`ProductId`) AS `ProductCount` FROM `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` GROUP BY a.`Id`,a.`BuyerId`) b ON a.`Id`=b.`BuyerId` WHERE b.`ProductCount`>1", sql1);

        var result1 = repository.From<User>()
            .WithQuery(f => f.From<Order>()
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
        Assert.IsNotEmpty(result1);

        var sql2 = repository
             .From<Order, User>()
             .WithQuery(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, TotalAmount = x.Sum(b.Amount) }))
            .Where((a, b, c) => a.BuyerId == b.Id && a.Id == c.OrderId)
            .Select((a, b, c) => new { Order = a, Buyer = b, OrderId = a.Id, a.BuyerId, c.TotalAmount })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`Id` AS `OrderId`,a.`BuyerId`,c.`TotalAmount` FROM `sys_order` a,`sys_user` b,(SELECT a.`Id` AS `OrderId`,SUM(b.`Amount`) AS `TotalAmount` FROM `sys_order` a,`sys_order_detail` b,`sys_user` c WHERE a.`Id`=b.`OrderId` AND a.`BuyerId`=c.`Id` AND c.`Age`>20 GROUP BY a.`Id`,a.`BuyerId` HAVING SUM(b.`Amount`)>500) c WHERE a.`BuyerId`=b.`Id` AND a.`Id`=c.`OrderId`", sql2);

        var result2 = await repository
             .From<Order, User>()
             .WithQuery(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, TotalAmount = x.Sum(b.Amount) }))
            .Where((a, b, c) => a.BuyerId == b.Id && a.Id == c.OrderId)
            .Select((a, b, c) => new { Order = a, Buyer = b, OrderId = a.Id, a.BuyerId, c.TotalAmount })
            .ToListAsync();
        Assert.IsNotEmpty(result2);
    }
    [Test]
    public void FromQuery_InnerJoin()
    {
        this.Initialize(1);
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
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE b.`ProductCount`>1", sql);

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
        Assert.IsNotEmpty(result);
        {
            Assert.IsNotNull(result[0]);
            Assert.IsNotNull(result[0].User);
            Assert.IsNotNull(result[0].Order);
            Assert.Greater(result[0].Order.ProductCount, 1);
        }
    }
    [Test]
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
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,c.`ProductCount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail` a GROUP BY a.`OrderId`) c ON b.`Id`=c.`OrderId` WHERE c.`ProductCount`>2", sql);

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
        Assert.IsNotEmpty(result);
        {
            Assert.IsNotNull(result[0]);
            Assert.IsNotNull(result[0].User);
            Assert.IsNotNull(result[0].Order);
            Assert.Greater(result[0].ProductCount, 2);
        }
    }
    [Test]
    public void Join_Cte()
    {
        this.Initialize(1);
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
        Assert.AreEqual(@"WITH `menuPageList`(`MenuId`,`ParentId`,`Url`) AS 
(
SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE a.`Id`=b.`PageId` AND b.`Id`>@MenuId
)
SELECT b.`MenuId`,a.`Name`,b.`ParentId`,a.`PageId`,b.`Url` FROM `sys_menu` a INNER JOIN `menuPageList` b ON a.`Id`=b.`MenuId` AND a.`PageId`>@p1", sql);
        Assert.AreEqual(2, dbParameters.Count);
        Assert.AreEqual("@MenuId", dbParameters[0].ParameterName);
        Assert.AreEqual(menuId, (int)dbParameters[0].Value);
        Assert.AreEqual(pageId, (int)dbParameters[1].Value);

        var result = repository.From<Menu>()
            .InnerJoin(menuPageList, (a, b) => a.Id == b.MenuId && a.PageId > pageId)
            .Select((a, b) => new { b.MenuId, a.Name, b.ParentId, a.PageId, b.Url })
            .ToList();
        Assert.IsNotEmpty(result);
        foreach (var item in result)
        {
            Assert.Greater(item.MenuId, menuId);
            Assert.Greater(item.PageId, pageId);
        }
        int parentId = 10;
        sql = repository.From<Menu>()
            .InnerJoin<Page>((a, b) => a.PageId == b.Id && b.Id > pageId)
            .Select((a, b) => new { MenuId = a.Id, a.ParentId, b.Url })
            .Union(f => menuPageList
                .Where(f => f.ParentId < parentId)
                .Select())
            .ToSql(out dbParameters);
        Assert.AreEqual(@"WITH `menuPageList`(`MenuId`,`ParentId`,`Url`) AS 
(
SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE a.`Id`=b.`PageId` AND b.`Id`>@MenuId
)
SELECT a.`Id` AS `MenuId`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN `sys_page` b ON a.`PageId`=b.`Id` AND b.`Id`>@p0 UNION
SELECT a.`MenuId`,a.`ParentId`,a.`Url` FROM `menuPageList` a WHERE a.`ParentId`<@p2", sql);
        Assert.AreEqual(3, dbParameters.Count);
        Assert.AreEqual("@p0", dbParameters[0].ParameterName);
        Assert.AreEqual("@MenuId", dbParameters[1].ParameterName);
        Assert.AreEqual("@p2", dbParameters[2].ParameterName);
        Assert.AreEqual(menuId, (int)dbParameters[0].Value);
        Assert.AreEqual(pageId, (int)dbParameters[1].Value);
        Assert.AreEqual(parentId, (int)dbParameters[2].Value);

        var result1 = repository.From<Menu>()
            .InnerJoin<Page>((a, b) => a.PageId == b.Id && b.Id > pageId)
            .Select((a, b) => new { MenuId = a.Id, a.ParentId, b.Url })
            .Union(f => menuPageList
                .Where(f => f.ParentId < parentId)
                .Select())
            .ToList();
        Assert.IsNotEmpty(result1);
        foreach (var item in result1)
        {
            Assert.Greater(item.MenuId, menuId);
            Assert.Less(item.ParentId, parentId);
        }
    }
    [Test]
    public async Task FromQuery_Include()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.From<Product>()
            .Include(f => f.Brand)
            .Where(f => f.ProductNo.Contains("PN-00"))
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'", sql);

        var result = await repository.From<Product>()
            .Include(f => f.Brand)
            .Where(f => f.ProductNo.Contains("PN-00"))
            .OrderBy(f => f.Id)
            .ToListAsync();

        Assert.IsNotEmpty(result);
        {
            Assert.IsNotNull(result[0].Brand);
            Assert.AreEqual("BN-001", result[0].Brand.BrandNo);
        }
        if (result.Count > 1)
        {
            Assert.IsNotNull(result[1].Brand);
            Assert.AreEqual("BN-002", result[1].Brand.BrandNo);
        }
    }
    [Test]
    public void FromQuery_IncludeMany()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Include((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.AreEqual(2, result.Count);
        Assert.IsNotNull(result[0].Order);
        Assert.IsNotNull(result[0].Order.Details);
        Assert.IsNotEmpty(result[0].Order.Details);
        Assert.AreEqual(3, result[0].Order.Details.Count);
        result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Include((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, "1", "2", "3"))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.AreEqual(2, result.Count);
        Assert.IsNotNull(result[0].Order);
        Assert.IsNotNull(result[0].Order.Details);
        Assert.IsNotEmpty(result[0].Order.Details);
        Assert.AreEqual(3, result[0].Order.Details.Count);

        result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.AreEqual(2, result.Count);
        Assert.IsNotNull(result[0].Order);
        Assert.IsNotNull(result[0].Order.Details);
        Assert.IsNotEmpty(result[0].Order.Details);
        Assert.AreEqual(3, result[0].Order.Details.Count);
        result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, "1", "2", "3"))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.AreEqual(2, result.Count);
        Assert.IsNotNull(result[0].Order);
        Assert.IsNotNull(result[0].Order.Details);
        Assert.IsNotEmpty(result[0].Order.Details);
        Assert.AreEqual(3, result[0].Order.Details.Count);
    }
    [Test]
    public void FromQuery_IncludeMany_Filter()
    {
        this.Initialize(1);
        int productId = 1;
        var repository = this.dbFactory.Create();
        var result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details, f => f.ProductId == productId)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Buyer = y, Test = x.OrderNo + "_" + y.Age % 4 })
            .ToList();

        Assert.AreEqual(2, result.Count);
        Assert.IsNotNull(result[0].Order);
        Assert.IsNotNull(result[0].Order.Details);
        Assert.IsNotEmpty(result[0].Order.Details);
        Assert.IsNotEmpty(result[0].Order.Details);
        Assert.AreEqual(productId, result[0].Order.Details[0].ProductId);
        Assert.IsNotEmpty(result[1].Order.Details);
        Assert.AreEqual(productId, result[1].Order.Details[0].ProductId);
    }
    [Test]
    public async Task FromQuery_Include_ThenInclude()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = await repository.From<Order>()
            .InnerJoin<User>((a, b) => a.SellerId == b.Id)
            .Include((x, y) => x.Buyer)
            .ThenInclude(f => f.Company)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Seller = y })
            .ToListAsync();

        Assert.IsNotEmpty(result);
        {
            Assert.IsNotNull(result[0].Order.Buyer);
            Assert.IsNotNull(result[0].Order.Buyer.Company);
            Assert.IsNotNull(result[0].Order.Buyer.SomeTimes.ToString());
        }
    }
    //[Test]
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
    //    Assert.IsTrue(result.Count == 2);
    //    Assert.NotNull(result[0].Order.Details);
    //   Assert.Greater(result[0].Order.Details);
    //    Assert.IsTrue(result[0].Order.Details.Count == 3);
    //    Assert.NotNull(result[0].Order.Details[0].Product);
    //    Assert.NotNull(result[0].Order.Details[1].Product);
    //    Assert.NotNull(result[0].Order.Details[2].Product);
    //}
    [Test]
    public void QueryPage_Include()
    {
        this.Initialize(1);
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
        Assert.IsNotNull(result);
        Assert.IsNotEmpty(result.Data);
        Assert.AreEqual(count, result.TotalCount);
        Assert.AreEqual(result.Count, result.Data.Count);
        Assert.AreEqual(1, result.Count);
        Assert.IsNotEmpty(result.Data);
        Assert.IsNotNull(result.Data[0].Product);
        Assert.AreEqual(1, result.Data[0].Product.Id);
    }
    [Test]
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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,CONVERT(b.`CreatedAt`,DATE)", sql);
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
        Assert.IsNotEmpty(result);
    }
    [Test]
    public void FromQuery_Groupby()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
           .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
           .IncludeMany((x, y) => x.Details)
           .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
           .Select((x, y) => new { Order = x, Buyer = y })
           .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`TotalAmount`>300 AND a.`Id` IN ('1','2','3')", sql);

        var result = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .IncludeMany((x, y) => x.Details)
            .Where((a, b) => a.TotalAmount > 300 && Sql.In(a.Id, new string[] { "1", "2", "3" }))
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.AreEqual(2, result.Count);
        Assert.IsNotNull(result[0].Order);
        Assert.IsNotNull(result[0].Order.Details);
        Assert.IsNotEmpty(result[0].Order.Details);
        Assert.AreEqual(3, result[0].Order.Details.Count);

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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`", sql1);
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
        Assert.IsNotEmpty(result1);
        {
            Assert.IsNotNull(result1[0].Grouping);
            Assert.IsNotNull(result1[0].Grouping.Name);
        }
        if (result1.Count > 1)
        {
            Assert.IsNotNull(result1[1].Grouping);
            Assert.IsNotNull(result1[1].Grouping.Name);
        }
    }
    [Test]
    public void FromQuery_Groupby_Fields()
    {
        this.Initialize(1);
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
        Assert.AreEqual("SELECT a.`Id` AS `UserId1`,a.`Name` AS `UserName`,CONVERT(b.`CreatedAt`,DATE) AS `CreatedDate1`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`", sql);
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
        Assert.GreaterOrEqual(result.Count, 2);
        Assert.IsNotNull(result[0].UserName);
        Assert.IsNotNull(result[1].UserName);
    }
    [Test]
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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)", sql);
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
        Assert.IsNotEmpty(result);

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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `CreatedAt`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)", sql1);
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
        Assert.IsNotEmpty(result1);
    }
    [Test]
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
        Assert.AreEqual("SELECT a.`Id` AS `UserId`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `CreatedDate`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name` DESC,CONVERT(b.`CreatedAt`,DATE)", sql);

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
    [Test]
    public async Task FromQuery_Groupby_Having()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.FromQuery(f => f.From<Order, OrderDetail>()
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
        Assert.AreEqual("SELECT a.`BuyerId`,b.`Name` AS `BuyerName`,a.`Date` AS `BuyDate`,a.`ProductCount`,a.`OrderCount`,a.`TotalAmount` FROM (SELECT a.`BuyerId`,CONVERT(a.`CreatedAt`,DATE) AS `Date`,COUNT(a.`Id`) AS `OrderCount`,COUNT(DISTINCT b.`ProductId`) AS `ProductCount`,SUM(a.`TotalAmount`) AS `TotalAmount` FROM `sys_order` a,`sys_order_detail` b WHERE a.`Id`=b.`OrderId` GROUP BY a.`BuyerId`,CONVERT(a.`CreatedAt`,DATE)) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>2 AND a.`TotalAmount`>300 ORDER BY b.`Id`", sql);

        var result = await repository.FromQuery(f => f
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
        Assert.IsNotEmpty(result);

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
        Assert.AreEqual("SELECT a.`Id` AS `BuyerId`,a.`Name` AS `BuyerName`,CONVERT(b.`CreatedAt`,DATE) AS `BuyDate`,COUNT(DISTINCT c.`ProductId`) AS `ProductCount`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 AND COUNT(DISTINCT c.`ProductId`)>2 ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)", sql1);
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
        Assert.IsNotEmpty(result1);
    }
    [Test]
    public void FromQuery_Groupby_Having_OrderBy()
    {
        this.Initialize(1);
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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND f.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)", sql);
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
        Assert.IsNotEmpty(result);
    }
    [Test]
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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND f.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`,a.`Name` DESC,CONVERT(b.`CreatedAt`,DATE)", sql);
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
        Assert.IsNotEmpty(result);
    }
    [Test]
    public void Where_Exists()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => repository.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_company` t WHERE t.`Name` LIKE '%谷歌%' AND a.`CompanyId`=t.`Id`)", sql);
        var result = repository.From<User>()
            .Where(f => repository.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToList();
        Assert.IsNotEmpty(result);

        sql = repository.From<User>()
            .Where(f => repository.From<Company>('b').Where(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id).Exists())
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_company` b WHERE b.`Name` LIKE '%谷歌%' AND a.`CompanyId`=b.`Id`)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Company>('b').Where(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id).Exists())
            .ToList();
        Assert.IsNotEmpty(result);

        sql = repository.From<User>()
            .Where(f => repository.From<Order>('b')
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .Where((x, y) => x.BuyerId == f.Id && y.Price > 200)
                .Exists())
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_order` b INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` WHERE b.`BuyerId`=a.`Id` AND c.`Price`>200)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Order>('b')
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .Where((x, y) => x.BuyerId == f.Id && y.Price > 200)
                .Exists())
            .ToList();
        Assert.IsNotEmpty(result);

        sql = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
               .InnerJoin((x, y) => x.Id == y.OrderId)
               .Where((x, y) => x.BuyerId == f.Id && y.Price > 200)
               .Exists())
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_order` b INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` WHERE b.`BuyerId`=a.`Id` AND c.`Price`>200)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
               .InnerJoin((x, y) => x.Id == y.OrderId)
               .Where((x, y) => x.BuyerId == f.Id && y.Price > 200)
               .Exists())
            .ToList();
        Assert.IsNotEmpty(result);

        sql = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
               .Where((x, y) => x.Id == y.OrderId && x.BuyerId == f.Id && y.Price > 200)
               .Exists())
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND b.`BuyerId`=a.`Id` AND c.`Price`>200)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
                .Where((x, y) => x.Id == y.OrderId && x.BuyerId == f.Id && y.Price > 200)
                .Exists())
           .ToList();
        Assert.IsNotEmpty(result);

        sql = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
                .Where((x, y) => x.Id == y.OrderId && x.BuyerId == f.Id && y.Price > 200)
                .Exists())
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND b.`BuyerId`=a.`Id` AND c.`Price`>200)", sql);
        result = repository.From<User>()
            .Where(f => repository.From<Order, OrderDetail>('b')
                .Where((x, y) => x.Id == y.OrderId && x.BuyerId == f.Id && y.Price > 200)
                .Exists())
            .ToList();
        Assert.IsNotEmpty(result);

        sql = repository.From<User>()
            .Where(f => Sql.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_company` t WHERE t.`Name` LIKE '%谷歌%' AND a.`CompanyId`=t.`Id`)", sql);
        result = repository.From<User>()
            .Where(f => Sql.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToList();
        Assert.IsNotEmpty(result);

        var sql1 = repository.From<User>()
            .Where(f => Sql.From<Order, OrderDetail>()
                .Where((x, y) => x.Id == y.OrderId && f.Id == x.BuyerId)
                .Exists())
            .GroupBy(f => new { f.Gender, f.CompanyId })
            .Select((t, a) => new { t.Grouping, UserTotal = t.CountDistinct(a.Id) })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Gender`,a.`CompanyId`,COUNT(DISTINCT a.`Id`) AS `UserTotal` FROM `sys_user` a WHERE EXISTS(SELECT b.`Id` FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND a.`Id`=b.`BuyerId` GROUP BY b.`Id` HAVING COUNT(DISTINCT c.`ProductId`)>1) GROUP BY a.`Gender`,a.`CompanyId`", sql1);
        var result1 = repository.From<User>()
            .Where(f => Sql.From<Order, OrderDetail>()
                .Where((x, y) => x.Id == y.OrderId && f.Id == x.BuyerId)
                .Exists())
            .GroupBy(f => new { f.Gender, f.CompanyId })
            .Select((t, a) => new { t.Grouping, UserTotal = t.CountDistinct(a.Id) })
            .ToList();
        Assert.IsNotEmpty(result1);
    }
    [Test]
    public async Task FromQuery_Exists()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.From<Order, OrderDetail>()
                .Where((a, b) => a.BuyerId == x.Id && a.Id == b.OrderId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => x.CountDistinct(b.ProductId) > 0)
                .Exists())
            .GroupBy((x, y) => new { x.Gender, x.CompanyId })
            .Select((x, a, b) => new { x.Grouping, UserTotal = x.CountDistinct(a.Id) })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Gender`,a.`CompanyId`,COUNT(DISTINCT a.`Id`) AS `UserTotal` FROM `sys_user` a INNER JOIN `sys_company` b ON a.`CompanyId`=b.`Id` WHERE EXISTS(SELECT c.`Id` FROM `sys_order` c,`sys_order_detail` d WHERE c.`BuyerId`=a.`Id` AND c.`Id`=d.`OrderId` GROUP BY c.`Id` HAVING COUNT(DISTINCT d.`ProductId`)>0) GROUP BY a.`Gender`,a.`CompanyId`", sql);
        var result = await repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.From<Order, OrderDetail>()
                .Where((a, b) => a.BuyerId == x.Id && a.Id == b.OrderId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => x.CountDistinct(b.ProductId) > 0)
                .Exists())
            .GroupBy((x, y) => new { x.Gender, x.CompanyId })
            .Select((x, a, b) => new { x.Grouping, UserTotal = x.CountDistinct(a.Id) })
            .ToListAsync();
        Assert.IsNotEmpty(result);
    }
    [Test]
    public void CteTable_Exists()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var myOrders = repository.From<OrderDetail, Order>()
            .Where((a, b) => a.OrderId == b.Id)
            .GroupBy((a, b) => new { a.OrderId, b.BuyerId })
            .Having((x, a, b) => x.CountDistinct(a.ProductId) > 1)
            .Select((x, a, b) => x.Grouping)
            .AsCteTable("myOrders");

        var sql = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.FromQuery(myOrders).Where(t => t.BuyerId == x.Id).Exists())
            .Select((a, b) => new { a.Id, a.Name, CompanyName = b.Name })
            .ToSql(out _);
        Assert.AreEqual(@"WITH `myOrders`(`OrderId`,`BuyerId`) AS 
(
SELECT a.`OrderId`,b.`BuyerId` FROM `sys_order_detail` a,`sys_order` b WHERE a.`OrderId`=b.`Id` GROUP BY a.`OrderId`,b.`BuyerId` HAVING COUNT(DISTINCT a.`ProductId`)>1
)
SELECT a.`Id`,a.`Name`,b.`Name` AS `CompanyName` FROM `sys_user` a INNER JOIN `sys_company` b ON a.`CompanyId`=b.`Id` WHERE EXISTS(SELECT * FROM `myOrders` f WHERE f.`BuyerId`=a.`Id`)", sql);

        var result = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.FromQuery(myOrders).Where(t => t.BuyerId == x.Id).Exists())
            .Select((a, b) => new { a.Id, a.Name, CompanyName = b.Name })
            .First();
        Assert.IsNotNull(result);
    }
    [Test]
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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE a.`Id` IN (1,2,3) AND EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND f.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`", sql);
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
        Assert.IsNotEmpty(result);

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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE a.`Id` IN (1,2,3) AND EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND f.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`,CONVERT(b.`CreatedAt`,DATE)", sql);
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
        Assert.IsNotEmpty(result);
    }
    [Test]
    public void FromQuery_In1()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => f.Id.In(Sql.From<Order>()
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` AND c.`ProductId`=1)", sql);
        var result = repository.From<User>()
            .Where(f => f.Id.In(Sql.From<Order>()
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .Select(f => f.Id)
            .ToList();
        Assert.IsNotEmpty(result);

        sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, Sql.From<Order, OrderDetail>()
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND c.`ProductId`=1)", sql);
        result = repository.From<User>()
           .Where(f => Sql.In(f.Id, Sql.From<Order, OrderDetail>()
               .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
               .Select((x, y) => x.BuyerId)))
           .Select(f => f.Id)
           .ToList();
        Assert.IsNotEmpty(result);

        var subQuery = repository.From<Order>('b')
              .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
              .Select((x, y) => x.BuyerId);
        sql = repository.From<User>()
            .Where(f => f.Id.In(subQuery))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` AND c.`ProductId`=1)", sql);
        result = repository.From<User>()
            .Where(f => f.Id.In(subQuery))
            .Select(f => f.Id)
            .ToList();
        Assert.IsNotEmpty(result);

        subQuery = repository.From<Order, OrderDetail>('b')
            .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
            .Select((x, y) => x.BuyerId);
        sql = repository.From<User>()
           .Where(f => Sql.In(f.Id, subQuery))
           .Select(f => f.Id)
           .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND c.`ProductId`=1)", sql);
        result = repository.From<User>()
           .Where(f => Sql.In(f.Id, subQuery))
           .Select(f => f.Id)
           .ToList();
        Assert.IsNotEmpty(result);
    }
    [Test]
    public void FromQuery_In_Exists1()
    {
        var repository = this.dbFactory.Create();
        bool? isMale = true;
        var sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, Sql.From<Order>()
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` AND c.`ProductId`=1) AND EXISTS(SELECT * FROM `sys_order` x,`sys_company` y WHERE a.`Id`=x.`SellerId` AND a.`CompanyId`=y.`Id`)", sql);
        var result = repository.From<User>()
            .Where(f => Sql.In(f.Id, Sql.From<Order>()
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToList();
        Assert.IsNotEmpty(result);

        sql = repository.From<User>()
            .Where(f => Sql.In(f.Id, Sql.From<Order, OrderDetail>()
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND c.`ProductId`=1) AND EXISTS(SELECT * FROM `sys_order` x,`sys_company` y WHERE a.`Id`=x.`SellerId` AND a.`CompanyId`=y.`Id`)", sql);
        result = repository.From<User>()
            .Where(f => Sql.In(f.Id, Sql.From<Order, OrderDetail>()
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToList();
        Assert.IsNotEmpty(result);
    }
    [Test]
    public void FromQuery_In_Exists_Group_CountDistinct_Count()
    {
        var repository = this.dbFactory.Create();
        bool? isMale = true;
        var sql = repository.From<User>()
            .Where(f => f.Id.In(Sql.From<OrderDetail>()
                .InnerJoin<Order>((a, b) => a.OrderId == b.Id && a.ProductId == 1)
                .Select((x, y) => y.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Company, Order>((x, y) => f.Id == y.SellerId && f.CompanyId == x.Id))
            .GroupBy(f => new { f.Gender, f.Age })
            .Select((t, a) => new { t.Grouping, CompanyCount = t.CountDistinct(a.CompanyId), UserCount = t.Count(a.Id) })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Gender`,a.`Age`,COUNT(DISTINCT a.`CompanyId`) AS `CompanyCount`,COUNT(a.`Id`) AS `UserCount` FROM `sys_user` a WHERE a.`Id` IN (SELECT c.`BuyerId` FROM `sys_order_detail` b INNER JOIN `sys_order` c ON b.`OrderId`=c.`Id` AND b.`ProductId`=1) AND EXISTS(SELECT * FROM `sys_company` x,`sys_order` y WHERE a.`Id`=y.`SellerId` AND a.`CompanyId`=x.`Id`) GROUP BY a.`Gender`,a.`Age`", sql);

        var result = repository.From<User>()
            .Where(f => f.Id.In(Sql.From<OrderDetail>()
                .InnerJoin<Order>((a, b) => a.OrderId == b.Id && a.ProductId == 1)
                .Select((x, y) => y.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Company, Order>((x, y) => f.Id == y.SellerId && f.CompanyId == x.Id))
            .GroupBy(f => new { f.Gender, f.Age })
            .Select((t, a) => new { t.Grouping, CompanyCount = t.CountDistinct(a.CompanyId), UserCount = t.Count(a.Id) })
            .ToList();
        Assert.IsNotEmpty(result);
    }
    [Test]
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
        Assert.AreEqual("SELECT COUNT(a.`Id`) AS `OrderCount`,SUM(a.`TotalAmount`) AS `TotalAmount` FROM `sys_order` a", sql);
        var result = repository.From<Order>()
            .SelectAggregate((x, a) => new
            {
                OrderCount = x.Count(a.Id),
                TotalAmount = x.Sum(a.TotalAmount)
            })
            .ToList();
        Assert.IsNotEmpty(result);
    }
    [Test]
    public void Query_Count()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var value1 = repository.From<User>().Count();
        var value2 = repository.From<User>().SelectAggregate((x, f) => x.Count()).First();
        var value3 = repository.QueryFirst<int>("SELECT COUNT(1) FROM sys_user");
        var value4 = repository.From<User>().Select(f => Sql.Raw<int>("COUNT(1)")).First();
        Assert.AreEqual(value2, value1);
        Assert.AreEqual(value3, value1);
        Assert.AreEqual(value4, value1);
    }
    [Test]
    public void Query_Where_Count()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = repository.From<User>()
            .Where(t => Sql.From<Order, OrderDetail>()
                .Where((a, b) => a.BuyerId == t.Id && a.Id == b.OrderId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => x.Count(b.Id) > 0)
                .Exists())
            .GroupBy(f => new { f.Gender, f.CompanyId })
            .Select((x, y) => new { x.Grouping, UserTotal = x.CountDistinct(y.Id) })
            .ToList();
        Assert.IsNotEmpty(result);
        {
            Assert.IsNotNull(result[0]);
            Assert.IsNotNull(result[0].Grouping);
            Assert.Greater(result[0].UserTotal, 0);
        }
    }
    [Test]
    public void Query_Max()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var value1 = repository.From<Order>().Max(f => f.TotalAmount);
        var value2 = repository.From<Order>().SelectAggregate((x, f) => x.Max(f.TotalAmount)).First();
        var value3 = repository.QueryFirst<double>("SELECT MAX(`TotalAmount`) FROM sys_order");
        var value4 = repository.From<Order>().Select(f => Sql.Raw<double>("MAX(`TotalAmount`)")).First();
        Assert.AreEqual(value2, value1);
        Assert.AreEqual(value3, value1);
        Assert.AreEqual(value4, value1);
    }
    [Test]
    public void Query_Min()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var value1 = repository.From<Order>().Min(f => f.TotalAmount);
        var value2 = repository.From<Order>().SelectAggregate((x, f) => x.Min(f.TotalAmount)).First();
        var value3 = repository.QueryFirst<double>("SELECT MIN(`TotalAmount`) FROM sys_order");
        var value4 = repository.From<Order>().Select(f => Sql.Raw<double>("MIN(`TotalAmount`)")).First();
        Assert.AreEqual(value2, value1);
        Assert.AreEqual(value3, value1);
        Assert.AreEqual(value4, value1);
    }
    [Test]
    public void Query_Avg()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var value1 = repository.From<Order>().Avg(f => f.TotalAmount);
        var value2 = repository.From<Order>().SelectAggregate((x, f) => x.Avg(f.TotalAmount)).First();
        var value3 = repository.QueryFirst<double>("SELECT AVG(`TotalAmount`) FROM sys_order");
        var value4 = repository.From<Order>().Select(f => Sql.Raw<double>("AVG(`TotalAmount`)")).First();
        Assert.AreEqual(value2, value1);
        Assert.AreEqual(value3, value1);
        Assert.AreEqual(value4, value1);
    }
    [Test]
    public void Query_ValueTuple()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = "SELECT Id,OrderNo,TotalAmount FROM sys_order";
        var result = repository.Query<(string OrderId, string OrderNo, double TotalAmount)>(sql);
        Assert.IsNotNull(result);
    }
    [Test]
    public void Query_Json()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = repository.QueryById<Order>("1");
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Products);
        Assert.IsNotNull(result.Disputes);
    }
    [Test]
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
        Assert.AreEqual("SELECT (CASE WHEN a.`OrderNo` IS NULL THEN 1 ELSE 0 END) AS `NoOrderNo`,(CASE WHEN a.`ProductCount` IS NOT NULL THEN 1 ELSE 0 END) AS `HasProduct` FROM `sys_order` a WHERE a.`ProductCount` IS NULL AND a.`ProductCount` IS NULL", sql);
        var result = repository.From<Order>()
            .Where(x => x.ProductCount == null)
            .And(true, f => !f.ProductCount.HasValue)
            .Select(x => new
            {
                NoOrderNo = x.OrderNo == null,
                HasProduct = x.ProductCount.HasValue
            })
            .ToList();
        Assert.IsNotEmpty(result);
    }
    [Test]
    public async Task Query_Where_IsNull()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .Where(x => x.ProductCount == null || x.BuyerId.IsNull())
            .And(true, f => !f.ProductCount.HasValue)
            .Select(x => x.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_order` a WHERE (a.`ProductCount` IS NULL OR a.`BuyerId` IS NULL) AND a.`ProductCount` IS NULL", sql);
        var result = repository.From<Order>()
            .Where(x => x.ProductCount == null || x.BuyerId.IsNull())
            .And(true, f => !f.ProductCount.HasValue)
            .Select(x => x.Id)
            .ToList();
        Assert.IsNotNull(result);

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
        Assert.AreEqual("SELECT a.`Id`,a.`OrderNo`,IFNULL(a.`ProductCount`,0) AS `ProductCount`,IFNULL(a.`BuyerId`,0) AS `BuyerId`,IFNULL(a.`TotalAmount`,0) AS `TotalAmount` FROM `sys_order` a WHERE IFNULL(a.`ProductCount`,0)>0 OR IFNULL(a.`BuyerId`,0)>=0", sql1);

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
        Assert.GreaterOrEqual(result1.Count, 3);
        Assert.AreEqual(0, myOrders[0].BuyerId);
        Assert.AreEqual(0, myOrders[1].ProductCount);
        Assert.AreEqual(0, myOrders[2].TotalAmount);
    }
    [Test]
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
        Assert.AreEqual(@"SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`=@p0 UNION ALL
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
        Assert.IsNotEmpty(result);
    }
    [Test]
    public async Task Query_Union_Take()
    {
        this.Initialize(1);
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
        Assert.AreEqual(@"SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId`,b.`Name` AS `BuyerName` FROM (SELECT * FROM (SELECT b.`Id`,b.`OrderNo`,b.`SellerId`,b.`BuyerId` FROM `sys_order` b WHERE b.`Id`=@p0 ORDER BY b.`Id` LIMIT 1) a UNION ALL
SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`<>@p1) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id`", sql);
        Assert.AreEqual("@p0", dbParameters[0].ParameterName);
        Assert.AreEqual("@p1", dbParameters[1].ParameterName);
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
        Assert.IsNotEmpty(result);

        var sql1 = repository
            .From<User>()
            .WithQuery(t => t
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
        Assert.AreEqual(@"SELECT b.`Id`,b.`OrderNo`,b.`SellerId`,a.`Name` AS `SellerName`,b.`BuyerId`,c.`Name` AS `BuyerName` FROM `sys_user` a INNER JOIN (SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`SellerId`=b.`Id` WHERE a.`Id`=@p0 ORDER BY a.`Id` LIMIT 1) a UNION ALL
SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`Id`<>@p1) b ON a.`Id`=b.`SellerId` INNER JOIN `sys_user` c ON b.`BuyerId`=c.`Id`", sql1);
        Assert.AreEqual("@p0", dbParameters1[0].ParameterName);
        Assert.AreEqual("@p1", dbParameters1[1].ParameterName);

        var result1 = repository
            .From<User>()
            .WithQuery(t => t
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
        Assert.IsNotEmpty(result1);
    }
    [Test]
    public async Task FromQuery_Union_Limit()
    {
        this.Initialize(1);
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
        Assert.AreEqual(@"SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`=@p0 ORDER BY a.`Id` LIMIT 1) a UNION ALL
SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`<>@p1 LIMIT 1) a", sql);

        Assert.AreEqual(2, dbParameters.Count);
        Assert.AreEqual(id1, (string)dbParameters[0].Value);
        Assert.AreEqual(id2, (string)dbParameters[1].Value);

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
        Assert.IsNotEmpty(result);
        foreach (var item in result)
        {
            Assert.IsTrue(item.Id == id1 || item.Id != id2);
        }
    }
    [Test]
    public void FromQuery_Union_SubQuery_Limit()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.FromQuery(f => f.From<Order>()
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
        Assert.AreEqual(@"SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`<>'3' ORDER BY a.`Id` LIMIT 1) a UNION ALL
SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`='3' LIMIT 1) a", sql);
        var result = repository.FromQuery(f => f.From<Order>()
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
        Assert.IsNotEmpty(result);
    }
    [Test]
    public async Task FromQuery_Union_SubQuery_OrderBy()
    {
        this.Initialize(1);
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
        Assert.AreEqual(@"SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`=@p0 ORDER BY a.`Id` LIMIT 1) a UNION ALL
SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`<>@p1 ORDER BY a.`Id` DESC LIMIT 1) a", sql);
        Assert.AreEqual(2, dbParameters.Count);
        Assert.AreEqual(id1, (string)dbParameters[0].Value);
        Assert.AreEqual(id2, (string)dbParameters[1].Value);

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
        Assert.IsNotEmpty(result);
        foreach (var item in result)
        {
            Assert.IsTrue(item.Id == id1 || item.Id != id2);
        }
    }
    [Test]
    public void Union_Take()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository
            .FromQuery(f => f.From<Menu>()
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
        Assert.AreEqual(@"SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM (SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a) a INNER JOIN `sys_page` b ON a.`Id`=b.`Id` UNION ALL
SELECT * FROM (SELECT a.`BuyerId`,a.`OrderNo`,a.`SellerId`,CAST(a.`BuyerId` AS CHAR) FROM `sys_order` a WHERE a.`Id`='2' ORDER BY a.`Id` DESC LIMIT 1) a", sql);

        var result = repository
            .FromQuery(f => f.From<Menu>()
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
        Assert.IsNotEmpty(result);
    }
    [Test]
    public async Task Query_WithCte_SelfRef()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        int menuId = 2;
        int pageId = 1;
        var sql = repository
            .FromQuery(f => f.From<Menu>()
                .Where(t => t.Id >= menuId)
                .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId })
                .AsCteTable("MenuList"))
            .InnerJoin<Page>((a, b) => a.Id == b.Id)
            .Where((x, y) => y.Id >= pageId)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out var dbParameters);

        Assert.AreEqual(@"WITH `MenuList`(`Id`,`Name`,`ParentId`,`PageId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a WHERE a.`Id`>=@p0
)
SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `MenuList` a INNER JOIN `sys_page` b ON a.`Id`=b.`Id` WHERE b.`Id`>=@p1", sql);
        Assert.AreEqual(2, dbParameters.Count);
        Assert.AreEqual(menuId, (int)dbParameters[0].Value);
        Assert.AreEqual(pageId, (int)dbParameters[1].Value);

        var result = await repository
            .FromQuery(f => f.From<Menu>()
                .Where(t => t.Id >= menuId)
                .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId })
                .AsCteTable("MenuList"))
            .InnerJoin<Page>((a, b) => a.Id == b.Id)
            .Where((x, y) => y.Id >= pageId)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToListAsync();
        Assert.IsNotEmpty(result);
    }
    [Test]
    public async Task Query_WithNextCte()
    {
        this.Initialize(1);
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
            .FromQuery(myCteTable1)
            .InnerJoin(myCteTable2, (a, b) => a.Id == b.Id)
            .Select((a, b) => new { b.Id, a.Name, b.ParentId, b.Url })
            .ToSql(out _);
        Assert.AreEqual(@"WITH RECURSIVE `myCteTable1`(`Id`,`Name`,`ParentId`) AS 
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
            .FromQuery(myCteTable2)
            .InnerJoin(myCteTable1, (a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, b.Name, a.ParentId, a.Url })
            .ToList();
        Assert.IsNotEmpty(result1);

        int pageId = 1;
        sql = repository
            .FromQuery(f => menuList)
            .WithQuery(x => x.From<Page>()
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
        Assert.AreEqual(@"WITH RECURSIVE `MenuList`(`Id`,`Name`,`ParentId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a WHERE a.`Id`=@RootId UNION ALL
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a INNER JOIN `MenuList` b ON a.`ParentId`=b.`Id`
)
SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `MenuList` a INNER JOIN (SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `sys_menu` b ON a.`Id`=b.`PageId` WHERE a.`Id`=@p1 UNION ALL
SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `MenuList` b ON a.`Id`=b.`Id` WHERE a.`Id`>@p2) b ON a.`Id`=b.`Id`", sql);
        var result2 = repository
            .FromQuery(f => menuList)
            .WithQuery(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()//.WithQuery(self)
                    .InnerJoin(menuList, (a, b) => a.Id == b.Id)
                    .Where((a, b) => a.Id > pageId)
                    .Select((x, y) => new { y.Id, x.Url })))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToList();
        Assert.IsNotEmpty(result2);

        sql = repository
            .FromQuery(menuList)
            .WithQuery(x => x.From<Page>()
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
        Assert.AreEqual(@"WITH RECURSIVE `MenuList`(`Id`,`Name`,`ParentId`) AS 
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
            .FromQuery(menuList)
            .WithQuery(x => x.From<Page>()
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
        Assert.IsNotEmpty(result3);
    }
    [Test]
    public async Task Query_WithTable()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Menu>()
            .WithQuery(f => f.From<Page, Menu>('c')
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
            .Where((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToSql(out _);
        Assert.AreEqual(@"SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `sys_menu` a,(SELECT d.`Id`,d.`ParentId`,c.`Url` FROM `sys_page` c,`sys_menu` d WHERE c.`Id`=d.`PageId`) b WHERE a.`Id`=b.`Id`", sql);

        var result = repository.From<Menu>()
            .WithQuery(f => f.From<Page, Menu>('c')
                .Where((a, b) => a.Id == b.PageId)
                .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
            .Where((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToList();
        Assert.IsNotEmpty(result);

        int menuId = 1;
        int pageId = 1;
        int pageId2 = 1;
        var sql1 = repository
            .FromQuery(f => f.From<Menu>()
                    .Where(x => x.Id == menuId)
                    .Select(x => new { x.Id, x.Name, x.ParentId })
                .UnionAllRecursive((x, y) => x.From<Menu>()
                    .InnerJoin(y, (a, b) => a.ParentId == b.Id)
                    .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
                .AsCteTable("myCteTable1"))
            .WithQuery(f => f.From<Page>()
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
        Assert.AreEqual(@"WITH RECURSIVE `myCteTable1`(`Id`,`Name`,`ParentId`) AS 
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
            .FromQuery(f => f.From<Menu>()
                    .Where(x => x.Id == menuId)
                    .Select(x => new { x.Id, x.Name, x.ParentId })
                .UnionAllRecursive((x, y) => x.From<Menu>()
                    .InnerJoin(y, (a, b) => a.ParentId == b.Id)
                    .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
                .AsCteTable("myCteTable1"))
            .WithQuery(f => f.From<Page>()
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
        Assert.IsNotEmpty(result1);

        var sql2 = repository.From<Order, OrderDetail>()
            .InnerJoin((x, y) => x.Id == y.OrderId)
            .Include((x, y) => x.Buyer)
            .Where((a, b) => a.Id == b.OrderId)
            .Select((a, b) => new { Order = a, a.BuyerId, DetailId = b.Id, b.Price, b.Quantity, b.Amount })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,c.`Id`,c.`TenantId`,c.`Name`,c.`Gender`,c.`Age`,c.`CompanyId`,c.`GuidField`,c.`SomeTimes`,c.`SourceType`,c.`IsEnabled`,c.`CreatedAt`,c.`CreatedBy`,c.`UpdatedAt`,c.`UpdatedBy`,a.`BuyerId`,b.`Id` AS `DetailId`,b.`Price`,b.`Quantity`,b.`Amount` FROM `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` LEFT JOIN `sys_user` c ON a.`BuyerId`=c.`Id` WHERE a.`Id`=b.`OrderId`", sql2);

        var result2 = repository.From<Order, OrderDetail>()
            .InnerJoin((x, y) => x.Id == y.OrderId)
            .Include((x, y) => x.Buyer)
            .Where((a, b) => a.Id == b.OrderId)
            .Select((a, b) => new { Order = a, a.BuyerId, DetailId = b.Id, b.Price, b.Quantity, b.Amount })
            .ToList();
        Assert.IsNotEmpty(result2);
        Assert.IsNotNull(result2[0].Order);
        Assert.IsNotNull(result2[0].Order.Buyer);

        var sql3 = repository.FromQuery(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .InnerJoin<Order>((a, b, c) => a.OrderId == c.Id)
            .Select((a, b, c) => new { a.OrderId, a.BuyerId, Buyer = b, Order = c, a.TotalAmount })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`OrderId`,a.`BuyerId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,c.`Id`,c.`TenantId`,c.`OrderNo`,c.`ProductCount`,c.`TotalAmount`,c.`BuyerId`,c.`BuyerSource`,c.`SellerId`,c.`Products`,c.`Disputes`,c.`IsEnabled`,c.`CreatedAt`,c.`CreatedBy`,c.`UpdatedAt`,c.`UpdatedBy`,a.`TotalAmount` FROM (SELECT a.`Id` AS `OrderId`,a.`BuyerId`,SUM(b.`Amount`) AS `TotalAmount` FROM `sys_order` a,`sys_order_detail` b,`sys_user` c WHERE a.`Id`=b.`OrderId` AND a.`BuyerId`=c.`Id` AND c.`Age`>20 GROUP BY a.`Id`,a.`BuyerId` HAVING SUM(b.`Amount`)>500) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` INNER JOIN `sys_order` c ON a.`OrderId`=c.`Id`", sql3);

        var result3 = repository.FromQuery(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .InnerJoin<Order>((a, b, c) => a.OrderId == c.Id)
            .Select((a, b, c) => new { a.OrderId, a.BuyerId, Buyer = b, Order = c, a.TotalAmount })
            .ToList();
        Assert.IsNotNull(result3);
    }
    [Test]
    public void SelectTo()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.DeleteById<Order>("8");
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

        var result = repository.UseMaster()
            .From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo<OrderInfo>()
            .ToList();
        Assert.AreEqual("8", result[0].Id);
        Assert.AreEqual(1, result[0].BuyerId);
        Assert.AreEqual("On-ZwYx", result[0].OrderNo);
        Assert.IsNull(result[0].Description);

        result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo(f => new OrderInfo
            {
                Description = "TotalAmount:" + f.TotalAmount
            })
            .ToList();
        Assert.AreEqual("8", result[0].Id);
        Assert.AreEqual(1, result[0].BuyerId);
        Assert.AreEqual("On-ZwYx", result[0].OrderNo);
        Assert.IsNotNull(result[0].Description);
        Assert.AreEqual("TotalAmount:500", result[0].Description);

        result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo(f => new OrderInfo
            {
                Description = this.DeferInvoke().Deferred()
            })
            .ToList();
        Assert.AreEqual("8", result[0].Id);
        Assert.AreEqual(1, result[0].BuyerId);
        Assert.AreEqual("On-ZwYx", result[0].OrderNo);
        Assert.IsNotNull(result[0].Description);
        Assert.AreEqual(this.DeferInvoke(), result[0].Description);

        var result1 = repository.FromQuery(f =>
               f.From<Order, OrderDetail>('a')
                .Where((a, b) => a.Id == b.OrderId)
                .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                .Having((x, a, b) => x.CountDistinct(b.ProductId) > 0)
                .Select((x, a, b) => new { x.Grouping.BuyerId, x.Grouping.OrderId, ProductTotal = x.CountDistinct(b.ProductId) }))
           .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
           .SelectTo((x, y) => new OrderBuyerInfo { BuyerName = y.Name })
           .First();
        if (result1 != null)
        {
            Assert.IsNotNull(result1);
            Assert.IsFalse(string.IsNullOrEmpty(result1.OrderId));
            Assert.Greater(result1.BuyerId, 0);
            Assert.IsNull(result1.OrderNo);
            Assert.IsNotNull(result1.BuyerName);
        }
    }
    [Test]
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
        Assert.AreEqual("SELECT a.`BuyerId`,a.`Id` AS `OrderId`,b.`Name` AS `BuyerName`,b.`Age` AS `BuyerAge`,COUNT(DISTINCT c.`ProductId`) AS `ProductCount`,IFNULL(MAX(b.`CreatedAt`),a.`CreatedAt`) AS `LastBuyAt` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` LEFT JOIN `sys_order_detail` c ON a.`Id`=c.`OrderId` GROUP BY a.`BuyerId`,a.`Id`,b.`Name`,b.`Age` ORDER BY IFNULL(MAX(b.`CreatedAt`),a.`CreatedAt`) DESC", sql);

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
        Assert.IsNotEmpty(result);
        if (result.Count > 1)
        {
            Assert.GreaterOrEqual(result[0].LastBuyAt, result[1].LastBuyAt);
        }
    }
    private string DeferInvoke() => "DeferInvoke";




    [Test]
    public void Update_AnonymousObject()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var user = repository.QueryById<User>(1);
        user.Name = "kevin";
        user.Gender = Gender.Female;
        user.SourceType = null;
        var count = repository.Update<User>(user);
        var changedUser = repository.QueryById<User>(1);
        Assert.Greater(count, 0);
        Assert.IsNotNull(changedUser);
        Assert.AreEqual(user.Name, changedUser.Name);
        Assert.AreEqual(changedUser.SourceType, changedUser.SourceType);

        count = repository.Update<User>(new
        {
            Id = 1,
            Name = (string)null,
            Gender = Gender.Male,
            SourceType = UserSourceType.Douyin
        });
        var result = repository.QueryById<User>(1);
        Assert.Greater(count, 0);
        Assert.IsNotNull(result);
        Assert.IsNull(result.Name);
        Assert.AreEqual(UserSourceType.Douyin, result.SourceType);
    }
    [Test]
    public async Task Update_AnonymousObjects()
    {
        this.Initialize(1);
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
        Assert.Greater(count, 0);
        Assert.AreEqual(orders.Count, parameters.Count);
        orders.Sort((x, y) => x.Id.CompareTo(y.Id));
        for (int i = 0; i < orders.Count; i++)
        {
            Assert.AreEqual(parameters[i].TotalAmount, (decimal)orders[i].TotalAmount);
        }
    }
    [Test]
    public async Task Update_SetBulk()
    {
        this.Initialize(1);
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
        Assert.AreEqual("UPDATE `sys_order_detail` SET `Amount`=@Amount0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Amount`=@Amount1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Amount`=@Amount2 WHERE `Id`=@kId2", sql);
        Assert.AreEqual(parameters.Count * 2, dbParameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.AreEqual($"@Amount{i}", dbParameters[i * 2].ParameterName);
            Assert.AreEqual($"@kId{i}", dbParameters[i * 2 + 1].ParameterName);
        }
    }
    [Test]
    public async Task Update_SetBulk_OnlyFields()
    {
        this.Initialize(1);
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
        Assert.AreEqual("UPDATE `sys_order_detail` SET `Price`=@Price0,`Quantity`=@Quantity0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Price`=@Price1,`Quantity`=@Quantity1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Price`=@Price2,`Quantity`=@Quantity2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Price`=@Price3,`Quantity`=@Quantity3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Price`=@Price4,`Quantity`=@Quantity4 WHERE `Id`=@kId4", sql);
        Assert.AreEqual(parameters.Count * 3, dbParameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.AreEqual($"@Price{i}", dbParameters[i * 3].ParameterName);
            Assert.AreEqual($"@Quantity{i}", dbParameters[i * 3 + 1].ParameterName);
            Assert.AreEqual($"@kId{i}", dbParameters[i * 3 + 2].ParameterName);
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
        Assert.AreEqual(parameters.Count, result);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.AreEqual(parameters[i].Price, updatedDetails[i].Price);
            Assert.AreEqual(parameters[i].Quantity, updatedDetails[i].Quantity);
            Assert.AreNotEqual(parameters[i].Amount, updatedDetails[i].Amount);
        }
    }
    [Test]
    public async Task Update_SetBulk_IgnoreFields()
    {
        this.Initialize(1);
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
        Assert.AreEqual("UPDATE `sys_order_detail` SET `Quantity`=@Quantity0,`Amount`=@Amount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Quantity`=@Quantity1,`Amount`=@Amount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Quantity`=@Quantity2,`Amount`=@Amount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Quantity`=@Quantity3,`Amount`=@Amount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Quantity`=@Quantity4,`Amount`=@Amount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4", sql);
        Assert.AreEqual(parameters.Count * 4, dbParameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.AreEqual($"@Quantity{i}", dbParameters[i * 4].ParameterName);
            Assert.AreEqual($"@Amount{i}", dbParameters[i * 4 + 1].ParameterName);
            Assert.AreEqual($"@UpdatedAt{i}", dbParameters[i * 4 + 2].ParameterName);
            Assert.AreEqual($"@kId{i}", dbParameters[i * 4 + 3].ParameterName);
        }
        var ids = parameters.Select(f => f.Id).ToList();
        repository.BeginTransaction();
        var result = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .IgnoreFields(f => f.Price)
            .Execute();
        var updatedDetails = await repository.QueryAsync<OrderDetail>(f => ids.Contains(f.Id));
        repository.Commit();
        Assert.AreEqual(parameters.Count, result);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.AreNotEqual(parameters[i].Price, updatedDetails[i].Price);
            Assert.AreEqual(parameters[i].Quantity, updatedDetails[i].Quantity);
            Assert.AreEqual(parameters[i].Amount, updatedDetails[i].Amount);
            Assert.AreEqual(parameters[i].UpdatedAt, updatedDetails[i].UpdatedAt);
            Assert.AreEqual(parameters[i].Id, updatedDetails[i].Id);
        }
    }
    [Test]
    public async Task Update_SetBulk_SetFields()
    {
        this.Initialize(1);
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
        Assert.AreEqual("UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4", sql);
        Assert.AreEqual(parameters.Count * 3 + 2, dbParameters.Count);

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
        Assert.AreEqual(parameters.Count, result);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.AreEqual(3, updatedDetails[i].ProductId);
            Assert.AreEqual(5, updatedDetails[i].Quantity);
            Assert.AreEqual(parameters[i].Amount, updatedDetails[i].Amount);
            Assert.AreEqual(parameters[i].UpdatedAt, updatedDetails[i].UpdatedAt);
        }
    }
    [Test]
    public void Update_Fields_Where()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = repository.Update<User>(f => new
        {
            Name = f.Name + "_1",
            Gender = Gender.Female,
            SourceType = DBNull.Value
        }, t => t.Id == 1);
        var result1 = repository.QueryById<User>(1);
        Assert.Greater(result, 0);
        Assert.IsNotNull(result1);
        Assert.AreEqual("leafkevin_1", result1.Name);
        Assert.IsFalse(result1.SourceType.HasValue);

        var result2 = repository.Update<User>(new
        {
            Id = 1,
            Name = "kevin",
            Gender = Gender.Female,
            SourceType = DBNull.Value
        });
        var result3 = repository.QueryById<User>(1);
        Assert.Greater(result2, 0);
        Assert.IsNotNull(result3);
        Assert.AreEqual("kevin", result3.Name);
        Assert.IsFalse(result3.SourceType.HasValue);
    }
    [Test]
    public void Update_Set_Fields_Where()
    {
        this.Initialize(1);
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
        var result2 = repository.QueryById<User>(1);
        Assert.Greater(result, 0);
        Assert.IsNotNull(result2);
        Assert.AreEqual("leafkevin22", result2.Name);
        Assert.AreEqual(25, result2.Age);
        Assert.AreEqual(0, result2.CompanyId);
    }
    [Test]
    public void Update_Fields_Parameters()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = repository.Update<User>(new { Id = 1, Name = "leafkevin11" });
        var result1 = repository.QueryById<User>(1);
        Assert.Greater(result, 0);
        Assert.IsNotNull(result1);
        Assert.AreEqual("leafkevin11", result1.Name);
    }
    [Test]
    public void Update_Set_AnonymousObject_Where()
    {
        this.Initialize(1);
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
        Assert.AreEqual("UPDATE `sys_user` SET `Name`=@Name,`Age`=@Age,`CompanyId`=@CompanyId WHERE `Id`=1", sql);
        Assert.AreEqual(3, dbParameters.Count);
        Assert.AreEqual("leafkevin22", (string)dbParameters[0].Value);
        Assert.AreEqual(25, (int)dbParameters[1].Value);
        Assert.AreEqual(DBNull.Value, dbParameters[2].Value);

        repository.Update<User>()
           .Set(f => new
           {
               Age = 25,
               Name = "leafkevin22",
               CompanyId = DBNull.Value
           })
           .Where(f => f.Id == 1)
           .Execute();
        var result = repository.QueryById<User>(1);
        Assert.AreEqual("leafkevin22", result.Name);
        Assert.AreEqual(25, result.Age);
        Assert.AreEqual(0, result.CompanyId);
    }
    [Test]
    public void Update_Set_AnonymousObject_Where_OnlyFields()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var user = repository.QueryById<User>(1);
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
        Assert.AreEqual("UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=1", sql);
        Assert.IsNotEmpty(dbParameters);
        Assert.AreEqual("leafkevinabc", (string)dbParameters[0].Value);

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
        var result = repository.QueryById<User>(1);
        Assert.AreEqual("leafkevinabc", result.Name);
        Assert.AreEqual(user.Age, result.Age);
        Assert.AreEqual(user.CompanyId, result.CompanyId);
    }
    [Test]
    public void Update_Set_AnonymousObject_Where_IgnoreFields()
    {
        this.Initialize(1);
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
        Assert.AreEqual("UPDATE `sys_user` SET `Age`=@Age,`CompanyId`=@CompanyId WHERE `Id`=1", sql);
        Assert.AreEqual(2, dbParameters.Count);
        Assert.AreEqual(25, (int)dbParameters[0].Value);
        Assert.AreEqual(DBNull.Value, dbParameters[1].Value);

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
        var result = repository.QueryById<User>(1);
        Assert.AreNotEqual("leafkevin22", result.Name);
        Assert.AreEqual(25, result.Age);
        Assert.AreEqual(0, result.CompanyId);
    }
    [Test]
    public void Update_SetWith_Parameters()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var result = repository.Update<Order>(new
        {
            ProductCount = 10,
            Id = 1
        });
        var result1 = repository.QueryById<Order>(new { Id = "1" });
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotNull(result1);
            Assert.AreEqual("1", result1.Id);
            Assert.AreEqual(10, result1.ProductCount);
        }
        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(new { ProductCount = 11 })
            .WhereBy(new { Id = "1" })
            .Execute();
        var result2 = repository.QueryById<Order>(new { Id = "1" });
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotNull(result2);
            Assert.AreEqual("1", result2.Id);
            Assert.AreEqual(11, result2.ProductCount);
        }
        var updateObj = new Dictionary<string, object>
        {
            { "ProductCount", result2.ProductCount + 1 },
            { "TotalAmount", result2.TotalAmount + 100 }
        };
        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(updateObj)
            .WhereBy(new { Id = "1" })
            .Execute();
        var result3 = repository.QueryById<Order>(new { Id = "1" });
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotNull(result3);
            Assert.AreEqual("1", result3.Id);
            Assert.AreEqual(result2.ProductCount + 1, result3.ProductCount);
            Assert.AreEqual(result2.TotalAmount + 100, result3.TotalAmount);
        }
    }
    [Test]
    public async Task Update_MultiParameters()
    {
        this.Initialize(1);
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
        Assert.Greater(count, 0);
        for (int i = 0; i < orderDetails.Count; i++)
        {
            Assert.AreEqual(parameters[i].Price, orderDetails[i].Price);
            Assert.AreEqual(parameters[i].Quantity, orderDetails[i].Quantity);
            Assert.AreEqual(parameters[i].Amount, orderDetails[i].Amount);
        }
    }
    [Test]
    public void Update_Set_MethodCall()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var parameter = repository.QueryById<Order>("1");
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
        var order = repository.QueryById<Order>("2");
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotEmpty(order.Products);
            Assert.AreEqual(3, order.Products.Count);
            Assert.AreEqual(1, order.Products[0]);
            Assert.AreEqual(2, order.Products[1]);
            Assert.AreEqual(3, order.Products[2]);
            Assert.AreEqual(this.CalcAmount(parameter.TotalAmount, 3), order.TotalAmount);
        }

        var updateObj = repository.QueryById<Order>("1");
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
            .WhereBy(new { updateObj.Id })
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order` SET `TotalAmount`=@p0,`Products`=@p1,`Disputes`=@p2,`UpdatedAt`=NOW() WHERE `Id`=@kId", sql);
        Assert.AreEqual(4, dbParameters.Count);
        Assert.AreEqual("@p0", dbParameters[0].ParameterName);
        Assert.AreEqual(this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3), (double)dbParameters[0].Value);
        Assert.AreEqual("@p1", dbParameters[1].ParameterName);
        Assert.AreEqual(new JsonTypeHandler().ToFieldValue(this.GetProducts()).ToString(), (string)dbParameters[1].Value);
        Assert.AreEqual("@p2", dbParameters[2].ParameterName);
        Assert.AreEqual(new JsonTypeHandler().ToFieldValue(updateObj.Disputes).ToString(), (string)dbParameters[2].Value);
        Assert.AreEqual("@kId", dbParameters[3].ParameterName);
        Assert.AreEqual(updateObj.Id, (string)dbParameters[3].Value);

        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(f => new
            {
                TotalAmount = this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3).Deferred(),
                Products = this.GetProducts(),
                updateObj.Disputes,
                updateObj.UpdatedAt
            })
            .WhereBy(new { updateObj.Id })
            .Execute();

        var updatedOrder = repository.QueryById<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotEmpty(updatedOrder.Products);
            Assert.AreEqual(3, updatedOrder.Products.Count);
            Assert.AreEqual(1, updatedOrder.Products[0]);
            Assert.AreEqual(2, updatedOrder.Products[1]);
            Assert.AreEqual(3, updatedOrder.Products[2]);
            Assert.AreEqual(this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3), updatedOrder.TotalAmount);
            //TODO:两个对象的hash值是不同的，各属性值都是一样
            Assert.AreEqual(JsonSerializer.Serialize(updateObj.Disputes), JsonSerializer.Serialize(updatedOrder.Disputes));
            //TODO:两个日期的ticks是不同的，MySqlConnector驱动保存时间就到秒
            //Assert.IsTrue(updatedOrder.UpdatedAt == updateObj.UpdatedAt);
        }
    }
    [Test]
    public void Update_Set_FromQuery_Multi()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Set(true, (x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .Set(x => x.Products, [1, 2, 3])
            .Where((a, b) => a.BuyerId == 1)
          .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1", sql);
        Assert.IsNotNull(dbParameters);
        Assert.AreEqual("@p0", dbParameters[0].ParameterName);
        Assert.AreEqual(200.56, (double)dbParameters[0].Value);
        Assert.AreEqual("@Products", dbParameters[1].ParameterName);
        Assert.AreEqual(JsonSerializer.Serialize(new List<int> { 1, 2, 3 }), (string)dbParameters[1].Value);

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
        Assert.Greater(count, 0);

        sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        count = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .Execute();
        Assert.Greater(count, 0);
    }
    [Test]
    public async Task Update_SetFrom()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        var count = await repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ExecuteAsync();
        Assert.Greater(count, 0);

        var orderAmounts = repository.From<OrderDetail>()
            .GroupBy(x => x.OrderId)
            .Select((f, a) => new
            {
                OrderId = f.Grouping,
                TotalAmount = f.Sum(a.Amount)
            })
            .ToList();
    }
    [Test]
    public void Update_Set_Join()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Set(true, (x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .Set(x => x.Products, [1, 2, 3])
            .Where((a, b) => a.BuyerId == 1)
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1", sql);
        Assert.IsNotNull(dbParameters);
        Assert.AreEqual("@p0", dbParameters[0].ParameterName);
        Assert.AreEqual(200.56, (double)dbParameters[0].Value);
        Assert.AreEqual("@Products", dbParameters[1].ParameterName);
        Assert.AreEqual(JsonSerializer.Serialize(new List<int> { 1, 2, 3 }), (string)dbParameters[1].Value);

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
        Assert.Greater(result, 0);

        sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        result = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .Execute();
        Assert.Greater(result, 0);
    }
    [Test]
    public async Task Update_Set_FromQuery_One()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var order = repository.QueryById<Order>("1");
        var totalAmount = await repository.From<OrderDetail>()
            .Where(f => f.OrderId == "1")
            .SumAsync(f => f.Amount);
        var sql = repository.Update<Order>()
            .SetFrom(f => f.TotalAmount, (x, y) => x
                .From<OrderDetail>('b')
                .Where(t => t.OrderId == y.Id)
                .SelectAggregate((x, t) => (double)x.Sum(t.Amount)))
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.Id == "1")
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`Id`='1'", sql);
        Assert.IsNotEmpty(dbParameters);
        Assert.AreEqual("ON_111", (string)dbParameters[0].Value);

        var count = await repository.Update<Order>()
            .SetFrom(f => f.TotalAmount, (x, y) => x
                .From<OrderDetail>('b')
                .Where(t => t.OrderId == y.Id)
                .SelectAggregate((x, t) => (double)x.Sum(t.Amount)))
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.Id == "1")
            .ExecuteAsync();
        var reult = repository.QueryById<Order>("1");
        Assert.Greater(count, 0);
        Assert.AreEqual((double)totalAmount, reult.TotalAmount);
        Assert.AreNotEqual(order.TotalAmount, reult.TotalAmount);

        sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount))
            })
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        count = await repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount))
            })
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ExecuteAsync();
        Assert.Greater(count, 0);
    }
    [Test]
    public void Update_Set_FromQuery_One_Enum()
    {
        this.Initialize(1);
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
        Assert.AreEqual("UPDATE `sys_company` a SET a.`Nature`=(SELECT b.`Nature` FROM `sys_company` b WHERE b.`Id`=1) WHERE a.`Nature`<>'Internet'", sql);
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
        Assert.Greater(result, 0);
        foreach (var company in companies)
        {
            Assert.AreEqual(microCompany.Nature, company.Nature);
        }
    }
    [Test]
    public async Task Update_Set_FromQuery_Fields()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .SetFrom((x, y) => new
            {
                TotalAmount = x.From<OrderDetail>('b')
                    .Where(f => f.OrderId == y.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount))
            })
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

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
        Assert.Greater(result, 0);
        foreach (var updatedValue in updatedValues)
        {
            var origValue = origValues.Find(f => f.Grouping.Id == updatedValue.Id);
            Assert.AreEqual(origValue.TotalAmount, updatedValue.TotalAmount);
            Assert.AreEqual(origValue.Grouping.OrderNo + "_111", updatedValue.OrderNo);
            Assert.AreEqual(default(int), updatedValue.BuyerId);
        }
    }
    [Test]
    public void Update_InnerJoin_One()
    {
        this.Initialize(1);
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
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);
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
        Assert.Greater(result, 0);
    }
    [Test]
    public async Task Update_InnerJoin_Multi()
    {
        this.Initialize(1);
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
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=b.`Amount`,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

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
        Assert.Greater(result, 0);
        foreach (var updatedValue in updatedValues)
        {
            var origValue = origValues.Find(f => f.Grouping.Id == updatedValue.Id);
            Assert.AreEqual(origValue.TotalAmount, updatedValue.TotalAmount);
            Assert.AreEqual(origValue.Grouping.OrderNo + "_111", updatedValue.OrderNo);
            Assert.AreEqual(default(int), updatedValue.BuyerId);
        }
    }
    [Test]
    public async Task Update_InnerJoin_Fields()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SetFrom((x, y) => new
            {
                TotalAmount = x.From<OrderDetail>('c')
                    .Where(f => f.OrderId == y.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount))
            })
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((a, b) => a.Id == "1")
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,' - ',CAST(b.`Id` AS CHAR)),a.`BuyerId`=NULL WHERE a.`Id`='1'", sql);

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
                    .SelectAggregate((x, t) => x.Sum(t.Amount))
            })
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((a, b) => a.Id == "1")
            .Execute();
        var order = await repository.QueryByIdAsync<Order>("1");
        var orderDetails = await repository.QueryAsync<OrderDetail>(f => f.OrderId == "1");
        await repository.CommitAsync();
        Assert.Greater(result, 0);
        Assert.AreEqual(orderDetails.Where(f => f.OrderId == order.Id).Sum(f => f.Amount), order.TotalAmount);
        Assert.AreEqual(origValues.OrderNo + " - " + origValues.Id.ToString(), order.OrderNo);
        Assert.AreEqual(default(int), order.BuyerId);

        var sql1 = repository.Update<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SetFrom(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
                .Where(f => f.OrderId == y.Id)
                .SelectAggregate((x, f) => (double)x.Sum(f.Amount)))
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((x, y) => x.Id == "2")
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,' - ',CAST(b.`Id` AS CHAR)),a.`BuyerId`=NULL WHERE a.`Id`='2'", sql1);

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
                .SelectAggregate((x, f) => (double)x.Sum(f.Amount)))
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((x, y) => x.Id == "2")
            .Execute();
        order = await repository.QueryByIdAsync<Order>("2");
        orderDetails = await repository.QueryAsync<OrderDetail>(f => f.OrderId == "2");
        await repository.CommitAsync();

        Assert.Greater(result, 0);
        Assert.AreEqual(orderDetails.Where(f => f.OrderId == order.Id).Sum(f => f.Amount), order.TotalAmount);
        Assert.AreEqual(origValues.OrderNo + " - " + origValues.Id.ToString(), order.OrderNo);
        Assert.AreEqual(default(int), order.BuyerId);
    }
    [Test]
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
        Assert.AreEqual("UPDATE `sys_order` SET `BuyerId`=NULL,`Seller`=NULL WHERE `OrderNo` IS NULL", sql);
    }
    [Test]
    public void Update_Set()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var parameter = repository.QueryById<Order>("1");
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
        var order = repository.QueryById<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotEmpty(order.Products);
            Assert.AreEqual(3, order.Products.Count);
            Assert.AreEqual(1, order.Products[0]);
            Assert.AreEqual(2, order.Products[1]);
            Assert.AreEqual(3, order.Products[2]);
            Assert.AreEqual(parameter.TotalAmount, order.TotalAmount);
        }

        repository.BeginTransaction();
        parameter = repository.QueryById<Order>("1");
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
        order = repository.QueryById<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotEmpty(order.Products);
            Assert.AreEqual(3, order.Products.Count);
            Assert.AreEqual(1, order.Products[0]);
            Assert.AreEqual(2, order.Products[1]);
            Assert.AreEqual(3, order.Products[2]);
            Assert.AreEqual(parameter.TotalAmount, order.TotalAmount);
        }
    }
    [Test]
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
        var order = repository.QueryById<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotEmpty(order.Products);
            Assert.AreEqual(3, order.Products.Count);
            Assert.AreEqual(1, order.Products[0]);
            Assert.AreEqual(2, order.Products[1]);
            Assert.AreEqual(3, order.Products[2]);
        }
    }
    [Test]
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
        var order = repository.QueryById<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotEmpty(order.Products);
            Assert.AreEqual(3, order.Products.Count);
            Assert.AreEqual(1, order.Products[0]);
            Assert.AreEqual(2, order.Products[1]);
            Assert.AreEqual(3, order.Products[2]);
        }
    }
    [Test]
    public void Update_Enum_Fields()
    {
        this.Initialize(1);
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
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1", sql);
        Assert.AreEqual("@p0", parameters[0].ParameterName);
        Assert.That(parameters[0].Value, Is.TypeOf<double>());
        Assert.AreEqual(200.56, (double)parameters[0].Value);
        Assert.AreEqual("@Products", parameters[1].ParameterName);
        Assert.That(parameters[1].Value, Is.TypeOf<string>());
        Assert.AreEqual(new JsonTypeHandler().ToFieldValue(new List<int> { 1, 2, 3 }).ToString(), (string)parameters[1].Value);

        var sql1 = repository.Update<User>()
            .Set(new { Gender = Gender.Male })
            .WhereBy(new { Id = 1 })
            .ToSql(out var parameters1);
        Assert.AreEqual("UPDATE `sys_user` SET `Gender`=@Gender WHERE `Id`=@kId", sql1);
        Assert.AreEqual("@Gender", parameters1[0].ParameterName);
        Assert.That(parameters1[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(Gender.Male.ToString(), (string)parameters1[0].Value);

        var sql2 = repository.Update<User>()
            .Set(f => new { Gender = Gender.Male })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters2);
        Assert.AreEqual("UPDATE `sys_user` SET `Gender`=@p0 WHERE `Id`=1", sql2);
        Assert.AreEqual("@p0", parameters2[0].ParameterName);
        Assert.That(parameters2[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(Gender.Male.ToString(), (string)parameters2[0].Value);

        var user = new User { Gender = Gender.Female };
        var sql3 = repository.Update<User>()
            .Set(new { user.Gender })
            .Set(f => f.Age, 20)
            .WhereBy(new { Id = 1 })
            .ToSql(out var parameters3);
        Assert.AreEqual("UPDATE `sys_user` SET `Age`=@Age,`Gender`=@Gender WHERE `Id`=@kId", sql3);
        Assert.AreEqual("@Gender", parameters3[1].ParameterName);
        Assert.That(parameters3[1].Value, Is.TypeOf<string>());
        Assert.AreEqual(Gender.Female.ToString(), (string)parameters3[1].Value);

        int age = 20;
        var sql7 = repository.Update<User>()
            .Set(new { Gender = Gender.Male })
            .Set(f => f.Age, age)
            .WhereBy(new { Id = 1 })
            .ToSql(out var parameters7);
        Assert.AreEqual("UPDATE `sys_user` SET `Age`=@Age,`Gender`=@Gender WHERE `Id`=@kId", sql7);
        Assert.AreEqual("@Gender", parameters7[1].ParameterName);
        Assert.That(parameters7[1].Value, Is.TypeOf<string>());
        Assert.AreEqual(Gender.Male.ToString(), (string)parameters7[1].Value);

        var sql4 = repository.Update<Company>()
            .Set(new { Nature = CompanyNature.Internet })
            .WhereBy(new { Id = 1 })
            .ToSql(out var parameters4);
        Assert.AreEqual("UPDATE `sys_company` SET `Nature`=@Nature WHERE `Id`=@kId", sql4);
        Assert.AreEqual("@Nature", parameters4[0].ParameterName);
        Assert.That(parameters4[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters4[0].Value);

        var sql5 = repository.Update<Company>()
            .Set(f => new { Nature = CompanyNature.Internet })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters5);
        Assert.AreEqual("UPDATE `sys_company` SET `Nature`=@p0 WHERE `Id`=1", sql5);
        Assert.AreEqual("@p0", parameters5[0].ParameterName);
        Assert.That(parameters5[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters5[0].Value);

        var sql6 = repository.Update<Company>()
            .Set(f => f.Nature, CompanyNature.Internet)
            .WhereBy(new { Id = 1 })
            .ToSql(out var parameters6);
        Assert.AreEqual("UPDATE `sys_company` SET `Nature`=@Nature WHERE `Id`=@kId", sql6);
        Assert.AreEqual("@Nature", parameters6[0].ParameterName);
        Assert.That(parameters6[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters6[0].Value);

        var company = new Company { Name = "facebook", Nature = CompanyNature.Internet };
        var sql8 = repository.Update<Company>()
            .Set(f => new { Name = f.Name + "_New", company.Nature })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters8);
        Assert.AreEqual("UPDATE `sys_company` SET `Name`=CONCAT(`Name`,'_New'),`Nature`=@p0 WHERE `Id`=1", sql8);
        Assert.AreEqual("@p0", parameters8[0].ParameterName);
        Assert.That(parameters8[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters8[0].Value);

        //批量表达式部分栏位更新
        var sql9 = repository.Update<Company>()
            .SetBulk(new[] { new { Id = 1, Name = "google" }, new { Id = 2, Name = "facebook" } })
            .Set(new { company.Nature })
            .ToSql(out var parameters9);
        Assert.AreEqual("UPDATE `sys_company` SET `Nature`=@Nature,`Name`=@Name0 WHERE `Id`=@kId0;UPDATE `sys_company` SET `Nature`=@Nature,`Name`=@Name1 WHERE `Id`=@kId1", sql9);
        Assert.AreEqual(5, parameters9.Count);
        Assert.AreEqual("@Nature", parameters9[0].ParameterName);
        Assert.That(parameters9[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters9[0].Value);

        CompanyNature? nature = CompanyNature.Production;
        var sql10 = repository.Update<Company>()
            .SetBulk(new[] { new { Id = 1, company.Nature }, new { Id = 2, Nature = nature } })
            .Set(f => new { company.Name })
            .OnlyFields(f => f.Nature)
            .ToSql(out var parameters10);
        Assert.AreEqual("UPDATE `sys_company` SET `Name`=@p0,`Nature`=@Nature0 WHERE `Id`=@kId0;UPDATE `sys_company` SET `Name`=@p0,`Nature`=@Nature1 WHERE `Id`=@kId1", sql10);
        Assert.AreEqual("@Nature0", parameters10[1].ParameterName);
        Assert.That(parameters10[1].Value, Is.TypeOf<string>());
        Assert.AreEqual(company.Nature.ToString(), (string)parameters10[1].Value);
        Assert.AreEqual("@Nature1", parameters10[3].ParameterName);
        Assert.That(parameters10[3].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Production.ToString(), (string)parameters10[3].Value);
        Assert.AreEqual("@p0", parameters10[0].ParameterName);
        Assert.That(parameters10[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(company.Name, (string)parameters10[0].Value);
    }
    [Test]
    public async Task Update_TimeSpan_Fields()
    {
        var repository = this.dbFactory.Create();
        var timeSpan = TimeSpan.FromMinutes(455);
        await repository.DeleteByIdAsync<UpdateEntity1>(1);
        await repository.CreateAsync<UpdateEntity1>(new UpdateEntity1
        {
            Id = 1,
            BooleanField = true,
            TimeSpanField = TimeSpan.FromSeconds(456),
#if NET6_0_OR_GREATER
            DateOnlyField = new DateOnly(2022, 05, 06),
#else
            DateOnlyField = new DateTime(2022, 05, 06),
#endif
            DateTimeField = DateTime.Now,
            DateTimeOffsetField = new DateTimeOffset(DateTime.Parse("2022-01-02 03:04:05")),
            EnumField = Gender.Male,
            GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
            TimeOnlyField = new TimeOnly(3, 5, 7)
#else
            TimeOnlyField = new TimeSpan(3, 5, 7)
#endif
        });
        var sql1 = repository.Update<User>()
            .Set(new { SomeTimes = timeSpan })
            .WhereBy(new { Id = 1 })
            .ToSql(out var parameters1);
        Assert.AreEqual("UPDATE `sys_user` SET `SomeTimes`=@SomeTimes WHERE `Id`=@kId", sql1);
        Assert.AreEqual("@SomeTimes", parameters1[0].ParameterName);

#if NET6_0_OR_GREATER
        Assert.That(parameters1[0].Value, Is.TypeOf<TimeOnly>());
        Assert.AreEqual(TimeOnly.FromTimeSpan(timeSpan), (TimeOnly)parameters1[0].Value);
#else
        Assert.That(parameters1[0].Value, Is.TypeOf<TimeSpan>());
        Assert.AreEqual(timeSpan, (TimeSpan)parameters1[0].Value);
#endif

        var sql2 = repository.Update<User>()
            .Set(f => new { SomeTimes = timeSpan })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters2);
        Assert.AreEqual("UPDATE `sys_user` SET `SomeTimes`=@p0 WHERE `Id`=1", sql2);
        Assert.AreEqual("@p0", parameters2[0].ParameterName);
#if NET6_0_OR_GREATER
        Assert.That(parameters2[0].Value, Is.TypeOf<TimeOnly>());
        Assert.AreEqual(TimeOnly.FromTimeSpan(timeSpan), (TimeOnly)parameters2[0].Value);
#else
        Assert.That(parameters2[0].Value, Is.TypeOf<TimeSpan>());
        Assert.AreEqual(timeSpan, (TimeSpan)parameters2[0].Value);
#endif
        repository.BeginTransaction();
        await repository.Update<User>()
            .Set(new { SomeTimes = timeSpan })
            .WhereBy(new { Id = 1 })
            .ExecuteAsync();
        var userInfo = repository.QueryById<User>(1);
        repository.Commit();
#if NET6_0_OR_GREATER
        Assert.AreEqual(TimeOnly.FromTimeSpan(timeSpan), userInfo.SomeTimes.Value);
#else
        Assert.AreEqual(timeSpan, userInfo.SomeTimes.Value);
#endif
    }
    [Test]
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

        Assert.AreEqual(orders.Count, count);
    }
    private double CalcAmount(double price, double amount) => price * amount - 150;
    private int[] GetProducts() => new int[] { 1, 2, 3 };




    [Test]
    public void IsEntityType()
    {
        Assert.IsFalse(typeof(Sex).IsEntityType(out _));
        Assert.IsFalse(typeof(Sex?).IsEntityType(out _));
        Assert.IsTrue(typeof(Studuent).IsEntityType(out _));
        Assert.IsFalse(typeof(string).IsEntityType(out _));
        Assert.IsFalse(typeof(int).IsEntityType(out _));
        Assert.IsFalse(typeof(int?).IsEntityType(out _));
        Assert.IsFalse(typeof(Guid).IsEntityType(out _));
        Assert.IsFalse(typeof(Guid?).IsEntityType(out _));
        Assert.IsFalse(typeof(DateTime).IsEntityType(out _));
        Assert.IsFalse(typeof(DateTime?).IsEntityType(out _));
        Assert.IsFalse(typeof(byte[]).IsEntityType(out _));
        Assert.IsFalse(typeof(int[]).IsEntityType(out _));
        Assert.IsFalse(typeof(List<int>).IsEntityType(out _));
        Assert.IsFalse(typeof(List<int[]>).IsEntityType(out _));
        Assert.IsFalse(typeof(Collection<string>).IsEntityType(out _));
        Assert.IsFalse(typeof(DBNull).IsEntityType(out _));

        var vt1 = ("kevin");
        Assert.IsFalse(vt1.GetType().IsEntityType(out _));
        var vt2 = (1, "kevin", 25, 30000.00d);
        Assert.IsTrue(vt2.GetType().IsEntityType(out _));
        Assert.IsTrue(typeof((string Name, int Age)).IsEntityType(out _));
        Assert.IsTrue(typeof(Dictionary<string, int>).IsEntityType(out _));
        Assert.IsTrue(typeof(Studuent).IsEntityType(out _));
        Assert.IsTrue(typeof(Teacher).IsEntityType(out _));

        Assert.IsTrue(typeof(Dictionary<string, int>[]).IsEntityType(out _));
        Assert.IsTrue(typeof(List<Dictionary<string, int>>).IsEntityType(out _));
        Assert.IsTrue(typeof(List<Dictionary<string, int>[]>).IsEntityType(out _));
        Assert.IsTrue(typeof(Collection<Dictionary<string, int>>).IsEntityType(out _));
        Assert.IsTrue(typeof(Dictionary<string, Dictionary<string, int>>).IsEntityType(out _));

        Assert.IsTrue(typeof(Teacher[]).IsEntityType(out _));
        Assert.IsTrue(typeof(List<Teacher>).IsEntityType(out _));
        Assert.IsTrue(typeof(List<Teacher[]>).IsEntityType(out _));
        Assert.IsTrue(typeof(Collection<Teacher>).IsEntityType(out _));
        Assert.IsTrue(typeof(Dictionary<string, Teacher>).IsEntityType(out _));

        Assert.IsTrue(typeof(Studuent[]).IsEntityType(out _));
        Assert.IsTrue(typeof(List<Studuent>).IsEntityType(out _));
        Assert.IsTrue(typeof(List<Studuent[]>).IsEntityType(out _));
        Assert.IsTrue(typeof(Collection<Studuent>).IsEntityType(out _));
        Assert.IsTrue(typeof(Dictionary<string, Studuent>).IsEntityType(out _));
    }
    [Test]
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
        Assert.AreEqual(1, count);
        count = await repository.DeleteAsync<User>(f => f.Id == 1);
        repository.Commit();
        Assert.AreEqual(1, count);

        var sql = repository.Delete<User>()
            .Where(f => f.Id == 1)
            .ToSql(out var parameters);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Id`=1", sql);
    }
    [Test]
    public async Task Delete_Multi()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.DeleteByIds<User>(new[] { new { Id = 1 }, new { Id = 2 } });
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
        Assert.AreEqual(2, count);
        count = await repository.DeleteByIdsAsync<User>(new[] { new { Id = 1 }, new { Id = 2 } });
        repository.Commit();
        Assert.AreEqual(2, count);

        var sql = repository.Delete<User>()
            .WhereByIds(new[] { new { Id = 1 }, new { Id = 2 } })
            .ToSql(out var parameters);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Id` IN (@Id0,@Id1)", sql);
        Assert.AreEqual(1, (int)parameters[0].Value);
        Assert.AreEqual(2, (int)parameters[1].Value);

        var sql1 = repository.Delete<Function>()
            .WhereByIds(new[] { new { MenuId = 1, PageId = 1 }, new { MenuId = 2, PageId = 2 } })
            .ToSql(out parameters);
        Assert.AreEqual("DELETE FROM `sys_function` WHERE `MenuId`=@MenuId0 AND `PageId`=@PageId0 OR `MenuId`=@MenuId1 AND `PageId`=@PageId1", sql1);
        Assert.AreEqual(4, parameters.Count);
        Assert.AreEqual(1, (int)parameters[0].Value);
        Assert.AreEqual(1, (int)parameters[1].Value);
        Assert.AreEqual(2, (int)parameters[2].Value);
        Assert.AreEqual(2, (int)parameters[3].Value);
    }
    [Test]
    public async Task Delete_Multi1()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.DeleteByIds<User>(new[] { 1, 2 });
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
        Assert.AreEqual(2, count);
        count = await repository.DeleteByIdsAsync<User>(new int[] { 1, 2 });
        repository.Commit();
        Assert.AreEqual(2, count);

        var sql = repository.Delete<User>()
            .WhereByIds(new int[] { 1, 2 })
            .ToSql(out var parameters);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Id` IN (@Id0,@Id1)", sql);
        Assert.AreEqual(1, (int)parameters[0].Value);
        Assert.AreEqual(2, (int)parameters[1].Value);

        var orderNos = new string[] { "ON_001", "ON_002", "ON_003" };
        sql = repository.Delete<Order>()
            .Where(f => f.BuyerId == 1 && orderNos.Contains(f.OrderNo))
            .ToSql(out parameters);
        Assert.AreEqual("DELETE FROM `sys_order` WHERE `BuyerId`=1 AND `OrderNo` IN (@p0,@p1,@p2)", sql);
        Assert.AreEqual(orderNos[0], (string)parameters[0].Value);
        Assert.AreEqual(orderNos[1], (string)parameters[1].Value);
        Assert.AreEqual(orderNos[2], (string)parameters[2].Value);
    }
    [Test]
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
        Assert.AreEqual(2, count);
        count = await repository.DeleteAsync<User>(f => new int[] { 1, 2 }.Contains(f.Id));
        repository.Commit();
        Assert.AreEqual(2, count);

        var sql = repository.Delete<User>()
           .Where(f => new int[] { 1, 2 }.Contains(f.Id))
           .ToSql(out var parameters);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Id` IN (1,2)", sql);
        //Assert.IsTrue((int)parameters[0].Value == 1);
        //Assert.IsTrue((int)parameters[1].Value == 2);
    }
    [Test]
    public void Delete_Where_And()
    {
        var repository = this.dbFactory.Create();
        bool? isMale = true;
        var sql = repository.Delete<User>()
            .Where(f => f.Name.Contains("kevin"))
            .And(isMale.HasValue, f => f.Age > 25)
            .ToSql(out _);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Name` LIKE '%kevin%' AND `Age`>25", sql);
    }
    [Test]
    public void Delete_Enum_Fields()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.Delete<User>()
            .Where(f => f.Gender == Gender.Male)
            .ToSql(out _);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Gender`='Male'", sql1);

        var gender = Gender.Male;
        var sql2 = repository.Delete<User>()
            .Where(f => f.Gender == gender)
            .ToSql(out var parameters1);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Gender`=@p0", sql2);
        Assert.AreEqual("@p0", parameters1[0].ParameterName);
        Assert.That(parameters1[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(gender.ToString(), (string)parameters1[0].Value);

        var sql3 = repository.Delete<Company>()
             .Where(f => f.Nature == CompanyNature.Internet)
             .ToSql(out _);
        Assert.AreEqual("DELETE FROM `sys_company` WHERE `Nature`='Internet'", sql3);

        var nature = CompanyNature.Internet;
        var sql4 = repository.Delete<Company>()
             .Where(f => f.Nature == nature)
             .ToSql(out var parameters2);
        Assert.AreEqual("DELETE FROM `sys_company` WHERE `Nature`=@p0", sql4);
        Assert.AreEqual("@p0", parameters2[0].ParameterName);
        Assert.That(parameters2[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters2[0].Value);
    }
    [Test]
    public void Transation()
    {
        var repository = this.dbFactory.Create();
        bool? isMale = true;
        repository.BeginTransaction();
        repository.Update<User>()
            .Set(new { Name = "leafkevin1" })
            .WhereById(new { Id = 1 })
            .Execute();
        repository.Update<User>(new { Name = "leafkevin1", Id = 1 });
        repository.Delete<User>()
            .Where(f => f.Name.Contains("kevin"))
            .And(isMale.HasValue, f => f.Age > 25)
            .Execute();
        repository.Commit();
    }





    [Test]
    public async Task MultipleQuery()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        using var reader = await repository.QueryMultipleAsync(f => f
            .QueryById<User>(new { Id = 1 })
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
            .FromQuery(f => f.From<Order, OrderDetail>('a')
                    .Where((a, b) => a.Id == b.OrderId && a.Id == "1")
                    .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                    .Having((x, a, b) => x.CountDistinct(b.ProductId) > 0)
                    .Select((x, a, b) => new { a.Id, x.Grouping, ProductTotal = x.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
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
        Assert.IsNotNull(userInfo);
        Assert.AreEqual(1, userInfo.Id);
        //Assert.AreEqual("1", orderInfo.Id);
        //Assert.AreEqual("1", groupedOrderInfo.Id);
        //Assert.AreEqual("1", groupedOrderInfo.Grouping.OrderId);
    }
    [Test]
    public async Task MultipleQuery_UseMaster()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        using var reader = await repository.UseMaster().QueryMultipleAsync(f => f
            .QueryById<User>(new { Id = 1 })
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
            .FromQuery(f => f.From<Order, OrderDetail>('a')
                    .Where((a, b) => a.Id == b.OrderId && a.Id == "1")
                    .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                    .Having((x, a, b) => x.CountDistinct(b.ProductId) > 0)
                    .Select((x, a, b) => new { a.Id, x.Grouping, ProductTotal = x.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
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
        Assert.IsNotNull(userInfo);
        Assert.AreEqual(1, userInfo.Id);
        //Assert.AreEqual("1", orderInfo.Id);
        //Assert.AreEqual("1", groupedOrderInfo.Id);
        //Assert.AreEqual("1", groupedOrderInfo.Grouping.OrderId);
    }
    //[Test]
    //public async Task MultipleCommand()
    //{
    //    var repository = this.dbFactory.Create();
    //    int[] productIds = new int[] { 2, 4, 5, 6 };
    //    int category = 1;
    //    await repository.Delete<Product>()
    //        .Where(f => productIds.Contains(f.Id))
    //        .ExecuteAsync();

    //    await repository.Create<Product>()
    //       .WithBy(new
    //       {
    //           Id = 2,
    //           ProductNo = "PN_111",
    //           Name = "PName_111",
    //           BrandId = 1,
    //           CategoryId = category,
    //           CompanyId = 1,
    //           IsEnabled = true,
    //           CreatedBy = 1,
    //           CreatedAt = DateTime.Now,
    //           UpdatedBy = 1,
    //           UpdatedAt = DateTime.Now
    //       })
    //       .ExecuteAsync();

    //    var insertCommand2 = repository.Create<Product>()
    //        .WithBulk(new[]
    //        {
    //            new
    //            {
    //                Id = 4,
    //                ProductNo="PN-004",
    //                Name = "波司登羽绒服",
    //                BrandId = 1,
    //                CategoryId = 1,
    //                IsEnabled = true,
    //                CreatedAt = DateTime.Now,
    //                CreatedBy = 1,
    //                UpdatedAt = DateTime.Now,
    //                UpdatedBy = 1
    //            },
    //            new
    //            {
    //                Id = 5,
    //                ProductNo="PN-005",
    //                Name = "雪中飞羽绒裤",
    //                BrandId = 2,
    //                CategoryId = 2,
    //                IsEnabled = true,
    //                CreatedAt = DateTime.Now,
    //                CreatedBy = 1,
    //                UpdatedAt = DateTime.Now,
    //                UpdatedBy = 1
    //            },
    //            new
    //            {
    //                Id = 6,
    //                ProductNo="PN-006",
    //                Name = "优衣库保暖内衣",
    //                BrandId = 3,
    //                CategoryId = 3,
    //                IsEnabled = true,
    //                CreatedAt = DateTime.Now,
    //                CreatedBy = 1,
    //                UpdatedAt = DateTime.Now,
    //                UpdatedBy = 1
    //            }
    //        })
    //        .OnlyFields(f => new { f.Id, f.ProductNo, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
    //        .ToMultipleCommand();

    //    var updateCommand = repository.Update<Order>()
    //       .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
    //       .Set(true, (x, y) => new
    //       {
    //           TotalAmount = 200.56,
    //           OrderNo = x.OrderNo + "-111",
    //           BuyerSource = y.SourceType
    //       })
    //       .Set(x => x.Products, new List<int> { 1, 2, 3 })
    //       .Where((a, b) => a.Id == "1")
    //       .ToMultipleCommand();

    //    var orderDetails = await repository.From<OrderDetail>().ToListAsync();
    //    var parameters = orderDetails.Select(f => new
    //    {
    //        f.Id,
    //        Amount = f.Amount + 50,
    //        UpdatedAt = f.UpdatedAt.AddDays(1)
    //    })
    //    .ToList();
    //    var bulkUpdateCommand = repository.Update<OrderDetail>()
    //        .SetBulk(parameters)
    //        .Set(f => f.ProductId, 3)
    //        .Set(new { Quantity = 5 })
    //        .Set(f => new { Price = f.Price + 10 })
    //        .ToMultipleCommand();

    //    commands.AddRange(new[] { deleteCommand, insertCommand, insertCommand2, updateCommand, bulkUpdateCommand });
    //    var count = repository.MultipleExecute(commands);
    //    Assert.IsTrue(count > 0);
    //}




    [Test]
    public async Task Create_WithBy_UseTable()
    {
        var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .WhereById(101)
            .ExecuteAsync();
        var count = repository.UseMaster()
            .From<User>()
            .UseTable("sys_user_104")
            .Where(f => f.Id == 101)
            .Count();
        Assert.AreEqual(0, count);

        repository.Create<User>()
            .UseTable("sys_user_104")
            .WithBy(new
            {
                Id = 101,
                TenantId = "104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2024-05-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2024-05-15 16:27:38"),
                UpdatedBy = 1
            })
            .Execute();
        var result = repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .First();
        Assert.IsNotNull(result);
        Assert.AreEqual("104", result.TenantId);
    }
    [Test]
    public async Task Create_WithBy_WithoutUseTable()
    {
        var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .WhereById(101)
            .ExecuteAsync();
        var count = repository.UseMaster()
            .From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .Count();
        Assert.AreEqual(0, count);

        await repository.Create<User>()
            .UseTableBy("104")
            .WithBy(new
            {
                Id = 101,
                TenantId = "104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                UpdatedBy = 1
            })
            .ExecuteAsync();
        var result = await repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .FirstAsync();
        Assert.IsNotNull(result);
        Assert.AreEqual("104", result.TenantId);
    }
    [Test]
    public async Task Create_WithBulk_UseTable()
    {
        var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .WhereById(101)
            .ExecuteAsync();
        var count = repository.UseMaster()
            .From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .Count();
        Assert.AreEqual(0, count);

        repository.Create<User>()
            .UseTableBy("104")
            .WithBulk(new[]{new
            {
                Id = 101,
                TenantId = "104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                UpdatedBy = 1
            }})
            .Execute();
        var result = repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .First();
        Assert.IsNotNull(result);
        Assert.AreEqual("104", result.TenantId);
    }
    [Test]
    public async Task Create_WithoutUseTable()
    {
        var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .UseTableBy("105")
            .WhereByIds(new object[] { 101, 102, 103 })
            .ExecuteAsync();
        await repository.CreateAsync<User>(new
        {
            Id = 102,
            TenantId = "105",
            Name = "cindy",
            Age = 21,
            CompanyId = 2,
            Gender = Gender.Female,
            GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
            SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
#else
            SomeTimes = TimeSpan.FromSeconds(5730),
#endif
            SourceType = UserSourceType.Taobao,
            IsEnabled = true,
            CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
            CreatedBy = 1,
            UpdatedAt = DateTime.Now,
            UpdatedBy = 1
        });

        await repository.CreateAsync<User>(new[]
        {
            new
            {
                Id = 101,
                TenantId ="104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
            SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
            SomeTimes = TimeSpan.FromSeconds(4769),
#endif
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                UpdatedBy = 1
            },
            new
            {
                Id = 103,
                TenantId ="105",
                Name = "xiyuan",
                Age = 17,
                CompanyId = 3,
                Gender = Gender.Female,
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
            SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
#else
            SomeTimes = TimeSpan.FromSeconds(5730),
#endif
                SourceType = UserSourceType.Taobao,
                IsEnabled = true,
                CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });
        var result = await repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .FirstAsync();
        Assert.IsNotNull(result);
        Assert.AreEqual("104", result.TenantId);

        result = await repository.From<User>()
           .UseTableBy("105")
           .Where(f => f.Id == 102)
           .FirstAsync();
        Assert.IsNotNull(result);
        Assert.AreEqual("105", result.TenantId);

        result = await repository.From<User>()
           .UseTableBy("105")
           .Where(f => f.Id == 103)
           .FirstAsync();
        Assert.IsNotNull(result);
        Assert.AreEqual("105", result.TenantId);
    }
    [Test]
    public async Task Create_WithBulk_WithoutUseTable()
    {
        var repository = this.dbFactory.Create();
        var userIds = new[] { 101, 102, 103 };
        await repository.Delete<User>()
            .UseTable("sys_user_104", "sys_user_105")
            .WhereByIds(userIds)
            .ExecuteAsync();
        await repository.Create<User>()
            .WithBulk(new[]
            {
                new User
                {
                    Id = 101,
                    TenantId ="104",
                    Name = "leafkevin",
                    Age = 25,
                    CompanyId = 1,
                    Gender = Gender.Male,
                    GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                    SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                    SomeTimes = TimeSpan.FromSeconds(4769),
#endif
                    SourceType = UserSourceType.Douyin,
                    IsEnabled = true,
                    CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                    CreatedBy = 1,
                    UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                    UpdatedBy = 1
                },
                new User
                {
                    Id = 102,
                    TenantId ="105",
                    Name = "cindy",
                    Age = 21,
                    CompanyId = 2,
                    Gender = Gender.Female,
                    GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                    SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
#else
                    SomeTimes = TimeSpan.FromSeconds(5730),
#endif
                    SourceType = UserSourceType.Taobao,
                    IsEnabled = true,
                    CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
                    CreatedBy = 1,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = 1
                },
                new User
                {
                    Id = 103,
                    TenantId ="105",
                    Name = "xiyuan",
                    Age = 17,
                    CompanyId = 3,
                    Gender = Gender.Female,
                    GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                    SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
#else
                    SomeTimes = TimeSpan.FromSeconds(5730),
#endif
                    SourceType = UserSourceType.Taobao,
                    IsEnabled = true,
                    CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
                    CreatedBy = 1,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = 1
                }
            })
            .ExecuteAsync();

        var result = await repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .FirstAsync();
        Assert.IsNotNull(result);
        Assert.AreEqual("104", result.TenantId);

        var result1 = await repository.From<User>()
            .UseTableBy("105")
            .Where(f => userIds.Contains(f.Id))
            .FirstAsync();
        Assert.IsNotNull(result);
        Assert.AreEqual("104", result.TenantId);
    }
    [Test]
    public async Task Create_BulkCopy_UseTable()
    {
        var createdAt = DateTime.Parse("2024-05-24");
        var orders = new List<Order>();
        var orderDetails = new List<OrderDetail>();
        for (int i = 1000; i < 2000; i++)
        {
            var orderId = $"ON_{i + 1}";
            orders.Add(new Order
            {
                Id = orderId,
                TenantId = "104",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 101,
                SellerId = 2,
                TotalAmount = 420,
                ProductCount = 2,
                Products = new List<int> { 1, 2 },
                Disputes = new Dispute
                {
                    Id = i + 1,
                    Content = "无良商家",
                    Result = "同意退款",
                    Users = "Buyer2,Seller2",
                    CreatedAt = createdAt
                },
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{1000 + (i - 1000) * 2 + 1}",
                TenantId = "104",
                Amount = 240,
                OrderId = orderId,
                Price = 120,
                ProductId = 11,
                Quantity = 2,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{1000 + (i - 1000) * 2 + 2}",
                TenantId = "104",
                Amount = 180,
                OrderId = orderId,
                Price = 180,
                ProductId = 12,
                Quantity = 1,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
        }
        var repository = this.dbFactory.Create();
        var removeIds = orders.Select(f => f.Id).ToList();

        await repository.BeginTransactionAsync();
        await repository.Delete<Order>()
           .UseTableBy("104", createdAt)
           .Where(f => removeIds.Contains(f.Id))
           .ExecuteAsync();
        await repository.Delete<OrderDetail>()
           .UseTableBy("104", createdAt)
           .Where(f => removeIds.Contains(f.OrderId))
           .ExecuteAsync();
        var count1 = await repository.Create<Order>()
            .UseTableBy("104", createdAt)
            .WithBulkCopy(orders)
            .ExecuteAsync();
        var count2 = await repository.Create<OrderDetail>()
             .UseTableBy("104", createdAt)
             .WithBulkCopy(orderDetails)
             .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(1000, count1);
        Assert.AreEqual(2000, count2);

        orders.Clear();
        orderDetails.Clear();
        for (int i = 2000; i < 3000; i++)
        {
            var orderId = $"ON_{i + 1}";
            orders.Add(new Order
            {
                Id = orderId,
                TenantId = "105",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 102,
                SellerId = 2,
                TotalAmount = 630,
                ProductCount = 2,
                Products = [1, 2],
                Disputes = new Dispute
                {
                    Id = i + 1,
                    Content = "无良商家",
                    Result = "同意退款",
                    Users = "Buyer2,Seller2",
                    CreatedAt = DateTime.Now
                },
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{2000 + (i - 2000) * 2 + 1}",
                TenantId = "105",
                Amount = 230,
                OrderId = orderId,
                Price = 230,
                ProductId = 13,
                Quantity = 1,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{2000 + (i - 2000) * 2 + 2}",
                TenantId = "105",
                Amount = 400,
                OrderId = orderId,
                Price = 200,
                ProductId = 14,
                Quantity = 2,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
        }

        removeIds = orders.Select(f => f.Id).ToList();
        await repository.BeginTransactionAsync();
        await repository.Delete<Order>()
            .UseTableBy("105", createdAt)
            .Where(f => removeIds.Contains(f.Id))
            .ExecuteAsync();
        await repository.Delete<OrderDetail>()
            .UseTableBy("105", createdAt)
            .Where(f => removeIds.Contains(f.OrderId))
            .ExecuteAsync();

        count1 = await repository.Create<Order>()
            .UseTableBy("105", createdAt)
            .WithBulkCopy(orders)
            .ExecuteAsync();
        count2 = await repository.Create<OrderDetail>()
            .UseTableBy("105", createdAt)
            .WithBulkCopy(orderDetails)
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(1000, count1);
        Assert.AreEqual(2000, count2);
    }
    [Test]
    public async Task Create_BulkCopy_WithoutUseTable()
    {
        var createdAt = DateTime.Parse("2024-05-24");
        var orders = new List<Order>();
        var orderDetails = new List<OrderDetail>();
        for (int i = 1000; i < 2000; i++)
        {
            var orderId = $"ON_{i + 1}";
            orders.Add(new Order
            {
                Id = orderId,
                TenantId = "104",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 101,
                SellerId = 2,
                TotalAmount = 420,
                ProductCount = 2,
                Products = new List<int> { 1, 2 },
                Disputes = new Dispute
                {
                    Id = i + 1,
                    Content = "无良商家",
                    Result = "同意退款",
                    Users = "Buyer2,Seller2",
                    CreatedAt = createdAt
                },
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{1000 + (i - 1000) * 2 + 1}",
                TenantId = "104",
                Amount = 240,
                OrderId = orderId,
                Price = 120,
                ProductId = 11,
                Quantity = 2,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{1000 + (i - 1000) * 2 + 2}",
                TenantId = "104",
                Amount = 180,
                OrderId = orderId,
                Price = 180,
                ProductId = 12,
                Quantity = 1,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
        }
        for (int i = 2000; i < 3000; i++)
        {
            var orderId = $"ON_{i + 1}";
            orders.Add(new Order
            {
                Id = orderId,
                TenantId = "105",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 102,
                SellerId = 2,
                TotalAmount = 630,
                ProductCount = 2,
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
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{2000 + (i - 2000) * 2 + 1}",
                TenantId = "105",
                Amount = 230,
                OrderId = orderId,
                Price = 230,
                ProductId = 13,
                Quantity = 1,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{2000 + (i - 2000) * 2 + 2}",
                TenantId = "105",
                Amount = 400,
                OrderId = orderId,
                Price = 200,
                ProductId = 14,
                Quantity = 2,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
        }
        var removeIds = orders.Select(f => f.Id).ToList();

        var repository = this.dbFactory.Create();
        await repository.BeginTransactionAsync();
        await repository.Delete<Order>()
            .UseTableBy("104", createdAt)
            .Where(f => removeIds.Contains(f.Id))
            .ExecuteAsync();
        await repository.Delete<Order>()
            .UseTableBy("105", createdAt)
            .Where(f => removeIds.Contains(f.Id))
            .ExecuteAsync();
        await repository.Delete<OrderDetail>()
            .UseTableBy("104", createdAt)
            .Where(f => removeIds.Contains(f.OrderId))
            .ExecuteAsync();
        await repository.Delete<OrderDetail>()
            .UseTableBy("105", createdAt)
            .Where(f => removeIds.Contains(f.OrderId))
            .ExecuteAsync();

        var count1 = await repository.Create<Order>()
            .WithBulkCopy(orders)
            .ExecuteAsync();
        var count2 = await repository.Create<OrderDetail>()
            .WithBulkCopy(orderDetails)
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.AreEqual(2000, count1);
        Assert.AreEqual(4000, count2);
    }
    [Test]
    public async Task Query_ManySharding_SingleTable()
    {
        await this.InitSharding();
        var productCount = 1;
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => f.ProductCount > productCount)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_105_202405` a WHERE a.`ProductCount`>@p0", sql);

        var result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => f.ProductCount > productCount)
            .ToList();
        Assert.IsNotEmpty(result);
        {
            var tenantIds = result.Select(f => f.TenantId).ToList();
            Assert.That(tenantIds, Has.Some.Matches<string>(f => f == "104" || f == "105"));
        }
    }
    [Test]
    public async Task Query_ManySharding_SingleTable_Include()
    {
        await this.InitSharding();
        var productCount = 1;
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Include(f => f.Details)
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToSql(out _);
        Assert.AreEqual("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order%' OR TABLE_NAME LIKE 'sys_order_detail%');SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0", sql);

        var result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Include(f => f.Details)
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToList();
        Assert.IsNotEmpty(result);
        {
            var tenantIds = result.Select(f => f.TenantId).Distinct().ToList();
            Assert.IsFalse(tenantIds.Exists(f => f != "104"));
            foreach (var order in result)
            {
                Assert.IsNotNull(order.Details);
                foreach (var orderDetail in order.Details)
                {
                    Assert.AreEqual("104", orderDetail.TenantId);
                }
            }
        }

        sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Include(f => f.Details)
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToSql(out _);
        Assert.AreEqual("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order_detail%');SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_105_202405` a WHERE a.`ProductCount`>@p0", sql);

        result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Include(f => f.Details)
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToList();
        Assert.IsNotEmpty(result);
        {
            var tenantIds = result.Select(f => f.TenantId).ToList();
            Assert.That(tenantIds, Has.Some.Matches<string>(f => f == "104" || f == "105"));
            foreach (var order in result)
            {
                Assert.IsNotNull(order.Details);
                foreach (var orderDetail in order.Details)
                {
                    Assert.That(orderDetail.TenantId, Is.AnyOf("104", "105"));
                }
            }
        }
    }
    [Test]
    public async Task Query_SingleSharding_Value()
    {
        await this.InitSharding();
        var orderId = "ON_1015";
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-01"))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableBy("104")
            .Where((x, y) => x.Id == orderId)
            .Select((x, y) => new { x.Id, x.OrderNo, x.TenantId, x.BuyerId, BuyerName = y.Name })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`OrderNo`,a.`TenantId`,a.`BuyerId`,b.`Name` AS `BuyerName` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`Id`=@p0", sql);

        var result = await repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-01"))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableBy("104")
            .Where((x, y) => x.Id == orderId)
            .Select((x, y) => new { x.Id, x.OrderNo, x.TenantId, x.BuyerId, BuyerName = y.Name })
            .FirstAsync();
        if (result != null)
        {
            Assert.AreEqual("104", result.TenantId);
        }
    }
    [Test]
    public async Task Query_ManySharding_SingleTable_SubQuery()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var sql = repository
            .FromQuery(f => f.From<OrderDetail>()
                .UseTable("sys_order_detail_104_202405", "sys_order_detail_105_202405")
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .UseTableMap((orderOrigName, userOrigName, orderTableName) => orderTableName.Replace(orderOrigName, userOrigName))
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
                orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                y.TenantId,
                Buyer = y,
                x.ProductCount
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order%' OR TABLE_NAME LIKE 'sys_user%');SELECT a.`OrderId`,a.`BuyerId`,b.`TenantId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail_104_202405` a INNER JOIN `sys_order_104_202405` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1 UNION ALL SELECT a.`OrderId`,a.`BuyerId`,b.`TenantId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail_105_202405` a INNER JOIN `sys_order_105_202405` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1", sql);

        var result = await repository
            .FromQuery(f => f.From<OrderDetail>()
                .UseTable("sys_order_detail_104_202405", "sys_order_detail_105_202405")
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .UseTableMap((orderOrigName, userOrigName, orderTableName) => orderTableName.Replace(orderOrigName, userOrigName))
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName[..^7];
            })
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                y.TenantId,
                Buyer = y,
                x.ProductCount
            })
            .ToListAsync();
        Assert.IsNotEmpty(result);
        {
            Assert.IsNotNull(result[0]);
            Assert.IsNotNull(result[0].Group);
            Assert.IsNotNull(result[0].Buyer);
            Assert.Greater(result[0].ProductCount, 1);
            var tenantIds = result.Select(f => f.TenantId).Distinct().ToList();
            Assert.IsFalse(tenantIds.Exists(f => f != "104" && f != "105"));
        }
    }
    [Test]
    public async Task Query_ManySharding_MultiTable1()
    {
        await this.InitSharding();
        var productCount = 1;
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName[..^7];
            })
            .Where((a, b) => a.ProductCount > productCount)
            .Select((x, y) => new
            {
                Order = x,
                Buyer = y
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_user%');SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0", sql);

        var result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName[..^7];
            })
            .Where((a, b) => a.ProductCount > productCount)
            .Select((x, y) => new
            {
                Order = x,
                Buyer = y
            })
            .ToList();
        Assert.IsNotEmpty(result);
        {
            var tenantIds = result.Select(f => f.Order.TenantId).ToList();
            Assert.That(tenantIds, Has.Some.Matches<string>(f => f == "104" || f == "105"));
        }
    }
    [Test]
    public async Task Query_ManySharding_MultiTable2()
    {
        await this.InitSharding();
        var productCount = 1;
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName[..^7];
            })
            .Where((a, b) => a.ProductCount > productCount)
            .Select((x, y) => new
            {
                Order = x,
                Buyer = y
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order%' OR TABLE_NAME LIKE 'sys_user%');SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202406` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0", sql);

        var result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName[..^7];
            })
            .Where((a, b) => a.ProductCount > productCount)
            .Select((x, y) => new
            {
                Order = x,
                Buyer = y
            })
            .ToList();
        Assert.IsNotEmpty(result);
        {
            var tenantIds = result.Select(f => f.Order.TenantId).ToList();
            Assert.That(tenantIds, Has.Some.Matches<string>(f => f == "104" || f == "105"));
        }
    }
    [Test]
    public async Task Query_ManySharding_MultiTable3()
    {
        await this.InitSharding();
        var productCount = 1;
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .UseTableMap((orderOrigName, orderDetailOrigName, orderTableName) => orderTableName.Replace(orderOrigName, orderDetailOrigName))
            .Where((a, b) => a.ProductCount > productCount)
            .Select((x, y) => new
            {
                Order = x,
                Detail = y
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order%' OR TABLE_NAME LIKE 'sys_order_detail%');SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderId`,b.`ProductId`,b.`Price`,b.`Quantity`,b.`Amount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_order_detail_104_202405` b ON a.`Id`=b.`OrderId` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderId`,b.`ProductId`,b.`Price`,b.`Quantity`,b.`Amount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_order_detail_105_202405` b ON a.`Id`=b.`OrderId` WHERE a.`ProductCount`>@p0", sql);

        var result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .UseTableMap((orderOrigName, orderDetailOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, orderDetailOrigName))
            .Where((a, b) => a.ProductCount > productCount)
            .Select((x, y) => new
            {
                Order = x,
                Detail = y
            })
            .ToList();
        Assert.IsNotEmpty(result);
        {
            var tenantIds = result.Select(f => f.Order.TenantId).Distinct().ToList();
            Assert.That(tenantIds, Has.Some.Matches<string>(f => f == "104" || f == "105"));
        }
    }
    [Test]
    public async Task Query_SingleSharding_Exists1()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104")
                .Where(t => t.Id == f.BuyerId && t.Age < 25)
                .Exists())
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE EXISTS(SELECT * FROM `sys_user_104` b WHERE b.`Id`=a.`BuyerId` AND b.`Age`<25)", sql);

        var result = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104")
                .Where(t => t.Id == f.BuyerId && t.Age < 25)
                .Exists())
            .ToList();
        Assert.IsNotEmpty(result);
        {
            var tenantIds = result.Select(f => f.TenantId).Distinct().ToList();
            Assert.Contains("104", tenantIds);
        }
    }
    [Test]
    public async Task Query_SingleSharding_Exists2()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104")
                .InnerJoin<OrderDetail>((x, y) => f.Id == y.OrderId)
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .Where((x, y) => x.Id == f.BuyerId && x.Age <= 25 && y.Price > 100)
                .Exists())
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE EXISTS(SELECT * FROM `sys_user_104` b INNER JOIN `sys_order_detail_104_202405` c ON a.`Id`=c.`OrderId` WHERE b.`Id`=a.`BuyerId` AND b.`Age`<=25 AND c.`Price`>100)", sql);

        sql = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104")
                .InnerJoin<OrderDetail>((x, y) => f.Id == y.OrderId)
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .Where((x, y) => x.Id == f.BuyerId && x.Age <= 25 && y.Price > 100)
                .Exists())
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE EXISTS(SELECT * FROM `sys_user_104` b INNER JOIN `sys_order_detail_104_202405` c ON a.`Id`=c.`OrderId` WHERE b.`Id`=a.`BuyerId` AND b.`Age`<=25 AND c.`Price`>100)", sql);

        var result = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104")
                .InnerJoin<OrderDetail>((x, y) => f.Id == y.OrderId)
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .Where((x, y) => x.Id == f.BuyerId && x.Age <= 25 && y.Price > 100)
                .Exists())
            .ToList();
        Assert.IsNotEmpty(result);
        {
            var tenantIds = result.Select(f => f.TenantId).Distinct().ToList();
            Assert.Contains("104", tenantIds);
        }
    }
    [Test]
    public async Task Update_SingleSharding()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var orderIds = new string[] { "ON_1001", "ON_1002", "ON_1003", "ON_1004" };
        var sql = repository.Update<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order_104_202405` SET `TotalAmount`=@TotalAmount WHERE `Id` IN (@p1,@p2,@p3,@p4)", sql);
        Assert.AreEqual(400, (double)dbParameters[0].Value);
        Assert.AreEqual(MySqlDbType.Double, ((MySqlParameter)dbParameters[0]).MySqlDbType);
        Assert.AreEqual(orderIds[0], (string)dbParameters[1].Value);
        Assert.AreEqual(orderIds[1], (string)dbParameters[2].Value);
        Assert.AreEqual(orderIds[2], (string)dbParameters[3].Value);
        Assert.AreEqual(orderIds[3], (string)dbParameters[4].Value);

        var result = await repository.Update<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ExecuteAsync();
        Assert.Greater(result, 0);
    }
    [Test]
    public async Task Update_ManySharding1()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var orderIds = new string[] { "ON_1001", "ON_1002", "ON_2003", "ON_2004" };
        var sql = repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order_104_202405` SET `TotalAmount`=@TotalAmount WHERE `Id` IN (@p1,@p2,@p3,@p4);UPDATE `sys_order_105_202405` SET `TotalAmount`=@TotalAmount WHERE `Id` IN (@p1,@p2,@p3,@p4)", sql);
        Assert.AreEqual(400, (double)dbParameters[0].Value);
        Assert.AreEqual(MySqlDbType.Double, ((MySqlParameter)dbParameters[0]).MySqlDbType);
        Assert.AreEqual(orderIds[0], (string)dbParameters[1].Value);
        Assert.AreEqual(orderIds[1], (string)dbParameters[2].Value);
        Assert.AreEqual(orderIds[2], (string)dbParameters[3].Value);
        Assert.AreEqual(orderIds[3], (string)dbParameters[4].Value);

        await repository.BeginTransactionAsync();
        var result = await repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ExecuteAsync();
        var orders = await repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => orderIds.Contains(f.Id))
            .ToListAsync();
        await repository.CommitAsync();

        Assert.Greater(result, 0);
        foreach (var order in orders)
        {
            Assert.AreEqual(400, order.TotalAmount);
            Assert.That(order.TenantId, Is.AnyOf("104", "105"));
            Assert.Contains(order.Id, orderIds);
        }
    }
    [Test]
    public async Task Update_ManySharding2()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var orderIds = new string[] { "ON_1001", "ON_1002", "ON_2003", "ON_2004" };
        var sql = repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ToSql(out var dbParameters);
        //Assert.IsTrue(sql == "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND TABLE_NAME LIKE 'sys_order%';UPDATE `sys_order_105_202405` SET `TotalAmount`=@TotalAmount WHERE `Id` IN (@p1,@p2,@p3,@p4);UPDATE `sys_order_104_202405` SET `TotalAmount`=@TotalAmount WHERE `Id` IN (@p1,@p2,@p3,@p4)");
        Assert.AreEqual(400, (double)dbParameters[0].Value);
        Assert.AreEqual(MySqlDbType.Double, ((MySqlParameter)dbParameters[0]).MySqlDbType);
        Assert.AreEqual(orderIds[0], (string)dbParameters[1].Value);
        Assert.AreEqual(orderIds[1], (string)dbParameters[2].Value);
        Assert.AreEqual(orderIds[2], (string)dbParameters[3].Value);
        Assert.AreEqual(orderIds[3], (string)dbParameters[4].Value);

        await repository.BeginTransactionAsync();
        var result = await repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ExecuteAsync();
        var orders = await repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => orderIds.Contains(f.Id))
            .ToListAsync();
        await repository.CommitAsync();

        Assert.Greater(result, 0);
        foreach (var order in orders)
        {
            Assert.AreEqual(400, order.TotalAmount);
            Assert.That(order.TenantId, Is.AnyOf("104", "105"));
            Assert.Contains(order.Id, orderIds);
        }
    }
    [Test]
    public async Task Update_SetBulk_ManySharding()
    {
        await this.InitSharding();
        var createdAt = DateTime.Parse("2024-05-24");
        var repository = this.dbFactory.Create();
        var orders = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Select(f => new
            {
                f.Id,
                f.OrderNo,
                f.BuyerId,
                f.SellerId,
                TotalAmount = f.TotalAmount + 50,
                ProductCount = 3,
                UpdatedAt = DateTime.Now
            })
            .OrderByDescending(f => f.Id)
            .Take(20)
            .ToList();
        var orderIds = orders.Select(f => f.Id).ToList();

        var sql = repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .SetBulk(orders, 10)
            .Set(f => f.BuyerSource, UserSourceType.Wechat)
            .IgnoreFields(f => new { f.OrderNo, f.BuyerId, f.SellerId })
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount0,`TotalAmount`=@TotalAmount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount0,`TotalAmount`=@TotalAmount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount1,`TotalAmount`=@TotalAmount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount1,`TotalAmount`=@TotalAmount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount2,`TotalAmount`=@TotalAmount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount2,`TotalAmount`=@TotalAmount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount3,`TotalAmount`=@TotalAmount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount3,`TotalAmount`=@TotalAmount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount4,`TotalAmount`=@TotalAmount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount4,`TotalAmount`=@TotalAmount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount5,`TotalAmount`=@TotalAmount5,`UpdatedAt`=@UpdatedAt5 WHERE `Id`=@kId5;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount5,`TotalAmount`=@TotalAmount5,`UpdatedAt`=@UpdatedAt5 WHERE `Id`=@kId5;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount6,`TotalAmount`=@TotalAmount6,`UpdatedAt`=@UpdatedAt6 WHERE `Id`=@kId6;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount6,`TotalAmount`=@TotalAmount6,`UpdatedAt`=@UpdatedAt6 WHERE `Id`=@kId6;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount7,`TotalAmount`=@TotalAmount7,`UpdatedAt`=@UpdatedAt7 WHERE `Id`=@kId7;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount7,`TotalAmount`=@TotalAmount7,`UpdatedAt`=@UpdatedAt7 WHERE `Id`=@kId7;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount8,`TotalAmount`=@TotalAmount8,`UpdatedAt`=@UpdatedAt8 WHERE `Id`=@kId8;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount8,`TotalAmount`=@TotalAmount8,`UpdatedAt`=@UpdatedAt8 WHERE `Id`=@kId8;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount9,`TotalAmount`=@TotalAmount9,`UpdatedAt`=@UpdatedAt9 WHERE `Id`=@kId9;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount9,`TotalAmount`=@TotalAmount9,`UpdatedAt`=@UpdatedAt9 WHERE `Id`=@kId9;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount10,`TotalAmount`=@TotalAmount10,`UpdatedAt`=@UpdatedAt10 WHERE `Id`=@kId10;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount10,`TotalAmount`=@TotalAmount10,`UpdatedAt`=@UpdatedAt10 WHERE `Id`=@kId10;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount11,`TotalAmount`=@TotalAmount11,`UpdatedAt`=@UpdatedAt11 WHERE `Id`=@kId11;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount11,`TotalAmount`=@TotalAmount11,`UpdatedAt`=@UpdatedAt11 WHERE `Id`=@kId11;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount12,`TotalAmount`=@TotalAmount12,`UpdatedAt`=@UpdatedAt12 WHERE `Id`=@kId12;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount12,`TotalAmount`=@TotalAmount12,`UpdatedAt`=@UpdatedAt12 WHERE `Id`=@kId12;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount13,`TotalAmount`=@TotalAmount13,`UpdatedAt`=@UpdatedAt13 WHERE `Id`=@kId13;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount13,`TotalAmount`=@TotalAmount13,`UpdatedAt`=@UpdatedAt13 WHERE `Id`=@kId13;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount14,`TotalAmount`=@TotalAmount14,`UpdatedAt`=@UpdatedAt14 WHERE `Id`=@kId14;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount14,`TotalAmount`=@TotalAmount14,`UpdatedAt`=@UpdatedAt14 WHERE `Id`=@kId14;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount15,`TotalAmount`=@TotalAmount15,`UpdatedAt`=@UpdatedAt15 WHERE `Id`=@kId15;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount15,`TotalAmount`=@TotalAmount15,`UpdatedAt`=@UpdatedAt15 WHERE `Id`=@kId15;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount16,`TotalAmount`=@TotalAmount16,`UpdatedAt`=@UpdatedAt16 WHERE `Id`=@kId16;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount16,`TotalAmount`=@TotalAmount16,`UpdatedAt`=@UpdatedAt16 WHERE `Id`=@kId16;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount17,`TotalAmount`=@TotalAmount17,`UpdatedAt`=@UpdatedAt17 WHERE `Id`=@kId17;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount17,`TotalAmount`=@TotalAmount17,`UpdatedAt`=@UpdatedAt17 WHERE `Id`=@kId17;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount18,`TotalAmount`=@TotalAmount18,`UpdatedAt`=@UpdatedAt18 WHERE `Id`=@kId18;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount18,`TotalAmount`=@TotalAmount18,`UpdatedAt`=@UpdatedAt18 WHERE `Id`=@kId18;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount19,`TotalAmount`=@TotalAmount19,`UpdatedAt`=@UpdatedAt19 WHERE `Id`=@kId19;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount19,`TotalAmount`=@TotalAmount19,`UpdatedAt`=@UpdatedAt19 WHERE `Id`=@kId19", sql);

        await repository.BeginTransactionAsync();
        var result = await repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .SetBulk(orders, 10)
            .Set(f => f.BuyerSource, UserSourceType.Wechat)
            .IgnoreFields(f => new { f.OrderNo, f.BuyerId, f.SellerId })
            .ExecuteAsync();
        var updatedOrders = await repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => orderIds.Contains(f.Id))
            .ToListAsync();
        await repository.CommitAsync();
        orders.Sort((x, y) => x.Id.CompareTo(y.Id));
        updatedOrders.Sort((x, y) => x.Id.CompareTo(y.Id));
        Assert.Greater(result, 0);
        for (int i = 0; i < orders.Count; i++)
        {
            Assert.AreEqual(updatedOrders[i].TotalAmount, orders[i].TotalAmount);
            Assert.AreEqual(updatedOrders[i].ProductCount, orders[i].ProductCount);

            Assert.AreEqual(updatedOrders[i].OrderNo, orders[i].OrderNo);
            Assert.AreEqual(updatedOrders[i].BuyerId, orders[i].BuyerId);
            Assert.AreEqual(updatedOrders[i].SellerId, orders[i].SellerId);
            Assert.That(updatedOrders[i].TenantId, Is.AnyOf("104", "105"));
        }
    }
    [Test]
    public async Task Update_BulkCopy_ManySharding()
    {
        await this.InitSharding();
        var createdAt = DateTime.Parse("2024-05-24");
        var repository = this.dbFactory.Create();
        var orders = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Select(f => new
            {
                f.Id,
                f.TenantId,
                TotalAmount = f.TotalAmount + 50,
                ProductCount = 3,
                UpdatedAt = DateTime.Now
            })
            .OrderByDescending(f => f.Id)
            .Take(20)
            .ToList();
        var orderIds = orders.Select(f => f.Id).ToList();

        var sql = repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .SetBulkCopy(orders)
            .ToSql(out var dbParameters);
        //Assert.IsTrue(sql == "CREATE TEMPORARY TABLE `sys_order_0c0f27d1c0224df38030d8e78b03f8c4`(\r\n`Id` varchar(50) NOT NULL,\r\n`TotalAmount` double,\r\n`ProductCount` int,\r\n`UpdatedAt` datetime,\r\nPRIMARY KEY(`Id`)\r\n);\r\nUPDATE `sys_order_104_202405` a INNER JOIN `sys_order_0c0f27d1c0224df38030d8e78b03f8c4` b ON a.`Id`=b.`Id` SET a.`TotalAmount`=b.`TotalAmount`,a.`ProductCount`=b.`ProductCount`,a.`UpdatedAt`=b.`UpdatedAt`;UPDATE `sys_order_105_202405` a INNER JOIN `sys_order_0c0f27d1c0224df38030d8e78b03f8c4` b ON a.`Id`=b.`Id` SET a.`TotalAmount`=b.`TotalAmount`,a.`ProductCount`=b.`ProductCount`,a.`UpdatedAt`=b.`UpdatedAt`;DROP TABLE `sys_order_0c0f27d1c0224df38030d8e78b03f8c4`");

        await repository.BeginTransactionAsync();
        var result = await repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .SetBulkCopy(orders)
            .ExecuteAsync();
        var updatedOrders = await repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => orderIds.Contains(f.Id))
            .ToListAsync();
        await repository.CommitAsync();
        orders.Sort((x, y) => x.Id.CompareTo(y.Id));
        updatedOrders.Sort((x, y) => x.Id.CompareTo(y.Id));
        Assert.AreEqual(orders.Count, result);
        for (int i = 0; i < orders.Count; i++)
        {
            Assert.AreEqual(updatedOrders[i].TotalAmount, orders[i].TotalAmount);
            Assert.AreEqual(updatedOrders[i].ProductCount, orders[i].ProductCount);
            Assert.That(updatedOrders[i].TenantId, Is.AnyOf("104", "105"));
        }
    }
    [Test]
    public async Task Update_ManySharding_Range()
    {
        await this.InitSharding();
        var beginTime = DateTime.Parse("2020-01-01");
        var endTime = DateTime.Parse("2024-12-31");
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTableByRange("104", beginTime, endTime)
            .Select(f => new
            {
                f.Id,
                f.TenantId,
                f.OrderNo,
                f.TotalAmount
            })
            .OrderByDescending(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order%');SELECT * FROM (SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`TotalAmount` FROM `sys_order_104_202406` a UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`TotalAmount` FROM `sys_order_104_202405` a) a ORDER BY `Id` DESC", sql);
        var orders = repository.From<Order>()
            .UseTableByRange("104", beginTime, endTime)
            .Select(f => new
            {
                f.Id,
                f.TenantId,
                f.OrderNo,
                f.TotalAmount
            })
            .OrderByDescending(f => f.Id)
            .ToList();

        sql = repository.From<Order>()
           .UseTableByRange("104", beginTime, endTime)
           .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
           .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
           {
               var tableName = orderTableName.Replace(orderOrigName, userOrigName);
               return tableName[..^7];
           })
           .Select((x, y) => new
           {
               x.Id,
               x.TenantId,
               BuyerName = y.Name,
               x.TotalAmount
           })
           .OrderByDescending(f => f.Id)
           .ToSql(out _);
        Assert.AreEqual("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order%' OR TABLE_NAME LIKE 'sys_user%');SELECT * FROM (SELECT a.`Id`,a.`TenantId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` UNION ALL SELECT a.`Id`,a.`TenantId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order_104_202406` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id`) a ORDER BY `Id` DESC", sql);
        var orderInfos = repository.From<Order>()
            .UseTableByRange("104", beginTime, endTime)
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName[..^7];
            })
            .Select((x, y) => new
            {
                x.Id,
                x.TenantId,
                BuyerName = y.Name,
                x.TotalAmount
            })
            .OrderByDescending(f => f.Id)
            .ToList();

        Assert.AreEqual(orders.Count, orderInfos.Count);
    }
    [Test]
    public async Task ManySharding_FromQuery_SubQuery()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var count = 1;
        var amount = 50;
        var sql = repository
            .FromQuery(f => f.From<Order>()
                .UseTable("sys_order_104_202405", "sys_order_105_202405")
                .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
                .UseTableMap((orderOrigName, userOrigName, orderTableName)
                    => orderTableName.Replace(orderOrigName, userOrigName).Slice(0, -7))
                .LeftJoin<OrderDetail>((a, b, c) => a.Id == c.OrderId)
                .UseTableMap((orderOrigName, orderDetailOrigName, orderTableName)
                    => orderTableName.Replace(orderOrigName, orderDetailOrigName))
                .GroupBy((a, b, c) => new { a.BuyerId, OrderId = a.Id, a.OrderNo })
                .Having((x, a, b, c) => x.CountDistinct(c.ProductId) > count)
                .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, a.Grouping.OrderNo, ProductTotal = a.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .IncludeMany((a, b) => b.Details, f => f.Amount > amount)
            .UseTableMap((orderOrigName, orderDetailOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, orderDetailOrigName))
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order%' OR TABLE_NAME LIKE 'sys_user%' OR TABLE_NAME LIKE 'sys_order_detail%' OR TABLE_NAME LIKE 'sys_order_detail%');SELECT b.`Disputes`,a.`BuyerId`,a.`OrderId`,a.`OrderNo`,a.`ProductTotal`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM (SELECT a.`BuyerId`,a.`Id` AS `OrderId`,a.`OrderNo`,COUNT(DISTINCT c.`ProductId`) AS `ProductTotal` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` LEFT JOIN `sys_order_detail_104_202405` c ON a.`Id`=c.`OrderId` GROUP BY a.`BuyerId`,a.`Id`,a.`OrderNo` HAVING COUNT(DISTINCT c.`ProductId`)>@p0) a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id` UNION ALL SELECT b.`Disputes`,a.`BuyerId`,a.`OrderId`,a.`OrderNo`,a.`ProductTotal`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM (SELECT a.`BuyerId`,a.`Id` AS `OrderId`,a.`OrderNo`,COUNT(DISTINCT c.`ProductId`) AS `ProductTotal` FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` LEFT JOIN `sys_order_detail_105_202405` c ON a.`Id`=c.`OrderId` GROUP BY a.`BuyerId`,a.`Id`,a.`OrderNo` HAVING COUNT(DISTINCT c.`ProductId`)>@p0) a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id`", sql);
        Assert.IsNotEmpty(dbParameters);
        Assert.AreEqual((int)dbParameters[0].Value, count);

        var result = repository
            .FromQuery(f => f.From<Order>()
                .UseTable("sys_order_104_202405", "sys_order_105_202405")
                .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
                .UseTableMap((orderOrigName, userOrigName, orderTableName)
                    => orderTableName.Replace(orderOrigName, userOrigName).Substring(0, -7))
                .LeftJoin<OrderDetail>((a, b, c) => a.Id == c.OrderId)
                .UseTableMap((orderOrigName, orderDetailOrigName, orderTableName)
                    => orderTableName.Replace(orderOrigName, orderDetailOrigName))
                .GroupBy((a, b, c) => new { a.BuyerId, OrderId = a.Id, a.OrderNo })
                .Having((x, a, b, c) => x.CountDistinct(c.ProductId) > count)
                .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, a.Grouping.OrderNo, ProductTotal = a.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .IncludeMany((a, b) => b.Details, f => f.Amount > amount)
            .UseTableMap((orderOrigName, orderDetailOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, orderDetailOrigName))
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .First();
        if (result != null)
        {
            Assert.IsNotNull(result.Disputes);
            Assert.IsNotNull(result.Order);
            Assert.IsNotNull(result.Order.Details);
            Assert.IsNotEmpty(result.Order.Details);
            Assert.Greater(result.Order.Details[0].Amount, 0);
        }
    }
    [Test]
    public void TableSchema()
    {
        var repository = this.dbFactory.Create();
        var sql = repository
            .FromQuery(f => f.From<OrderDetail>()
                .UseTableSchema("myschema")
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .UseTableSchema("myschema")
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .UseTableSchema("myschema")
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                Buyer = y,
                x.ProductCount
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`OrderId`,a.`BuyerId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `myschema`.`sys_order_detail` a INNER JOIN `myschema`.`sys_order` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `myschema`.`sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1", sql);

        var result = repository
            .FromQuery(f => f.From<OrderDetail>()
                .UseTableSchema("fengling")
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .UseTableSchema("fengling")
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .UseTableSchema("fengling")
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                Buyer = y,
                x.ProductCount
            })
            .ToList();
        Assert.IsNotEmpty(result);
        {
            Assert.IsNotNull(result[0]);
            Assert.IsNotNull(result[0].Group);
            Assert.IsNotNull(result[0].Buyer);
            Assert.Greater(result[0].ProductCount, 1);
        }
    }
    [Test]
    public async Task Query_ManySharding_SingleTable_Include_TableSchema()
    {
        await this.InitSharding();
        var productCount = 1;
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .UseTableSchema("fengling")
            .Include(f => f.Details)
            .UseTableSchema("fengling")
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToSql(out _);
        Assert.AreEqual("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order%' OR TABLE_NAME LIKE 'sys_order_detail%');SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0", sql);

        var result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .UseTableSchema("fengling")
            .Include(f => f.Details)
            .UseTableSchema("fengling")
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToList();
        Assert.IsNotEmpty(result);
        {
            var tenantIds = result.Select(f => f.TenantId).Distinct().ToList();
            Assert.IsFalse(tenantIds.Exists(f => f != "104"));
            foreach (var order in result)
            {
                Assert.IsNotNull(order.Details);
                foreach (var orderDetail in order.Details)
                {
                    Assert.AreEqual("104", orderDetail.TenantId);
                }
            }
        }

        sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .UseTableSchema("fengling")
            .Include(f => f.Details)
            .UseTableSchema("fengling")
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToSql(out _);
        Assert.AreEqual("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order_detail%');SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_105_202405` a WHERE a.`ProductCount`>@p0", sql);

        result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .UseTableSchema("fengling")
            .Include(f => f.Details)
            .UseTableSchema("fengling")
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToList();
        Assert.IsNotEmpty(result);
        {
            var tenantIds = result.Select(f => f.TenantId).ToList();
            Assert.That(tenantIds, Has.Some.Matches<string>(f => f == "104" || f == "105"));
            foreach (var order in result)
            {
                Assert.IsNotNull(order.Details);
                foreach (var orderDetail in order.Details)
                {
                    Assert.That(orderDetail.TenantId, Is.AnyOf("104", "105"));
                }
            }
        }
    }
    [Test]
    public async Task Create_Without_Sharding()
    {
        var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .WhereById(11)
            .ExecuteAsync();
        repository.Create<User>()
            .WithBy(new
            {
                Id = 11,
                TenantId = "104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2024-05-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2024-05-15 16:27:38"),
                UpdatedBy = 1
            })
            .Execute();
        var result = repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 11)
            .First();
        Assert.IsNotNull(result);
        Assert.AreEqual("104", result.TenantId);
    }





    [Test]
    public async Task WhereBoolean()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => f.IsEnabled);
        Assert.IsNotEmpty(result1);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.IsNotEmpty(result2);
        Assert.AreEqual(result2.Count, result1.Count);
    }
    [Test]
    public async Task WhereMemberVisit()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => !(f.IsEnabled == false) && f.Id > 0);
        Assert.IsNotEmpty(result1);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.IsNotEmpty(result2);
        Assert.AreEqual(result2.Count, result1.Count);
    }
    [Test]
    public async Task WhereStringEnum()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Company>()
            .Where(f => f.Nature == CompanyNature.Internet)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE a.`Nature`='Internet'", sql1);
        var result1 = await repository.QueryAsync<Company>(f => f.Nature == CompanyNature.Internet);
        Assert.GreaterOrEqual(result1.Count, 2);

        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')='Internet'", sql2);
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.GreaterOrEqual(result2.Count, 2);

        var localNature = CompanyNature.Internet;
        var sql3 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')=@p0", sql3);
        Assert.AreEqual(localNature.ToString(), (string)dbParameters[0].Value);
        Assert.That(dbParameters[0].Value, Is.TypeOf<string>());
        var result3 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.GreaterOrEqual(result3.Count, 2);
    }
    [Test]
    public async Task WhereCoalesceConditional2()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')='Internet'", sql1);
        var result1 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.GreaterOrEqual(result1.Count, 2);
        Assert.AreEqual(CompanyNature.Internet, (result1[0].Nature ?? CompanyNature.Internet));

        var localNature = CompanyNature.Internet;
        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')=@p0", sql2);
        Assert.AreEqual(localNature.ToString(), (string)dbParameters[0].Value);
        Assert.That(dbParameters[0].Value, Is.TypeOf<string>());
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.GreaterOrEqual(result2.Count, 2);
        Assert.AreEqual(localNature, (result2[0].Nature ?? CompanyNature.Internet));

        var sql3 = repository.From<Company>()
            .Where(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature)
            .ToSql(out dbParameters);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN a.`Nature` ELSE 'Internet' END)=@p0", sql3);
        Assert.AreEqual(localNature.ToString(), (string)dbParameters[0].Value);
        Assert.That(dbParameters[0].Value, Is.TypeOf<string>());
        var result3 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.GreaterOrEqual(result3.Count, 2);
        Assert.AreEqual(localNature, result3[0].Nature);

        var sql4 = repository.From<User>()
            .Where(f => (f.IsEnabled ? f.SourceType : UserSourceType.Website) > UserSourceType.Website)
            .Select(f => f.Id)
            .ToSql(out dbParameters);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN a.`SourceType` ELSE 'Website' END)>'Website'", sql4);
        var result5 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.GreaterOrEqual(result5.Count, 2);
        Assert.AreEqual(localNature, result5[0].Nature);
    }
    [Test]
    public async Task WhereIsNull()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Order>()
           .Where(f => f.BuyerId.IsNull())
           .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order` a WHERE a.`BuyerId` IS NULL", sql1);
        repository.BeginTransaction();
        repository.Update<Order>(f => new { BuyerId = DBNull.Value }, f => f.Id == "1");
        var result1 = repository.QueryById<Order>("1");
        repository.Commit();
        Assert.AreEqual(0, result1.BuyerId);
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.GreaterOrEqual(result2.Count, 2);
        var localNature = CompanyNature.Internet;
        var result3 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.GreaterOrEqual(result2.Count, 2);
    }
    [Test]
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
        Assert.AreEqual("SELECT * FROM `sys_order` a,`sys_user` b WHERE a.`BuyerId`=b.`Id` AND (a.`SellerId` IS NULL OR a.`ProductCount` IS NULL) AND a.`Products` IS NOT NULL AND (a.`Products` IS NULL OR a.`Disputes` IS NULL)", sql);

        var filterExpr = Sql.Where<Order, User>()
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
        Assert.AreEqual("SELECT * FROM `sys_order` a,`sys_user` b WHERE (a.`BuyerId`=b.`Id` OR b.`SourceType`='Douyin') AND (a.`BuyerSource`='Taobao' OR (a.`SellerId` IS NULL AND a.`ProductCount` IS NULL) OR a.`ProductCount`>1 OR (a.`TotalAmount`>500 AND a.`BuyerSource`='Website')) AND ((a.`BuyerId`<=10 AND a.`ProductCount`>5 AND b.`SourceType`='Douyin') OR (a.`BuyerId`>10 AND a.`ProductCount`<=5 AND b.`SourceType`='Website') OR a.`BuyerSource`='Taobao') AND (a.`Products` IS NULL OR a.`Disputes` IS NULL)", sql);
    }
    [Test]
    public void Where()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") && string.IsNullOrEmpty(f.OrderNo)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_order` a WHERE EXISTS(SELECT * FROM `sys_user` t WHERE t.`Id`=a.`BuyerId` AND t.`IsEnabled`=1) AND (a.`BuyerId` IS NULL OR a.`BuyerId`=2) AND a.`OrderNo` LIKE '%ON_%' AND (a.`OrderNo` IS NULL OR a.`OrderNo`='')", sql1);
        var result1 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo)))
            .Select(f => f.Id)
            .ToList();
        Assert.IsNotEmpty(result1);

        var sql2 = repository.From<Order>()
            .Where(f => (f.BuyerId.IsNull() || f.BuyerId == 2) && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo))
                && (Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) || f.SellerId.IsNull()))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_order` a WHERE (a.`BuyerId` IS NULL OR a.`BuyerId`=2) AND (a.`OrderNo` LIKE '%ON_%' OR (a.`OrderNo` IS NULL OR a.`OrderNo`='')) AND (EXISTS(SELECT * FROM `sys_user` t WHERE t.`Id`=a.`BuyerId` AND t.`IsEnabled`=1) OR a.`SellerId` IS NULL)", sql2);
        var result2 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo)))
            .Select(f => f.Id)
            .ToList();
        Assert.IsNotEmpty(result2);

        var sql3 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") && string.IsNullOrEmpty(f.OrderNo)) || DateTime.IsLeapYear(f.CreatedAt.Year))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_order` a WHERE EXISTS(SELECT * FROM `sys_user` t WHERE t.`Id`=a.`BuyerId` AND t.`IsEnabled`=1) AND (a.`BuyerId` IS NULL OR a.`BuyerId`=2) AND a.`OrderNo` LIKE '%ON_%' AND (a.`OrderNo` IS NULL OR a.`OrderNo`='') OR (YEAR(a.`CreatedAt`)%4=0 AND YEAR(a.`CreatedAt`)%100<>0 OR YEAR(a.`CreatedAt`)%400=0)", sql3);
        var result3 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") && string.IsNullOrEmpty(f.OrderNo)) || DateTime.IsLeapYear(f.CreatedAt.Year))
            .Select(f => f.Id)
            .ToList();
        Assert.IsNotNull(result3);
        //Assert.IsTrue(result3.Count > 0);
    }
}