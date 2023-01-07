Trolley - 一个轻量级高性能的.NET ORM框架
========================================


框架特点
------------------------------------------------------------
强类型的DDD仓储操作,基本可以不用写SQL,支持多种数据库终端，目前是在.NET 6 基础上开发的。  
目前支持：MySql,PostgreSql,Sql Sever,其他的provider会稍后慢慢提供。  

支持分页查询  
支持Join、group by,order by等操作  
支持各种聚合查询，Count,Max,Min,Avg,Sum等操作  
支持In,Exists操作  
支持Insert Select From  
支持Update From Join  
支持批量插入、更新、删除   
支持模型导航属性，值对象导航属性，就是瘦身版模型  
支持模型映射，采用流畅API方式，目前不支持特性方式映射  
支持多租户分库，不同租户不同的数据库。  

首先，在系统中要注册IOrmDbFactory
------------------------------------------------------------
系统中每个连接串对应一个OrmProvider，每种类型的OrmProvider以单例形式存在，一个应用中可以存在多种类型的OrmProvider。   
在Trolley中，一个dbKey代表一个或是多个结构相同的数据库，可以是不同的OrmProvider。  
常见的场景是：一些租户独立分库，数据库类型也不一定一样，但结构是一样的，那他们就可以是同一个dbKey。  
如: A租户是MySql数据库，B租户是PostgreSql，他们的数据库的结构是相同的。  
在代入租户ID的时候，Trolley会根据租户ID自动找到对应的数据库，进行操作。  
没有租户ID，就是默认的数据库，就是没有指定独立分库的其他所有租户的数据库。  

在注册IOrmDbFactory的时候，同时也要把数据库结构的模型映射配置起来。  
模型映射采用的是Fluent Api方式，类似EF，通常是继承IModelConfiguration的子类。  


示例:

没有租户或是没有租户独立分库的场景
```csharp
var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
var builder = new OrmDbFactoryBuilder();
builder.Register("fengling", true, f => f.Add<MySqlProvider>(connectionString, true))
    .Configure(f => new ModelConfiguration().OnModelCreating(f));
var dbFactory = builder.Build();
```
多租户，不同租户，不同数据库的场景

```csharp
var connectionString1 = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
var connectionString2 = "User id=postgres;Password=123456;Host=localhost;Port=5432;Database=fengling;Pooling=true;Min Pool Size=5;Max Pool Size=100;";
var builder = new OrmDbFactoryBuilder();
builder.Register("fengling", true, f =>
{
    f.Add<MySqlProvider>(connectionString1, true) //默认数据库，除了指定租户外的其他所有租户使用的数据库
     .Add<NpgSqlProvider>(connectionString2, false, new List<int> { 1, 2, 3, 4, 5 });//租户ID为1，2，3，4，5的租户使用的数据库
})
.Configure(f => new ModelConfiguration().OnModelCreating(f));
var dbFactory = builder.Build();

```

导航属性的设置，是单向的，只需要把本模型内的导航属性列出来就可以了。  
对应的导航属性类，再设置它所引用的模型映射。  
这里的ModelConfiguration类，就是模型映射类，内容如下：
```csharp
class ModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(f =>
        {
            //这里只列出了需要特殊指定的列，其他的列在Trolley Build的时候，会自动根据模型结构添加进来的。
            f.ToTable("sys_user").Key(t => t.Id);//表，主键
	    //导航属性的设置，是单向的，只需要把本模型内的导航属性列出来就可以了。  
	    //对应的导航属性类，在应设置再设置它所引用的类型映射。  
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();//导航属性，这里是值对象，不是真正的模型，是模型Company的瘦身版，使用MapTo指定对应的模型Company
            f.HasMany(t => t.Orders).HasForeignKey(t => t.BuyerId);
        });
        builder.Entity<Company>(f =>
        {
            f.ToTable("sys_company").Key(t => t.Id).AutoIncrement(t => t.Id);//表，主键，自动增长列
	    //导航属性的设置，是单向的，只需要把本模型内的导航属性列出来就可以了。  
	    //对应的导航属性类，在应设置再设置它所引用的类型映射。  
            f.HasMany(t => t.Users).HasForeignKey(t => t.CompanyId);//导航属性，这里是真正的模型
        });
        builder.Entity<Order>(f =>
        {
            f.ToTable("sys_order").Key(t => t.Id);
            f.HasOne(t => t.Buyer).HasForeignKey(t => t.BuyerId);	    
            f.HasOne(t => t.Seller).HasForeignKey(t => t.SellerId).MapTo<User>();//导航属性，这里是值对象，不是真正的模型，是模型User的瘦身版，使用MapTo指定对应的模型User
            f.HasMany(t => t.Details).HasForeignKey(t => t.OrderId);
        });
        builder.Entity<OrderDetail>(f =>
        {
            f.ToTable("sys_order_detail").Key(f => f.Id);
            f.HasOne(t => t.Order).HasForeignKey(t => t.OrderId);
        });
    }
}
```


