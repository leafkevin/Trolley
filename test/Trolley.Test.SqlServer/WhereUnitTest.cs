﻿using Microsoft.Extensions.DependencyInjection;
using System;
using Trolley.SqlServer;
using Xunit;

namespace Trolley.Test.SqlServer;

public class WhereUnitTest : UnitTestBase
{
    public WhereUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register<SqlServerProvider>("fengling", "Server=127.0.0.1;Database=fengling;Uid=sa;password=SQLserverSA123456;TrustServerCertificate=true", true)
            .Configure<SqlServerProvider, ModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async void WhereBoolean()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => f.IsEnabled);
        Assert.True(result1.Count > 0);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.True(result2.Count > 0);
        Assert.True(result1.Count == result2.Count);
    }
    [Fact]
    public async void WhereMemberVisit()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => !(f.IsEnabled == false) && f.Id > 0);
        Assert.True(result1.Count > 0);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.True(result2.Count > 0);
        Assert.True(result1.Count == result2.Count);
    }
    [Fact]
    public async void WhereStringEnum()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql1 = repository.From<Company>()
          .Where(f => f.Nature == CompanyNature.Internet)
          .ToSql(out _);
        Assert.True(sql1 == "SELECT [Id],[Name],[Nature],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy] FROM [sys_company] WHERE [Nature]='Internet'");
        var result1 = await repository.QueryAsync<Company>(f => f.Nature == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);

        var sql2 = repository.From<Company>()
         .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
         .ToSql(out _);
        Assert.True(sql2 == "SELECT [Id],[Name],[Nature],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy] FROM [sys_company] WHERE COALESCE([Nature],'Internet')='Internet'");
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result2.Count >= 2);

        var localNature = CompanyNature.Internet;
        var sql3 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.True(sql3 == "SELECT [Id],[Name],[Nature],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy] FROM [sys_company] WHERE COALESCE([Nature],'Internet')=@p0");
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result3 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result3.Count >= 2);
    }
    [Fact]
    public async void WhereCoalesceConditional()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql1 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.True(sql1 == "SELECT [Id],[Name],[Nature],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy] FROM [sys_company] WHERE COALESCE([Nature],'Internet')='Internet'");
        var result1 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);
        Assert.True(result1[0].Nature == CompanyNature.Internet);

        var localNature = CompanyNature.Internet;
        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.True(sql2 == "SELECT [Id],[Name],[Nature],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy] FROM [sys_company] WHERE COALESCE([Nature],'Internet')=@p0");
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result2.Count >= 2);
        Assert.True(result2[0].Nature == localNature);

        var sql3 = repository.From<Company>()
        .Where(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature)
        .ToSql(out dbParameters);
        Assert.True(sql3 == "SELECT [Id],[Name],[Nature],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy] FROM [sys_company] WHERE (CASE WHEN [IsEnabled]=1 THEN [Nature] ELSE 'Internet' END)=@p0");
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result3 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.True(result3.Count >= 2);
        Assert.True(result3[0].Nature == localNature);
    }
    [Fact]
    public async void WhereIsNull()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql1 = repository.From<Order>()
           .Where(f => f.BuyerId.IsNull())
           .ToSql(out _);
        Assert.True(sql1 == "SELECT [Id],[OrderNo],[ProductCount],[TotalAmount],[BuyerId],[BuyerSource],[SellerId],[Products],[Disputes],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy] FROM [sys_order] WHERE [BuyerId] IS NULL");
        repository.BeginTransaction();
        repository.Update<Order>(f => new { BuyerId = DBNull.Value }, new { Id = 1 });
        var result1 = repository.From<Order>()
            .Where(f => f.BuyerId.IsNull())
            .First();
        repository.Commit();
        Assert.True(result1.Id == 1);
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result2.Count >= 2);
        var localNature = CompanyNature.Internet;
        var result3 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result2.Count >= 2);
    }
    [Fact]
    public void WhereAndOr()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<Order, User>()
            .Where((a, b) => a.BuyerId == b.Id)
            .And(true, (a, b) => a.SellerId.IsNull() || !a.ProductCount.HasValue)
            .And(true, (a, b) => a.Products != null)
            .And(true, (a, b) => a.Products == null || a.Disputes == null)
            .ToSql(out _);
        Assert.True(sql == "SELECT * FROM [sys_order] a,[sys_user] b WHERE a.[BuyerId]=b.[Id] AND (a.[SellerId] IS NULL OR a.[ProductCount] IS NULL) AND a.[Products] IS NOT NULL AND (a.[Products] IS NULL OR a.[Disputes] IS NULL)");
    }
    [Fact]
    public void Where()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql1 = repository.From<Order>()
            .Where(f => Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) && (f.BuyerId.IsNull() || f.BuyerId == 2)
                && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo)))
            .Select(f => f.Id)
            .ToSql(out _);
        Assert.True(sql1 == "SELECT a.[Id] FROM [sys_order] a WHERE EXISTS(SELECT * FROM [sys_user] t WHERE t.[Id]=a.[BuyerId] AND t.[IsEnabled]=1) AND (a.[BuyerId] IS NULL OR a.[BuyerId]=2) AND (a.[OrderNo] LIKE '%ON_%' OR (a.[OrderNo] IS NULL OR a.[OrderNo]=''))");

        var sql2 = repository.From<Order>()
          .Where(f => (f.BuyerId.IsNull() || f.BuyerId == 2) && (f.OrderNo.Contains("ON_") || string.IsNullOrEmpty(f.OrderNo))
              && (Sql.Exists<User>(t => t.Id == f.BuyerId && t.IsEnabled) || f.SellerId.IsNull()))
          .Select(f => f.Id)
          .ToSql(out _);
        Assert.True(sql2 == "SELECT a.[Id] FROM [sys_order] a WHERE ([BuyerId] IS NULL OR [BuyerId]=2) AND ([OrderNo] LIKE '%ON_%' OR ([OrderNo] IS NULL OR [OrderNo]='')) AND (EXISTS(SELECT * FROM [sys_user] t WHERE t.[Id]=a.[BuyerId] AND t.[IsEnabled]=1) OR a.[SellerId] IS NULL)");
    }
}
