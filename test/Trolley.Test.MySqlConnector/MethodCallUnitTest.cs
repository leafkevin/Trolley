using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trolley.MySqlConnector;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.MySqlConnector;

public class MethodCallUnitTest : UnitTestBase
{
    private static int connTotal = 0;
    private static int connOpenTotal = 0;
    private readonly ITestOutputHelper output;
    public MethodCallUnitTest(ITestOutputHelper output)
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
    public async Task Contains()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "SELECT a.`Id` FROM `sys_user` a WHERE a.`Name` IN ('kevin','cindy')");
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
        Assert.True(sql == "SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (@p0,@p1)");

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
        Assert.True(result1.Count > 0);
    }
    [Fact]
    public async Task Concat()
    {
        using var repository = dbFactory.Create();
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
        Assert.True(result == "leafkevin_1_False30False_2_25_3_False_4_10");
    }
    [Fact]
    public async Task Format()
    {
        using var repository = dbFactory.Create();
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
        using var repository = dbFactory.Create();
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
        using var repository = dbFactory.Create();
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
        using var repository = dbFactory.Create();
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
        using var repository = dbFactory.Create();
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
        using var repository = dbFactory.Create();
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
        using var repository = dbFactory.Create();
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
        using var repository = dbFactory.Create();
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
        using var repository = dbFactory.Create();
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
        using var repository = dbFactory.Create();
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
}