对应的模型结构如下：
```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public int CompanyId { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CompanyInfo Company { get; set; }//值对象，是模型Company的瘦身版
    public List<Order> Orders { get; set; }
}
//模型Company
public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<User> Users { get; set; }
}
//瘦身版模型CompanyInfo，只有两个字段
public class CompanyInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```
在实际应用中，值对象在模型中定义很常见，没必要引用整个模型，真正使用的就是几个栏位，轻量化模型结构。  




其次，创建IRepository对象。
------------------------------------------------------------
所有的操作都是从创建IRepository对象开始的，IRepository可以开启事务，设置command超时时间、各种查询、命令的执行。 
不同模型的操作都是采用IRepository泛型方法来完成的。  


查询操作

```csharp
using var repository = this.dbFactory.Create();

//QueryFirst
var result = repository.QueryFirst<User>(f => f.Id == 1);
var result = await repository.QueryFirstAsync<User>(f => f.Name == "leafkevin");
//SELECT `UpdatedAt`,`CreatedBy`,`CreatedAt`,`Id`,`Age`,`UpdatedBy`,`Name`,`CompanyId`,`IsEnabled`,`Gender` FROM `sys_user` WHERE `Id`=1

//Query
var result = repository.Query<Product>(f => f.ProductNo.Contains("PN-00"));
var result = await repository.QueryAsync<Product>(f => f.ProductNo.Contains("PN-00"));
//SELECT `CompanyId`,`IsEnabled`,`Id`,`Name`,`CreatedBy`,`CategoryId`,`UpdatedBy`,`UpdatedAt`,`BrandId`,`ProductNo`,`CreatedAt` FROM `sys_product` WHERE `ProductNo` LIKE '%PN-00%'

//Page 分页
var result = repository.QueryPage<OrderDetail>(2, 10, f => f.ProductId == 1);
var result = await repository.QueryPageAsync<OrderDetail>(2, 10, f => f.ProductId == 1);
//SELECT COUNT(*) FROM `sys_order_detail` WHERE `ProductId`=1;SELECT `Id`,`IsEnabled`,`CreatedBy`,`UpdatedAt`,`CreatedAt`,`Price`,`Quantity`,`Amount`,`OrderId`,`UpdatedBy`,`ProductId` FROM `sys_order_detail`  WHERE `ProductId`=1 LIMIT 10 OFFSET 10

```


From支持各种复杂查询  

