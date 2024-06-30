Trolley - 一个轻量级高性能的.NET ORM框架
========================================


## 框架特点
------------------------------------------------------------
强类型的DDD仓储操作,基本可以不用写SQL,支持多种数据库终端，目前是在.NET 6 基础上开发的。
目前支持：`MySql`,`PostgreSql`,`SqlSever`,其他的`OrmProvider`会稍后慢慢提供。

支持`Page`分页查询
支持`Join`, `GroupBy`, `OrderBy`等操作
支持`Count`, `Max`, `Min`, `Avg`, `Sum`等聚合操作
支持`In`,`Exists`操作
支持`Insert Select From`
支持`Update Join`
支持条件`Insert`，条件`Update`
支持批量`Insert`、`Update`、`Delete`   
支持导航属性，值对象导航属性(瘦身版模型)
支持模型映射，采用流畅API方式，目前不支持特性方式映射  
支持多租户分库，不同租户不同的数据库。  

## 引入`Trolley`对应数据库驱动的Nuget包，在系统中要注册`IOrmDbFactory`并注册映射  
------------------------------------------------------------
系统中每个连接串对应一个`OrmProvider`，每种类型的`OrmProvider`以单例形式存在，一个应用中可以存在多种类型的`OrmProvider`。
引入`Trolley`对应数据库驱动的Nuget包，如：`Trolley.MySqlConnector`，`Trolley.SqlServer`...等   
在`Trolley`中，一个`dbKey`代表一个数据库，可以是不同的`OrmProvider`。  
必须指定`dbKey`才能创建`IOrmDbFactory`对象，没有指定就是使用默认的`dbKey`,默认数据库。  

示例:
```csharp
var dbFactory = new OrmDbFactoryBuilder()
	.Register<MySqlProvider>("fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4, true)
	.AddTypeHandler<JsonTypeHandler>()
	.Configure<MySqlProvider, MySqlModelConfiguration>();
return builder.Build();
```

多个不同数据库的场景
```csharp
var connectionString1 = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
var connectionString2 = "User id=postgres;Password=123456;Host=localhost;Port=5432;Database=fengling;Pooling=true;Min Pool Size=5;Max Pool Size=100;";
var builder = new OrmDbFactoryBuilder()
	.Register<MySqlProvider>("fengling", connectionString1，true)
	.Register<NpgSqlProvider>("fengling_tanent001", connectionString2, false)
	.AddTypeHandler<JsonTypeHandler>()
	.Configure<MySqlProvider, MySqlModelConfiguration>()
	.Configure<MySqlProvider, NpgSqlModelConfiguration>();
var dbFactory = builder.Build();
```

在注册`IOrmDbFactory`的时候，同时也要把数据库结构的模型映射配置出来。
模型映射采用的是Fluent Api方式，类似EF，通常是继承`IModelConfiguration`的子类。
Trolley, 目前只支持Fluent Api方式，这样能使模型更加纯净，不受ORM污染。

导航属性的设置，是单向的，只需要把本模型内的导航属性列出来就可以了。
对应的导航属性类，再设置它所引用的模型映射。
这里的`ModelConfiguration`类，就是模型映射类，内容如下：
```csharp
class ModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(f =>
        {
            //设置列映射，可以只设置特殊列，如：主键列、自增列、枚举字符串列等等，在Trolley Build的时候，会自动根据模型结构按照默认映射添加进来的。
            //也可以全部列出来。映射的NativeDbType可以是整形数也可以是对应的数据库驱动中的本地DbType，如：SqlDbType，或是MySqlDbType...类型等。
            f.ToTable("sys_user").Key(t => t.Id);
			f.Member(t => t.Id).Field(nameof(User.Id)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(1).Required();
			f.Member(t => t.TenantId).Field(nameof(User.TenantId)).DbColumnType("varchar(255)").NativeDbType(MySqlDbType.VarChar).Position(2).Length(255);
			f.Member(t => t.Name).Field(nameof(User.Name)).DbColumnType("varchar(255)").NativeDbType(MySqlDbType.VarChar).Position(3).Length(255);
			f.Member(t => t.Gender).Field(nameof(User.Gender)).DbColumnType("enum('Unknown','Male','Female')").NativeDbType(MySqlDbType.Enum).Position(4);
			f.Member(t => t.Age).Field(nameof(User.Age)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(5).Required();
			f.Member(t => t.CompanyId).Field(nameof(User.CompanyId)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(6);
			f.Member(t => t.GuidField).Field(nameof(User.GuidField)).DbColumnType("char(36)").NativeDbType(MySqlDbType.Guid).Position(7);
			f.Member(t => t.SomeTimes).Field(nameof(User.SomeTimes)).DbColumnType("time(6)").NativeDbType(MySqlDbType.Time).Position(8);
			f.Member(t => t.SourceType).Field(nameof(User.SourceType)).DbColumnType("enum('Website','Wechat','Douyin','Taobao')").NativeDbType(MySqlDbType.Enum).Position(9);
			f.Member(t => t.IsEnabled).Field(nameof(User.IsEnabled)).DbColumnType("bit(1)").NativeDbType(MySqlDbType.Bool).Position(10).Required();
			f.Member(t => t.CreatedAt).Field(nameof(User.CreatedAt)).DbColumnType("datetime(3)").NativeDbType(MySqlDbType.DateTime).Position(11).Required();
			f.Member(t => t.CreatedBy).Field(nameof(User.CreatedBy)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(12).Required();
			f.Member(t => t.UpdatedAt).Field(nameof(User.UpdatedAt)).DbColumnType("datetime(3)").NativeDbType(MySqlDbType.DateTime).Position(13).Required();
			f.Member(t => t.UpdatedBy).Field(nameof(User.UpdatedBy)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(14).Required();

            //导航属性的设置，是单向的，只需要把本模型内的导航属性列出来就可以了。  
            //对应的导航属性类，在应设置再设置它所引用的类型映射。  
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();//导航属性，这里是值对象，不是真正的模型，是模型Company的瘦身版，使用MapTo指定对应的模型Company
            f.HasMany(t => t.Orders).HasForeignKey(t => t.BuyerId);
        });
        builder.Entity<Company>(f =>
        {
            f.ToTable("sys_company").Key(t => t.Id);//表，主键
            //自动增长列
            f.Member(t => t.Id).Field(nameof(Company.Id)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(1).AutoIncrement().Required();
			f.Member(t => t.Name).Field(nameof(Company.Name)).DbColumnType("varchar(50)").NativeDbType(MySqlDbType.VarChar).Position(2).Length(50);
			f.Member(t => t.Nature).Field(nameof(Company.Nature)).DbColumnType("varchar(50)").NativeDbType(MySqlDbType.VarChar).Position(3).Length(50);
			f.Member(t => t.IsEnabled).Field(nameof(Company.IsEnabled)).DbColumnType("tinyint(1)").NativeDbType(MySqlDbType.Bool).Position(4);
			f.Member(t => t.CreatedAt).Field(nameof(Company.CreatedAt)).DbColumnType("datetime").NativeDbType(MySqlDbType.DateTime).Position(5);
			f.Member(t => t.CreatedBy).Field(nameof(Company.CreatedBy)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(6);
			f.Member(t => t.UpdatedAt).Field(nameof(Company.UpdatedAt)).DbColumnType("datetime").NativeDbType(MySqlDbType.DateTime).Position(7);
			f.Member(t => t.UpdatedBy).Field(nameof(Company.UpdatedBy)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(8);

            //导航属性的设置，是单向的，只需要把本模型内的导航属性列出来就可以了。  
            //对应的导航属性类，在应设置再设置它所引用的类型映射。  
            f.HasMany(t => t.Users).HasForeignKey(t => t.CompanyId);//导航属性，这里是真正的模型
        });
        builder.Entity<Order>(f =>
        {
            f.ToTable("sys_order").Key(t => t.Id);
			f.Member(t => t.Id).Field(nameof(Order.Id)).DbColumnType("varchar(50)").NativeDbType(MySqlDbType.VarChar).Position(1).Length(50).Required();
			f.Member(t => t.TenantId).Field(nameof(Order.TenantId)).DbColumnType("varchar(50)").NativeDbType(MySqlDbType.VarChar).Position(2).Length(50).Required();
			f.Member(t => t.OrderNo).Field(nameof(Order.OrderNo)).DbColumnType("varchar(50)").NativeDbType(MySqlDbType.VarChar).Position(3).Length(50);
			f.Member(t => t.ProductCount).Field(nameof(Order.ProductCount)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(4);
			f.Member(t => t.TotalAmount).Field(nameof(Order.TotalAmount)).DbColumnType("double").NativeDbType(MySqlDbType.Double).Position(5);
			f.Member(t => t.BuyerId).Field(nameof(Order.BuyerId)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(6);
			f.Member(t => t.BuyerSource).Field(nameof(Order.BuyerSource)).DbColumnType("varchar(50)").NativeDbType(MySqlDbType.VarChar).Position(7).Length(50);
			f.Member(t => t.SellerId).Field(nameof(Order.SellerId)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(8);
			
            //特殊类型JSON，Trolley预置了对json的处理，直接引用JsonTypeHandler，也可以自定义类型处理，只需要实现ITypeHandler就可以
            //如果列是Class类型，没有设置导航属性，也没有设置ITypeHandler，也没有设置Ignore将会报错
            f.Member(t => t.Products).Field(nameof(Order.Products)).DbColumnType("longtext").NativeDbType(MySqlDbType.JSON).Position(9).TypeHandler<JsonTypeHandler>();
			f.Member(t => t.Disputes).Field(nameof(Order.Disputes)).DbColumnType("longtext").NativeDbType(MySqlDbType.JSON).Position(10).TypeHandler<JsonTypeHandler>();
			f.Member(t => t.IsEnabled).Field(nameof(Order.IsEnabled)).DbColumnType("tinyint(1)").NativeDbType(MySqlDbType.Bool).Position(11);
			f.Member(t => t.CreatedAt).Field(nameof(Order.CreatedAt)).DbColumnType("datetime").NativeDbType(MySqlDbType.DateTime).Position(12);
			f.Member(t => t.CreatedBy).Field(nameof(Order.CreatedBy)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(13);
			f.Member(t => t.UpdatedAt).Field(nameof(Order.UpdatedAt)).DbColumnType("datetime").NativeDbType(MySqlDbType.DateTime).Position(14);
			f.Member(t => t.UpdatedBy).Field(nameof(Order.UpdatedBy)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(15);
	    
            f.HasOne(t => t.Buyer).HasForeignKey(t => t.BuyerId);	    
            f.HasOne(t => t.Seller).HasForeignKey(t => t.SellerId).MapTo<User>();//导航属性，这里是值对象，不是真正的模型，是模型User的瘦身版，使用MapTo指定对应的模型User
            f.HasMany(t => t.Details).HasForeignKey(t => t.OrderId);
        });
        builder.Entity<OrderDetail>(f =>
        {
            f.ToTable("sys_order_detail").Key(t => t.Id);
			f.Member(t => t.Id).Field(nameof(OrderDetail.Id)).DbColumnType("varchar(50)").NativeDbType(MySqlDbType.VarChar).Position(1).Length(50).Required();
			f.Member(t => t.TenantId).Field(nameof(OrderDetail.TenantId)).DbColumnType("varchar(50)").NativeDbType(MySqlDbType.VarChar).Position(2).Length(50).Required();
			f.Member(t => t.OrderId).Field(nameof(OrderDetail.OrderId)).DbColumnType("varchar(50)").NativeDbType(MySqlDbType.VarChar).Position(3).Length(50);
			f.Member(t => t.ProductId).Field(nameof(OrderDetail.ProductId)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(4);
			f.Member(t => t.Price).Field(nameof(OrderDetail.Price)).DbColumnType("double(10,2)").NativeDbType(MySqlDbType.Double).Position(5);
			f.Member(t => t.Quantity).Field(nameof(OrderDetail.Quantity)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(6);
			f.Member(t => t.Amount).Field(nameof(OrderDetail.Amount)).DbColumnType("double(10,2)").NativeDbType(MySqlDbType.Double).Position(7);
			f.Member(t => t.IsEnabled).Field(nameof(OrderDetail.IsEnabled)).DbColumnType("tinyint(1)").NativeDbType(MySqlDbType.Bool).Position(8);
			f.Member(t => t.CreatedAt).Field(nameof(OrderDetail.CreatedAt)).DbColumnType("datetime").NativeDbType(MySqlDbType.DateTime).Position(9);
			f.Member(t => t.CreatedBy).Field(nameof(OrderDetail.CreatedBy)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(10);
			f.Member(t => t.UpdatedAt).Field(nameof(OrderDetail.UpdatedAt)).DbColumnType("datetime").NativeDbType(MySqlDbType.DateTime).Position(11);
			f.Member(t => t.UpdatedBy).Field(nameof(OrderDetail.UpdatedBy)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(12);
	    
            f.HasOne(t => t.Order).HasForeignKey(t => t.OrderId);
            f.HasOne(t => t.Product).HasForeignKey(t => t.ProductId);
        });
    }
}
```

