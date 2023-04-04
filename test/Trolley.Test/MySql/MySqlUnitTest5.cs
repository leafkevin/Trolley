using Microsoft.Extensions.DependencyInjection;
using Trolley.Test.MySql;
using Xunit;

namespace Trolley.Test;

public class MySqlUnitTest5
{
    private readonly IOrmDbFactory dbFactory;
    public MySqlUnitTest5()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register("fengling", true, f =>
            {
                var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
                f.Add<MySqlProvider>(connectionString, true);
            })
            .AddTypeHandler<JsonTypeHandler>()
            .Configure<MySqlProvider, MySqlModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();

        var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
        var generator = new MySqlDatabaseGenerator(connectionString);
        //generator.CreateTable(typeof(Person));
        //generator.CreateTable(typeof(PersonWithAutoId));
        //generator.CreateTable(typeof(PersonWithNullableAutoId));
        //generator.CreateTable(typeof(EntityWithId));
        //generator.CreateTable(typeof(PersonWithAliasedAge));
        //generator.CreateTable(typeof(PersonUsingEnumAsInt));
        generator.CreateTable(typeof(PersonWithReferenceType));
        generator.CreateTable(typeof(TestProduct));
    }

    [Fact]
    public void API_MySql_Legacy_Examples_Async()
    {
          using var repository = this.dbFactory.Create();
        var sql = repository.From<Person>()
            .Where(x => x.Age > 40)
            .OrderBy(x => x.Id)
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id`,`FirstName`,`LastName`,`Age` FROM `Person` WHERE `Age`>40 ORDER BY `Id`");

        sql = repository.From<Person>()
            .Where(x => x.Age > 40)
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id`, `FirstName`, `LastName`, `Age` \nFROM `Person`\nWHERE (`Age` > @0)");

        sql = repository.From<Person>()
            .Where(x => x.Age == 42)
            .ToSql(out _);
        Assert.True(sql == "SELECT `Id`, `FirstName`, `LastName`, `Age` \nFROM `Person`\nWHERE (`Age` = @0)\nLIMIT 1");

        sql = repository.Create<PersonWithAutoId>()
            .WithBy(new PersonWithAutoId { FirstName = "Amy", Age = 27 })
            .ToSql(out _);
        Assert.True(sql == "INSERT INTO `PersonWithAutoId` (`FirstName`,`Age`) VALUES (@FirstName,@Age)");

        sql = repository.Create<PersonWithAutoId>()
            .WithBy(new PersonWithAutoId { FirstName = "Amy", Age = 27 })
            .ToSql(out _);
        Assert.True(sql == "INSERT INTO `PersonWithAutoId` (`FirstName`,`Age`) VALUES (@FirstName,@Age)");

        sql = repository.Update<Person>()
            .Set(f => new { FirstName = "JJ" })
            .ToSql(out _);
        Assert.True(sql == "UPDATE `Person` SET `FirstName`=@FirstName");

        sql = repository.Update<Person>()
            .Set(f => new { FirstName = "JJ" })
            .Where(x => x.FirstName == "Jimi")
            .ToSql(out _);
        Assert.True(sql == "UPDATE `Person` SET `FirstName`=@FirstName WHERE (`FirstName` = @0)");

        sql = repository.Delete<Person>()
            .Where(p => p.Age == 27)
            .ToSql(out _);
        Assert.True(sql == "DELETE FROM `Person` WHERE (`Age` = @0)");


        repository.From<Person>()
            .Where(x => x.Age > 40)
            .OrderBy(x => x.Id)
            .GroupBy(f => f.LastName)
            .Select((x, f) => x.GroupConcat(f.Age.ToString() + "-" + f.FirstName, ","))
            .ToSql(out _);
    }
}
