Trolley - 一个轻量级高性能的.NET ORM框架
========================================


框架特点
------------------------------------------------------------
支持分页，可以不用写SQL,也支持动态SQL，强类型的DDD仓促操作,也支持无类型的SQL操作,支持多种数据库终端。
目前支持.NET Standard1.5,.NET Framework 4.5+,.NET Core 1.0+ 等平台。
支持：MySql,Oracle,PostgreSql,Sql Sever 2012+,SqlLite
性能与dapper相当，使用强类型仓储使用sql比dapper快。
数据库支持：Sql Server 2012，Postgresql，测试通过。（Oracle，Mysql暂时还不支持.NET Core跨平台）

首先在系统中要注册OrmProvider，只需要注册一次
------------------------------------------------------------
通常一个连接串对应一个OrmProvider，注册放到全局初次加载的地方。

示例:

```csharp
var connString = "Server=.;Initial Catalog=test;User Id=sa;Password=test;Connect Timeout=30";
OrmProviderFactory.RegisterProvider(connString, new SqlServerProvider(), true);

```
也可以使用多个连接串进行注册

```csharp
var sqlConnString = "Server=.;Initial Catalog=test;User Id=sa;Password=test;Connect Timeout=30;";
OrmProviderFactory.RegisterProvider(sqlConnString, new SqlServerProvider(), true);

var psqlConnString = "Server=192.168.1.15;Port=5432;Database=test;User Id=postgres;Password=test;Pooling=true;";
OrmProviderFactory.RegisterProvider(psqlConnString, new PostgreSqlProvider());

```

其次，创建Repository对象或Repository<TEntity>对象。
------------------------------------------------------------
接下来就可以用这个仓储对象进行操作了。

示例:

```csharp
public class User
{
    [PrimaryKey("Id")]
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    [Column(typeof(string))]
    public Sex? Sex { get; set; }
    public Guid? CardId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
public enum Sex : byte
{
    Male = 1,
    Female = 2
}            

var repository = new Repository();

var user = repository.Query<User>("select Age = @Age, Id = @UniqueId", new User { Age = (int?)null, UniqueId = 1 });
user = await repository.QueryAsync<User>("select Age = @Age, Id = @UniqueId", new User { Age = (int?)null, UniqueId = 1 });

```


Repository<TEntity>仓储对象，在获取参数时，查询会比Repository无类型的仓储对象稍快一点。
Repository<TEntity>仓储对象，操作方法中，都是TEntity类型参数，获取数据会快。
Repository无类型的仓储对象，操作方法中，都是object匿名类型参数，获取数据要先获取元数据，同比强类型Repository<TEntity>仓储对象会慢一点。
如果有跨越多个实体对象，使用Repository无类型的仓储对象会合适一些。


```csharp
 public class User
{
    [PrimaryKey("Id")]
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    [Column(typeof(string))]
    public Sex? Sex { get; set; }
    public int DeptId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
public class Dept
{
    [PrimaryKey("Id")]
    public int UniqueId { get; set; }
    public string DeptName { get; set; }
    public int PersonTotal { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
public class DeptInfo
{
    public int DeptId { get; set; }
    public int PersonTotal { get; set; }
}

var repository = new Repository();
var deptInfo = repository.QueryFirst<DeptInfo>("SELECT A.DeptId,B.PersonTotal FORM Coin_User A,Coin_Dept B WHERE A.DeptId=B.Id AND A.Id=@UniqueId", new { UniqueId = 1 });

```

可以更改数据库连接串,跨库操作
------------------------------------------------------------

```csharp
var sqlConnString = "Server=.;Initial Catalog=test;User Id=sa;Password=test;Connect Timeout=30;";
var psqlConnString = "Server=192.168.1.15;Port=5432;Database=test;User Id=postgres;Password=test;Pooling=true;";

//默认使用的连接串
OrmProviderFactory.RegisterProvider(sqlConnString, new SqlServerProvider(), true);
OrmProviderFactory.RegisterProvider(psqlConnString, new PostgreSqlProvider());

var repository = new Repository(sqlConnString);
var user = repository.Query<User>("select Age = @Age, Id = @UniqueId", new User { Age = (int?)null, UniqueId = 1 });
user = await repository.QueryAsync<User>("select Age = @Age, Id = @UniqueId", new User { Age = (int?)null, UniqueId = 1 });

var repository = new Repository(psqlConnString);
var user = repository.Query<User>("select Age = @Age, Id = @UniqueId", new User { Age = (int?)null, UniqueId = 1 });
user = await repository.QueryAsync<User>("select Age = @Age, Id = @UniqueId", new User { Age = (int?)null, UniqueId = 1 });

```

