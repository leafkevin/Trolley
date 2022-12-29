using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Trolley.Tests;

public class MySqlUnitTest1
{
    private readonly IOrmDbFactory dbFactory;
    public MySqlUnitTest1()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOrmProvider, MySqlProvider>();
        services.AddSingleton<IOrmDbFactory, OrmDbFactory>(f =>
        {
            var dbFactory = new OrmDbFactory(f);
            var connString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
            dbFactory.Register("mysql", true, f => f.Add<MySqlProvider>(connString, true));
            dbFactory.Configure(f => new ModelConfiguration().OnModelCreating(f));
            return dbFactory;
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async void Insert_WithBy_AnonymousObject()
    {
        using var repository = this.dbFactory.Create();
        var count = repository.Delete<User>().Where(f => f.Id == 1).Execute();
        count = await repository.Create<User>()
            .WithBy(new
            {
                Id = 1,
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }).ExecuteAsync();
        Assert.Equal(1, count);
    }
    [Fact]
    public async void Insert_WithBy_Dictionary_AutoIncrement()
    {
        using var repository = this.dbFactory.Create();
        await repository.Delete<Company>().Where(f => f.Id == 1).ExecuteAsync();
        var id = repository.Create<Company>()
            .WithBy(new Dictionary<string, object>()
            {
                { "Name","Î¢Èí11"},
                { "IsEnabled", true},
                { "CreatedAt", DateTime.Now},
                { "CreatedBy", 1},
                { "UpdatedAt", DateTime.Now},
                { "UpdatedBy", 1}
            }).Execute();
        var maxId = repository.From<Company>().Max(f => f.Id);
        Assert.Equal(maxId, id);
    }
    [Fact]
    public async void Insert_WithBy_Batch_AnonymousObjects()
    {
        using var repository = this.dbFactory.Create();
        await repository.Delete<Product>().Where(new int[] { 1, 2, 3 }).ExecuteAsync();
        var count = repository.Create<Product>()
            .WithBy(new[]
            {
                new
                {
                    Id = 1,
                    ProductNo="PN-001",
                    Name = "²¨Ë¾µÇÓðÈÞ·þ",
                    BrandId = 1,
                    CategoryId = 1,
                    IsEnabled = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = 1,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = 1
                },
                new
                {
                    Id = 2,
                    ProductNo="PN-002",
                    Name = "Ñ©ÖÐ·ÉÓðÈÞ¿ã",
                    BrandId = 2,
                    CategoryId = 2,
                    IsEnabled = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = 1,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = 1
                },
                new
                {
                    Id = 3,
                    ProductNo="PN-003",
                    Name = "ÓÅÒÂ¿â±£Å¯ÄÚÒÂ",
                    BrandId = 3,
                    CategoryId = 3,
                    IsEnabled = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = 1,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = 1
                }
            }).Execute();
        Assert.Equal(3, count);
    }
    [Fact]
    public async void Insert_WithBy_Batch_Dictionaries()
    {
        using var repository = this.dbFactory.Create();
        await repository.Delete<Product>().Where(new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } }).ExecuteAsync();
        var count = repository.Create<Product>()
            .WithBy(new[]
            {
                new Dictionary<string,object>
                {
                    { "Id",1 },
                    { "ProductNo","PN-001"},
                    { "Name","²¨Ë¾µÇÓðÈÞ·þ"},
                    { "BrandId",1},
                    { "CategoryId",1},
                    { "IsEnabled",true},
                    { "CreatedAt",DateTime.Now},
                    { "CreatedBy",1},
                    { "UpdatedAt",DateTime.Now},
                    { "UpdatedBy",1}
                },
                new Dictionary<string,object>
                {
                    { "Id",2},
                    { "ProductNo","PN-002"},
                    { "Name","Ñ©ÖÐ·ÉÓðÈÞ¿ã"},
                    { "BrandId",2},
                    { "CategoryId",2},
                    { "IsEnabled",true},
                    { "CreatedAt",DateTime.Now},
                    { "CreatedBy",1},
                    { "UpdatedAt",DateTime.Now},
                    { "UpdatedBy",1}
                },
                new Dictionary<string,object>
                {
                    { "Id",3},
                    { "ProductNo","PN-003"},
                    { "Name","ÓÅÒÂ¿â±£Å¯ÄÚÒÂ"},
                    { "BrandId",3},
                    { "CategoryId",3},
                    { "IsEnabled",true},
                    { "CreatedAt",DateTime.Now},
                    { "CreatedBy",1},
                    { "UpdatedAt",DateTime.Now},
                    { "UpdatedBy",1}
    }
            }).Execute();
        Assert.Equal(3, count);
    }
    [Fact]
    public async void Insert_RawSql()
    {
        using var repository = this.dbFactory.Create();
        repository.Delete<Brand>().Where(new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } }).Execute();
        var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES(@Id,@BrandNo,@Name,1,NOW(),@User,NOW(),@User)";
        var count = await repository.Create<Brand>().RawSql(rawSql, new
        {
            Id = 1,
            BrandNo = "BN-001",
            Name = "²¨Ë¾µÇ",
            User = 1
        }).ExecuteAsync();
        Assert.Equal(1, count);
        count = await repository.Create<Brand>().RawSql(rawSql, new
        {
            Id = 2,
            BrandNo = "BN-002",
            Name = "Ñ©ÖÐ·É",
            User = 1
        }).ExecuteAsync();
        Assert.Equal(1, count);
        count = await repository.Create<Brand>().RawSql(rawSql, new
        {
            Id = 3,
            BrandNo = "BN-003",
            Name = "ÓÅÒÂ¿â",
            User = 1
        }).ExecuteAsync();
        Assert.Equal(1, count);
    }
}