```csharp
using var repository = this.dbFactory.Create();
//Simple
var result = await repository.From<Product>()
    .Where(f => f.ProductNo.Contains("PN-00"))
    .ToListAsync();
//SELECT `CompanyId`,`IsEnabled`,`Id`,`Name`,`CreatedBy`,`CategoryId`,`UpdatedBy`,`UpdatedAt`,`BrandId`,`ProductNo`,`CreatedAt` FROM `sys_product` WHERE `ProductNo` LIKE '%PN-00%'
```
```csharp
//One to One  Include
var result = await repository.From<Product>()
            .Include(f => f.Brand)
            .Where(f => f.ProductNo.Contains("PN-00"))
            .ToListAsync();	    
//SELECT a.`CategoryId`,a.`UpdatedAt`,a.`ProductNo`,a.`CreatedAt`,a.`BrandId`,a.`IsEnabled`,a.`Name`,a.`Id`,a.`UpdatedBy`,a.`CompanyId`,a.`CreatedBy`,b.`Name`,b.`Id`,b.`BrandNo` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
//一对一的Include查询，Include表数据和主表一起查出来。
```
```csharp    
//InnerJoin and IncludeMany    
var result = repository.From<Order>()
    .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
    .IncludeMany((x, y) => x.Details)
    .Where((a, b) => a.TotalAmount > 300)
    .Select((x, y) => new { Order = x, Buyer = y })
    .ToList();
//SELECT a.`SellerId`,a.`UpdatedAt`,a.`UpdatedBy`,a.`Id`,a.`IsEnabled`,a.`OrderNo`,a.`BuyerId`,a.`CreatedBy`,a.`TotalAmount`,a.`CreatedAt`,b.`Gender`,b.`UpdatedAt`,b.`UpdatedBy`,b.`Id`,b.`IsEnabled`,b.`CreatedBy`,b.`CompanyId`,b.`Name`,b.`CreatedAt`,b.`Age` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`TotalAmount`>300
//一对多的IncludeMany查询，分2次查询，第一次如上SQL，把主表数据和其他Join、Include表数据查询出来，第二次把所有IncludeMany的数据都查询出来，再设置到对应主表模型中。
//第二次查询SQL如下：
//SELECT `Amount`,`Price`,`ProductId`,`Quantity`,`UpdatedAt`,`UpdatedBy`,`Id`,`IsEnabled`,`OrderId`,`CreatedBy`,`CreatedAt` FROM `sys_order_detail` WHERE OrderId IN (1,2)
//第二次查询，会根据第一次查询主表主键数据和Filter条件，再去查询IncludeMany表数据。
```
```csharp
//Join、IncludeMany and Filter
var result = repository.From<Order>()
    .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
    .IncludeMany((x, y) => x.Details, f => f.ProductId == 1)
    .Where((a, b) => a.TotalAmount > 300)
    .Select((x, y) => new { Order = x, Buyer = y })
    .ToList();
//
```
```csharp    
//Include and ThenInclude
var result = await repository.From<Order>()
    .InnerJoin<User>((a, b) => a.SellerId == b.Id)
    .Include((x, y) => x.Buyer)
    .ThenInclude(f => f.Company)
    .Where((a, b) => a.TotalAmount > 300)
    .Select((x, y) => new { Order = x, Seller = y })
    .ToListAsync();
//
```
```csharp
//Page and Include
var result = repository.From<OrderDetail>()
    .Include(f => f.Product)
    .Where(f => f.ProductId == 1)
    .ToPageList(2, 10);
//
```
```csharp
//虽然Include的，但是没有查询对应模型，会忽略Include
var sql = repository.From<User>()
    .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
    .IncludeMany((a, b) => a.Orders)
    .ThenIncludeMany(f => f.Details)
    .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
    .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.Id })
    .Select((x, a, b) => new
    {
        x.Grouping,
        OrderCount = x.Count(b.Id),
        TotalAmount = x.Sum(b.TotalAmount)
    })
    .ToSql(out _);
//生成的SQL如下：
//SELECT a.`Id`,a.`Name`,CAST(DATE_FORMAT(b.`CreatedAt`,'%Y-%m-%d') AS DATETIME),COUNT(b.`Id`) AS OrderCount,SUM(b.`TotalAmount`) AS TotalAmount FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CAST(DATE_FORMAT(b.`CreatedAt`,'%Y-%m-%d') AS DATETIME) ORDER BY a.`Id`,b.`Id`
//
```
```csharp
//Group 使用Grouping
var result = repository.From<User>()
    .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
    .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
    .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.Id })
    .Select((x, a, b) => new
    {
        x.Grouping,
        OrderCount = x.Count(b.Id),
        TotalAmount = x.Sum(b.TotalAmount)
    })
    .ToList();
//SELECT a.`Id`,a.`Name`,CAST(DATE_FORMAT(b.`CreatedAt`,'%Y-%m-%d') AS DATETIME),COUNT(b.`Id`) AS OrderCount,SUM(b.`TotalAmount`) AS TotalAmount FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CAST(DATE_FORMAT(b.`CreatedAt`,'%Y-%m-%d') AS DATETIME) ORDER BY a.`Id`,b.`Id`
//
```
```csharp
//
```

支持跨库查询，只要指定对应的dbKey就可以了
------------------------------------------------------------

