using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        var vt1 = ValueTuple.Create("kevin");
        Assert.False(vt1.GetType().IsEntityType(out _));
        var vt2 = ValueTuple.Create(1, "kevin", 25, 30000.00d);
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
}
