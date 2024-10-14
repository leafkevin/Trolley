using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trolley.MySqlConnector;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.MySqlConnector;

public class UnitTest7 : UnitTestBase
{
    private readonly ITestOutputHelper output;
    public UnitTest7(ITestOutputHelper output)
    {
        this.output = output;
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
            var builder = new OrmDbFactoryBuilder()
                .Register(OrmProviderType.MySql, "fengling", connectionString, f =>
                {
                    //两个读库
                    var connectionString1 = "Server=localhost;Database=fengling1;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
                    var connectionString2 = "Server=localhost;Database=fengling2;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
                    f.UseSlave(connectionString1, connectionString2)
                    .AsDefaultDatabase();
                })
                .Configure<ModelConfiguration>(OrmProviderType.MySql)
                .UseInterceptors(df =>
                {
                    df.OnConnectionCreated += evt =>
                    {
                        Interlocked.Increment(ref connTotal);
                        this.output.WriteLine($"Connection {evt.ConnectionId} Created, Total:{Volatile.Read(ref connTotal)}");
                    };
                    df.OnConnectionOpened += evt =>
                    {
                        Interlocked.Increment(ref connOpenTotal);
                        this.output.WriteLine($"Connection {evt.ConnectionId} Opened, Total:{Volatile.Read(ref connOpenTotal)}");
                    };
                    df.OnConnectionClosed += evt =>
                    {
                        Interlocked.Decrement(ref connOpenTotal);
                        Interlocked.Decrement(ref connTotal);
                        this.output.WriteLine($"Connection {evt.ConnectionId} Closed, Total:{Volatile.Read(ref connOpenTotal)}");
                    };
                    df.OnCommandExecuting += evt =>
                    {
                        this.output.WriteLine($"{evt.SqlType} Begin, TransactionId:{evt.TransactionId} Sql: {evt.Sql}, Parameters: {evt.DbParameters.ToMySqlParametersString()}");
                    };
                    df.OnCommandExecuted += evt =>
                    {
                        this.output.WriteLine($"{evt.SqlType} End, TransactionId:{evt.TransactionId} Elapsed: {evt.Elapsed} ms, Sql: {evt.Sql}, Parameters: {evt.DbParameters.ToMySqlParametersString()}");
                    };
                    df.OnTransactionCreated += evt =>
                    {
                        Interlocked.Increment(ref tranTotal);
                        this.output.WriteLine($"Transaction {evt.TransactionId} Created, Total:{Volatile.Read(ref tranTotal)}");
                    };
                    df.OnTransactionCompleted += evt =>
                    {
                        Interlocked.Decrement(ref tranTotal);
                        this.output.WriteLine($"Transaction {evt.TransactionId} {evt.Action} Completed, Transaction Total:{Volatile.Read(ref tranTotal)}");
                    };
                });
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public void TableSchema()
    {
        var repository = this.dbFactory.Create();
        var sql = repository
            .From(f => f.From<OrderDetail>()
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
        Assert.Equal("SELECT a.`OrderId`,a.`BuyerId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `myschema`.`sys_order_detail` a INNER JOIN `myschema`.`sys_order` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `myschema`.`sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1", sql);

        var result = repository
            .From(f => f.From<OrderDetail>()
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
        if (result.Count > 0)
        {
            Assert.NotNull(result[0]);
            Assert.NotNull(result[0].Group);
            Assert.NotNull(result[0].Buyer);
            Assert.True(result[0].ProductCount > 1);
        }
    }
    [Fact]
    public async Task Query_WithNextCte()
    {
        this.Initialize();
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
            .From(myCteTable1)
            .InnerJoin(myCteTable2, (a, b) => a.Id == b.Id)
            .Select((a, b) => new { b.Id, a.Name, b.ParentId, b.Url })
            .ToSql(out _);
        Assert.Equal(@"WITH RECURSIVE `myCteTable1`(`Id`,`Name`,`ParentId`) AS 
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
            .From(myCteTable2)
            .InnerJoin(myCteTable1, (a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, b.Name, a.ParentId, a.Url })
            .ToList();
        Assert.NotNull(result1);
        Assert.True(result1.Count > 0);

        int pageId = 1;
        sql = repository
            .From(menuList)
            .WithTable(x => x.From<Page>()
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
        Assert.Equal(@"WITH RECURSIVE `MenuList`(`Id`,`Name`,`ParentId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a WHERE a.`Id`=@RootId UNION ALL
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a INNER JOIN `MenuList` b ON a.`ParentId`=b.`Id`
)
SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `MenuList` a INNER JOIN (SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `sys_menu` b ON a.`Id`=b.`PageId` WHERE a.`Id`=@p1 UNION ALL
SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `MenuList` b ON a.`Id`=b.`Id` WHERE a.`Id`>@p2) b ON a.`Id`=b.`Id`", sql);
        var result2 = repository
            .From(menuList)
            .WithTable(x => x.From<Page>()
                    .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
                    .Where((a, b) => a.Id == pageId)
                    .Select((x, y) => new { y.Id, x.Url })
                .UnionAll(x => x.From<Page>()//.WithTable(self)
                    .InnerJoin(menuList, (a, b) => a.Id == b.Id)
                    .Where((a, b) => a.Id > pageId)
                    .Select((x, y) => new { y.Id, x.Url })))
            .InnerJoin((a, b) => a.Id == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
            .ToList();
        Assert.NotNull(result2);
        Assert.True(result2.Count > 0);

        sql = repository
            .From(menuList)
            .WithTable(x => x.From<Page>()
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
        Assert.Equal(@"WITH RECURSIVE `MenuList`(`Id`,`Name`,`ParentId`) AS 
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
            .From(menuList)
            .WithTable(x => x.From<Page>()
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
        Assert.NotNull(result3);
        Assert.True(result3.Count > 0);
    }
    [Fact]
    public async Task Update_SetBulk_OnlyFields()
    {
        Initialize();
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
        Assert.Equal("UPDATE `sys_order_detail` SET `Price`=@Price0,`Quantity`=@Quantity0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Price`=@Price1,`Quantity`=@Quantity1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Price`=@Price2,`Quantity`=@Quantity2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Price`=@Price3,`Quantity`=@Quantity3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Price`=@Price4,`Quantity`=@Quantity4 WHERE `Id`=@kId4", sql);
        Assert.Equal(parameters.Count * 3, dbParameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.Equal($"@Price{i}", dbParameters[i * 3].ParameterName);
            Assert.Equal($"@Quantity{i}", dbParameters[i * 3 + 1].ParameterName);
            Assert.Equal($"@kId{i}", dbParameters[i * 3 + 2].ParameterName);
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
        Assert.True(result == parameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.True(updatedDetails[i].Price == parameters[i].Price);
            Assert.True(updatedDetails[i].Quantity == parameters[i].Quantity);
            Assert.True(updatedDetails[i].Amount != parameters[i].Amount);
        }
    }
}
