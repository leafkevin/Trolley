using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Trolley.MySqlConnector;
using Xunit;

namespace Trolley.Test.MySql;

public class MySqlSqlMethodUnitTest : UnitTestBase
{
    public MySqlSqlMethodUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register<MySqlProvider>("fengling", true, f =>
            {
                f.Add("Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;", true);
            })
            .AddTypeHandler<JsonTypeHandler>()
            .Configure<MySqlProvider, MySqlModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();
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
        Assert.True(sql == "SELECT `Id` FROM `sys_user` WHERE `Id` IN (1,2,3)");

        sql = repository.From<User>()
            .Where(f => Sql.In(f.CreatedAt, new DateTime[] { DateTime.Parse("2023-03-03"), DateTime.Parse("2023-03-03 00:00:00"), DateTime.Parse("2023-03-03 06:06:06") }))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id` FROM `sys_user` WHERE `CreatedAt` IN ('2023-03-03 00:00:00.0000000','2023-03-03 00:00:00.0000000','2023-03-03 06:06:06.0000000')");
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
        Assert.True(sql == "SELECT CONCAT(LOWER(`OrderNo`),'_ABCD') AS `Col1`,CONCAT(UPPER(`OrderNo`),'_abcd') AS `Col2` FROM `sys_order`");

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
    }
}
