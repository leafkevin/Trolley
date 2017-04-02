using System;
using System.Threading.Tasks;
using Trolley;
using Trolley.Attributes;
using Trolley.Providers;

namespace ConsoleCoreTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var connString = "Server=.;initial catalog=Coin;user id=sa;password=test;Connect Timeout=30";
            OrmProviderFactory.RegisterProvider(connString, new SqlServerProvider(), true);
            Test();
            Console.ReadLine();
        }
        public static void Test()
        {
            var repository = new Repository<User>();
            DateTime? beginDate = DateTime.Parse("2017-01-01");
            var user = new User { UniqueId = 1, UserName = "Kevin", Sex = Sex.Male };

            repository.Update(f => f.Sex, user);
            repository.Update(f => new { f.UserName, f.Sex }, user);

            var builder = new SqlBuilder();
            builder.RawSql("UPDATE Coin_User SET UserName=@UserName")
                   .AddField(user.Sex.HasValue, "Sex=@Sex")
                   .AddSql("WHERE Id=@UniqueId")
                   .Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt");
            repository.Update(builder.BuildSql(), user);


            var count = repository.Update(f => f.RawSql("UPDATE Coin_User SET UserName=@UserName")
                    .AddField(user.Sex.HasValue, "Sex=@Sex")
                    .AddSql("WHERE Id=@UniqueId")
                    .Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt"), user);

            var list = repository.Query(f => f.RawSql("SELECT * FROM Coin_User")
                    .Where(user.Sex.HasValue, "Sex=@Sex")
                    .Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt")
                    .AddSql("ORDER BY UpdatedAt DESC"), user);

            repository.QueryFirst("SELECT UserName,Sex FROM User WHERE Id=@UniqueId", new User { UniqueId = 1 });

            var userInfoList = repository.QueryPage<UserInfo>("SELECT Id UniqueId,UserName,Sex FROM User WHERE Id>@UniqueId",
                0, 10, null, new User { UniqueId = 1 });


            var context = new RepositoryContext();
            var repository1 = context.RepositoryFor();
            var repositoryUser = context.RepositoryFor<User>();
            var repositoryDept = context.RepositoryFor<Dept>();

            context.Begin();
            var deptInfo = repository1.QueryFirst<DeptInfo>("SELECT A.DeptId,B.PersonTotal FORM Coin_User A,Coin_Dept B WHERE A.DeptId=B.Id AND A.Id=@UniqueId", new { UniqueId = 1 });
            repositoryUser.Delete(new User { UniqueId = 1 });
            repositoryDept.Update(f => f.PersonTotal, new Dept { UniqueId = deptInfo.DeptId, PersonTotal = deptInfo.PersonTotal });
            context.Commit();

            int dsfdf = 0;
        }
        public static async Task TestAsync()
        {
            var repository = new Repository<User>();
            var user = await repository.GetAsync(new User { UniqueId = 1 });
            int sdfds = 0;
        }
        public class UserInfo
        {
            public int UniqueId { get; set; }
            public string UserName { get; set; }
            public Sex Sex { get; set; }
        }
        public class User
        {
            [PrimaryKey("Id")]
            public int UniqueId { get; set; }
            public string UserName { get; set; }
            [Column(typeof(string))]
            public Sex? Sex { get; set; }
            public int DeptId { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
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