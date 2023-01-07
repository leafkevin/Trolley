using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Trolley;

namespace ConsoleAppTest;

enum Sex
{
    Male,
    Female
}
class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOrmProvider, MySqlProvider>();
        services.AddSingleton(f =>
        {
            var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
            var ormProvider = f.GetService<IOrmProvider>();
            var builder = new OrmDbFactoryBuilder();
            builder.Register("fengling", true, f => f.Add(connectionString, ormProvider, true))
                .Configure(f => new ModelConfiguration().OnModelCreating(f));
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        var dbFactory = serviceProvider.GetService<IOrmDbFactory>();
        var ormProvider = serviceProvider.GetService<IOrmProvider>();
        var repository = dbFactory.Create();

        //var userQuery = repository
        //   .From<Order, User>()
        //   .InnerJoin((x, y) => x.BuyerId == y.Id)
        //   .Select((x, y) => Sql.CountDistinct(x.Id))
        //   .ToSql(out _);

        //visitor.VisitSqlMethodCall(new SqlSegment { Expression = expr.Body });

        ////From Order a;LeftJoin User b includeForm Last;Where Order a;ToList解析
        ////From User;From Order includeForm Last Filter;From Detail includeForm Last Filter;ToList解析
        ////结构：EntityType,QueryType(from),IncludeFrom,IncludeManyFrom,Body,JoinOn,AlaisName
        ////到ToList,First才开始解析，前面只生成方便解析的结构
        ////Stack<>
        var rep = dbFactory.Create();
        //var result = rep.From<Order, User>()
        //    .Include((a, b) => a.Buyer.Company)
        //    //.ThenInclude(f => f.Company)
        //    .InnerJoin((x, y) => x.SellerId == y.Id && x.IsEnabled && y.IsEnabled)
        //    .Where((x, y) => Sql.Exists<Order>(f => x.Id == f.Id))
        //    .Select((a, b) => new { a.OrderNo, BuyerName = a.Buyer.Name, Seller = b })
        //    .First();
        List<Order> orders = null;
        var sql = rep.Update<Order>()
            .WithBy(f => new { OrderNo = "ON_" + f.OrderNo, TotalAmount = DBNull.Value, f.BuyerId, f.SellerId }, orders)
            .ToSql(out _);
        int sfds = 0;
        //rep.Update<Order>().From<User, Company>((a, b, c) => a.BuyerId == b.Id && b.CompanyId == c.Id && c.Name == "pa")
        //    .Set((x, y, z) => new { OrderNo = "Order_" + y.Name + Guid.NewGuid().ToString() });

        //rep.From<Order>().Include(f => f.Buyer).Include(f => f.Details)
        //    .InnerJoin(f => f.BuyerId == f.Buyer.Id)
        //    .Where(f => f.CreatedAt > DateTime.Parse("2021-10-01"))
        //    .SelectAggregate((a, b) => new { b.BuyerId, b.OrderNo, Order = b, ProductCount = Sql.Count(b.Details) })
        //    .WithTable(f => f.From<Seller>().Select(f => new { SellerId = f.Id, f.Name }))
        //    .InnerJoin((a, b) => a.BuyerId == b.SellerId)
        //    .Where((a, b, c) => a.Exists<User>(t => t.Id == b.BuyerId && t.Age > 40)
        //        && a.In(b.BuyerId, f => f.From<User>().Where(t => t.Id == b.BuyerId && t.Age > 40).Select(t => t.Id))
        //        && a.In(b.BuyerId, new int[] { 1, 2, 3 }))
        //    .Select((a, b) => new { UserId = a.BuyerId, UserName = b.Name, a.OrderNo, a.ProductCount })
        //    .ToList();

        rep.From<Order>().InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Where((a, b) => a.CreatedAt > DateTime.Parse("2021-10-01"))
            .SelectAggregate((a, b, c) => new { b.BuyerId, b.OrderNo, ProductCount = a.Max(b.Id) });

        //rep.WithTable(f => f.From<Order>().SelectAggregate((a, b) => new { b.Id, b.OrderNo, ProductCount = a.Count(b.Id) }))
        //    .InnerJoin<User>((a, b) => true)
        //    .Where((a, b, c) => a.Exists<User>(t => t.Id == c.Id) && a.In(c.Id, new int[] { 1, 2, 3 }))
        //    .Select((a, b) => new { a = a, b = b })
        //    .ToList();
        ////.ToSql();
        //repository.From<Order>().Include(f => f.Buyer).Where(f => f.IsEnabled).ToList();
        //repository.From<User>().IncludeMany(f => f.Orders, t > t.IsEnabled)
        //        .ThenIncludeMany(f => f.Details, t => t.IsEnabled)
        //        .Where(f => f.Name.Contains("leafkevin")).ToList();

        //fromQuery.InnerJoin<Order>(null)
        //        .InnerJoin<OrderDetail>(null)

        //        .GroupBy((a, b, c) => new
        //        {
        //            a.Id,
        //            a.Name,
        //            b.OrderNo
        //        })
        //        .Having((a, b, c, d) => a.Count() > 4)

        //        .Select((a, b, c, d) => new
        //        {
        //            a.Grouping.Id,
        //            a.Grouping.Name,
        //            a.Grouping.OrderNo,
        //            ProductCount = a.Count(c.Id),
        //            TotalAmount = a.Sum(c.TotalAmount)
        //        });

        sql = repository.From<User>()
               .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
               .IncludeMany((a, b) => a.Orders, b => b.OrderNo.Contains("20221001"))
               .ThenIncludeMany(f => f.Details)
               .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
               .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.Id })
               .Select((x, a, b) => new
               {
                   x.Grouping,
                   OrderCount = x.Count(b.Id),
                   TotalAmount = x.Sum(b.TotalAmount)
               })
               .ToSql(out _);

        //fromQuery.IncludeMany(f => f.Orders).ThenIncludeMany(f => f.Details)
        //    .GroupBy((a, b, c) => new { a.Id, a.Name, b.OrderNo })
        //    .Having((a, b, c, d) => a.Min(c.TotalAmount) > 5)
        //    .OrderBy((a, b, c, d) => new { UserId = b.Id, OrderId = c.Id })
        //    .Select((a, b, c, d) => new
        //    {
        //        a.Grouping.Id,
        //        a.Grouping.Name,
        //        a.Grouping.OrderNo,
        //        ProductCount = a.Count(c.Id),
        //        TotalAmount = a.Sum(c.TotalAmount)
        //    });


        Console.WriteLine("Hello World!");
    }

    static void Init(Expression<Func<Order, object>> expr)
    {
        int dsf = 0;
        //var memberInitExpr = expr.Body as MemberInitExpression;
        //var newExpr = memberInitExpr.NewExpression;
        //foreach (var bindingExpr in memberInitExpr.Bindings)
        //{
        //    var bindingType = bindingExpr.BindingType;
        //    var typeName = bindingExpr.GetType().FullName;
        //    var memberAssignment = bindingExpr as MemberAssignment;
        //    var exprString = memberAssignment.Expression.ToString();
        //    MemberMemberBinding dd;
        //    MemberListBinding dsd;
        //    int dsf = 0;
        //} 
    }
}

