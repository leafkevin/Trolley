using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Trolley.Test.MySqlConnector;

public class ExpressionUnitTest : UnitTestBase
{
    [SetUp]
    public void Setup()
    {
        var connectionString = "Server=192.168.61.67;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
        var builder = new OrmDbFactoryBuilder()
            .Register(OrmProviderType.MySql, "fengling", f => f.Use(connectionString), true)
            .UseMapping<ModelMappingConfiguration>(OrmProviderType.MySql)
            .UseInterceptor(new MyDbInterceptor());
        this.dbFactory = builder.Build();
        this.Initialize(1);
    }
    [Test]
    public void Coalesce()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        string firstName = "kevin", lastName = null;
        var sql = repository.From<User>()
            .Where(f => f.Name.Contains(lastName ?? firstName))
            .Select(f => new { HasName = f.Name ?? "NoName" })
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT COALESCE(a.`Name`,'NoName') AS `HasName` FROM `sys_user` a WHERE a.`Name` LIKE CONCAT('%',@p0,'%')", sql);
        Assert.AreEqual(dbParameters[0].Value.ToString(), firstName);

        repository.BeginTransaction();
        var count = repository.Update<User>(new { Id = 1, Name = "leafkevin111" });
        var result = repository.From<User>()
            .Where(f => f.Name.Contains(lastName ?? firstName))
            .Select(f => new { f.Id, HasName = f.Name ?? "NoName" })
            .ToList();
        repository.Commit();
        Assert.IsNotEmpty(result);
        Assert.IsTrue(result.Exists(f => f.Id == 1));

