using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Trolley;
using Trolley.Attributes;
using Trolley.Providers;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var connString = "Server=.;initial catalog=Coin;user id=sa;password=angangyur;Connect Timeout=30";
            OrmProviderFactory.RegisterProvider(connString, new SqlServerProvider(), true);

            Test();
            int sdfds = 0;
        }
        public static void Test()
        {
            var repository = new Repository<Account>();
            var user = repository.Create(new Account { UserName = "kevin" });
            //var repository = new Repository<User>();
            //var user = repository.Get(new User { UniqueId = 1 });
            ////user.Sex = Sex.Male;
            //repository.Update(f => f.RawSql("UPDATE Coin_Test SET UserName=@UserName WHERE ID=@UniqueId"), new Account { UniqueId = 1, UserName = "Cindy" });
            //var list = repository.QueryPage("SELECT Id UniqueId FROM (SELECT * FROM COIN_USER WHERE SEX='Male' Union all SELECT * FROM COIN_USER WHERE SEX='Female') t", 1, 10, "order by id");
            //var ss = list[0].Sex;
            //var ss1s = ss == Sex.Male || ss == Sex.Female;
            int sdfds = 0;
        }
        public static async Task TestAsync()
        {
            var repository = new Repository<User>();
            var user = await repository.GetAsync(new User { UniqueId = 1 });
            int sdfds = 0;
        }
    }
    [Table("Coin_Test")]
    public class Account
    {
        [PrimaryKey("Id", AutoIncrement = true)]
        public int UniqueId { get; set; }
        public string UserName { get; set; }
    }
    [Table("Coin_User")]
    public class User
    {
        [PrimaryKey("Id")]
        public int UniqueId { get; set; }
        public string UserName { get; set; }
        [Column(typeof(string))]
        public Sex Sex { get; set; }
        public Guid UID { get; set; }
    }
    public enum Sex
    {
        Male = 1,
        Female = 2
    }
}
