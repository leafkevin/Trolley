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
所有的查询操作，都支持ToSql方法，可以查看生成SQL语句，方便诊断。


查询操作
查询类语句，只有String类型做了参数化，其他的数据类型都没有参数化。  

```csharp
using var repository = this.dbFactory.Create();
//扩展的简化查询，不支持ToSql方法，是由From语句来包装的，From语句支持ToSql查看SQL
//QueryFirst
var result = repository.QueryFirst<User>(f => f.Id == 1);
var result = await repository.QueryFirstAsync<User>(f => f.Name == "leafkevin");
//SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id`=1

//Query
var result = repository.Query<Product>(f => f.ProductNo.Contains("PN-00"));
var result = await repository.QueryAsync<Product>(f => f.ProductNo.Contains("PN-00"));
//SELECT `Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_product` WHERE `ProductNo` LIKE '%PN-00%'

//Page 分页
var result = repository.From<OrderDetail>()
    .Where(f => f.ProductId == 1)
    .OrderByDescending(f => f.CreatedAt)
    .ToPageList(2, 10);
//SELECT COUNT(*) FROM `sys_order_detail` WHERE `ProductId`=1;SELECT `Id`,`OrderId`,`ProductId`,`Price`,`Quantity`,`Amount`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_order_detail` WHERE `ProductId`=1 ORDER BY `CreatedAt` DESC LIMIT 10 OFFSET 10

//Dictionary
var result = await repository.QueryDictionaryAsync<Product, int, string>(f => f.ProductNo.Contains("PN-00"), f => f.Id, f => f.Name);
//SELECT `Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_product` WHERE `ProductNo` LIKE '%PN-00%'
//先把实体查询出来，再根据Key,Value的选取，生成Dictionary
```


From支持各种复杂查询  

```csharp
using var repository = this.dbFactory.Create();
//Simple
var result = await repository.From<Product>()
    .Where(f => f.ProductNo.Contains("PN-00"))
    .ToListAsync();
//SELECT `Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_product` WHERE `ProductNo` LIKE '%PN-00%'
```
```csharp
//One to One  Include
var result = await repository.From<Product>()
            .Include(f => f.Brand)
            .Where(f => f.ProductNo.Contains("PN-00"))
            .ToListAsync();	    
//SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
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
//SELECT a.`Id`,a.`OrderNo`,a.`TotalAmount`,a.`BuyerId`,a.`SellerId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`TotalAmount`>300
//一对多的IncludeMany查询，分2次查询，第一次如上SQL，把主表数据和其他Join、Include表数据查询出来，第二次把所有IncludeMany的数据都查询出来，再设置到对应主表模型中。
//第二次查询SQL如下：
//SELECT `Id`,`OrderId`,`ProductId`,`Price`,`Quantity`,`Amount`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_order_detail` WHERE OrderId IN (1,2)
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
//第一次查询SQL:
//SELECT a.`Id`,a.`OrderNo`,a.`TotalAmount`,a.`BuyerId`,a.`SellerId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`TotalAmount`>300
//第二次查询SQL:
//SELECT `Id`,`OrderId`,`ProductId`,`Price`,`Quantity`,`Amount`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_order_detail` WHERE OrderId IN (1,2) AND `ProductId`=1
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
//SELECT a.`Id`,a.`OrderNo`,a.`TotalAmount`,a.`BuyerId`,a.`SellerId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,c.`Id`,c.`Name`,c.`Gender`,c.`Age`,c.`CompanyId`,c.`IsEnabled`,c.`CreatedBy`,c.`CreatedAt`,c.`UpdatedBy`,c.`UpdatedAt`,d.`Id`,d.`Name`,b.`Id`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`SellerId`=b.`Id` LEFT JOIN `sys_user` c ON a.`BuyerId`=c.`Id` LEFT JOIN `sys_company` d ON c.`CompanyId`=d.`Id` WHERE a.`TotalAmount`>300
```

```csharp
//Page and Include
var result = repository.From<OrderDetail>()
    .Include(f => f.Product)
    .Where(f => f.ProductId == 1)
    .ToPageList(2, 10);
