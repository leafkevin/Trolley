using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Trolley;
using Trolley.Providers;

namespace WinTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var sqlConnString = "Data Source=192.168.1.10;Initial Catalog=testdb;User Id=test;Password=123456;";
            var npgsqlConnString = "Server=192.168.1.10;Port=5432;Database=testdb;User Id=test;Password=123456;Pooling=true;";
            var mysqlConnString = "Server=192.168.1.10;Database=testdb;Uid=test;Pwd=123456;Pooling=true;";
            OrmProviderFactory.RegisterProvider(sqlConnString, new SqlServerProvider());
            OrmProviderFactory.RegisterProvider(npgsqlConnString, new NpgsqlProvider());
            OrmProviderFactory.RegisterProvider(mysqlConnString, new MySqlProvider());


            TestHelper.Test(sqlConnString);
            TestHelper.TestAsync(sqlConnString).Wait();
            TestHelper.TestAsync(npgsqlConnString).Wait();
            TestHelper.Test(mysqlConnString);
            Console.ReadLine();
        }
    }
}