```csharp
appsetting.json中的数据库配置，如下
{
  "Database": {
    "fengling": {
      "IsDefault": true,
      "ConnectionStrings": [
        {
          "ConnectionString": "Server=localhost;Port=3306;Database=fengling;User Id=root;Password=123456;Pooling=true;",
          "IsDefault": true,
	  //默认使用MySql数据库
          "OrmProvider": "Trolley.MySqlProvider"
        },
        {
          "ConnectionString": "Server=192.168.1.15;Port=5432;Database=fengling;User Id=postgres;Password=123456;Pooling=true;",
          "IsDefault": false,
	  //指定租户Id:1,2,3,4,5，使用PostgreSql数据库
          "OrmProvider": "Trolley.NpgSqlProvider",
          "TenantIds": [ 1, 2, 3, 4, 5 ]
        }
      ]
    }
  }
}
var psqlConnString = "Server=192.168.1.15;Port=5432;Database=fengling;User Id=postgres;Password=123456;Pooling=true;";
using var repository = this.dbFactory.Create("mysql");


//默认使用的连接串
OrmProviderFactory.RegisterProvider(sqlConnString, new SqlServerProvider(), true);
OrmProviderFactory.RegisterProvider(psqlConnString, new PostgreSqlProvider());

var repository = new Repository(sqlConnString);
var user = repository.Query<User>("select Age = @Age, Id = @UniqueId", new User { Age = (int?)null, UniqueId = 1 });
user = await repository.QueryAsync<User>("select Age = @Age, Id = @UniqueId", new User { Age = (int?)null, UniqueId = 1 });

var repository = new Repository(psqlConnString);
var user = repository.Query<User>("select Age = @Age, Id = @UniqueId", new User { Age = (int?)null, UniqueId = 1 });
user = await repository.QueryAsync<User>("select Age = @Age, Id = @UniqueId", new User { Age = (int?)null, UniqueId = 1 });

也可以使用默认的字符串构造仓储对象
var repository = new Repository();
var repository = new Repository<User>();
上面两种方式都可以构造仓储对象
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
builder.RawSql("UPDATE Coin_User SET UserName=@UserName", user.UserName)
        .AddField(user.Sex.HasValue, "Sex=@Sex", user.Sex)
        .RawSql("WHERE Id=@UniqueId", user.UniqueId)
        .AndWhere(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt", user.UpdatedAt);
count = await repository.UpdateAsync(builder.BuildSql(), user);

//全量更新
repository.Update(user);
await repository.UpdateAsync(user);

//更新
repository.Update(builder.BuildSql(), user);
await repository.UpdateAsync(builder.BuildSql(), user);

//或者
var count = repository.Update(f => 
			f.RawSql("UPDATE Coin_User SET UserName=@UserName", user.UserName)
			.AddField(user.Sex.HasValue, "Sex=@Sex", user.Sex)
			.RawSql("WHERE Id=@UniqueId", user.UniqueId)
			.AndWhere(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt", user.UpdatedAt));

//查询
var list = repository.Query(f =>
			f.RawSql("SELECT * FROM Coin_User")
			.AndWhere(user.Sex.HasValue, "Sex=@Sex", user.Sex)
			.AndWhere(user.UpdatedAt.HasValue, "UpdatedAt>@UpdatedAt", user.UpdatedAt)
			.RawSql("ORDER BY UpdatedAt DESC"));

//查询字典
var dictRepository = new Repository();
var dict = dictRepository.QueryDictionary<int, string>("SELECT Id Key,UserName Value FROM Coin_User");
```


所有SQL操作都支持异步
------------------------------------------------------------

