using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace Trolley.Tests;

public class SqlServerUnitTest4
{
    private readonly IOrmDbFactory dbFactory;
    public SqlServerUnitTest4()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var connectionString = "Server=.;Database=fengling;Uid=sa;password=Angangyur123456;TrustServerCertificate=true";
            var ormProvider = f.GetService<IOrmProvider>();
            var builder = new OrmDbFactoryBuilder();
            builder.Register("fengling", true, f => f.Add<SqlServerProvider>(connectionString, true))
                .Configure(f => new ModelConfiguration().OnModelCreating(f));
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async void Delete()
    {
        using var repository = this.dbFactory.Create();
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
        Assert.Equal(1, count);
    }
    [Fact]
    public async void Delete_Multi()
    {
        using var repository = this.dbFactory.Create();
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
        Assert.Equal(2, count);
    }
    [Fact]
    public async void Delete_Multi1()
    {
        using var repository = this.dbFactory.Create();
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
        Assert.Equal(2, count);
    }
    [Fact]
    public async void Delete_Multi_Where()
    {
        using var repository = this.dbFactory.Create();
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
        Assert.Equal(2, count);
    }
    [Fact]
    public void Delete_Where_And()
    {
        using var repository = this.dbFactory.Create();
        bool? isMale = true;
        var sql = repository.Delete<User>()
            .Where(f => f.Name.Contains("kevin"))
            .And(isMale.HasValue, f => f.Age > 25)
            .ToSql(out _);
        Assert.True(sql == "DELETE FROM [sys_user] WHERE [Name] LIKE '%kevin%' AND [Age]>25");
    }
    [Fact]
    public void Transation()
    {
        using var repository = this.dbFactory.Create();
        bool? isMale = true;
        repository.Timeout(60);
        repository.BeginTransaction();
        repository.Update<User>()
            .WithBy(new { Name = "leafkevin1", Id = 1 })
            .Execute();
        repository.Delete<User>()
            .Where(f => f.Name.Contains("kevin"))
            .And(isMale.HasValue, f => f.Age > 25)
            .Execute();
        repository.Commit();
    }
}
