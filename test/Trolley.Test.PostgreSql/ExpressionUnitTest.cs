using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Trolley.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.PostgreSql;

public class ExpressionUnitTest : UnitTestBase
{
    private static int connTotal = 0;
    private static int connOpenTotal = 0;
    private readonly ITestOutputHelper output;
    public ExpressionUnitTest(ITestOutputHelper output)
    {
        this.output = output;
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
                .Register(OrmProviderType.PostgreSql, "fengling", "Host=localhost;Database=fengling;Username=postgres;Password=123456;SearchPath=public", true, "public")
                .Configure<ModelConfiguration>(OrmProviderType.PostgreSql)
                .UseDbFilter(df =>
                {
                    df.OnConnectionCreated += evt =>
                    {
                        Interlocked.Increment(ref connTotal);
                        this.output.WriteLine($"{evt.ConnectionId} Created, Total:{Volatile.Read(ref connTotal)}");
                    };
                    df.OnConnectionOpened += evt =>
                    {
                        Interlocked.Increment(ref connOpenTotal);
                        this.output.WriteLine($"{evt.ConnectionId} Opened, Total:{Volatile.Read(ref connOpenTotal)}");
                    };
                    df.OnConnectionClosed += evt =>
                    {
                        Interlocked.Decrement(ref connOpenTotal);
                        this.output.WriteLine($"{evt.ConnectionId} Closed, Total:{Volatile.Read(ref connOpenTotal)}");
                    };
                    df.OnCommandExecuting += evt =>
                    {
                        this.output.WriteLine($"{evt.SqlType} Begin, Sql: {evt.Sql}");
                    };
                    df.OnCommandExecuted += evt =>
                    {
                        this.output.WriteLine($"{evt.SqlType} End, Elapsed: {evt.Elapsed} ms, Sql: {evt.Sql}");
                    };
                });
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        dbFactory = serviceProvider.GetService<IOrmDbFactory>();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
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
            .ToSql(out var dbParameters);
        Assert.True(sql == "SELECT COALESCE(a.\"Name\",'NoName') AS \"HasName\" FROM \"sys_user\" a WHERE a.\"Name\" LIKE CONCAT('%',@p0,'%')");
        Assert.True(dbParameters[0].Value.ToString() == firstName);

        sql = repository.From<User>()
            .Where(f => (f.Name ?? f.Id.ToString()) == "leafkevin")
            .Select(f => f.Id)
            .ToSql(out dbParameters);
        Assert.True(sql == "SELECT a.\"Id\" FROM \"sys_user\" a WHERE COALESCE(a.\"Name\",CAST(a.\"Id\" AS VARCHAR))='leafkevin'");
    }
    [Fact]
    public void Conditional()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => (f.IsEnabled ? "Enabled" : "Disabled") == "Enabled"
                && (f.GuidField.HasValue ? "HasValue" : "NoValue") == "HasValue")
            .Select(f => new
            {
                IsEnabled = f.IsEnabled ? "Enabled" : "Disabled",
                GuidField = f.GuidField.HasValue ? "HasValue" : "NoValue",
                IsOld = f.Age > 35 ? true : false,
                IsNeedParameter = f.Name.Contains("kevin") ? "Yes" : "No",
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT (CASE WHEN a.\"IsEnabled\"=TRUE THEN 'Enabled' ELSE 'Disabled' END) AS \"IsEnabled\",(CASE WHEN a.\"GuidField\" IS NOT NULL THEN 'HasValue' ELSE 'NoValue' END) AS \"GuidField\",(CASE WHEN a.\"Age\">35 THEN TRUE ELSE FALSE END) AS \"IsOld\",(CASE WHEN a.\"Name\" LIKE '%kevin%' THEN 'Yes' ELSE 'No' END) AS \"IsNeedParameter\" FROM \"sys_user\" a WHERE (CASE WHEN a.\"IsEnabled\"=TRUE THEN 'Enabled' ELSE 'Disabled' END)='Enabled' AND (CASE WHEN a.\"GuidField\" IS NOT NULL THEN 'HasValue' ELSE 'NoValue' END)='HasValue'");

        var enabled = "Enabled";
        var hasValue = "HasValue";
        sql = repository.From<User>()
            .Where(f => (f.IsEnabled ? enabled : "Disabled") == enabled
                && (f.GuidField.HasValue ? hasValue : "NoValue") == hasValue)
            .Select(f => new
            {
                IsEnabled = f.IsEnabled ? enabled : "Disabled",
                GuidField = f.GuidField.HasValue ? hasValue : "NoValue",
                IsOld = f.Age > 35 ? true : false,
                IsNeedParameter = f.Name.Contains("kevin") ? "Yes" : "No",
            })
            .ToSql(out var dbParameters);
        Assert.True(sql == "SELECT (CASE WHEN a.\"IsEnabled\"=TRUE THEN @p4 ELSE 'Disabled' END) AS \"IsEnabled\",(CASE WHEN a.\"GuidField\" IS NOT NULL THEN @p5 ELSE 'NoValue' END) AS \"GuidField\",(CASE WHEN a.\"Age\">35 THEN TRUE ELSE FALSE END) AS \"IsOld\",(CASE WHEN a.\"Name\" LIKE '%kevin%' THEN 'Yes' ELSE 'No' END) AS \"IsNeedParameter\" FROM \"sys_user\" a WHERE (CASE WHEN a.\"IsEnabled\"=TRUE THEN @p0 ELSE 'Disabled' END)=@p1 AND (CASE WHEN a.\"GuidField\" IS NOT NULL THEN @p2 ELSE 'NoValue' END)=@p3");
        Assert.True(dbParameters.Count == 6);
        Assert.True(dbParameters[0].Value.ToString() == enabled);
        Assert.True(dbParameters[1].Value.ToString() == enabled);
        Assert.True(dbParameters[2].Value.ToString() == hasValue);
        Assert.True(dbParameters[3].Value.ToString() == hasValue);
        Assert.True(dbParameters[4].Value.ToString() == enabled);
        Assert.True(dbParameters[5].Value.ToString() == hasValue);

        var result = repository.From<User>()
            .Where(f => (f.IsEnabled ? enabled : "Disabled") == enabled
                && (f.GuidField.HasValue ? hasValue : "NoValue") == hasValue)
            .Select(f => new
            {
                IsEnabled = f.IsEnabled ? enabled : "Disabled",
                GuidField = f.GuidField.HasValue ? hasValue : "NoValue",
                IsOld = f.Age > 35 ? true : false,
                IsNeedParameter = f.Name.Contains("kevin") ? "Yes" : "No",
            })
            .ToList();
        Assert.NotNull(result);
    }
    [Fact]
    public async void WhereCoalesceConditional()
    {
        this.Initialize();
        using var repository = dbFactory.Create();
        var sql1 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.True(sql1 == "SELECT a.\"Id\",a.\"Name\",a.\"Nature\",a.\"IsEnabled\",a.\"CreatedAt\",a.\"CreatedBy\",a.\"UpdatedAt\",a.\"UpdatedBy\" FROM \"sys_company\" a WHERE COALESCE(a.\"Nature\",'Internet')='Internet'");
        var result1 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);
        Assert.True((result1[0].Nature ?? CompanyNature.Internet) == CompanyNature.Internet);

        var localNature = CompanyNature.Internet;
        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.True(sql2 == "SELECT a.\"Id\",a.\"Name\",a.\"Nature\",a.\"IsEnabled\",a.\"CreatedAt\",a.\"CreatedBy\",a.\"UpdatedAt\",a.\"UpdatedBy\" FROM \"sys_company\" a WHERE COALESCE(a.\"Nature\",'Internet')=@p0");
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result2.Count >= 2);
        Assert.True((result2[0].Nature ?? CompanyNature.Internet) == localNature);

        var sql3 = repository.From<Company>()
        .Where(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature)
        .ToSql(out dbParameters);
        Assert.True(sql3 == "SELECT a.\"Id\",a.\"Name\",a.\"Nature\",a.\"IsEnabled\",a.\"CreatedAt\",a.\"CreatedBy\",a.\"UpdatedAt\",a.\"UpdatedBy\" FROM \"sys_company\" a WHERE (CASE WHEN a.\"IsEnabled\"=TRUE THEN a.\"Nature\" ELSE 'Internet' END)=@p0");
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result3 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.True(result3.Count >= 2);
        Assert.True((result3[0].Nature ?? CompanyNature.Internet) == localNature);
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
            .ToSql(out var dbParameters);
        Assert.True(sql == "SELECT @p2 AS \"False\",@p3 AS \"Unknown\",CONCAT(@p4,' and ',@p5) AS \"MyLove\" FROM \"sys_user\" a WHERE a.\"Name\" LIKE CONCAT('%',@p0,'%') OR CAST(a.\"IsEnabled\" AS VARCHAR)=@p1");
        Assert.True(dbParameters.Count == 6);
        Assert.True((string)dbParameters[0].Value == dict["1"]);
        Assert.True((string)dbParameters[1].Value == strCollection[0]);
        Assert.True((string)dbParameters[2].Value == strArray[2]);
        Assert.True((string)dbParameters[3].Value == strCollection[2]);
        Assert.True((string)dbParameters[4].Value == dict["2"]);
        Assert.True((string)dbParameters[5].Value == dict["3"]);

        var result = repository.From<User>()
            .Where(f => (f.Name.Contains(dict["1"]) || f.IsEnabled.ToString() == strCollection[0]))
            .Select(f => new
            {
                False = strArray[2],
                Unknown = strCollection[2],
                MyLove = dict["2"] + " and " + dict["3"]
            })
            .ToList();
        Assert.NotNull(result);
    }
}

