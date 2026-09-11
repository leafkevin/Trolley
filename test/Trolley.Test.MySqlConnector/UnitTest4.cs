using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Trolley.MySqlConnector;

namespace Trolley.Test.MySqlConnector;

[TestFixture]
public class UnitTest4 : UnitTestBase
{
    [SetUp]
    public void Setup()
    {
        var connectionString = "Server=192.168.61.67;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
        var builder = new OrmDbFactoryBuilder()
            .Register(OrmProviderType.MySql, "fengling", f => f.Use(connectionString), true)
            .UseMapping<ModelMappingConfiguration>(OrmProviderType.MySql)
            .UseInterceptor(new MyDbInterceptor());
        this.dbFactory = builder.Build();
        this.Initialize(1);
    }
    [Test]
    public void IsEntityType()
    {
        Assert.IsFalse(typeof(Sex).IsEntityType(out _));
        Assert.IsFalse(typeof(Sex?).IsEntityType(out _));
        Assert.IsTrue(typeof(Studuent).IsEntityType(out _));
        Assert.IsFalse(typeof(string).IsEntityType(out _));
        Assert.IsFalse(typeof(int).IsEntityType(out _));
        Assert.IsFalse(typeof(int?).IsEntityType(out _));
        Assert.IsFalse(typeof(Guid).IsEntityType(out _));
        Assert.IsFalse(typeof(Guid?).IsEntityType(out _));
        Assert.IsFalse(typeof(DateTime).IsEntityType(out _));
        Assert.IsFalse(typeof(DateTime?).IsEntityType(out _));
        Assert.IsFalse(typeof(byte[]).IsEntityType(out _));
        Assert.IsFalse(typeof(int[]).IsEntityType(out _));
        Assert.IsFalse(typeof(List<int>).IsEntityType(out _));
        Assert.IsFalse(typeof(List<int[]>).IsEntityType(out _));
        Assert.IsFalse(typeof(Collection<string>).IsEntityType(out _));
        Assert.IsFalse(typeof(DBNull).IsEntityType(out _));

        var vt1 = ("kevin");
        Assert.IsFalse(vt1.GetType().IsEntityType(out _));
        var vt2 = (1, "kevin", 25, 30000.00d);
        Assert.IsTrue(vt2.GetType().IsEntityType(out _));
        Assert.IsTrue(typeof((string Name, int Age)).IsEntityType(out _));
        Assert.IsTrue(typeof(Dictionary<string, int>).IsEntityType(out _));
        Assert.IsTrue(typeof(Studuent).IsEntityType(out _));
        Assert.IsTrue(typeof(Teacher).IsEntityType(out _));

        Assert.IsTrue(typeof(Dictionary<string, int>[]).IsEntityType(out _));
        Assert.IsTrue(typeof(List<Dictionary<string, int>>).IsEntityType(out _));
        Assert.IsTrue(typeof(List<Dictionary<string, int>[]>).IsEntityType(out _));
        Assert.IsTrue(typeof(Collection<Dictionary<string, int>>).IsEntityType(out _));
        Assert.IsTrue(typeof(Dictionary<string, Dictionary<string, int>>).IsEntityType(out _));

        Assert.IsTrue(typeof(Teacher[]).IsEntityType(out _));
        Assert.IsTrue(typeof(List<Teacher>).IsEntityType(out _));
        Assert.IsTrue(typeof(List<Teacher[]>).IsEntityType(out _));
        Assert.IsTrue(typeof(Collection<Teacher>).IsEntityType(out _));
        Assert.IsTrue(typeof(Dictionary<string, Teacher>).IsEntityType(out _));

        Assert.IsTrue(typeof(Studuent[]).IsEntityType(out _));
        Assert.IsTrue(typeof(List<Studuent>).IsEntityType(out _));
        Assert.IsTrue(typeof(List<Studuent[]>).IsEntityType(out _));
        Assert.IsTrue(typeof(Collection<Studuent>).IsEntityType(out _));
        Assert.IsTrue(typeof(Dictionary<string, Studuent>).IsEntityType(out _));
    }
    [Test]
    public async Task Delete()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(f => f.Id == 1);
        var count = repository.Create<User>(new User
        {
            Id = 1,
            TenantId = "1",
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
        Assert.AreEqual(1, count);
        count = await repository.DeleteAsync<User>(f => f.Id == 1);
        repository.Commit();
        Assert.AreEqual(1, count);

        var id = 1;
        var sql = repository.Delete<User>()
            .Where(f => f.Id == id)
            .ToSql(out var parameters);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Id`=@p0", sql);
    }
    [Test]
    public async Task Delete_Multi()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.DeleteByIds<User>(new[] { new { Id = 1 }, new { Id = 2 } });
        var count = repository.Create<User>(new[]
        {
            new User
            {
                Id = 1,
                TenantId = "1",
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
                TenantId = "2",
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
        Assert.AreEqual(2, count);
        count = await repository.DeleteByIdsAsync<User>(new[] { new { Id = 1 }, new { Id = 2 } });
        repository.Commit();
        Assert.AreEqual(2, count);

        var sql = repository.Delete<User>()
            .WhereByIds(new[] { new { Id = 1 }, new { Id = 2 } })
            .ToSql(out var parameters);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Id` IN (@Id0,@Id1)", sql);
        Assert.AreEqual(1, (int)parameters[0].Value);
        Assert.AreEqual(2, (int)parameters[1].Value);

        var sql1 = repository.Delete<Function>()
            .WhereByIds(new[] { new { MenuId = 1, PageId = 1 }, new { MenuId = 2, PageId = 2 } })
            .ToSql(out parameters);
        Assert.AreEqual("DELETE FROM `sys_function` WHERE `MenuId`=@MenuId0 AND `PageId`=@PageId0 OR `MenuId`=@MenuId1 AND `PageId`=@PageId1", sql1);
        Assert.AreEqual(4, parameters.Count);
        Assert.AreEqual(1, (int)parameters[0].Value);
        Assert.AreEqual(1, (int)parameters[1].Value);
        Assert.AreEqual(2, (int)parameters[2].Value);
        Assert.AreEqual(2, (int)parameters[3].Value);
    }
    [Test]
    public async Task Delete_Multi1()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.DeleteByIds<User>(new[] { 1, 2 });
        var count = repository.Create<User>(new[]
        {
            new User
            {
                Id = 1,
                TenantId = "1",
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
                TenantId = "2",
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
        Assert.AreEqual(2, count);
        count = await repository.DeleteByIdsAsync<User>(new int[] { 1, 2 });
        repository.Commit();
        Assert.AreEqual(2, count);

        var sql = repository.Delete<User>()
            .WhereByIds(new int[] { 1, 2 })
            .ToSql(out var parameters);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Id` IN (@Id0,@Id1)", sql);
        Assert.AreEqual(1, (int)parameters[0].Value);
        Assert.AreEqual(2, (int)parameters[1].Value);

        var orderNos = new string[] { "ON_001", "ON_002", "ON_003" };
        sql = repository.Delete<Order>()
            .Where(f => f.BuyerId == 1 && orderNos.Contains(f.OrderNo))
            .ToSql(out parameters);
        Assert.AreEqual("DELETE FROM `sys_order` WHERE `BuyerId`=1 AND `OrderNo` IN (@p0,@p1,@p2)", sql);
        Assert.AreEqual(orderNos[0], (string)parameters[0].Value);
        Assert.AreEqual(orderNos[1], (string)parameters[1].Value);
        Assert.AreEqual(orderNos[2], (string)parameters[2].Value);
    }
    [Test]
    public async Task Delete_Multi_Where()
    {
        var repository = this.dbFactory.Create();
        repository.BeginTransaction();
        repository.Delete<User>(f => new int[] { 1, 2 }.Contains(f.Id));
        var count = repository.Create<User>(new[]
        {
            new User
            {
                Id = 1,
                TenantId = "1",
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
                TenantId = "2",
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
        Assert.AreEqual(2, count);
        count = await repository.DeleteAsync<User>(f => new int[] { 1, 2 }.Contains(f.Id));
        repository.Commit();
        Assert.AreEqual(2, count);

        var sql = repository.Delete<User>()
           .Where(f => new int[] { 1, 2 }.Contains(f.Id))
           .ToSql(out var parameters);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Id` IN (1,2)", sql);
        //Assert.IsTrue((int)parameters[0].Value == 1);
        //Assert.IsTrue((int)parameters[1].Value == 2);
    }
    [Test]
    public void Delete_Where_And()
    {
        var repository = this.dbFactory.Create();
        bool? isMale = true;
        var sql = repository.Delete<User>()
            .Where(f => f.Name.Contains("kevin"))
            .And(isMale.HasValue, f => f.Age > 25)
            .ToSql(out _);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Name` LIKE '%kevin%' AND `Age`>25", sql);
    }
    [Test]
    public async Task Delete_Where_Returning()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql = repository.Delete<User>()
            .Where(f => f.Id == 1)
            .Returning(f => new { f.Id, f.TenantId, Info = $"{f.Gender}-{f.Age}-{f.Name.ToUpper()}" })
            .ToSql(out _);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Id`=1 RETURNING `Id`,`TenantId`,CONCAT(`Gender`,'-',CAST(`Age` AS CHAR),'-',UPPER(`Name`)) AS `Info`", sql);

        var user = await repository.QueryByIdAsync<User>(1);
        var result1 = await repository.Delete<User>()
            .Where(f => f.Id == 1)
            .Returning(f => new { f.Id, f.TenantId, Info = $"{f.Gender}-{f.Age}-{f.Name.ToUpper()}" })
            .ExecuteAsync();
        result1.Sort((x, y) => x.Id.CompareTo(y.Id));
        Assert.AreEqual(user.Id, result1[0].Id);
        Assert.AreEqual(user.TenantId, result1[0].TenantId);
        Assert.AreEqual($"{user.Gender}-{user.Age}-{user.Name.ToUpper()}", result1[0].Info);

        var sql2 = repository.Delete<User>()
            .Where(f => f.Id == 1)
            .Returning<User>("*")
            .ToSql(out _);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Id`=1 RETURNING *", sql2);

        await repository.DeleteByIdAsync<User>(1);
        await repository.Create<User>()
            .WithBy(new
            {
                Id = 1,
                TenantId = "1",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            })
            .ExecuteAsync();

        user = await repository.QueryByIdAsync<User>(1);
        var users = await repository.Delete<User>()
            .Where(f => f.Id == 1)
            .Returning<User>("*")
            .ExecuteAsync();
        Assert.AreEqual(user.Id, users[0].Id);
        Assert.AreEqual(user.TenantId, users[0].TenantId);

        this.Initialize(1);
        var userIds = new int[] { 1, 2, 3 };
        sql = repository.Delete<User>()
          .Where(f => userIds.Contains(f.Id))
          .Returning(f => new { f.Id, f.TenantId, Info = $"{f.Gender}-{f.Age}-{f.Name.ToUpper()}" })
          .ToSql(out _);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Id` IN (@p0,@p1,@p2) RETURNING `Id`,`TenantId`,CONCAT(`Gender`,'-',CAST(`Age` AS CHAR),'-',UPPER(`Name`)) AS `Info`", sql);

        users = await repository.QueryByIdsAsync<User>(userIds);
        result1 = await repository.Delete<User>()
            .Where(f => userIds.Contains(f.Id))
            .Returning(f => new { f.Id, f.TenantId, Info = $"{f.Gender}-{f.Age}-{f.Name.ToUpper()}" })
            .ExecuteAsync();
        users.Sort((x, y) => x.Id.CompareTo(y.Id));
        result1.Sort((x, y) => x.Id.CompareTo(y.Id));
        Assert.AreEqual(users[0].Id, result1[0].Id);
        Assert.AreEqual(users[0].TenantId, result1[0].TenantId);
        Assert.AreEqual($"{users[0].Gender}-{users[0].Age}-{users[0].Name.ToUpper()}", result1[0].Info);
    }
    [Test]
    public void Delete_Enum_Fields()
    {
        var repository = this.dbFactory.Create();
        var sql1 = repository.Delete<User>()
            .Where(f => f.Gender == Gender.Male)
            .ToSql(out _);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Gender`='Male'", sql1);

        var gender = Gender.Male;
        var sql2 = repository.Delete<User>()
            .Where(f => f.Gender == gender)
            .ToSql(out var parameters1);
        Assert.AreEqual("DELETE FROM `sys_user` WHERE `Gender`=@p0", sql2);
        Assert.AreEqual("@p0", parameters1[0].ParameterName);
        Assert.That(parameters1[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(gender.ToString(), (string)parameters1[0].Value);

        var sql3 = repository.Delete<Company>()
             .Where(f => f.Nature == CompanyNature.Internet)
             .ToSql(out _);
        Assert.AreEqual("DELETE FROM `sys_company` WHERE `Nature`='Internet'", sql3);

        var nature = CompanyNature.Internet;
        var sql4 = repository.Delete<Company>()
             .Where(f => f.Nature == nature)
             .ToSql(out var parameters2);
        Assert.AreEqual("DELETE FROM `sys_company` WHERE `Nature`=@p0", sql4);
        Assert.AreEqual("@p0", parameters2[0].ParameterName);
        Assert.That(parameters2[0].Value, Is.TypeOf<string>());
        Assert.AreEqual(CompanyNature.Internet.ToString(), (string)parameters2[0].Value);
    }
    [Test]
    public async Task Transation()
    {
        var repository = this.dbFactory.Create();
        bool? isMale = true;
        await repository.BeginTransactionAsync();
        await repository.Update<User>()
            .Set(new { Name = "leafkevin1" })
            .WhereBy(new { Id = 1 })
            .ExecuteAsync();
        await repository.UpdateAsync<User>(new { Name = "leafkevin1", Id = 1 });
        await repository.Delete<User>()
            .Where(f => f.Name.Contains("kevin"))
            .And(isMale.HasValue, f => f.Age > 25)
            .ExecuteAsync();
        await repository.CommitAsync();
        if (!await repository.ExistsByIdAsync<User>(1))
            await repository.CreateAsync<User>(new User
            {
                Id = 1,
                TenantId = "1",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                UpdatedBy = 1
            });
        var user = await repository.QueryByIdAsync<User>(1);
        Assert.IsNotNull(user);
    }
    [Test]
    public async Task Multi_Transation()
    {
        var repository = this.dbFactory.Create();
        bool? isMale = true;
        await repository.BeginTransactionAsync();
        await repository.Update<User>()
            .Set(new { Name = "leafkevin1" })
            .WhereById(new { Id = 1 })
            .ExecuteAsync();
        await repository.UpdateAsync<User>(new { Name = "leafkevin1", Id = 1 });

        var newResitory = this.dbFactory.Create();
        await newResitory.Delete<User>()
            .Where(f => f.Name.Contains("kevin"))
            .And(isMale.HasValue, f => f.Age > 25)
            .ExecuteAsync();
        await newResitory.CommitAsync();
        if (!await repository.ExistsByIdAsync<User>(1))
            await repository.CreateAsync<User>(new User
            {
                Id = 1,
                TenantId = "1",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                UpdatedBy = 1
            });
        var user = await repository.QueryByIdAsync<User>(1);
        Assert.IsNotNull(user);
    }
}
