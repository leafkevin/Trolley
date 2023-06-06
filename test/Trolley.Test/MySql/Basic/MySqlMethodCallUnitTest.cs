using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Trolley.MySqlConnector;
using Xunit;

namespace Trolley.Test.MySql;

public class MySqlMethodCallUnitTest : UnitTestBase
{
    public MySqlMethodCallUnitTest()
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
    public async void Contains()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => new int[] { 1, 2 }.Contains(f.Id))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id` FROM `sys_user` WHERE `Id` IN (1,2)");
        var result = repository.From<User>()
             .Where(f => new int[] { 1, 2 }.Contains(f.Id))
             .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count == 2);

        sql = repository.From<User>()
            .Where(f => f.Name.Contains("kevin"))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id` FROM `sys_user` WHERE `Name` LIKE '%kevin%'");
        result = await repository.From<User>()
            .Where(f => f.Name.Contains("kevin"))
            .ToListAsync();
        Assert.NotNull(result);
        Assert.True(result.Count >= 1);

        sql = repository.From<User>()
            .Where(f => new List<string> { "kevin", "cindy" }.Contains(f.Name))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id` FROM `sys_user` WHERE `Name` IN ('kevin','cindy')");
        result = await repository.From<User>()
            .Where(f => new List<string> { "kevin", "cindy" }.Contains(f.Name))
            .ToListAsync();
        Assert.NotNull(result);
        Assert.True(result.Count == 1);
    }
    [Fact]
    public void Concat()
    {
        using var repository = dbFactory.Create();
        bool isMale = false;
        int count = 10;
        var sql = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => string.Concat(f.Name + "_1_" + isMale, f.Age + 5, isMale) + "_2_" + f.Age + "_3_" + isMale + "_4_" + count)
            .ToSql(out _);
        Assert.True(sql == "SELECT CONCAT(`Name`,'_1_False',CAST(`Age`+5 AS CHAR),'False_2_',CAST(`Age` AS CHAR),'_3_False_4_10') FROM `sys_user` WHERE `Id`=1");
        var result = repository.From<User>()
            .Select(f => string.Concat(f.Name + "_1_" + isMale, f.Age + 5, isMale) + "_2_" + f.Age + "_3_" + isMale + "_4_" + count)
            .First();
        Assert.NotNull(result);
        Assert.True(result == "leafkevin_1_False30False_2_25_3_False_4_10");
    }
    [Fact]
    public async void Format()
    {
        using var repository = dbFactory.Create();
        bool isMale = false;
        int count = 5;
        var sql = repository.From<User>()
            .Where(f => f.Name.Contains("cindy"))
            .Select(f => $"{f.Name + "222"}_111_{f.Age + isMale.ToString()}_{isMale}_{count}")
            .ToSql(out _);
        Assert.True(sql == "SELECT CONCAT(`Name`,'222_111_',CAST(`Age` AS CHAR),'False_False_5') FROM `sys_user` WHERE `Name` LIKE '%cindy%'");
        var result = await repository.From<User>()
            .Where(f => f.Name.Contains("cindy"))
            .Select(f => $"{f.Name + "222"}_111_{f.Age + isMale.ToString()}_{isMale}_{count}")
            .FirstAsync();
        Assert.True(result == "cindy222_111_21False_False_5");
    }
    [Fact]
    public void Compare()
    {
        using var repository = dbFactory.Create();
        var sql1 = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NameCompare = string.Compare(f.Name, "leafkevin"),
                CreatedAtCompare = DateTime.Compare(f.CreatedAt, DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))),
                CreatedAtCompare1 = DateTime.Compare(f.CreatedAt, DateTime.Now),
                UpdatedAtCompare = DateTime.Compare(f.UpdatedAt, f.UpdatedAt.Subtract(TimeSpan.FromMinutes(5)))
            })
            .ToSql(out _);
        Assert.True(sql1 == "SELECT (CASE WHEN `Name`='leafkevin' THEN 0 WHEN `Name`>'leafkevin' THEN 1 ELSE -1 END) AS `NameCompare`,(CASE WHEN `CreatedAt`=CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) THEN 0 WHEN TIMESTAMPDIFF(MICROSECOND,`CreatedAt`,CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME))<0 THEN 1 ELSE -1 END) AS `CreatedAtCompare`,(CASE WHEN `CreatedAt`=NOW() THEN 0 WHEN TIMESTAMPDIFF(MICROSECOND,`CreatedAt`,NOW())<0 THEN 1 ELSE -1 END) AS `CreatedAtCompare1`,(CASE WHEN `UpdatedAt`=DATE_SUB(`UpdatedAt`,INTERVAL 5*60000000 MICROSECOND) THEN 0 WHEN TIMESTAMPDIFF(MICROSECOND,`UpdatedAt`,DATE_SUB(`UpdatedAt`,INTERVAL 5*60000000 MICROSECOND))<0 THEN 1 ELSE -1 END) AS `UpdatedAtCompare` FROM `sys_user` WHERE `Id`=1");

        var sql2 = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NameCompare = string.Compare(f.Name, "leafkevin".ToParameter()),
                CreatedAtCompare = DateTime.Compare(f.CreatedAt, DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))),
                CreatedAtCompare1 = DateTime.Compare(f.CreatedAt, DateTime.Now),
                UpdatedAtCompare = DateTime.Compare(f.UpdatedAt, f.UpdatedAt.Subtract(TimeSpan.FromMinutes(5)))
            })
            .ToSql(out _);
        Assert.True(sql2 == "SELECT (CASE WHEN `Name`=@p0 THEN 0 WHEN `Name`>@p0 THEN 1 ELSE -1 END) AS `NameCompare`,(CASE WHEN `CreatedAt`=CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME) THEN 0 WHEN TIMESTAMPDIFF(MICROSECOND,`CreatedAt`,CAST(DATE_FORMAT(NOW(),'%Y-%m-%d %H:%i:%s') AS DATETIME))<0 THEN 1 ELSE -1 END) AS `CreatedAtCompare`,(CASE WHEN `CreatedAt`=NOW() THEN 0 WHEN TIMESTAMPDIFF(MICROSECOND,`CreatedAt`,NOW())<0 THEN 1 ELSE -1 END) AS `CreatedAtCompare1`,(CASE WHEN `UpdatedAt`=DATE_SUB(`UpdatedAt`,INTERVAL 5*60000000 MICROSECOND) THEN 0 WHEN TIMESTAMPDIFF(MICROSECOND,`UpdatedAt`,DATE_SUB(`UpdatedAt`,INTERVAL 5*60000000 MICROSECOND))<0 THEN 1 ELSE -1 END) AS `UpdatedAtCompare` FROM `sys_user` WHERE `Id`=1");

        var result = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                NameCompare = string.Compare(f.Name, "leafkevin"),
                CreatedAtCompare = DateTime.Compare(f.CreatedAt, DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))),
                CreatedAtCompare1 = DateTime.Compare(f.CreatedAt, DateTime.Now),
                UpdatedAtCompare = DateTime.Compare(f.UpdatedAt, f.UpdatedAt.Subtract(TimeSpan.FromMinutes(5)))
            })
            .First();
        Assert.NotNull(result);
        Assert.True(result.NameCompare == 0);
        Assert.True(result.CreatedAtCompare == -1);
        Assert.True(result.CreatedAtCompare1 == -1);
        Assert.True(result.UpdatedAtCompare == 1);
    }
    [Fact]
    public void CompareTo()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .Select(f => new
            {
                IntCompare = f.Id.CompareTo(1),
                StringCompare = f.OrderNo.CompareTo("OrderNo-001"),
                DateTimeCompare = f.CreatedAt.CompareTo(DateTime.Parse("2022-12-20")),
                BooleanCompare = f.IsEnabled.CompareTo(false)
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT (CASE WHEN `Id`=1 THEN 0 WHEN `Id`>1 THEN 1 ELSE -1 END) AS `IntCompare`,(CASE WHEN `OrderNo`='OrderNo-001' THEN 0 WHEN `OrderNo`>'OrderNo-001' THEN 1 ELSE -1 END) AS `StringCompare`,(CASE WHEN `CreatedAt`='2022-12-20 00:00:00.000' THEN 0 WHEN TIMESTAMPDIFF(MICROSECOND,`CreatedAt`,'2022-12-20 00:00:00.000')<0 THEN 1 ELSE -1 END) AS `DateTimeCompare`,(CASE WHEN `IsEnabled`=0 THEN 0 WHEN `IsEnabled`>0 THEN 1 ELSE -1 END) AS `BooleanCompare` FROM `sys_order`");
    }
    [Fact]
    public void Trims()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .Select(f => new
            {
                Trim = "Begin_" + f.OrderNo.Trim() + "  123   ".Trim() + "_End",
                TrimStart = "Begin_" + f.OrderNo.TrimStart() + "  123   ".TrimStart() + "_End",
                TrimEnd = "Begin_" + f.OrderNo.TrimEnd() + "  123   ".TrimEnd() + "_End"
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT CONCAT('Begin_',TRIM(`OrderNo`),TRIM('  123   '),'_End') AS `Trim`,CONCAT('Begin_',LTRIM(`OrderNo`),LTRIM('  123   '),'_End') AS `TrimStart`,CONCAT('Begin_',RTRIM(`OrderNo`),RTRIM('  123   '),'_End') AS `TrimEnd` FROM `sys_order`");
        repository.BeginTransaction();
        repository.Delete<Order>(new[] { 1, 2, 3 });
        var count = repository.Create<Order>(new[]
        {
            new Order
            {
                Id = 1,
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
                Id = 2,
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
                Id = 3,
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
            .Where(f => Sql.In(f.Id, new[] { 1, 2, 3 }))
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
            Assert.True(result[0].Trim == "Begin_ON-001123_End");
            Assert.True(result[0].TrimStart == "Begin_ON-001 123   _End");
            Assert.True(result[0].TrimEnd == "Begin_ ON-001  123_End");
        }
    }
    [Fact]
    public void ToUpper_ToLower()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .Select(f => new
            {
                Col1 = f.OrderNo.ToLower() + "_AbCd".ToUpper(),
                Col2 = f.OrderNo.ToUpper() + "_AbCd".ToLower()
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT CONCAT(LOWER(`OrderNo`),UPPER('_AbCd')) AS `Col1`,CONCAT(UPPER(`OrderNo`),LOWER('_AbCd')) AS `Col2` FROM `sys_order`");

        repository.BeginTransaction();
        repository.Delete<Order>(1);
        var count = repository.Create<Order>(new Order
        {
            Id = 1,
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
            .Where(f => Sql.In(f.Id, new[] { 1, 2, 3 }))
            .Select(f => new
            {
                Col1 = f.OrderNo.ToLower() + "_AbCd".ToUpper(),
                Col2 = f.OrderNo.ToUpper() + "_AbCd".ToLower()
            })
            .ToList();
        repository.Commit();
        if (count > 0)
        {
            Assert.True(result[0].Col1 == "on-zwyx_ABCD");
            Assert.True(result[0].Col2 == "ON-ZWYX_abcd");
        }
    }
    [Fact]
    public void Test_ToString()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .Select(f => new
            {
                Col1 = f.OrderNo.ToLower() + "_AbCd".ToUpper(),
                Col2 = f.OrderNo.ToUpper() + "_AbCd".ToLower()
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT CONCAT(LOWER(`OrderNo`),UPPER('_AbCd')) AS `Col1`,CONCAT(UPPER(`OrderNo`),LOWER('_AbCd')) AS `Col2` FROM `sys_order`");

        repository.BeginTransaction();
        repository.Delete<Order>(1);
        repository.Create<Order>(new Order
        {
            Id = 1,
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
            .Where(f => Sql.In(f.Id, new[] { 1, 2, 3 }))
            .Select(f => new
            {
                Col1 = f.OrderNo.ToLower() + "_AbCd".ToUpper(),
                Col2 = f.OrderNo.ToUpper() + "_AbCd".ToLower()
            })
            .ToList();
        Assert.True(result[0].Col1 == "on-zwyx_ABCD");
        Assert.True(result[0].Col2 == "ON-ZWYX_abcd");
    }
    [Fact]
    public void ToFlatten()
    {
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .Select(f => new
            {
                Col1 = f.OrderNo.ToLower() + "_AbCd".ToUpper(),
                Col2 = f.OrderNo.ToUpper() + "_AbCd".ToLower()
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT CONCAT(LOWER(`OrderNo`),UPPER('_AbCd')) AS `Col1`,CONCAT(UPPER(`OrderNo`),LOWER('_AbCd')) AS `Col2` FROM `sys_order`");

        repository.BeginTransaction();
        repository.Delete<Order>(8);
        repository.Create<Order>(new Order
        {
            Id = 8,
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
            .Where(f => Sql.In(f.Id, new[] { 8 }))
            .Select(f => Sql.FlattenTo<OrderInfo>())
            .ToList();
        Assert.True(result[0].Id == 8);
        Assert.True(result[0].BuyerId == 1);
        Assert.True(result[0].OrderNo == "On-ZwYx");
        Assert.Null(result[0].Description);

        result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { 8 }))
            .Select(f => Sql.FlattenTo<OrderInfo>(() => new
            {
                Description = "TotalAmount:" + f.TotalAmount
            }))
            .ToList();
        Assert.True(result[0].Id == 8);
        Assert.True(result[0].BuyerId == 1);
        Assert.True(result[0].OrderNo == "On-ZwYx");
        Assert.NotNull(result[0].Description);
        Assert.True(result[0].Description == "TotalAmount:500");

        result = repository.From<Order>()
           .Where(f => Sql.In(f.Id, new[] { 8 }))
           .Select(f => Sql.FlattenTo<OrderInfo>(() => new
           {
               Description = this.DeferInvoke().Deferred()
           }))
           .ToList();
        Assert.True(result[0].Id == 8);
        Assert.True(result[0].BuyerId == 1);
        Assert.True(result[0].OrderNo == "On-ZwYx");
        Assert.NotNull(result[0].Description);
        Assert.True(result[0].Description == this.DeferInvoke());
    }

    [Fact]
    public void Convert()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        byte id = 1;
        var age = repository.From<User>()
            .Where(f => f.Id == id)
            .Select(f => (short)f.Age)
            .First();
    }
    [Fact]
    public void Update_Call()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        int id = 1;
        var orderNos = new string[] { "ON_001", "ON_002", "ON_003" };
        var sql = repository.Update<Order>()
            .Set(f => new { TotalAmount = 100 })
            .Where(f => f.BuyerId == id && orderNos.Contains(f.OrderNo))
            .ToSql(out _);
        int sdfsdf = 0;
    }
    private string DeferInvoke() => "DeferInvoke";
}
