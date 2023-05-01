using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Trolley.SqlServer;
using Xunit;

namespace Trolley.Test.SqlServer;

public class SqlServerExpressionUnitTest : UnitTestBase
{
    public SqlServerExpressionUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register("fengling", true, f =>
            {
                var connectionString = "Server=127.0.0.1;Database=fengling;Uid=sa;password=SQLserverSA123456;TrustServerCertificate=true";
                f.Add<SqlServerProvider>(connectionString, true);
            })
            .AddTypeHandler<JsonTypeHandler>()
            .Configure<SqlServerProvider, SqlServerModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }

    [Fact]
    public void Coalesce()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        string firstName = "kevin", lastName = null;
        var sql = repository.From<User>()
            .Where(f => f.Name.Contains(lastName ?? firstName))
            .Select(f => new { HasName = f.Name ?? "NoName" })
            .ToSql(out _);
        Assert.True(sql == "SELECT COALESCE([Name],'NoName') AS [HasName] FROM [sys_user] WHERE [Name] LIKE '%kevin%'");
    }
    [Fact]
    public void Conditional()
    {
        this.Initialize();
        string noParameter = "No";
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => (f.IsEnabled ? "Enabled" : "Disabled") == "Enabled")
            .Select(f => new
            {
                IsEnabled = f.IsEnabled ? "Enabled" : "Disabled",
                GuidField = f.GuidField.HasValue ? "HasValue" : "NoValue",
                IsOld = f.Age > 35 ? true : false,
                IsNeedParameter = f.Name.Contains("kevin") ? "Yes" : noParameter.ToParameter(),
            })
            .ToSql(out var parameters);
        Assert.True(sql == "SELECT (CASE WHEN [IsEnabled]=1 THEN 'Enabled' ELSE 'Disabled' END) AS [IsEnabled],(CASE WHEN [GuidField] IS NOT NULL THEN 'HasValue' ELSE 'NoValue' END) AS [GuidField],(CASE WHEN [Age]>35 THEN 1 ELSE 0 END) AS [IsOld],(CASE WHEN [Name] LIKE '%kevin%' THEN 'Yes' ELSE @p0 END) AS [IsNeedParameter] FROM [sys_user] WHERE (CASE WHEN [IsEnabled]=1 THEN 'Enabled' ELSE 'Disabled' END)='Enabled'");
        Assert.True((string)parameters[0].Value == noParameter);
    }
    [Fact]
    public void Index()
    {
        this.Initialize();
        string[] strArray = { "True", "False", "Unknown" };
        var strCollection = new ReadOnlyCollection<string>(strArray);
        var dict = new Dictionary<string, string>
        {
            {"1","leafkevin" },
            {"2","cindy" },
            {"3","xiyuan" }
        };
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => (f.Name.Contains(dict["1"]) || f.IsEnabled.ToString() == strCollection[0]))
            .Select(f => new
            {
                False = strArray[2],
                Unknown = strCollection[2],
                MyLove = dict["2"] + " and " + dict["3"]
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT 'Unknown' AS [False],'Unknown' AS [Unknown],'cindy and xiyuan' AS [MyLove] FROM [sys_user] WHERE [Name] LIKE '%leafkevin%' OR CAST([IsEnabled] AS NVARCHAR(MAX))='True'");
    }
}

