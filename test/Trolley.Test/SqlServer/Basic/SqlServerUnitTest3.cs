using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Trolley.SqlServer;
using Xunit;

namespace Trolley.Test.SqlServer;

public class SqlServerUnitTest3 : UnitTestBase
{
    public SqlServerUnitTest3()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register("fengling", true, f =>
            {
                var connectionString = "Server=127.0.0.1;Database=fengling;Uid=sa;password=SQLserverSA123456;TrustServerCertificate=true";
                f.Add<SqlServerProvider>(connectionString, true);
            })
            .AddTypeHandler<JsonTypeHandler>()
            .Configure<SqlServerProvider, SqlServerModelConfiguration>();
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
    public void Update_Fields_Where()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.Update<User>(f => new { Name = f.Name + "_1", Gender = Gender.Female }, t => t.Id == 1);
        var result1 = repository.Get<User>(1);
        Assert.True(result > 0);
        Assert.NotNull(result1);
        Assert.True(result1.Name == result1.Name);
        Assert.True(result1.Name == "leafkevin_1");
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
        result = repository.Update<User>(f => f.Name, new { Id = 1, Name = "leafkevin22" });
        var result2 = repository.Get<User>(1);
        Assert.True(result > 0);
        Assert.NotNull(result2);
        Assert.True(result2.Name == "leafkevin22");
    }
    [Fact]
    public void Update_Fields_Parameters_One()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.Update<User>(f => new { Age = 24, f.Name, CompanyId = DBNull.Value }, new { Id = 1, Age = 18, Name = "leafkevin11" });
        var result1 = repository.Get<User>(1);
        Assert.True(result > 0);
        Assert.NotNull(result1);
        Assert.True(result1.Name == "leafkevin11");
        Assert.True(result1.Age == 24);
        Assert.True(result1.CompanyId == 0);
        result = repository.Update<User>()
            .SetWith(f => new
            {
                Age = 25,
                f.Name,
                CompanyId = DBNull.Value
            }, new
            {
                Id = 1,
                Age = 18,
                Name = "leafkevin22"
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
    public async void Update_Fields_Parameters_Multi()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var orderDetails = await repository.From<OrderDetail>().ToListAsync();
        var parameters = orderDetails.Select(f => new { f.Id, Price = f.Price + 80, Quantity = f.Quantity + 1, Amount = f.Amount + 50 }).ToList();
        repository.BeginTransaction();
        var result = repository.Update<OrderDetail>(f => new { Price = 200, f.Quantity, UpdatedBy = 2, f.Amount, ProductId = DBNull.Value }, parameters);
        var updatedDetails = await repository.QueryAsync<OrderDetail>();
        repository.Commit();
        Assert.True(result == parameters.Count);
        int index = 0;
        updatedDetails.ForEach(f =>
        {
            Assert.True(f.Price == 200);
            Assert.True(f.Quantity == parameters[index].Quantity);
            Assert.True(f.Amount == parameters[index].Amount);
            Assert.True(f.UpdatedBy == 2);
            Assert.True(f.ProductId == 0);
            index++;
        });
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
        var result1 = repository.Get<Order>(new { Id = 1 });
        repository.Commit();
        if (result > 0)
        {
            Assert.NotNull(result1);
            Assert.True(result1.Id == 1);
            Assert.True(result1.ProductCount == 10);
        }
        repository.BeginTransaction();
        result = repository.Update<Order>()
            .Set(new { ProductCount = 11 })
            .Where(new { Id = 1 })
            .Execute();
        var result2 = repository.Get<Order>(new { Id = 1 });
        repository.Commit();
        if (result > 0)
        {
            Assert.NotNull(result2);
            Assert.True(result2.Id == 1);
            Assert.True(result2.ProductCount == 11);
        }
    }
    [Fact]
    public async void Update_MultiParameters()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        var parameters = await repository.From<OrderDetail>()
            .Where(f => new int[] { 1, 2, 3, 4, 5, 6 }.Contains(f.Id))
            .Select(f => new { f.Id, Price = f.Price + 80, Quantity = f.Quantity + 2, Amount = f.Amount + 100 })
            .ToListAsync();
        var count = repository.Update<OrderDetail>(parameters);
        var orderDetails = await repository.QueryAsync<OrderDetail>(f => new int[] { 1, 2, 3, 4, 5, 6 }.Contains(f.Id));
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
    public async void Update_WithBulk_Fields_Parameters_Multi()
    {
        using var repository = dbFactory.Create();
        var parameters = await repository.From<OrderDetail>()
            .Where(f => new int[] { 1, 2, 3, 4, 5, 6 }.Contains(f.Id))
            .Select(f => new
            {
                f.Id,
                Price = f.Price + 80,
                Quantity = f.Quantity + 2,
                Amount = f.Amount + 100
            })
            .ToListAsync();
        var sql = repository.Update<OrderDetail>()
            .WithBulk(f => new
            {
                Price = 200,
                f.Quantity,
                UpdatedBy = 2,
                f.Amount,
                ProductId = DBNull.Value
            }, parameters)
            .ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order_detail] SET [Price]=@Price,[UpdatedBy]=@UpdatedBy,[ProductId]=NULL,[Quantity]=@Quantity0,[Amount]=@Amount0 WHERE [Id]=@kId0;UPDATE [sys_order_detail] SET [Price]=@Price,[UpdatedBy]=@UpdatedBy,[ProductId]=NULL,[Quantity]=@Quantity1,[Amount]=@Amount1 WHERE [Id]=@kId1;UPDATE [sys_order_detail] SET [Price]=@Price,[UpdatedBy]=@UpdatedBy,[ProductId]=NULL,[Quantity]=@Quantity2,[Amount]=@Amount2 WHERE [Id]=@kId2;UPDATE [sys_order_detail] SET [Price]=@Price,[UpdatedBy]=@UpdatedBy,[ProductId]=NULL,[Quantity]=@Quantity3,[Amount]=@Amount3 WHERE [Id]=@kId3;UPDATE [sys_order_detail] SET [Price]=@Price,[UpdatedBy]=@UpdatedBy,[ProductId]=NULL,[Quantity]=@Quantity4,[Amount]=@Amount4 WHERE [Id]=@kId4;UPDATE [sys_order_detail] SET [Price]=@Price,[UpdatedBy]=@UpdatedBy,[ProductId]=NULL,[Quantity]=@Quantity5,[Amount]=@Amount5 WHERE [Id]=@kId5");
    }
    [Fact]
    public async void Update_Parameters_WithBulk()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var orders = await repository.From<Order>()
            .Where(f => new int[] { 1, 2, 3 }.Contains(f.Id))
            .ToListAsync();
        var sql = repository.Update<Order>()
            .WithBulk(f => new
            {
                BuyerId = DBNull.Value,
                OrderNo = "ON_" + f.OrderNo,
                f.TotalAmount
            }, orders)
            .ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order] SET [BuyerId]=NULL,[OrderNo]=('ON_'+[OrderNo]),[TotalAmount]=@TotalAmount0 WHERE [Id]=@kId0;UPDATE [sys_order] SET [BuyerId]=NULL,[OrderNo]=('ON_'+[OrderNo]),[TotalAmount]=@TotalAmount1 WHERE [Id]=@kId1;UPDATE [sys_order] SET [BuyerId]=NULL,[OrderNo]=('ON_'+[OrderNo]),[TotalAmount]=@TotalAmount2 WHERE [Id]=@kId2");
    }
    [Fact]
    public void Update_SetWith()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        var updateObj = repository.Get<Order>(1);
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
        var result = repository.Update<Order>(f => new
        {
            BuyerId = DBNull.Value,
            OrderNo = "ON_" + f.OrderNo,
            TotalAmount = f.TotalAmount + increasedAmount,
            Products = this.GetProducts(),
            f.Disputes,
            f.UpdatedAt
        }, updateObj);

        var updatedOrder = repository.Get<Order>(1);
        repository.Commit();
        if (result > 0)
        {
            Assert.NotEmpty(updatedOrder.Products);
            Assert.True(updatedOrder.Products.Count == 3);
            Assert.True(updatedOrder.Products[0] == 1);
            Assert.True(updatedOrder.Products[1] == 2);
            Assert.True(updatedOrder.Products[2] == 3);
            Assert.True(updatedOrder.TotalAmount == updateObj.TotalAmount + increasedAmount);
            Assert.True(JsonSerializer.Serialize(updatedOrder.Disputes) == JsonSerializer.Serialize(updateObj.Disputes));
            //Assert.True(updatedOrder.UpdatedAt == updateObj.UpdatedAt);
        }
        var result1 = repository.Update<Order>()
            .Set(new { ProductCount = 10, BuyerSource = DBNull.Value })
            .Where(new { Id = 1 })
            .Execute();
        updatedOrder = repository.Get<Order>(new { Id = 1 });
        repository.Commit();
        if (result > 0)
        {
            Assert.NotNull(updatedOrder);
            Assert.True(updatedOrder.Id == 1);
            Assert.True(updatedOrder.ProductCount == 10);
            Assert.True(updatedOrder.BuyerSource.HasValue == false);
        }
    }
    [Fact]
    public void Update_SetWith_MethodCall()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        var updateObj = repository.Get<Order>(1);
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
        var result = repository.Update<Order>()
            .SetWith(f => new
            {
                TotalAmount = this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3).Deferred(),
                Products = this.GetProducts(),
                f.Disputes,
                f.UpdatedAt
            }, updateObj)
            .Where(new { updateObj.Id })
            .Execute();

        var updatedOrder = repository.Get<Order>(1);
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
        var sql = repository.Update<Order>()
            .SetWith(f => new
            {
                TotalAmount = this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3),
                Products = this.GetProducts(),
                f.Disputes,
                f.UpdatedAt
            }, updateObj)
            .Where(new { updateObj.Id })
            .ToSql(out var dbParameters);
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[Products]=@Products,[Disputes]=@Disputes,[UpdatedAt]=@UpdatedAt WHERE [Id]=@kId");
        Assert.True(dbParameters[0].ParameterName == "@TotalAmount");
        Assert.True((double)dbParameters[0].Value == this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3));
        Assert.True(dbParameters[1].ParameterName == "@Products");
        Assert.True((string)dbParameters[1].Value == JsonSerializer.Serialize(this.GetProducts()));
        Assert.True(dbParameters[2].ParameterName == "@Disputes");
        Assert.True((string)dbParameters[2].Value == JsonSerializer.Serialize(updateObj.Disputes));
        Assert.True(dbParameters[3].ParameterName == "@UpdatedAt");
        Assert.True((DateTime)dbParameters[3].Value == updateObj.UpdatedAt);
    }
    [Fact]
    public void Update_Set_FromQuery_Multi()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .From<User>()
            .Set(true, (x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .Set(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == b.Id && a.BuyerId == 1)
          .ToSql(out var dbParameters);
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1");
        Assert.NotNull(dbParameters);
        Assert.True(dbParameters[0].ParameterName == "@TotalAmount");
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
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL WHERE [sys_order].[BuyerId]=1");
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
            .Set(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[OrderNo]=@OrderNo,[BuyerId]=NULL WHERE [sys_order].[BuyerId]=1");
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
        Assert.True(sql == "UPDATE [sys_company] SET [Nature]=(SELECT b.[Nature] FROM [sys_company] b WHERE b.[Name] LIKE '%Internet%') WHERE [sys_company].[Nature]='Production'");
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
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[OrderNo]=@OrderNo,[BuyerId]=NULL WHERE [sys_order].[BuyerId]=1");
    }
    [Fact]
    public void Update_From_One()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .From<OrderDetail>()
            .Set(x => x.TotalAmount, 200.56)
            .Set((a, b) => new
            {
                OrderNo = a.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where((x, y) => x.Id == y.OrderId && x.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1");
    }
    [Fact]
    public void Update_From_Multi()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .From<OrderDetail>()
            .Set((x, y) => new
            {
                TotalAmount = y.Amount,
                OrderNo = x.OrderNo + "_111",
                BuyerId = DBNull.Value
            })
            .Where((x, y) => x.Id == y.OrderId && x.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=b.[Amount],[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1");
    }
    [Fact]
    public void Update_From_Fields()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .From<OrderDetail>()
            .SetFrom((x, y, z) => new
            {
                TotalAmount = x.From<OrderDetail>('c')
                    .Where(f => f.OrderId == y.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .Set((a, b) => new { OrderNo = a.OrderNo + b.ProductId.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((x, y) => x.Id == y.OrderId && x.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(c.[Amount]) FROM [sys_order_detail] c WHERE c.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+CAST(b.[ProductId] AS NVARCHAR(MAX))),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1");
        var sql1 = repository.Update<Order>()
            .From<OrderDetail>()
            .SetFrom(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
                .Where(f => f.OrderId == y.Id)
                .Select(t => Sql.Sum(t.Amount)))
            .Set((a, b) => new { OrderNo = a.OrderNo + b.ProductId.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((x, y) => x.Id == y.OrderId && x.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql1 == "UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(c.[Amount]) FROM [sys_order_detail] c WHERE c.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+CAST(b.[ProductId] AS NVARCHAR(MAX))),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1");
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
        Assert.True(sql == "UPDATE [sys_order] SET [BuyerId]=NULL,[Seller]=NULL WHERE [OrderNo] IS NULL");
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
            .From<User>()
            .Set((x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .Set(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == b.Id && a.BuyerId == 1)
            .ToSql(out var parameters);
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=([sys_order].[OrderNo]+'-111'),[BuyerSource]=b.[SourceType],[Products]=@Products FROM [sys_user] b WHERE [sys_order].[BuyerId]=b.[Id] AND [sys_order].[BuyerId]=1");
        Assert.True(parameters[0].ParameterName == "@TotalAmount");
        Assert.True(parameters[0].Value.GetType() == typeof(double));
        Assert.True((double)parameters[0].Value == 200.56);
        Assert.True(parameters[1].ParameterName == "@Products");
        Assert.True(parameters[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters[1].Value == JsonSerializer.Serialize(new List<int> { 1, 2, 3 }));

        var sql1 = repository.Update<User>()
            .Set(new { Gender = Gender.Male })
            .Where(new { Id = 1 })
            .ToSql(out var parameters1);
        Assert.True(sql1 == "UPDATE [sys_user] SET [Gender]=@Gender WHERE [Id]=@kId");
        Assert.True(parameters1[0].ParameterName == "@Gender");
        Assert.True(parameters1[0].Value.GetType() == typeof(byte));
        Assert.True((byte)parameters1[0].Value == (byte)Gender.Male);

        var sql2 = repository.Update<User>()
            .Set(f => new { Gender = Gender.Male })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters2);
        Assert.True(sql2 == "UPDATE [sys_user] SET [Gender]=@Gender WHERE [Id]=1");
        Assert.True(parameters2[0].ParameterName == "@Gender");
        Assert.True(parameters2[0].Value.GetType() == typeof(byte));
        Assert.True((byte)parameters2[0].Value == (byte)Gender.Male);

        var user = new User { Gender = Gender.Female };
        var sql3 = repository.Update<User>()
            .SetWith(f => new { f.Age, user.Gender }, new { Id = 1, Age = 20 })
            .Where(new { Id = 1 })
            .ToSql(out var parameters3);
        Assert.True(sql3 == "UPDATE [sys_user] SET [Age]=@Age,[Gender]=@Gender WHERE [Id]=@kId");
        Assert.True(parameters3[1].ParameterName == "@Gender");
        Assert.True(parameters3[1].Value.GetType() == typeof(byte));
        Assert.True((byte)parameters3[1].Value == (byte)Gender.Female);

        int age = 20;
        var sql7 = repository.Update<User>()
            .SetWith(f => new { f.Gender, Age = age }, new { Id = 1, Gender = Gender.Male })
            .Where(new { Id = 1 })
            .ToSql(out var parameters7);
        Assert.True(sql7 == "UPDATE [sys_user] SET [Gender]=@Gender,[Age]=@Age WHERE [Id]=@kId");
        Assert.True(parameters7[0].ParameterName == "@Gender");
        Assert.True(parameters7[0].Value.GetType() == typeof(byte));
        Assert.True((byte)parameters7[0].Value == (byte)Gender.Male);

        var sql4 = repository.Update<Company>()
             .Set(new { Nature = CompanyNature.Internet })
             .Where(new { Id = 1 })
             .ToSql(out var parameters4);
        Assert.True(sql4 == "UPDATE [sys_company] SET [Nature]=@Nature WHERE [Id]=@kId");
        Assert.True(parameters4[0].ParameterName == "@Nature");
        Assert.True(parameters4[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters4[0].Value == CompanyNature.Internet.ToString());

        var sql5 = repository.Update<Company>()
            .Set(f => new { Nature = CompanyNature.Internet })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters5);
        Assert.True(sql5 == "UPDATE [sys_company] SET [Nature]=@Nature WHERE [Id]=1");
        Assert.True(parameters5[0].ParameterName == "@Nature");
        Assert.True(parameters5[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters5[0].Value == CompanyNature.Internet.ToString());

        var sql6 = repository.Update<Company>()
            .SetWith(f => new { f.Nature }, new { Id = 1, Nature = CompanyNature.Internet })
            .Where(new { Id = 1 })
            .ToSql(out var parameters6);
        Assert.True(sql6 == "UPDATE [sys_company] SET [Nature]=@Nature WHERE [Id]=@kId");
        Assert.True(parameters6[0].ParameterName == "@Nature");
        Assert.True(parameters6[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters6[0].Value == CompanyNature.Internet.ToString());

        var company = new Company { Name = "facebook", Nature = CompanyNature.Internet };
        var sql8 = repository.Update<Company>()
            .Set(f => new { Name = f.Name + "_New", company.Nature })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters8);
        Assert.True(sql8 == "UPDATE [sys_company] SET [Name]=([Name]+'_New'),[Nature]=@Nature WHERE [Id]=1");
        Assert.True(parameters8[0].ParameterName == "@Nature");
        Assert.True(parameters8[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters8[0].Value == CompanyNature.Internet.ToString());

        //批量表达式部分栏位更新
        var sql9 = repository.Update<Company>()
            .WithBulk(f => new { f.Name, company.Nature }, new[] { new { Id = 1, Name = "google" }, new { Id = 2, Name = "facebook" } })
            .ToSql(out var parameters9);
        Assert.True(sql9 == "UPDATE [sys_company] SET [Nature]=@Nature,[Name]=@Name0 WHERE [Id]=@kId0;UPDATE [sys_company] SET [Nature]=@Nature,[Name]=@Name1 WHERE [Id]=@kId1");
        Assert.True(parameters9[0].ParameterName == "@Nature");
        Assert.True(parameters9[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters9[0].Value == CompanyNature.Internet.ToString());

        CompanyNature? nature = CompanyNature.Production;
        var sql10 = repository.Update<Company>()
            .WithBulk(f => new { f.Nature, company.Name }, new[] { new { Id = 1, company.Nature }, new { Id = 2, Nature = nature } })
            .ToSql(out var parameters10);
        Assert.True(sql10 == "UPDATE [sys_company] SET [Name]=@Name,[Nature]=@Nature0 WHERE [Id]=@kId0;UPDATE [sys_company] SET [Name]=@Name,[Nature]=@Nature1 WHERE [Id]=@kId1");
        Assert.True(parameters10[1].ParameterName == "@Nature0");
        Assert.True(parameters10[1].Value.GetType() == typeof(string));
        Assert.True((string)parameters10[1].Value == company.Nature.ToString());
        Assert.True(parameters10[3].ParameterName == "@Nature1");
        Assert.True(parameters10[3].Value.GetType() == typeof(string));
        Assert.True((string)parameters10[3].Value == CompanyNature.Production.ToString());
        Assert.True(parameters10[0].ParameterName == "@Name");
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
        Assert.True(sql1 == "UPDATE [sys_user] SET [SomeTimes]=@SomeTimes WHERE [Id]=@kId");
        Assert.True(parameters1[0].ParameterName == "@SomeTimes");
        Assert.True(parameters1[0].Value.GetType() == typeof(TimeSpan));
        Assert.True((TimeSpan)parameters1[0].Value == TimeSpan.FromMinutes(1455));

        var sql2 = repository.Update<User>()
            .Set(f => new { SomeTimes = TimeSpan.FromMinutes(1455) })
            .Where(f => f.Id == 1)
            .ToSql(out var parameters2);
        Assert.True(sql2 == "UPDATE [sys_user] SET [SomeTimes]=@SomeTimes WHERE [Id]=1");
        Assert.True(parameters2[0].ParameterName == "@SomeTimes");
        Assert.True(parameters1[0].Value.GetType() == typeof(TimeSpan));
        Assert.True((TimeSpan)parameters1[0].Value == TimeSpan.FromMinutes(1455));

        int age = 20;
        var sql7 = repository.Update<User>()
            .SetWith(f => new { f.SomeTimes, Age = age }, new { Id = 1, SomeTimes = TimeSpan.FromMinutes(1455) })
            .Where(new { Id = 1 })
            .ToSql(out var parameters7);
        Assert.True(sql7 == "UPDATE [sys_user] SET [SomeTimes]=@SomeTimes,[Age]=@Age WHERE [Id]=@kId");
        Assert.True(parameters7[0].ParameterName == "@SomeTimes");
        Assert.True(parameters1[0].Value.GetType() == typeof(TimeSpan));
        Assert.True((TimeSpan)parameters1[0].Value == TimeSpan.FromMinutes(1455));

        //TODO:SQL SERVER不支持超过1天的时间类型
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
