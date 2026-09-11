using MySqlConnector;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Trolley.Test.MySqlConnector;

[TestFixture]
public class UnitTest2 : UnitTestBase
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
    public async Task QueryFirst()
    {
        var repository = this.dbFactory.Create();
        var result1 = repository.QueryFirst<User>(new { Name = "leafkevin" });
        Assert.AreEqual("leafkevin", result1.Name);
        var result2 = repository.QueryFirst<User>(f => f.Id == 1);
        Assert.AreEqual("leafkevin", result2.Name);
        var result3 = repository.QueryFirst<User>("SELECT * FROM sys_user where Id=1");
        Assert.AreEqual("leafkevin", result3.Name);
        var result4 = repository.QueryFirst<User>("SELECT * FROM sys_user where Id=@Id", new { Id = 1 });
        Assert.AreEqual("leafkevin", result4.Name);
        var result5 = repository.QueryFirst<User>("SELECT * FROM sys_user where Id=@Id", new List<IDbDataParameter> { new MySqlParameter("@Id", MySqlDbType.Int32) { Value = 1 } });
        Assert.AreEqual("leafkevin", result5.Name);
        var result6 = repository.QueryFirst<User>(new[] { new { Name = "leafkevin" }, new { Name = "cindy" } });
        Assert.AreEqual("leafkevin", result6.Name);

        result1 = await repository.QueryFirstAsync<User>(new { Name = "leafkevin" });
        Assert.AreEqual("leafkevin", result1.Name);
        result2 = await repository.QueryFirstAsync<User>(f => f.Id == 1);
        Assert.AreEqual("leafkevin", result2.Name);
        result3 = await repository.QueryFirstAsync<User>("SELECT * FROM sys_user where Id=1");
        Assert.AreEqual("leafkevin", result3.Name);
        result4 = await repository.QueryFirstAsync<User>("SELECT * FROM sys_user where Id=@Id", new { Id = 1 });
        Assert.AreEqual("leafkevin", result4.Name);
        result5 = await repository.QueryFirstAsync<User>("SELECT * FROM sys_user where Id=@Id", new List<IDbDataParameter> { new MySqlParameter("@Id", MySqlDbType.Int32) { Value = 1 } });
        Assert.AreEqual("leafkevin", result5.Name);
        result6 = await repository.QueryFirstAsync<User>(new[] { new { Name = "leafkevin" }, new { Name = "cindy" } });
        Assert.AreEqual("leafkevin", result6.Name);
    }
    [Test]
    public async Task QueryById()
    {
        var repository = this.dbFactory.Create();
        var result1 = repository.QueryById<User>(1);
        Assert.AreEqual("leafkevin", result1.Name);
        var result2 = repository.QueryById<User>(new { Id = 1 });
        Assert.AreEqual("leafkevin", result2.Name);
        var result3 = repository.QueryById<User>(new[] { 1, 2, 3 });
        Assert.AreEqual("leafkevin", result3.Name);
        var result4 = repository.QueryById<User>(new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } });
        Assert.AreEqual("leafkevin", result4.Name);

        result1 = await repository.QueryByIdAsync<User>(1);
        Assert.AreEqual("leafkevin", result1.Name);
        result2 = await repository.QueryByIdAsync<User>(new { Id = 1 });
        Assert.AreEqual("leafkevin", result2.Name);
        result3 = await repository.QueryByIdAsync<User>(new[] { 1, 2, 3 });
        Assert.AreEqual("leafkevin", result3.Name);
        result4 = await repository.QueryByIdAsync<User>(new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } });
        Assert.AreEqual("leafkevin", result4.Name);
    }
    [Test]
    public async Task QueryByIds()
    {
        var repository = this.dbFactory.Create();
        var userIds = new int[] { 1, 2, 3 };
        var result1 = repository.QueryByIds<User>(userIds);
        Assert.AreEqual(userIds.Length, result1.Count);
        Assert.AreEqual(result1[0].Id, userIds[0]);
        Assert.AreEqual(result1[1].Id, userIds[1]);
        Assert.AreEqual(result1[2].Id, userIds[2]);

        var result2 = await repository.QueryByIdsAsync<User>(new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } });
        Assert.AreEqual(userIds.Length, result2.Count);
        Assert.AreEqual(result2[0].Id, userIds[0]);
        Assert.AreEqual(result2[1].Id, userIds[1]);
        Assert.AreEqual(result2[2].Id, userIds[2]);
    }
    [Test]
    public async Task Query()
    {
        var repository = this.dbFactory.Create();
        var result1 = repository.Query<User>(new { IsEnabled = true });
        result1.ForEach(f => Assert.IsTrue(f.IsEnabled));
        var result2 = repository.Query<User>(f => f.IsEnabled);
        result2.ForEach(f => Assert.IsTrue(f.IsEnabled));
        var result3 = repository.Query<User>("SELECT * FROM sys_user where IsEnabled=1");
        result3.ForEach(f => Assert.IsTrue(f.IsEnabled));
        var result4 = repository.Query<User>("SELECT * FROM sys_user where IsEnabled=@IsEnabled", new { IsEnabled = true });
        result4.ForEach(f => Assert.IsTrue(f.IsEnabled));
        var result5 = repository.Query<User>("SELECT * FROM sys_user where IsEnabled=@IsEnabled", new List<IDbDataParameter> { new MySqlParameter("@IsEnabled", MySqlDbType.Bool) { Value = true } });
        result5.ForEach(f => Assert.IsTrue(f.IsEnabled));
        var result6 = repository.Query<User>(new[] { new { Name = "leafkevin" }, new { Name = "cindy" } });
        Assert.AreEqual(2, result6.Count);
        Assert.AreEqual("leafkevin", result6[0].Name);
        Assert.AreEqual("cindy", result6[1].Name);

        result1 = await repository.QueryAsync<User>(new { IsEnabled = true });
        result1.ForEach(f => Assert.IsTrue(f.IsEnabled));
        result2 = await repository.QueryAsync<User>(f => f.IsEnabled);
        result2.ForEach(f => Assert.IsTrue(f.IsEnabled));
        result3 = await repository.QueryAsync<User>("SELECT * FROM sys_user where IsEnabled=1");
        result3.ForEach(f => Assert.IsTrue(f.IsEnabled));
        result4 = await repository.QueryAsync<User>("SELECT * FROM sys_user where IsEnabled=@IsEnabled", new { IsEnabled = true });
        result4.ForEach(f => Assert.IsTrue(f.IsEnabled));
        result5 = await repository.QueryAsync<User>("SELECT * FROM sys_user where IsEnabled=@IsEnabled", new List<IDbDataParameter> { new MySqlParameter("@IsEnabled", MySqlDbType.Bool) { Value = true } });
        result5.ForEach(f => Assert.IsTrue(f.IsEnabled));
        result6 = await repository.QueryAsync<User>(new[] { new { Name = "leafkevin" }, new { Name = "cindy" } });
        Assert.AreEqual(2, result6.Count);
        Assert.AreEqual("leafkevin", result6[0].Name);
        Assert.AreEqual("cindy", result6[1].Name);
    }
    [Test]
    public async Task QueryPage()
    {
        var repository = this.dbFactory.Create();
        var result1 = repository.From<OrderDetail>()
            .Where(f => f.IsEnabled)
            .OrderByDescending(f => f.CreatedAt)
            .Page(2, 2)
            .ToPageList();
        Assert.AreEqual(result1.Count, result1.Data.Count);
        Assert.AreEqual(2, result1.Count);

        var result2 = await repository.From<OrderDetail>()
           .Where(f => f.IsEnabled)
           .OrderByDescending(f => f.CreatedAt)
           .Page(2, 2)
           .ToPageListAsync();
        Assert.AreEqual(result2.Count, result2.Data.Count);
        Assert.AreEqual(2, result2.Count);

        var count1 = repository.From<OrderDetail>().Where(f => f.IsEnabled).Count();
        var count2 = await repository.From<OrderDetail>().Where(f => f.IsEnabled).CountAsync();
        Assert.AreEqual(count1, result1.TotalCount);
        Assert.AreEqual(count1, result2.TotalCount);
        Assert.AreEqual(count2, count1);

        var result3 = repository.From<OrderDetail>()
            .Where(f => f.IsEnabled)
            .OrderByDescending(f => f.CreatedAt)
            .Page(2, 2)
            .ToList();
        Assert.AreEqual(2, result3.Count);
        Assert.AreEqual(result1.Data[0].Id, result3[0].Id);
        Assert.AreEqual(result1.Data[1].Id, result3[1].Id);

        var result4 = await repository.From<OrderDetail>()
            .Where(f => f.IsEnabled)
            .OrderByDescending(f => f.CreatedAt)
            .Page(2, 2)
            .ToListAsync();
        Assert.AreEqual(2, result4.Count);
        Assert.AreEqual(result2.Data[0].Id, result4[0].Id);
        Assert.AreEqual(result2.Data[1].Id, result4[1].Id);
    }
    [Test]
    public async Task FromQuery_Simple_NotSelect()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Order>()
            .Where(f => f.ProductCount > 1)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order` a WHERE a.`ProductCount`>1", sql1);
        var result1 = repository.From<Order>()
            .Where(f => f.ProductCount > 1)
            .First();
        Assert.Greater(result1.ProductCount, 1);

        var result2 = repository.From<Order>()
            .Where(f => f.ProductCount > 1)
            .ToList();
        Assert.IsNotEmpty(result2);
            result2.ForEach(f => Assert.Greater(f.ProductCount, 1));

        result1 = await repository.From<Order>()
           .Where(f => f.ProductCount > 1)
           .FirstAsync();
        Assert.Greater(result1.ProductCount, 1);

        result2 = await repository.From<Order>()
            .Where(f => f.ProductCount > 1)
            .ToListAsync();
        Assert.IsNotEmpty(result2);
            result2.ForEach(f => Assert.Greater(f.ProductCount, 1));
    }
    [Test]
    public async Task FromQuery_Simple_HasSelect()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Order>()
            .Where(f => f.TotalAmount > 50)
            .Select(f => new
            {
                f.Id,
                f.OrderNo,
                f.TotalAmount,
                f.ProductCount
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`OrderNo`,a.`TotalAmount`,a.`ProductCount` FROM `sys_order` a WHERE a.`TotalAmount`>50", sql1);
        var result1 = repository.From<Order>()
            .Where(f => f.TotalAmount > 50)
            .Select(f => new
            {
                f.Id,
                f.OrderNo,
                f.TotalAmount,
                f.ProductCount
            })
            .First();
        Assert.Greater(result1.TotalAmount, 50);

        var result2 = repository.From<Order>()
            .Where(f => f.TotalAmount > 50)
            .Select(f => new
            {
                f.Id,
                f.OrderNo,
                f.TotalAmount,
                f.ProductCount
            })
            .ToList();
        Assert.IsNotEmpty(result2);
            result2.ForEach(f => Assert.Greater(f.TotalAmount, 50));

        result1 = await repository.From<Order>()
            .Where(f => f.TotalAmount > 50)
            .Select(f => new
            {
                f.Id,
                f.OrderNo,
                f.TotalAmount,
                f.ProductCount
            })
            .FirstAsync();
        Assert.Greater(result1.TotalAmount, 50);

        result2 = await repository.From<Order>()
            .Where(f => f.TotalAmount > 50)
            .Select(f => new
            {
                f.Id,
                f.OrderNo,
                f.TotalAmount,
                f.ProductCount
            })
            .ToListAsync();
        Assert.IsNotEmpty(result2);
            result2.ForEach(f => Assert.Greater(f.TotalAmount, 50));
    }
    [Test]
    public async Task FromQuery_Simple_HasGroupBy()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Order>()
            .GroupBy(f => f.BuyerId)
            .Select((x, f) => new
            {
                f.BuyerId,
                TotalCount = x.Count(f.Id),
                TotalAmount = x.Sum(f.TotalAmount)
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`BuyerId`,COUNT(a.`Id`) AS `TotalCount`,SUM(a.`TotalAmount`) AS `TotalAmount` FROM `sys_order` a GROUP BY a.`BuyerId`", sql1);
        var result1 = repository.From<Order>()
            .GroupBy(f => f.BuyerId)
            .Select((x, f) => new
            {
                f.BuyerId,
                TotalCount = x.Count(f.Id),
                TotalAmount = x.Sum(f.TotalAmount)
            })
            .First();
        Assert.IsNotNull(result1);
        result1 = await repository.From<Order>()
            .GroupBy(f => f.BuyerId)
            .Select((x, f) => new
            {
                f.BuyerId,
                TotalCount = x.Count(f.Id),
                TotalAmount = x.Sum(f.TotalAmount)
            })
            .FirstAsync();
        Assert.IsNotNull(result1);

        var sql2 = repository.From<Order>()
            .GroupBy(f => f.BuyerId)
            .Select((x, f) => new
            {
                f.BuyerId,
                TotalCount = x.Count(f.Id),
                TotalAmount = x.Sum(f.TotalAmount)
            })
            .OrderBy(f => f.BuyerId)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`BuyerId`,COUNT(a.`Id`) AS `TotalCount`,SUM(a.`TotalAmount`) AS `TotalAmount` FROM `sys_order` a GROUP BY a.`BuyerId` ORDER BY a.`BuyerId`", sql2);

        var result2 = repository.From<Order>()
            .GroupBy(f => f.BuyerId)
            .Select((x, f) => new
            {
                f.BuyerId,
                TotalCount = x.Count(f.Id),
                TotalAmount = x.Sum(f.TotalAmount)
            })
            .OrderBy(f => f.BuyerId)
            .ToList();
        Assert.IsNotEmpty(result2);
    }
    [Test]
    public async Task FromQuery_Simple_HasGroupBy_OrderBy()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Order>()
            .GroupBy(f => new { f.CreatedAt.Date })
            .OrderBy((x, f) => f.CreatedAt.Date)
            .Select((x, f) => new
            {
                f.CreatedAt.Date,
                TotalCount = x.Count(f.Id),
                TotalAmount = x.Sum(f.TotalAmount)
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT CONVERT(a.`CreatedAt`,DATE) AS `Date`,COUNT(a.`Id`) AS `TotalCount`,SUM(a.`TotalAmount`) AS `TotalAmount` FROM `sys_order` a GROUP BY CONVERT(a.`CreatedAt`,DATE) ORDER BY `Date`", sql1);

        var result1 = repository.From<Order>()
            .GroupBy(f => new { f.CreatedAt.Date })
            .OrderBy((x, f) => f.CreatedAt.Date)
            .Select((x, f) => new
            {
                f.CreatedAt.Date,
                TotalCount = x.Count(f.Id),
                TotalAmount = x.Sum(f.TotalAmount)
            })
            .ToList();
        Assert.IsNotEmpty(result1);

        var sql2 = repository.From<Order>()
            .GroupBy(f => new { f.CreatedAt.Date })
            .OrderBy((x, f) => f.Id)
            .Select((x, f) => new
            {
                f.CreatedAt.Date,
                TotalCount = x.Count(f.Id),
                TotalAmount = x.Sum(f.TotalAmount)
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT CONVERT(a.`CreatedAt`,DATE) AS `Date`,COUNT(a.`Id`) AS `TotalCount`,SUM(a.`TotalAmount`) AS `TotalAmount` FROM `sys_order` a GROUP BY CONVERT(a.`CreatedAt`,DATE) ORDER BY a.`Id`", sql2);

        var result2 = repository.From<Order>()
            .GroupBy(f => f.CreatedAt.Date)
            .Select((x, f) => new
            {
                x.Grouping,
                TotalCount = x.Count(f.Id),
                TotalAmount = x.Sum(f.TotalAmount)
            })
            .OrderBy(f => f.Grouping)
            .ToList();
        Assert.IsNotEmpty(result2);

        var sql3 = repository.From<Order>()
            .GroupBy(f => f.CreatedAt.Date)
            .Select((x, f) => new
            {
                x.Grouping,
                TotalCount = x.Count(f.Id),
                TotalAmount = x.Sum(f.TotalAmount)
            })
            .OrderBy(f => f.Grouping)
        .ToSql(out _);
        Assert.AreEqual("SELECT CONVERT(a.`CreatedAt`,DATE) AS `Grouping`,COUNT(a.`Id`) AS `TotalCount`,SUM(a.`TotalAmount`) AS `TotalAmount` FROM `sys_order` a GROUP BY CONVERT(a.`CreatedAt`,DATE) ORDER BY `Grouping`", sql3);

        var result3 = repository.From<Order>()
            .GroupBy(f => f.CreatedAt.Date)
            .Select((x, f) => new
            {
                x.Grouping,
                TotalCount = x.Count(f.Id),
                TotalAmount = x.Sum(f.TotalAmount)
            })
            .OrderBy(f => f.Grouping)
            .ToList();
        Assert.IsNotEmpty(result3);
    }
    [Test]
    public void FromQuery_Select()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository
            .From<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Id,
                x.BuyerId,
                Buyer = y,
                x.ProductCount
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`BuyerId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1", sql1);
        var result1 = repository
            .From<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Id,
                x.BuyerId,
                Buyer = y,
                x.ProductCount
            })
            .ToList();
        Assert.IsNotEmpty(result1);
        {
            result1.ForEach(f => Assert.IsNotNull(f.Buyer));
            result1.ForEach(f => Assert.Greater(f.ProductCount, 1));
        }
    }
    class OrderBuyerInfo
    {
        public string OrderId { get; set; }
        public string OrderNo { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int ProductTotal { get; set; }
    }
    [Test]
    public void FromQuery_SubQuery()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository
            .FromQuery(f => f.From<OrderDetail>()
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .GroupBy((a, b) => new { b.Id, b.BuyerId })
                .Select((x, a, b) => new { b.Id, b.BuyerId, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Id,
                x.BuyerId,
                Buyer = y,
                x.ProductCount
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`BuyerId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail` a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1", sql1);
        var result1 = repository
            .FromQuery(f => f.From<OrderDetail>()
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .GroupBy((a, b) => new { b.Id, b.BuyerId })
                .Select((x, a, b) => new { b.Id, b.BuyerId, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Id,
                x.BuyerId,
                Buyer = y,
                x.ProductCount
            })
            .ToList();
        Assert.IsNotEmpty(result1);
        {
            result1.ForEach(f => Assert.IsNotNull(f.Buyer));
            result1.ForEach(f => Assert.Greater(f.ProductCount, 1));
        }

        var sql2 = repository
            .FromQuery(f => f.From<OrderDetail>()
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                Grouping = x.Group,
                Buyer = y,
                NewProductCount = x.ProductCount
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`OrderId`,a.`BuyerId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` AS `NewProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail` a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1", sql2);

        var result2 = repository
            .FromQuery(f => f.From<OrderDetail>()
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                Grouping = x.Group,
                Buyer = y,
                NewProductCount = x.ProductCount
            })
            .ToList();
        Assert.IsNotEmpty(result2);
        {
            Assert.IsNotNull(result2[0]);
            Assert.IsNotNull(result2[0].Grouping);
            Assert.IsNotNull(result2[0].Buyer);
            result2.ForEach(f => Assert.Greater(f.NewProductCount, 1));
        }
    }
    [Test]
    public void FromQuery_SubQuery1()
    {
        var repository = this.dbFactory.Create();
        var sql = repository
            .FromQuery(f => f.From<Page, Menu>('o')
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
        var sql1 = repository
            .FromQuery(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => x.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = a.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .Include((a, b) => b.Details)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .ToSql(out var dbParameters1);
        Assert.AreEqual("SELECT b.`Disputes`,a.`BuyerId`,a.`OrderId`,a.`OrderNo`,a.`ProductTotal`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM (SELECT b.`BuyerId`,b.`Id` AS `OrderId`,b.`OrderNo`,COUNT(DISTINCT c.`ProductId`) AS `ProductTotal` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` LEFT JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` GROUP BY b.`BuyerId`,b.`Id`,b.`OrderNo` HAVING COUNT(DISTINCT c.`ProductId`)>@p0) a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id`", sql1);
        Assert.AreEqual(count, (int)dbParameters1[0].Value);

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
            Assert.IsNotNull(result.Disputes);
            Assert.IsNotNull(result.Order);
            Assert.IsNotNull(result.Order.Details);
            Assert.IsNotEmpty(result.Order.Details);
            Assert.Greater(result.Order.Details[0].Amount, 0);
        }

        var amount = 100;
        var sql2 = repository
            .FromQuery(f => f.From<User>()
                 .InnerJoin<Order>((a, b) => a.Id == b.BuyerId)
                 .LeftJoin<OrderDetail>((a, b, c) => b.Id == c.OrderId)
                 .GroupBy((a, b, c) => new { b.BuyerId, OrderId = b.Id, b.OrderNo })
                 .Having((x, a, b, c) => x.CountDistinct(c.ProductId) > count)
                 .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, c.OrderNo, ProductTotal = a.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .IncludeMany((a, b) => b.Details, f => f.Amount > amount)
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .ToSql(out var dbParameters2);
        Assert.AreEqual("SELECT b.`Disputes`,a.`BuyerId`,a.`OrderId`,a.`OrderNo`,a.`ProductTotal`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM (SELECT b.`BuyerId`,b.`Id` AS `OrderId`,b.`OrderNo`,COUNT(DISTINCT c.`ProductId`) AS `ProductTotal` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` LEFT JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` GROUP BY b.`BuyerId`,b.`Id`,b.`OrderNo` HAVING COUNT(DISTINCT c.`ProductId`)>@p0) a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id`", sql2);
        Assert.AreEqual(1, dbParameters2.Count);
        Assert.AreEqual(count, (int)dbParameters2[0].Value);

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
            Assert.IsNotNull(result.Disputes);
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
        var sql1 = repository
            .FromQuery(f => f.From<Order, OrderDetail>('a')
                .Where((a, b) => a.Id == b.OrderId)
                .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                .Having((x, a, b) => x.CountDistinct(b.ProductId) > 0)
                .Select((x, a, b) => new { x.Grouping, ProductTotal = x.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
            .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
            .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`BuyerId`,a.`OrderId`,a.`BuyerId`,a.`ProductTotal`,b.`Name` AS `BuyerName`,a.`BuyerId1` AS `BuyerId2` FROM (SELECT a.`BuyerId`,a.`Id` AS `OrderId`,COUNT(DISTINCT b.`ProductId`) AS `ProductTotal`,a.`BuyerId` AS `BuyerId1` FROM `sys_order` a,`sys_order_detail` b WHERE a.`Id`=b.`OrderId` GROUP BY a.`BuyerId`,a.`Id` HAVING COUNT(DISTINCT b.`ProductId`)>0) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id`", sql1);

        var result1 = repository
            .FromQuery(f => f.From<Order, OrderDetail>('a')
                .Where((a, b) => a.Id == b.OrderId)
                .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
                .Having((x, a, b) => x.CountDistinct(b.ProductId) > 0)
                .Select((x, a, b) => new { x.Grouping, ProductTotal = x.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
            .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
            .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
            .First();
        if (result1 != null)
        {
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result1.Grouping);
            Assert.IsNotNull(result1.BuyerName);
        }
        var minAmount = 200;
        var minAge = 15;
        using var subQuery = repository.From<Order, OrderDetail>()
            .Where((a, b) => a.Id == b.OrderId && a.TotalAmount > minAmount)
            .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
            .Having((x, a, b) => x.CountDistinct(b.ProductId) > 0)
            .Select((x, a, b) => new
            {
                x.Grouping,
                ProductTotal = x.CountDistinct(b.ProductId),
                BuyerId1 = x.Grouping.BuyerId
            });
        var sql2 = repository
            .FromQuery(subQuery)
            .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
            .Where((x, y) => y.Age >= minAge)
            .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`BuyerId`,a.`OrderId`,a.`BuyerId`,a.`ProductTotal`,b.`Name` AS `BuyerName`,a.`BuyerId1` AS `BuyerId2` FROM (SELECT a.`BuyerId`,a.`Id` AS `OrderId`,COUNT(DISTINCT b.`ProductId`) AS `ProductTotal`,a.`BuyerId` AS `BuyerId1` FROM `sys_order` a,`sys_order_detail` b WHERE a.`Id`=b.`OrderId` AND a.`TotalAmount`>@p0 GROUP BY a.`BuyerId`,a.`Id` HAVING COUNT(DISTINCT b.`ProductId`)>0) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE b.`Age`>=@p1", sql2);

        var result2 = repository
            .FromQuery(subQuery)
            .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
            .Where((x, y) => y.Age >= minAge)
            .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
            .First();
        if (result2 != null)
        {
            Assert.IsNotNull(result2);
            Assert.IsNotNull(result2.Grouping);
            Assert.IsNotNull(result2.BuyerName);
        }

        using var subQuery1 = repository.From<Order, OrderDetail>()
            .Where((a, b) => a.Id == b.OrderId && a.TotalAmount > minAmount)
            .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
            .Having((x, a, b) => x.CountDistinct(b.ProductId) > 0)
            .Select((x, a, b) => new
            {
                x.Grouping,
                ProductTotal = x.CountDistinct(b.ProductId),
                BuyerId1 = x.Grouping.BuyerId
            });
        var sql3 = repository.From<User>()
            .InnerJoin(subQuery1, (x, y) => x.Id == y.Grouping.BuyerId)
            .Where((x, y) => x.Age >= minAge)
            .Select((x, y) => new { y.Grouping, y.Grouping.BuyerId, y.ProductTotal, BuyerName = x.Name, BuyerId2 = y.BuyerId1 })
            .ToSql(out _);
        Assert.AreEqual("SELECT b.`BuyerId`,b.`OrderId`,b.`BuyerId`,b.`ProductTotal`,a.`Name` AS `BuyerName`,b.`BuyerId1` AS `BuyerId2` FROM `sys_user` a INNER JOIN (SELECT a.`BuyerId`,a.`Id` AS `OrderId`,COUNT(DISTINCT b.`ProductId`) AS `ProductTotal`,a.`BuyerId` AS `BuyerId1` FROM `sys_order` a,`sys_order_detail` b WHERE a.`Id`=b.`OrderId` AND a.`TotalAmount`>@p0 GROUP BY a.`BuyerId`,a.`Id` HAVING COUNT(DISTINCT b.`ProductId`)>0) b ON a.`Id`=b.`BuyerId` WHERE a.`Age`>=@p1", sql3);

        var result3 = repository.From<User>()
            .InnerJoin(subQuery1, (x, y) => x.Id == y.Grouping.BuyerId)
            .Where((x, y) => x.Age >= minAge)
            .Select((x, y) => new { y.Grouping, y.Grouping.BuyerId, y.ProductTotal, BuyerName = x.Name, BuyerId2 = y.BuyerId1 })
            .First();
        if (result3 != null)
        {
            Assert.IsNotNull(result3);
            Assert.IsNotNull(result3.Grouping);
            Assert.IsNotNull(result3.BuyerName);
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

        var result = repository
            .From<User, Order, OrderDetail>()
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
    public void FromQuery_SubQuery5()
    {
        this.Initialize(1);
        var menuId = 1;
        var pageId = 1;
        var repository = this.dbFactory.Create();
        using var menuPageList = repository.From<Page, Menu>()
            .Where((a, b) => a.Id == b.PageId && b.Id > menuId.ToParameter("@MenuId"))
            .Select((x, y) => new { MenuId = y.Id, y.ParentId, x.Url })
            .AsRefQueryObj();
        var sql1 = repository.From<Menu>()
            .InnerJoin(menuPageList, (a, b) => a.Id == b.MenuId && a.PageId > pageId)
            .Select((a, b) => new { b.MenuId, a.Name, b.ParentId, a.PageId, b.Url })
            .ToSql(out var dbParameters1);
        Assert.AreEqual("SELECT b.`MenuId`,a.`Name`,b.`ParentId`,a.`PageId`,b.`Url` FROM `sys_menu` a INNER JOIN (SELECT b.`Id` AS `MenuId`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE a.`Id`=b.`PageId` AND b.`Id`>@MenuId) b ON a.`Id`=b.`MenuId` AND a.`PageId`>@p1", sql1);
        Assert.AreEqual(2, dbParameters1.Count);
        Assert.AreEqual("@MenuId", dbParameters1[0].ParameterName);
        Assert.AreEqual(menuId, (int)dbParameters1[0].Value);
        Assert.AreEqual(pageId, (int)dbParameters1[1].Value);

        var result1 = repository.From<Menu>()
            .InnerJoin(menuPageList, (a, b) => a.Id == b.MenuId && a.PageId > pageId)
            .Select((a, b) => new { b.MenuId, a.Name, b.ParentId, a.PageId, b.Url })
            .ToList();
        Assert.IsNotEmpty(result1);
        result1.ForEach(f =>
        {
            Assert.Greater(f.MenuId, menuId);
            Assert.Greater(f.PageId, pageId);
        });

        int parentId = 10;
        var sql2 = repository.From<Menu>()
            .InnerJoin(menuPageList.Where(t => t.ParentId < parentId), (a, b) => a.Id == b.MenuId && a.PageId > pageId)
            .Select((a, b) => new { b.MenuId, a.Name, b.ParentId, a.PageId, b.Url })
            .ToSql(out var dbParameters2);
        Assert.AreEqual("SELECT b.`MenuId`,a.`Name`,b.`ParentId`,a.`PageId`,b.`Url` FROM `sys_menu` a INNER JOIN (SELECT b.`Id` AS `MenuId`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE a.`Id`=b.`PageId` AND b.`Id`>@MenuId AND b.`ParentId`<@p1) b ON a.`Id`=b.`MenuId` AND a.`PageId`>@p2", sql2);
        Assert.AreEqual(3, dbParameters2.Count);
        Assert.AreEqual("@MenuId", dbParameters2[0].ParameterName);
        Assert.AreEqual("@p1", dbParameters2[1].ParameterName);
        Assert.AreEqual("@p2", dbParameters2[2].ParameterName);
        Assert.AreEqual(menuId, (int)dbParameters2[0].Value);
        Assert.AreEqual(parentId, (int)dbParameters2[1].Value);
        Assert.AreEqual(pageId, (int)dbParameters2[2].Value);

        var result2 = repository.From<Menu>()
            .InnerJoin(menuPageList.Where(t => t.ParentId < parentId), (a, b) => a.Id == b.MenuId && a.PageId > pageId)
            .Select((a, b) => new { b.MenuId, a.Name, b.ParentId, a.PageId, b.Url })
            .ToList();
        Assert.IsNotEmpty(result2);
        result2.ForEach(f =>
        {
            Assert.Greater(f.MenuId, menuId);
            Assert.Greater(f.PageId, pageId);
        });

        var sql3 = repository.From<Menu>()
            .InnerJoin<Page>((a, b) => a.PageId == b.Id && b.Id > pageId)
            .Select((a, b) => new { MenuId = a.Id, a.ParentId, b.Url })
            .Union(f => menuPageList.Where(t => t.ParentId < parentId))
            .ToSql(out var dbParameters3);
        Assert.AreEqual(@"SELECT a.`Id` AS `MenuId`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN `sys_page` b ON a.`PageId`=b.`Id` AND b.`Id`>@p0 UNION
SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE a.`Id`=b.`PageId` AND b.`Id`>@MenuId AND b.`ParentId`<@p1 AND b.`ParentId`<@p2 AND b.`ParentId`<@p4", sql3);
        Assert.AreEqual(3, dbParameters3.Count);
        Assert.AreEqual("@p0", dbParameters3[0].ParameterName);
        Assert.AreEqual("@MenuId", dbParameters3[1].ParameterName);
        Assert.AreEqual("@p2", dbParameters3[2].ParameterName);
        Assert.AreEqual(menuId, (int)dbParameters3[0].Value);
        Assert.AreEqual(pageId, (int)dbParameters3[1].Value);
        Assert.AreEqual(parentId, (int)dbParameters3[2].Value);

        var result3 = repository.From<Menu>()
            .InnerJoin<Page>((a, b) => a.PageId == b.Id && b.Id > pageId)
            .Select((a, b) => new { MenuId = a.Id, a.ParentId, b.Url })
            .Union(f => menuPageList.Where(t => t.ParentId < parentId))
            .ToList();
        Assert.IsNotEmpty(result3);
        result3.ForEach(f =>
        {
            Assert.Greater(f.MenuId, menuId);
            Assert.Less(f.ParentId, parentId);
        });
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

        var sql3 = repository
            .From<Order, User>()
            .WithQuery(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount.IsNull(0)) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, TotalAmount = x.Sum(b.Amount.IsNull(0)) }))
            .Where((a, b, c) => a.BuyerId == b.Id && a.Id == c.OrderId)
            .Select((a, b, c) => new { OrderId = a.Id, a.BuyerId, BuyerName = b.Name, Grouping = c })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` AS `OrderId`,a.`BuyerId`,b.`Name` AS `BuyerName`,c.`OrderId`,c.`TotalAmount` FROM `sys_order` a,`sys_user` b,(SELECT a.`Id` AS `OrderId`,SUM(IFNULL(b.`Amount`,0)) AS `TotalAmount` FROM `sys_order` a,`sys_order_detail` b,`sys_user` c WHERE a.`Id`=b.`OrderId` AND a.`BuyerId`=c.`Id` AND c.`Age`>20 GROUP BY a.`Id`,a.`BuyerId` HAVING SUM(IFNULL(b.`Amount`,0))>500) c WHERE a.`BuyerId`=b.`Id` AND a.`Id`=c.`OrderId`", sql3);

        var result3 = await repository
            .From<Order, User>()
            .WithQuery(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount.IsNull(0)) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, TotalAmount = x.Sum(b.Amount.IsNull(0)) }))
            .Where((a, b, c) => a.BuyerId == b.Id && a.Id == c.OrderId)
            .Select((a, b, c) => new { OrderId = a.Id, a.BuyerId, BuyerName = b.Name, Grouping = c })
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
        using var menuPageList = repository.From<Page, Menu>()
            .Where((a, b) => a.Id == b.PageId && b.Id > menuId.ToParameter("@MenuId"))
            .Select((x, y) => new { MenuId = y.Id, y.ParentId, x.Url })
            .AsCteTable("menuPageList");
        var sql1 = repository.From<Menu>()
            .InnerJoin(menuPageList, (a, b) => a.Id == b.MenuId && a.PageId > pageId)
            .Select((a, b) => new { b.MenuId, a.Name, b.ParentId, a.PageId, b.Url })
            .ToSql(out var dbParameters1);
        Assert.AreEqual(@"WITH `menuPageList`(`MenuId`,`ParentId`,`Url`) AS 
(
SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE a.`Id`=b.`PageId` AND b.`Id`>@MenuId
)
SELECT b.`MenuId`,a.`Name`,b.`ParentId`,a.`PageId`,b.`Url` FROM `sys_menu` a INNER JOIN `menuPageList` b ON a.`Id`=b.`MenuId` AND a.`PageId`>@p0", sql1);
        Assert.AreEqual(2, dbParameters1.Count);
        Assert.AreEqual("@MenuId", dbParameters1[0].ParameterName);
        Assert.AreEqual(menuId, (int)dbParameters1[0].Value);
        Assert.AreEqual(pageId, (int)dbParameters1[1].Value);

        var result1 = repository.From<Menu>()
            .InnerJoin(menuPageList, (a, b) => a.Id == b.MenuId && a.PageId > pageId)
            .Select((a, b) => new { b.MenuId, a.Name, b.ParentId, a.PageId, b.Url })
            .ToList();
        Assert.IsNotEmpty(result1);
        result1.ForEach(f =>
        {
            Assert.Greater(f.MenuId, menuId);
            Assert.Greater(f.PageId, pageId);
        });
        int parentId = 10;
        var sql2 = repository.From<Menu>()
            .InnerJoin<Page>((a, b) => a.PageId == b.Id && b.Id > pageId)
            .Select((a, b) => new { MenuId = a.Id, a.ParentId, b.Url })
            .Union(f => menuPageList.Where(t => t.ParentId < parentId))
            .ToSql(out var dbParameters2);
        Assert.AreEqual(@"WITH `menuPageList`(`MenuId`,`ParentId`,`Url`) AS 
(
SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE a.`Id`=b.`PageId` AND b.`Id`>@MenuId
)
SELECT a.`Id` AS `MenuId`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN `sys_page` b ON a.`PageId`=b.`Id` AND b.`Id`>@p0 UNION
SELECT a.`MenuId`,a.`ParentId`,a.`Url` FROM `menuPageList` a WHERE a.`ParentId`<@p2", sql2);
        Assert.AreEqual(3, dbParameters2.Count);
        Assert.AreEqual("@p0", dbParameters2[0].ParameterName);
        Assert.AreEqual("@MenuId", dbParameters2[1].ParameterName);
        Assert.AreEqual("@p2", dbParameters2[2].ParameterName);
        Assert.AreEqual(menuId, (int)dbParameters2[0].Value);
        Assert.AreEqual(pageId, (int)dbParameters2[1].Value);
        Assert.AreEqual(parentId, (int)dbParameters2[2].Value);

        var result2 = repository.From<Menu>()
            .InnerJoin<Page>((a, b) => a.PageId == b.Id && b.Id > pageId)
            .Select((a, b) => new { MenuId = a.Id, a.ParentId, b.Url })
            .Union(f => menuPageList
                .Where(f => f.ParentId < parentId)
                .Select())
            .ToList();
        Assert.IsNotEmpty(result2);
        result2.ForEach(f =>
        {
            Assert.Greater(f.MenuId, menuId);
            Assert.Less(f.ParentId, parentId);
        });
        //var sql3 = repository.From<Menu>()
        //    .InnerJoin<Page>((a, b) => a.PageId == b.Id && b.Id > pageId)
        //    .Select((a, b) => new { MenuId = a.Id, a.ParentId, b.Url })
        //    .Union(f => f.UseQuery(menuPageList)
        //        .Where(t => t.ParentId < parentId)
        //        .Select())
        //    .ToSql(out var dbParameters3);
        //Assert.AreEqual(@"WITH `menuPageList`(`MenuId`,`ParentId`,`Url`) AS 
        //(
        //SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE a.`Id`=b.`PageId` AND b.`Id`>@MenuId
        //)
        //SELECT a.`Id` AS `MenuId`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN `sys_page` b ON a.`PageId`=b.`Id` AND b.`Id`>@p0 UNION
        //SELECT a.`MenuId`,a.`ParentId`,a.`Url` FROM `menuPageList` a WHERE a.`ParentId`<@p2", sql3);
        //Assert.AreEqual(3, dbParameters3.Count);
        //Assert.AreEqual("@p0", dbParameters3[0].ParameterName);
        //Assert.AreEqual("@MenuId", dbParameters3[1].ParameterName);
        //Assert.AreEqual("@p2", dbParameters3[2].ParameterName);
        //Assert.IsTrue((int)dbParameters3[0].Value == menuId);
        //Assert.IsTrue((int)dbParameters3[1].Value == pageId);
        //Assert.IsTrue((int)dbParameters3[2].Value == parentId);

        //result1 = repository.From<Menu>()
        //    .InnerJoin<Page>((a, b) => a.PageId == b.Id && b.Id > pageId)
        //    .Select((a, b) => new { MenuId = a.Id, a.ParentId, b.Url })
        //    .Union(f => f.UseQuery(menuPageList)
        //        .Where(f => f.ParentId < parentId)
        //        .Select())
        //    .ToList();
        //Assert.IsTrue(result1.Count > 0);
        //foreach (var item in result1)
        //{
        //    Assert.IsTrue(item.MenuId > menuId);
        //    Assert.IsTrue(item.ParentId < parentId);
        //}
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
        Assert.AreEqual(1, result[0].Order.Details.Count);
        Assert.AreEqual(productId, result[0].Order.Details[0].ProductId);
        Assert.AreEqual(1, result[1].Order.Details.Count);
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
            .Where((a, b) => a.Buyer.Name.Contains("kevin"))
            .Select((x, y) => new { Order = x, Seller = y, x.Buyer })
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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,IFNULL(SUM(b.`TotalAmount`),0) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,CONVERT(b.`CreatedAt`,DATE)", sql);
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
        Assert.AreEqual("SELECT a.`Id` AS `UserId1`,a.`Name` AS `UserName`,CONVERT(b.`CreatedAt`,DATE) AS `CreatedDate1`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY `UserId1`", sql);
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
    public async Task FromQuery_OrderBy()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .OrderBy(f => new { f.Gender, f.Age })
            .OrderByDescending(f => new { f.TenantId, f.CreatedAt })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a ORDER BY a.`Gender`,a.`Age`,a.`TenantId` DESC,a.`CreatedAt` DESC", sql);

        var result = await repository.From<User>()
            .OrderBy(f => new { f.Gender, f.Age })
            .OrderByDescending(f => new { f.TenantId, f.CreatedAt })
            .ToListAsync();
        Assert.IsNotEmpty(result);
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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,`Date`", sql);
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
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                CreatedAt = x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .OrderByDescending(f => new { f.TotalAmount, f.OrderCount })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `CreatedAt`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY `TotalAmount` DESC,`OrderCount` DESC", sql1);
        var result1 = repository.From<User>()
            .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
            .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
            .Select((x, a, b) => new
            {
                x.Grouping.Id,
                x.Grouping.Name,
                CreatedAt = x.Grouping.Date,
                OrderCount = x.Count(b.Id),
                TotalAmount = x.Sum(b.TotalAmount)
            })
            .OrderByDescending(f => new { f.TotalAmount, f.OrderCount })
            .ToList();
        Assert.IsNotEmpty(result1);
    }
    [Test]
    public async Task FromQuery_Groupby_OrderBy_Fields()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<User>()
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
        Assert.AreEqual("SELECT a.`Id` AS `UserId`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `CreatedDate`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY `UserId`,a.`Name` DESC,`CreatedDate`", sql1);

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
        var sql2 = repository.From<User>()
          .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
          .GroupBy((a, b) => new { UserId = a.Id, a.Name, CreatedDate = b.CreatedAt.Date })
          .OrderBy((x, a, b) => x.Grouping.UserId)
          .OrderByDescending((x, a, b) => x.Grouping.Name)
          .OrderBy((x, a, b) => x.Grouping.CreatedDate)
          .Select((x, a, b) => new
          {
              x.Grouping,
              OrderCount = x.Count(b.Id),
              TotalAmount = x.Sum(b.TotalAmount.IsNull(0))
          })
          .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` AS `UserId`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `CreatedDate`,COUNT(b.`Id`) AS `OrderCount`,SUM(IFNULL(b.`TotalAmount`,0)) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY `UserId`,a.`Name` DESC,`CreatedDate`", sql2);

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
               TotalAmount = x.Sum(b.TotalAmount.IsNull(0))
           })
           .ToListAsync();
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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE EXISTS(SELECT * FROM `sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND c.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`,a.`Name`,`Date`", sql);
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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE EXISTS(SELECT * FROM `sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND c.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`,a.`Name` DESC,`Date`", sql);
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
    public async Task Where_Exists()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => repository.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_company` b WHERE b.`Name` LIKE '%谷歌%' AND a.`CompanyId`=b.`Id`)", sql);
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
        Assert.AreEqual("SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_company` b WHERE b.`Name` LIKE '%谷歌%' AND a.`CompanyId`=b.`Id`)", sql);
        result = repository.From<User>()
              .Where(f => Sql.Exists<Company>(t => t.Name.Contains("谷歌") && f.CompanyId == t.Id))
              .ToList();
        Assert.IsNotEmpty(result);

        var sql1 = repository.From<User>()
            .Where(f => Sql.From<Order, OrderDetail>()
                .Where((x, y) => x.Id == y.OrderId && f.Id == x.BuyerId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => x.CountDistinct(b.ProductId) > 1)
                .Exists())
            .GroupBy(f => new { f.Gender, f.CompanyId })
            .Select((t, a) => new { t.Grouping, UserTotal = t.CountDistinct(a.Id) })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Gender`,a.`CompanyId`,COUNT(DISTINCT a.`Id`) AS `UserTotal` FROM `sys_user` a WHERE EXISTS(SELECT b.`Id` FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND a.`Id`=b.`BuyerId` GROUP BY b.`Id` HAVING COUNT(DISTINCT c.`ProductId`)>1) GROUP BY a.`Gender`,a.`CompanyId`", sql1);
        var result1 = repository.From<User>()
            .Where(f => Sql.From<Order, OrderDetail>()
                .Where((x, y) => x.Id == y.OrderId && f.Id == x.BuyerId)
                .GroupBy((a, b) => a.Id)
                .Having((x, a, b) => x.CountDistinct(b.ProductId) > 1)
                .Exists())
            .GroupBy(f => new { f.Gender, f.CompanyId })
            .Select((t, a) => new { t.Grouping, UserTotal = t.CountDistinct(a.Id) })
            .ToList();
        Assert.IsNotEmpty(result1);

        var userKeys = await repository.From<User>()
            .Select(f => f.Id).Take(5).ToListAsync();
        var result3 = await repository.ExistsByIdsAsync<User>(userKeys);
        Assert.IsTrue(result3);
    }
    [Test]
    public async Task FromQuery_Exists()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.Exists<Order>(f => f.BuyerId == x.Id))
            .GroupBy((x, y) => new { x.Gender, x.CompanyId })
            .Select((x, a, b) => new { x.Grouping, UserTotal = x.CountDistinct(a.Id) })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Gender`,a.`CompanyId`,COUNT(DISTINCT a.`Id`) AS `UserTotal` FROM `sys_user` a INNER JOIN `sys_company` b ON a.`CompanyId`=b.`Id` WHERE EXISTS(SELECT * FROM `sys_order` c WHERE c.`BuyerId`=a.`Id`) GROUP BY a.`Gender`,a.`CompanyId`", sql1);
        var result1 = await repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.Exists<Order>(f => f.BuyerId == x.Id))
            .GroupBy((x, y) => new { x.Gender, x.CompanyId })
            .Select((x, a, b) => new { x.Grouping, UserTotal = x.CountDistinct(a.Id) })
            .ToListAsync();
        Assert.IsNotEmpty(result1);

        var result2 = repository.From<Order>()
            .Where(f => f.TotalAmount > 0)
            .ExistsAsync();
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
            .Where((x, y) => myOrders.Where(f => f.BuyerId == x.Id).Exists())
            .Select((a, b) => new { a.Id, a.Name, CompanyName = b.Name })
            .ToSql(out _);
        Assert.AreEqual(@"WITH `myOrders`(`OrderId`,`BuyerId`) AS 
(
SELECT a.`OrderId`,b.`BuyerId` FROM `sys_order_detail` a,`sys_order` b WHERE a.`OrderId`=b.`Id` GROUP BY a.`OrderId`,b.`BuyerId` HAVING COUNT(DISTINCT a.`ProductId`)>1
)
SELECT a.`Id`,a.`Name`,b.`Name` AS `CompanyName` FROM `sys_user` a INNER JOIN `sys_company` b ON a.`CompanyId`=b.`Id` WHERE EXISTS(SELECT * FROM `myOrders` c WHERE c.`BuyerId`=a.`Id`)", sql);

        var result = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => myOrders.Where(f => f.BuyerId == x.Id).Exists())
            .Select((a, b) => new { a.Id, a.Name, CompanyName = b.Name })
            .First();
        Assert.IsNotNull(result);

        sql = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.FromQuery(myOrders).Where(f => f.BuyerId == x.Id).Exists())
            .Select((a, b) => new { a.Id, a.Name, CompanyName = b.Name })
            .ToSql(out _);
        Assert.AreEqual(@"WITH `myOrders`(`OrderId`,`BuyerId`) AS 
(
SELECT a.`OrderId`,b.`BuyerId` FROM `sys_order_detail` a,`sys_order` b WHERE a.`OrderId`=b.`Id` GROUP BY a.`OrderId`,b.`BuyerId` HAVING COUNT(DISTINCT a.`ProductId`)>1
)
SELECT a.`Id`,a.`Name`,b.`Name` AS `CompanyName` FROM `sys_user` a INNER JOIN `sys_company` b ON a.`CompanyId`=b.`Id` WHERE EXISTS(SELECT * FROM `myOrders` c WHERE c.`BuyerId`=a.`Id`)", sql);

        result = repository.From<User>()
            .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
            .Where((x, y) => Sql.FromQuery(myOrders).Where(f => f.BuyerId == x.Id).Exists())
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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE a.`Id` IN (1,2,3) AND EXISTS(SELECT * FROM `sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND c.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`", sql);
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
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE a.`Id` IN (1,2,3) AND EXISTS(SELECT * FROM `sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND c.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`,`Date`", sql);
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
            .Where(f => f.Id.In(Sql.From<Order, OrderDetail>()
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND c.`ProductId`=1)", sql);
        result = repository.From<User>()
           .Where(f => f.Id.In(Sql.From<Order, OrderDetail>()
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
            .Where(f => Sql.In(f.Id, subQuery))
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
            .Where(f => f.Id.In(Sql.From<Order>()
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` AND c.`ProductId`=1) AND EXISTS(SELECT * FROM `sys_order` b,`sys_company` c WHERE a.`Id`=b.`SellerId` AND a.`CompanyId`=c.`Id`)", sql);
        var result = repository.From<User>()
            .Where(f => f.Id.In(Sql.From<Order>()
                .InnerJoin<OrderDetail>((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToList();
        Assert.IsNotEmpty(result);

        sql = repository.From<User>()
            .Where(f => f.Id.In(Sql.From<Order, OrderDetail>()
                .Where((a, b) => a.Id == b.OrderId && b.ProductId == 1)
                .Select((x, y) => x.BuyerId)))
            .And(isMale.HasValue, f => Sql.Exists<Order, Company>((x, y) => f.Id == x.SellerId && f.CompanyId == y.Id))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE a.`Id` IN (SELECT b.`BuyerId` FROM `sys_order` b,`sys_order_detail` c WHERE b.`Id`=c.`OrderId` AND c.`ProductId`=1) AND EXISTS(SELECT * FROM `sys_order` b,`sys_company` c WHERE a.`Id`=b.`SellerId` AND a.`CompanyId`=c.`Id`)", sql);
        result = repository.From<User>()
            .Where(f => f.Id.In(Sql.From<Order, OrderDetail>()
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
        Assert.AreEqual("SELECT a.`Gender`,a.`Age`,COUNT(DISTINCT a.`CompanyId`) AS `CompanyCount`,COUNT(a.`Id`) AS `UserCount` FROM `sys_user` a WHERE a.`Id` IN (SELECT c.`BuyerId` FROM `sys_order_detail` b INNER JOIN `sys_order` c ON b.`OrderId`=c.`Id` AND b.`ProductId`=1) AND EXISTS(SELECT * FROM `sys_company` b,`sys_order` c WHERE a.`Id`=c.`SellerId` AND a.`CompanyId`=b.`Id`) GROUP BY a.`Gender`,a.`Age`", sql);

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
        var sql = repository.From<User>()
            .Where(t => Sql.From<Order, OrderDetail>()
                    .Where((a, b) => a.BuyerId == t.Id && a.Id == b.OrderId)
                    .GroupBy((a, b) => a.Id)
                    .Having((x, a, b) => x.Count(b.Id) > 0)
                    .Exists())
            .GroupBy(f => new { f.Gender, f.CompanyId })
            .Select((x, y) => new { x.Grouping, UserTotal = x.CountDistinct(y.Id) })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Gender`,a.`CompanyId`,COUNT(DISTINCT a.`Id`) AS `UserTotal` FROM `sys_user` a WHERE EXISTS(SELECT b.`Id` FROM `sys_order` b,`sys_order_detail` c WHERE b.`BuyerId`=a.`Id` AND b.`Id`=c.`OrderId` GROUP BY b.`Id` HAVING COUNT(c.`Id`)>0) GROUP BY a.`Gender`,a.`CompanyId`", sql);

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
        var value3 = repository.QueryFirst<double>("SELECT MAX(TotalAmount) FROM sys_order");
        var value4 = repository.From<Order>().Select(f => Sql.Raw<double>("MAX(TotalAmount)")).First();
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
        var value3 = repository.QueryFirst<double>("SELECT MIN(TotalAmount) FROM sys_order");
        var value4 = repository.From<Order>().Select(f => Sql.Raw<double>("MIN(TotalAmount)")).First();
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
        var value3 = repository.QueryFirst<double>("SELECT AVG(TotalAmount) FROM sys_order");
        var value4 = repository.From<Order>().Select(f => Sql.Raw<double>("AVG(TotalAmount)")).First();
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
        var result = repository.Query<(string, string, double)>(sql);
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
        Assert.AreEqual("SELECT a.`OrderNo` IS NULL AS `NoOrderNo`,a.`ProductCount` IS NOT NULL AS `HasProduct` FROM `sys_order` a WHERE a.`ProductCount` IS NULL AND a.`ProductCount` IS NULL", sql);
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
            .From<Order>()
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
        Assert.AreEqual(@"SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId`,b.`Name` AS `BuyerName` FROM (SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`=@p0 ORDER BY a.`Id` LIMIT 1) a UNION ALL
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
        result.ForEach(f => Assert.IsTrue(f.Id == id1 || f.Id != id2));
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
    public async Task FromQueryAndJoin()
    {
        int productId = 1;
        var repository = this.dbFactory.Create();
        using var myProductOrders = repository
            .From<OrderDetail>()
            .Where(x => x.ProductId == productId)
            .GroupBy(f => f.OrderId)
            .Select((x, f) => new { f.OrderId, TotalAmout = x.Sum(f.Amount), Count = x.Count(f.Id) });
        var sql = repository
            .FromQuery(x => x.From<Order>()
                .InnerJoin(myProductOrders, (a, b) => a.Id == b.OrderId)
                .Select((x, y) => new
                {
                    x.Id,
                    x.BuyerId,
                    x.TotalAmount,
                    MyProductTotalAmont = y.TotalAmout,
                    MyProductCount = y.Count
                }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Select((a, b) => new
            {
                a.Id,
                a.BuyerId,
                BuyerName = b.Name,
                a.TotalAmount,
                a.MyProductTotalAmont,
                a.MyProductCount
            })
            .ToSql(out _);
        Assert.AreEqual(@"SELECT a.`Id`,a.`BuyerId`,b.`Name` AS `BuyerName`,a.`TotalAmount`,a.`MyProductTotalAmont`,a.`MyProductCount` FROM (SELECT a.`Id`,a.`BuyerId`,a.`TotalAmount`,b.`TotalAmout` AS `MyProductTotalAmont`,b.`Count` AS `MyProductCount` FROM `sys_order` a INNER JOIN (SELECT a.`OrderId`,SUM(a.`Amount`) AS `TotalAmout`,COUNT(a.`Id`) AS `Count` FROM `sys_order_detail` a WHERE a.`ProductId`=@p0 GROUP BY a.`OrderId`) b ON a.`Id`=b.`OrderId`) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id`", sql);
        var result = await repository
            .FromQuery(x => x.From<Order>()
                .InnerJoin(myProductOrders, (a, b) => a.Id == b.OrderId)
                .Select((x, y) => new
                {
                    x.Id,
                    x.BuyerId,
                    x.TotalAmount,
                    MyProductTotalAmont = y.TotalAmout,
                    MyProductCount = y.Count
                }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Select((a, b) => new
            {
                a.Id,
                a.BuyerId,
                BuyerName = b.Name,
                a.TotalAmount,
                a.MyProductTotalAmont,
                a.MyProductCount
            })
            .ToListAsync();
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

        int rootId = 1;
        var sql1 = repository
            .FromQuery(x => x.From<Menu>()
                .Where(x => x.Id == rootId)
                .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId })
                .UnionAllRecursive((x, self) => x.From<Menu>()
                    .InnerJoin(self, (a, b) => a.ParentId == b.Id)
                    .Select((a, b) => new { a.Id, a.Name, a.ParentId, a.PageId }))
                .AsCteTable("MenuList"))
            .InnerJoin<Page>((a, b) => a.PageId == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, a.PageId, b.Url })
            .ToSql(out _);
        Assert.AreEqual(@"WITH RECURSIVE `MenuList`(`Id`,`Name`,`ParentId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a WHERE a.`Id`=@RootId UNION ALL
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a INNER JOIN `MenuList` b ON a.`ParentId`=b.`Id`
)
SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `MenuList` a INNER JOIN (SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `sys_menu` b ON a.`Id`=b.`PageId` WHERE a.`Id`=@p1 UNION ALL
SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `MenuList` b ON a.`Id`=b.`Id` WHERE a.`Id`>@p2) b ON a.`Id`=b.`Id`", sql);
        var result1 = repository
            .FromQuery(x => x.From<Menu>()
                .Where(x => x.Id == rootId)
                .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId })
                .UnionAllRecursive((x, self) => x.From<Menu>()
                    .InnerJoin(self, (a, b) => a.ParentId == b.Id)
                    .Select((a, b) => new { a.Id, a.Name, a.ParentId, a.PageId }))
                .AsCteTable("MenuList"))
            .InnerJoin<Page>((a, b) => a.PageId == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId, a.PageId, b.Url })
            .ToList();
        Assert.IsNotEmpty(result1);
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
            .FromQuery(f => myCteTable2)
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
                .UnionAll(x => x.From<Page>()//.WithQuery(self)
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
            .FromQuery(menuList)
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
        Assert.AreEqual("SELECT a.`OrderId`,a.`BuyerId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,c.`Id`,c.`TenantId`,c.`OrderNo`,c.`ProductCount`,c.`TotalAmount`,c.`BuyerId`,c.`BuyerSource`,c.`SellerId`,c.`Products`,c.`Disputes`,c.`IsEnabled`,c.`CreatedAt`,c.`CreatedBy`,c.`UpdatedAt`,c.`UpdatedBy`,a.`TotalAmount` FROM (SELECT a.`Id` AS `OrderId`,a.`BuyerId`,IFNULL(SUM(b.`Amount`),0) AS `TotalAmount` FROM `sys_order` a,`sys_order_detail` b,`sys_user` c WHERE a.`Id`=b.`OrderId` AND a.`BuyerId`=c.`Id` AND c.`Age`>20 GROUP BY a.`Id`,a.`BuyerId` HAVING SUM(b.`Amount`)>500) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` INNER JOIN `sys_order` c ON a.`OrderId`=c.`Id`", sql3);

        var result3 = repository.FromQuery(f => f.From<Order, OrderDetail, User>()
                .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
                .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
                .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
                .Select((x, a, b, c) => new { x.Grouping.OrderId, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) }))
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .InnerJoin<Order>((a, b, c) => a.OrderId == c.Id)
            .Select((a, b, c) => new { a.OrderId, a.BuyerId, Buyer = b, Order = c, a.TotalAmount })
            .ToList();
        Assert.IsNotEmpty(result3);
    }
    [Test]
    public void SelectTo()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.DeleteByIdAsync<Order>("8");
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
            .SelectTo<OrderInfo>()
            .First();
        Assert.AreEqual("8", result.Id);
        Assert.AreEqual(1, result.BuyerId);
        Assert.AreEqual("On-ZwYx", result.OrderNo);
        Assert.IsNull(result.Description);

        result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo(f => new OrderInfo
            {
                Description = "TotalAmount:" + f.TotalAmount
            })
            .First();
        Assert.AreEqual("8", result.Id);
        Assert.AreEqual(1, result.BuyerId);
        Assert.AreEqual("On-ZwYx", result.OrderNo);
        Assert.IsNotNull(result.Description);
        Assert.AreEqual("TotalAmount:500", result.Description);

        result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo(f => new OrderInfo
            {
                Description = this.DeferInvoke().Deferred()
            })
            .First();
        Assert.AreEqual("8", result.Id);
        Assert.AreEqual(1, result.BuyerId);
        Assert.AreEqual("On-ZwYx", result.OrderNo);
        Assert.IsNotNull(result.Description);
        Assert.AreEqual(this.DeferInvoke(), result.Description);

        result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo(f => new OrderInfo
            {
                Description = $"TotalAmount: {f.TotalAmount.ToString("C")}"
            })
            .First();
        Assert.AreEqual("8", result.Id);
        Assert.AreEqual(1, result.BuyerId);
        Assert.AreEqual("On-ZwYx", result.OrderNo);
        Assert.IsNotNull(result.Description);
        Assert.AreEqual($"TotalAmount: {result.TotalAmount.ToString("C")}", result.Description);

        var sql1 = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo(f => new OrderInfo
            {
                Description = f.TotalAmount.ToString("C") + f.OrderNo
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`TotalAmount`,a.`OrderNo`,a.`Id`,a.`OrderNo`,a.`BuyerId`,a.`TotalAmount` FROM `sys_order` a WHERE a.`Id` IN ('8')", sql1);

        result = repository.From<Order>()
           .Where(f => Sql.In(f.Id, new[] { "8" }))
           .SelectTo(f => new OrderInfo
           {
               Description = $"{f.OrderNo}: {f.TotalAmount.ToString("C")}"
           })
           .First();
        Assert.AreEqual("8", result.Id);
        Assert.AreEqual(1, result.BuyerId);
        Assert.AreEqual("On-ZwYx", result.OrderNo);
        Assert.IsNotNull(result.Description);
        Assert.AreEqual($"{result.OrderNo}: {result.TotalAmount.ToString("C")}", result.Description);

        var sql = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo(f => new OrderInfo
            {
                Description = f.TotalAmount.ToString("C") + f.OrderNo
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`TotalAmount`,a.`OrderNo`,a.`Id`,a.`OrderNo`,a.`BuyerId`,a.`TotalAmount` FROM `sys_order` a WHERE a.`Id` IN ('8')", sql);

        result = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo(f => new OrderInfo
            {
                Description = f.TotalAmount.ToString("C") + f.OrderNo
            })
            .First();
        Assert.AreEqual("8", result.Id);
        Assert.AreEqual(1, result.BuyerId);
        Assert.AreEqual("On-ZwYx", result.OrderNo);
        Assert.IsNotNull(result.Description);
        Assert.AreEqual(result.TotalAmount.ToString("C") + result.OrderNo, result.Description);

        var result1 = repository.FromQuery(f => f.From<Order, OrderDetail>('a')
            .Where((a, b) => a.Id == b.OrderId)
            .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
            .Having((x, a, b) => x.CountDistinct(b.ProductId) > 0)
            .Select((x, a, b) => new { x.Grouping.BuyerId, x.Grouping.OrderId, ProductTotal = x.CountDistinct(b.ProductId) }))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SelectTo((x, y) => new OrderBuyerInfo { BuyerName = y.Name })
            .First();
        Assert.IsNotNull(result1);
        Assert.IsFalse(string.IsNullOrEmpty(result1.OrderId));
        Assert.Greater(result1.BuyerId, 0);
        Assert.IsNull(result1.OrderNo);
        Assert.IsNotNull(result1.BuyerName);
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
    [Test]
    public async Task DeferredField()
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
        var sql1 = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo(f => new OrderInfo
            {
                Description = f.TotalAmount.ToString("C") + f.OrderNo
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`TotalAmount`,a.`OrderNo`,a.`Id`,a.`OrderNo`,a.`BuyerId`,a.`TotalAmount` FROM `sys_order` a WHERE a.`Id` IN ('8')", sql1);

        var result1 = await repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo(f => new OrderInfo
            {
                Description = f.TotalAmount.ToString("C") + f.OrderNo
            })
            .FirstAsync();
        Assert.AreEqual("8", result1.Id);
        Assert.AreEqual(1, result1.BuyerId);
        Assert.AreEqual("On-ZwYx", result1.OrderNo);
        Assert.IsNotNull(result1.Description);
        Assert.AreEqual(result1.TotalAmount.ToString("C") + result1.OrderNo, result1.Description);

        var sql2 = repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo(f => new OrderInfo
            {
                Description = $"{f.OrderNo}: {f.TotalAmount.ToString("C")}"
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`OrderNo`,a.`TotalAmount`,a.`Id`,a.`OrderNo`,a.`BuyerId`,a.`TotalAmount` FROM `sys_order` a WHERE a.`Id` IN ('8')", sql2);

        var result2 = await repository.From<Order>()
            .Where(f => Sql.In(f.Id, new[] { "8" }))
            .SelectTo(f => new OrderInfo
            {
                Description = $"{f.OrderNo}: {f.TotalAmount.ToString("C")}"
            })
            .FirstAsync();
        Assert.AreEqual("8", result2.Id);
        Assert.AreEqual(1, result2.BuyerId);
        Assert.AreEqual("On-ZwYx", result2.OrderNo);
        Assert.IsNotNull(result2.Description);
        Assert.AreEqual($"{result2.OrderNo}: {result2.TotalAmount.ToString("C")}", result2.Description);
    }
    [Test]
    public async Task RawParameters()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var parameters = new List<IDbDataParameter>
        {
            new MySqlParameter("pId", MySqlDbType.Int32) { Value = 1 },
            new MySqlParameter("pOut", MySqlDbType.VarChar) { Size = 50, Direction = ParameterDirection.Output }
        };
        var result = await repository.QueryFirstAsync<User>("GET_USER", parameters, CommandType.StoredProcedure);
        if (result != null)
        {
            Assert.IsNotNull(result.Name);
            Assert.AreEqual("OK", parameters[1].Value.ToString());
        }
        await repository.BeginTransactionAsync();
        await repository.ExecuteAsync("UPDATE_USER", parameters, CommandType.StoredProcedure);
        result = await repository.QueryByIdAsync<User>(1);
        await repository.CommitAsync();
        Assert.AreEqual("UpdatedName", result.Name);
        Assert.AreEqual(18, result.Age);
        Assert.AreEqual("OK", parameters[1].Value.ToString());
    }
    [Test]
    public async Task FromQuery_PartitionBy()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Select((a, b) => new
            {
                Rank = Sql.Over().PartitionBy(a.SellerId).OrderByDescending(a.CreatedAt)
                    .OrderBy(new { a.BuyerId, a.OrderNo }).Rank(),
                a.Id,
                a.OrderNo,
                a.SellerId,
                a.BuyerId,
                BuyerName = b.Name,
                a.TotalAmount
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT RANK() OVER(PARTITION BY a.`SellerId` ORDER BY a.`CreatedAt` DESC,a.`BuyerId`,a.`OrderNo`) AS `Rank`,a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id`", sql);
        var result = await repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Select((a, b) => new
            {
                Rank = Sql.Over().PartitionBy(a.SellerId).OrderBy(new { a.BuyerId, a.OrderNo })
                    .OrderByDescending(a.CreatedAt).Rank(),
                a.Id,
                a.OrderNo,
                a.SellerId,
                a.BuyerId,
                BuyerName = b.Name,
                a.TotalAmount
            })
            .ToListAsync();
        Assert.IsNotEmpty(result);
        sql = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Select((a, b) => new
            {
                Count = Sql.Over().PartitionBy(a.SellerId)
                    .OrderBy(new { a.BuyerId, a.OrderNo }).OrderByDescending(a.CreatedAt).Count(a.TenantId),
                a.Id,
                a.OrderNo,
                a.SellerId,
                a.BuyerId,
                BuyerName = b.Name,
                a.TotalAmount
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT COUNT(a.`TenantId`) OVER(PARTITION BY a.`SellerId` ORDER BY a.`BuyerId`,a.`OrderNo`,a.`CreatedAt` DESC) AS `Count`,a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id`", sql);
        var result1 = await repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Select((a, b) => new
            {
                Count = Sql.Over().PartitionBy(a.SellerId).OrderBy(new { a.BuyerId, a.OrderNo })
                    .OrderByDescending(a.CreatedAt).Count(a.TenantId),
                a.Id,
                a.OrderNo,
                a.SellerId,
                a.BuyerId,
                BuyerName = b.Name,
                a.TotalAmount
            })
            .ToListAsync();
        Assert.IsNotEmpty(result1);
    }
    [Test]
    public async Task FromQuery_GroupConcat()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Select((a, b) => new
            {
                Count = Sql.GroupConcat(new { a.TenantId, Buyer = a.BuyerId + "-" + b.Name })
                    .OrderBy(new { a.BuyerId, a.OrderNo }).OrderByDescending(a.CreatedAt).Distinct().ToValue(),
                a.Id,
                a.OrderNo,
                a.SellerId,
                a.BuyerId,
                BuyerName = b.Name,
                a.TotalAmount
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT GROUP_CONCAT(DISTINCT a.`TenantId`,CONCAT(CAST(a.`BuyerId` AS CHAR),'-',b.`Name`) ORDER BY a.`BuyerId`,a.`OrderNo`,a.`CreatedAt` DESC) AS `Count`,a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id`", sql);
        await repository.From<Order>()
           .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
           .Select((a, b) => new
           {
               Count = Sql.GroupConcat(new { a.TenantId, Buyer = a.BuyerId + "-" + b.Name })
                    .OrderBy(new { a.BuyerId, a.OrderNo }).OrderByDescending(a.CreatedAt).Distinct().ToValue(),
               a.Id,
               a.OrderNo,
               a.SellerId,
               a.BuyerId,
               BuyerName = b.Name,
               a.TotalAmount
           })
           .ToListAsync();
    }
    [Test]
    public async Task SelectAndOrExpr()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<Product, Brand>()
            .InnerJoin((x, y) => x.BrandId == y.Id)
            .SelectTo((x, y) => new ProductInfo { IsEnabled = x.IsEnabled && y.IsEnabled || x.CompanyId.IsNull() })
            .ToSql(out _);
        Assert.AreEqual("SELECT (CASE WHEN a.`IsEnabled`=1 AND b.`IsEnabled`=1 OR a.`CompanyId` IS NULL THEN 1 ELSE 0 END) AS `IsEnabled` FROM `sys_product` a INNER JOIN `sys_brand` b ON a.`BrandId`=b.`Id`", sql);
        var result = await repository.From<Product, Brand>()
            .InnerJoin((x, y) => x.BrandId == y.Id && x.IsEnabled && y.IsEnabled || x.CompanyId.IsNull())
            .SelectTo((x, y) => new ProductInfo { IsEnabled = x.IsEnabled && y.IsEnabled || x.CompanyId.IsNull() })
            .FirstAsync();
        Assert.IsTrue(result.IsEnabled);
    }
    [Test]
    public async Task SelectRawSql()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Product, Brand>()
            .InnerJoin((x, y) => x.BrandId == y.Id)
            .Select<MyProductInfo>("a.Id as ProductId,a.ProductNo,a.Name as ProductName,a.BrandId,b.Name as BrandName")
            .ToSql(out _);
        Assert.AreEqual("SELECT a.Id as ProductId,a.ProductNo,a.Name as ProductName,a.BrandId,b.Name as BrandName FROM `sys_product` a INNER JOIN `sys_brand` b ON a.`BrandId`=b.`Id`", sql1);
        var result1 = await repository.From<Product, Brand>()
            .InnerJoin((x, y) => x.BrandId == y.Id)
            .Select<MyProductInfo>("a.Id as ProductId,a.ProductNo,a.Name as ProductName,a.BrandId,b.Name as BrandName")
            .FirstAsync();
        Assert.IsNotNull(result1);

        var sql2 = repository.From<Product>()
            .Select<string>("CONCAT(ProductNo,'-',Name)")
            .ToSql(out _);
        Assert.AreEqual("SELECT CONCAT(ProductNo,'-',Name) FROM `sys_product` a", sql2);
        var result2 = await repository.From<Product>()
            .Select<string>("CONCAT(ProductNo,'-',Name)")
            .FirstAsync();
        Assert.IsNotNull(result2);
        Assert.IsTrue(result2.Contains("-"));

        var sql3 = repository.From<Product>()
            .Select(f => new MyProductInfo
            {
                ProductId = f.Id,
                ProductName = Sql.Raw<string>("CONCAT(ProductNo, '-', Name)")
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` AS `ProductId`,CONCAT(ProductNo, '-', Name) AS `ProductName` FROM `sys_product` a", sql3);
        var result3 = await repository.From<Product>()
            .Select(f => new MyProductInfo
            {
                ProductId = f.Id,
                ProductName = Sql.Raw<string>("CONCAT(ProductNo, '-', Name)")
            })
            .FirstAsync();
        Assert.IsNotNull(result3);

        var sql4 = repository.From<Product, Brand>()
            .InnerJoin((x, y) => x.BrandId == y.Id)
            .Select((x, y) => new MyProductInfo
            {
                ProductId = x.Id,
                ProductName = Sql.Raw<string>("CONCAT(a.ProductNo, '-', a.Name)"),
                Brand = Sql.Raw<BrandInfo>("b.Id,b.BrandNo,b.Name", 3)
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id` AS `ProductId`,CONCAT(a.ProductNo, '-', a.Name) AS `ProductName`,b.Id,b.BrandNo,b.Name FROM `sys_product` a INNER JOIN `sys_brand` b ON a.`BrandId`=b.`Id`", sql4);
        var result4 = await repository.From<Product, Brand>()
            .InnerJoin((x, y) => x.BrandId == y.Id)
            .Select((x, y) => new MyProductInfo
            {
                ProductId = x.Id,
                ProductName = Sql.Raw<string>("CONCAT(a.ProductNo, '-', a.Name)"),
                Brand = Sql.Raw<BrandInfo>("b.Id,b.BrandNo,b.Name", 3)
            })
            .FirstAsync();
        Assert.IsNotNull(result4);

        var sql5 = repository.From<Brand>()
            .Select(f => Sql.Raw<BrandInfo>("a.Id,a.BrandNo,a.Name"))
            .ToSql(out _);
        Assert.AreEqual("SELECT a.Id,a.BrandNo,a.Name FROM `sys_brand` a", sql5);
        var result5 = await repository.From<Brand>()
            .Select(f => Sql.Raw<BrandInfo>("a.Id,a.BrandNo,a.Name"))
            .FirstAsync();
        Assert.IsNotNull(result5);

        var sql6 = repository.From<Brand>()
            .Select(f => Sql.Raw<int>("a.Id"))
            .ToSql(out _);
        Assert.AreEqual("SELECT a.Id FROM `sys_brand` a", sql6);
        var result6 = await repository.From<Brand>()
            .Select(f => Sql.Raw<int>("a.Id"))
            .FirstAsync();
        Assert.Greater(result6, 0);
    }
    private string DeferInvoke() => "DeferInvoke";
}
