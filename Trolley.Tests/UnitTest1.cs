using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Trolley.Tests;

public class UnitBase
{
    private readonly IOrmDbFactory dbFactory;
    public UnitBase()
    {
        var dbKeys = new Dictionary<string, string>();
        dbKeys.Add("mysql", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;");
        //dbKeys.Add("npgsql", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;");
        //dbKeys.Add("sql", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;");
        var services = new ServiceCollection();
        services.AddSingleton<IOrmProvider, MySqlProvider>();
        services.AddSingleton<IOrmDbFactory, OrmDbFactory>(f =>
        {
            var dbFactory = new OrmDbFactory(f);
            dbFactory.Register("mysql", true, f => f.Add<MySqlProvider>(dbKeys["mysql"], true));
            //dbFactory.Register("npgsql", false, f => f.Add<NpgSqlProvider>(dbKeys["mysql"], true));
            //dbFactory.Register("sql", false, f => f.Add<SqlServerProvider>(dbKeys["sql"], true));
            dbFactory.Configure(f => new ModelConfiguration().OnModelCreating(f));
            return dbFactory;
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public void Query()
    {
        string dbKey = "mysql";
        using var repository = this.dbFactory.Create(dbKey);
        //var count1 = repository.Create<User>().WithBy(new[]
        //{
        //    new
        //    {
        //        Id = 1,
        //        Name = "leafkevin",
        //        Age = 25,
        //        CompanyId = 1,
        //        Gender = Gender.Male,
        //        IsEnabled = true,
        //        CreatedAt = DateTime.Now,
        //        CreatedBy = 1,
        //        UpdatedAt = DateTime.Now,
        //        UpdatedBy = 1
        //    },
        //    new
        //    {
        //        Id = 2,
        //        Name = "cindy",
        //        Age = 22,
        //        CompanyId = 1,
        //        Gender = Gender.Male,
        //        IsEnabled = true,
        //        CreatedAt = DateTime.Now,
        //        CreatedBy = 1,
        //        UpdatedAt = DateTime.Now,
        //        UpdatedBy = 1
        //    }
        //}).Execute();
        //Assert.Equal(2, count1);
        var company1 = new Dictionary<string, object>()
        {
                { "Id", 1},
                { "Name","Œ¢»Ì"},
                { "IsEnabled", true},
                { "CreatedAt", DateTime.Now},
                { "CreatedBy", 1},
                { "UpdatedAt", DateTime.Now},
                { "UpdatedBy", 1}
         };
        var company2 = new Dictionary<string, object>()
        {
                { "Id", 2},
                { "Name","π»∏Ë"},
                { "IsEnabled", true},
                { "CreatedAt", DateTime.Now},
                { "CreatedBy", 1},
                { "UpdatedAt", DateTime.Now},
                { "UpdatedBy", 1}
         };
        var count2 = repository.Create<Company>().WithBy(company2).Execute();
        Assert.Equal(2, count2);

        //var sql = rep.From<Order, User>()
        //  .Include((a, b) => a.Buyer.Company)
        //  .InnerJoin((x, y) => x.SellerId == y.Id && x.IsEnabled && y.IsEnabled)
        //  .Select((a, b) => new { a.OrderNo, BuyerName = a.Buyer.Name, Seller = b })
        //  .ToSql(out _);
        int sfsdfs = 0;
    }
}