也可以使用强类型的Repository仓储对象，操作更便捷。
------------------------------------------------------------

```csharp
var repository = new Repository<User>();

var user = repository.Query("select Age = @Age, Id = @UniqueId", new User { Age = (int?)null, UniqueId = 1 });
user = repository.QueryAsync("select Age = @Age, Id = @UniqueId", new User { Age = (int?)null, UniqueId = 1 });
```

大部分SQL操作都支持动态SQL
------------------------------------------------------------
多常见查询和更新，删除命令操作
查询，可以动态拼接SELECT语句和WHERE语句
更新，可以动态拼接UPDATE语句和WHERE语句
删除，可以动态拼接WHERE语句

```csharp
var repository = new Repository<User>();
DateTime? beginDate = DateTime.Parse("2017-01-01");
var user = new User { UniqueId = 1, UserName = "Kevin", Sex = Sex.Male };

var builder = new SqlBuilder();
builder.RawSql("UPDATE Coin_User SET UserName=@UserName")
		   .AddField(user.Sex.HasValue, "Sex=@Sex")
		   .AddSql("WHERE Id=@UniqueId")
		   .Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt");

//更新
repository.Update(builder.BuildSql(), user);
await repository.UpdateAsync(builder.BuildSql(), user);

//或者
var count = repository.Update(f => 
			f.RawSql("UPDATE Coin_User SET UserName=@UserName")
			.AddField(user.Sex.HasValue, "Sex=@Sex")
			.AddSql("WHERE Id=@UniqueId")
			.Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt"), user);
//查询
var list = repository.Query(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.Where(user.Sex.HasValue, "Sex=@Sex")
			.Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt")
			.AddSql("ORDER BY UpdatedAt DESC"), user);
```


所有SQL操作都支持异步
------------------------------------------------------------

```csharp
var repository = new Repository<User>();
DateTime? beginDate = DateTime.Parse("2017-01-01");
var user = new User { UniqueId = 1, UserName = "Kevin", Sex = Sex.Male };

//更新
await repository.UpdateAsync(builder.BuildSql(), user);

var count = await repository.UpdateAsync(f => f.RawSql("UPDATE Coin_User SET UserName=@UserName")
			f.RawSql("UPDATE Coin_User SET UserName=@UserName")
			.AddField(user.Sex.HasValue, "Sex=@Sex")
			.AddSql("WHERE Id=@UniqueId")
			.Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt"), user);

var list = repository.QueryAsync(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.Where(user.Sex.HasValue, "Sex=@Sex")
			.Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt")
			.AddSql("ORDER BY UpdatedAt DESC"), user);			
```


支持事务操作
------------------------------------------------------------

```csharp
 public class User
{
    [PrimaryKey("Id")]
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    [Column(typeof(string))]
    public Sex? Sex { get; set; }
    public int DeptId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
public class Dept
{
    [PrimaryKey("Id")]
    public int UniqueId { get; set; }
    public string DeptName { get; set; }
    public int PersonTotal { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
public class DeptInfo
{
    public int DeptId { get; set; }
    public int PersonTotal { get; set; }
}


var context = new RepositoryContext();

var repository = context.RepositoryFor();
var repositoryUser = context.RepositoryFor<User>();
var repositoryDept = context.RepositoryFor<Dept>();

//事务开始
context.Begin();
var deptInfo = repository.QueryFirst<DeptInfo>("SELECT A.DeptId,B.PersonTotal FORM Coin_User A,Coin_Dept B WHERE A.DeptId=B.Id AND A.Id=@UniqueId", new { UniqueId = 1 });

repositoryUser.Delete(new User { UniqueId = 1 });
repositoryDept.Update(f => f.PersonTotal, new Dept { UniqueId = deptInfo.DeptId, PersonTotal = deptInfo.PersonTotal });

//事务提交
context.Commit();

```
对于枚举类型做了特殊支持
------------------------------------------------------------
枚举属性对应的数据库栏位可为数字类型或是字符串类型。
如果字符串类型需要在枚举类型的属性上增加[Column(typeof(string))]特性，标注数据库栏位类型。


