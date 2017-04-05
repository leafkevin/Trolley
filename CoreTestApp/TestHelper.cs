﻿using System;
using System.Threading.Tasks;
using Trolley;
using Trolley.Attributes;

namespace CoreTestApp
{
    public class TestHelper
    {
        public static void Test(string connString)
        {
            int count = 0;
            var user = new User { UniqueId = 1, UserName = "Kevin", Age = 28, Sex = Sex.Male, DeptId = 1, UpdatedAt = DateTime.Now };
            var user1 = new User { UniqueId = 2, UserName = "Cindy", Age = 24, Sex = Sex.Female, DeptId = 2, UpdatedAt = DateTime.Now };
            var repository = new Repository<User>(connString);

            //删除
            count = repository.Delete(user);
            count = repository.Delete(user1);

            //创建
            count = repository.Create(user);
            count = repository.Create(user1);

            //获取
            user = repository.QueryFirst("SELECT Id UniqueId,UserName,Sex FROM Coin_User WHERE Id=@UniqueId", user);
            user = repository.Get(user);

            //更新
            user.UserName = "Kevin-Test";
            user.Sex = Sex.Female;
            count = repository.Update(f => f.Sex, user);
            count = repository.Update(f => new { f.UserName, f.Sex }, user);

            //动态SQL更新
            var builder = new SqlBuilder();
            builder.RawSql("UPDATE Coin_User SET UserName=@UserName")
                   .AddField(user.Sex.HasValue, "Sex=@Sex")
                   .AddSql("WHERE Id=@UniqueId")
                   .Where(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt");
            count = repository.Update(builder.BuildSql(), user);

            count = repository.Update(f => f.RawSql("UPDATE Coin_User SET UserName=@UserName")
                  .AddField(user.Sex.HasValue, "Sex=@Sex")
                  .AddSql("WHERE Id=@UniqueId")
                  .Where(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt"), user);

            //查询&动态SQL
            var list = repository.Query(f => f.RawSql("SELECT * FROM Coin_User")
                    .Where(user.Sex.HasValue, "Sex=@Sex")
                    .Where(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt")
                    .AddSql("ORDER BY UpdatedAt DESC"), user);

            //分页
            var userInfoList = repository.QueryPage<UserInfo>("SELECT Id UniqueId,UserName,Sex FROM Coin_User WHERE Id>@UniqueId", 0, 10, "ORDER BY Id", user);

            //事务
            var context = new RepositoryContext(connString);
            try
            {
                var repository1 = context.RepositoryFor();
                var repositoryUser = context.RepositoryFor<User>();
                var repositoryDept = context.RepositoryFor<Dept>();
                context.Begin();
                var deptInfo = repository1.QueryFirst<DeptInfo>("SELECT A.DeptId,B.PersonTotal FROM Coin_User A,Coin_Dept B WHERE A.DeptId=B.Id AND A.Id=@UniqueId", new { UniqueId = 1 });
                repositoryUser.Delete(new User { UniqueId = 1 });
                repositoryDept.Update(f => f.PersonTotal, new Dept { UniqueId = deptInfo.DeptId, PersonTotal = deptInfo.PersonTotal - 1 });
                context.Commit();
            }
            catch (Exception ex)
            {
                context.Rollback();
            }
            var reader = repository.QueryMultiple("SELECT * FROM Coin_User;SELECT * FROM Coin_Dept", user);
            var userList = reader.ReadList<User>();
            var deptList = reader.ReadPageList<Dept>();
            count = repository.Create(user);
            user.UserName = "Kevin-Test";
            user.Sex = Sex.Female;
            count = repository.Update(f => f.Sex, user);
        }
        public static async Task TestAsync(string connString)
        {
            int count = 0;
            var user = new User { UniqueId = 1, UserName = "Kevin", Age = 28, Sex = Sex.Male, DeptId = 1, UpdatedAt = DateTime.Now };
            var user1 = new User { UniqueId = 2, UserName = "Cindy", Age = 24, Sex = Sex.Female, DeptId = 2, UpdatedAt = DateTime.Now };
            var repository = new Repository<User>(connString);

            //删除
            count = await repository.DeleteAsync(user);
            count = await repository.DeleteAsync(user1);

            //创建
            count = await repository.CreateAsync(user);
            count = await repository.CreateAsync(user1);

            //获取          
            user = await repository.QueryFirstAsync("SELECT Id UniqueId,UserName,Sex FROM Coin_User WHERE Id=@UniqueId", user);
            user = await repository.GetAsync(user);

            //更新
            user.UserName = "Kevin-Test";
            user.Sex = Sex.Female;
            count = await repository.UpdateAsync(f => f.Sex, user);
            count = await repository.UpdateAsync(f => new { f.UserName, f.Sex }, user);

            //动态SQL更新
            var builder = new SqlBuilder();
            builder.RawSql("UPDATE Coin_User SET UserName=@UserName")
                    .AddField(user.Sex.HasValue, "Sex=@Sex")
                    .AddSql("WHERE Id=@UniqueId")
                    .Where(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt");
            count = await repository.UpdateAsync(builder.BuildSql(), user);

            count = await repository.UpdateAsync(f => f.RawSql("UPDATE Coin_User SET UserName=@UserName")
                    .AddField(user.Sex.HasValue, "Sex=@Sex")
                    .AddSql("WHERE Id=@UniqueId")
                    .Where(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt"), user);

            //查询&动态SQL
            var list = await repository.QueryAsync(f => f.RawSql("SELECT * FROM Coin_User")
                    .Where(user.Sex.HasValue, "Sex=@Sex")
                    .Where(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt")
                    .AddSql("ORDER BY UpdatedAt DESC"), user);

            //分页
            var userInfoList = await repository.QueryPageAsync<UserInfo>("SELECT Id UniqueId,UserName,Sex FROM Coin_User WHERE Id>@UniqueId", 0, 10, "ORDER BY Id", user);

            //事务
            var context = new RepositoryContext(connString);
            try
            {
                var repository1 = context.RepositoryFor();
                var repositoryUser = context.RepositoryFor<User>();
                var repositoryDept = context.RepositoryFor<Dept>();
                context.Begin();
                var deptInfo = await repository1.QueryFirstAsync<DeptInfo>("SELECT A.DeptId,B.PersonTotal FROM Coin_User A,Coin_Dept B WHERE A.DeptId=B.Id AND A.Id=@UniqueId", new { UniqueId = 1 });
                count = await repositoryUser.DeleteAsync(new User { UniqueId = 1 });
                count = await repositoryDept.UpdateAsync(f => f.PersonTotal, new Dept { UniqueId = deptInfo.DeptId, PersonTotal = deptInfo.PersonTotal - 1 });
                context.Commit();
            }
            catch
            {
                context.Rollback();
            }
            //多结果集
            var reader = await repository.QueryMultipleAsync("SELECT * FROM Coin_User;SELECT * FROM Coin_Dept", user);
            var userList = await reader.ReadListAsync<User>();
            var deptList = await reader.ReadPageListAsync<Dept>();
            count = await repository.CreateAsync(user);
            user.UserName = "Kevin-Test";
            user.Sex = Sex.Female;
            count = await repository.UpdateAsync(f => f.Sex, user);
        }
        public class UserInfo
        {
            public int UniqueId { get; set; }
            public string UserName { get; set; }
            public Sex Sex { get; set; }
        }
        [Table("Coin_User")]
        public class User
        {
            [PrimaryKey("Id")]
            public int UniqueId { get; set; }
            public string UserName { get; set; }
            [Column(typeof(string))]
            public Sex? Sex { get; set; }
            public int Age { get; set; }
            public int DeptId { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
        [Table("Coin_Dept")]
        public class Dept
        {
            [PrimaryKey("Id")]
            public int UniqueId { get; set; }
            public string DeptName { get; set; }
            public int PersonTotal { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
        public class DeptInfo
        {
            public int DeptId { get; set; }
            public int PersonTotal { get; set; }
        }
        public enum Sex : byte
        {
            Male = 1,
            Female = 2
        }
    }
}