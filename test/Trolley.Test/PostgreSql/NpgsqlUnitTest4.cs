﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace Trolley.Test;

public class NpgsqlUnitTest4
{
    enum Sex { Male, Female }
    struct Studuent
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    private readonly IOrmDbFactory dbFactory;
    public NpgsqlUnitTest4()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register("fengling", true, f =>
            {
                var connectionString = "Host=localhost;Username=postgres;Password=123456;Database=fengling;Maximum Pool Size=20";
                f.Add<NpgSqlProvider>(connectionString, true);
            })
            .AddTypeHandler<JsonTypeHandler>()
            .Configure<NpgSqlProvider, NpgsqlModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public void IsEntityType()
    {
        Assert.False(typeof(Sex).IsEntityType());
        Assert.False(typeof(Sex?).IsEntityType());
        Assert.True(typeof(Studuent).IsEntityType());
        Assert.False(typeof(string).IsEntityType());
        Assert.False(typeof(int).IsEntityType());
        Assert.False(typeof(int?).IsEntityType());
        Assert.False(typeof(Guid).IsEntityType());
        Assert.False(typeof(Guid?).IsEntityType());
        Assert.False(typeof(DateTime).IsEntityType());
        Assert.False(typeof(DateTime?).IsEntityType());
        Assert.False(typeof(byte[]).IsEntityType());
        Assert.False(typeof(int[]).IsEntityType());
    }
    [Fact]
    public async void Delete()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(f => f.Id == 1);
        var count = repository.Create<User>(new User
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
        });
        Assert.Equal(1, count);
        count = await repository.DeleteAsync<User>(f => f.Id == 1);
        repository.Commit();
        Assert.Equal(1, count);
    }
    [Fact]
    public async void Delete_Multi()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(new[] { new { Id = 1 }, new { Id = 2 } });
        var count = repository.Create<User>(new[]
        {
            new User
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
            },
            new User
            {
                Id = 2,
                Name = "cindy",
                Age = 21,
                CompanyId = 2,
                Gender = Gender.Male,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });
        Assert.Equal(2, count);
        count = await repository.DeleteAsync<User>(new[] { new { Id = 1 }, new { Id = 2 } });
        repository.Commit();
        Assert.Equal(2, count);
    }
    [Fact]
    public async void Delete_Multi1()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(new[] { 1, 2 });
        var count = repository.Create<User>(new[]
        {
            new User
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
            },
            new User
            {
                Id = 2,
                Name = "cindy",
                Age = 21,
                CompanyId = 2,
                Gender = Gender.Male,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });
        Assert.Equal(2, count);
        count = await repository.DeleteAsync<User>(new int[] { 1, 2 });
        repository.Commit();
        Assert.Equal(2, count);
    }
    [Fact]
    public async void Delete_Multi_Where()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(f => new int[] { 1, 2 }.Contains(f.Id));
        var count = repository.Create<User>(new[]
        {
            new User
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
            },
            new User
            {
                Id = 2,
                Name = "cindy",
                Age = 21,
                CompanyId = 2,
                Gender = Gender.Male,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });
        Assert.Equal(2, count);
        count = await repository.DeleteAsync<User>(f => new int[] { 1, 2 }.Contains(f.Id));
        repository.Commit();
        Assert.Equal(2, count);
    }
}