```csharp
CREATE TABLE Coin_User(
	Id int NOT NULL,
	UserName nvarchar(50) NULL,
	Sex nvarchar(50) NULL,
	UID uniqueidentifier NULL,
	UpdatedAt datetime NULL,
	Age int NULL,
	CONSTRAINT PK_Coin_User PRIMARY KEY CLUSTERED 
	(
		Id ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON PRIMARY
) ON PRIMARY

GO

public class User
{
    [PrimaryKey("Id")]
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    [Column(typeof(string))]
    public Sex? Sex { get; set; }
    public Guid? CardId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

var repository = new Repository<User>();

var user = repository.Get(new User { UniqueId = 1 });
user = await repository.GetAsync(new User { UniqueId = 1 });

repository.Update(f => f.Sex, user);
await repository.UpdateAsync(f => f.Sex, user);

```

支持的DDD仓储操作
------------------------------------------------------------

Get方法，根据主键查找对象，需要使用PrimaryKey来标记主键

```csharp
public class User
{
    [PrimaryKey("Id")]
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    [Column(typeof(string))]
    public Sex? Sex { get; set; }
    public Guid? CardId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
var repository = new Repository<User>();

var user = repository.Get(new User { UniqueId = 1 });
user = await repository.GetAsync(new User { UniqueId = 1 });

```
也可以多个字段联合主键
```csharp
public class User
{
    [PrimaryKey("Id")]
    public int UniqueId { get; set; }
	[PrimaryKey]
    public string UserName { get; set; }
    [Column(typeof(string))]
    public Sex? Sex { get; set; }
    public Guid? CardId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```
 
 Create方法，将实体数据插入到数据库
 
```csharp
var repository = new Repository<User>();

var user = repository.Create(new User { UniqueId = 1, UserName = "Keivn" });
user = await repository.CreateAsync(new User { UniqueId = 1, UserName = "Keivn" });
```

Delete方法，根据数据库主键删除数据
 
```csharp
public class User
{
    [PrimaryKey("Id")]
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    [Column(typeof(string))]
    public Sex? Sex { get; set; }
    public Guid? CardId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

var repository = new Repository<User>();
var user = repository.Delete(new User { UniqueId = 1 });

```
Update方法，根据SQL和数据库主键进行更新
```csharp
var repository = new Repository<User>();
var user = new User { UniqueId = 1, Sex = Sex.Male };

repository.Update("UPDATE Coin_User SET Sex=@Sex WHERE ID=@UniqueId", user);
await repository.UpdateAsync("UPDATE Coin_User SET Sex=@Sex WHERE ID=@UniqueId", user);

```

也可以不使用SQL,支持更新一个或多个字段
```csharp
var repository = new Repository<User>();
var user = new User { UniqueId = 1, UserName = "Kevin", Sex = Sex.Male };

repository.Update(f => f.Sex, user);
repository.Update(f => new { f.UserName, f.Sex }, user);

await repository.UpdateAsync(f => f.Sex, user);
await repository.UpdateAsync(f => new { f.UserName, f.Sex }, user);

```


