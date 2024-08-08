using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Trolley.SqlServer;
using Xunit;

namespace Trolley.Test.SqlServer;

public class ExpressionUnitTest : UnitTestBase
{
    public ExpressionUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register<SqlServerProvider>("fengling", "Server=172.16.30.190;Database=fengling;Uid=sa;password=SQLserverSA123456;TrustServerCertificate=true", true)
            .Configure<SqlServerProvider, ModelConfiguration>();
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
        string firstName = "千", lastName = null;
        var sql = repository.From<User>()
            .Where(f => f.Name.Contains(lastName ?? firstName))
            .Select(f => new { HasName = f.Name ?? "NoName" })
            .ToSql(out var dbParameters);
        Assert.True(sql == "SELECT COALESCE(a.[Name],N'NoName') AS [HasName] FROM [sys_user] a WHERE a.[Name] LIKE '%'+@p0+'%'");
        Assert.True(dbParameters[0].Value.ToString() == firstName);

        repository.BeginTransaction();
        var count = repository.Update<User>(new { Id = 1, Name = "千叶111" });
        var result = repository.From<User>()
            .Where(f => f.Name.Contains(lastName ?? firstName))
            .Select(f => new { f.Id, HasName = f.Name ?? "NoName" })
            .ToList();
        repository.Commit();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
        Assert.True(result.Exists(f => f.Id == 1));

        sql = repository.From<User>()
            .Where(f => (f.Name ?? f.Id.ToString()) == "leafkevin")
            .Select(f => f.Id)
            .ToSql(out dbParameters);
        Assert.True(sql == "SELECT a.[Id] FROM [sys_user] a WHERE COALESCE(a.[Name],CAST(a.[Id] AS NVARCHAR(MAX)))=N'leafkevin'");
        repository.BeginTransaction();
        count = repository.Update<User>(new { Id = 1, Name = "leafkevin" });
        var result1 = repository.From<User>()
            .Where(f => (f.Name ?? f.Id.ToString()) == "leafkevin")
            .Select(f => f.Id)
            .ToList();
        repository.Commit();
        Assert.NotNull(result);
        Assert.True(result.Exists(f => f.Id == 1));
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
        Assert.True(sql == "SELECT (CASE WHEN a.[IsEnabled]=1 THEN N'Enabled' ELSE N'Disabled' END) AS [IsEnabled],(CASE WHEN a.[GuidField] IS NOT NULL THEN N'HasValue' ELSE N'NoValue' END) AS [GuidField],(CASE WHEN a.[Age]>35 THEN 1 ELSE 0 END) AS [IsOld],(CASE WHEN a.[Name] LIKE N'%kevin%' THEN N'Yes' ELSE N'No' END) AS [IsNeedParameter] FROM [sys_user] a WHERE (CASE WHEN a.[IsEnabled]=1 THEN N'Enabled' ELSE N'Disabled' END)=N'Enabled' AND (CASE WHEN a.[GuidField] IS NOT NULL THEN N'HasValue' ELSE N'NoValue' END)=N'HasValue'");

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
        Assert.True(sql == "SELECT (CASE WHEN a.[IsEnabled]=1 THEN @p4 ELSE N'Disabled' END) AS [IsEnabled],(CASE WHEN a.[GuidField] IS NOT NULL THEN @p5 ELSE N'NoValue' END) AS [GuidField],(CASE WHEN a.[Age]>35 THEN 1 ELSE 0 END) AS [IsOld],(CASE WHEN a.[Name] LIKE N'%kevin%' THEN N'Yes' ELSE N'No' END) AS [IsNeedParameter] FROM [sys_user] a WHERE (CASE WHEN a.[IsEnabled]=1 THEN @p0 ELSE N'Disabled' END)=@p1 AND (CASE WHEN a.[GuidField] IS NOT NULL THEN @p2 ELSE N'NoValue' END)=@p3");
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
        Assert.True(sql1 == "SELECT a.[Id],a.[Name],a.[Nature],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_company] a WHERE COALESCE(a.[Nature],N'Internet')=N'Internet'");
        var result1 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.True(result1.Count >= 2);
        Assert.True((result1[0].Nature ?? CompanyNature.Internet) == CompanyNature.Internet);

        var localNature = CompanyNature.Internet;
        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.True(sql2 == "SELECT a.[Id],a.[Name],a.[Nature],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_company] a WHERE COALESCE(a.[Nature],N'Internet')=@p0");
        Assert.True((string)dbParameters[0].Value == localNature.ToString());
        Assert.True(dbParameters[0].Value.GetType() == typeof(string));
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.True(result2.Count >= 2);
        Assert.True((result2[0].Nature ?? CompanyNature.Internet) == localNature);

        var sql3 = repository.From<Company>()
        .Where(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature)
        .ToSql(out dbParameters);
        Assert.True(sql3 == "SELECT a.[Id],a.[Name],a.[Nature],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_company] a WHERE (CASE WHEN a.[IsEnabled]=1 THEN a.[Nature] ELSE N'Internet' END)=@p0");
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
        Assert.True(sql == "SELECT @p2 AS [False],@p3 AS [Unknown],(@p4+' and '+@p5) AS [MyLove] FROM [sys_user] a WHERE a.[Name] LIKE '%'+@p0+'%' OR CAST(a.[IsEnabled] AS NVARCHAR(MAX))=@p1");
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

