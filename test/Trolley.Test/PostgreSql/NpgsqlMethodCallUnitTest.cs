﻿using Microsoft.Extensions.DependencyInjection;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Trolley.Test;

public class NpgsqlMethodCallUnitTest
{
    private readonly IOrmDbFactory dbFactory;
    public NpgsqlMethodCallUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register("fengling", true, f =>
            {
                var connectionString = "Host=localhost;Username=postgres;Password=123456;Database=fengling;Maximum Pool Size=20";
                f.Add<NpgSqlProvider>(connectionString, true);
            })
            .AddTypeHandler<JsonTypeHandler>()
            .Configure<NpgSqlProvider, NpgsqlModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public void Contains()
    {
        using var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => new int[] { 1, 2, 3 }.Contains(f.Id))
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id` IN (1,2,3)");
        sql = repository.From<User>()
            .Where(f => f.Name.Contains("kevin"))
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Name` LIKE '%kevin%'");
        sql = repository.From<User>()
            .Where(f => new List<string> { "keivn", "cindy" }.Contains(f.Name))
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Name` IN ('keivn','cindy')");
    }
    [Fact]
    public void Concat()
    {
        using var repository = this.dbFactory.Create();
        bool isMale = false;
        int count = 10;
        var sql = repository.From<User>()
            .Select(f => string.Concat(f.Name + "_1_" + isMale, f.Age, isMale) + "_2_" + f.Age + "_3_" + isMale + "_4_" + count)
            .ToSql(out _);
        Assert.True(sql == "SELECT CONCAT(`Name`,'_1_','False',`Age`,'False','_2_',`Age`,'_3_','False','_4_','10') FROM `sys_user`");
    }
    [Fact]
    public void Format()
    {
        using var repository = this.dbFactory.Create();
        bool isMale = false;
        int count = 5;
        var sql = repository.From<User>()
            .Select(f => $"{f.Name + "222"}_111_{f.Age + isMale.ToString()}_{isMale}_{count}")
            .ToSql(out _);
        Assert.True(sql == "SELECT CONCAT(CONCAT(`Name`,'222'),'_111_',CONCAT(`Age`,'False'),'_False_5') FROM `sys_user`");
    }
    [Fact]
    public void Compare()
    {
        using var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Select(f => new
            {
                NameCompare = string.Compare(f.Name, "leafkevin"),
                CreatedAtCompare = DateTime.Compare(f.CreatedAt, DateTime.Parse("2022-12-20"))
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT (CASE WHEN \"Name\"='leafkevin' THEN 0 WHEN \"Name\">'leafkevin' THEN 1 ELSE -1 END) AS NameCompare,(CASE WHEN \"CreatedAt\"=CAST('2022-12-20' AS TIMESTAMP) THEN 0 WHEN \"CreatedAt\">CAST('2022-12-20' AS TIMESTAMP) THEN 1 ELSE -1 END) AS CreatedAtCompare FROM \"sys_user\"");
    }
    [Fact]
    public void CompareTo()
    {
        using var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .Select(f => new
            {
                IntCompare = f.Id.CompareTo(1),
                StringCompare = f.OrderNo.CompareTo("OrderNo-001"),
                DateTimeCompare = f.CreatedAt.CompareTo(DateTime.Parse("2022-12-20")),
                BooleanCompare = f.IsEnabled.CompareTo(false)
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT (CASE WHEN `Id`=1 THEN 0 WHEN `Id`>1 THEN 1 ELSE -1 END) AS IntCompare,(CASE WHEN `OrderNo`='OrderNo-001' THEN 0 WHEN `OrderNo`>'OrderNo-001' THEN 1 ELSE -1 END) AS StringCompare,(CASE WHEN `CreatedAt`=CAST('2022-12-20' AS DATETIME) THEN 0 WHEN `CreatedAt`>CAST('2022-12-20' AS DATETIME) THEN 1 ELSE -1 END) AS DateTimeCompare,(CASE WHEN `IsEnabled`=0 THEN 0 WHEN `IsEnabled`>0 THEN 1 ELSE -1 END) AS BooleanCompare FROM `sys_order`");
    }
    [Fact]
    public void Trims()
    {
        using var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .Select(f => new
            {
                Trim = "Begin_" + f.OrderNo.Trim() + "  123   ".Trim() + "_End",
                TrimStart = "Begin_" + f.OrderNo.TrimStart() + "  123   ".TrimStart() + "_End",
                TrimEnd = "Begin_" + f.OrderNo.TrimEnd() + "  123   ".TrimEnd() + "_End"
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT CONCAT('Begin_',LTRIM(RTRIM(`OrderNo`)),LTRIM(RTRIM('  123   ')),'_End') AS Trim,CONCAT('Begin_',LTRIM(`OrderNo`),LTRIM('  123   '),'_End') AS TrimStart,CONCAT('Begin_',RTRIM(`OrderNo`),RTRIM('  123   '),'_End') AS TrimEnd FROM `sys_order`");

        repository.Delete<Order>(new[] { 1, 2, 3 });
        repository.Create<Order>(new[]
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
        Assert.True(result.Count == 3);
        Assert.True(result[0].Trim == "Begin_ON-001123_End");
        Assert.True(result[0].TrimStart == "Begin_ON-001 123   _End");
        Assert.True(result[0].TrimEnd == "Begin_ ON-001  123_End");
    }
    [Fact]
    public void ToUpper_ToLower()
    {
        using var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .Select(f => new
            {
                Col1 = f.OrderNo.ToLower() + "_AbCd".ToUpper(),
                Col2 = f.OrderNo.ToUpper() + "_AbCd".ToLower()
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT CONCAT(LOWER(`OrderNo`),UPPER('_AbCd')) AS Col1,CONCAT(UPPER(`OrderNo`),LOWER('_AbCd')) AS Col2 FROM `sys_order`");

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
}
