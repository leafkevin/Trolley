using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using Trolley;

namespace ConsoleAppTest;

enum Sex
{
    Male,
    Female
}
class Program
{
    static void Main(string[] args)
    {
        int? gender = 2;
        object parameterValue = gender;

        object npgSqlNativeDbType = NpgsqlDbType.Smallint;
        var npgSqlParameter = new NpgsqlParameter("@Gender", (NpgsqlDbType)npgSqlNativeDbType);
        var npgSqlValueParameter = new NpgsqlParameter("@Gender", parameterValue);
        var npgSqlDbType = npgSqlParameter.DbType;
        var npgSqlValueDbType = npgSqlValueParameter.DbType;

        object mySqlNativeDbType = MySqlDbType.Int16;
        var mysqlParameter = new MySqlParameter("@Gender", (MySqlDbType)mySqlNativeDbType);
        var mysqlValueParameter = new MySqlParameter("@Gender", parameterValue);
        var mySqlDbType = mysqlParameter.DbType;
        var mySqlValueDbType = mysqlValueParameter.DbType;

        object sqlNativeDbType = SqlDbType.SmallInt;
        var sqlParameter = new SqlParameter("@Gender", (SqlDbType)sqlNativeDbType);
        var sqlValueParameter = new SqlParameter("@Gender", parameterValue);
        var sqlDbType = sqlParameter.DbType;
        var sqlValueDbType = sqlValueParameter.DbType;



        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register("fengling", true, f =>
            {
                var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
                f.Add<MySqlProvider>(connectionString, true)
                 .Configure(new ModelConfiguration());
            })
            .AddTypeHandler<JsonTypeHandler>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        var dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
}

