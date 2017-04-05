using System;
using System.Data;
using System.Data.SqlClient;
using Trolley;
using Trolley.Providers;

namespace WinTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var sqlConnString = "Server=.;initial catalog=testdb;user id=test;password=123456;Connect Timeout=30";
            var npgsqlConnString = "Server=192.168.1.10;Port=5432;Database=testdb;User Id=test;Password=123456;Pooling=true;";
            var mysqlConnString = "Server=192.168.1.10;Database=testdb;Uid=test;Pwd=123456;Pooling=true;";
            OrmProviderFactory.RegisterProvider(sqlConnString, new SqlServerProvider());
            OrmProviderFactory.RegisterProvider(npgsqlConnString, new NpgsqlProvider());
            OrmProviderFactory.RegisterProvider(mysqlConnString, new MySqlProvider());

            string sql = @"SELECT DeptId FROM Coin_User WHERE Id=1;SELECT DeptName FROM Coin_Dept WHERE Id=1";
            SqlConnection conn = new SqlConnection(sqlConnString);
            SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();
            var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            do
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var name = reader.GetName(0);
                        var value = reader.GetValue(0);
                        Console.WriteLine();
                    }
                }
            } while (reader.NextResult());
            conn.Close();
            //TestHelper.Test(sqlConnString);
            TestHelper.TestAsync(sqlConnString).Wait();
            //TestHelper.Test(npgsqlConnString);
            //TestHelper.Test(mysqlConnString);
            Console.ReadLine();
        }

    }
}