//SELECT COUNT(*) FROM `sys_order_detail` a LEFT JOIN `sys_product` b ON a.`ProductId`=b.`Id` WHERE a.`ProductId`=1;SELECT a.`Id`,a.`OrderId`,a.`ProductId`,a.`Price`,a.`Quantity`,a.`Amount`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`ProductNo`,b.`Name`,b.`BrandId`,b.`CategoryId`,b.`CompanyId`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt` FROM `sys_order_detail` a LEFT JOIN `sys_product` b ON a.`ProductId`=b.`Id`  WHERE a.`ProductId`=1 LIMIT 10 OFFSET 10
```

```csharp
//虽然有Include，但是没有查询对应模型，会忽略Include
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
```

```csharp
//Group 使用IGroupingAggregate类型的Grouping属性，也可以不使用
//IGroupingAggregate类型的Grouping属性，是group by字段选择后的对象，这里有3个字段：Id,Name,Date
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
```

```csharp
//Group 打开Grouping，使用里面的字段
//IGroupingAggregate类型的Grouping属性，是group by字段选择后的对象，这里有3个字段：Id,Name,Date
var result = repository.From<User>()
   .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
   .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
   .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.Id })
   .Select((x, a, b) => new
   {
       x.Grouping.Id,
       x.Grouping.Name,
       x.Grouping.Date,
       OrderCount = x.Count(b.Id),
       TotalAmount = x.Sum(b.TotalAmount)
   })
   .ToList();
 //SELECT a.`Id`,a.`Name`,CAST(DATE_FORMAT(b.`CreatedAt`,'%Y-%m-%d') AS DATETIME) AS Date,COUNT(b.`Id`) AS OrderCount,SUM(b.`TotalAmount`) AS TotalAmount FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CAST(DATE_FORMAT(b.`CreatedAt`,'%Y-%m-%d') AS DATETIME) ORDER BY a.`Id`,b.`Id`
 //打开Grouping属性，和不打开生成的SQL基本差不多，唯一不同的地方是：打开时有AS别名，如上面的Date字段，Id,Name与原字段相同就不用AS了
```

```csharp
//Group and Having 、Exists
var sql = repository.From<User>()
   .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
   .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
   .Having((x, a, b) => x.Sum(b.TotalAmount) > 300 && Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && x.CountDistinct(f.ProductId) > 2))
   .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.Id })
   .Select((x, a, b) => new
   {
       x.Grouping.Id,
       x.Grouping.Name,
       x.Grouping.Date,
       OrderCount = x.Count(b.Id),
       TotalAmount = x.Sum(b.TotalAmount)
   })
   .ToSql(out _);
//SELECT a.`Id`,a.`Name`,CAST(DATE_FORMAT(b.`CreatedAt`,'%Y-%m-%d') AS DATETIME) AS Date,COUNT(b.`Id`) AS OrderCount,SUM(b.`TotalAmount`) AS TotalAmount FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CAST(DATE_FORMAT(b.`CreatedAt`,'%Y-%m-%d') AS DATETIME) HAVING SUM(b.`TotalAmount`)>300 AND EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND COUNT(DISTINCT f.`ProductId`)>2) ORDER BY a.`Id`,b.`Id`
```


```csharp
//In and Exists 
//In、Exists操作是通过静态Sql类来完成的，书写起来比较简单。  
var sql = repository.From<User>()
    .Where(f => Sql.In(f.Id, new int[] { 1, 2, 3 }))
    .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
    .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
    .Having((x, a, b) => x.Sum(b.TotalAmount) > 300 && Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && x.CountDistinct(f.ProductId) > 2))
    .OrderBy((x, a, b) => new { UserId = a.Id, OrderId = b.Id })
    .Select((x, a, b) => new
    {
	x.Grouping,
	OrderCount = x.Count(b.Id),
	TotalAmount = x.Sum(b.TotalAmount)
    })
    .ToSql(out _);
//SELECT a.`Id`,a.`Name`,CAST(DATE_FORMAT(b.`CreatedAt`,'%Y-%m-%d') AS DATETIME),COUNT(b.`Id`) AS OrderCount,SUM(b.`TotalAmount`) AS TotalAmount FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE `Id` IN (@p0,@p1,@p2) GROUP BY a.`Id`,a.`Name`,CAST(DATE_FORMAT(b.`CreatedAt`,'%Y-%m-%d') AS DATETIME) HAVING SUM(b.`TotalAmount`)>300 AND EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND COUNT(DISTINCT f.`ProductId`)>2) ORDER BY a.`Id`,b.`Id`
```
```csharp
//In、Exists 支持查询表数据，甚至更复杂的SQL
var sql = repository.From<User>()
    .Include(f => f.Company)
    .Where(f => Sql.In(f.Id, t => t.From<Order>().Select(p => p.BuyerId)))
    .ToSql(out _);
//SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id` IN (SELECT a.`BuyerId` FROM `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` AND b.`ProductId`=1)

//In 从Order,OrderDetail中关联查询条件中使用
bool? isMale = true;
var sql = repository.From<User>()
    .Where(f => Sql.In(f.Id, t => t.From<OrderDetail, Order>('b').InnerJoin((a, b) => a.OrderId == b.Id && a.ProductId == 1).Select((x, y) => y.BuyerId)))
    .And(isMale.HasValue, f => Sql.Exists<Company, Order>((x, y) => f.Id == y.SellerId && f.CompanyId == x.Id))
    .GroupBy(f => new { f.Gender, f.Age })
    .Select((t, a) => new { t.Grouping, CompanyCount = t.CountDistinct(a.CompanyId), UserCount = t.Count(a.Id) })
    .ToSql(out _);
