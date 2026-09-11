using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Trolley.Test.MySqlConnector;

public class MethodCallUnitTest : UnitTestBase
{
    [SetUp]
    public void Setup()
    {
        var connectionString = "Server=192.168.61.67;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
        var builder = new OrmDbFactoryBuilder()
            .Register(OrmProviderType.MySql, "fengling", f => f.Use(connectionString), true)
            .UseMapping<ModelMappingConfiguration>(OrmProviderType.MySql)
            .UseInterceptor(new MyDbInterceptor());
        this.dbFactory = builder.Build();
        this.Initialize(1);
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
        Assert.IsNotEmpty(result1);
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
    public void Compare()
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
        repository.Delete<Order>().WhereByIds(new[] { "1", "2", "3" }).Execute();
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
        var result = repository.From<Order>()
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
    public async Task Method_Property_Deferred()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        //测试值类型缓存是否正确
        var result1 = await repository.From<UpdateEntity1>()
            .Where(f => f.Id == 1)
            .Select(f => f.DateTimeField)
            .FirstAsync();
        var result2 = await repository.From<UpdateEntity1>()
            .Where(f => f.Id == 1)
            .Select(f => new DateTimeOffset(DateTime.SpecifyKind(f.DateTimeField, DateTimeKind.Local)).UtcDateTime.Deferred())
            .FirstAsync();
        var sql = repository.From<UpdateEntity1>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                UtcDateTime = new DateTimeOffset(DateTime.SpecifyKind(f.DateTimeField, DateTimeKind.Local)).UtcDateTime.Deferred(),
                Timestamp = f.DateTimeOffsetField.ToUnixTimeMilliseconds()
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`DateTimeField`,a.`DateTimeOffsetField` FROM `sys_update_entity` a WHERE a.`Id`=1", sql);
        var result = await repository.From<UpdateEntity1>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                UtcDateTime = new DateTimeOffset(DateTime.SpecifyKind(f.DateTimeField, DateTimeKind.Local)).UtcDateTime.Deferred(),
                Timestamp = f.DateTimeOffsetField.ToUnixTimeMilliseconds()
            })
            .FirstAsync();
        Assert.IsNotNull(result);
        Assert.Greater(result.UtcDateTime, DateTime.UtcNow.AddDays(-1));
        Assert.Greater(result.Timestamp, DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds());
    }
}