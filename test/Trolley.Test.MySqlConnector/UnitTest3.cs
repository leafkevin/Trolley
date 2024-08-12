using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trolley.MySqlConnector;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.MySqlConnector;

public class UnitTest3 : UnitTestBase
{
    private static int connTotal = 0;
    private static int connOpenTotal = 0;
    private readonly ITestOutputHelper output;
    public UnitTest3(ITestOutputHelper output)
    {
        this.output = output;
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
                .Register(OrmProviderType.MySql, "fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", true)
                .Configure<ModelConfiguration>(OrmProviderType.MySql)
                .UseDbFilter(df =>
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
    public void Update_AnonymousObject()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var user = repository.Get<User>(1);
        user.Name = "kevin";
        user.Gender = Gender.Female;
        user.SourceType = null;
        var count = repository.Update<User>(user);
        var changedUser = repository.Get<User>(1);
        Assert.True(count > 0);
        Assert.NotNull(changedUser);
        Assert.True(changedUser.Name == user.Name);
        Assert.True(changedUser.SourceType == changedUser.SourceType);

        count = repository.Update<User>(new
        {
            Id = 1,
            Name = (string)null,
            Gender = Gender.Male,
            SourceType = UserSourceType.Douyin
        });
        var result = repository.Get<User>(1);
        Assert.True(count > 0);
        Assert.NotNull(result);
        Assert.True(result.Name == null);
        Assert.True(result.SourceType == UserSourceType.Douyin);
    }
    [Fact]
    public async Task Update_AnonymousObjects()
    {
        Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(count > 0);
        Assert.True(parameters.Count == orders.Count);
        orders.Sort((x, y) => x.Id.CompareTo(y.Id));
        for (int i = 0; i < orders.Count; i++)
        {
            Assert.True(orders[i].TotalAmount == parameters[i].TotalAmount);
        }
    }
    [Fact]
    public async Task Update_SetBulk()
    {
        Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "UPDATE `sys_order_detail` SET `Amount`=@Amount0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Amount`=@Amount1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Amount`=@Amount2 WHERE `Id`=@kId2");
        Assert.True(dbParameters.Count == parameters.Count * 2);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.True(dbParameters[i * 2].ParameterName == $"@Amount{i}");
            Assert.True(dbParameters[i * 2 + 1].ParameterName == $"@kId{i}");
        }
    }
    [Fact]
    public async Task Update_SetBulk_OnlyFields()
    {
        Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "UPDATE `sys_order_detail` SET `Price`=@Price0,`Quantity`=@Quantity0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Price`=@Price1,`Quantity`=@Quantity1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Price`=@Price2,`Quantity`=@Quantity2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Price`=@Price3,`Quantity`=@Quantity3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Price`=@Price4,`Quantity`=@Quantity4 WHERE `Id`=@kId4");
        Assert.True(dbParameters.Count == parameters.Count * 3);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.True(dbParameters[i * 3].ParameterName == $"@Price{i}");
            Assert.True(dbParameters[i * 3 + 1].ParameterName == $"@Quantity{i}");
            Assert.True(dbParameters[i * 3 + 2].ParameterName == $"@kId{i}");
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
        var updatedDetails = await repository.QueryAsync<OrderDetail>(f => ids.Contains(f.Id));
        repository.Commit();
        Assert.True(result == parameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.True(updatedDetails[i].Price == parameters[i].Price);
            Assert.True(updatedDetails[i].Quantity == parameters[i].Quantity);
            Assert.True(updatedDetails[i].Amount != parameters[i].Amount);
        }
    }
    [Fact]
    public async Task Update_SetBulk_IgnoreFields()
    {
        Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "UPDATE `sys_order_detail` SET `Quantity`=@Quantity0,`Amount`=@Amount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Quantity`=@Quantity1,`Amount`=@Amount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Quantity`=@Quantity2,`Amount`=@Amount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Quantity`=@Quantity3,`Amount`=@Amount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Quantity`=@Quantity4,`Amount`=@Amount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4");
        Assert.True(dbParameters.Count == parameters.Count * 4);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.True(dbParameters[i * 4].ParameterName == $"@Quantity{i}");
            Assert.True(dbParameters[i * 4 + 1].ParameterName == $"@Amount{i}");
            Assert.True(dbParameters[i * 4 + 2].ParameterName == $"@UpdatedAt{i}");
            Assert.True(dbParameters[i * 4 + 3].ParameterName == $"@kId{i}");
        }
        var ids = parameters.Select(f => f.Id).ToList();
        repository.BeginTransaction();
        var result = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .IgnoreFields(f => f.Price)
            .Execute();
        var updatedDetails = await repository.QueryAsync<OrderDetail>(f => ids.Contains(f.Id));
        repository.Commit();
        Assert.True(result == parameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.True(updatedDetails[i].Price != parameters[i].Price);
            Assert.True(updatedDetails[i].Quantity == parameters[i].Quantity);
            Assert.True(updatedDetails[i].Amount == parameters[i].Amount);
            Assert.True(updatedDetails[i].UpdatedAt == parameters[i].UpdatedAt);
            Assert.True(updatedDetails[i].Id == parameters[i].Id);
        }
    }
    [Fact]
    public async Task Update_SetBulk_SetFields()
    {
        Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4");
        Assert.True(dbParameters.Count == parameters.Count * 3 + 2);

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
        Assert.True(result == parameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.True(updatedDetails[i].ProductId == 3);
            Assert.True(updatedDetails[i].Quantity == 5);
            Assert.True(updatedDetails[i].Amount == parameters[i].Amount);
            Assert.True(updatedDetails[i].UpdatedAt == parameters[i].UpdatedAt);
        }
    }
    [Fact]
    public void Update_Fields_Where()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.Update<User>(f => new
        {
            Name = f.Name + "_1",
            Gender = Gender.Female,
            SourceType = DBNull.Value
        }, t => t.Id == 1);
        var result1 = repository.Get<User>(1);
        Assert.True(result > 0);
        Assert.NotNull(result1);
        Assert.True(result1.Name == "leafkevin_1");
        Assert.True(result1.SourceType.HasValue == false);

        var result2 = repository.Update<User>(new
        {
            Id = 1,
            Name = "kevin",
            Gender = Gender.Female,
            SourceType = DBNull.Value
        });
        var result3 = repository.Get<User>(1);
        Assert.True(result2 > 0);
        Assert.NotNull(result3);
        Assert.True(result3.Name == "kevin");
        Assert.True(result3.SourceType.HasValue == false);
    }
    [Fact]
    public void Update_Set_Fields_Where()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.Update<User>()
            .Set(f => new
            {
                Age = 25,
                Name = "leafkevin22",
                CompanyId = DBNull.Value
            })
            .Where(f => f.Id == 1)
            .Execute();
        var result2 = repository.Get<User>(1);
        Assert.True(result > 0);
        Assert.NotNull(result2);
        Assert.True(result2.Name == "leafkevin22");
        Assert.True(result2.Age == 25);
        Assert.True(result2.CompanyId == 0);
    }
    [Fact]
    public void Update_Fields_Parameters()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.Update<User>(new { Id = 1, Name = "leafkevin11" });
        var result1 = repository.Get<User>(1);
        Assert.True(result > 0);
        Assert.NotNull(result1);
        Assert.True(result1.Name == "leafkevin11");
    }
    [Fact]
    public void Update_Set_AnonymousObject_Where()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<User>()
            .Set(new
            {
                Age = 25,
                Name = "leafkevin22",
                CompanyId = DBNull.Value
            })
            .Where(f => f.Id == 1)
            .ToSql(out var dbParameters);
        Assert.True(sql == "UPDATE `sys_user` SET `Age`=@Age,`Name`=@Name,`CompanyId`=@CompanyId WHERE `Id`=1");
        Assert.True(dbParameters.Count == 3);
        Assert.True((int)dbParameters[0].Value == 25);
        Assert.True((string)dbParameters[1].Value == "leafkevin22");
        Assert.True(dbParameters[2].Value == DBNull.Value);

        repository.Update<User>()
           .Set(f => new
           {
               Age = 25,
               Name = "leafkevin22",
               CompanyId = DBNull.Value
           })
           .Where(f => f.Id == 1)
           .Execute();
        var result = repository.Get<User>(1);
        Assert.True(result.Name == "leafkevin22");
        Assert.True(result.Age == 25);
        Assert.True(result.CompanyId == 0);
    }
    [Fact]
    public void Update_Set_AnonymousObject_Where_OnlyFields()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var user = repository.Get<User>(1);
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
        Assert.True(sql == "UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=1");
        Assert.True(dbParameters.Count == 1);
        Assert.True((string)dbParameters[0].Value == "leafkevinabc");

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
        var result = repository.Get<User>(1);
        Assert.True(result.Name == "leafkevinabc");
        Assert.True(result.Age == user.Age);
        Assert.True(result.CompanyId == user.CompanyId);
    }
    [Fact]
    public void Update_Set_AnonymousObject_Where_IgnoreFields()
    {
        Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "UPDATE `sys_user` SET `Age`=@Age,`CompanyId`=@CompanyId WHERE `Id`=1");
        Assert.True(dbParameters.Count == 2);
        Assert.True((int)dbParameters[0].Value == 25);
        Assert.True(dbParameters[1].Value == DBNull.Value);

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
        var result = repository.Get<User>(1);
        Assert.True(result.Name != "leafkevin22");
        Assert.True(result.Age == 25);
        Assert.True(result.CompanyId == 0);
    }
    [Fact]
    public void Update_SetWith_Parameters()
    {
        Initialize();
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        var result = repository.Update<Order>(new
        {
            ProductCount = 10,
            Id = 1
        });
        var result1 = repository.Get<Order>(new { Id = "1" });
        repository.Commit();
        if (result > 0)
        {
            Assert.NotNull(result1);
            Assert.True(result1.Id == "1");
            Assert.True(result1.ProductCount == 10);
        }
        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(new { ProductCount = 11 })
            .Where(new { Id = "1" })
            .Execute();
        var result2 = repository.Get<Order>(new { Id = "1" });
        repository.Commit();
        if (result > 0)
        {
            Assert.NotNull(result2);
            Assert.True(result2.Id == "1");
            Assert.True(result2.ProductCount == 11);
        }
        var updateObj = new Dictionary<string, object>();
        updateObj.Add("ProductCount", result2.ProductCount + 1);
        updateObj.Add("TotalAmount", result2.TotalAmount + 100);
        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(updateObj)
            .Where(new { Id = "1" })
            .Execute();
        var result3 = repository.Get<Order>(new { Id = "1" });
        repository.Commit();
        if (result > 0)
        {
            Assert.NotNull(result3);
            Assert.True(result3.Id == "1");
            Assert.True(result3.ProductCount == result2.ProductCount + 1);
            Assert.True(result3.TotalAmount == result2.TotalAmount + 100);
        }
    }
    [Fact]
    public async Task Update_MultiParameters()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        var parameters = await repository.From<OrderDetail>()
            .Where(f => new[] { "1", "2", "3", "4", "5", "6" }.Contains(f.Id))
            .OrderBy(f => f.Id)
            .Select(f => new { f.Id, Price = f.Price + 80, Quantity = f.Quantity + 2, Amount = f.Amount + 100 })
            .ToListAsync();
        var count = repository.Update<OrderDetail>(parameters);
        var orderDetails = await repository.From<OrderDetail>()
            .Where(f => new[] { "1", "2", "3", "4", "5", "6" }.Contains(f.Id))
            .Select()
            .ToListAsync();
        repository.Commit();
        Assert.True(count > 0);
        for (int i = 0; i < orderDetails.Count; i++)
        {
            Assert.True(orderDetails[i].Price == parameters[i].Price);
            Assert.True(orderDetails[i].Quantity == parameters[i].Quantity);
            Assert.True(orderDetails[i].Amount == parameters[i].Amount);
        }
    }
    [Fact]
    public void Update_Set_MethodCall()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        var parameter = repository.Get<Order>("1");
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
        var order = repository.Get<Order>("2");
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(order.Products);
            Assert.True(order.Products.Count == 3);
            Assert.True(order.Products[0] == 1);
            Assert.True(order.Products[1] == 2);
            Assert.True(order.Products[2] == 3);
            Assert.True(order.TotalAmount == this.CalcAmount(parameter.TotalAmount, 3));
        }

        var updateObj = repository.Get<Order>("1");
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
            .Where(new { updateObj.Id })
            .ToSql(out var dbParameters);
        Assert.True(sql == "UPDATE `sys_order` SET `TotalAmount`=@p0,`Products`=@p1,`Disputes`=@p2,`UpdatedAt`=NOW() WHERE `Id`=@kId");
        Assert.True(dbParameters.Count == 4);
        Assert.True(dbParameters[0].ParameterName == "@p0");
        Assert.True((double)dbParameters[0].Value == this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3));
        Assert.True(dbParameters[1].ParameterName == "@p1");
        Assert.True((string)dbParameters[1].Value == new JsonTypeHandler().ToFieldValue(null, this.GetProducts()).ToString());
        Assert.True(dbParameters[2].ParameterName == "@p2");
        Assert.True((string)dbParameters[2].Value == new JsonTypeHandler().ToFieldValue(null, updateObj.Disputes).ToString());
        Assert.True(dbParameters[3].ParameterName == "@kId");
        Assert.True((string)dbParameters[3].Value == updateObj.Id);

        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(f => new
            {
                TotalAmount = this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3).Deferred(),
                Products = this.GetProducts(),
                updateObj.Disputes,
                updateObj.UpdatedAt
            })
            .Where(new { updateObj.Id })
            .Execute();

        var updatedOrder = repository.Get<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(updatedOrder.Products);
            Assert.True(updatedOrder.Products.Count == 3);
            Assert.True(updatedOrder.Products[0] == 1);
            Assert.True(updatedOrder.Products[1] == 2);
            Assert.True(updatedOrder.Products[2] == 3);
            Assert.True(updatedOrder.TotalAmount == this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3));
            //TODO:两个对象的hash值是不同的，各属性值都是一样
            Assert.True(JsonSerializer.Serialize(updatedOrder.Disputes) == JsonSerializer.Serialize(updateObj.Disputes));
            //TODO:两个日期的ticks是不同的，MySqlConnector驱动保存时间就到秒
            //Assert.True(updatedOrder.UpdatedAt == updateObj.UpdatedAt);
        }
    }
    [Fact]
    public void Update_Set_FromQuery_Multi()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1");
        Assert.NotNull(dbParameters);
        Assert.True(dbParameters[0].ParameterName == "@p0");
        Assert.True((double)dbParameters[0].Value == 200.56);
        Assert.True(dbParameters[1].ParameterName == "@Products");
        Assert.True((string)dbParameters[1].Value == JsonSerializer.Serialize(new List<int> { 1, 2, 3 }));

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
        Assert.True(count > 0);

        sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1");

        count = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .Execute();
        Assert.True(count > 0);
    }
    [Fact]
    public async Task Update_SetFrom()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1");

        var count = await repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ExecuteAsync();
        Assert.True(count > 0);

        var orderAmounts = repository.From<OrderDetail>()
            .GroupBy(x => x.OrderId)
            .Select((f, a) => new
            {
                OrderId = f.Grouping,
                TotalAmount = f.Sum(a.Amount)
            })
            .ToList();
    }
    [Fact]
    public void Update_Set_Join()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1");
        Assert.NotNull(dbParameters);
        Assert.True(dbParameters[0].ParameterName == "@p0");
        Assert.True((double)dbParameters[0].Value == 200.56);
        Assert.True(dbParameters[1].ParameterName == "@Products");
        Assert.True((string)dbParameters[1].Value == JsonSerializer.Serialize(new List<int> { 1, 2, 3 }));

        var result = repository.Update<Order>()
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
        Assert.True(result > 0);

        sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1");

        result = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount)),
                OrderNo = b.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where(a => a.BuyerId == 1)
            .Execute();
        Assert.True(result > 0);
    }
    [Fact]
    public async Task Update_Set_FromQuery_One()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var order = repository.Get<Order>("1");
        var totalAmount = await repository.From<OrderDetail>()
            .Where(f => f.OrderId == "1")
            .SumAsync(f => f.Amount);
        var sql = repository.Update<Order>()
            .SetFrom(f => f.TotalAmount, (x, y) => x
                .From<OrderDetail>('b')
                .Where(t => t.OrderId == y.Id)
                .Select(f => Sql.Sum(f.Amount)))
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.Id == "1")
            .ToSql(out var dbParameters);
        Assert.True(sql == "UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`Id`='1'");
        Assert.True(dbParameters.Count == 1);
        Assert.True((string)dbParameters[0].Value == "ON_111");

        var count = await repository.Update<Order>()
            .SetFrom(f => f.TotalAmount, (x, y) => x
                .From<OrderDetail>('b')
                .Where(t => t.OrderId == y.Id)
                .Select(f => Sql.Sum(f.Amount)))
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.Id == "1")
            .ExecuteAsync();
        var reult = repository.Get<Order>("1");
        Assert.True(count > 0);
        Assert.True(reult.TotalAmount == totalAmount);
        Assert.True(reult.TotalAmount != order.TotalAmount);

        sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`BuyerId`=1");

        count = await repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ExecuteAsync();
        Assert.True(count > 0);
    }
    [Fact]
    public void Update_Set_FromQuery_One_Enum()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<Company>()
            .SetFrom((a, b) => new
            {
                Nature = a.From<Company>('b')
                    .Where(f => f.Id == 1)
                    .Select(t => t.Nature)
            })
            .Where(f => f.Nature != CompanyNature.Internet)
            .ToSql(out _);
        Assert.True(sql == "UPDATE `sys_company` a SET a.`Nature`=(SELECT b.`Nature` FROM `sys_company` b WHERE b.`Id`=1) WHERE a.`Nature`<>'Internet'");
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
            .Where(f => f.Name.Contains("微软"))
            .First();
        var companies = repository.Query<Company>(f => f.Id > 1);
        repository.Commit();
        Assert.True(result > 0);
        foreach (var company in companies)
        {
            Assert.True(company.Nature == microCompany.Nature);
        }
    }
    [Fact]
    public async Task Update_Set_FromQuery_Fields()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .SetFrom((x, y) => new
            {
                TotalAmount = x.From<OrderDetail>('b')
                    .Where(f => f.OrderId == y.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`BuyerId`=1");

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
        Assert.True(result > 0);
        foreach (var updatedValue in updatedValues)
        {
            var origValue = origValues.Find(f => f.Grouping.Id == updatedValue.Id);
            Assert.True(updatedValue.TotalAmount == origValue.TotalAmount);
            Assert.True(updatedValue.OrderNo == origValue.Grouping.OrderNo + "_111");
            Assert.True(updatedValue.BuyerId == default);
        }
    }
    [Fact]
    public void Update_InnerJoin_One()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1");
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
        Assert.True(result > 0);
    }
    [Fact]
    public async Task Update_InnerJoin_Multi()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=b.`Amount`,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1");

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
        Assert.True(result > 0);
        foreach (var updatedValue in updatedValues)
        {
            var origValue = origValues.Find(f => f.Grouping.Id == updatedValue.Id);
            Assert.True(updatedValue.TotalAmount == origValue.TotalAmount);
            Assert.True(updatedValue.OrderNo == origValue.Grouping.OrderNo + "_111");
            Assert.True(updatedValue.BuyerId == default);
        }
    }
    [Fact]
    public async Task Update_InnerJoin_Fields()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SetFrom((x, y) => new
            {
                TotalAmount = x.From<OrderDetail>('c')
                    .Where(f => f.OrderId == y.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((a, b) => a.Id == "1")
            .ToSql(out _);
        Assert.True(sql == "UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,' - ',CAST(b.`Id` AS CHAR)),a.`BuyerId`=NULL WHERE a.`Id`='1'");

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
                    .Select(t => Sql.Sum(t.Amount))
            })
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((a, b) => a.Id == "1")
            .Execute();
        var order = await repository.GetAsync<Order>("1");
        var orderDetails = await repository.QueryAsync<OrderDetail>(f => f.OrderId == "1");
        await repository.CommitAsync();
        Assert.True(result > 0);
        Assert.True(order.TotalAmount == orderDetails.Where(f => f.OrderId == order.Id).Sum(f => f.Amount));
        Assert.True(order.OrderNo == origValues.OrderNo + " - " + origValues.Id.ToString());
        Assert.True(order.BuyerId == default);

        var sql1 = repository.Update<Order>()
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .SetFrom(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
                .Where(f => f.OrderId == y.Id)
                .Select(t => Sql.Sum(t.Amount)))
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((x, y) => x.Id == "2")
            .ToSql(out _);
        Assert.True(sql1 == "UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,' - ',CAST(b.`Id` AS CHAR)),a.`BuyerId`=NULL WHERE a.`Id`='2'");

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
                .Select(t => Sql.Sum(t.Amount)))
            .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((x, y) => x.Id == "2")
            .Execute();
        order = await repository.GetAsync<Order>("2");
        orderDetails = await repository.QueryAsync<OrderDetail>(f => f.OrderId == "2");
        await repository.CommitAsync();

        Assert.True(result > 0);
        Assert.True(order.TotalAmount == orderDetails.Where(f => f.OrderId == order.Id).Sum(f => f.Amount));
        Assert.True(order.OrderNo == origValues.OrderNo + " - " + origValues.Id.ToString());
        Assert.True(order.BuyerId == default);
    }
    [Fact]
    public void Update_SetNull_WhereNull()
    {
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .Set(x => new
            {
                BuyerId = DBNull.Value,
                Seller = (int?)null
            })
            .Where(x => x.OrderNo == null)
            .ToSql(out _);
        Assert.True(sql == "UPDATE `sys_order` SET `BuyerId`=NULL,`Seller`=NULL WHERE `OrderNo` IS NULL");
    }
    [Fact]
    public void Update_Set()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        var parameter = repository.Get<Order>("1");
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
        var order = repository.Get<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(order.Products);
            Assert.True(order.Products.Count == 3);
            Assert.True(order.Products[0] == 1);
            Assert.True(order.Products[1] == 2);
            Assert.True(order.Products[2] == 3);
            Assert.True(order.TotalAmount == parameter.TotalAmount);
        }

        repository.BeginTransaction();
        parameter = repository.Get<Order>("1");
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
        order = repository.Get<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(order.Products);
            Assert.True(order.Products.Count == 3);
            Assert.True(order.Products[0] == 1);
            Assert.True(order.Products[1] == 2);
            Assert.True(order.Products[2] == 3);
            Assert.True(order.TotalAmount == parameter.TotalAmount);
        }
    }
    [Fact]
    public void Update_SetJson()
    {
        using var repository = dbFactory.Create();
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
        var order = repository.Get<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(order.Products);
            Assert.True(order.Products.Count == 3);
            Assert.True(order.Products[0] == 1);
            Assert.True(order.Products[1] == 2);
            Assert.True(order.Products[2] == 3);
        }
    }
    [Fact]
    public void Update_SetJson1()
    {
        using var repository = dbFactory.Create();
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
        var order = repository.Get<Order>("1");
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(order.Products);
            Assert.True(order.Products.Count == 3);
            Assert.True(order.Products[0] == 1);
            Assert.True(order.Products[1] == 2);
            Assert.True(order.Products[2] == 3);
        }
    }
    [Fact]
    public void Update_Enum_Fields()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
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
        Assert.True(sql == "UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p0,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products WHERE a.`BuyerId`=1");
        Assert.True(parameters[0].ParameterName == "@p0");
        Assert.True(parameters[0].Value.GetType() == typeof(double));
        Assert.True((double)parameters[0].Value == 200.56);
        Assert.True(parameters[1].ParameterName == "@Products");
        Assert.True(parameters[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters[1].Value == new JsonTypeHandler().ToFieldValue(null, new List<int> { 1, 2, 3 }).ToString());

        var sql1 = repository.Update<User>()
            .Set(new { Gender = Gender.Male })
            .Where(new { Id = 1 })
            .ToSql(out var parameters1);
        Assert.True(sql1 == "UPDATE `sys_user` SET `Gender`=@Gender WHERE `Id`=@kId");
        Assert.True(parameters1[0].ParameterName == "@Gender");
        Assert.True(parameters1[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters1[0].Value == Gender.Male.ToString());

        var sql2 = repository.Update<User>()
            .Set(f => new { Gender = Gender.Male })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters2);
        Assert.True(sql2 == "UPDATE `sys_user` SET `Gender`=@p0 WHERE `Id`=1");
        Assert.True(parameters2[0].ParameterName == "@p0");
        Assert.True(parameters2[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters2[0].Value == Gender.Male.ToString());

        var user = new User { Gender = Gender.Female };
        var sql3 = repository.Update<User>()
            .Set(f => f.Age, 20)
            .Set(new { user.Gender })
            .Where(new { Id = 1 })
            .ToSql(out var parameters3);
        Assert.True(sql3 == "UPDATE `sys_user` SET `Age`=@Age,`Gender`=@Gender WHERE `Id`=@kId");
        Assert.True(parameters3[1].ParameterName == "@Gender");
        Assert.True(parameters3[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters3[1].Value == Gender.Female.ToString());

        int age = 20;
        var sql7 = repository.Update<User>()
            .Set(f => f.Age, age)
            .Set(new { Gender = Gender.Male })
            .Where(new { Id = 1 })
            .ToSql(out var parameters7);
        Assert.True(sql7 == "UPDATE `sys_user` SET `Age`=@Age,`Gender`=@Gender WHERE `Id`=@kId");
        Assert.True(parameters7[1].ParameterName == "@Gender");
        Assert.True(parameters7[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters7[1].Value == Gender.Male.ToString());

        var sql4 = repository.Update<Company>()
             .Set(new { Nature = CompanyNature.Internet })
             .Where(new { Id = 1 })
             .ToSql(out var parameters4);
        Assert.True(sql4 == "UPDATE `sys_company` SET `Nature`=@Nature WHERE `Id`=@kId");
        Assert.True(parameters4[0].ParameterName == "@Nature");
        Assert.True(parameters4[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters4[0].Value == CompanyNature.Internet.ToString());

        var sql5 = repository.Update<Company>()
            .Set(f => new { Nature = CompanyNature.Internet })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters5);
        Assert.True(sql5 == "UPDATE `sys_company` SET `Nature`=@p0 WHERE `Id`=1");
        Assert.True(parameters5[0].ParameterName == "@p0");
        Assert.True(parameters5[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters5[0].Value == CompanyNature.Internet.ToString());

        var sql6 = repository.Update<Company>()
            .Set(f => f.Nature, CompanyNature.Internet)
            .Where(new { Id = 1 })
            .ToSql(out var parameters6);
        Assert.True(sql6 == "UPDATE `sys_company` SET `Nature`=@Nature WHERE `Id`=@kId");
        Assert.True(parameters6[0].ParameterName == "@Nature");
        Assert.True(parameters6[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters6[0].Value == CompanyNature.Internet.ToString());

        var company = new Company { Name = "facebook", Nature = CompanyNature.Internet };
        var sql8 = repository.Update<Company>()
            .Set(f => new { Name = f.Name + "_New", company.Nature })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters8);
        Assert.True(sql8 == "UPDATE `sys_company` SET `Name`=CONCAT(`Name`,'_New'),`Nature`=@p0 WHERE `Id`=1");
        Assert.True(parameters8[0].ParameterName == "@p0");
        Assert.True(parameters8[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters8[0].Value == CompanyNature.Internet.ToString());

        //批量表达式部分栏位更新
        var sql9 = repository.Update<Company>()
            .SetBulk(new[] { new { Id = 1, Name = "google" }, new { Id = 2, Name = "facebook" } })
            .Set(new { company.Nature })
            .ToSql(out var parameters9);
        Assert.True(sql9 == "UPDATE `sys_company` SET `Nature`=@Nature,`Name`=@Name0 WHERE `Id`=@kId0;UPDATE `sys_company` SET `Nature`=@Nature,`Name`=@Name1 WHERE `Id`=@kId1");
        Assert.True(parameters9.Count == 5);
        Assert.True(parameters9[0].ParameterName == "@Nature");
        Assert.True(parameters9[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters9[0].Value == CompanyNature.Internet.ToString());

        CompanyNature? nature = CompanyNature.Production;
        var sql10 = repository.Update<Company>()
            .SetBulk(new[] { new { Id = 1, company.Nature }, new { Id = 2, Nature = nature } })
            .Set(f => new { company.Name })
            .OnlyFields(f => f.Nature)
            .ToSql(out var parameters10);
        Assert.True(sql10 == "UPDATE `sys_company` SET `Name`=@p0,`Nature`=@Nature0 WHERE `Id`=@kId0;UPDATE `sys_company` SET `Name`=@p0,`Nature`=@Nature1 WHERE `Id`=@kId1");
        Assert.True(parameters10[1].ParameterName == "@Nature0");
        Assert.True(parameters10[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters10[1].Value == company.Nature.ToString());
        Assert.True(parameters10[3].ParameterName == "@Nature1");
        Assert.True(parameters10[3].Value.GetType() == typeof(string));
        Assert.True((string)parameters10[3].Value == CompanyNature.Production.ToString());
        Assert.True(parameters10[0].ParameterName == "@p0");
        Assert.True(parameters10[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters10[0].Value == company.Name);
    }
    [Fact]
    public async Task Update_TimeSpan_Fields()
    {
        using var repository = dbFactory.Create();
        var timeSpan = TimeSpan.FromMinutes(455);
        await repository.DeleteAsync<UpdateEntity>(1);
        await repository.CreateAsync<UpdateEntity>(new UpdateEntity
        {
            Id = 1,
            BooleanField = true,
            TimeSpanField = TimeSpan.FromSeconds(456),
            DateOnlyField = new DateOnly(2022, 05, 06),
            DateTimeField = DateTime.Now,
            DateTimeOffsetField = new DateTimeOffset(DateTime.Parse("2022-01-02 03:04:05")),
            EnumField = Gender.Male,
            GuidField = Guid.NewGuid(),
            TimeOnlyField = new TimeOnly(3, 5, 7)
        });
        var sql1 = repository.Update<User>()
            .Set(new { SomeTimes = timeSpan })
            .Where(new { Id = 1 })
            .ToSql(out var parameters1);
        Assert.True(sql1 == "UPDATE `sys_user` SET `SomeTimes`=@SomeTimes WHERE `Id`=@kId");
        Assert.True(parameters1[0].ParameterName == "@SomeTimes");
        Assert.True(parameters1[0].Value.GetType() == typeof(TimeOnly));
        Assert.True((TimeOnly)parameters1[0].Value == TimeOnly.FromTimeSpan(timeSpan));

        var sql2 = repository.Update<User>()
            .Set(f => new { SomeTimes = timeSpan })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters2);
        Assert.True(sql2 == "UPDATE `sys_user` SET `SomeTimes`=@p0 WHERE `Id`=1");
        Assert.True(parameters2[0].ParameterName == "@p0");
        Assert.True(parameters2[0].Value.GetType() == typeof(TimeOnly));
        Assert.True((TimeOnly)parameters2[0].Value == TimeOnly.FromTimeSpan(timeSpan));

        repository.BeginTransaction();
        await repository.Update<User>()
            .Set(new { SomeTimes = timeSpan })
            .Where(new { Id = 1 })
            .ExecuteAsync();
        var userInfo = repository.Get<User>(1);
        repository.Commit();
        Assert.True(userInfo.SomeTimes.Value == TimeOnly.FromTimeSpan(timeSpan));
    }
    [Fact]
    public async Task Update_BulkCopy()
    {
        using var repository = dbFactory.Create();
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
                //Disputes = new Dispute
                //{
                //    Id = i + 1,
                //    Content = "无良商家",
                //    Result = "同意退款",
                //    Users = "Buyer2,Seller2",
                //    CreatedAt = DateTime.Now
                //},
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

        Assert.True(count == orders.Count);
    }
    private double CalcAmount(double price, double amount)
    {
        return price * amount - 150;
    }
    private int[] GetProducts()
    {
        return new int[] { 1, 2, 3 };
    }
}