//SELECT a.`Id`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt` FROM `sys_user` a WHERE `Id` IN (SELECT b.`BuyerId` FROM `sys_order` b INNER JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId` AND c.`ProductId`=1) AND EXISTS(SELECT * FROM `sys_order` x,`sys_company` y WHERE a.`Id`=x.`SellerId` AND a.`CompanyId`=y.`Id`)
//这里又使用And，带有条件
//这里有个不够优雅的地方，第一个In条件的时候，是单表查询没有别名，到后面Exists语句必须带上别名，后面生成的所有SQL都带上了别名。
```

```csharp
//聚合查询
使用SelectAggregate可以聚合查询，也可以使用Select+Sql静态类
var sql = repository.From<User>()
    .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
    .IncludeMany((a, b) => a.Orders)
    .OrderBy((a, b) => new { UserId = a.Id, OrderId = b.Id })
    .SelectAggregate((x, a, b) => new
    {
	UserId = a.Id,
	OrderId = b.Id,
	OrderCount = x.Count(b.Id),
	TotalAmount = x.Sum(b.TotalAmount)
    })
    .ToSql(out _);
//SELECT a.`Id` AS UserId,b.`Id` AS OrderId,COUNT(b.`Id`) AS OrderCount,SUM(b.`TotalAmount`) AS TotalAmount FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId`ORDER BY a.`Id`,b.`Id`

//使用Sql静态类
var sql = repository.From<User>()
    .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
    .IncludeMany((a, b) => a.Orders)
    .OrderBy((a, b) => new { UserId = a.Id, OrderId = b.Id })
    .Select((a, b) => new
    {
	UserId = a.Id,
	OrderId = b.Id,
	OrderCount = Sql.Count(b.Id),
	TotalAmount = Sql.Sum(b.TotalAmount)
    })
    .ToSql(out _);
//SELECT a.`Id` AS UserId,b.`Id` AS OrderId,COUNT(b.`Id`) AS OrderCount,SUM(b.`TotalAmount`) AS TotalAmount FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId`ORDER BY a.`Id`,b.`Id`
//两者生成的SQL完全一样的。
```

```csharp
//查询NULL Where Null
var sql = repository.From<Order>()
    .Where(x => x.ProductCount == null)
    .And(true, f => !f.ProductCount.HasValue)
    .Select(x => new
    {
	NoOrderNo = x.OrderNo == null,
	HasProduct = x.ProductCount.HasValue
    })
    .ToSql(out _);
//SELECT (`OrderNo` IS NULL) AS NoOrderNo,(`ProductCount` IS NOT NULL) AS HasProduct FROM `sys_order` WHERE `ProductCount` IS NULL AND `ProductCount` IS NOT NULL
```


```csharp
//查询ValueTuple
var sql = "SELECT Id,OrderNo,TotalAmount FROM sys_order";
var result = repository.Query<(int OrderId, string OrderNo, double TotalAmount)>(sql);
```

```csharp
//单表Count
var count = repository.From<User>().Count();
var count1 = repository.From<User>().Select(f => Sql.Count()).First();
var count2 = repository.QueryFirst<int>("SELECT COUNT(1) FROM sys_user");

//单表Max
var count = repository.From<Order>().Max(f => f.TotalAmount);
var count1 = repository.From<Order>().Select(f => Sql.Max(f.TotalAmount)).First();
var count2 = repository.QueryFirst<double>("SELECT MAX(TotalAmount) FROM sys_order");

//单表Min
var count = repository.From<Order>().Min(f => f.TotalAmount);
var count1 = repository.From<Order>().Select(f => Sql.Min(f.TotalAmount)).First();
var count2 = repository.QueryFirst<double>("SELECT MIN(TotalAmount) FROM sys_order");