`Trolley`底层使用的`DbType`是各个数据库驱动的本地DbType，如：`MySqlProvider`使用的DbType是`MySqlConnector.MySqlDbType`。  
`Trolley`在配置各个数据库模型映射时，可以使用`int`类型，也可以使用本地`DbType`类型，如：`SqlDbType`，或是`MySqlDbType`...类型等  
如果不设置`NativeDbType`类型映射，`Trolley`会按照默认的类型映射完成映射。 
在实际项目中，可会使用`Trolley.T4`中的各个驱动下的`Entities.tt`，`Entity.tt`，`ModelConfiguration.tt`，`ModelConfigurations.tt`模板，来生成。
路径在：`Trolley.T4\SqlServer\ModelConfiguration.tt`, `Trolley.T4\SqlServer\ModelConfigurations.tt`, `Trolley.T4\MySql\ModelConfiguration.tt`, `Trolley.T4\MySql\ModelConfigurations.tt`

对应的模型结构如下：
```csharp
public enum Gender : byte
{
    Unknown = 0,
    Female,
    Male
}
public class User
{
    public int Id { get; set; }
	public string TenantId { get; set; }
    public string Name { get; set; }
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public int CompanyId { get; set; }
    public TimeOnly? SomeTimes { get; set; }
    public Guid? GuidField { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CompanyInfo Company { get; set; }
    public List<Order> Orders { get; set; }
}
public enum CompanyNature
{
    Internet = 1,
    Industry = 2,
    Production = 3
}
public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public CompanyNature? Nature { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<User> Users { get; set; }
    public List<Brand> Brands { get; set; }
    public List<Product> Products { get; set; }
}
//值对象，就是瘦身版模型CompanyInfo，只有两个字段
public class CompanyInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```
在实际应用中，值对象在模型中定义很常见，没必要引用整个模型，真正使用的就是几个栏位，轻量化模型结构。  



### 创建IRepository对象，就可以做各种操作了  
------------------------------------------------------------
所有的操作都是从创建`IRepository`对象开始的，`IRepository`可以开启事务，设置`command`超时时间、设置参数化、各种查询、命令的执行。   
不同模型的操作都是采用`IRepository`泛型方法来完成的。  
所有的查询操作，都支持`ToSql`方法，可以查看生成SQL语句，方便诊断。  
默认情况下，所有数据库操作，Trolley只对变量做了参数化处理。
所有创建的`IRepository`都要使用using，连接使用完立即返回到连接池，以便适应更大的并发请求。
也可以在使用完`IRepository`后，立即调用`Close`/`CloseAsync`及时释放到连接池，下个数据库操作可以直接使用`IRepository`对象，连接自动会打开。
在执行过程中发生异常，如果没有设置Transaction，链接将自动关闭，有设置事务将不会关闭链接。

`dbKey`的选取逻辑：
```csharp
using var repository = this.dbFactory.CreateRepository();
```
如果有指定`dbKey`就是使用指定的`dbKey`创建`IRepository`对象
如果没有指定`dbKey`，再判断是否有指定分库规则，有指定就调用分库规则获取`dbKey`
如果也没有指定分库规则，就使用配置的默认`dbKey`

### 基本简单查询

```csharp
using var repository = this.dbFactory.CreateRepository();
//扩展的简化查询，不支持ToSql方法，是由From语句包装而来的，From语句支持ToSql查看SQL

//QueryFirst方法，查询单条，不支持分表
var result = repository.QueryFirst<User>(f => f.Id == 1);
var result = await repository.QueryFirstAsync<User>(f => f.Name == "leafkevin");

也支持命名对象或是匿名对象，必须实体类型，不能是基础类型
var result = repository.QueryFirst<User>(new { Id = 1 });
var result = await repository.QueryFirstAsync<User>(new { Name = "leafkevin" });
//SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id`=1

//QueryFirst方法，原始SQL
var result = await repository.QueryFirstAsync<Product>("SELECT * FROM sys_product where Id=@ProductId", new { ProductId = 1 });


//Query方法，查询多条，不支持分表
var result = repository.Query<Product>(f => f.ProductNo.Contains("PN-00"));
var result = await repository.QueryAsync<Product>(f => f.ProductNo.Contains("PN-00"));
//SELECT `Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_product` WHERE `ProductNo` LIKE '%PN-00%'

也支持命名对象或是匿名对象，必须实体类型，不能是基础类型
var result = repository.Query<User>(new { Id = 1 });
var result = await repository.QueryAsync<User>(new { Name = "leafkevin" });

//Query方法，原始SQL
var result = await repository.QueryAsync<Product>("SELECT * FROM sys_product where BrandId=@BrandId", new { BrandId = 1 });


//Get方法，根据主键来查询，参数可以是匿名对象，也可以主键值，不支持分表
var result = repository.Get<Product>(1);
//SELECT `Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_product` WHERE `Id`=1

//也可以使用匿名对象
var result = repository.Get<Product>(new { Id = 1 });
```


### `From`查询，支持各种复杂查询，支持分表操作


简单表达式查询

```csharp
using var repository = this.dbFactory.CreateRepository();
var result = await repository.From<Product>()
    .Where(f => f.ProductNo.Contains("PN-00"))
    .ToListAsync();
//SELECT `Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_product` WHERE `ProductNo` LIKE '%PN-00%'
```

`Page` 分页查询

```csharp
var result = repository.From<OrderDetail>()
    .Where(f => f.ProductId == 1)
    .OrderByDescending(f => f.CreatedAt)
	.Page(1, 10)
    .ToPageList();
//SELECT COUNT(*) FROM `sys_order_detail` WHERE `ProductId`=1;SELECT `Id`,`OrderId`,`ProductId`,`Price`,`Quantity`,`Amount`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_order_detail` WHERE `ProductId`=1 ORDER BY `CreatedAt` DESC LIMIT 10 OFFSET 10

//也可以单独使用`Skip`、 `Take`方法实现SQL中的`Offset`, `Limit`功能
var result = repository.From<OrderDetail>()
    .Where(f => f.ProductId == 1)
    .OrderByDescending(f => f.CreatedAt)
	.Skip(100)
	.Take(20)
    .ToPageList();
```

`Include`查询
1:1关系的联表查询，`Include`表数据和主表一起查出来。
子表使用LEFT JOIN连接

```csharp
//One to One  Include
var result = await repository.From<Product>()
    .Include(f => f.Brand)
    .Where(f => f.ProductNo.Contains("PN-00"))
    .ToListAsync();	    
//SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
```

`IncludeMany`查询
1:N关系的联表查询，分两次查询。
第一次查询，会把主表和其他`Join`、`Include`表数据查询出来。
第二次把所有`IncludeMany`的数据都查询出来，再设置到对应主表模型中。

```csharp    
//InnerJoin and IncludeMany    
var result = repository.From<Order>()
    .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
    .IncludeMany((x, y) => x.Details)
    .Where((a, b) => a.TotalAmount > 300)
    .Select((x, y) => new { Order = x, Buyer = y })
    .ToList();

//第一次查询SQL:
//SELECT a.`Id`,a.`OrderNo`,a.`TotalAmount`,a.`BuyerId`,a.`SellerId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`TotalAmount`>300

//第二次查询SQL：
//SELECT `Id`,`OrderId`,`ProductId`,`Price`,`Quantity`,`Amount`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_order_detail` WHERE OrderId IN (1,2)
//第二次查询，会根据第一次查询主表主键数据和Filter条件，再去查询IncludeMany表数据。
```

`IncludeMany`查询 + `Filter`过滤条件

```csharp
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

`Include`后`ThenInclude`查询，都是1:1关系联表
子表使用`LEFT JOIN`连接
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

