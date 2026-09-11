using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Trolley.MySqlConnector;

namespace Trolley.Test.MySqlConnector;

[TestFixture]
public class UnitTest3 : UnitTestBase
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
    public void Update_AnonymousObject()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var user = repository.QueryById<User>(1);
        user.Name = "kevin";
        user.Gender = Gender.Female;
        user.SourceType = null;
        var count = repository.Update<User>(user);
        var changedUser = repository.QueryById<User>(1);
        Assert.Greater(count, 0);
        Assert.IsNotNull(changedUser);
        Assert.AreEqual(user.Name, changedUser.Name);
        Assert.AreEqual(changedUser.SourceType, changedUser.SourceType);

        count = repository.Update<User>(new
        {
            Id = 1,
            Name = (string)null,
            Gender = Gender.Male,
            SourceType = UserSourceType.Douyin
        });
        var result = repository.QueryById<User>(1);
        Assert.Greater(count, 0);
        Assert.IsNotNull(result);
        Assert.IsNull(result.Name);
        Assert.AreEqual(UserSourceType.Douyin, result.SourceType);
    }
    [Test]
    public async Task Update_AnonymousObjects()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var parameters = await repository.From<OrderDetail>()
            .GroupBy(f => f.OrderId)
            .OrderBy((x, f) => f.OrderId)
            .Select((x, f) => new
            {
                Id = x.Grouping,
                TotalAmount = x.Sum(f.Amount) + 50
            })
            .ToListAsync();
        var count = await repository.UpdateAsync<Order>(parameters);
        var ids = parameters.Select(f => f.Id).ToList();
        var orders = await repository.QueryAsync<Order>(f => ids.Contains(f.Id));
        repository.Commit();
        Assert.Greater(count, 0);
        Assert.AreEqual(orders.Count, parameters.Count);
        orders.Sort((x, y) => x.Id.CompareTo(y.Id));
        for (int i = 0; i < orders.Count; i++)
        {
            Assert.AreEqual(parameters[i].TotalAmount, (decimal)orders[i].TotalAmount);
        }
    }
    [Test]
    public async Task Update_SetBulk()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var parameters = await repository.From<OrderDetail>()
           .GroupBy(f => f.OrderId)
           .Select((x, f) => new
           {
               Id = x.Grouping,
               Amount = x.Sum(f.Amount) + 50
           })
           .ToListAsync();
        var sql = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order_detail` SET `Amount`=@Amount0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Amount`=@Amount1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Amount`=@Amount2 WHERE `Id`=@kId2", sql);
        Assert.AreEqual(parameters.Count * 2, dbParameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.AreEqual($"@Amount{i}", dbParameters[i * 2].ParameterName);
            Assert.AreEqual($"@kId{i}", dbParameters[i * 2 + 1].ParameterName);
        }
    }
    [Test]
    public async Task Update_SetBulk_OnlyFields()
    {
        this.Initialize(1);
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
        Assert.AreEqual("UPDATE `sys_order_detail` SET `Price`=@Price0,`Quantity`=@Quantity0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Price`=@Price1,`Quantity`=@Quantity1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Price`=@Price2,`Quantity`=@Quantity2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Price`=@Price3,`Quantity`=@Quantity3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Price`=@Price4,`Quantity`=@Quantity4 WHERE `Id`=@kId4", sql);
        Assert.AreEqual(parameters.Count * 3, dbParameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.AreEqual($"@Price{i}", dbParameters[i * 3].ParameterName);
            Assert.AreEqual($"@Quantity{i}", dbParameters[i * 3 + 1].ParameterName);
            Assert.AreEqual($"@kId{i}", dbParameters[i * 3 + 2].ParameterName);
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
        Assert.AreEqual(parameters.Count, result);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.AreEqual(parameters[i].Price, updatedDetails[i].Price);
            Assert.AreEqual(parameters[i].Quantity, updatedDetails[i].Quantity);
            Assert.AreNotEqual(parameters[i].Amount, updatedDetails[i].Amount);
        }
    }
    [Test]
    public async Task Update_SetBulk_IgnoreFields()
    {
        this.Initialize(1);
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
            .IgnoreFields(f => f.Price)
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order_detail` SET `Quantity`=@Quantity0,`Amount`=@Amount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Quantity`=@Quantity1,`Amount`=@Amount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Quantity`=@Quantity2,`Amount`=@Amount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Quantity`=@Quantity3,`Amount`=@Amount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Quantity`=@Quantity4,`Amount`=@Amount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4", sql);
        Assert.AreEqual(parameters.Count * 4, dbParameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.AreEqual($"@Quantity{i}", dbParameters[i * 4].ParameterName);
            Assert.AreEqual($"@Amount{i}", dbParameters[i * 4 + 1].ParameterName);
            Assert.AreEqual($"@UpdatedAt{i}", dbParameters[i * 4 + 2].ParameterName);
            Assert.AreEqual($"@kId{i}", dbParameters[i * 4 + 3].ParameterName);
        }
        var ids = parameters.Select(f => f.Id).ToList();
        repository.BeginTransaction();
        var result = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .IgnoreFields(f => f.Price)
            .Execute();
        var updatedDetails = await repository.QueryAsync<OrderDetail>(f => ids.Contains(f.Id));
        repository.Commit();
        Assert.AreEqual(parameters.Count, result);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.AreNotEqual(parameters[i].Price, updatedDetails[i].Price);
            Assert.AreEqual(parameters[i].Quantity, updatedDetails[i].Quantity);
            Assert.AreEqual(parameters[i].Amount, updatedDetails[i].Amount);
            Assert.AreEqual(parameters[i].UpdatedAt, updatedDetails[i].UpdatedAt);
            Assert.AreEqual(parameters[i].Id, updatedDetails[i].Id);
        }
    }
    [Test]
    public async Task Update_SetBulk_SetFields()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var orderDetails = await repository.From<OrderDetail>()
            .OrderBy(f => f.Id)
            .Take(5)
            .ToListAsync();
        var parameters = orderDetails.Select(f => new
        {
            f.Id,
            Amount = f.Amount + 50,
            UpdatedAt = f.UpdatedAt.AddDays(1)
        })
        .ToList();
        var sql = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .Set(f => f.ProductId, 3)
            .Set(new { Quantity = 5 })
            .Set(f => new { Price = f.Price + 10 })
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4", sql);
        Assert.AreEqual(parameters.Count * 3 + 2, dbParameters.Count);

        var ids = parameters.Select(f => f.Id).ToList();
        repository.BeginTransaction();
        var result = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .Set(f => f.ProductId, 3)
            .Set(new { Price = 200, Quantity = 5 })
            .IgnoreFields(f => f.Price)
            .Execute();
        var updatedDetails = await repository.QueryAsync<OrderDetail>(f => ids.Contains(f.Id));
        repository.Commit();
        Assert.AreEqual(parameters.Count, result);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.AreEqual(3, updatedDetails[i].ProductId);
            Assert.AreEqual(5, updatedDetails[i].Quantity);
            Assert.AreEqual(parameters[i].Amount, updatedDetails[i].Amount);
            Assert.AreEqual(parameters[i].UpdatedAt, updatedDetails[i].UpdatedAt);
        }
    }
    [Test]
    public void Update_Fields_Where()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = repository.Update<User>(f => new
        {
            Name = f.Name + "_1",
            Gender = Gender.Female,
            SourceType = DBNull.Value
        }, t => t.Id == 1);
        var result1 = repository.QueryById<User>(1);
        Assert.Greater(result, 0);
        Assert.IsNotNull(result1);
        Assert.AreEqual("leafkevin_1", result1.Name);
        Assert.IsFalse(result1.SourceType.HasValue);

        var result2 = repository.Update<User>(new
        {
            Id = 1,
            Name = "kevin",
            Gender = Gender.Female,
            SourceType = DBNull.Value
        });
        var result3 = repository.QueryById<User>(1);
        Assert.Greater(result2, 0);
        Assert.IsNotNull(result3);
        Assert.AreEqual("kevin", result3.Name);
        Assert.IsFalse(result3.SourceType.HasValue);
    }
    [Test]
    public void Update_Set_Fields_Where()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = repository.Update<User>()
            .Set(f => new
            {
                Age = 25,
                Name = "leafkevin22",
                CompanyId = DBNull.Value
            })
            .Where(f => f.Id == 1)
            .Execute();
        var result2 = repository.QueryById<User>(1);
        Assert.Greater(result, 0);
        Assert.IsNotNull(result2);
        Assert.AreEqual("leafkevin22", result2.Name);
        Assert.AreEqual(25, result2.Age);
        Assert.AreEqual(0, result2.CompanyId);
    }
    [Test]
    public void Update_Fields_Parameters()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var result = repository.Update<User>(new { id = 1, name = "leafkevin11" });
        var result1 = repository.QueryById<User>(1);
        Assert.Greater(result, 0);
        Assert.IsNotNull(result1);
        Assert.AreEqual("leafkevin11", result1.Name);
        result = repository.Update<User>(new Dictionary<string, object> { { "id", 1 }, { "name", "leafkevin22" } });
        result1 = repository.QueryById<User>(1);
        Assert.Greater(result, 0);
        Assert.IsNotNull(result1);
        Assert.AreEqual("leafkevin22", result1.Name);
    }
    [Test]
    public void Update_Set_AnonymousObject_Where()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<User>()
            .Set(new
            {
                Age = 25,
                Name = "leafkevin22",
                CompanyId = DBNull.Value
            })
            .Where(f => f.Id == 1)
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_user` SET `Name`=@Name,`Age`=@Age,`CompanyId`=@CompanyId WHERE `Id`=1", sql);
        Assert.AreEqual(3, dbParameters.Count);
        Assert.AreEqual("leafkevin22", (string)dbParameters[0].Value);
        Assert.AreEqual(25, (int)dbParameters[1].Value);
        Assert.AreEqual(DBNull.Value, dbParameters[2].Value);

        repository.Update<User>()
           .Set(f => new
           {
               Age = 25,
               Name = "leafkevin22",
               CompanyId = DBNull.Value
           })
           .Where(f => f.Id == 1)
           .Execute();
        var result = repository.QueryById<User>(1);
        Assert.AreEqual("leafkevin22", result.Name);
        Assert.AreEqual(25, result.Age);
        Assert.AreEqual(0, result.CompanyId);
    }
    [Test]
    public void Update_Set_AnonymousObject_Where_OnlyFields()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var user = repository.QueryById<User>(1);
        var sql = repository.Update<User>()
            .Set(new
            {
                Age = 30,
                Name = "leafkevinabc",
                CompanyId = 1
            })
            .OnlyFields(f => f.Name)
            .Where(f => f.Id == 1)
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=1", sql);
        Assert.AreEqual(1, dbParameters.Count);
        Assert.AreEqual("leafkevinabc", (string)dbParameters[0].Value);

        repository.Update<User>()
            .Set(new
            {
                Age = 30,
                Name = "leafkevinabc",
                CompanyId = 1
            })
            .OnlyFields(f => f.Name)
            .Where(f => f.Id == 1)
            .Execute();
        var result = repository.QueryById<User>(1);
        Assert.AreEqual("leafkevinabc", result.Name);
        Assert.AreEqual(user.Age, result.Age);
        Assert.AreEqual(user.CompanyId, result.CompanyId);
    }
    [Test]
    public void Update_Set_AnonymousObject_Where_IgnoreFields()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<User>()
            .Set(new
            {
                Age = 25,
                Name = "leafkevin22",
                CompanyId = DBNull.Value
            })
            .IgnoreFields(f => f.Name)
            .Where(f => f.Id == 1)
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_user` SET `Age`=@Age,`CompanyId`=@CompanyId WHERE `Id`=1", sql);
        Assert.AreEqual(2, dbParameters.Count);
        Assert.AreEqual(25, (int)dbParameters[0].Value);
        Assert.AreEqual(DBNull.Value, dbParameters[1].Value);

        repository.Update<User>()
            .Set(new
            {
                Age = 25,
                Name = "leafkevin22",
                CompanyId = DBNull.Value
            })
            .IgnoreFields(f => f.Name)
            .Where(f => f.Id == 1)
            .Execute();
        var result = repository.QueryById<User>(1);
        Assert.AreNotEqual("leafkevin22", result.Name);
        Assert.AreEqual(25, result.Age);
        Assert.AreEqual(0, result.CompanyId);
    }
    [Test]
    public void Update_SetWith_Parameters()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var result = repository.Update<Order>(new
        {
            ProductCount = 10,
            Id = 1
        });
        var result1 = repository.QueryById<Order>(new { Id = "1" });
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotNull(result1);
            Assert.AreEqual("1", result1.Id);
            Assert.AreEqual(10, result1.ProductCount);
        }
        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(new { ProductCount = 11 })
            .WhereBy(new { Id = "1" })
            .Execute();
        var result2 = repository.QueryById<Order>(new { Id = "1" });
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotNull(result2);
            Assert.AreEqual("1", result2.Id);
            Assert.AreEqual(11, result2.ProductCount);
        }
        var updateObj = new Dictionary<string, object>
        {
            { "ProductCount", result2.ProductCount + 1 },
            { "TotalAmount", result2.TotalAmount + 100 }
        };
        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(updateObj)
            .WhereBy(new { Id = "1" })
            .Execute();
        var result3 = repository.QueryById<Order>(new { Id = "1" });
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotNull(result3);
            Assert.AreEqual("1", result3.Id);
            Assert.AreEqual(result2.ProductCount + 1, result3.ProductCount);
            Assert.AreEqual(result2.TotalAmount + 100, result3.TotalAmount);
        }
    }
    [Test]
    public async Task Update_MultiParameters()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var parameters = await repository.From<OrderDetail>()
            .Where(f => new[] { "1", "2", "3", "4", "5", "6" }.Contains(f.Id))
            .OrderBy(f => f.Id)
            .Select(f => new { f.Id, Price = f.Price + 80, Quantity = f.Quantity + 2, Amount = f.Amount + 100 })
            .ToListAsync();
        var count = repository.Update<OrderDetail>(parameters);
        var orderDetails = await repository.From<OrderDetail>()
            .Where(f => new[] { "1", "2", "3", "4", "5", "6" }.Contains(f.Id))
            .OrderBy(f => f.Id)
            .Select()
            .ToListAsync();
        repository.Commit();
        Assert.Greater(count, 0);
        for (int i = 0; i < orderDetails.Count; i++)
        {
            Assert.AreEqual(parameters[i].Price, orderDetails[i].Price);
            Assert.AreEqual(parameters[i].Quantity, orderDetails[i].Quantity);
            Assert.AreEqual(parameters[i].Amount, orderDetails[i].Amount);
        }
    }
    [Test]
    public void Update_Set_Expression()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .Set(f => new
            {
                TotalAmount = f.TotalAmount + 200.56,
                OrderNo = f.OrderNo + "-111",
            })
            .Where(f => f.Id == "1")
          .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1", sql);
        Assert.IsNotNull(dbParameters);
        Assert.AreEqual("@p0", dbParameters[0].ParameterName);
        Assert.AreEqual(200.56, (double)dbParameters[0].Value);
        Assert.AreEqual("@Products", dbParameters[1].ParameterName);
        Assert.AreEqual(JsonSerializer.Serialize(new List<int> { 1, 2, 3 }), (string)dbParameters[1].Value);
    }
    [Test]
    public void Update_Set_MethodCall()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var parameter = repository.QueryById<Order>("1");
        parameter.TotalAmount += 50;
        var result = repository.Update<Order>()
            .Set(f => new
            {
                TotalAmount = this.CalcAmount(parameter.TotalAmount, 3),
                Products = this.GetProducts(),
                OrderNo = string.Concat("Order", "111").Substring(0, 7) + "_123"
            })
            .Where(x => x.Id == "2")
            .Execute();
        var order = repository.QueryById<Order>("2");
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotEmpty(order.Products);
            Assert.AreEqual(3, order.Products.Count);
            Assert.AreEqual(1, order.Products[0]);
            Assert.AreEqual(2, order.Products[1]);
            Assert.AreEqual(3, order.Products[2]);
            Assert.AreEqual(this.CalcAmount(parameter.TotalAmount, 3), order.TotalAmount);
        }

        var updateObj = repository.QueryById<Order>("1");
        updateObj.Disputes = new Dispute
        {
            Id = 2,
            Content = "无良商家",
            Result = "同意退款",
            Users = "Buyer2,Seller2",
            CreatedAt = DateTime.Now
        };
        updateObj.UpdatedAt = DateTime.Now;
        int increasedAmount = 50;
        var sql = repository.Update<Order>()
            .Set(f => new
            {
                TotalAmount = this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3),
                Products = this.GetProducts(),
                updateObj.Disputes,
                UpdatedAt = DateTime.Now
            })
            .WhereBy(new { updateObj.Id })
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order` SET `TotalAmount`=@p0,`Products`=@p1,`Disputes`=@p2,`UpdatedAt`=NOW() WHERE `Id`=@kId", sql);
        Assert.AreEqual(4, dbParameters.Count);
        Assert.AreEqual("@p0", dbParameters[0].ParameterName);
        Assert.AreEqual(this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3), (double)dbParameters[0].Value);
        Assert.AreEqual("@p1", dbParameters[1].ParameterName);
        Assert.AreEqual(new JsonTypeHandler().ToFieldValue(this.GetProducts()).ToString(), (string)dbParameters[1].Value);
        Assert.AreEqual("@p2", dbParameters[2].ParameterName);
        Assert.AreEqual(new JsonTypeHandler().ToFieldValue(updateObj.Disputes).ToString(), (string)dbParameters[2].Value);
        Assert.AreEqual("@kId", dbParameters[3].ParameterName);
        Assert.AreEqual(updateObj.Id, (string)dbParameters[3].Value);

        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(f => new
            {
                TotalAmount = this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3).Deferred(),
                Products = this.GetProducts(),
                updateObj.Disputes,
                updateObj.UpdatedAt
            })
            .WhereBy(new { updateObj.Id })
            .Execute();

        var updatedOrder = repository.QueryById<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotEmpty(updatedOrder.Products);
            Assert.AreEqual(3, updatedOrder.Products.Count);
            Assert.AreEqual(1, updatedOrder.Products[0]);
            Assert.AreEqual(2, updatedOrder.Products[1]);
            Assert.AreEqual(3, updatedOrder.Products[2]);
            Assert.AreEqual(this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3), updatedOrder.TotalAmount);
            //TODO:两个对象的hash值是不同的，各属性值都是一样
            Assert.AreEqual(JsonSerializer.Serialize(updateObj.Disputes), JsonSerializer.Serialize(updatedOrder.Disputes));
            //TODO:两个日期的ticks是不同的，MySqlConnector驱动保存时间就到秒
            //Assert.IsTrue(updatedOrder.UpdatedAt == updateObj.UpdatedAt);
        }
    }
    [Test]
    public void Update_Set_FromQuery_Multi()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Set(true, (x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .Set(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == 1)
          .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1", sql);
        Assert.IsNotNull(dbParameters);
        Assert.AreEqual("@p0", dbParameters[0].ParameterName);
        Assert.AreEqual(200.56, (double)dbParameters[0].Value);
        Assert.AreEqual("@Products", dbParameters[1].ParameterName);
        Assert.AreEqual(JsonSerializer.Serialize(new List<int> { 1, 2, 3 }), (string)dbParameters[1].Value);

        var count = repository.Update<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Set(true, (x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .Set(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == 1)
            .Execute();
        Assert.Greater(count, 0);

        sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT IFNULL(SUM(b.`Amount`),0) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        count = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .Execute();
        Assert.Greater(count, 0);
    }
    [Test]
    public async Task Update_SetFrom()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT IFNULL(SUM(b.`Amount`),0) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        var count = await repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ExecuteAsync();
        Assert.Greater(count, 0);

        var orderAmounts = repository.From<OrderDetail>()
            .GroupBy(x => x.OrderId)
            .Select((f, a) => new
            {
                OrderId = f.Grouping,
                TotalAmount = f.Sum(a.Amount)
            })
            .ToList();
    }
    [Test]
    public void Update_Set_Join()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Set(true, (x, y) => new
            {
                totalAmount = 200.56,
                orderNo = x.OrderNo + "-111",
                buyerSource = y.SourceType
            })
            .Set(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == 1)
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1", sql);
        Assert.IsNotNull(dbParameters);
        Assert.AreEqual("@p0", dbParameters[0].ParameterName);
        Assert.AreEqual(200.56, (double)dbParameters[0].Value);
        Assert.AreEqual("@Products", dbParameters[1].ParameterName);
        Assert.AreEqual(JsonSerializer.Serialize(new List<int> { 1, 2, 3 }), (string)dbParameters[1].Value);

        var result = repository.Update<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Set(true, (x, y) => new
            {
                totalAmount = 200.56,
                orderNo = x.OrderNo + "-111",
                buyerSource = y.SourceType
            })
            .Set(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == 1)
            .Execute();
        Assert.Greater(result, 0);

        sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                totalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount)),
                orderNo = b.OrderNo + "_111",
                buyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT IFNULL(SUM(b.`Amount`),0) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        result = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                totalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount)),
                orderNo = b.OrderNo + "_111",
                buyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .Execute();
        Assert.Greater(result, 0);
    }
    [Test]
    public async Task Update_Set_FromQuery_One()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var order = repository.QueryById<Order>("1");
        var totalAmount = await repository.From<OrderDetail>()
            .Where(f => f.OrderId == "1")
            .SumAsync(f => f.Amount);
        var sql = repository.Update<Order>()
            .SetFrom(f => f.TotalAmount, (x, y) => x
                .From<OrderDetail>('b')
                .Where(t => t.OrderId == y.Id)
                .SelectAggregate((x, f) => (double)x.Sum(f.Amount)))
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.Id == "1")
            .ToSql(out var dbParameters);
        Assert.AreEqual("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT IFNULL(SUM(b.`Amount`),0) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`Id`='1'", sql);
        Assert.AreEqual(1, dbParameters.Count);
        Assert.AreEqual("ON_111", (string)dbParameters[0].Value);

        var count = await repository.Update<Order>()
            .SetFrom(f => f.TotalAmount, (x, y) => x
                .From<OrderDetail>('b')
                .Where(t => t.OrderId == y.Id)
                .SelectAggregate((x, f) => (double)x.Sum(f.Amount)))
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.Id == "1")
            .ExecuteAsync();
        var reult = repository.QueryById<Order>("1");
        Assert.Greater(count, 0);
        Assert.AreEqual(totalAmount, (decimal)reult.TotalAmount);
        Assert.AreNotEqual(order.TotalAmount, reult.TotalAmount);

        sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount))
            })
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT IFNULL(SUM(b.`Amount`),0) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        count = await repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount))
            })
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ExecuteAsync();
        Assert.Greater(count, 0);
    }
    [Test]
    public void Update_Set_FromQuery_One_Enum()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Company>()
            .SetFrom((a, b) => new
            {
                Nature = a.From<Company>('b')
                    .Where(f => f.Id == 1)
                    .Select(t => t.Nature)
            })
            .Where(f => f.Nature != CompanyNature.Internet)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_company` a SET a.`Nature`=(SELECT b.`Nature` FROM `sys_company` b WHERE b.`Id`=1) WHERE a.`Nature`<>'Internet'", sql);
        repository.BeginTransaction();
        repository.Update<Company>()
            .Set(f => f.Nature, CompanyNature.Industry)
            .Where(f => f.Id > 1)
            .Execute();
        var result = repository.Update<Company>()
            .SetFrom((a, b) => new
            {
                Nature = a.From<Company>('b')
                    .Where(f => f.Id == 1)
                    .Select(t => t.Nature)
            })
            .Where(f => f.Nature != CompanyNature.Internet)
            .Execute();
        var microCompany = repository.From<Company>('b')
            .Where(f => f.Id == 1)
            .First();
        var companies = repository.Query<Company>(f => f.Id > 1);
        repository.Commit();
        Assert.Greater(result, 0);
        foreach (var company in companies)
        {
            Assert.AreEqual(microCompany.Nature, company.Nature);
        }
    }
    [Test]
    public async Task Update_Set_FromQuery_Fields()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .SetFrom((x, y) => new
            {
                TotalAmount = x.From<OrderDetail>('b')
                    .Where(f => f.OrderId == y.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount))
            })
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT IFNULL(SUM(b.`Amount`),0) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        var origValues = await repository.From<Order, OrderDetail>()
            .InnerJoin((x, y) => x.Id == y.OrderId)
            .Where((a, b) => a.BuyerId == 1)
            .GroupBy((a, b) => new { a.Id, a.OrderNo, a.BuyerId })
            .Select((x, a, b) => new { x.Grouping, TotalAmount = x.Sum(b.Amount) })
            .ToListAsync();

        await repository.BeginTransactionAsync();
        var result = repository.Update<Order>()
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .Set((x, y) => new
            {
                TotalAmount = y.Amount,
                OrderNo = x.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where((a, b) => a.BuyerId == 1)
            .Execute();
        var updatedValues = await repository.From<Order, OrderDetail>()
           .InnerJoin((x, y) => x.Id == y.OrderId)
           .Where((a, b) => a.BuyerId == 1)
           .GroupBy((a, b) => new { a.Id, a.OrderNo, a.BuyerId })
           .Select((x, a, b) => new { x.Grouping.Id, x.Grouping.OrderNo, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) })
           .ToListAsync();
        await repository.CommitAsync();
        Assert.Greater(result, 0);
        foreach (var updatedValue in updatedValues)
        {
            var origValue = origValues.Find(f => f.Grouping.Id == updatedValue.Id);
            Assert.AreEqual(origValue.TotalAmount, updatedValue.TotalAmount);
            Assert.AreEqual(origValue.Grouping.OrderNo + "_111", updatedValue.OrderNo);
            Assert.AreEqual(default(int), updatedValue.BuyerId);
        }
    }
    [Test]
    public void Update_InnerJoin_One()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .Set(x => x.TotalAmount, 200.56)
            .Set((a, b) => new
            {
                OrderNo = a.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where((a, b) => a.BuyerId == 1)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);
        var result = repository.Update<Order>()
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .Set(x => x.TotalAmount, 200.56)
            .Set((a, b) => new
            {
                OrderNo = a.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where((a, b) => a.BuyerId == 1)
            .Execute();
        Assert.Greater(result, 0);
    }
    [Test]
    public async Task Update_InnerJoin_Multi()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .Set((x, y) => new
            {
                TotalAmount = y.Amount,
                OrderNo = x.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where((a, b) => a.BuyerId == 1)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=b.`Amount`,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1", sql);

        var origValues = await repository.From<Order, OrderDetail>()
           .InnerJoin((x, y) => x.Id == y.OrderId)
           .Where((a, b) => a.BuyerId == 1)
           .GroupBy((a, b) => new { a.Id, a.OrderNo, a.BuyerId })
           .Select((x, a, b) => new { x.Grouping, TotalAmount = x.Sum(b.Amount) })
           .ToListAsync();

        await repository.BeginTransactionAsync();
        var result = repository.Update<Order>()
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .Set((x, y) => new
            {
                TotalAmount = y.Amount,
                OrderNo = x.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where((a, b) => a.BuyerId == 1)
            .Execute();
        var updatedValues = await repository.From<Order, OrderDetail>()
           .InnerJoin((x, y) => x.Id == y.OrderId)
           .Where((a, b) => a.BuyerId == 1)
           .GroupBy((a, b) => new { a.Id, a.OrderNo, a.BuyerId })
           .Select((x, a, b) => new { x.Grouping.Id, x.Grouping.OrderNo, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) })
           .ToListAsync();
        await repository.CommitAsync();
        Assert.Greater(result, 0);
        foreach (var updatedValue in updatedValues)
        {
            var origValue = origValues.Find(f => f.Grouping.Id == updatedValue.Id);
            Assert.AreEqual(origValue.TotalAmount, updatedValue.TotalAmount);
            Assert.AreEqual(origValue.Grouping.OrderNo + "_111", updatedValue.OrderNo);
            Assert.AreEqual(default(int), updatedValue.BuyerId);
        }
    }
    [Test]
    public async Task Update_InnerJoin_Fields()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SetFrom((x, y) => new
            {
                TotalAmount = x.From<OrderDetail>('c')
                    .Where(f => f.OrderId == y.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount))
            })
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((a, b) => a.Id == "1")
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT IFNULL(SUM(c.`Amount`),0) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,' - ',CAST(b.`Id` AS CHAR)),a.`BuyerId`=NULL WHERE a.`Id`='1'", sql);

        var origValues = await repository.From<Order, User>()
            .InnerJoin((x, y) => x.BuyerId == y.Id)
            .Where((a, b) => a.Id == "1")
            .Select((a, b) => new { a.OrderNo, b.Id })
            .FirstAsync();
        await repository.BeginTransactionAsync();
        var result = repository.Update<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SetFrom((x, y) => new
            {
                TotalAmount = x.From<OrderDetail>('c')
                    .Where(f => f.OrderId == y.Id)
                    .SelectAggregate((x, t) => x.Sum(t.Amount))
            })
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((a, b) => a.Id == "1")
            .Execute();
        var order = await repository.QueryByIdAsync<Order>("1");
        var orderDetails = await repository.QueryAsync<OrderDetail>(f => f.OrderId == "1");
        await repository.CommitAsync();
        Assert.Greater(result, 0);
        Assert.AreEqual(orderDetails.Where(f => f.OrderId == order.Id).Sum(f => f.Amount), order.TotalAmount);
        Assert.AreEqual(origValues.OrderNo + " - " + origValues.Id.ToString(), order.OrderNo);
        Assert.AreEqual(default(int), order.BuyerId);

        var sql1 = repository.Update<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SetFrom(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
                .Where(f => f.OrderId == y.Id)
                .SelectAggregate((x, t) => (double)x.Sum(t.Amount)))
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((x, y) => x.Id == "2")
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT IFNULL(SUM(c.`Amount`),0) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,' - ',CAST(b.`Id` AS CHAR)),a.`BuyerId`=NULL WHERE a.`Id`='2'", sql1);

        origValues = await repository.From<Order, User>()
            .InnerJoin((x, y) => x.BuyerId == y.Id)
            .Where((x, y) => x.Id == "2")
            .Select((a, b) => new { a.OrderNo, b.Id })
            .FirstAsync();
        await repository.BeginTransactionAsync();
        result = repository.Update<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SetFrom(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
                .Where(f => f.OrderId == y.Id)
                .SelectAggregate((x, t) => (double)x.Sum(t.Amount)))
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((x, y) => x.Id == "2")
            .Execute();
        order = await repository.QueryByIdAsync<Order>("2");
        orderDetails = await repository.QueryAsync<OrderDetail>(f => f.OrderId == "2");
        await repository.CommitAsync();

        Assert.Greater(result, 0);
        Assert.AreEqual(orderDetails.Where(f => f.OrderId == order.Id).Sum(f => f.Amount), order.TotalAmount);
        Assert.AreEqual(origValues.OrderNo + " - " + origValues.Id.ToString(), order.OrderNo);
        Assert.AreEqual(default(int), order.BuyerId);
    }
    [Test]
    public void Update_SetNull_WhereNull()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .Set(x => new
            {
                BuyerId = DBNull.Value,
                Seller = (int?)null
            })
            .Where(x => x.OrderNo == null)
            .ToSql(out _);
        Assert.AreEqual("UPDATE `sys_order` SET `BuyerId`=NULL,`Seller`=NULL WHERE `OrderNo` IS NULL", sql);
    }
    [Test]
    public void Update_Set()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var parameter = repository.QueryById<Order>("1");
        parameter.TotalAmount += 50;
        var result = repository.Update<Order>()
            .Set(f => new
            {
                parameter.TotalAmount,
                Products = new List<int> { 1, 2, 3 },
                Disputes = new Dispute
                {
                    Id = 1,
                    Content = "43dss",
                    Users = "1,2",
                    Result = "OK",
                    CreatedAt = DateTime.Now
                }
            })
            .Where(x => x.Id == "1")
            .Execute();
        var order = repository.QueryById<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotEmpty(order.Products);
            Assert.AreEqual(3, order.Products.Count);
            Assert.AreEqual(1, order.Products[0]);
            Assert.AreEqual(2, order.Products[1]);
            Assert.AreEqual(3, order.Products[2]);
            Assert.AreEqual(parameter.TotalAmount, order.TotalAmount);
        }

        repository.BeginTransaction();
        parameter = repository.QueryById<Order>("1");
        parameter.TotalAmount += 50;
        result = repository.Update<Order>()
            .Set(new
            {
                parameter.TotalAmount,
                Products = new List<int> { 1, 2, 3 },
                Disputes = new Dispute
                {
                    Id = 1,
                    Content = "43dss",
                    Users = "1,2",
                    Result = "OK",
                    CreatedAt = DateTime.Now
                }
            })
          .Where(x => x.Id == "1")
          .Execute();
        order = repository.QueryById<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotEmpty(order.Products);
            Assert.AreEqual(3, order.Products.Count);
            Assert.AreEqual(1, order.Products[0]);
            Assert.AreEqual(2, order.Products[1]);
            Assert.AreEqual(3, order.Products[2]);
            Assert.AreEqual(parameter.TotalAmount, order.TotalAmount);
        }
    }
    [Test]
    public void Update_SetJson()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var result = repository.Update<Order>()
            .Set(f => new
            {
                Products = new List<int> { 1, 2, 3 },
                Disputes = new Dispute
                {
                    Id = 1,
                    Content = "43dss",
                    Users = "1,2",
                    Result = "OK",
                    CreatedAt = DateTime.Now
                }
            })
            .Where(x => x.Id == "1")
            .Execute();
        var order = repository.QueryById<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotEmpty(order.Products);
            Assert.AreEqual(3, order.Products.Count);
            Assert.AreEqual(1, order.Products[0]);
            Assert.AreEqual(2, order.Products[1]);
            Assert.AreEqual(3, order.Products[2]);
        }
    }
    [Test]
    public void Update_SetJson1()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        var result = repository.Update<Order>()
            .Set(f => new
            {
                OrderNo = f.OrderNo + "111",
                Products = new List<int> { 1, 2, 3 },
                BuyerId = DBNull.Value,
                UpdatedAt = DateTime.UtcNow
            })
            .Where(x => x.Id == "1")
            .Execute();
        repository.Update<Order>()
            .Set(f => new
            {
                UpdatedAt = DateTime.Now
            })
            .Where(x => x.Id == "2")
            .Execute();
        var order = repository.QueryById<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.IsNotEmpty(order.Products);
            Assert.AreEqual(3, order.Products.Count);
            Assert.AreEqual(1, order.Products[0]);
            Assert.AreEqual(2, order.Products[1]);
            Assert.AreEqual(3, order.Products[2]);
        }
    }
    [Test]
    public void Update_Enum_Fields()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Set((x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .Set(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == 1)
            .ToSql(out var parameters);
        Assert.AreEqual("UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1", sql);
        Assert.AreEqual("@p0", parameters[0].ParameterName);
        Assert.That(parameters[0].Value, Is.TypeOf<double>());
        Assert.AreEqual(200.56, (double)parameters[0].Value);
        Assert.AreEqual("@Products", parameters[1].ParameterName);
        Assert.That(parameters[1].Value, Is.TypeOf<string>());
        Assert.AreEqual(new JsonTypeHandler().ToFieldValue(new List<int> { 1, 2, 3 }).ToString(), (string)parameters[1].Value);

        var sql1 = repository.Update<User>()
            .Set(new { Gender = Gender.Male })
            .WhereBy(new { Id = 1 })
            .ToSql(out var parameters1);
        Assert.AreEqual("UPDATE `sys_user` SET `Gender`=@Gender WHERE `Id`=@kId", sql1);
        Assert.AreEqual("@Gender", parameters1[0].ParameterName);
        Assert.That(parameters1[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(Gender.Male.ToString(), (string)parameters1[0].Value);

        var sql2 = repository.Update<User>()
            .Set(f => new { Gender = Gender.Male })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters2);
        Assert.AreEqual("UPDATE `sys_user` SET `Gender`=@p0 WHERE `Id`=1", sql2);
        Assert.AreEqual("@p0", parameters2[0].ParameterName);
        Assert.That(parameters2[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(Gender.Male.ToString(), (string)parameters2[0].Value);

        var user = new User { Gender = Gender.Female };
        var sql3 = repository.Update<User>()
            .Set(new { user.Gender })
            .Set(f => f.Age, 20)
            .WhereBy(new { Id = 1 })
            .ToSql(out var parameters3);
        Assert.AreEqual("UPDATE `sys_user` SET `Age`=@Age,`Gender`=@Gender WHERE `Id`=@kId", sql3);
        Assert.AreEqual("@Gender", parameters3[1].ParameterName);
        Assert.That(parameters3[1].Value, Is.TypeOf<string>());
        Assert.AreEqual(Gender.Female.ToString(), (string)parameters3[1].Value);

        int age = 20;
        var sql7 = repository.Update<User>()
            .Set(new { Gender = Gender.Male })
            .Set(f => f.Age, age)
            .WhereBy(new { Id = 1 })
            .ToSql(out var parameters7);
        Assert.AreEqual("UPDATE `sys_user` SET `Age`=@Age,`Gender`=@Gender WHERE `Id`=@kId", sql7);
        Assert.AreEqual("@Gender", parameters7[1].ParameterName);
        Assert.That(parameters7[1].Value, Is.TypeOf<string>());
        Assert.AreEqual(Gender.Male.ToString(), (string)parameters7[1].Value);

        var sql4 = repository.Update<Company>()
            .Set(new { Nature = CompanyNature.Internet })
            .WhereBy(new { Id = 1 })
            .ToSql(out var parameters4);
        Assert.AreEqual("UPDATE `sys_company` SET `Nature`=@Nature WHERE `Id`=@kId", sql4);
        Assert.AreEqual("@Nature", parameters4[0].ParameterName);
        Assert.That(parameters4[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters4[0].Value);

        var sql5 = repository.Update<Company>()
            .Set(f => new { Nature = CompanyNature.Internet })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters5);
        Assert.AreEqual("UPDATE `sys_company` SET `Nature`=@p0 WHERE `Id`=1", sql5);
        Assert.AreEqual("@p0", parameters5[0].ParameterName);
        Assert.That(parameters5[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters5[0].Value);

        var sql6 = repository.Update<Company>()
            .Set(f => f.Nature, CompanyNature.Internet)
            .WhereBy(new { Id = 1 })
            .ToSql(out var parameters6);
        Assert.AreEqual("UPDATE `sys_company` SET `Nature`=@Nature WHERE `Id`=@kId", sql6);
        Assert.AreEqual("@Nature", parameters6[0].ParameterName);
        Assert.That(parameters6[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters6[0].Value);

        var company = new Company { Name = "facebook", Nature = CompanyNature.Internet };
        var sql8 = repository.Update<Company>()
            .Set(f => new { Name = f.Name + "_New", company.Nature })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters8);
        Assert.AreEqual("UPDATE `sys_company` SET `Name`=CONCAT(`Name`,'_New'),`Nature`=@p0 WHERE `Id`=1", sql8);
        Assert.AreEqual("@p0", parameters8[0].ParameterName);
        Assert.That(parameters8[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters8[0].Value);

        //批量表达式部分栏位更新
        var sql9 = repository.Update<Company>()
            .SetBulk(new[] { new { Id = 1, Name = "google" }, new { Id = 2, Name = "facebook" } })
            .Set(new { company.Nature })
            .ToSql(out var parameters9);
        Assert.AreEqual("UPDATE `sys_company` SET `Nature`=@Nature,`Name`=@Name0 WHERE `Id`=@kId0;UPDATE `sys_company` SET `Nature`=@Nature,`Name`=@Name1 WHERE `Id`=@kId1", sql9);
        Assert.AreEqual(5, parameters9.Count);
        Assert.AreEqual("@Nature", parameters9[0].ParameterName);
        Assert.That(parameters9[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters9[0].Value);

        CompanyNature? nature = CompanyNature.Production;
        var sql10 = repository.Update<Company>()
            .SetBulk(new[] { new { Id = 1, company.Nature }, new { Id = 2, Nature = nature } })
            .Set(f => new { company.Name })
            .OnlyFields(f => f.Nature)
            .ToSql(out var parameters10);
        Assert.AreEqual("UPDATE `sys_company` SET `Name`=@p0,`Nature`=@Nature0 WHERE `Id`=@kId0;UPDATE `sys_company` SET `Name`=@p0,`Nature`=@Nature1 WHERE `Id`=@kId1", sql10);
        Assert.AreEqual("@Nature0", parameters10[1].ParameterName);
        Assert.That(parameters10[1].Value, Is.TypeOf<string>());
        Assert.AreEqual(company.Nature.ToString(), (string)parameters10[1].Value);
        Assert.AreEqual("@Nature1", parameters10[3].ParameterName);
        Assert.That(parameters10[3].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Production.ToString(), (string)parameters10[3].Value);
        Assert.AreEqual("@p0", parameters10[0].ParameterName);
        Assert.That(parameters10[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(company.Name, (string)parameters10[0].Value);
    }
    [Test]
    public async Task Update_TimeSpan_Fields()
    {
        var repository = this.dbFactory.Create();
        var timeSpan = TimeSpan.FromMinutes(455);
        await repository.DeleteByIdAsync<UpdateEntity1>(1);
        await repository.CreateAsync<UpdateEntity1>(new UpdateEntity1
        {
            Id = 1,
            BooleanField = true,
            TimeSpanField = TimeSpan.FromSeconds(456),
#if NET6_0_OR_GREATER
            DateOnlyField = new DateOnly(2022, 05, 06),
#else
            DateOnlyField = new DateTime(2022, 05, 06),
#endif
            DateTimeField = DateTime.Now,
            DateTimeOffsetField = new DateTimeOffset(DateTime.Parse("2022-01-02 03:04:05")),
            EnumField = Gender.Male,
            GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
            TimeOnlyField = new TimeOnly(3, 5, 7)
#else
            TimeOnlyField = new TimeSpan(3, 5, 7)
#endif
        });
        var sql1 = repository.Update<User>()
            .Set(new { SomeTimes = timeSpan })
            .WhereBy(new { Id = 1 })
            .ToSql(out var parameters1);
        Assert.AreEqual("UPDATE `sys_user` SET `SomeTimes`=@SomeTimes WHERE `Id`=@kId", sql1);
        Assert.AreEqual("@SomeTimes", parameters1[0].ParameterName);
#if NET6_0_OR_GREATER
        Assert.That(parameters1[0].Value, Is.TypeOf<TimeOnly>());
        Assert.AreEqual(TimeOnly.FromTimeSpan(timeSpan), (TimeOnly)parameters1[0].Value);
#else
        Assert.That(parameters1[0].Value, Is.TypeOf<TimeSpan>());
        Assert.AreEqual(timeSpan, (TimeSpan)parameters1[0].Value);
#endif

        var sql2 = repository.Update<User>()
            .Set(f => new { SomeTimes = timeSpan })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters2);
        Assert.AreEqual("UPDATE `sys_user` SET `SomeTimes`=@p0 WHERE `Id`=1", sql2);
        Assert.AreEqual("@p0", parameters2[0].ParameterName);
#if NET6_0_OR_GREATER
        Assert.That(parameters2[0].Value, Is.TypeOf<TimeOnly>());
        Assert.AreEqual(TimeOnly.FromTimeSpan(timeSpan), (TimeOnly)parameters2[0].Value);
#else
        Assert.That(parameters2[0].Value, Is.TypeOf<TimeSpan>());
        Assert.AreEqual(timeSpan, (TimeSpan)parameters2[0].Value);
#endif

        repository.BeginTransaction();
        await repository.Update<User>()
            .Set(new { SomeTimes = timeSpan })
            .WhereBy(new { Id = 1 })
            .ExecuteAsync();
        var userInfo = repository.QueryById<User>(1);
        repository.Commit();
#if NET6_0_OR_GREATER
        Assert.AreEqual(TimeOnly.FromTimeSpan(timeSpan), userInfo.SomeTimes.Value);
#else
        Assert.AreEqual(timeSpan, userInfo.SomeTimes.Value);
#endif
    }
    [Test]
    public async Task Update_BulkCopy()
    {
        var repository = this.dbFactory.Create();
        var orders = new List<Order>();
        for (int i = 1000; i < 2000; i++)
        {
            orders.Add(new Order
            {
                Id = $"ON_{i + 1}",
                TenantId = "3",
                OrderNo = $"ON-{i + 1}",
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
        }
        var removeIds = orders.Select(f => f.Id).ToList();
        await repository.BeginTransactionAsync();
        await repository.Delete<Order>()
            .Where(f => removeIds.Contains(f.Id))
            .ExecuteAsync();
        var count = await repository.Create<Order>()
            .WithBulkCopy(orders)
            .ExecuteAsync();
        await repository.CommitAsync();

        var updateObjs = orders.Select(f => new
        {
            f.Id,
            TenantId = "1",
            TotalAmount = f.TotalAmount + 20,
            Products = new List<int> { 1, 2, 3 },
            Disputes = new Dispute
            {
                Id = 1,
                Content = "无良商家",
                Result = "同意退款",
                Users = "Buyer1,Seller1",
                CreatedAt = DateTime.Now
            }
        });
        count = await repository.Update<Order>()
            .SetBulkCopy(updateObjs)
            .ExecuteAsync();

        Assert.AreEqual(orders.Count, count);
    }
    private double CalcAmount(double price, double amount) => price * amount - 150;
    private int[] GetProducts() => new int[] { 1, 2, 3 };
}