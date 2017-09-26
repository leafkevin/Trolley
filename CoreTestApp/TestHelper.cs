using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trolley;

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

            count = repository.Update(user);

            //动态SQL更新
            var builder = new SqlBuilder();
            builder.RawSql("UPDATE Coin_User SET UserName=@UserName")
                   .AndWhere(user.Sex.HasValue, "Sex=@Sex")
                   .RawSql("WHERE Id=@UniqueId")
                   .AndWhere(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt");
            count = repository.Update(builder.BuildSql(), user);

            count = repository.Update(f =>
            {
                f.RawSql("UPDATE Coin_User SET UserName=@UserName", user.UserName)
                 .AddField(user.Sex.HasValue, "Sex=@Sex", user.Sex)
                 .RawSql("WHERE Id=@UniqueId", user.UniqueId)
                 .AndWhere(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt", user.UpdatedAt);
            });

            //查询&动态SQL
            var list = repository.Query(f =>
                f.RawSql("SELECT * FROM Coin_User")
                 .AndWhere(user.Sex.HasValue, "Sex=@Sex", user.Sex)
                 .AndWhere(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt", user.UpdatedAt)
                 .RawSql("ORDER BY UpdatedAt DESC"));

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
            //多结果集
            var order = new Order { Id = 1 };
            var orderRepository = new Repository<Order>(connString);
            var sql = "SELECT * FROM Coin_Order WHERE Id=@Id;SELECT * FROM Coin_OrderLine WHERE OrderId=@Id";
            var reader = orderRepository.QueryMultiple(sql, order);
            order = reader.Read<Order>();
            order.Lines = reader.ReadList<OrderLine>();

            order = orderRepository.QueryMap(map =>
            {
                var result = map.Read<Order>();
                result.Lines = map.ReadList<OrderLine>();
                return result;
            }, sql, order);

            order.Number = "123456789";
            orderRepository.Update(f => f.Number, order);
        }
        public static async Task TestAsync(string connString)
        {
            int count = 0;
            var user = new User { UniqueId = 1, UserName = "Kevin", Age = 28, Sex = Sex.Male, DeptId = 1, UpdatedAt = DateTime.Now };
            var user1 = new User { UniqueId = 2, UserName = "Cindy", Age = 24, Sex = Sex.Female, DeptId = 2, UpdatedAt = DateTime.Now };
            var dept = new Dept { UniqueId = 1, DeptName = "IT", PersonTotal = 1, UpdatedAt = DateTime.Now };
            var dept1 = new Dept { UniqueId = 2, DeptName = "HR", PersonTotal = 1, UpdatedAt = DateTime.Now };
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

            count = await repository.UpdateAsync(user);

            //动态SQL更新
            var builder = new SqlBuilder();
            builder.RawSql("UPDATE Coin_User SET UserName=@UserName", user.UserName)
                    .AddField(user.Sex.HasValue, "Sex=@Sex", user.Sex)
                    .RawSql("WHERE Id=@UniqueId", user.UniqueId)
                    .AndWhere(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt", user.UpdatedAt);
            count = await repository.UpdateAsync(builder.BuildSql(), user);

            count = await repository.UpdateAsync(f => f.RawSql("UPDATE Coin_User SET UserName=@UserName", user.UserName)
                    .AddField(user.Sex.HasValue, "Sex=@Sex", user.Sex)
                    .RawSql("WHERE Id=@UniqueId", user.UniqueId)
                    .AndWhere(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt", user.UpdatedAt));

            //查询&动态SQL
            var list = await repository.QueryAsync(f =>
            {
                f.RawSql("SELECT * FROM Coin_User")
                 .AndWhere(user.Sex.HasValue, "Sex=@Sex")
                 .AndWhere(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt")
                 .RawSql("ORDER BY UpdatedAt DESC");
            });
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
                count = await repositoryDept.CreateAsync(dept);
                count = await repositoryDept.CreateAsync(dept1);

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
            var order = new Order { Id = 1 };
            var orderRepository = new Repository<Order>(connString);
            var sql = "SELECT * FROM Coin_Order WHERE Id=@Id;SELECT * FROM Coin_OrderLine WHERE OrderId=@Id";

            List<OrderLine> orderLineList = new List<OrderLine>();
            orderLineList.Add(new OrderLine
            {
                LineId = 1,
                ProductId = 1,
                Price = 15.5,
                Quantity = 2,
                Amount = 15.5 * 2
            });
            orderLineList.Add(new OrderLine
            {
                LineId = 2,
                ProductId = 2,
                Price = 20,
                Quantity = 2,
                Amount = 40
            });
            var order1 = new Order { Id = 1, BuyerId = 1, Number = "12345", Lines = orderLineList };
            count = await orderRepository.DeleteAsync(order1);
            count = await orderRepository.CreateAsync(order1);

            var reader = await orderRepository.QueryMultipleAsync(sql, order);
            order = reader.Read<Order>();
            order.Lines = reader.ReadList<OrderLine>();

            order = await orderRepository.QueryMapAsync(map =>
            {
                var result = map.Read<Order>();
                result.Lines = map.ReadList<OrderLine>();
                return result;
            }, sql, order);

            order.Number = "123456789";
            await orderRepository.UpdateAsync(f => f.Number, order);
        }
    }
}