`Page`分页+`Include`
//也是可以使用`Skip`、`Take`
```csharp
var result = repository.From<OrderDetail>()
    .Include(f => f.Product)
    .Where(f => f.ProductId == 1)
    .Page(2, 10)
    .ToPageList();
var result = repository.From<OrderDetail>()
    .Include(f => f.Product)
    .Where(f => f.ProductId == 1)
    .Skip(10)
    .Take(10)
    .ToPageList();
//SELECT COUNT(*) FROM `sys_order_detail` a LEFT JOIN `sys_product` b ON a.`ProductId`=b.`Id` WHERE a.`ProductId`=1;SELECT a.`Id`,a.`OrderId`,a.`ProductId`,a.`Price`,a.`Quantity`,a.`Amount`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`ProductNo`,b.`Name`,b.`BrandId`,b.`CategoryId`,b.`CompanyId`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt` FROM `sys_order_detail` a LEFT JOIN `sys_product` b ON a.`ProductId`=b.`Id`  WHERE a.`ProductId`=1 LIMIT 10 OFFSET 10
```

虽然有`Include`，但是没有`Select`对应模型字段，会忽略`Include`
```csharp
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
//SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE),COUNT(b.`Id`) AS OrderCount,SUM(b.`TotalAmount`) AS TotalAmount FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,b.`Id`
```

分组查询
`GroupBy`以后，可以使用`IGroupingAggregate`类型的`Grouping`属性
`IGroupingAggregate`类型的`Grouping`属性，是`GroupBy`的所有字段，这里有3个字段：`Id`,`Name`,`Date`

```csharp
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
//SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE),COUNT(b.`Id`) AS OrderCount,SUM(b.`TotalAmount`) AS TotalAmount FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,b.`Id`

//Group 打开Grouping，使用里面的字段
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
 //SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS Date,COUNT(b.`Id`) AS OrderCount,SUM(b.`TotalAmount`) AS TotalAmount FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,b.`Id`
 //打开Grouping属性，和不打开生成的SQL是一样的，唯一不同的地方是：打开时有AS别名，如上面的Date字段，Id,Name与原字段相同就不用AS了
```

`GroupBy`后，可使用`Having`、`OrderBy`
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
//SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS Date,COUNT(b.`Id`) AS OrderCount,SUM(b.`TotalAmount`) AS TotalAmount FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 AND EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND COUNT(DISTINCT f.`ProductId`)>2) ORDER BY a.`Id`,b.`Id`
```

使用`In`、`Exists`
`In`、`Exists`操作是通过静态`Sql`类来完成的，书写起来比较简单，`In`、`Exists`子句不支持分表。
`Exists`也可以使用`From`后，再调用`Exists`方法来完成，这种方式支持分表。

`Sql.In` 有以下方法重载：
bool In&lt;TElement&gt;(TElement value, params TElement[] list);
bool In&lt;TElement&gt;(TElement value, IEnumerable&lt;TElement&gt; list);
bool In&lt;TElement&gt;(TElement value, IQuery&lt;TElement&gt; subQuery);
bool In&lt;TElement&gt;(TElement value, Func&lt;IFromQuery, IQuery&lt;TElement&gt;&gt; subQuery);

`Sql.Exists` 有以下方法重载：
bool Exists(Func&lt;IFromQuery, IQueryAnonymousObject&gt; subQuery);
bool Exists(IQuery&lt;T&gt; subQuery, Expression&lt;Func&lt;T, bool&gt;&gt; predicate) //支持使用CTE子查询
bool Exists&lt;T&gt;(Expression&lt;Func&lt;T, bool&gt;&gt; filter);
bool Exists&lt;T1, T2&gt;(Expression&lt;Func&lt;T1, T2, bool&gt;&gt; filter);
bool Exists&lt;T1, T2, T3&gt;(Expression&lt;Func&lt;T1, T2, T3, bool&gt;&gt; filter)
bool Exists&lt;T1, T2, T3, T4&gt;(Expression&lt;Func&lt;T1, T2, T3, T4, bool&gt;&gt; filter);
bool Exists&lt;T1, T2, T3, T4, T5&gt;(Expression&lt;Func&lt;T1, T2, T3, T4, T5, bool&gt;&gt; filter);
bool Exists&lt;T1, T2, T3, T4, T5, T6&gt;(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, bool&gt;&gt; filter);

```csharp
//In and Exists
var sql = repository.From<User>()
    .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
    .Where((a, b) => Sql.In(a.Id, new int[] { 1, 2, 3 }) && Sql.Exists<OrderDetail>(f => b.Id == f.OrderId && f.ProductId == 2))
    .GroupBy((a, b) => new { a.Id, a.Name, b.CreatedAt.Date })
    .Having((x, a, b) => x.Sum(b.TotalAmount) > 300)
    .OrderBy((x, a, b) => new { UserId = a.Id, b.CreatedAt.Date })
    .Select((x, a, b) => new
    {
        x.Grouping,
        OrderCount = x.Count(b.Id),
        TotalAmount = x.Sum(b.TotalAmount)
    })
    .ToSql(out _);
//SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE a.`Id` IN (1,2,3) AND EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND f.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`,CONVERT(b.`CreatedAt`,DATE)");
 
//子查询
var sql = repository.From<User>()
    .Where(f => Sql.Exists(t => t
		.From<OrderDetail>('b')
        .GroupBy(a => a.OrderId)
		.Having((x, a) => Sql.CountDistinct(a.ProductId) > 0)
		.SelectAnonymous()))
    .GroupBy(f => new { f.Gender, f.CompanyId })
    .Select((t, a) => new { t.Grouping, UserTotal = t.CountDistinct(a.Id) })
    .ToSql(out _);
//SELECT a.`Gender`,a.`CompanyId`,COUNT(DISTINCT a.`Id`) AS `UserTotal` FROM `sys_user` a WHERE EXISTS(SELECT * FROM `sys_order_detail` b GROUP BY b.`OrderId` HAVING COUNT(DISTINCT b.`ProductId`)>0) GROUP BY a.`Gender`,a.`CompanyId`

//子查询和CTE子查询
var myOrders = repository.From<OrderDetail, Order>()
    .Where((a, b) => a.OrderId == b.Id)
    .GroupBy((a, b) => new { a.OrderId, b.BuyerId })
    .Having((x, a, b) => x.CountDistinct(a.ProductId) > 2)
    .Select((x, a, b) => x.Grouping)
    .AsCteTable("myOrders");
var result = repository.From<User>()
    .InnerJoin<Company>((a, b) => a.CompanyId == b.Id)
    .Where((x, y) => Sql.Exists(myOrders, f => f.BuyerId == x.Id))
    .Select((a, b) => new { a.Id, a.Name, CompanyName = b.Name })
    .First();
SQL:
WITH `myOrders`(`OrderId`,`BuyerId`) AS 
(
SELECT a.`OrderId`,b.`BuyerId` FROM `sys_order_detail` a,`sys_order` b WHERE a.`OrderId`=b.`Id` GROUP BY a.`OrderId`,b.`BuyerId` HAVING COUNT(DISTINCT a.`ProductId`)>2
)
SELECT a.`Id`,a.`Name`,b.`Name` AS `CompanyName` FROM `sys_user` a INNER JOIN `sys_company` b ON a.`CompanyId`=b.`Id` WHERE EXISTS(SELECT * FROM `myOrders` f WHERE f.`BuyerId`=a.`Id`)");
```

聚合查询,可以使用`SelectAggregate`可以聚合查询，也可以使用`Select`+`Sql`静态类
两者生成的SQL完全一样的
```csharp
//SelectAggregate方法
在没有GroupBy分组的情况，使用聚合查询，不同的数据库支持的场景都不一样。
下面的语句在MySql中可以执行，在SqlServer中不能执行
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

//Sql静态类
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
```

子查询，From和WithTable
From，主要在最开始时使用
WithTable，适合From之后的任意地方使用
两者生成的SQL完全一样的
From之后的子查询，完全可以由Join完成，Join本身就支持子查询
```csharp
var sql = repository
    .From(f => f.From<Order>()
        .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
        .GroupBy((a, b) => new { OrderId = a.Id, a.BuyerId })
        .Select((x, a, b) => new { x.Grouping, ProductCount = x.CountDistinct(b.ProductId) }))
    .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
    .Where((a, b) => a.ProductCount > 1)
    .Select((x, y) => new
    {
        x.Grouping,
        Buyer = y,
        x.ProductCount
    })
    .ToSql(out _);
//SELECT a.`OrderId`,a.`BuyerId`,b.`Id`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT a.`Id` AS `OrderId`,a.`BuyerId`,COUNT(DISTINCT b.`ProductId`) AS `ProductCount` FROM `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId` GROUP BY a.`Id`,a.`BuyerId`) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1

