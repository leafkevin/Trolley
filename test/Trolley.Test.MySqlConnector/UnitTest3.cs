using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Trolley.MySqlConnector;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.MySqlConnector;

public class UnitTest3 : UnitTestBase
{
    protected readonly ITestOutputHelper logger;
    public UnitTest3(ITestOutputHelper logger)
    {
        this.logger = logger;
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register<MySqlProvider>("fengling", true, f =>
            {
                f.Add("Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;", true);
            })
            .AddTypeHandler<JsonTypeHandler>()
            .Configure<MySqlProvider, ModelConfiguration>();
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
    public async void Update_AnonymousObjects()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var parameters = await repository.From<OrderDetail>()
            .GroupBy(f => f.OrderId)
            .Select((x, f) => new
            {
                Id = x.Grouping,
                TotalAmount = x.Sum(f.Amount) + 50
            })
            .ToListAsync();
        var count = await repository.UpdateAsync<Order>(parameters);
        var ids = parameters.Select(f => f.Id).ToList();
        var orders = await repository.QueryAsync<Order>(f => ids.Contains(f.Id));
        Assert.True(count > 0);
        Assert.True(parameters.Count == orders.Count);
        for (int i = 0; i < orders.Count; i++)
        {
            Assert.True(orders[i].TotalAmount == parameters[i].TotalAmount);
        }
    }
    [Fact]
    public async void Update_SetBulk()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var parameters = await repository.From<OrderDetail>()
           .GroupBy(f => f.OrderId)
           .Select((x, f) => new
           {
               Id = x.Grouping,
               TotalAmount = x.Sum(f.Amount) + 50
           })
           .ToListAsync();
        repository.BeginTransaction();
        var sql = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .ToSql(out var dbParameters);
        Assert.True(sql == "");
        Assert.True(dbParameters.Count == parameters.Count * 2);
        int i = 0;
        while (i < parameters.Count)
        {
            Assert.True(dbParameters[i].ParameterName == "TotalAmount");
            Assert.True((double)dbParameters[i].Value == parameters[i].TotalAmount);
            Assert.True(dbParameters[i + 1].ParameterName == "Id");
            i = i + 2;
        }
    }
    [Fact]
    public async void Update_SetBulk_OnlyFields()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var orderDetails = await repository.From<OrderDetail>().ToListAsync();
        var parameters = orderDetails.Select(f => new
        {
            f.Id,
            Price = f.Price + 80,
            Quantity = f.Quantity + 1,
            Amount = f.Amount + 50,
            UpdatedAt = f.UpdatedAt.AddDays(1)
        })
        .ToList();
        var result = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .OnlyFields(f => new
            {
                f.Price,
                f.Quantity
            })
            .Execute();
        var updatedDetails = await repository.QueryAsync<OrderDetail>();
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
    public async void Update_SetBulk_IgnoreFields()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var orderDetails = await repository.From<OrderDetail>().ToListAsync();
        var parameters = orderDetails.Select(f => new
        {
            f.Id,
            Price = f.Price + 80,
            Quantity = f.Quantity + 1,
            Amount = f.Amount + 50,
            UpdatedAt = f.UpdatedAt.AddDays(1)
        })
        .ToList();
        var result = repository.Update<OrderDetail>()
            .SetBulk(parameters)
            .IgnoreFields(f => f.Price)
            .Execute();
        var updatedDetails = await repository.QueryAsync<OrderDetail>();
        repository.Commit();
        Assert.True(result == parameters.Count);
        for (int i = 0; i < parameters.Count; i++)
        {
            Assert.True(updatedDetails[i].Price != parameters[i].Price);
            Assert.True(updatedDetails[i].Quantity == parameters[i].Quantity);
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
        Assert.True(sql == "");
        Assert.True(dbParameters.Count == 4);
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
        Assert.True(sql == "");
        Assert.True(result.Name == "leafkevin22");
        Assert.True(result.Age == 25);
        Assert.True(dbParameters[2].Value == DBNull.Value);
        Assert.True(result.CompanyId == 0);
    }
    [Fact]
    public void Update_Set_AnonymousObject_Where_OnlyFields()
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
            .OnlyFields(f => f.Name)
            .Where(f => f.Id == 1)
            .ToSql(out var dbParameters);
        Assert.True(sql == "");
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
            .OnlyFields(f => f.Name)
            .Where(f => f.Id == 1)
            .Execute();
        var result = repository.Get<User>(1);
        Assert.True(result.Name == "leafkevin22");
        Assert.True(result.Age != 25);
        Assert.True(result.CompanyId != 0);
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
        Assert.True(sql == "");
        Assert.True(dbParameters.Count == 3);
        Assert.True((int)dbParameters[1].Value == 25);
        Assert.True(dbParameters[2].Value == DBNull.Value);

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
    public async void Update_SetFrom()
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
            .ToSql(out var dbParameters);
        Assert.True(sql == "UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1");
        Assert.True(dbParameters[0].ParameterName == "@p0");
        Assert.True((double)dbParameters[0].Value == 200.56);
        Assert.True(dbParameters[1].ParameterName == "@Products");

        await repository.Update<Order>()
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
        var orderAmounts = repository.From<OrderDetail>()
            .GroupBy(x => x.OrderId)
            .Select((f, a) => new
            {
                OrderId = f.Grouping,
                TotalAmount = f.Sum(a.Amount)
            })
            .ToList();
        for (int i = 0; i < orderAmounts.Count; i++)
        {
            Assert.True(dbParameters[0].ParameterName == "@p0");
            Assert.True((double)dbParameters[0].Value == 200.56);
            Assert.True(dbParameters[1].ParameterName == "@Products");
        }

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
    }
    [Fact]
    public void Update_Set_FromQuery_One()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .SetFrom((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .SetFrom(f => f.TotalAmount, (x, y) =>
                x.From<OrderDetail>('a')
                .Where(t => t.OrderId == y.Id)
                .Select(f => Sql.Sum(f.Amount)))
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`BuyerId`=1");
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
                    .Where(f => f.Name.Contains("Internet"))
                    .Select(t => t.Nature)
            })
            .Where(f => f.Nature == CompanyNature.Production)
            .ToSql(out _);
        Assert.True(sql == "UPDATE `sys_company` a SET a.`Nature`=(SELECT b.`Nature` FROM `sys_company` b WHERE b.`Name` LIKE '%Internet%') WHERE a.`Nature`='Production'");
    }
    [Fact]
    public void Update_Set_FromQuery_Fields()
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
    }
    [Fact]
    public void Update_InnerJoin_Multi()
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
    }
    [Fact]
    public void Update_InnerJoin_Fields()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .SetFrom((x, y, z) => new
            {
                TotalAmount = x.From<OrderDetail>('c')
                    .Where(f => f.OrderId == y.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .Set((a, b) => new { OrderNo = a.OrderNo + b.ProductId.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((a, b) => a.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,CAST(b.`ProductId` AS CHAR)),a.`BuyerId`=NULL WHERE a.`BuyerId`=1");

        var sql1 = repository.Update<Order>()
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .SetFrom(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
                .Where(f => f.OrderId == y.Id)
                .Select(t => Sql.Sum(t.Amount)))
            .Set((a, b) => new { OrderNo = a.OrderNo + b.ProductId.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((x, y) => x.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql1 == "UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,CAST(b.`ProductId` AS CHAR)),a.`BuyerId`=NULL WHERE a.`BuyerId`=1");
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
        var parameter = repository.Get<Order>(1);
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
            .Where(x => x.Id == 1)
            .Execute();
        var order = repository.Get<Order>(1);
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
        parameter = repository.Get<Order>(1);
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
          .Where(x => x.Id == 1)
          .Execute();
        order = repository.Get<Order>(1);
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
            .Where(x => x.Id == 1)
            .Execute();
        var order = repository.Get<Order>(1);
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
            .Where(x => x.Id == 1)
            .Execute();
        repository.Update<Order>()
            .Set(f => new
            {
                UpdatedAt = DateTime.Now
            })
            .Where(x => x.Id == 2)
            .Execute();
        var order = repository.Get<Order>(1);
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
        Assert.True((string)parameters[1].Value == JsonSerializer.Serialize(new List<int> { 1, 2, 3 }));

        var sql1 = repository.Update<User>()
            .Set(new { Gender = Gender.Male })
            .Where(new { Id = 1 })
            .ToSql(out var parameters1);
        Assert.True(sql1 == "UPDATE `sys_user` SET `Gender`=@Gender WHERE `Id`=@kId");
        Assert.True(parameters1[0].ParameterName == "@Gender");
        Assert.True(parameters1[0].Value.GetType() == typeof(sbyte));
        Assert.True((sbyte)parameters1[0].Value == (sbyte)Gender.Male);

        var sql2 = repository.Update<User>()
            .Set(f => new { Gender = Gender.Male })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters2);
        Assert.True(sql2 == "UPDATE `sys_user` SET `Gender`=@p0 WHERE `Id`=1");
        Assert.True(parameters2[0].ParameterName == "@p0");
        Assert.True(parameters2[0].Value.GetType() == typeof(sbyte));
        Assert.True((sbyte)parameters2[0].Value == (sbyte)Gender.Male);

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
            .Set(f => f.Nature, company.Nature)
            .OnlyFields(f => new { f.Name, f.Nature })
            .ToSql(out var parameters9);
        Assert.True(sql9 == "UPDATE `sys_company` SET `Nature`=@p0,`Name`=@Name0 WHERE `Id`=@kId0;UPDATE `sys_company` SET `Nature`=@p0,`Name`=@Name1 WHERE `Id`=@kId1");
        Assert.True(parameters9[0].ParameterName == "@p0");
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
    public async void Update_TimeSpan_Fields()
    {
        using var repository = dbFactory.Create();
        var sql1 = repository.Update<User>()
            .Set(new { SomeTimes = TimeSpan.FromMinutes(1455) })
            .Where(new { Id = 1 })
            .ToSql(out var parameters1);
        Assert.True(sql1 == "UPDATE `sys_user` SET `SomeTimes`=@SomeTimes WHERE `Id`=@kId");
        Assert.True(parameters1[0].ParameterName == "@SomeTimes");
        Assert.True(parameters1[0].Value.GetType() == typeof(TimeSpan));
        Assert.True((TimeSpan)parameters1[0].Value == TimeSpan.FromMinutes(1455));

        var sql2 = repository.Update<User>()
            .Set(f => new { SomeTimes = TimeSpan.FromMinutes(1455) })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters2);
        Assert.True(sql2 == "UPDATE `sys_user` SET `SomeTimes`=@p0 WHERE `Id`=1");
        Assert.True(parameters2[0].ParameterName == "@p0");
        Assert.True(parameters1[0].Value.GetType() == typeof(TimeSpan));
        Assert.True((TimeSpan)parameters1[0].Value == TimeSpan.FromMinutes(1455));

        int age = 20;
        var sql7 = repository.Update<User>()
            .Set(new { Id = 1, Age = age, SomeTimes = TimeSpan.FromMinutes(1455) })
            .Where(new { Id = 1 })
            .ToSql(out var parameters7);
        Assert.True(sql7 == "UPDATE `sys_user` SET `SomeTimes`=@SomeTimes,`Age`=@p1 WHERE `Id`=@kId");
        Assert.True(parameters7[0].ParameterName == "@SomeTimes");
        Assert.True(parameters1[0].Value.GetType() == typeof(TimeSpan));
        Assert.True((TimeSpan)parameters1[0].Value == TimeSpan.FromMinutes(1455));

        repository.BeginTransaction();
        await repository.Update<User>()
            .Set(new { SomeTimes = TimeSpan.FromMinutes(55) })
            .Where(new { Id = 1 })
            .ExecuteAsync();
        var userInfo = repository.Get<User>(1);
        repository.Commit();
        Assert.True(userInfo.SomeTimes.Value == TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(55)));
    }
    [Fact]
    public void Update_Set_MethodCall()
    {
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        var parameter = repository.Get<Order>(1);
        parameter.TotalAmount += 50;
        var result = repository.Update<Order>()
            .Set(f => new
            {
                TotalAmount = this.CalcAmount(parameter.TotalAmount, 3),
                Products = this.GetProducts(),
                OrderNo = string.Concat("Order", "111").Substring(0, 7) + "_123"
            })
            .Where(x => x.Id == 1)
            .Execute();
        var order = repository.Get<Order>(1);
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