//单表Avg
var value1 = repository.From<Order>().Avg(f => f.TotalAmount);
var value2 = repository.From<Order>().Select(f => Sql.Avg(f.TotalAmount)).First();
var value3 = repository.QueryFirst<double>("SELECT AVG(TotalAmount) FROM sys_order");
```
 

	
支持跨库查询，只要指定对应的dbKey就可以了
------------------------------------------------------------
使用Trolley.AspNetCore扩展后，可以使用json文件来配置数据库连接串信息  
如果有多租户，代入对应的租户ID，就使用对应的数据库连接串了  
appsetting.json中的数据库配置，如下
```json
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
```
```csharp
//按照上面的数据库配置文件
using var repository = this.dbFactory.Create("fengling", 1);
//使用fengling dbKey下租户ID:1 的PostgreSql数据库
using var repository = this.dbFactory.Create("fengling");
//使用的默认fengling dbkey下默认数据库
```


各种操作命令
------------------------------------------------------------

# 新增
```csharp
using var repository = this.dbFactory.Create();
//扩展简化操作
var result = await repository.CreateAsync<User>(new
{
    Id = 1,
    Name = "leafkevin",
    Age = 25,
    CompanyId = 1,
    Gender = Gender.Male,
    IsEnabled = true,
    CreatedAt = DateTime.Now,
    CreatedBy = 1,
    UpdatedAt = DateTime.Now,
    UpdatedBy = 1
});
//INSERT INTO `sys_user` (`Id`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)
```

```csharp
//使用字典参数,自增长列
var result = repository.Create<Company>(new Dictionary<string, object>()
{
	//{ "Id", 1}, //可以带主键，插入的值就是代入的值，不代值，就是数据库自增长的值
	{ "Name","微软11"},
	{ "IsEnabled", true},
	{ "CreatedAt", DateTime.Now},
	{ "CreatedBy", 1},
	{ "UpdatedAt", DateTime.Now},
	{ "UpdatedBy", 1}
});
//INSERT INTO `sys_company` (`Id`,`Name`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@Name,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) RETURNING Id
//自增长列会返回插入的主键列值，使用RETURNING语句返回值，这样可以返回代入的主键值
```

```csharp
//批量新增
var count = repository.Create<Product>(new[]
{
    new
    {
	Id = 1,
	ProductNo="PN-001",
	Name = "波司登羽绒服",
	BrandId = 1,
	CategoryId = 1,
	IsEnabled = true,
	CreatedAt = DateTime.Now,
	CreatedBy = 1,
	UpdatedAt = DateTime.Now,
	UpdatedBy = 1
    },
    new
    {
	Id = 2,
	ProductNo="PN-002",
	Name = "雪中飞羽绒裤",
	BrandId = 2,
	CategoryId = 2,
	IsEnabled = true,
	CreatedAt = DateTime.Now,
	CreatedBy = 1,
	UpdatedAt = DateTime.Now,
	UpdatedBy = 1
    },
    new
    {
	Id = 3,
	ProductNo="PN-003",
	Name = "优衣库保暖内衣",
	BrandId = 3,
	CategoryId = 3,
	IsEnabled = true,
	CreatedAt = DateTime.Now,
	CreatedBy = 1,
	UpdatedAt = DateTime.Now,
	UpdatedBy = 1
    }
});
//INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)

```

```csharp
//使用Create<User>() WithBy
var count = await repository.Create<User>()
    .WithBy(new
    {
	Id = 1,
	Name = "leafkevin",
	Age = 25,
	CompanyId = 1,
	Gender = Gender.Male,
	IsEnabled = true,
	CreatedAt = DateTime.Now,
	CreatedBy = 1,
	UpdatedAt = DateTime.Now,
	UpdatedBy = 1
    }).ExecuteAsync();
//INSERT INTO `sys_user` (`Id`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)
```


```csharp
//WithBy  字典
var id = repository.Create<Company>()
    .WithBy(new Dictionary<string, object>()
    {
    	{ "Id", 1},
    	{ "Name","微软11"},
    	{ "IsEnabled", true},
    	{ "CreatedAt", DateTime.Now},
    	{ "CreatedBy", 1},
    	{ "UpdatedAt", DateTime.Now},
    	{ "UpdatedBy", 1}
    }).Execute();
//INSERT INTO `sys_company` (`Id`,`Name`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@Name,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) RETURNING Id
```  

```csharp
//可为null字段不赋值
var count = repository.Create<Order>(new Order
{
    Id = 1,
    OrderNo = "ON-001",
    BuyerId = 1,
    SellerId = 2,
    TotalAmount = 500,
    //此字段可为空，但不赋值
    //ProductCount = 3,
    IsEnabled = true,
    CreatedAt = DateTime.Now,
    CreatedBy = 1,
    UpdatedAt = DateTime.Now,
    UpdatedBy = 1
});
//进入到数据库中ProductCount字段值为null
```  

```csharp
//WithBy 批量插入
var count = repository.Create<Product>()
    .WithBy(new[]
    {
	new
	{
	    Id = 1,
	    ProductNo="PN-001",
	    Name = "波司登羽绒服",
	    BrandId = 1,
	    CategoryId = 1,
	    IsEnabled = true,
	    CreatedAt = DateTime.Now,
	    CreatedBy = 1,
	    UpdatedAt = DateTime.Now,
	    UpdatedBy = 1
	},
	new
	{
	    Id = 2,
	    ProductNo="PN-002",
	    Name = "雪中飞羽绒裤",
	    BrandId = 2,
	    CategoryId = 2,
	    IsEnabled = true,
	    CreatedAt = DateTime.Now,
	    CreatedBy = 1,
	    UpdatedAt = DateTime.Now,
	    UpdatedBy = 1
	},
	new
	{
	    Id = 3,
	    ProductNo="PN-003",
	    Name = "优衣库保暖内衣",
	    BrandId = 3,
	    CategoryId = 3,
	    IsEnabled = true,
	    CreatedAt = DateTime.Now,
	    CreatedBy = 1,
	    UpdatedAt = DateTime.Now,
	    UpdatedBy = 1
	}
    }).Execute();
//INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)
```  

```csharp
//WithBy 批量字典
var count = repository.Create<Product>()
    .WithBy(new[]
    {
	new Dictionary<string,object>
	{
	    { "Id",1 },
	    { "ProductNo","PN-001"},
	    { "Name","波司登羽绒服"},
	    { "BrandId",1},
	    { "CategoryId",1},
	    { "IsEnabled",true},
	    { "CreatedAt",DateTime.Now},
	    { "CreatedBy",1},
	    { "UpdatedAt",DateTime.Now},
	    { "UpdatedBy",1}
	},
	new Dictionary<string,object>
	{
	    { "Id",2},
	    { "ProductNo","PN-002"},
	    { "Name","雪中飞羽绒裤"},
	    { "BrandId",2},
	    { "CategoryId",2},
	    { "IsEnabled",true},
	    { "CreatedAt",DateTime.Now},
	    { "CreatedBy",1},
	    { "UpdatedAt",DateTime.Now},
	    { "UpdatedBy",1}
	},
	new Dictionary<string,object>
	{
	    { "Id",3},
	    { "ProductNo","PN-003"},
	    { "Name","优衣库保暖内衣"},
	    { "BrandId",3},
	    { "CategoryId",3},
	    { "IsEnabled",true},
	    { "CreatedAt",DateTime.Now},
	    { "CreatedBy",1},
	    { "UpdatedAt",DateTime.Now},
	    { "UpdatedBy",1}
	}
    }).Execute();
    
//INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)
```  

```csharp
//Insert From 单表
var sql = repository.Create<Product>()
    .From<Brand>(f => new
    {
	Id = f.Id + 1,
	ProductNo = "PN_" + f.BrandNo,
	Name = "PName_" + f.Name,
	BrandId = f.Id,
	CategoryId = 1,
	f.CompanyId,
	f.IsEnabled,
	f.CreatedBy,
	f.CreatedAt,
	f.UpdatedBy,
	f.UpdatedAt
    })
    .Where(f => f.Id == 1)
    .ToSql(out _);
//INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT a.`Id`+1,@ProductNo,@Name,a.`Id`,@CategoryId,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt` FROM `sys_brand` a WHERE a.`Id`=1
//使用常量的地方，变成了参数
```  

```csharp
//Insert From 多表
var sql = repository.Create<OrderDetail>()
    .From<Order, Product>((x, y) => new OrderDetail
    {
	Id = 7,
	OrderId = x.Id,
	ProductId = y.Id,
	Price = y.Price,
	Quantity = 3,
	Amount = y.Price * 3,
	IsEnabled = x.IsEnabled,
	CreatedBy = x.CreatedBy,
	CreatedAt = x.CreatedAt,
	UpdatedBy = x.UpdatedBy,
	UpdatedAt = x.UpdatedAt
    })
    .Where((a, b) => a.Id == 3 && b.Id == 1)
    .ToSql(out _);
