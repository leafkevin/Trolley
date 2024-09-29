using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Xunit;

namespace Trolley.Test;

public class UnitTest1
{
    [Fact]
    public void IsEntityType()
    {
        Assert.False(typeof(Sex).IsEntityType(out _));
        Assert.False(typeof(Sex?).IsEntityType(out _));
        Assert.True(typeof(Studuent).IsEntityType(out _));
        Assert.False(typeof(string).IsEntityType(out _));
        Assert.False(typeof(int).IsEntityType(out _));
        Assert.False(typeof(int?).IsEntityType(out _));
        Assert.False(typeof(Guid).IsEntityType(out _));
        Assert.False(typeof(Guid?).IsEntityType(out _));
        Assert.False(typeof(DateTime).IsEntityType(out _));
        Assert.False(typeof(DateTime?).IsEntityType(out _));
        Assert.False(typeof(byte[]).IsEntityType(out _));
        Assert.False(typeof(int[]).IsEntityType(out _));
        Assert.False(typeof(List<int>).IsEntityType(out _));
        Assert.False(typeof(List<int[]>).IsEntityType(out _));
        Assert.False(typeof(Collection<string>).IsEntityType(out _));
        Assert.False(typeof(DBNull).IsEntityType(out _));

        var vt1 = ("kevin");
        Assert.False(vt1.GetType().IsEntityType(out _));
        var vt2 = (1, "kevin", 25, 30000.00d);
        Assert.True(vt2.GetType().IsEntityType(out _));
        Assert.True(typeof((string Name, int Age)).IsEntityType(out _));
        Assert.True(typeof(Dictionary<string, int>).IsEntityType(out _));
        Assert.True(typeof(Studuent).IsEntityType(out _));
        Assert.True(typeof(Teacher).IsEntityType(out _));

        Assert.True(typeof(Dictionary<string, int>[]).IsEntityType(out _));
        Assert.True(typeof(List<Dictionary<string, int>>).IsEntityType(out _));
        Assert.True(typeof(List<Dictionary<string, int>[]>).IsEntityType(out _));
        Assert.True(typeof(Collection<Dictionary<string, int>>).IsEntityType(out _));
        Assert.True(typeof(Dictionary<string, Dictionary<string, int>>).IsEntityType(out _));

        Assert.True(typeof(Teacher[]).IsEntityType(out _));
        Assert.True(typeof(List<Teacher>).IsEntityType(out _));
        Assert.True(typeof(List<Teacher[]>).IsEntityType(out _));
        Assert.True(typeof(Collection<Teacher>).IsEntityType(out _));
        Assert.True(typeof(Dictionary<string, Teacher>).IsEntityType(out _));

        Assert.True(typeof(Studuent[]).IsEntityType(out _));
        Assert.True(typeof(List<Studuent>).IsEntityType(out _));
        Assert.True(typeof(List<Studuent[]>).IsEntityType(out _));
        Assert.True(typeof(Collection<Studuent>).IsEntityType(out _));
        Assert.True(typeof(Dictionary<string, Studuent>).IsEntityType(out _));
    }
    [Fact]
    public void PredicateExpression()
    {
        int productId = 1;
        var expression1 = PredicateBuilder.Create<User, Company, Order>()
            .And((a, b, c) => a.Name.Contains("kevin"))
            .And((a, b, c) => a.CompanyId == b.Id)
            .AndMerge(f => f.Or(productId == 1, (a, b, c) => c.CreatedAt.Date == DateTime.Parse("2023-12-01"))
                .Or(productId == 2, (a, b, c) => c.CreatedAt > DateTime.Parse("2023-12-01") && c.CreatedAt < DateTime.Parse("2023-12-07"))
                .Or(productId == 3, (a, b, c) => c.CreatedAt > DateTime.Parse("2023-12-01") && c.CreatedAt < DateTime.Parse("2023-12-31")))
            .Build();
        Expression<Func<User, Company, Order, bool>> predicate = (a, b, c) =>
            a.Name.Contains("kevin") && a.CompanyId == b.Id && c.CreatedAt.Date == DateTime.Parse("2023-12-01");
        Assert.True(expression1.ToString() == predicate.ToString());

        productId = 2;
        var expression2 = PredicateBuilder.Create<User, Company, Order>()
            .Or(productId == 1, (a, b, c) => c.CreatedAt.Date == DateTime.Parse("2023-12-01"))
            .Or(productId == 2, (a, b, c) => c.CreatedAt > DateTime.Parse("2023-12-01") && c.CreatedAt < DateTime.Parse("2023-12-07"))
            .Or(productId == 3, (a, b, c) => c.CreatedAt > DateTime.Parse("2023-12-01") && c.CreatedAt < DateTime.Parse("2023-12-31"))
            .And((a, b, c) => a.Name.Contains("kevin"))
            .And((a, b, c) => a.CompanyId == b.Id)
            .AndMerge(f => f.Or((a, b, c) => a.CreatedAt.Date == DateTime.Parse("2023-12-01"))
                .Or((a, b, c) => a.CreatedAt > DateTime.Today.AddDays(-7) && a.CreatedAt > DateTime.Parse("2023-12-01") && a.CreatedAt < DateTime.Parse("2023-12-07"))
                .Or((a, b, c) => a.CreatedAt > DateTime.Today.AddDays(-30) && a.CreatedAt > DateTime.Parse("2023-12-01") && a.CreatedAt < DateTime.Parse("2023-12-31")))
            .Build();
        predicate = (a, b, c) => c.CreatedAt > DateTime.Parse("2023-12-01") && c.CreatedAt < DateTime.Parse("2023-12-07")
            && a.Name.Contains("kevin") && a.CompanyId == b.Id && (a.CreatedAt.Date == DateTime.Parse("2023-12-01")
                || a.CreatedAt > DateTime.Today.AddDays(-7) && a.CreatedAt > DateTime.Parse("2023-12-01") && a.CreatedAt < DateTime.Parse("2023-12-07")
                || a.CreatedAt > DateTime.Today.AddDays(-30) && a.CreatedAt > DateTime.Parse("2023-12-01") && a.CreatedAt < DateTime.Parse("2023-12-31"));
        Assert.True(expression2.ToString() == predicate.ToString());
    }
}
