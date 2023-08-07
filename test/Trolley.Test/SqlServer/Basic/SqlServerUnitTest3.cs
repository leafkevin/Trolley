using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Gender = Gender.Female
        }, t => t.Id == 1);
        var result1 = repository.Get<User>(1);
        Assert.True(result > 0);
        Assert.NotNull(result1);
        Assert.True(result1.Name == result1.Name);
        Assert.True(result1.Name == "leafkevin_1");
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
        var result = repository.Update<User>(f => f.Name, new { Id = 1, Name = "leafkevin11" });
        var result1 = repository.Get<User>(1);
        Assert.True(result > 0);
        Assert.NotNull(result1);
        Assert.True(result1.Name == result1.Name);
        Assert.True(result1.Name == "leafkevin11");
    }
    [Fact]
    public void Update_Fields_Parameters_One()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var result = repository.Update<User>(f => new { Age = 25, f.Name, CompanyId = DBNull.Value }, new { Id = 1, Age = 18, Name = "leafkevin22" });
        var result1 = repository.Get<User>(1);
        Assert.True(result > 0);
        Assert.NotNull(result1);
        Assert.True(result1.Name == result1.Name);
        Assert.True(result1.Name == "leafkevin22");
        Assert.True(result1.Age == 25);
        Assert.True(result1.CompanyId == 0);
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
    public void Update_WithBy_Parameters()
    {
        Initialize();
        using var repository = dbFactory.Create();
        repository.BeginTransaction();
        var result = repository.Update<Order>().WithBy(new
        {
            ProductCount = 10,
            Id = 1
        }).Execute();
        var result1 = repository.Get<Order>(new { Id = 1 });
        repository.Commit();
        if (result > 0)
        {
            Assert.NotNull(result1);
            Assert.True(result1.Id == 1);
            Assert.True(result1.ProductCount == 10);
        }
    }
    [Fact]
    public async void Update_WithBy_Parameters_Multi()
    {
        using var repository = dbFactory.Create();
        var parameters = await repository.From<OrderDetail>()
            .Where(f => new int[] { 1, 2, 3, 4, 5, 6 }.Contains(f.Id))
            .Select(f => new { f.Id, Price = f.Price + 80, Quantity = f.Quantity + 2, Amount = f.Amount + 100 })
            .ToListAsync();
        var sql = repository.Update<OrderDetail>().WithBulkBy(parameters).ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order_detail] SET [Price]=@Price0,[Quantity]=@Quantity0,[Amount]=@Amount0 WHERE [Id]=@kId0;UPDATE [sys_order_detail] SET [Price]=@Price1,[Quantity]=@Quantity1,[Amount]=@Amount1 WHERE [Id]=@kId1;UPDATE [sys_order_detail] SET [Price]=@Price2,[Quantity]=@Quantity2,[Amount]=@Amount2 WHERE [Id]=@kId2;UPDATE [sys_order_detail] SET [Price]=@Price3,[Quantity]=@Quantity3,[Amount]=@Amount3 WHERE [Id]=@kId3;UPDATE [sys_order_detail] SET [Price]=@Price4,[Quantity]=@Quantity4,[Amount]=@Amount4 WHERE [Id]=@kId4;UPDATE [sys_order_detail] SET [Price]=@Price5,[Quantity]=@Quantity5,[Amount]=@Amount5 WHERE [Id]=@kId5");
    }
    [Fact]
    public async void Update_WithBy_Fields_Parameters_Multi()
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
            .WithBulkBy(f => new
            {
                Price = 200,
                f.Quantity,
                UpdatedBy = 2,
                f.Amount,
                ProductId = DBNull.Value
            }, parameters)
            .ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order_detail] SET [Price]=@Price,[Quantity]=@Quantity0,[UpdatedBy]=@UpdatedBy,[Amount]=@Amount0,[ProductId]=NULL WHERE [Id]=@kId0;UPDATE [sys_order_detail] SET [Price]=@Price,[Quantity]=@Quantity1,[UpdatedBy]=@UpdatedBy,[Amount]=@Amount1,[ProductId]=NULL WHERE [Id]=@kId1;UPDATE [sys_order_detail] SET [Price]=@Price,[Quantity]=@Quantity2,[UpdatedBy]=@UpdatedBy,[Amount]=@Amount2,[ProductId]=NULL WHERE [Id]=@kId2;UPDATE [sys_order_detail] SET [Price]=@Price,[Quantity]=@Quantity3,[UpdatedBy]=@UpdatedBy,[Amount]=@Amount3,[ProductId]=NULL WHERE [Id]=@kId3;UPDATE [sys_order_detail] SET [Price]=@Price,[Quantity]=@Quantity4,[UpdatedBy]=@UpdatedBy,[Amount]=@Amount4,[ProductId]=NULL WHERE [Id]=@kId4;UPDATE [sys_order_detail] SET [Price]=@Price,[Quantity]=@Quantity5,[UpdatedBy]=@UpdatedBy,[Amount]=@Amount5,[ProductId]=NULL WHERE [Id]=@kId5");
    }
    [Fact]
    public async void Update_Parameters_WithBy()
    {
        using var repository = dbFactory.Create();
        var orders = await repository.From<Order>()
            .Where(f => new int[] { 1, 2, 3 }.Contains(f.Id))
            .ToListAsync();
        var sql = repository.Update<Order>()
            .WithBulkBy(f => new
            {
                BuyerId = DBNull.Value,
                OrderNo = "ON_" + f.OrderNo,
                f.TotalAmount
            }, orders)
            .ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order] SET [BuyerId]=NULL,[OrderNo]=('ON_'+[OrderNo]),[TotalAmount]=@TotalAmount0 WHERE [Id]=@kId0;UPDATE [sys_order] SET [BuyerId]=NULL,[OrderNo]=('ON_'+[OrderNo]),[TotalAmount]=@TotalAmount1 WHERE [Id]=@kId1;UPDATE [sys_order] SET [BuyerId]=NULL,[OrderNo]=('ON_'+[OrderNo]),[TotalAmount]=@TotalAmount2 WHERE [Id]=@kId2");
    }
    [Fact]
    public void Update_Set_FromQuery_Multi()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .From<User>()
            .SetIf(true, (x, y) => new
            {
                TotalAmount = 200.56,
                OrderNo = x.OrderNo + "-111",
                BuyerSource = y.SourceType
            })
            .SetValue(x => x.Products, new List<int> { 1, 2, 3 })
            .Where((a, b) => a.BuyerId == b.Id && a.BuyerId == 1)
          .ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+'_111'),[BuyerId]=NULL WHERE [sys_order].[BuyerId]=1");
    }
    [Fact]
    public void Update_Set_FromQuery_One()
    {
        Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .Set((a, b) => new
            {
                TotalAmount = a.From<OrderDetail>('b')
                    .Where(f => f.OrderId == b.Id)
                    .Select(t => Sql.Sum(t.Amount))
            })
            .SetValue(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[OrderNo]=@OrderNo,[BuyerId]=NULL WHERE [sys_order].[BuyerId]=1");
    }
    [Fact]
    public void Update_Set_FromQuery_One_Enum()
    {
        using var repository = dbFactory.Create();
        var sql = repository.Update<Company>()
            .Set((a, b) => new
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
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .Set((x, y) => new
            {
                TotalAmount = x.From<OrderDetail>('b')
                .Where(f => f.OrderId == y.Id)
                .Select(t => Sql.Sum(t.Amount))
            })
            .SetValue(x => x.OrderNo, "ON_111")
            .Set(f => new { BuyerId = DBNull.Value })
            .Where(a => a.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(b.[Amount]) FROM [sys_order_detail] b WHERE b.[OrderId]=[sys_order].[Id]),[OrderNo]=@OrderNo,[BuyerId]=NULL WHERE [sys_order].[BuyerId]=1");
    }
    [Fact]
    public void Update_From_One()
    {
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .From<OrderDetail>()
            .SetValue(x => x.TotalAmount, 200.56)
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
        using var repository = dbFactory.Create();
        var sql = repository.Update<Order>()
            .From<OrderDetail>()
            .Set(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
                .Where(f => f.OrderId == y.Id)
                .Select(t => Sql.Sum(t.Amount)))
            .Set((a, b) => new { OrderNo = a.OrderNo + b.ProductId.ToString() })
            .Set((x, y) => new { BuyerId = DBNull.Value })
            .Where((x, y) => x.Id == y.OrderId && x.BuyerId == 1)
            .ToSql(out _);
        Assert.True(sql == "UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(c.[Amount]) FROM [sys_order_detail] c WHERE c.[OrderId]=[sys_order].[Id]),[OrderNo]=([sys_order].[OrderNo]+CAST(b.[ProductId] AS NVARCHAR(MAX))),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1");
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
        using var repository = dbFactory.Create();
        var sql1 = repository.Update<User>()
            .WithBy(new
            {
                Id = 1,
                Gender = Gender.Male
            })
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
            .WithBy(f => new { f.Age, user.Gender }, new { Id = 1, Age = 20 })
            .ToSql(out var parameters3);
        Assert.True(sql3 == "UPDATE [sys_user] SET [Age]=@Age,[Gender]=@Gender WHERE [Id]=@kId");
        Assert.True(parameters3[1].ParameterName == "@Gender");
        Assert.True(parameters3[1].Value.GetType() == typeof(byte));
        Assert.True((byte)parameters3[1].Value == (byte)Gender.Female);

        int age = 20;
        var sql7 = repository.Update<User>()
            .WithBy(f => new { f.Gender, Age = age }, new { Id = 1, Gender = Gender.Male })
            .ToSql(out var parameters7);
        Assert.True(sql7 == "UPDATE [sys_user] SET [Gender]=@Gender,[Age]=@Age WHERE [Id]=@kId");
        Assert.True(parameters7[0].ParameterName == "@Gender");
        Assert.True(parameters7[0].Value.GetType() == typeof(byte));
        Assert.True((byte)parameters7[0].Value == (byte)Gender.Male);

        var sql4 = repository.Update<Company>()
             .WithBy(new
             {
                 Id = 1,
                 Nature = CompanyNature.Internet
             })
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
            .WithBy(f => new { f.Nature }, new { Id = 1, Nature = CompanyNature.Internet })
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
            .WithBulkBy(f => new { f.Name, company.Nature }, new[] { new { Id = 1, Name = "google" }, new { Id = 2, Name = "facebook" } })
            .ToSql(out var parameters9);
        Assert.True(sql9 == "UPDATE [sys_company] SET [Name]=@Name0,[Nature]=@Nature WHERE [Id]=@kId0;UPDATE [sys_company] SET [Name]=@Name1,[Nature]=@Nature WHERE [Id]=@kId1");
        Assert.True(parameters9[parameters9.Count - 1].ParameterName == "@Nature");
        Assert.True(parameters9[parameters9.Count - 1].Value.GetType() == typeof(string));
        Assert.True((string)parameters9[parameters9.Count - 1].Value == CompanyNature.Internet.ToString());

        CompanyNature? nature = CompanyNature.Production;
        var sql10 = repository.Update<Company>()
            .WithBulkBy(f => new { f.Nature, company.Name }, new[] { new { Id = 1, company.Nature }, new { Id = 2, Nature = nature } })
            .ToSql(out var parameters10);
        Assert.True(sql10 == "UPDATE [sys_company] SET [Nature]=@Nature0,[Name]=@Name WHERE [Id]=@kId0;UPDATE [sys_company] SET [Nature]=@Nature1,[Name]=@Name WHERE [Id]=@kId1");
        Assert.True(parameters10[0].ParameterName == "@Nature0");
        Assert.True(parameters10[0].Value.GetType() == typeof(string));
        Assert.True((string)parameters10[0].Value == company.Nature.ToString());
        Assert.True(parameters10[2].ParameterName == "@Nature1");
        Assert.True(parameters10[2].Value.GetType() == typeof(string));
        Assert.True((string)parameters10[2].Value == CompanyNature.Production.ToString());
        Assert.True(parameters10[parameters10.Count - 1].ParameterName == "@Name");
        Assert.True(parameters10[parameters10.Count - 1].Value.GetType() == typeof(string));
        Assert.True((string)parameters10[parameters10.Count - 1].Value == company.Name);
    }
    [Fact]
    public async void Update_TimeSpan_Fields()
    {
        using var repository = dbFactory.Create();
        var sql1 = repository.Update<User>()
            .WithBy(new
            {
                Id = 1,
                SomeTimes = TimeSpan.FromMinutes(1455)
            })
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
            .WithBy(f => new { f.SomeTimes, Age = age }, new { Id = 1, SomeTimes = TimeSpan.FromMinutes(1455) })
            .ToSql(out var parameters7);
        Assert.True(sql7 == "UPDATE [sys_user] SET [SomeTimes]=@SomeTimes,[Age]=@Age WHERE [Id]=@kId");
        Assert.True(parameters7[0].ParameterName == "@SomeTimes");
        Assert.True(parameters1[0].Value.GetType() == typeof(TimeSpan));
        Assert.True((TimeSpan)parameters1[0].Value == TimeSpan.FromMinutes(1455));

        //TODO:SQL SERVER不支持超过1天的时间类型
        repository.BeginTransaction();
        await repository.Update<User>()
            .WithBy(new
            {
                Id = 1,
                SomeTimes = TimeSpan.FromMinutes(55)
            })
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