//INSERT INTO `sys_order_detail` (`Id`,`OrderId`,`ProductId`,`Price`,`Quantity`,`Amount`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT @Id,a.`Id`,b.`Id`,b.`Price`,@Quantity,b.`Price`*3,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt` FROM `sys_order` a,`sys_product` b WHERE a.`Id`=3 AND b.`Id`=1
//使用常量的地方，变成了参数
```  



# 更新

```csharp
//简化操作
var result = repository.Update<User>(f => new { Name = f.Name + "_1", Gender = Gender.Female }, t => t.Id == 1);
//UPDATE `sys_user` SET `Name`=CONCAT(`Name`,'_1'),`Gender`=@Gender WHERE `Id`=1
```

```csharp
//带有参数，局部更新
var result = repository.Update<User>(f => f.Name, new { Id = 1, Name = "leafkevin11" });
//UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=@kId
```

```csharp
//部分表达式更新，部分参数更新，更新的字段由前面的表达式指定，Where条件是主键
var result = repository.Update<User>(f => new { Age = 25, f.Name, CompanyId = DBNull.Value }, new { Id = 1, Age = 18, Name = "leafkevin22" });
//UPDATE `sys_user` SET `Age`=25,`CompanyId`=NULL,`Name`=@Name WHERE `Id`=@kId
//说明：
//Age = 25 ，CompanyId = DBNull.Value 表达式更新，直接以SQL形式更新
//DBNull.Value ,null 都可用来更新NULL字段
//f.Name 只成员访问，将作为后面参数更新的字段
//后面参数的中，有Age字段，但是前面的表达式是 Age = 25，所以不生效，如果是 f.Age ,后面的参数就生效了
```


```csharp
//批量参数更新，Where条件是主键，其他的更新字段由表达式指定
var orderDetails = await repository.From<OrderDetail>().ToListAsync();
var parameters = orderDetails.Select(f => new { f.Id, Price = f.Price + 80, Quantity = f.Quantity + 1, Amount = f.Amount + 50 }).ToList();
var result = repository.Update<OrderDetail>(f => new { Price = 200, f.Quantity, UpdatedBy = 2, f.Amount, ProductId = DBNull.Value }, parameters);
//UPDATE `sys_order_detail` SET `Price`=200,`UpdatedBy`=2,`ProductId`=NULL,`Quantity`=@Quantity0,`Amount`=@Amount0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Price`=200,`UpdatedBy`=2,`ProductId`=NULL,`Quantity`=@Quantity1,`Amount`=@Amount1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Price`=200,`UpdatedBy`=2,`ProductId`=NULL,`Quantity`=@Quantity2,`Amount`=@Amount2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Price`=200,`UpdatedBy`=2,`ProductId`=NULL,`Quantity`=@Quantity3,`Amount`=@Amount3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Price`=200,`UpdatedBy`=2,`ProductId`=NULL,`Quantity`=@Quantity4,`Amount`=@Amount4 WHERE `Id`=@kId4;UPDATE `sys_order_detail` SET `Price`=200,`UpdatedBy`=2,`ProductId`=NULL,`Quantity`=@Quantity5,`Amount`=@Amount5 WHERE `Id`=@kId5
//说明：
//Price = 200 ，UpdatedBy = 2 ，ProductId = DBNull.Value 表达式更新，直接以SQL形式更新
//DBNull.Value ,null 都可用来更新NULL字段
//f.Quantity ，f.Amount 只成员访问，将作为后面参数更新的字段
//后面参数的中，有Price，Quantity，Amount 字段，但是前面的表达式是 Price = 200，所以不生效，后面的只更新Quantity，Amount 字段
```


使用Update<T>，支持各种复杂更新操作

```csharp
//WithBy 单个更新，Where条件是主键
var result = repository.Update<User>().WithBy(new { Name = "leafkevin1", Id = 1 }).Execute();
//UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=@kId
```
	
	
```csharp
//WithBy 批量更新 Where条件是主键
var parameters = await repository.From<OrderDetail>()
    .Where(f => new int[] { 1, 2, 3, 4, 5, 6 }.Contains(f.Id))
    .Select(f => new { f.Id, Price = f.Price + 80, Quantity = f.Quantity + 2, Amount = f.Amount + 100 })
    .ToListAsync();
var sql = repository.Update<OrderDetail>().WithBy(parameters).ToSql(out _);
//UPDATE `sys_order_detail` SET `Price`=@Price0,`Quantity`=@Quantity0,`Amount`=@Amount0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Price`=@Price1,`Quantity`=@Quantity1,`Amount`=@Amount1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Price`=@Price2,`Quantity`=@Quantity2,`Amount`=@Amount2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Price`=@Price3,`Quantity`=@Quantity3,`Amount`=@Amount3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Price`=@Price4,`Quantity`=@Quantity4,`Amount`=@Amount4 WHERE `Id`=@kId4;UPDATE `sys_order_detail` SET `Price`=@Price5,`Quantity`=@Quantity5,`Amount`=@Amount5 WHERE `Id`=@kId5
```

```csharp
//WithBy 部分表达式，部分参数 批量更新 Where条件是主键
var parameters = await repository.From<OrderDetail>()
    .Where(f => new int[] { 1, 2, 3, 4, 5, 6 }.Contains(f.Id))
    .Select(f => new { f.Id, Price = f.Price + 80, Quantity = f.Quantity + 2, Amount = f.Amount + 100 })
    .ToListAsync();
var sql = repository.Update<OrderDetail>()
    .WithBy(f => new { Price = 200, f.Quantity, UpdatedBy = 2, f.Amount, ProductId = DBNull.Value }, parameters)
    .ToSql(out _);