也支持动态SQL
```csharp
var repository = new Repository<User>();
DateTime? beginDate = DateTime.Parse("2017-01-01");
var user = new User { UniqueId = 1, UserName = "Kevin", Sex = Sex.Male };

var builder = new SqlBuilder();
builder.RawSql("UPDATE Coin_User SET UserName=@UserName")
		   .AddField(user.Sex.HasValue, "Sex=@Sex")
		   .AddSql("WHERE Id=@UniqueId")
		   .Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt");

repository.Update(builder.BuildSql(), user);

await repository.UpdateAsync(builder.BuildSql(), user);

或者

var count = repository.Update(f => 
			f.RawSql("UPDATE Coin_User SET UserName=@UserName")
			.AddField(user.Sex.HasValue, "Sex=@Sex")
			.AddSql("WHERE Id=@UniqueId")
			.Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt"), user);

var count = await repository.UpdateAsync(f => f.RawSql("UPDATE Coin_User SET UserName=@UserName")
			f.RawSql("UPDATE Coin_User SET UserName=@UserName")
			.AddField(user.Sex.HasValue, "Sex=@Sex")
			.AddSql("WHERE Id=@UniqueId")
			.Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt"), user);

			
//动态SQL

var list = repository.Query(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.Where(user.Sex.HasValue, "Sex=@Sex")
			.Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt")
			.AddSql("ORDER BY UpdatedAt DESC"), user);

var list = repository.QueryAsync(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.Where(user.Sex.HasValue, "Sex=@Sex")
			.Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt")
			.AddSql("ORDER BY UpdatedAt DESC"), user);			
```
QueryFirst方法，根据SQL获取单条数据

```csharp
var repository = new Repository<User>();

var user = repository.QueryFirst("SELECT UserName,Sex FROM User WHERE Id=@UniqueId", new User { UniqueId = 1 });
user = await repository.QueryFirstAsync("SELECT UserName,Sex FROM User WHERE Id=@UniqueId", new User { UniqueId = 1 });

```

也可以返回其他类型的单条数据,可以是实体或是单个值

```csharp
public enum Sex : byte
{
    Male = 1,
    Female = 2
}  

var repository = new Repository<User>();

var sex = repository.QueryFirst<Sex>("SELECT Sex FROM User WHERE Id=@UniqueId", new User { UniqueId = 1 });
sex = await repository.QueryFirstAsync<Sex>("SELECT Sex FROM User WHERE Id=@UniqueId", new User { UniqueId = 1 });

```

如果字段名称和实体属性名称不一致，SQL中要使用别名

```csharp
public class UserInfo
{
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    public Sex Sex { get; set; }
}
var userInfo = repository.QueryFirst<UserInfo>("SELECT Id UniqueId,UserName,Sex FROM User WHERE Id=@UniqueId", new User { UniqueId = 1 });
userInfo = await repository.QueryFirstAsync<UserInfo>("SELECT Id UniqueId,UserName,Sex FROM User WHERE Id=@UniqueId", new User { UniqueId = 1 });
```

或者在实体属性上增加特性

```csharp
public class UserInfo
{
	[PrimaryKey("Id")]
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    public Sex Sex { get; set; }
}
或者
public class UserInfo
{
	[Column("Id")]
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    public Sex Sex { get; set; }
}

//实体属性上有特性，所以不需要加别名
var userInfo = repository.QueryFirst<UserInfo>("SELECT Id,UserName,Sex FROM User WHERE Id=@UniqueId", new User { UniqueId = 1 });
userInfo = await repository.QueryFirstAsync<UserInfo>("SELECT Id,UserName,Sex FROM User WHERE Id=@UniqueId", new User { UniqueId = 1 });

```

Query方法，根据SQL获取多条数据。

使用功能同QueryFirst方法

