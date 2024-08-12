using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trolley.MySqlConnector;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.MySqlConnector;

public class UnitTest5 : UnitTestBase
{
    private static int connTotal = 0;
    private static int connOpenTotal = 0;
    private readonly ITestOutputHelper output;
    public UnitTest5(ITestOutputHelper output)
    {
        this.output = output;
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
                .Register(OrmProviderType.MySql, "fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;", true)
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
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async Task MultipleQuery()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var reader = await repository.QueryMultipleAsync(f => f
            .Get<User>(new { Id = 1 })
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
            .From(f => f.From<Order, OrderDetail>('a')
                    .Where((a, b) => a.Id == b.OrderId && a.Id == "1")
                    .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                    .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                    .Select((x, a, b) => new { a.Id, x.Grouping, ProductTotal = Sql.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
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
        Assert.NotNull(userInfo);
        Assert.True(userInfo.Id == 1);
        Assert.True(orderInfo.Id == "1");
        Assert.True(groupedOrderInfo.Id == "1");
        Assert.True(groupedOrderInfo.Grouping.OrderId == "1");
    }
    [Fact]
    public async Task MultipleCommand()
    {
        using var repository = dbFactory.Create();
        int[] productIds = new int[] { 2, 4, 5, 6 };
        int category = 1;
        var commands = new List<MultipleCommand>();
        var deleteCommand = repository.Delete<Product>()
            .Where(f => productIds.Contains(f.Id))
            .ToMultipleCommand();

        var insertCommand = repository.Create<Product>()
           .WithBy(new
           {
               Id = 2,
               ProductNo = "PN_111",
               Name = "PName_111",
               BrandId = 1,
               CategoryId = category,
               CompanyId = 1,
               IsEnabled = true,
               CreatedBy = 1,
               CreatedAt = DateTime.Now,
               UpdatedBy = 1,
               UpdatedAt = DateTime.Now
           })
           .ToMultipleCommand();

        var insertCommand2 = repository.Create<Product>()
            .WithBulk(new[]
            {
                new
                {
                    Id = 4,
                    ProductNo="PN-004",
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
                    Id = 5,
                    ProductNo="PN-005",
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
                    Id = 6,
                    ProductNo="PN-006",
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
            .OnlyFields(f => new { f.Id, f.ProductNo, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
            .ToMultipleCommand();

        var updateCommand = repository.Update<Order>()
           .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
           .Set(true, (x, y) => new
           {
               TotalAmount = 200.56,
               OrderNo = x.OrderNo + "-111",
               BuyerSource = y.SourceType
           })
           .Set(x => x.Products, new List<int> { 1, 2, 3 })
           .Where((a, b) => a.Id == "1")
           .ToMultipleCommand();

        var orderDetails = await repository.From<OrderDetail>().ToListAsync();
        var parameters = orderDetails.Select(f => new
        {
            f.Id,
            Amount = f.Amount + 50,
            UpdatedAt = f.UpdatedAt.AddDays(1)
        })
        .ToList();
        var bulkUpdateCommand = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .Set(f => f.ProductId, 3)
            .Set(new { Quantity = 5 })
            .Set(f => new { Price = f.Price + 10 })
            .ToMultipleCommand();

        commands.AddRange(new[] { deleteCommand, insertCommand, insertCommand2, updateCommand, bulkUpdateCommand });
        var count = repository.MultipleExecute(commands);
        Assert.True(count > 0);
    }
}