//UPDATE `sys_order_detail` SET `Price`=200,`UpdatedBy`=2,`ProductId`=NULL,`Quantity`=@Quantity0,`Amount`=@Amount0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Price`=200,`UpdatedBy`=2,`ProductId`=NULL,`Quantity`=@Quantity1,`Amount`=@Amount1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Price`=200,`UpdatedBy`=2,`ProductId`=NULL,`Quantity`=@Quantity2,`Amount`=@Amount2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Price`=200,`UpdatedBy`=2,`ProductId`=NULL,`Quantity`=@Quantity3,`Amount`=@Amount3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Price`=200,`UpdatedBy`=2,`ProductId`=NULL,`Quantity`=@Quantity4,`Amount`=@Amount4 WHERE `Id`=@kId4;UPDATE `sys_order_detail` SET `Price`=200,`UpdatedBy`=2,`ProductId`=NULL,`Quantity`=@Quantity5,`Amount`=@Amount5 WHERE `Id`=@kId5
	
//原理同上
```

Update<T>  Set子句 联合表进行更新
支持的数据库：
Sql Server
PostgreSql
MySql
Oracle

```csharp
//Set子句 From 多个字段
var sql = repository.Update<Order>()
    //new 表达式支持多字段
    .Set((a, b) => new
    {
	TotalAmount = a.From<OrderDetail>('b')
	    .Where(f => f.OrderId == b.Id)
	    .Select(t => Sql.Sum(t.Amount)),
	OrderNo = b.OrderNo + "_111",
	BuyerId = DBNull.Value
    })
    .Where(a => a.BuyerId == 1)
    .ToSql(out _);
//UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1
```
	
```csharp
//Set子句 From 多个字段，与其他Set子句一起使用，单个字段、多个字段都支持
var sql = repository.Update<Order>()
    //new 表达式支持多字段 
    .Set((a, b) => new
    {
	TotalAmount = a.From<OrderDetail>('b')
	    .Where(f => f.OrderId == b.Id)
	    .Select(t => Sql.Sum(t.Amount))
    })
    //单个字段+值方式
    .Set(x => x.OrderNo, "ON_111")
    //单个字段、多个字段 表达式方式
    .Set(f => new { BuyerId = DBNull.Value })
    .Where(a => a.BuyerId == 1)
    .ToSql(out _);
//UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`BuyerId`=1
```

Update<T> InnerJoin/LeftJoin 联合表更新
支持的数据库：
MySql

```csharp
//Update<T> InnerJoin 一个或多个字段
var sql = repository.Update<Order>()
    //可以关联一或多个表
    .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
    //单个字段+值方式
    .Set(x => x.TotalAmount, 200.56)
    //new 表达式支持多字段，这里用到了联表
    .Set((a, b) => new
    {
	OrderNo = a.OrderNo + "_111",
	BuyerId = DBNull.Value
    })
    .Where((a, b) => a.BuyerId == 1)
    .ToSql(out _);
//UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId`SET a.`TotalAmount`=@TotalAmount,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1

```
	
```csharp
//Update<T> InnerJoin 一个或多个字段
var sql = repository.Update<Order>()
    .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
    .Set((x, y) => new
    {
	TotalAmount = y.Amount,
	OrderNo = x.OrderNo + "_111",
	BuyerId = DBNull.Value
    })
    .Where((a, b) => a.BuyerId == 1)
    .ToSql(out _);
//UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId`SET a.`TotalAmount`=b.`Amount`,a.`OrderNo`=CONCAT(a.`OrderNo`,'_111'),a.`BuyerId`=NULL WHERE a.`BuyerId`=1
```
	
```csharp	
//Update<T> InnerJoin 一个或多个字段 + Set联合表子句
var sql = repository.Update<Order>()
    .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
    //Set联合表子句，自己单独联合其他表进行更新
    .Set(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
	.Where(f => f.OrderId == y.Id)
	.Select(t => Sql.Sum(t.Amount)))
    //后面2个Set子句，都是和OrderDetail表联合进行更新的
    .Set((a, b) => new { OrderNo = a.OrderNo + b.ProductId.ToString() })
    .Set((x, y) => new { BuyerId = DBNull.Value })
    .Where((a, b) => a.BuyerId == 1)
    .ToSql(out _);
//UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId`SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,CAST(b.`ProductId` AS CHAR)),a.`BuyerId`=NULL WHERE a.`BuyerId`=1
```

Set Null， 是用表达式，用DBNull.Value或是null都可以实现Set Null
```csharp
var sql = repository.Update<Order>()
    .Set(x => new
    {
	BuyerId = DBNull.Value,
	Seller = (int?)null
    })
    .Where(x => x.OrderNo == null)
    .ToSql(out _);
