using Microsoft.Extensions.DependencyInjection;
using System;
using Trolley.MySqlConnector;
using Xunit;

namespace Trolley.Test.MySql;

public class MySqlUnitTest5 : UnitTestBase
{
    public MySqlUnitTest5()
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
    public async void QueryMutil()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var reader = await repository.QueryMultipleAsync(f => f
            .Get<User>(new { Id = 1 })            
            .Exists<Order>(f => f.BuyerId.IsNull())
            //.Execute("DELETE FROM sys_page where Id=10")
            .From<Order>()
                .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
                .Where((x, y) => x.Id == 1)
                .Select((x, y) => new { x.Id, x.OrderNo, x.BuyerId, BuyerName = y.Name, x.TotalAmount })
                .First()
            .QueryFirst<User>(new { Id = 2 })
            .Update<Order>()
                .Set(t => t.Disputes, new Dispute
                {
                    Id = 1,
                    Content = "无良商家，投诉，投诉",
                    Result = "同意更换",
                    Users = "Buyer1,Seller1",
                    CreatedAt = DateTime.Now
                })
                .SetWith(new { BuyerId = 2, Products = new int[] { 1, 2, 3 } })
                .SetFrom(t => t.TotalAmount, (x, y) => x.From<OrderDetail>('b')
                    .Where(x => x.OrderId == y.Id)
                    .SelectAggregate((x, y) => x.Sum(y.Amount)))
                .Where(t => t.Id == 1)
                .Execute()
            .From<Product>()
                .Include(f => f.Brand)
                .Where(f => f.ProductNo.Contains("PN-00"))
                .ToList()
            .From(f => f
                .From<Order, OrderDetail>('a')
                .Where((a, b) => a.Id == b.OrderId && a.Id == 1)
                .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
                .Select((x, a, b) => new { x.Grouping, ProductTotal = Sql.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
                .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
                .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
                .First());
        var sql = reader.ToSql(out var dbParameters);
        var userInfo = await reader.ReadFirstAsync<User>();        
        var isExists = await reader.ReadFirstAsync<bool>();
        //var deletedCount = await reader.ReadFirstAsync<int>();
        var orderInfo = await reader.ReadFirstAsync<dynamic>();
        var userInfo2 = await reader.ReadFirstAsync<User>();
        var updatedCount = await reader.ReadFirstAsync<int>();
        var products = await reader.ReadAsync<Product>();
        var groupedOrderInfo = await reader.ReadFirstAsync<dynamic>();
        Assert.NotNull(userInfo);
        Assert.True(userInfo.Id == 1);
    }
}
