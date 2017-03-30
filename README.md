# Trolley - 一个轻量级高性能的.NET ORM框架
一款轻量级高性能的ORM，可以写SQL语句，支持动态SQL语句，分页，多种数据库终端支持，性能与dapper相当。

特点
--------

示例:

首先在系统中要注册OrmProvider，只需要注册一次
------------------------------------------------------------

通常一个连接串对应一个OrmProvider

```csharp
var connString = "Server=.;initial catalog=Coin;user id=sa;password=angangyur;Connect Timeout=30";
OrmProviderFactory.RegisterProvider(connString, new SqlServerProvider(), true);

```
其次，要创建Repository对象。
Example usage:

```csharp
public class Dog
{
    public int? Age { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public float? Weight { get; set; }

    public int IgnoredProperty { get { return 1; } }
}            
            
var guid = Guid.NewGuid();
var repository = new Repository();
var dog = repository.Query<Dog>("select Age = @Age, Id = @Id", new { Age = (int?)null, Id = guid });
```

也可以使用强类型的Repository仓储对象。

```csharp
var guid = Guid.NewGuid();
var repository = new Repository<Dog>();
var dog = repository.Query("select Age = @Age, Id = @Id", new { Age = (int?)null, Id = guid });
```

待续。。。

欢迎大家使用
---------------------
欢迎大家广提Issue，我的联系方式：
QQ：39253425
Mail：leafkevin@126.com