//可以多个表直接查询
var sql = repository
    .From(f => f.From<Page, Menu>('o')
        .Where((a, b) => a.Id == b.PageId)
        .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
    .InnerJoin<Menu>((a, b) => a.Id == b.Id)
    .Where((a, b) => a.Id == b.Id)
    .Select((a, b) => new { a.Id, b.Name, a.ParentId, a.Url })
    .ToSql(out _);
//SELECT a.`Id`,b.`Name`,a.`ParentId`,a.`Url` FROM (SELECT p.`Id`,p.`ParentId`,o.`Url` FROM `sys_page` o,`sys_menu` p WHERE o.`Id`=p.`PageId`) a INNER JOIN `sys_menu` b ON a.`Id`=b.`Id` WHERE a.`Id`=b.`Id`");

//WidthTable子查询
var sql = repository.From<Menu>()
    .WithTable(f => f.From<Page, Menu>('c')
        .Where((a, b) => a.Id == b.PageId)
        .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
    .Where((a, b) => a.Id == b.Id)
    .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
    .ToSql(out _);
//SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `sys_menu` a,(SELECT d.`Id`,d.`ParentId`,c.`Url` FROM `sys_page` c,`sys_menu` d WHERE c.`Id`=d.`PageId`) b WHERE a.`Id`=b.`Id`

var sql = repository
    .From<Order, User>()
    .WithTable(f => f.From<Order, OrderDetail, User>()
        .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
        .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
        .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
        .Select((x, a, b, c) => new { x.Grouping.OrderId, TotalAmount = x.Sum(b.Amount) }))
    .Where((a, b, c) => a.BuyerId == b.Id && a.Id == c.OrderId)
    .Select((a, b, c) => new { Order = a, Buyer = b, OrderId = a.Id, a.BuyerId, c.TotalAmount })
    .ToSql(out _);
//SELECT a.`Id`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`Id` AS `OrderId`,a.`BuyerId`,c.`TotalAmount` FROM `sys_order` a,`sys_user` b,(SELECT a.`Id` AS `OrderId`,SUM(b.`Amount`) AS `TotalAmount` FROM `sys_order` a,`sys_order_detail` b,`sys_user` c WHERE a.`Id`=b.`OrderId` AND a.`BuyerId`=c.`Id` AND c.`Age`>20 GROUP BY a.`Id`,a.`BuyerId` HAVING SUM(b.`Amount`)>500) c WHERE a.`BuyerId`=b.`Id` AND a.`Id`=c.`OrderId`
```
> 注意：
> WithTable就相当于添加一张子查询表，后续可以Join关联，也可以在where中直接关联，类似于：SELECT * FROM Table1 a,(....) b WHERE ...
> 如果使用Join关联的话，Join操作本身就可以直接关联子查询，无需使用WithTable了。




`Join`表连接,`Trolley`支持三种`Join`表连接，`InnerJoin`、`LeftJoin`、`RightJoin`
有两种方式`Join`关联表：
1.一张表一张表的`Join`关联起来
2.一次`From`多张表，一次只能关联两个表，但可以多次Join关联
直接关联实体表，也可以关联子查询表，相当于先`WithTable`后再`Join`

```csharp
//INNER JOIN
//一张表一张表关联
var sql = repository.From<User>()
    .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
    .Where((a, b) => b.ProductCount > 1)
    .Select((x, y) => new
    {
        User = x,
        Order = y
    })
    .ToSql(out _);
//SELECT a.`Id`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE b.`ProductCount`>1

//直接From多张表，分别连接，每次只能关联两张表
var sql = repository.From<User, Order, OrderDetail>()
    .InnerJoin((a, b, c) => a.Id == b.BuyerId)
    .LeftJoin((a, b, c) => b.Id == c.OrderId)
    .Select((a, b, c) => new { OrderId = b.Id, b.OrderNo, b.Disputes, b.BuyerId, Buyer = a, TotalAmount = Sql.Sum(c.Amount) })
    .ToSql(out _);
//SELECT b.`Id` AS `OrderId`,b.`OrderNo`,b.`Disputes`,b.`BuyerId`,a.`Id`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,SUM(c.`Amount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` LEFT JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId`


//Join子查询
var sql = repository
    .From(f => f.From<Order, OrderDetail>('a')
        .Where((a, b) => a.Id == b.OrderId)
        .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
        .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
        .Select((x, a, b) => new { x.Grouping, ProductTotal = Sql.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
    .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
    .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
    .ToSql(out _);
//SELECT a.`BuyerId`,a.`OrderId`,a.`BuyerId`,a.`ProductTotal`,b.`Name` AS `BuyerName`,a.`BuyerId1` AS `BuyerId2` FROM (SELECT a.`BuyerId`,a.`Id` AS `OrderId`,COUNT(DISTINCT b.`ProductId`) AS `ProductTotal`,a.`BuyerId` AS `BuyerId1` FROM `sys_order` a,`sys_order_detail` b WHERE a.`Id`=b.`OrderId` GROUP BY a.`BuyerId`,a.`Id` HAVING COUNT(DISTINCT b.`ProductId`)>0) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id`

//LeftJoin查询，三个Join写法一样，只是方法名字不同
var sql = repository.From<Product>()
    .LeftJoin<Brand>((a, b) => a.BrandId = b.Id)
    .Where((a, b) => a.ProductNo.Contains("PN-00"))
    .ToSql(out _);
//SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
```
> 注意：
> 使用Join<T>(f=>f.From...)的子查询，相当于WithTable+Join两个操作


单表聚合操作
使用`Sql`静态类和直接使用`Count`、`CountDistinct`、`LongConunt`、`Max`、`Min`、`Sum`、`Avg`方法，效果是一样的

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

Union和UnionAll查询

```csharp
var sql = repository.From<Order>()
    .Where(x => x.Id == 1)
    .Select(x => new
    {
        x.Id,
        x.OrderNo,
        x.SellerId,
        x.BuyerId
    })
    .UnionAll(f => f.From<Order>()
        .Where(x => x.Id > 1)
        .Select(x => new
        {
            x.Id,
            x.OrderNo,
            x.SellerId,
            x.BuyerId
        }))
    .ToSql(out _);
//生成的SQL:
SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`=1 UNION ALL
SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`>1");

//带有OrderBy和Take的Union，会变成一个子查询用SELECT * FROM ()包装一下，在里面完成OrderBy和Take操作
var sql = repository
    .From<Order>()
        .Where(x => x.Id < 3)
        .OrderBy(f => f.Id)
        .Select(x => new
        {
            x.Id,
            x.OrderNo,
            x.SellerId,
            x.BuyerId
        })
        .Take(1)
    .UnionAll(f => f.From<Order>()
        .Where(x => x.Id > 2)
        .Select(x => new
        {
            x.Id,
            x.OrderNo,
            x.SellerId,
            x.BuyerId
        }).Take(1))
    .ToSql(out _);
//生成的SQL:
SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`<3 ORDER BY a.`Id` LIMIT 1) a UNION ALL
SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`>2 LIMIT 1) a"
```
> 注意：
> 带有OrderBy和Take的Union，会生成一个子查询，在里面完成OrderBy和Take操作


`CTE`支持
CTE其实也是一个子查询，使用AsCteTable(string tableName)方法把一个子查询包装成一个CTE表
可以在查询中直接使用，也可以单独声明使用，两者效果是一样的
在CTE的子查询中，使用UnionAllRecursive方法，可以自身引用实现递归查询

```csharp
//直接在查询中使用
int menuId = 2;
int pageId = 1;
var sql = repository
	.From(f => f.From<Menu>()
		.Where(t => t.Id >= menuId)
		.Select(x => new { x.Id, x.Name, x.ParentId, x.PageId })
		.AsCteTable("MenuList"))
	.InnerJoin<Page>((a, b) => a.Id == b.Id)
	.Where((x, y) => y.Id >= pageId)
	.Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
	.ToSql(out var dbParameters);

生成的SQL:
WITH `MenuList`(`Id`,`Name`,`ParentId`,`PageId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a WHERE a.`Id`>=@p0
)
SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `MenuList` a INNER JOIN `sys_page` b ON a.`Id`=b.`Id` WHERE b.`Id`>=@p1


//也可以单独声明CTE表，在后续的查询中使用
var myCteTable1 = repository
    .From<Menu>()
        .Where(x => x.Id == rootId)
        .Select(x => new { x.Id, x.Name, x.ParentId })
    .UnionAllRecursive((x, self) => x.From<Menu>()
        .InnerJoin(self, (a, b) => a.ParentId == b.Id)
        .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
    .AsCteTable("myCteTable1");

//上面子句中，包含UnionAllRecursive方法，递归查询
var myCteTable2 = repository
    .From<Page, Menu>()
        .Where((a, b) => a.Id == b.PageId)
        .Select((x, y) => new { y.Id, y.ParentId, x.Url })
    .UnionAll(x => x.From<Menu>()
        .InnerJoin<Page>((a, b) => a.PageId == b.Id)
        .Select((x, y) => new { x.Id, x.ParentId, y.Url }))
    .AsCteTable("myCteTable2");

var sql = repository
    .From(myCteTable1)
    .InnerJoin(myCteTable2, (a, b) => a.Id == b.Id)
    .Select((a, b) => new { a.Id, b.Name, a.ParentId, a.Url })
    .ToSql(out _);
	
生成的SQL:
WITH RECURSIVE `myCteTable1`(`Id`,`Name`,`ParentId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a WHERE a.`Id`=@p0 UNION ALL
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a INNER JOIN `myCteTable1` b ON a.`ParentId`=b.`Id`
),
`myCteTable2`(`Id`,`ParentId`,`Url`) AS 
(
SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE a.`Id`=b.`PageId` UNION ALL
SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN `sys_page` b ON a.`PageId`=b.`Id`
)
SELECT b.`Id`,a.`Name`,b.`ParentId`,b.`Url` FROM `myCteTable1` a INNER JOIN `myCteTable2` b ON a.`Id`=b.`Id`

多个CTE子句，单独声明更清晰一点
单独声明CTE表，可以在多个查询中引用，通常会发生参数名重复情况，可以使用ToParameter方法，改变参数名，如下：

int rootId = 1;
var menuList = repository
    .From<Menu>()
        .Where(x => x.Id == rootId.ToParameter("@RootId"))
        .Select(x => new { x.Id, x.Name, x.ParentId })
    .UnionAllRecursive((x, y) => x.From<Menu>()
        .InnerJoin(y, (a, b) => a.ParentId == b.Id)
        .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
    .AsCteTable("MenuList");

rootId变量生成的参数名为：@RootId,这样可以避免后面查询中的参数重名

多个CTE表也可以直接声明使用
var result = await repository
    .From(f => f.From<Menu>()
            .Where(x => x.Id == menuId)
            .Select(x => new { x.Id, x.Name, x.ParentId })
        .UnionAllRecursive((x, y) => x.From<Menu>()
            .InnerJoin(y, (a, b) => a.ParentId == b.Id)
            .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
        .AsCteTable("myCteTable1"))
    .WithTable(f => f.From<Page>()
            .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
            .Where((a, b) => a.Id == pageId)
            .Select((x, y) => new { y.Id, x.Url })
        .UnionAll(x => x.From<Page>()
            .InnerJoin<Menu>((a, b) => a.Id == b.PageId)
            .Where((a, b) => a.Id > pageId2)
            .Select((x, y) => new { y.Id, x.Url }))
        .AsCteTable("myCteTable2"))
    .InnerJoin((a, b) => a.Id == b.Id)
    .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
	.ToListAsync();

生成的SQL:
WITH RECURSIVE `myCteTable1`(`Id`,`Name`,`ParentId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a WHERE a.`Id`=@p0 UNION ALL
SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a INNER JOIN `myCteTable1` b ON a.`ParentId`=b.`Id`
),
`myCteTable2`(`Id`,`Url`) AS 
(
SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `sys_menu` b ON a.`Id`=b.`PageId` WHERE a.`Id`=@p1 UNION ALL
SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `sys_menu` b ON a.`Id`=b.`PageId` WHERE a.`Id`>@p2
)
SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `myCteTable1` a INNER JOIN `myCteTable2` b ON a.`Id`=b.`Id`

一般CTE常用来处理树形结构递归操作，比如：根据当前角色获取菜单列表
从叶子找菜单根，或是从菜单根查找所有叶子，都是递归CTE的常用场景
```


特殊用法：

对`NULL`的支持
`Nullable<>`字段，可以使用`null`进行判断，赋值
```csharp
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

对于非`Nullable<>`字段，也可以使用`IsNull()`扩展方法来进行判断
```csharp
var sql = repository.From<Order>()
    .Where(x => x.ProductCount == null || x.BuyerId.IsNull())
    .And(true, f => !f.ProductCount.HasValue)
    .Select(x => x.Id)
    .ToSql(out _);
//SELECT `Id` FROM `sys_order` WHERE (`ProductCount` IS NULL OR `BuyerId` IS NULL) AND `ProductCount` IS NULL
```

`ITypeHandler`类型处理器
对特殊类型进行处理，不是默认映射，就需要`TypeHandler`类型处理器类处理，完成模型与数据库之间的数据转换
通常是Class类型或是特殊类型，比如：`TimeOnly`,`DateOnly`...等
比如：模型的`SomeTime`属性是`TimeOnly`类型，数据库字段是`bitint`类型,并不是默认映射，就需要重写一个类型处理器，完成模型与数据库之间的数据转换。


在要注册`Trolley`的时候，进行注册`ITypeHandler`类型处理器，在模型映射的需要指定这个`ITypeHandler`类型处理器
`Trolley`提供了`JsonTypeHandler`类来支持`Json`处理。

```csharp
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

//略 ... ...
builder.Entity<Order>(f =>
{
    //略 ... ...
    //特殊类型JSON
    f.Member(t => t.Products).Field(nameof(Order.Products)).DbColumnType("longtext").NativeDbType(MySqlDbType.JSON).Position(9).TypeHandler<JsonTypeHandler>();
    f.Member(t => t.Disputes).Field(nameof(Order.Disputes)).DbColumnType("longtext").NativeDbType(MySqlDbType.JSON).Position(10).TypeHandler<JsonTypeHandler>();
    //略 ... ...
});

//Order模型的属性Products和Disputes，数据库中都是Json类型，Products类型是List<int>,Disputes类型是Dispute类
var result = repository.Get<Order>(1);
Assert.NotNull(result);
Assert.NotNull(result.Products);
Assert.NotNull(result.Disputes);
```


`Select`操作，可以使用`SelectFlattenTo`方法，直接完成到目标类型的直接转换，减少很多代码量，通常是`DTO`。
`SelectFlattenTo`方法，会先按照方法参数中指定的字段进行设置，其他字段会根据当前所有`Select`出来的字段，根据相同的名称进行设置目标属性，如果有相同的字段，取第一个表的字段。
`SelectFlattenTo`方法，会从数据库直接和DTO类型映射，不会生成模型。

```cshar
var result = repository.From<Order>()
    .Where(f => Sql.In(f.Id, new[] { "8" }))
    .SelectFlattenTo<OrderInfo>()
    .ToList();
//所有的字段，不做特殊处理
result = repository.From<Order>()
    .Where(f => Sql.In(f.Id, new[] { "8" }))
    .SelectFlattenTo(f => new OrderInfo
    {
        Description = "TotalAmount:" + f.TotalAmount
    })
    .ToList();
//只有Description字段，做特殊处理，其他字段直接映射
```

对本地方法调用的支持
`Trolley`会优先查询数据，查询完后，再调用本地方法完成实体映射
比如：获取枚举名称，调用本地方法

```csharp
public enum ActivityType
{
    [Description("线上")]
    Online,
    [Description("线下")]
    Offline
}
public class ActivityQueryResponse
{
    public string Id { get; set; }
    public string Title { get; set; }
    //枚举类型
    public ActivityType ActivityType { get; set; }
    //枚举类型
    public ActivityStatus Status { get; set; }
    ... ...

    public string ActivityTypeName { get; set; }
    public string StatusName { get; set; }
}
private static ConcurrentDictionary<Type, Dictionary<object, string>> enumDescriptions = new();
//扩展方法ToDescription，获取枚举的描述并缓存
public static string ToDescription<TEnum>(this TEnum enumObj) where TEnum : struct, Enum
{
    var enumType = typeof(TEnum);
    object enumValue = null;
    if (enumObj is TEnum typedValue)
        enumValue = typedValue;
    else enumValue = Enum.ToObject(enumType, enumObj);
    if (!enumDescriptions.TryGetValue(enumType, out var descriptions))
    {
        var enumValues = Enum.GetValues(enumType);
        descriptions = new Dictionary<object, string>();
        foreach (var value in enumValues)
        {
            string description = null;
            var enumName = Enum.GetName(enumType, value);
            var fieldInfo = enumType.GetField(enumName);
            if (fieldInfo != null)
            {
                var descAttr = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
                if (descAttr != null)
                    description = descAttr.Description;
            }
            descriptions.Add(value, description ?? enumName);
        }
        enumDescriptions.TryAdd(enumType, descriptions);
    }
    return descriptions[enumValue];
}

var passport = this.User.ToPassport();
var result = await repository.From<Activity>()
    .Where(f => f.IsEnabled && f.TenantId == passport.TenantId)
    .And(!string.IsNullOrEmpty(request.Title), f => f.Title == request.Title)
    .SelectFlattenTo<ActivityQueryResponse>(f => new
    {
        ActivityTypeName = f.ActivityType.ToDescription(),
        StatusName = f.Status.ToDescription()
    }))
    .Page(request.PageIndex, request.PageSize)
    .ToPageListAsync();
//属性ActivityTypeName和StatusName做了特殊处理，其他的属性根据名称相同匹配原则，自动设置到ActivityQueryResponse中

var result = await repository.From<Activity>()
    .Where(f => f.IsEnabled && f.TenantId == passport.TenantId)
    .And(!string.IsNullOrEmpty(request.Title), f => f.Title == request.Title)
    .SelectFlattenTo<ActivityQueryResponse>(f => new
    {
        ActivityTypeName = $"{f.ActivityType}",
        StatusName = $"{f.Status}"
    }))
    .Page(request.PageIndex, request.PageSize)
    .ToPageListAsync();
//获取枚举名称，也会延迟调用获取名称方法Enum.GetName
```

有些方法的调用，解析到数据库中去执行会发生错误，可以使用Deferred方法强制延迟执行，在数据库执行之后，再执行

```csharp
var result = repository.From<Order>()
    .Where(f => Sql.In(f.Id, new[] { "8" }))
    .SelectFlattenTo(f => new OrderInfo
    {
        Description = this.DeferInvoke().Deferred()
    })
    .ToList();
private string DeferInvoke() => "DeferInvoke";
```

指定参数名称，ToParameter方法，如下：

```csharp
int rootId = 1;
var menuList = repository
    .From<Menu>()
        .Where(x => x.Id == rootId.ToParameter("@RootId"))
        .Select(x => new { x.Id, x.Name, x.ParentId })
    .UnionAllRecursive((x, y) => x.From<Menu>()
        .InnerJoin(y, (a, b) => a.ParentId == b.Id)
        .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
    .AsCteTable("MenuList");
```
`ValueTuple`,多字段`Struct`支持

```csharp
//查询ValueTuple
var sql = "SELECT Id,OrderNo,TotalAmount FROM sys_order";
var result = repository.Query<(int OrderId, string OrderNo, double TotalAmount)>(sql);
```
> 注意：
> 有`DTO`对象接收，有`SelectFlattenTo`方法支持，无需使用`ValueTuple`

`IsNull`扩展方法
有两个方法
bool IsNull<TField>(this TField field)
TField IsNull<TField>(this TField field, TField nullVaueExpr)

有时候数据库字段是可为空的，实体字段却是不是可为空类型，需要判断是否为空，可以使用本方法
```csharp
var result = repository.From<Order>()
   .Where(f => f.BuyerId.IsNull())
   .First();

var sql = repository.From<Order>()
   .Where(x => x.ProductCount.IsNull(0) > 0 || x.BuyerId.IsNull(0) >= 0)
   .Select(f => new
   {
       f.Id,
       f.OrderNo,
       ProductCount = f.ProductCount.IsNull(0),
       BuyerId = f.BuyerId.IsNull(0),
       TotalAmount = f.TotalAmount.IsNull(0)
   })
   .ToSql(out _);
```

跨库查询
------------------------------------------------------------
在实际项目，经常会有同时访问多个数据库，可能都是不同的`OrmProvider`，也就是不同种类的数据库。
`Trolley`支持跨库查询，只需要指定`dbKey`就可以了。 
如果有指定分库规则，则会根据分库规则来获取dbKey，无需指定dbKey了

`appsettin.json`中数据库连接的配置：
```json
"ConnectionString": {
    "sqlServer": "Server=127.0.0.1;Database=fengling;Uid=sa;password=ABCwsx123456;TrustServerCertificate=true",
    "mySql": "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;"
  }
```

```csharp
var services = new ServiceCollection();
services.AddSingleton(f =>
{
    var builder = new OrmDbFactoryBuilder()
		.Register<SqlServerProvider>("dbKey1", "Server=127.0.0.1;Database=fengling;Uid=sa;password=ABCwsx123456;TrustServerCertificate=true", true)
		.Register<MySqlProvider>("dbKey2", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;")
		.AddTypeHandler<JsonTypeHandler>()
		.Configure<SqlServerProvider, SqlServerModelConfiguration>()
		.Configure<MySqlProvider, MySqlModelConfiguration>();
    return builder.Build();
});
var serviceProvider = services.BuildServiceProvider();
dbFactory = serviceProvider.GetService<IOrmDbFactory>();

using var repository = this.dbFactory.CreateRepository("dbKey1");
访问的就是SqlServer数据库

using var repository = this.dbFactory.CreateRepository("dbKey2");
访问的就是MySql数据库
```

指定分库规则，`Trolley`会根据分库规则来获取`dbKey`
下面的例子，是根据当前token中的租户ID来获取dbKey
```csharp
public interface IPassport
{
    //只用于演示，实际使用中要与ASP.NET CORE中间件或是IOC组件相结合，或是从token中获取赋值此对象
    string TenantId { get; set; }
    string UserId { get; set; }
	...
}

var services = new ServiceCollection();
services.AddSingleton(f =>
{
	//添加一个默认数据库和两个租户数据库
    var builder = new OrmDbFactoryBuilder()
	.Register<MySqlProvider>("fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", true)
    .Register<MySqlProvider>("fengling_tenant1", "Server=localhost;Database=fengling_tenant1;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", false)
    .Register<MySqlProvider>("fengling_tenant2", "Server=localhost;Database=fengling_tenant2;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", false)
    .UseSharding(s =>
    {
		//分库规则
        s.UseDatabase(() =>
        {
            //可以硬编码分库，也可以使用redis，映射表 ...，其他方式等
            var passport = f.GetService<IPassport>();
            return passport.TenantId switch
            {
                "200" => "fengling_tenant1",
                "300" => "fengling_tenant2",
                _ => "fengling"
            };
        })
		...
	}
});
var serviceProvider = services.BuildServiceProvider();
dbFactory = serviceProvider.GetService<IOrmDbFactory>();
using var repository = this.dbFactory.CreateRepository();
根据分库规则，当前租户ID是200,就会使用`fengling_tenant1`的`dbKey`
如果当前租户ID是300,就会使用`fengling_tenant2`的`dbKey`，否则就是默认`dbKey`: `fengling`
```


各种操作命令
------------------------------------------------------------
`Trolley`的所有插入、更新操作，都可以使用命名、匿名对象、字典类型(Dictionary<string, object>)等参数完成，
插入和更新接口支持得比较丰富，基本大多数场景都支持，对`enum`,`null`都做了支持，还支持特殊的本地化数据库操作，比如：mySql的Insert Ignore, OnDuplicateKeyUpdate等
所有插入、更新、删除操作都支持批量操作，插入和更新还支持BulkCopy操作



#### 新增
支持匿名对象、实体对象、字典参数插入
支持`enum`,`null`做了特殊支持
支持批量插入，支持匿名对象、实体对象、字典参数插入
实体对象，原值插入，字段值是`0`，插入数据库后也是`0`，字段值是`null`，插入数据库后也是`NULL`
如果不想插入类型默认值，可以使用匿名对象不插入字段，或是把字段设置为可为null类型，带`?`类型


扩展简化操作方法
```csharp
Create<User>(object insertObjs, int bulkCount = 500);
CreateAsync<TEntity>(object insertObjs, int bulkCount = 500, CancellationToken cancellationToken = default);
```

支持单插入，也支持多条批量插入
```csharp
using var repository = this.dbFactory.CreateRepository();

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

批量插入
采用的是多表值方式，就是`INSERT TABLE(...) VALUES(..),(...),(...)...`
这种方式相对于普通插入方式性能要高，但不适合大批量，适合小批量的
可设置单次入库的数据条数，可根据表插入字段个数和条数，设置一个性能最高的条数
通常会有一个插入性能最高的阈值，高于或低于这个阈值，批量插入的性能都会有所下降
这个阈值和数据库类型、插入字段的个数，两者都有关系。
```csharp
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

自增长列，无需为自增长列赋值，并返回增长的ID值。在字段映射的时候需要设置为自增长列`AutoIncrement()`。
```csharp
int CreateIdentity<TEntity>(object insertObj);
Task<int> CreateIdentityAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default);
long CreateIdentityLong<TEntity>(object insertObj);
Task<long> CreateIdentityLongAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default);
```
这几个方法直接返回的是自增列的ID

```csharp
builder.Entity<Company>(f =>
{
    f.ToTable("sys_company").Key(t => t.Id);
	f.Member(t => t.Id).Field(nameof(Company.Id)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).AutoIncrement().Position(1).Required();
	...
});
var result = repository.CreateIdentity<Company>(new 
{
    Name = "微软11",
    IsEnabled = true,
    CreatedAt = DateTime.Now,
    CreatedBy = 1,
    UpdatedAt = DateTime.Now,
    UpdatedBy = 1
});
//INSERT INTO `sys_company` (`Name`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Name,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy);SELECT @@IDENTITY
	
//使用字典参数, 自增长列
var result = repository.CreateIdentity<Company>(new Dictionary<string, object>()
{
    { "Name","微软11"},
    { "IsEnabled", true},
    { "CreatedAt", DateTime.Now},
    { "CreatedBy", 1},
    { "UpdatedAt", DateTime.Now},
    { "UpdatedBy", 1}
});
//INSERT INTO `sys_company` (`Name`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Name,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy);SELECT @@IDENTITY
```

使用`Create<User>()`方法，支持更复杂的场景
WithBy方法可以多次调用

带条件的插入字段数据
这样可避免字符串列，插入空字符串数据
或者插入的列数据赋值`null`或不赋值，也不会插入空字符串数据

```csharp
var sql = repository.Create<User>()
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
     })
    .WithBy(user.SomeTimes.HasValue, new { user.SomeTimes })
    .WithBy(guidField.HasValue, new { GuidField = guidField })
    .ToSql(out _);
repository.Commit();
//user.SomeTimes栏位没有值，guidField有值，得到如下SQL:
//INSERT INTO `sys_user` (`Id`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`,`GuidField`) VALUES(@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy,@GuidField)
	
//未赋值的字段，进入数据库后，将是NULL值
var count = repository.Create<Order>(new
{
    Id = 1,
    OrderNo = "ON-001",
    BuyerId = 1,
    SellerId = 2,
    TotalAmount = 500,
    IsEnabled = true,
    CreatedAt = DateTime.Now,
    CreatedBy = 1,
    UpdatedAt = DateTime.Now,
    UpdatedBy = 1
});
//属性Products、ProductCount、Disputes都没有赋值，进入到数据库后，这三个字段是NULL值
```

对`json`的支持
入库的时候，`Trolley`会调用`JsonTypeHandler`的`ToFieldValue`方法将参数值序列化为字符串
```csharp
var count = repository.Create<Order>()
    .WithBy(new Order
    {
		Id = 4,
		OrderNo = "ON-001",
		BuyerId = 1,
		SellerId = 2,
		TotalAmount = 500,
		Products = new List<int> { 1, 2 },
		Disputes = new Dispute
		{
			Id = 2,
			Content = "无良商家",
			Result = "同意退款",
			Users = "Buyer2,Seller2",
			CreatedAt = DateTime.Parse("2023-03-05")
		},
		IsEnabled = true,
		CreatedAt = DateTime.Now,
		CreatedBy = 1,
		UpdatedAt = DateTime.Now,
		UpdatedBy = 1
    })
    .Execute();
//INSERT INTO `sys_order` (`Id`,`OrderNo`,`ProductCount`,`TotalAmount`,`BuyerId`,`SellerId`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@OrderNo,@ProductCount,@TotalAmount,@BuyerId,@SellerId,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)

//两个JSON参数的值：
@Products: "[1,2]"
@Disputes: "{\"id\":2,\"users\":\"Buyer2,Seller2\",\"content\":\"无良商家\",\"result\":\"同意退款\",\"createdAt\":\"2023-03-05T00:00:00\"}"
```

使用`WithBulk`方法，支持批量操作
`Trolley`的所有批量新增采用的是多表值方式，就是`INSERT TABLE(...) VALUES(..),(...),(...)...`
这种方式相对于普通插入方式性能要高，但不适合大批量，适合小批量的
可设置单次入库的数据条数，可根据表插入字段个数和条数，设置一个性能最高的条数
通常会有一个插入性能最高的阈值，高于或低于这个阈值，批量插入的性能都会有所下降
这个阈值和数据库类型、插入字段的个数，两者都有关系。

效果同扩展简化操作方法是一样的。
```csharp
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

var sql = repository.Create<Product>()
    .WithBulk(new[]
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
    }, 50)
    .ToSql(out _);
//INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)

var count = repository.Create<Product>()
    .WithBulk(new[]
    {
		new Dictionary<string,object>
		{
			{ "Id", 1 },
			{ "ProductNo", "PN-001"},
			{ "Name", "波司登羽绒服"},
			{ "BrandId", 1},
			{ "CategoryId", 1},
			{ "IsEnabled", true},
			{ "CreatedAt", DateTime.Now},
			{ "CreatedBy", 1},
			{ "UpdatedAt", DateTime.Now},
			{ "UpdatedBy", 1}
		},
		new Dictionary<string,object>
		{
			{ "Id", 2},
			{ "ProductNo", "PN-002"},
			{ "Name", "雪中飞羽绒裤"},
			{ "BrandId", 2},
			{ "CategoryId", 2},
			{ "IsEnabled", true},
			{ "CreatedAt", DateTime.Now},
			{ "CreatedBy", 1},
			{ "UpdatedAt", DateTime.Now},
			{ "UpdatedBy", 1}
		},
		new Dictionary<string,object>
		{
			{ "Id", 3},
			{ "ProductNo", "PN-003"},
			{ "Name", "优衣库保暖内衣"},
			{ "BrandId", 3},
			{ "CategoryId", 3},
			{ "IsEnabled", true},
			{ "CreatedAt", DateTime.Now},
			{ "CreatedBy", 1},
			{ "UpdatedAt", DateTime.Now},
			{ "UpdatedBy", 1}
			}
		}
	})
    .Execute();
```

`BulkCopy`支持
大批量的插入，推荐使用WithBulkCopy方法，性能更优，底层实现就是BulkCopy
```csharp
var orders = new List<Order>();
for (int i = 0; i < 10000; i++)
{
    orders.Add(new Order
    {
        Id = $"ON_{i + 1}",
        TenantId = "3",
        OrderNo = $"ON-{i + 1}",
        BuyerId = 1,
        SellerId = 2,
        TotalAmount = 500,
        Products = new List<int> { 1, 2 },
        Disputes = new Dispute
        {
            Id = i + 1,
            Content = "无良商家",
            Result = "同意退款",
            Users = "Buyer2,Seller2",
            CreatedAt = DateTime.Now
        },
        IsEnabled = true,
        CreatedAt = DateTime.Now,
        CreatedBy = 1,
        UpdatedAt = DateTime.Now,
        UpdatedBy = 1
    });
}
var count = await repository.Create<Order>()
    .WithBulkCopy(orders)
    .ExecuteAsync();
```

部分字段插入`OnlyFields`
单条插入、批量插入，都可以使用

```csharp
OnlyFields(params string[] fieldNames);
OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
```

```csharp
var count = await repository.Create<User>()
    .WithBy(new
    {
        Id = 1,
        TenantId = "1",
        Name = "leafkevin",
        Age = 25,
        CompanyId = 1,
        Gender = Gender.Male,
        IsEnabled = true,
        CreatedAt = DateTime.Now,
        CreatedBy = 1,
        UpdatedAt = DateTime.Now,
        UpdatedBy = 1
    })
    .OnlyFields(f => new { f.Id, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
    .ExecuteAsync();
SQL: INSERT IGNORE INTO `sys_user` (`Id`,`Name`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@Name,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)

var count = repository.Create<Product>()
    .WithBulk(new[]
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
    })
    .OnlyFields(f => new { f.Id, f.ProductNo, f.Name, f.IsEnabled, f.CreatedBy, f.CreatedAt, f.UpdatedAt, f.UpdatedBy })
    .Execute();
SQL: INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)
```

忽略字段插入`IgnoreFields`
单条插入、批量插入，都可以使用

```csharp
IgnoreFields(params string[] fieldNames);
IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
```

```csharp
repository.Create<User>()
    .WithBy(new
    {
        Id = 1,
        TenantId = "1",
        Name = "leafkevin",
        Age = 25,
        CompanyId = 1,
        Gender = Gender.Male,
        SourceType = UserSourceType.Douyin,
        IsEnabled = true,
        CreatedAt = now,
        CreatedBy = 1,
        UpdatedAt = now,
        UpdatedBy = 1
    })
    .IgnoreFields("CompanyId", "SourceType")
    .Execute();
repository.Create<User>()
   .WithBy(new
   {
	   Id = 1,
	   TenantId = "1",
	   Name = "leafkevin",
	   Age = 25,
	   CompanyId = 1,
	   Gender = Gender.Male,
	   IsEnabled = true,
	   CreatedAt = now,
	   CreatedBy = 1,
	   UpdatedAt = now,
	   UpdatedBy = 1
   })
   .IgnoreFields(f => new { f.Gender, f.CompanyId })
   .Execute();
```

对枚举`enum`的支持
`Trolley`允许在代码中使用`enum`类型方便开发，在数据库端可以是数字类型或是字符串类型
只需要在映射的时候设置为对应的`DbType`或是对应的`Int`类型值即可
在实际项目中，通常会在项目中使用`enum`类型，数据库端使用字符串类型字段，这样可读性比较强

```csharp
builder.Entity<User>(f =>
{
    f.ToTable("sys_user").Key(t => t.Id);
    ... ...
    //设置数字类型，MySqlDbType.Byte,Int16,Int32,UByte,UInt16,UInt32,.. 等数字类型都可以，不要设置为浮点数
    f.Member(t => t.Gender).Field(nameof(User.Gender)).NativeDbType(MySqlDbType.Byte);
    ... ...
});
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
    })
    .ExecuteAsync();