        sql = repository.From<User>()
            .Where(f => (f.Name ?? f.Id.ToString()) == "leafkevin")
            .Select(f => f.Id)
            .ToSql(out dbParameters);
        Assert.AreEqual("SELECT a.`Id` FROM `sys_user` a WHERE COALESCE(a.`Name`,CAST(a.`Id` AS CHAR))='leafkevin'", sql);
        repository.BeginTransaction();
        count = repository.Update<User>(new { Id = 1, Name = "leafkevin" });
        var result1 = repository.From<User>()
            .Where(f => (f.Name ?? f.Id.ToString()) == "leafkevin")
            .Select(f => f.Id)
            .ToList();
        repository.Commit();
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Exists(f => f.Id == 1));
    }
    [Test]
    public void Conditional()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
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
        Assert.AreEqual("SELECT (CASE WHEN a.`IsEnabled`=1 THEN 'Enabled' ELSE 'Disabled' END) AS `IsEnabled`,(CASE WHEN a.`GuidField` IS NOT NULL THEN 'HasValue' ELSE 'NoValue' END) AS `GuidField`,(CASE WHEN a.`Age`>35 THEN 1 ELSE 0 END) AS `IsOld`,(CASE WHEN INSTR('kevin',a.`Name`)>0 THEN 'Yes' ELSE 'No' END) AS `IsNeedParameter` FROM `sys_user` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN 'Enabled' ELSE 'Disabled' END)='Enabled' AND (CASE WHEN a.`GuidField` IS NOT NULL THEN 'HasValue' ELSE 'NoValue' END)='HasValue'", sql);

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
        Assert.AreEqual("SELECT (CASE WHEN a.`IsEnabled`=1 THEN @p4 ELSE 'Disabled' END) AS `IsEnabled`,(CASE WHEN a.`GuidField` IS NOT NULL THEN @p5 ELSE 'NoValue' END) AS `GuidField`,(CASE WHEN a.`Age`>35 THEN 1 ELSE 0 END) AS `IsOld`,(CASE WHEN INSTR('kevin',a.`Name`)>0 THEN 'Yes' ELSE 'No' END) AS `IsNeedParameter` FROM `sys_user` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN @p0 ELSE 'Disabled' END)=@p1 AND (CASE WHEN a.`GuidField` IS NOT NULL THEN @p2 ELSE 'NoValue' END)=@p3", sql);
        Assert.AreEqual(6, dbParameters.Count);
        Assert.AreEqual(enabled, dbParameters[0].Value.ToString());
        Assert.AreEqual(enabled, dbParameters[1].Value.ToString());
        Assert.AreEqual(hasValue, dbParameters[2].Value.ToString());
        Assert.AreEqual(hasValue, dbParameters[3].Value.ToString());
        Assert.AreEqual(enabled, dbParameters[4].Value.ToString());
        Assert.AreEqual(hasValue, dbParameters[5].Value.ToString());

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
        Assert.IsNotNull(result);
    }
    [Test]
    public async Task WhereCoalesceConditional()
    {
        this.Initialize(1);
        var repository = this.dbFactory.Create();
        var sql1 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet)
            .ToSql(out _);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')='Internet'", sql1);
        var result1 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == CompanyNature.Internet);
        Assert.GreaterOrEqual(result1.Count, 2);
        Assert.AreEqual(CompanyNature.Internet, (result1[0].Nature ?? CompanyNature.Internet));

        var localNature = CompanyNature.Internet;
        var sql2 = repository.From<Company>()
            .Where(f => (f.Nature ?? CompanyNature.Internet) == localNature)
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE COALESCE(a.`Nature`,'Internet')=@p0", sql2);
        Assert.AreEqual(localNature.ToString(), (string)dbParameters[0].Value);
        Assert.That(dbParameters[0].Value, Is.TypeOf<string>());
        var result2 = await repository.QueryAsync<Company>(f => (f.Nature ?? CompanyNature.Internet) == localNature);
        Assert.GreaterOrEqual(result2.Count, 2);
        Assert.AreEqual(localNature, (result2[0].Nature ?? CompanyNature.Internet));

        var sql3 = repository.From<Company>()
        .Where(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature)
        .ToSql(out dbParameters);
        Assert.AreEqual("SELECT a.`Id`,a.`Name`,a.`Nature`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_company` a WHERE (CASE WHEN a.`IsEnabled`=1 THEN a.`Nature` ELSE 'Internet' END)=@p0", sql3);
        Assert.AreEqual(localNature.ToString(), (string)dbParameters[0].Value);
        Assert.That(dbParameters[0].Value, Is.TypeOf<string>());
        var result3 = await repository.QueryAsync<Company>(f => (f.IsEnabled ? f.Nature : CompanyNature.Internet) == localNature);
        Assert.GreaterOrEqual(result3.Count, 2);
        Assert.AreEqual(localNature, (result3[0].Nature ?? CompanyNature.Internet));
    }
    [Test]
    public void Index()
    {
        this.Initialize(1);
        string[] strArray = { "True", "False", "Unknown" };
        var strCollection = new ReadOnlyCollection<string>(strArray);
        var dict = new Dictionary<string, string>
        {
            {"1","leafkevin" },
            {"2","cindy" },
            {"3","xiyuan" }
        };
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => (f.Name.Contains(dict["1"]) || f.IsEnabled.ToString() == strCollection[0]))
            .Select(f => new
            {
                False = strArray[2],
                Unknown = strCollection[2],
                MyLove = dict["2"] + " and " + dict["3"]
            })
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT @p2 AS `False`,@p3 AS `Unknown`,CONCAT(@p4,' and ',@p5) AS `MyLove` FROM `sys_user` a WHERE a.`Name` LIKE CONCAT('%',@p0,'%') OR CAST(a.`IsEnabled` AS CHAR)=@p1", sql);
        Assert.AreEqual(6, dbParameters.Count);
        Assert.AreEqual(dict["1"], (string)dbParameters[0].Value);
        Assert.AreEqual(strCollection[0], (string)dbParameters[1].Value);
        Assert.AreEqual(strArray[2], (string)dbParameters[2].Value);
        Assert.AreEqual(strCollection[2], (string)dbParameters[3].Value);
        Assert.AreEqual(dict["2"], (string)dbParameters[4].Value);
        Assert.AreEqual(dict["3"], (string)dbParameters[5].Value);

        var result = repository.From<User>()
            .Where(f => (f.Name.Contains(dict["1"]) || f.IsEnabled.ToString() == strCollection[0]))
            .Select(f => new
            {
                False = strArray[2],
                Unknown = strCollection[2],
                MyLove = dict["2"] + " and " + dict["3"]
            })
            .ToList();
        Assert.IsNotNull(result);
    }
}