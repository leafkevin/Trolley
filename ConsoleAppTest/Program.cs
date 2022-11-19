using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System;
using System.Linq.Expressions;
using Trolley;
using Trolley.Providers;

namespace ConsoleAppTest;

enum Sex
{
    Male,
    Female
}
class Program
{
    class Tedss
    {
        public int Id { get; set; }
        public string Name => "5555";
    }
    static void Main(string[] args)
    {
        //var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
        //var connection = new MySqlConnection(connectionString);
        //var command = new MySqlCommand("select Name from sys_user where Id=3", connection);
        //connection.Open();
        //var reader = command.ExecuteReader();
        //if (reader.Read())
        //{
        //    var readerValue = reader.GetValue(0);
        //    int sfdsfsdf = 0;
        //}

        //Init(() => new Order
        //{
        //    OrderNo = "ddd",
        //    TotalAmount = 5 + amount,
        //    Buyer = new User { Age = 40, Name = "leafkevin" },
        //    Details = new List<OrderDetail>() { new OrderDetail { Price = 2, Amount = 3 } }
        //});
        //Init(f => f.TotalAmount == 456 + 15);

        var services = new ServiceCollection();
        services.AddSingleton<IOrmProvider, MySqlProvider>();
        services.AddSingleton<IOrmDbFactory, OrmDbFactory>(f =>
        {
            var dbFactory = new OrmDbFactory(f);
            //var connectionString = "Server=bj-cdb-o9bbr5vl.sql.tencentcdb.com;Port=63227;Database=fengling;Uid=root;password=Siia@TxDb582e4sdf;charset=utf8mb4;";
            var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
            dbFactory.Register("fengling", true, f => f.Add<MySqlProvider>(connectionString, true));
            dbFactory.BuildModel(f => new ModelConfiguration().OnModelCreating(f));
            return dbFactory;
        });
        var serviceProvider = services.BuildServiceProvider();
        var dbFactory = serviceProvider.GetService<IOrmDbFactory>();
        var repository = dbFactory.Create();

        ////From Order a;LeftJoin User b includeForm Last;Where Order a;ToList解析
        ////From User;From Order includeForm Last Filter;From Detail includeForm Last Filter;ToList解析
        ////结构：EntityType,QueryType(from),IncludeFrom,IncludeManyFrom,Body,JoinOn,AlaisName
        ////到ToList,First才开始解析，前面只生成方便解析的结构
        ////Stack<>
        var rep = dbFactory.Create();
        var result = rep.From<Order>().Include(f => f.Buyer).First();


        //rep.From<Order>().Include(f => f.Buyer).Include(f => f.Details)
        //    .InnerJoin(f => f.BuyerId == f.Buyer.Id)
        //    .Where(f => f.CreatedAt > DateTime.Parse("2021-10-01"))
        //    .SelectAggregate((a, b) => new { b.BuyerId, b.OrderNo, Order = b, ProductCount = a.Count(b.Details) })
        //    .WithTable(f => f.From<Seller>().Select(f => new { SellerId = f.Id, f.Name }))
        //    .InnerJoin((a, b) => a.BuyerId == b.SellerId)
        //    .Where((a, b, c) => a.Exists<User>(t => t.Id == b.BuyerId && t.Age > 40)
        //        && a.In(b.BuyerId, f => f.From<User>().Where(t => t.Id == b.BuyerId && t.Age > 40).Select(t => t.Id))
        //        && a.In(b.BuyerId, new int[] { 1, 2, 3 }))
        //    .Select((a, b) => new { UserId = a.BuyerId, UserName = b.Name, a.OrderNo, a.ProductCount })
        //    .ToList();

        //rep.From<Order>().InnerJoin<User>((a, b) => a.BuyerId == b.Id)
        // .Where((a, b) => a.CreatedAt > DateTime.Parse("2021-10-01"))
        // .SelectAggregate((a, b, c) => new { b.BuyerId, b.OrderNo, ProductCount = a.Max(b.Id) });

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

        //fromQuery.IncludeMany(f => f.Orders).Filter(f => f.OrderNo.Contains("20221001")).ThenIncludeMany(f => f.Details)
        //    .GroupBy((a, b, c) => new { a.Id, a.Name, b.OrderNo })
        //    .OrderBy((a, b, c, d) => new { UserId = a.Id, OrderId = b.Id })
        //    .Select((a, b, c, d) => new
        //    {
        //        a.Id,
        //        a.Name,
        //        b.OrderNo,
        //        //ProductCount = a.Count((a, b) => b.Id),
        //        //TotalAmount = a.Sum((a, b) => b.TotalAmount)
        //    }).ToList();

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