SQL: INSERT INTO `sys_user` (`Id`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
@Gender参数会直接映射成对应的数据库类型

builder.Entity<Company>(f =>
{
    f.ToTable("sys_company").Key(t => t.Id);
    ... ...
    f.Member(t => t.Nature).Field(nameof(Company.Nature)).NativeDbType(MySqlDbType.VarChar);
    ... ...
});
var sql2 = repository.Create<Company>()
     .WithBy(new Company
     {
		 Id = 1,
		 Name = "leafkevin",
		 Nature = CompanyNature.Internet,
		 IsEnabled = true,
		 CreatedAt = DateTime.Now,
		 CreatedBy = 1,
		 UpdatedAt = DateTime.Now,
		 UpdatedBy = 1
     })
     .ToSql(out var parameters2);
Assert.True(sql2 == "INSERT INTO `sys_company` (`Id`,`Name`,`Nature`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Id,@Name,@Nature,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)");
Assert.True(parameters2[2].ParameterName == "@Nature");
Assert.True(parameters2[2].Value.GetType() == typeof(string));
Assert.True((string)parameters2[2].Value == CompanyNature.Internet.ToString());
```

 
`Insert Select`联表插入
`Trolley`支持联表1-6个表，最多6个，不同的数据库生成的SQL会有些差异

```csharp
var count = await repository.Create<Product>()
    .From<Brand>()
	.Where(f => f.Id == 1)
	.Select(f => new
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
    .ExecuteAsync();
//INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT a.`Id`+1,CONCAT('PN_',a.`BrandNo`),CONCAT('PName_',a.`Name`),a.`Id`,@CategoryId,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt` FROM `sys_brand` a WHERE a.`Id`=1
```
	
`Insert From`联多表插入
```csharp
//Insert From 多表
var count = await repository.Create<OrderDetail>()
    .From<Order, Product>()
	.Where((a, b) => a.Id == 3 && b.Id == 1)
	.Select((x, y) => new OrderDetail
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
    .ExecuteAsync();
//INSERT INTO `sys_order_detail` (`Id`,`OrderId`,`ProductId`,`Price`,`Quantity`,`Amount`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT @Id,a.`Id`,b.`Id`,b.`Price`,@Quantity,b.`Price`*3,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt` FROM `sys_order` a,`sys_product` b WHERE a.`Id`=3 AND b.`Id`=1
```

本地化数据库支持
`mysql`/`mariadb`数据库支持以下操作
`IgnoreInto`,`OnDuplicateKeyUpdate`
单条插入、批量插入，都可以使用

```csharp
var count = await repository.Create<User>()
    .IgnoreInto()
    .WithBy(new
    {
        Id = 1,
        TenantId = "1",
        Name = "leafkevin",
        Age = 25,
        CompanyId = 1,
        Gender = Gender.Male,
        IsEnabled = true,
        CreatedAt = DateTime.Now,
        CreatedBy = 1,
        UpdatedAt = DateTime.Now,
        UpdatedBy = 1
    })
    .ExecuteAsync();
SQL: INSERT IGNORE INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)	
```

`OnDuplicateKeyUpdate`支持三种方式
直接使用参数，使用`VALUES`带别名，一种不带别名，根据`mysql`/`mariadb`的版本决定使用哪个
尽力使用VALUES方式

```csharp
//直接使用参数方式
var count = await repository.Create<Order>()
     .WithBy(new
     {
         Id = "9",
         TenantId = "3",
         OrderNo = "ON-001",
         BuyerId = 1,
         SellerId = 2,
         TotalAmount = 500,
         Products = new List<int> { 1, 2 },
         Disputes = new Dispute
         {
			Id = 2,
			Content = "无良商家",
			Result = "同意退款",
			Users = "Buyer2,Seller2",
			CreatedAt = DateTime.Now
         },
         IsEnabled = true,
         CreatedAt = DateTime.Now,
         CreatedBy = 1,
         UpdatedAt = DateTime.Now,
         UpdatedBy = 1
     })
     .OnDuplicateKeyUpdate(x => x
        .Set(new
        {
            TotalAmount = 25,
            Products = new List<int> { 1, 2 }
        })
        .Set(buyerSource.HasValue, f => f.BuyerSource, buyerSource)
     )
    .ExecuteAsync();
SQL: INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`TotalAmount`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@OrderNo,@BuyerId,@SellerId,@TotalAmount,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON DUPLICATE KEY UPDATE `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerSource`=@BuyerSource

//使用`VALUES`,不带别名
count = await repository.Create<Order>()
    .WithBy(new
    {
        Id = "9",
        TenantId = "3",
        OrderNo = "ON-001",
        BuyerId = 1,
        SellerId = 2,
        BuyerSource = buyerSource,
        TotalAmount = 600,
        Products = new List<int> { 1, 2 },
        Disputes = new Dispute
        {
            Id = 2,
            Content = "无良商家",
            Result = "同意退款",
            Users = "Buyer2,Seller2",
            CreatedAt = DateTime.Now
        },
        IsEnabled = true,
        CreatedAt = DateTime.Now,
        CreatedBy = 1,
        UpdatedAt = DateTime.Now,
        UpdatedBy = 1
    })
    .OnDuplicateKeyUpdate(x => x
        .Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
        .Set(true, f => f.Products, f => x.Values(f.Products)))
    .ExecuteAsync();
SQL: INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`TotalAmount`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@OrderNo,@BuyerId,@SellerId,@BuyerSource,@TotalAmount,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) ON DUPLICATE KEY UPDATE `TotalAmount`=VALUES(`TotalAmount`),`Products`=VALUES(`Products`)	

//使用`VALUES`,带别名
count = await repository.Create<Order>()
    .WithBy(new
    {
        Id = "9",
        TenantId = "3",
        OrderNo = "ON-001",
        BuyerId = 1,
        SellerId = 2,
        BuyerSource = buyerSource,
        TotalAmount = 600,
        Products = new List<int> { 1, 2 },
        Disputes = new Dispute
        {
            Id = 2,
            Content = "无良商家",
            Result = "同意退款",
            Users = "Buyer2,Seller2",
            CreatedAt = DateTime.Now
        },
        IsEnabled = true,
        CreatedAt = DateTime.Now,
        CreatedBy = 1,
        UpdatedAt = DateTime.Now,
        UpdatedBy = 1
    })
    .OnDuplicateKeyUpdate(x => x.UseAlias()
        .Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
        .Set(true, f => f.Products, f => x.Values(f.Products)))
    .ExecuteAsync();
INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`TotalAmount`,`Products`,`Disputes`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@OrderNo,@BuyerId,@SellerId,@BuyerSource,@TotalAmount,@Products,@Disputes,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=newRow.`TotalAmount`,`Products`=newRow.`Products`
```

`mariadb`数据库还支持`Returning`操作，也依赖`mariadb`数据库的版本，有的版本支持，有的版本不支持
同样支持单挑、批量插入

```csharp
var count = await repository.Create<User>()
    .WithBy(new
    {
        Id = 1,
        TenantId = "1",
        Name = "leafkevin",
        Age = 25,
        CompanyId = 1,
        Gender = Gender.Male,
        IsEnabled = true,
        CreatedAt = DateTime.Now,
        CreatedBy = 1,
        UpdatedAt = DateTime.Now,
        UpdatedBy = 1
    })
    .Returning(f => new { f.Id, f.TenantId })
    .ExecuteAsync();
SQL: INSERT INTO `sys_user` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy) RETURNING Id,TenantId

