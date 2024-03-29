﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Trolley.SqlServer;
using Xunit;

namespace Trolley.Test.SqlServer;

public class MethodCallUnitTest : UnitTestBase
{
    public MethodCallUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register<SqlServerProvider>("fengling", "Server=127.0.0.1;Database=fengling;Uid=sa;password=SQLserverSA123456;TrustServerCertificate=true", true)
            .Configure<SqlServerProvider, ModelConfiguration>();
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
        Assert.True(sql == "SELECT [Id] FROM [sys_user] WHERE [Id] IN (1,2)");
        var result = repository.From<User>()
            .Where(f => new int[] { 1, 2 }.Contains(f.Id))
            .ToList();
        Assert.NotNull(result);
        Assert.True(result.Count == 2);

        sql = repository.From<User>()
            .Where(f => f.Name.Contains("kevin"))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.True(sql == "SELECT [Id] FROM [sys_user] WHERE [Name] LIKE '%kevin%'");
        result = await repository.From<User>()
            .Where(f => f.Name.Contains("kevin"))
            .ToListAsync();
        Assert.NotNull(result);
        Assert.True(result.Count >= 1);
        sql = repository.From<User>()
            .Where(f => new List<string> { "kevin", "cindy" }.Contains(f.Name))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.True(sql == "SELECT [Id] FROM [sys_user] WHERE [Name] IN ('kevin','cindy')");
        result = await repository.From<User>()
            .Where(f => new List<string> { "kevin", "cindy" }.Contains(f.Name))
            .ToListAsync();
        Assert.NotNull(result);
        Assert.True(result.Count == 1);
    }
    [Fact]
    public async void Concat()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        bool isMale = false;
        int count = 10;
        var sql = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => string.Concat(f.Name + "_1_" + isMale, f.Age + 5, isMale) + "_2_" + f.Age + "_3_" + isMale + "_4_" + count)
            .ToSql(out var dbParameters);
        Assert.True(sql == "SELECT ([Name]+'_1_'+@p0+CAST([Age]+5 AS NVARCHAR(MAX))+@p1+'_2_'+CAST([Age] AS NVARCHAR(MAX))+'_3_'+@p2+'_4_'+@p3) FROM [sys_user] WHERE [Id]=1");
        Assert.True((string)dbParameters[0].Value == isMale.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        Assert.True((string)dbParameters[1].Value == isMale.ToString());
        Assert.True(dbParameters[1].Value.GetType() == typeof(string));
        Assert.True((string)dbParameters[2].Value == isMale.ToString());
        Assert.True(dbParameters[2].Value.GetType() == typeof(string));
        Assert.True((string)dbParameters[3].Value == count.ToString());
        Assert.True(dbParameters[3].Value.GetType() == typeof(string));

        var result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => string.Concat(f.Name + "_1_" + isMale, f.Age + 5, isMale) + "_2_" + f.Age + "_3_" + isMale + "_4_" + count)
            .FirstAsync();
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
            .ToSql(out var dbParameters);
        Assert.True(sql == "SELECT ([Name]+'222_111_'+CAST([Age] AS NVARCHAR(MAX))+@p0+'_'+@p1+'_'+@p2) FROM [sys_user] WHERE [Name] LIKE '%cindy%'");
        Assert.True((string)dbParameters[0].Value == isMale.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        Assert.True((string)dbParameters[1].Value == isMale.ToString());
        Assert.True(dbParameters[1].Value.GetType() == typeof(string));
        Assert.True((string)dbParameters[2].Value == count.ToString());
        Assert.True(dbParameters[2].Value.GetType() == typeof(string));
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
        Assert.True(sql1 == "SELECT (CASE WHEN [Name]='leafkevin' THEN 0 WHEN [Name]>'leafkevin' THEN 1 ELSE -1 END) AS [NameCompare],(CASE WHEN [CreatedAt]=CAST(FORMAT(GETDATE(),'yyyy-MM-dd HH:mm:ss') AS DATETIME) THEN 0 WHEN [CreatedAt]>CAST(FORMAT(GETDATE(),'yyyy-MM-dd HH:mm:ss') AS DATETIME) THEN 1 ELSE -1 END) AS [CreatedAtCompare],(CASE WHEN [CreatedAt]=GETDATE() THEN 0 WHEN [CreatedAt]>GETDATE() THEN 1 ELSE -1 END) AS [CreatedAtCompare1],(CASE WHEN [UpdatedAt]=DATEADD(MILLISECOND,-33900000,DATEADD(DAY,-1,[UpdatedAt])) THEN 0 WHEN [UpdatedAt]>DATEADD(MILLISECOND,-33900000,DATEADD(DAY,-1,[UpdatedAt])) THEN 1 ELSE -1 END) AS [UpdatedAtCompare] FROM [sys_user] WHERE [Id]=1");

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
        Assert.True(result1.NameCompare == 0);
        Assert.True(result1.CreatedAtCompare == -1);
        Assert.True(result1.CreatedAtCompare1 == -1);
        Assert.True(result1.UpdatedAtCompare == 1);

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
        Assert.True(sql2 == "SELECT (CASE WHEN [Name]='leafkevin' THEN 0 WHEN [Name]>'leafkevin' THEN 1 ELSE -1 END) AS [NameCompare],(CASE WHEN [CreatedAt]=CAST(FORMAT(GETDATE(),'yyyy-MM-dd HH:mm:ss') AS DATETIME) THEN 0 WHEN [CreatedAt]>CAST(FORMAT(GETDATE(),'yyyy-MM-dd HH:mm:ss') AS DATETIME) THEN 1 ELSE -1 END) AS [CreatedAtCompare],(CASE WHEN [CreatedAt]=GETDATE() THEN 0 WHEN [CreatedAt]>GETDATE() THEN 1 ELSE -1 END) AS [CreatedAtCompare1],(CASE WHEN [UpdatedAt]=DATEADD(MILLISECOND,-900000,[UpdatedAt]) THEN 0 WHEN [UpdatedAt]>DATEADD(MILLISECOND,-900000,[UpdatedAt]) THEN 1 ELSE -1 END) AS [UpdatedAtCompare] FROM [sys_user] WHERE [Id]=1");

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
        Assert.True(result2.NameCompare == 0);
        Assert.True(result2.CreatedAtCompare == -1);
        Assert.True(result2.CreatedAtCompare1 == -1);
        Assert.True(result2.UpdatedAtCompare == 1);
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
        Assert.True(sql == "SELECT (CASE WHEN [Id]=1 THEN 0 WHEN [Id]>1 THEN 1 ELSE -1 END) AS [IntCompare],(CASE WHEN [OrderNo]='OrderNo-001' THEN 0 WHEN [OrderNo]>'OrderNo-001' THEN 1 ELSE -1 END) AS [StringCompare],(CASE WHEN [CreatedAt]='2022-12-20 00:00:00.000' THEN 0 WHEN [CreatedAt]>'2022-12-20 00:00:00.000' THEN 1 ELSE -1 END) AS [DateTimeCompare],(CASE WHEN [IsEnabled]=0 THEN 0 WHEN [IsEnabled]>0 THEN 1 ELSE -1 END) AS [BooleanCompare] FROM [sys_order]");

        var result = repository.From<Order>()
            .Where(f => f.Id == "1")
            .Select(f => new
            {
                f.Id,
                f.TenantId,
                f.OrderNo,
                f.CreatedAt,
                f.IsEnabled,
                IntCompare = f.Id.CompareTo(1),
                StringCompare = f.OrderNo.CompareTo("OrderNo-001"),
                DateTimeCompare = f.CreatedAt.CompareTo(DateTime.Parse("2022-12-20")),
                BooleanCompare = f.IsEnabled.CompareTo(false)
            })
            .First();
        Assert.NotNull(result);
        Assert.True(result.IntCompare == result.Id.CompareTo(1));
        Assert.True(result.StringCompare == result.OrderNo.CompareTo("OrderNo-001"));
        Assert.True(result.DateTimeCompare == result.CreatedAt.CompareTo(DateTime.Parse("2022-12-20")));
        Assert.True(result.BooleanCompare == result.IsEnabled.CompareTo(false));
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
        Assert.True(sql == "SELECT ('Begin_'+TRIM([OrderNo])+'123_End') AS [Trim],('Begin_'+LTRIM([OrderNo])+'123   _End') AS [TrimStart],('Begin_'+RTRIM([OrderNo])+'  123_End') AS [TrimEnd] FROM [sys_order]");

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
        Assert.True(sql1 == "SELECT (@p0+TRIM([OrderNo])+@p1+@p2) AS [Trim],('Begin_'+LTRIM([OrderNo])+@p3+'_End') AS [TrimStart],('Begin_'+RTRIM([OrderNo])+@p4+'_End') AS [TrimEnd] FROM [sys_order]");
        Assert.True((string)dbParameters[0].Value == strValue1);
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        Assert.True((string)dbParameters[1].Value == strValue2.Trim());
        Assert.True(dbParameters[1].Value.GetType() == typeof(string));
        Assert.True((string)dbParameters[2].Value == strValue3);
        Assert.True(dbParameters[2].Value.GetType() == typeof(string));
        Assert.True((string)dbParameters[3].Value == strValue2.TrimStart());
        Assert.True(dbParameters[3].Value.GetType() == typeof(string));
        Assert.True((string)dbParameters[4].Value == strValue2.TrimEnd());
        Assert.True(dbParameters[4].Value.GetType() == typeof(string));

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
        Assert.True(sql == "SELECT (LOWER([OrderNo])+'_ABCD') AS [Col1],(UPPER([OrderNo])+'_abcd') AS [Col2] FROM [sys_order]");

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
        Assert.True(sql == "SELECT (LOWER([OrderNo])+'_ABCD') AS [Col1],(UPPER([OrderNo])+'_abcd') AS [Col2] FROM [sys_order]");

        var strValue = "_AbCd";
        var sql1 = repository.From<Order>()
           .Select(f => new
           {
               Col1 = f.OrderNo.ToLower() + strValue.ToUpper(),
               Col2 = f.OrderNo.ToUpper() + strValue.ToLower()
           })
           .ToSql(out var dbParameters);
        Assert.True(sql1 == "SELECT (LOWER([OrderNo])+@p0) AS [Col1],(UPPER([OrderNo])+@p1) AS [Col2] FROM [sys_order]");
        Assert.True((string)dbParameters[0].Value == strValue.ToUpper());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        Assert.True((string)dbParameters[1].Value == strValue.ToLower());
        Assert.True(dbParameters[1].Value.GetType() == typeof(string));

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
        Assert.True(sql == "SELECT (LOWER([OrderNo])+'_ABCD') AS [Col1],(UPPER([OrderNo])+'_abcd') AS [Col2] FROM [sys_order]");

        repository.BeginTransaction();
        repository.Delete<Order>("8");
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
        var result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectFlattenTo<OrderInfo>()
            .ToList();
        Assert.True(result[0].Id == "8");
        Assert.True(result[0].BuyerId == 1);
        Assert.True(result[0].OrderNo == "On-ZwYx");
        Assert.Null(result[0].Description);

        result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectFlattenTo<OrderInfo>(f => new OrderInfo
            {
                Description = "TotalAmount:" + f.TotalAmount
            })
            .ToList();
        Assert.True(result[0].Id == "8");
        Assert.True(result[0].BuyerId == 1);
        Assert.True(result[0].OrderNo == "On-ZwYx");
        Assert.NotNull(result[0].Description);
        Assert.True(result[0].Description == "TotalAmount:500");

        result = repository.From<Order>()
           .Where(f => Sql.In(f.Id, new[] { "8" }))
           .SelectFlattenTo<OrderInfo>(f => new OrderInfo
           {
               Description = this.DeferInvoke().Deferred()
           })
           .ToList();
        Assert.True(result[0].Id == "8");
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
    }
    private string DeferInvoke() => "DeferInvoke";
}