```csharp
var repository = new Repository<User>();
DateTime? beginDate = DateTime.Parse("2017-01-01");
var user = new User { UniqueId = 1, UserName = "Kevin", Sex = Sex.Male };

//全量更新
await repository.UpdateAsync(user);
//更新
await repository.UpdateAsync(builder.BuildSql(), user);

var count = await repository.UpdateAsync(f => 
			f.RawSql("UPDATE Coin_User SET UserName=@UserName", user.UserName)
			.AddField(user.Sex.HasValue, "Sex=@Sex", user.Sex)
			.RawSql("WHERE Id=@UniqueId", user.UniqueId)
			.AndWhere(beginDate.HasValue, "UpdatedAt>@UpdatedAt", user.UpdatedAt));

var list = repository.QueryAsync(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.AndWhere(user.Sex.HasValue, "Sex=@Sex", user.Sex)
			.AndWhere(beginDate.HasValue, "UpdatedAt>@UpdatedAt", user.UpdatedAt)
			.RawSql("ORDER BY UpdatedAt DESC"), user);
			
//查询字典
var dictRepository = new Repository();
var dict = await dictRepository.QueryDictionaryAsync<int, string>("SELECT Id Key,UserName Value FROM Coin_User");
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
builder.RawSql("UPDATE Coin_User SET UserName=@UserName",user.UserName)
		   .AddField(user.Sex.HasValue, "Sex=@Sex",user.Sex)
		   .RawSql("WHERE Id=@UniqueId",user.UniqueId)
		   .AndWhere(beginDate.HasValue, "UpdatedAt>@UpdatedAt",user.UpdatedAt);

repository.Update(builder.BuildSql(), user);

await repository.UpdateAsync(builder.BuildSql(), user);

或者

var count = repository.Update(f => 
			f.RawSql("UPDATE Coin_User SET UserName=@UserName",user.UserName)
			.AddField(user.Sex.HasValue, "Sex=@Sex",user.Sex)
			.RawSql("WHERE Id=@UniqueId",user.UniqueId)
			.AndWhere(beginDate.HasValue, "UpdatedAt>@UpdatedAt",user.UpdatedAt));

var count = await repository.UpdateAsync(f => 
			f.RawSql("UPDATE Coin_User SET UserName=@UserName",user.UserName)
			.AddField(user.Sex.HasValue, "Sex=@Sex",user.Sex)
			.RawSql("WHERE Id=@UniqueId",user.UniqueId)
			.AndWhere(beginDate.HasValue, "UpdatedAt>@UpdatedAt",user.UpdatedAt));

			
//动态SQL
var list = repository.Query(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.AndWhere(user.Sex.HasValue, "Sex=@Sex",user.Sex)
			.AndWhere(beginDate.HasValue, "UpdatedAt>@UpdatedAt",user.UpdatedAt)
			.RawSql("ORDER BY UpdatedAt DESC"));

var list = repository.QueryAsync(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.AndWhere(user.Sex.HasValue, "Sex=@Sex",user.Sex)
			.AndWhere(beginDate.HasValue, "UpdatedAt>@UpdatedAt",user.UpdatedAt)
			.RawSql("ORDER BY UpdatedAt DESC"));			
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
			.AndWhere(user.Sex.HasValue, "Sex=@Sex",user.Sex)
			.AndWhere(beginDate.HasValue, "UpdatedAt>@UpdatedAt",user.UpdatedAt)
			.RawSql("ORDER BY UpdatedAt DESC"), user);

var list = repository.QueryAsync(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.AndWhere(user.Sex.HasValue, "Sex=@Sex",user.Sex)
			.AndWhere(beginDate.HasValue, "UpdatedAt>@UpdatedAt",user.UpdatedAt)
			.RawSql("ORDER BY UpdatedAt DESC"), user);

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
			.AndWhere(user.Sex.HasValue, "Sex=@Sex",user.Sex)
			.AndWhere(beginDate.HasValue, "UpdatedAt>@UpdatedAt",user.UpdatedAt)
			.RawSql("ORDER BY UpdatedAt DESC"), 0, 10, "ORDER BY Id");

var list = repository.QueryPageAsync(f => 
			f.RawSql("SELECT * FROM Coin_User")
			.AndWhere(user.Sex.HasValue, "Sex=@Sex",user.Sex)
			.AndWhere(beginDate.HasValue, "UpdatedAt>@UpdatedAt",user.UpdatedAt)
			.RawSql("ORDER BY UpdatedAt DESC"), 0, 10, "ORDER BY Id");

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

IOrmProvider接口中的IsMappingIgnoreCase，表示数据库中的字段映射到实体中，是否忽略大小写，有时候很有用，比如Postgresql的大小写问题。


QueryMultiple方法，获取多个结果集，返回一个QueryReader对象。
再根据Read<T>()，ReadList<T>，ReadPageList<T>三个方法进一步获取强类型对象。
```csharp
var order = new Order { Id = 1 };
var orderRepository = new Repository<Order>(connString);
var sql = "SELECT * FROM Coin_Order WHERE Id=@Id;SELECT * FROM Coin_OrderLine WHERE OrderId=@Id";
var reader = orderRepository.QueryMultiple(sql, order);
order = reader.Read<Order>();
order.Lines = reader.ReadList<OrderLine>();

order.Number = "123456789";
orderRepository.Update(f => f.Number, order);

```


也可以使用QueryMap方法，直接返回你想要的结果。
```csharp
var order = new Order { Id = 1 };
var orderRepository = new Repository<Order>(connString);
var sql = "SELECT * FROM Coin_Order WHERE Id=@Id;SELECT * FROM Coin_OrderLine WHERE OrderId=@Id";

order = orderRepository.QueryMap(map =>
{
    var result = map.Read();
    result.Lines = map.ReadList<OrderLine>();
    return result;
}, sql, order);

order.Number = "123456789";
orderRepository.Update(f => f.Number, order);

```

欢迎大家使用
---------------------
欢迎大家广提Issue，我的联系方式：
QQ：39253425
Mail：leafkevin@126.com