var count = await repository.Create<Product>()
    .WithBulk(products)
    .Returning<Product>("*")
    .ExecuteAsync();
SQL: INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2) RETURNING *
```

`SqlServer`数据库支持`Output`操作,效果同`mariadb`数据库的`Returning`，也依赖`SqlServer`数据库的版本，有的版本支持，有的版本不支持
同样支持单挑、批量插入

```csharp
var result = await repository.Create<User>()
    .WithBy(new
    {
        Id = 1,
        TenantId = "1",
        Name = "leafkevin",
        Age = 25,
        CompanyId = 1,
        Gender = Gender.Male,
        IsEnabled = true,
        CreatedAt = DateTime.Now,
        CreatedBy = 1,
        UpdatedAt = DateTime.Now,
        UpdatedBy = 1
    })
    .Output(f => new { f.Id, f.TenantId })
    .ExecuteAsync();
SQL: INSERT INTO [sys_user] ([Id],[TenantId],[Name],[Age],[CompanyId],[Gender],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.Id,INSERTED.TenantId VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)

var products = new[]
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
};
var results = await repository.Create<Product>()
    .WithBulk(products)
    .Output(f => new { f.Id, f.ProductNo })
    .ExecuteAsync();
SQL: INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[BrandId],[CategoryId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.Id,INSERTED.ProductNo VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)

