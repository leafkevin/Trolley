// See https://aka.ms/new-console-template for more information
using MySqlConnector;
using NpgsqlTypes;
using System;
using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

//var dict = new Dictionary<string, string>
//{
//    { "1", "111" },
//    { "2", "222" }
//};
//var propertyInfo = typeof(Dictionary<string, string>).GetProperty("Item", new Type[] { typeof(string) });
//var indexExpr = Expression.MakeIndex(Expression.Constant(dict), propertyInfo, new[] { Expression.Constant("1") });
Sex? sex = Sex.Male;
object objEnum = sex;
var dddfsd = Convert.ChangeType(objEnum, typeof(Sex));

var timeSpan = TimeSpan.FromTicks(TimeSpan.TicksPerDay - 1).Add(TimeSpan.FromDays(29));
var sss = timeSpan.ToString("d\\ hh\\:mm\\:ss\\.fffffff");

var dsas = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(3.12345678)).Millisecond;
var o1 = TimeOnly.FromTimeSpan(TimeSpan.FromTicks(TimeSpan.TicksPerDay - 1)).ToString("o");
var o2 = TimeOnly.FromTimeSpan(TimeSpan.FromTicks(TimeSpan.TicksPerDay - 1)).ToString("O");
var r1 = TimeOnly.FromTimeSpan(TimeSpan.FromTicks(31234567)).ToString("r");
var r2 = TimeOnly.FromTimeSpan(TimeSpan.FromTicks(31234567)).ToString("R");
var ddds1 = TimeSpan.FromTicks(TimeSpan.TicksPerDay - 1).Add(TimeSpan.FromDays(29)).ToString("c");
var ddds2 = TimeSpan.FromTicks(TimeSpan.TicksPerDay - 1).Add(TimeSpan.FromDays(29)).ToString("g");
var ddds3 = TimeSpan.FromTicks(TimeSpan.TicksPerDay - 1).Add(TimeSpan.FromDays(29)).ToString("G");
var names = Enum.GetNames(typeof(SqlDbType));
var builder = new StringBuilder();
foreach (var name in names)
{
    builder.AppendLine($"nativeDbTypes[(int)Enum.Parse(dbTypeType, \"{name}\")] = Enum.Parse(dbTypeType, \"{name}\");");
}
var content = builder.ToString();
Console.WriteLine(TimeSpan.FromSeconds(124));
Console.WriteLine(DateTime.MinValue.ToString());

Console.WriteLine($"MinValue={long.MinValue}");
Console.WriteLine($"MinValue={long.MaxValue}");
object provider = CultureInfo.InvariantCulture;

//DateTime.ParseExact("2023-01-01", "yyyy-MM-dd", (IFormatProvider)provider);
//Convert.ToString(od);

var dd1 = string.Format("{0},ddd{1 }", "123", 455);
int sdfdsf = 0;
enum Sex
{
    Male = 1,
    Female = 2
}