```csharp
public enum Sex : byte
{
    Male = 1,
    Female = 2
}  

var repository = new Repository<User>();

var userList = repository.Query("SELECT UserName,Sex FROM User WHERE Id=@UniqueId", new User { UniqueId = 1 });
userList = await repository.QueryAsync("SELECT UserName,Sex FROM User WHERE Id=@UniqueId", new User { UniqueId = 1 });

var list = repository.Query(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.Where(user.Sex.HasValue, "Sex=@Sex")
			.Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt")
			.AddSql("ORDER BY UpdatedAt DESC"), user);

var list = repository.QueryAsync(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.Where(user.Sex.HasValue, "Sex=@Sex")
			.Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt")
			.AddSql("ORDER BY UpdatedAt DESC"), user);

var sexList = repository.Query<Sex>("SELECT Sex FROM User WHERE Id>@UniqueId", new User { UniqueId = 1 });
sexList = await repository.QueryAsync<Sex>("SELECT Sex FROM User WHERE Id>@UniqueId", new User { UniqueId = 1 });

public class UserInfo
{
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    public Sex Sex { get; set; }
}

//如果字段名称和实体属性名称不一致，SQL中要使用别名
var userInfoList = repository.Query<UserInfo>("SELECT Id UniqueId,UserName,Sex FROM User WHERE Id>@UniqueId", new User { UniqueId = 1 });
userInfoList = await repository.QueryAsync<UserInfo>("SELECT Id UniqueId,UserName,Sex FROM User WHERE Id>@UniqueId", new User { UniqueId = 1 });

//或者在实体属性上增加特性
public class UserInfo
{
	[PrimaryKey("Id")]
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    public Sex Sex { get; set; }
}

或者

public class UserInfo
{
	[Column("Id")]
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    public Sex Sex { get; set; }
}

//实体属性上有特性，所以不需要加别名
var userInfoList = repository.Query<UserInfo>("SELECT Id,UserName,Sex FROM User WHERE Id>@UniqueId", new User { UniqueId = 1 });
userInfoList = await repository.QueryAsync<UserInfo>("SELECT Id,UserName,Sex FROM User WHERE Id>@UniqueId", new User { UniqueId = 1 });

```

QueryPage方法，支持分页

pageIndex：从0开始的索引
orderBy：排序，可选
使用功能同Query，QueryFirst方法

```csharp
var repository = new Repository<User>();

var userList = repository.QueryPage("SELECT Id UniqueId,UserName,Sex FROM User WHERE Id>@UniqueId", 0, 10, null, new User { UniqueId = 1 });
userList = await repository.QueryPageAsync("SELECT Id UniqueId,UserName,Sex FROM User WHERE Id>@UniqueId", "ORDER BY Id", new User { UniqueId = 1 });

var list = repository.QueryPage(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.Where(user.Sex.HasValue, "Sex=@Sex")
			.Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt")
			.AddSql("ORDER BY UpdatedAt DESC"), 0, 10, "ORDER BY Id", user);

var list = repository.QueryPageAsync(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.Where(user.Sex.HasValue, "Sex=@Sex")
			.Where(beginDate.HasValue, "UpdatedAt>@UpdatedAt")
			.AddSql("ORDER BY UpdatedAt DESC"), 0, 10, null, user);

var sexList = repository.QueryPage<Sex>("SELECT Sex FROM User WHERE Id>@UniqueId", 0, 10, null, new User { UniqueId = 1 });
sexList = await repository.QueryPageAsync<Sex>("SELECT Sex FROM User WHERE Id>@UniqueId", 0, 10, "ORDER BY Id", new User { UniqueId = 1 });

//如果字段名称和实体属性名称不一致，SQL中要使用别名
var userInfoList = repository.QueryPage<UserInfo>("SELECT Id UniqueId,UserName,Sex FROM User WHERE Id>@UniqueId", 0, 10, null, new User { UniqueId = 1 });
userInfoList = await repository.QueryPageAsync<UserInfo>("SELECT Id UniqueId,UserName,Sex FROM User WHERE Id>@UniqueId", 0, 10, "ORDER BY Id", new User { UniqueId = 1 });


//或者在实体属性上增加特性
public class UserInfo
{
	[PrimaryKey("Id")]
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    public Sex Sex { get; set; }
}

或者

public class UserInfo
{
	[Column("Id")]
    public int UniqueId { get; set; }
    public string UserName { get; set; }
    public Sex Sex { get; set; }
}

//实体属性上有特性，所以不需要加别名
var userInfoList = repository.QueryPage<UserInfo>("SELECT Id,UserName,Sex FROM User WHERE Id>@UniqueId", 0, 10, null, new User { UniqueId = 1 });
userInfoList = await repository.QueryPageAsync<UserInfo>("SELECT Id,UserName,Sex FROM User WHERE Id>@UniqueId", 0, 10, "ORDER BY Id", new User { UniqueId = 1 });

```

待续。。。

欢迎大家使用
---------------------
欢迎大家广提Issue，我的联系方式：
QQ：39253425
Mail：leafkevin@126.com