var results = await repository.Create<Product>()
    .WithBulk(products)
    .Output<Product>("*")
    .ExecuteAsync();
SQL: INSERT INTO [sys_product] ([Id],[ProductNo],[Name],[BrandId],[CategoryId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) OUTPUT INSERTED.* VALUES (@Id0,@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)
```

其他数据库本地化支持，有待后续丰富



#### 更新
支持匿名对象、字典参数插入
支持`enum`,`null`做了特殊支持
支持批量更新，支持匿名对象、字典参数插入，是`By`主键更新


简化操作
支持单条、批量操作
```csharp
var result = repository.Update<User>(new { Id = 1, Name = "leafkevin11" });
//UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=@kId
var result = repository.Update<User>(f => f.Name, new { Id = 1, Name = "leafkevin11" });
//UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=@kId
var result = repository.Update<User>(f => new { Name = f.Name + "_1", Gender = Gender.Female }, t => t.Id == 1);
//UPDATE `sys_user` SET `Name`=CONCAT(`Name`,'_1'),`Gender`=@Gender WHERE `Id`=1

```

部分字段更新
直接使用匿名对象参数
带有明确更新字段的表达式+对象参数，对象参数可以是匿名也可以是实体对象
```csharp
var result = repository.Update<User>(new { Id = 1, Name = "leafkevin11" });
//UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=@kId
var result = repository.Update<User>(f => f.Name, new { Id = 1, Name = "leafkevin11" });
//UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=@kId
var result = repository.Update<User>(f => new { Name = f.Name + "_1", Gender = Gender.Female }, t => t.Id == 1);
//UPDATE `sys_user` SET `Name`=CONCAT(`Name`,'_1'),`Gender`=@Gender WHERE `Id`=1
```

对`null`的支持
直接使用匿名对象，对应字段设置为DBNull.Value
或是使用`From Set`直接设置`null`

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


#### 删除

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
using var repository = this.dbFactory.CreateRepository();
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

分库分表支持
`Trolley`对分库分表的支持，非常灵活。依赖分库分表规则，可以租户、时间、租户+时间、任何自定义规则
在配置数据库时，调用UseSharding方法，实现分库分表规则的配置

分库规则，调用UseDatabase方法，实现分库规则配置
下面的示例，就是使用租户来做分库，根据当前登录用户的租户ID，生成dbKey

```csharp
var services = new ServiceCollection();
services.AddSingleton(f =>
{
	//添加一个默认数据库和两个租户数据库
    var builder = new OrmDbFactoryBuilder()
    .Register<MySqlProvider>("fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", true)
    .Register<MySqlProvider>("fengling_tenant1", "Server=localhost;Database=fengling_tenant1;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", false)
    .Register<MySqlProvider>("fengling_tenant2", "Server=localhost;Database=fengling_tenant2;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", false)
    .UseSharding(s =>
    {
		//分库规则
        s.UseDatabase(() =>
        {
            //可以硬编码分库，也可以使用redis，映射表 ...，其他方式等
            var passport = f.GetService<IPassport>();
            return passport.TenantId switch
            {
                "200" => "fengling_tenant1",
                "300" => "fengling_tenant2",
                _ => "fengling"
            };
        })
        //按照租户+时间分表
        .UseTable<Order>(t =>
        {
            t.DependOn(d => d.TenantId).DependOn(d => d.CreatedAt)
            .UseRule((dbKey, origName, tenantId, createdAt) => $"{origName}_{tenantId}_{createdAt:yyyyMM}", "^sys_order_\\d{1,4}_[1,2]\\d{3}[0,1][0-9]$")
            //时间分表，通常都是支持范围查询
            .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =>
            {
                var tableNames = new List<string>();
                var current = beginTime;
                while (current <= endTime)
                {
                    var tableName = $"{origName}_{tenantId}_{current:yyyyMM}";
                    if (tableNames.Contains(tableName))
                        continue;
                    tableNames.Add(tableName);
                }
                return tableNames;
            });
        })
        //按照租户+时间分表
        .UseTable<OrderDetail>(t =>
        {
            t.DependOn(d => d.TenantId).DependOn(d => d.CreatedAt)
            .UseRule((dbKey, origName, tenantId, createdAt) => $"{origName}_{tenantId}_{createdAt:yyyyMM}", "^sys_order_detail_\\d{1,4}_[1,2]\\d{3}[0,1][0-9]$")
            //时间分表，通常都是支持范围查询
            .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =>
            {
                var tableNames = new List<string>();
                var current = beginTime;
                while (current <= endTime)
                {
                    var tableName = $"{origName}_{tenantId}_{current:yyyyMM}";
                    if (tableNames.Contains(tableName))
                        continue;
                    tableNames.Add(tableName);
                }
                return tableNames;
            });
        })
        //按租户分表
        //.UseTable<Order>(t => t.DependOn(d => d.TenantId).UseRule((dbKey, origName, tenantId) => $"{origName}_{tenantId}", "^sys_order_\\d{1,4}$"))
        ////按照Id字段分表，Id字段是带有时间属性的ObjectId
        //.UseTable<Order>(t => t.DependOn(d => d.Id).UseRule((dbKey, origName, id) => $"{origName}_{new DateTime(ObjectId.Parse(id).Timestamp):yyyyMM}", "^sys_order_\\S{24}$"))
        ////按照Id字段哈希取模分表
        //.UseTable<Order>(t => t.DependOn(d => d.Id).UseRule((dbKey, origName, id) => $"{origName}_{HashCode.Combine(id) % 5}", "^sys_order_\\S{24}$"))
        .UseTable<User>(t => t.DependOn(d => d.TenantId).UseRule((dbKey, origName, tenantId) => $"{origName}_{tenantId}", "^sys_user_\\d{1,4}$"));
    })
    .Configure<MySqlProvider, ModelConfiguration>();
    return builder.Build();
});
services.AddTransient<IPassport>(f => new Passport { TenantId = "104", UserId = "1" });
var serviceProvider = services.BuildServiceProvider();
this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
```

分表规则,调用UseTable方法，实现分表规则配置
如上示例，


```csharp
var services = new ServiceCollection();
services.AddSingleton(f =>
{
	//添加一个默认数据库和两个租户数据库
    var builder = new OrmDbFactoryBuilder()
    .Register<MySqlProvider>("fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", true)
    .Register<MySqlProvider>("fengling_tenant1", "Server=localhost;Database=fengling_tenant1;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", false)
    .Register<MySqlProvider>("fengling_tenant2", "Server=localhost;Database=fengling_tenant2;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", false)
    .UseSharding(s =>
    {
		//分库规则
        s.UseDatabase(() =>
        {
            //可以硬编码分库，也可以使用redis，映射表 ...，其他方式等
            var passport = f.GetService<IPassport>();
            return passport.TenantId switch
            {
                "200" => "fengling_tenant1",
                "300" => "fengling_tenant2",
                _ => "fengling"
            };
        })
        //按照租户+时间分表
        .UseTable<Order>(t =>
        {
            t.DependOn(d => d.TenantId).DependOn(d => d.CreatedAt)
            .UseRule((dbKey, origName, tenantId, createdAt) => $"{origName}_{tenantId}_{createdAt:yyyyMM}", "^sys_order_\\d{1,4}_[1,2]\\d{3}[0,1][0-9]$")
            //时间分表，通常都是支持范围查询
            .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =>
            {
                var tableNames = new List<string>();
                var current = beginTime;
                while (current <= endTime)
                {
                    var tableName = $"{origName}_{tenantId}_{current:yyyyMM}";
                    if (tableNames.Contains(tableName))
                        continue;
                    tableNames.Add(tableName);
                }
                return tableNames;
            });
        })
        //按照租户+时间分表
        .UseTable<OrderDetail>(t =>
        {
            t.DependOn(d => d.TenantId).DependOn(d => d.CreatedAt)
            .UseRule((dbKey, origName, tenantId, createdAt) => $"{origName}_{tenantId}_{createdAt:yyyyMM}", "^sys_order_detail_\\d{1,4}_[1,2]\\d{3}[0,1][0-9]$")
            //时间分表，通常都是支持范围查询
            .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =>
            {
                var tableNames = new List<string>();
                var current = beginTime;
                while (current <= endTime)
                {
                    var tableName = $"{origName}_{tenantId}_{current:yyyyMM}";
                    if (tableNames.Contains(tableName))
                        continue;
                    tableNames.Add(tableName);
                }
                return tableNames;
            });
        })
        //按租户分表
        //.UseTable<Order>(t => t.DependOn(d => d.TenantId).UseRule((dbKey, origName, tenantId) => $"{origName}_{tenantId}", "^sys_order_\\d{1,4}$"))
        ////按照Id字段分表，Id字段是带有时间属性的ObjectId
        //.UseTable<Order>(t => t.DependOn(d => d.Id).UseRule((dbKey, origName, id) => $"{origName}_{new DateTime(ObjectId.Parse(id).Timestamp):yyyyMM}", "^sys_order_\\S{24}$"))
        ////按照Id字段哈希取模分表
        //.UseTable<Order>(t => t.DependOn(d => d.Id).UseRule((dbKey, origName, id) => $"{origName}_{HashCode.Combine(id) % 5}", "^sys_order_\\S{24}$"))
        .UseTable<User>(t => t.DependOn(d => d.TenantId).UseRule((dbKey, origName, tenantId) => $"{origName}_{tenantId}", "^sys_user_\\d{1,4}$"));
    })
    .Configure<MySqlProvider, ModelConfiguration>();
    return builder.Build();
});
services.AddTransient<IPassport>(f => new Passport { TenantId = "104", UserId = "1" });
var serviceProvider = services.BuildServiceProvider();
this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
```


欢迎大家使用
---------------------
欢迎大家广提Issue，我的联系方式：
QQ：39253425
Mail：leafkevin@126.com