//UPDATE `sys_order` SET `BuyerId`=NULL,`Seller`=NULL WHERE `OrderNo` IS NULL
```

Update<T> From 联合表更新
支持的数据库：
Sql Server
PostgreSql


```csharp
//Update<T> From 同样支持 一个或多个字段 + Set联合表子句
var sql = repository.Update<Order>()
    .From<OrderDetail>()
    .Set(x => x.TotalAmount, 200.56)
    .Set((a, b) => new
    {
	OrderNo = a.OrderNo + "_111",
	BuyerId = DBNull.Value
    })
    .Where((x, y) => x.Id == y.OrderId && x.BuyerId == 1)
    .ToSql(out _);
//UPDATE [sys_order] SET [TotalAmount]=@TotalAmount,[OrderNo]=[sys_order].[OrderNo]+'_111',[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
```


```csharp
//Update<T> From 同样支持 多个字段
var sql = repository.Update<Order>()
    .From<OrderDetail>()
    .Set((x, y) => new
    {
	TotalAmount = y.Amount,
	OrderNo = x.OrderNo + "_111",
	BuyerId = DBNull.Value
    })
    .Where((x, y) => x.Id == y.OrderId && x.BuyerId == 1)
    .ToSql(out _);
//UPDATE [sys_order] SET [TotalAmount]=b.[Amount],[OrderNo]=[sys_order].[OrderNo]+'_111',[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
```  

```csharp
//Update<T> From 同样支持 一个或多个字段 + Set联合表子句
var sql = repository.Update<Order>()
    .From<OrderDetail>()
    .Set(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
	.Where(f => f.OrderId == y.Id)
	.Select(t => Sql.Sum(t.Amount)))
    .Set((a, b) => new { OrderNo = a.OrderNo + b.ProductId.ToString() })
    .Set((x, y) => new { BuyerId = DBNull.Value })
    .Where((x, y) => x.Id == y.OrderId && x.BuyerId == 1)
    .ToSql(out _);
//UPDATE [sys_order] SET [TotalAmount]=(SELECT SUM(c.[Amount]) FROM [sys_order_detail] c WHERE c.[OrderId]=[sys_order].[Id]),[OrderNo]=[sys_order].[OrderNo]+CAST(b.[ProductId] AS NVARCHAR(MAX)),[BuyerId]=NULL FROM [sys_order_detail] b WHERE [sys_order].[Id]=b.[OrderId] AND [sys_order].[BuyerId]=1
```	

# 删除

```csharp
//单个表达式
var count = await repository.DeleteAsync<User>(f => f.Id == 1);	
//DELETE FROM [sys_user] WHERE [Id]=1
```

```csharp
//批量删除 表达式
条件是带有主键的多个对象
var count = await repository.DeleteAsync<User>(new[] { new { Id = 1 }, new { Id = 2 } });
//DELETE FROM [sys_user] WHERE [Id]=@Id0;DELETE FROM [sys_user] WHERE [Id]=@Id1
//批量删除会生成多个删除语句

//多个主键值也可以，这种情况只适合只有一个主键字段
var count = await repository.DeleteAsync<User>(new int[] { 1, 2 });
//DELETE FROM [sys_user] WHERE [Id]=@Id0;DELETE FROM [sys_user] WHERE [Id]=@Id1
```


```csharp
//也支持Where条件表达式
var count = await repository.DeleteAsync<User>(f => new int[] { 1, 2 }.Contains(f.Id));
//DELETE FROM [sys_user] WHERE [Id] IN (1,2)
```
	
同样支持Delete<T> 支持更多的删除操作
```csharp
repository.Delete<User>().Where(f => f.Id == 1).Execute();
repository.Delete<User>().Where(new int[] { 1, 2 }).Execute()
	
bool? isMale = true;
var sql = repository.Delete<User>()
    .Where(f => f.Name.Contains("kevin"))
    .And(isMale.HasValue, f => f.Age > 25)
    .ToSql(out _);
//DELETE FROM [sys_user] WHERE [Name] LIKE '%kevin%' AND [Age]>25
```

	
仓储对象IRepository，提交事务，设置超时时间
------------------------------------------------------------

```csharp
using var repository = this.dbFactory.Create();
bool? isMale = true;
//设置60秒
repository.Timeout(60);
repository.BeginTransaction();
repository.Update<User>()
    .WithBy(new { Name = "leafkevin1", Id = 1 })
    .Execute();
repository.Delete<User>()
    .Where(f => f.Name.Contains("kevin"))
    .And(isMale.HasValue, f => f.Age > 25)
    .Execute();
repository.Commit();
```


欢迎大家使用
---------------------
欢迎大家广提Issue，我的联系方式：
QQ：39253425
Mail：leafkevin@126.com
