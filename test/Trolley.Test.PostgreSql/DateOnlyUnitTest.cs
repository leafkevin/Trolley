#if NET6_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trolley.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.PostgreSql;

public class DateOnlyUnitTest : UnitTestBase
{
    private readonly ITestOutputHelper output;
    public DateOnlyUnitTest()
    {
        this.output = output;
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var connectionString = "Host=localhost;Database=fengling;Username=postgres;Password=123456;SearchPath=public";
            var builder = new OrmDbFactoryBuilder()
                .Register(OrmProviderType.PostgreSql, "fengling", f => f.Use(connectionString), true)
                .Configure<ModelConfiguration>(OrmProviderType.PostgreSql)
                .Configure<ModelConfiguration>("fengling")
                .UseInterceptors(df =>
                {
                    df.OnConnectionCreated += evt =>
                    {
                        Interlocked.Increment(ref connTotal);
                        this.output.WriteLine($"Connection {evt.ConnectionId} Created, Total:{Volatile.Read(ref connTotal)}");
                    };
                    df.OnConnectionOpened += evt =>
                    {
                        Interlocked.Increment(ref connOpenTotal);
                        this.output.WriteLine($"Connection {evt.ConnectionId} Opened, Total:{Volatile.Read(ref connOpenTotal)}");
                    };
                    df.OnConnectionClosed += evt =>
                    {
                        Interlocked.Decrement(ref connOpenTotal);
                        Interlocked.Decrement(ref connTotal);
                        this.output.WriteLine($"Connection {evt.ConnectionId} Closed, Total:{Volatile.Read(ref connOpenTotal)}");
                    };
                    df.OnCommandExecuting += evt =>
                    {
                        var builder = new StringBuilder();
                        foreach (var parameter in evt.DbParameters)
                        {
                            var dbParameter = parameter as NpgsqlParameter;
                            builder.Append($"{dbParameter.ParameterName}={dbParameter.Value},DbType={dbParameter.NpgsqlDbType}; ");
                        }
                        var parameters = builder.ToString();
                        this.output.WriteLine($"{evt.SqlType} Begin, TransactionId:{evt.TransactionId} Sql: {evt.Sql}, Parameters: {evt.DbParameters.ToPostgreSqlParametersString()}");
                    };
                    df.OnCommandExecuted += evt =>
                    {
                        this.output.WriteLine($"{evt.SqlType} End, TransactionId:{evt.TransactionId} Elapsed: {evt.Elapsed} ms, Sql: {evt.Sql}, Parameters: {evt.DbParameters.ToPostgreSqlParametersString()}");
                    };
                    df.OnTransactionCreated += evt =>
                    {
                        Interlocked.Increment(ref tranTotal);
                        this.output.WriteLine($"Transaction {evt.TransactionId} Created, Total:{Volatile.Read(ref tranTotal)}");
                    };
                    df.OnTransactionCompleted += evt =>
                    {
                        Interlocked.Decrement(ref tranTotal);
                        this.output.WriteLine($"Transaction {evt.TransactionId} {evt.Action} Completed, Transaction Total:{Volatile.Read(ref tranTotal)}");
                    };
                });
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Test]
    public async Task MemberAccess()
    {
        this.Initialize(2);
        var localDate = DateOnly.FromDateTime(DateTime.Parse("2023-05-06"));
        var repository = this.dbFactory.Create();
        var sql = repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                DateTime.Today,
                Today1 = DateOnly.FromDateTime(DateTime.Now),
                FromDayNumber = DateOnly.FromDayNumber(739081),
                localDate,
                DateOnly.MinValue,
                DateOnly.MaxValue,
                IsEquals = f.UpdatedAt.Equals(DateOnly.Parse("2023-03-25")),
                IsEquals1 = f.UpdatedAt.Equals(localDate),
                DateOnly.FromDateTime(DateTime.Now).DayNumber,
                DateOnly.FromDateTime(DateTime.Now).Day,
                DateOnly.FromDateTime(DateTime.Now).Month,
                DateOnly.FromDateTime(DateTime.Now).Year,
                DateOnly.FromDateTime(DateTime.Now).DayOfWeek
            })
            .ToSql(out var dbParameters);
        Assert.AreEqual("SELECT CURRENT_DATE AS \"Today\",(CURRENT_TIMESTAMP::DATE) AS \"Today1\",DATE '2024-07-15' AS \"FromDayNumber\",@p0 AS \"localDate\",DATE '0001-01-01' AS \"MinValue\",DATE '9999-12-31' AS \"MaxValue\",(a.\"UpdatedAt\"=DATE '2023-03-25') AS \"IsEquals\",(a.\"UpdatedAt\"=@p1) AS \"IsEquals1\",(CURRENT_TIMESTAMP::DATE-DATE '0001-01-01') AS \"DayNumber\",(EXTRACT(DAY FROM CURRENT_TIMESTAMP::DATE)::INT4) AS \"Day\",(EXTRACT(MONTH FROM CURRENT_TIMESTAMP::DATE)::INT4) AS \"Month\",(EXTRACT(YEAR FROM CURRENT_TIMESTAMP::DATE)::INT4) AS \"Year\",(EXTRACT(DOW FROM CURRENT_TIMESTAMP::DATE)::INT4) AS \"DayOfWeek\" FROM \"sys_user\" a WHERE a.\"Id\"=1", sql);
        Assert.AreEqual(2, dbParameters.Count);
        Assert.IsTrue(dbParameters[0].Value.GetType() == typeof(DateOnly));
        Assert.IsTrue(dbParameters[1].Value.GetType() == typeof(DateOnly));
        Assert.AreEqual(localDate, (DateOnly)dbParameters[0].Value);
        Assert.AreEqual(localDate, (DateOnly)dbParameters[1].Value);

        var result = await repository.From<User>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.UpdatedAt,
                DateTime.Today,
                Today1 = DateOnly.FromDateTime(DateTime.Now),
                FromDayNumber = DateOnly.FromDayNumber(739081),
                localDate,
                DateOnly.MinValue,
                DateOnly.MaxValue,
                IsEquals = f.UpdatedAt.Equals(DateOnly.Parse("2023-03-25")),
                IsEquals1 = f.UpdatedAt.Equals(localDate),
                DateOnly.FromDateTime(DateTime.Now).DayNumber,
                DateOnly.FromDateTime(DateTime.Now).Day,
                DateOnly.FromDateTime(DateTime.Now).Month,
                DateOnly.FromDateTime(DateTime.Now).Year,
                DateOnly.FromDateTime(DateTime.Now).DayOfWeek
            })
            .FirstAsync();
        Assert.IsTrue(result.MinValue == DateOnly.MinValue);
        Assert.IsTrue(result.MaxValue == DateOnly.MaxValue);
        Assert.IsTrue(result.Today == DateTime.UtcNow.Date);
        Assert.IsTrue(result.Today1 == DateOnly.FromDateTime(DateTime.UtcNow));
        Assert.IsTrue(result.localDate == localDate);
        Assert.IsTrue(result.IsEquals == result.UpdatedAt.Equals(DateTime.Parse("2023-03-25")));
        Assert.IsTrue(result.IsEquals1 == result.UpdatedAt.Equals(localDate));
        Assert.IsTrue(result.DayNumber == DateOnly.FromDateTime(DateTime.UtcNow).DayNumber);
        Assert.IsTrue(result.Day == DateOnly.FromDateTime(DateTime.UtcNow).Day);
        Assert.IsTrue(result.Month == DateOnly.FromDateTime(DateTime.UtcNow).Month);
        Assert.IsTrue(result.Year == DateOnly.FromDateTime(DateTime.UtcNow).Year);
        Assert.AreEqual(result.DayOfWeek, DateOnly.FromDateTime(DateTime.UtcNow).DayOfWeek);
    }
    [Test]
    public async Task AddCompareTo()
    {
        this.Initialize(2);
        var localDate = DateOnly.FromDateTime(DateTime.Parse("2023-05-06"));
        var repository = this.dbFactory.Create();
        var sql = repository.From<UpdateEntity2>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                AddDays = f.DateOnlyField.AddDays(30),
                AddMonths = f.DateOnlyField.AddMonths(5),
                AddYears = f.DateOnlyField.AddYears(2),
                CompareTo = f.DateOnlyField.CompareTo(localDate),
                Parse = DateOnly.Parse(localDate.ToString("yyyy-MM-dd")),
                ParseExact = DateOnly.ParseExact("05-07/2023", "MM-dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None)
            })
            .ToSql(out _);
        Assert.AreEqual("SELECT (a.\"DateOnlyField\"+30) AS \"AddDays\",((a.\"DateOnlyField\"+INTERVAL '1 MON'*5)::DATE) AS \"AddMonths\",((a.\"DateOnlyField\"+INTERVAL '1Y'*2)::DATE) AS \"AddYears\",(CASE WHEN a.\"DateOnlyField\"=@p0 THEN 0 WHEN a.\"DateOnlyField\">@p0 THEN 1 ELSE -1 END) AS \"CompareTo\",@p1 AS \"Parse\",DATE '2023-05-07' AS \"ParseExact\" FROM \"sys_update_entity\" a WHERE a.\"Id\"=1", sql);

        var now = DateTime.Now;
        var result = await repository.From<UpdateEntity2>()
            .Where(f => f.Id == 1)
            .Select(f => new
            {
                f.DateOnlyField,
                AddDays = f.DateOnlyField.AddDays(30),
                AddMonths = f.DateOnlyField.AddMonths(5),
                AddYears = f.DateOnlyField.AddYears(2),
                CompareTo = f.DateOnlyField.CompareTo(localDate),
                Parse = DateOnly.Parse(localDate.ToString("yyyy-MM-dd")),
                ParseExact = DateOnly.ParseExact("05-07/2023", "MM-dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None)
            })
            .FirstAsync();
        Assert.IsTrue(result.AddDays == result.DateOnlyField.AddDays(30));
        Assert.IsTrue(result.AddMonths == result.DateOnlyField.AddMonths(5));
        Assert.IsTrue(result.AddYears == result.DateOnlyField.AddYears(2));
        Assert.IsTrue(result.CompareTo == result.DateOnlyField.CompareTo(localDate));
        Assert.IsTrue(result.Parse == DateOnly.Parse(localDate.ToString("yyyy-MM-dd")));
        Assert.IsTrue(result.ParseExact == DateOnly.ParseExact("05-07/2023", "MM-dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None));
    }
}
#endif