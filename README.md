Trolley - 一个轻量级高性能的.NET ORM框架
========================================


框架特点
------------------------------------------------------------
强类型的DDD仓储操作，基本可以不用写SQL，支持多种数据库终端，目前是在.NET 6 基础上开发的。
目前支持：`MySql`,`PostgreSql`,`SqlSever`,其他的`OrmProvider`会稍后慢慢提供。

支持`Page`分页查询  
支持`Join`，`GroupBy`，`OrderBy`等操作  
支持`Count`, `Max`, `Min`， `Avg`，`Sum`等聚合操作  
支持`In`，`Exists`操作  
支持`Insert Select From`，`Update Join`  
支持批量`Insert`，`Update`，`Delete`  
支持`BulkCopy`插入，`BulkCopy`更新  
支持`Include`导航属性，值对象导航属性(瘦身版模型)  
支持`CTE`公共表达式，多语句查询`MultipleQuery`，多命令查询`MultipleExecute`  
支持模型映射，自动映射，采用流畅API方式，目前不支持特性方式映射  
支持多租户，指定TableSchema，分库，分表，读写分离

## 引入`Trolley`对应数据库驱动的Nuget包，在系统中要注册`IOrmDbFactory`并注册映射
系统中每个连接串对应一个`OrmProvider`，每种类型的`OrmProvider`以单例形式存在，一个应用中可以存在多种类型的`OrmProvider`。
引入`Trolley`对应数据库驱动的Nuget包，如：`Trolley.MySqlConnector`，`Trolley.SqlServer`...等   
在`Trolley`中，一个`dbKey`代表一个数据库，可以是不同的`OrmProvider`。  
必须指定`dbKey`才能创建`IOrmDbFactory`对象，没有指定就是使用默认的`dbKey`,默认数据库。  

示例:
```csharp
var dbFactory = new OrmDbFactoryBuilder()
    .Register(OrmProviderType.MySql, "fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4, true)
    .Configure<MySqlModelConfiguration>(OrmProviderType.MySql);
return builder.Build();
```

#### 多个不同数据库的场景
```csharp
var connectionString1 = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
var connectionString2 = "User id=postgres;Password=123456;Host=localhost;Port=5432;Database=fengling;Pooling=true;Min Pool Size=5;Max Pool Size=100;";
var builder = new OrmDbFactoryBuilder()
    .Register(OrmProviderType.MySql, "fengling", connectionString1，true)
    .Register(OrmProviderType.PostgreSql, "fengling_tanent001", connectionString2, false)
    .Configure<MySqlModelConfiguration>(OrmProviderType.MySql)
    .Configure<NpgSqlModelConfiguration>(OrmProviderType.PostgreSql);
var dbFactory = builder.Build();
```

在注册`IOrmDbFactory`的时候，同时也要把数据库结构的模型映射配置出来。
模型映射采用的是Fluent Api方式，类似EF，通常是继承`IModelConfiguration`的子类。
Trolley, 目前只支持Fluent Api方式，这样能使模型更加纯净，不受ORM污染。

实体映射，可以按照数据库类型进行设置，也可以按照dbKey进行设置，`dbKey`设置的实体映射只针对这个dbKey数据库生效，按照数据库类型配置的，这个数据库类型的所有数据库都生效，类似于全局配置了。  
优先级顺序是`dbKey`配置的实体映射优先于数据库库类型配置的实体映射。
`.Configure<MySqlModelConfiguration>(OrmProviderType.MySql)`
`.Configure<MySqlModelConfiguration>("myDbKey")`


导航属性的设置，是单向的，只需要把本模型内的导航属性列出来就可以了。
对应的导航属性类，再设置它所引用的模型映射。
这里的`ModelConfiguration`类，就是模型映射类，内容如下：
```csharp
class ModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<User>(f =>
            {
		//导航属性的设置，是单向的，只需要把本模型内的导航属性列出来就可以了。  
		//对应的导航属性类，在应设置再设置它所引用的类型映射。  
		f.ToTable("sys_user");
		f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();//导航属性，这里是值对象，不是真正的模型，是模型Company的瘦身版，使用MapTo指定对应的模型Company
		f.HasMany(t => t.Orders).HasForeignKey(t => t.BuyerId);
            })
            .Entity<Company>(f =>
            {
		f.ToTable("sys_company");
		//导航属性的设置，是单向的，只需要把本模型内的导航属性列出来就可以了。  
		//对应的导航属性类，在应设置再设置它所引用的类型映射。 
		f.HasMany(t => t.Users).HasForeignKey(t => t.CompanyId);//导航属性，这里是真正的模型
		f.HasMany(t => t.Brands).HasForeignKey(t => t.CompanyId);
		f.HasMany(t => t.Products).HasForeignKey(t => t.CompanyId);
            })
            .Entity<Order>(f =>
            {
		//表有Products、Disputes两个栏位，数据库类型是JSONB类型，自动使用JsonTypeHandler类型处理器，映射到实体对用实体类型
		f.ToTable("sys_order");  
		f.HasOne(t => t.Buyer).HasForeignKey(t => t.BuyerId);
		f.HasOne(t => t.Seller).HasForeignKey(t => t.SellerId).MapTo<User>();//导航属性，这里是值对象，不是真正的模型，是模型User的瘦身版，使用MapTo指定对应的模型User
		f.HasMany(t => t.Details).HasForeignKey(t => t.OrderId);
            })
            .Entity<OrderDetail>(f =>
            {
		f.ToTable("sys_order_detail");
		f.HasOne(t => t.Order).HasForeignKey(t => t.OrderId);
		f.HasOne(t => t.Product).HasForeignKey(t => t.ProductId);
            })	
            .Entity<OrderDetail>(f =>
            {
		f.ToTable("sys_order_detail");
		f.HasOne(t => t.Order).HasForeignKey(t => t.OrderId);
		f.HasOne(t => t.Product).HasForeignKey(t => t.ProductId);
            })
            //参数值表sys_lookup_value，有个字段名lookup_value，与实体中的Value属性映射，不满足字段映射处理器规则，需要手动指定
            .Entity<LookupValue>(f => f.ToTable("sys_lookup_value").Member(f => f.Value).Field("lookup_value"))			
            .UseAutoMap();  
    }
}
```
上面的映射配置，采用了自动映射，`Trolley`会根据数据库表的结构，与实体中公共字段或属性进行匹配映射，匹配的逻辑是通过`IFieldMapHandler`字段映射处理器对应的4个方法来实现的。  
实体中其他的成员，如：导航属性，将不会被映射到，因为与数据库表的字段无法关联上，所以，需要手动指定，如：上面的实体中，导航属性都是手动指定。  

默认的字段映射处理器是`DefaultFieldMapHandler`类，匹配的逻辑是：
1. 根据数据字段名与实体中的公共成员名(字段或属性)区分大小写，匹配成功则返回，否则继续下面匹配
2. 根据数据字段名与实体中的公共成员名(字段或属性)不区分大小写，匹配成功则返回，否则继续下面匹配
3. 去掉数据字段名中`_`下划线，再根据字段名与实体中的公共成员名(字段或属性)区分大小写，匹配成功则返回，否则继续下面匹配
4. 去掉数据字段名中`_`下划线，再根据字段名与实体中的公共成员名(字段或属性)不区分大小写，匹配成功则返回

上面4个原则都不满足，则跳过此数据字段的映射。

在实际应用场景中，自动映射，会有字段与实体中的成员不满足上面匹配原则的，可以手动指定，如：上面的`sys_lookup_value`表中的字段`lookup_value`与实体`LookupValue`中的`Value`属性映射。


如果上面的映射逻辑不满足需求，可以自己实现`IFieldMapHandler`字段映射处理器。 
```csharp
bool TryFindMember(string fieldName, List<MemberMap> memberMappers, out MemberMap memberMapper);
bool TryFindMember(string fieldName, List<MemberInfo> memberInfos, out MemberInfo memberInfo);
bool TryFindField(string memberName, List<MemberMap> memberMappers, out MemberMap memberMapper);
bool TryFindField(string memberName, List<string> fieldNames, out string fieldName);
```
在配置的时候，应用这个字段映射处理器，如： `.UseFieldMapHandler<MyFieldMapHandler>()`


也可以不自动映射，使用完整映射，把所有的字段手动映射一遍，书写的工作量会大一点，也可以使用`Trolley.T4`中的各个驱动下的`Entities.tt`，`Entity.tt`，`ModelConfiguration.tt`，`ModelConfigurations.tt`模板，来生成。
路径在：`Trolley.T4\SqlServer\ModelConfiguration.tt`, `Trolley.T4\SqlServer\ModelConfigurations.tt`, `Trolley.T4\MySql\ModelConfiguration.tt`, `Trolley.T4\MySql\ModelConfigurations.tt`  

`Trolley`底层使用的`DbType`是各个数据库驱动的本地DbType，如：`MySqlProvider`使用的DbType是`MySqlConnector.MySqlDbType`。  
`Trolley`在配置各个数据库模型映射时，可以使用`int`类型，也可以使用本地`DbType`类型，如：`SqlDbType`，或是`MySqlDbType`...类型等  
完整映射的代码，类似于下面：  
```csharp
class ModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
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
            //特殊类型JSON
            f.Member(t => t.Products).Field(nameof(Order.Products)).DbColumnType("longtext").NativeDbType(MySqlDbType.LongText).Position(9).TypeHandler<JsonTypeHandler>();
            f.Member(t => t.Disputes).Field(nameof(Order.Disputes)).DbColumnType("longtext").NativeDbType(MySqlDbType.LongText).Position(10).TypeHandler<JsonTypeHandler>();
            f.Member(t => t.IsEnabled).Field(nameof(Order.IsEnabled)).DbColumnType("tinyint(1)").NativeDbType(MySqlDbType.Bool).Position(11);
            f.Member(t => t.CreatedAt).Field(nameof(Order.CreatedAt)).DbColumnType("datetime").NativeDbType(MySqlDbType.DateTime).Position(12);
            f.Member(t => t.CreatedBy).Field(nameof(Order.CreatedBy)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(13);
            f.Member(t => t.UpdatedAt).Field(nameof(Order.UpdatedAt)).DbColumnType("datetime").NativeDbType(MySqlDbType.DateTime).Position(14);
            f.Member(t => t.UpdatedBy).Field(nameof(Order.UpdatedBy)).DbColumnType("int").NativeDbType(MySqlDbType.Int32).Position(15);
       
            f.HasOne(t => t.Buyer).HasForeignKey(t => t.BuyerId);
            f.HasOne(t => t.Seller).HasForeignKey(t => t.SellerId).MapTo<User>();
            f.HasMany(t => t.Details).HasForeignKey(t => t.OrderId);
        });
    }
}
```
自动映射与上面手写映射生成的配置数据是相同的。  


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



创建IRepository对象，就可以做各种操作了  
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
var repository = this.dbFactory.CreateRepository();
```
如果有指定`dbKey`就是使用指定的`dbKey`创建`IRepository`对象
如果没有指定`dbKey`，再判断是否有指定分库规则，有指定就调用分库规则获取`dbKey`
如果也没有指定分库规则，就使用配置的默认`dbKey`

#### 基本简单查询

```csharp
var repository = this.dbFactory.CreateRepository();
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


### `From`&lt;T...&gt;()查询，支持各种复杂场景，支持分表操作

#### 简单表达式查询

```csharp
var repository = this.dbFactory.CreateRepository();
var result = await repository.From<Product>()
    .Where(f => f.ProductNo.Contains("PN-00"))
    .ToListAsync();
//SELECT `Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_product` WHERE `ProductNo` LIKE '%PN-00%'
```

#### `Page` 分页查询

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

#### `Include`查询
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

#### `IncludeMany`查询
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

#### `IncludeMany`查询 + `Filter`过滤条件

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

#### `Include`后`ThenInclude`查询，都是1:1关系联表
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

#### 使用`Skip`、`Take`也可以实现分页
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

#### 分组查询Group
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

Group 打开Grouping，使用里面的字段
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

#### `GroupBy`后，可使用`Having`、`OrderBy`
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

#### 使用`In`、`Exists` 
`In`、`Exists`操作是通过静态`Sql`类来完成的，书写起来比较简单，`In`、`Exists`子句不支持分表。
`Exists`也可以使用`From`后，再调用`Exists`方法来完成，这种方式支持分表。

`Sql.In` 有以下方法重载：
```
csharp
bool In<TElement>(TElement value, params TElement[] list);
bool In<TElement>(TElement value, IEnumerable<TElement> list);
bool In<TElement>(TElement value, IQuery<TElement> subQuery);
bool In<TElement>(TElement value, Func<IFromQuery, IQuery<TElement>> subQuery);
```

`Sql.Exists` 有以下方法重载：
```csharp
bool Exists<TTarget>(Func<IFromQuery, IQuery<TTarget>> subQuery)
bool Exists<T>(ICteQuery<T> subQuery, Expression<Func<T, bool>> predicate)//支持CTE子查询
bool Exists(IQuery<T> subQuery, Expression<Func<T, bool>> predicate) 
bool Exists<T>(Expression<Func<T, bool>> filter);
bool Exists<T1, T2>(Expression<Func<T1, T2, bool>> filter);
bool Exists<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> filter)
bool Exists<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> filter);
bool Exists<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> filter);
bool Exists<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> filter);
```

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
//SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE a.`Id` IN (1,2,3) AND EXISTS(SELECT * FROM `sys_order_detail` f WHERE b.`Id`=f.`OrderId` AND f.`ProductId`=2) GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) HAVING SUM(b.`TotalAmount`)>300 ORDER BY a.`Id`,CONVERT(b.`CreatedAt`,DATE)
 ```

子查询
```csharp
var sql = repository.From<User>()
    .Where(f => Sql.Exists(t => t
        .From<OrderDetail>('b')
        .GroupBy(a => a.OrderId)
        .Having((x, a) => Sql.CountDistinct(a.ProductId) > 0)
        .Select()))
    .GroupBy(f => new { f.Gender, f.CompanyId })
    .Select((t, a) => new { t.Grouping, UserTotal = t.CountDistinct(a.Id) })
    .ToSql(out _);
//SELECT a.`Gender`,a.`CompanyId`,COUNT(DISTINCT a.`Id`) AS `UserTotal` FROM `sys_user` a WHERE EXISTS(SELECT b.`OrderId` FROM `sys_order_detail` b GROUP BY b.`OrderId` HAVING COUNT(DISTINCT b.`ProductId`)>0) GROUP BY a.`Gender`,a.`CompanyId`
 ```

子查询和CTE子查询
```csharp
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

#### 聚合查询
可以使用`SelectAggregate`实现聚合查询，也可以使用`Select`+`Sql`静态类，两者生成的SQL完全一样的
```csharp
//SelectAggregate方法
在没有GroupBy分组的情况，使用聚合查询，不同的数据库支持的场景都不一样。
下面的语句在MySql中可以执行，在SqlServer中不能执行
var sql = repository.From<Order>()
    .SelectAggregate((x, a) => new
    {
	OrderCount = x.Count(a.Id),
	TotalAmount = x.Sum(a.TotalAmount)
    })
    .ToSql(out _);
//SELECT COUNT(a.`Id`) AS `OrderCount`,SUM(a.`TotalAmount`) AS `TotalAmount` FROM `sys_order` a

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

#### 子查询 使用`From`和`WithTable`方法
`From`，主要在最开始时使用，`WithTable`，在使用`From`之后的任意地方，两者生成的SQL完全一样的
> 注意：`From`之后的子查询，完全可以由`Join`完成，`Join`本身就支持子查询
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
```

#### 子查询 可以多个表直接查询，最多支持10个表
`From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')`
```csharp
var result = await repository
    .From(f => f.From<Page, Menu>('o')
        .Where((a, b) => a.Id == b.PageId)
        .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
    .InnerJoin<Menu>((a, b) => a.Id == b.Id)
    .Where((a, b) => a.Id == b.Id)
    .Select((a, b) => new { a.Id, b.Name, a.ParentId, a.Url })
    .ToListAsync();
//SELECT a.`Id`,b.`Name`,a.`ParentId`,a.`Url` FROM (SELECT p.`Id`,p.`ParentId`,o.`Url` FROM `sys_page` o,`sys_menu` p WHERE o.`Id`=p.`PageId`) a INNER JOIN `sys_menu` b ON a.`Id`=b.`Id` WHERE a.`Id`=b.`Id`");
```

`WidthTable`子查询，同样也支持多个表直接关联
```csharp
var result = repository.From<Menu>()
    .WithTable(f => f.From<Page, Menu>('c')
        .Where((a, b) => a.Id == b.PageId)
        .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
    .Where((a, b) => a.Id == b.Id)
    .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
    .First();
//SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `sys_menu` a,(SELECT d.`Id`,d.`ParentId`,c.`Url` FROM `sys_page` c,`sys_menu` d WHERE c.`Id`=d.`PageId`) b WHERE a.`Id`=b.`Id`

var result = repository
    .From<Order, User>()
    .WithTable(f => f.From<Order, OrderDetail, User>()
        .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id && c.Age > 20)
        .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
        .Having((x, a, b, c) => x.Sum(b.Amount) > 500)
        .Select((x, a, b, c) => new { x.Grouping.OrderId, TotalAmount = x.Sum(b.Amount) }))
    .Where((a, b, c) => a.BuyerId == b.Id && a.Id == c.OrderId)
    .Select((a, b, c) => new { Order = a, Buyer = b, OrderId = a.Id, a.BuyerId, c.TotalAmount })
    .First();
//SELECT a.`Id`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`Id` AS `OrderId`,a.`BuyerId`,c.`TotalAmount` FROM `sys_order` a,`sys_user` b,(SELECT a.`Id` AS `OrderId`,SUM(b.`Amount`) AS `TotalAmount` FROM `sys_order` a,`sys_order_detail` b,`sys_user` c WHERE a.`Id`=b.`OrderId` AND a.`BuyerId`=c.`Id` AND c.`Age`>20 GROUP BY a.`Id`,a.`BuyerId` HAVING SUM(b.`Amount`)>500) c WHERE a.`BuyerId`=b.`Id` AND a.`Id`=c.`OrderId`
```
> 注意：
> WithTable就相当于添加一张子查询表，后续可以Join关联，也可以在where中直接关联，类似于：SELECT * FROM Table1 a,(....) b WHERE ...
> 如果使用Join关联的话，Join操作本身就可以直接关联子查询，无需使用WithTable了。



#### `Join`表关联
`Trolley`支持三种`Join`表连接，`InnerJoin`、`LeftJoin`、`RightJoin`
有两种方式`Join`关联表：  
    1. 一张表一张表的`Join`关联起来  
    2. 一次`From`多张表，再挨个关联 
直接关联实体表，也可以关联子查询表，相当于先`WithTable`后再`Join`，每次关联只能关联两张表，但可以多次关联

#### `Join`关联实体表

```csharp
//INNER JOIN
//一张表一张表关联
var result = repository.From<User>()
    .InnerJoin<Order>((x, y) => x.Id == y.BuyerId)
    .Where((a, b) => b.ProductCount > 1)
    .Select((x, y) => new
    {
        User = x,
        Order = y
    })
    .ToList();
//SELECT a.`Id`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE b.`ProductCount`>1

//直接From多张表，再挨个关联
var result = repository.From<User, Order, OrderDetail>()
    .InnerJoin((a, b, c) => a.Id == b.BuyerId)
    .LeftJoin((a, b, c) => b.Id == c.OrderId)
    .Select((a, b, c) => new { OrderId = b.Id, b.OrderNo, b.Disputes, b.BuyerId, Buyer = a, TotalAmount = Sql.Sum(c.Amount) })
    .ToList();
//SELECT b.`Id` AS `OrderId`,b.`OrderNo`,b.`Disputes`,b.`BuyerId`,a.`Id`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,SUM(c.`Amount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` LEFT JOIN `sys_order_detail` c ON b.`Id`=c.`OrderId`
```

#### `Join`关联子查询
```csharp
var result = repository
    .From(f => f.From<Order, OrderDetail>('a')
        .Where((a, b) => a.Id == b.OrderId)
        .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
        .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
        .Select((x, a, b) => new { x.Grouping, ProductTotal = Sql.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
    .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
    .Select((x, y) => new { x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
    .ToList();
//SELECT a.`BuyerId`,a.`OrderId`,a.`BuyerId`,a.`ProductTotal`,b.`Name` AS `BuyerName`,a.`BuyerId1` AS `BuyerId2` FROM (SELECT a.`BuyerId`,a.`Id` AS `OrderId`,COUNT(DISTINCT b.`ProductId`) AS `ProductTotal`,a.`BuyerId` AS `BuyerId1` FROM `sys_order` a,`sys_order_detail` b WHERE a.`Id`=b.`OrderId` GROUP BY a.`BuyerId`,a.`Id` HAVING COUNT(DISTINCT b.`ProductId`)>0) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id`

//LeftJoin查询，三个Join写法一样，只是方法名字不同
var result = repository.From<Product>()
    .LeftJoin<Brand>((a, b) => a.BrandId = b.Id)
    .Where((a, b) => a.ProductNo.Contains("PN-00"))
    .ToList;
//SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
```
> 注意：
> 使用Join<T>(f=>f.From...)的子查询，相当于`WithTable`+`Join`两个操作


#### 单表聚合操作
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

#### `Union`和`UnionAll`查询

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
```

#### 带有`OrderBy`和`Take`的`Union`，会变成一个子查询用`SELECT * FROM ()`包装一下，在里面完成`OrderBy`和`Take`操作
```csharp
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
//SQL:
SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`<3 ORDER BY a.`Id` LIMIT 1) a UNION ALL
SELECT * FROM (SELECT a.`Id`,a.`OrderNo`,a.`SellerId`,a.`BuyerId` FROM `sys_order` a WHERE a.`Id`>2 LIMIT 1) a
```
> 注意：
> 带有`OrderBy`和`Take`的`Union`，会生成一个子查询，在里面完成`OrderBy`和`Take`操作


#### `CTE`支持
`CTE`其实也是一个子查询，使用`AsCteTable(string tableName)`方法把一个子查询包装成一个`CTE`表，可以在查询中直接使用，也可以单独声明使用，两者效果是一样的  
在`CTE`的子查询中，可以使用`UnionAllRecursive`或`UnionRecursive`方法，完成自身引用实现递归查询


#### 直接在查询中使用
```csharp
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
    .ToList();
SQL:
WITH `MenuList`(`Id`,`Name`,`ParentId`,`PageId`) AS 
(
SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a WHERE a.`Id`>=@p0
)
SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `MenuList` a INNER JOIN `sys_page` b ON a.`Id`=b.`Id` WHERE b.`Id`>=@p1
```

#### 也可以单独声明`CTE`表，在后续的多个查询中都可以引用
```csharp
var myCteTable1 = repository
    .From<Menu>()
        .Where(x => x.Id == rootId)
        .Select(x => new { x.Id, x.Name, x.ParentId })
    .UnionAllRecursive((x, self) => x.From<Menu>()
        .InnerJoin(self, (a, b) => a.ParentId == b.Id)
        .Select((a, b) => new { a.Id, a.Name, a.ParentId }))
    .AsCteTable("myCteTable1");

//上面子句中，包含`UnionAllRecursive`方法，递归查询
var myCteTable2 = repository
    .From<Page, Menu>()
        .Where((a, b) => a.Id == b.PageId)
        .Select((x, y) => new { y.Id, y.ParentId, x.Url })
    .UnionAll(x => x.From<Menu>()
        .InnerJoin<Page>((a, b) => a.PageId == b.Id)
        .Select((x, y) => new { x.Id, x.ParentId, y.Url }))
    .AsCteTable("myCteTable2");

var result = repository
    .From(myCteTable1)
    .InnerJoin(myCteTable2, (a, b) => a.Id == b.Id)
    .Select((a, b) => new { a.Id, b.Name, a.ParentId, a.Url })
    .ToList();
	
//SQL:
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
```
#### `CTE`递归查询
只需要在子句中使用`UnionAllRecursive`或`UnionRecursive`方法，就可以实现递归查询，如上面的`CTE`子句中`myCteTable1`和下面`CTE`子句中`menuList`


多个`CTE`子句，单独声明更清晰一点
单独声明`CTE`表，可以在多个查询中引用，通常会发生参数名重复情况，可以使用`ToParameter`方法，改变参数名，不只是`CTE`表，子查询也可以使用`ToParameter`方法更改参数名，如下：

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
`rootId`变量生成的参数名为：`@RootId`,这样可以避免后面查询中的参数重名

多个`CTE`表也可以直接声明使用
```csharp
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

//SQL:
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
```
> 注意： 一般CTE常用来处理树形结构递归操作，比如：根据当前角色获取菜单列表
> 从叶子找菜单根，或是从菜单根查找所有叶子，都是递归CTE的常用场景



#### 多语句查询 QueryMultiple
`Trolley`提供多语句查询，通过调用下面方法实现，不需要写SQL  
每个子句都可以使用上面用到的所有查询方法

```csharp
IMultiQueryReader QueryMultiple(Action<IMultipleQuery> subQueries);
Task<IMultiQueryReader> QueryMultipleAsync(Action<IMultipleQuery> subQueries, CancellationToken cancellationToken = default);
```

```csharp
var repository = dbFactory.Create();
var reader = await repository.QueryMultipleAsync(f => f
    .Get<User>(new { Id = 1 })
    .Exists<Order>(f => f.BuyerId.IsNull())
    .From<Order>()
        .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
        .Where((x, y) => x.Id == "1")
        .Select((x, y) => new { x.Id, x.OrderNo, x.BuyerId, BuyerName = y.Name, x.TotalAmount })
        .First()
    .QueryFirst<User>(new { Id = 2 })
    .From<Product>()
        .Include(f => f.Brand)
        .Where(f => f.ProductNo.Contains("PN-00"))
        .ToList()
    .From(f => f.From<Order, OrderDetail>('a')
            .Where((a, b) => a.Id == b.OrderId && a.Id == "1")
            .GroupBy((a, b) => new { a.BuyerId, OrderId = a.Id })
            .Having((x, a, b) => Sql.CountDistinct(b.ProductId) > 0)
            .Select((x, a, b) => new { a.Id, x.Grouping, ProductTotal = Sql.CountDistinct(b.ProductId), BuyerId1 = x.Grouping.BuyerId }))
        .InnerJoin<User>((x, y) => x.Grouping.BuyerId == y.Id)
        .Select((x, y) => new { x.Id, x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })
        .First());
var sql = reader.ToSql(out var dbParameters);
var userInfo = await reader.ReadFirstAsync<User>();
var isExists = await reader.ReadFirstAsync<bool>();
var orderInfo = await reader.ReadFirstAsync<dynamic>();
var userInfo2 = await reader.ReadFirstAsync<User>();
var products = await reader.ReadAsync<Product>();
var groupedOrderInfo = await reader.ReadFirstAsync<dynamic>();

//生成的SQL，用;拼接在一起
//SELECT `Id`,`TenantId`,`Name`,`Gender`,`Age`,`CompanyId`,`SomeTimes`,`GuidField`,`SourceType`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id`=@Id_m0;SELECT COUNT(1) FROM `sys_order` a WHERE a.`BuyerId` IS NULL;SELECT a.`Id`,a.`OrderNo`,a.`BuyerId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`Id`='1';SELECT `Id`,`TenantId`,`Name`,`Gender`,`Age`,`CompanyId`,`SomeTimes`,`GuidField`,`SourceType`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id`=@Id_m3;SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%';SELECT a.`Id`,a.`BuyerId`,a.`OrderId`,a.`BuyerId`,a.`ProductTotal`,b.`Name` AS `BuyerName`,a.`BuyerId1` AS `BuyerId2` FROM (SELECT a.`Id`,a.`BuyerId`,a.`Id` AS `OrderId`,COUNT(DISTINCT b.`ProductId`) AS `ProductTotal`,a.`BuyerId` AS `BuyerId1` FROM `sys_order` a,`sys_order_detail` b WHERE a.`Id`=b.`OrderId` AND a.`Id`='1' GROUP BY a.`BuyerId`,a.`Id` HAVING COUNT(DISTINCT b.`ProductId`)>0) a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id`

上面语句`var groupedOrderInfo = await reader.ReadFirstAsync<dynamic>();`中，尽管使用了`dynamic`关键字，其实里面的类型就是`.Select((x, y) => new { x.Id, x.Grouping, x.Grouping.BuyerId, x.ProductTotal, BuyerName = y.Name, BuyerId2 = x.BuyerId1 })`方法返回的类型
```




特殊用法
------------------------------------------------------------

#### 对`NULL`的支持
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

#### 对于非`Nullable<>`字段，也可以使用`IsNull()`扩展方法来进行判断
```csharp
var sql = repository.From<Order>()
    .Where(x => x.ProductCount == null || x.BuyerId.IsNull())
    .And(true, f => !f.ProductCount.HasValue)
    .Select(x => x.Id)
    .ToSql(out _);
//SELECT `Id` FROM `sys_order` WHERE (`ProductCount` IS NULL OR `BuyerId` IS NULL) AND `ProductCount` IS NULL
```

#### `ITypeHandler`类型处理器
对特殊类型进行处理，不是默认映射，就需要`TypeHandler`类型处理器类处理，完成模型与数据库之间的数据转换  
`Trolley`内置2个类型处理器`JsonTypeHandler`,`ToStringTypeHandler`，这2个类型处理器在启动时会自动注册，映射时可以直接使用  
默认情况下，只有`object`类型或是实体类类型的类成员，才需要手动指定，一般的基础类型，`Trolley`都会根据字段类型进行自动映射，除非你想特殊处理，才需要创建自己的类型处理器  

#### `JsonTypeHandler`类型处理器支持`Json`
在类型映射时，配置字段为`json`类型，并指定`JsonTypeHandler`类型处理器  
在查询中，直接实体成员操作，到数据库中就是`json`类型，`JsonTypeHandler`类型处理器会自动把实体成员序列化为`json`，从数据库中读取时，也会自动反序列化操作

```csharp
builder.Entity<Order>(f =>
{
    //...
    //手动指定JsonTypeHandler
    f.Member(t => t.Products).Field(nameof(Order.Products)).DbColumnType("longtext").NativeDbType(MySqlDbType.JSON).Position(9).TypeHandler<JsonTypeHandler>();
    f.Member(t => t.Disputes).Field(nameof(Order.Disputes)).DbColumnType("longtext").NativeDbType(MySqlDbType.JSON).Position(10).TypeHandler<JsonTypeHandler>();
    //...
});

//Order模型的属性Products和Disputes，数据库中都是Json类型，Products类型是List<int>，Disputes类型是Dispute类，自动反序列化
var result = repository.Get<Order>(1);
Assert.NotNull(result);
Assert.NotNull(result.Products);
Assert.NotNull(result.Disputes);
```


#### 可以使用`SelectFlattenTo`方法完成`SELECT`操作，直接完成到目标类型的直接转换，减少很多代码量，通常是`DTO`。
`SelectFlattenTo`方法，会先按照方法参数中指定的字段进行设置，其他字段会根据当前所有`Select`出来的字段，根据相同的名称进行设置目标属性，如果存在多个相同的字段，取第一个表的字段。
`SelectFlattenTo`方法，会从数据库直接和DTO类型映射。

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

#### 支持本地方法调用
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
//本地的扩展方法ToDescription，获取枚举的描述并缓存
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
        //调用本地扩展方法
        ActivityTypeName = f.ActivityType.ToDescription(),
        //调用本地扩展方法
        StatusName = f.Status.ToDescription()
    }))
    .Page(request.PageIndex, request.PageSize)
    .ToPageListAsync();
```
属性`ActivityTypeName`和`StatusName`做了特殊处理，其他的属性根据名称相同匹配原则，自动设置到`ActivityQueryResponse`中

```csharp
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

#### 使用`Deferred`方法强制延迟执行
有些方法的调用，解析到数据库中去执行会发生错误，使用此方法可以在数据库执行之后再执行。

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

#### 使用`ToParameter`方法，指定参数名称
子查询或是单独的`CTE`表包含参数，与查询语句中参数名重复，导致报错无法执行，可以使用本方法更改参数名

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

#### `ValueTuple`和多字段`Struct`支持

```csharp
//查询ValueTuple
var sql = "SELECT Id,OrderNo,TotalAmount FROM sys_order";
var result = repository.Query<(int OrderId, string OrderNo, double TotalAmount)>(sql);
```
> 注意：
> 有`DTO`对象接收，有`SelectFlattenTo`方法支持，无需使用`ValueTuple`


#### `IsNull`扩展方法
有两个方法，前者主要用于`WHERE`条件判断，后者主要用在`SELECT`中`NULL`值的默认值
```csharp
bool IsNull<TField>(this TField field)
TField IsNull<TField>(this TField field, TField nullVaueExpr)
```

有时候数据库字段是可为空的，实体字段却不是可为空类型的，需要判断是否为空，可以使用本方法
```csharp
var result = repository.From<Order>()
	.Where(x => x.ProductCount == null || x.BuyerId.IsNull())
	.And(true, f => !f.ProductCount.HasValue)
	.Select(x => x.Id)
	.ToList();
//SELECT a.`Id` FROM `sys_order` a WHERE (a.`ProductCount` IS NULL OR a.`BuyerId` IS NULL) AND a.`ProductCount` IS NULL


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
//SELECT a.`Id`,a.`OrderNo`,IFNULL(a.`ProductCount`,0) AS `ProductCount`,IFNULL(a.`BuyerId`,0) AS `BuyerId`,IFNULL(a.`TotalAmount`,0) AS `TotalAmount` FROM `sys_order` a WHERE IFNULL(a.`ProductCount`,0)>0 OR IFNULL(a.`BuyerId`,0)>=0
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
       .Register(OrmProviderType.SqlServer, "dbKey1", "Server=127.0.0.1;Database=fengling;Uid=sa;password=ABCwsx123456;TrustServerCertificate=true", true)
       .Register(OrmProviderType.MySql, "dbKey2", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;")
       .Configure<SqlServerModelConfiguration>(OrmProviderType.SqlServer)
       .Configure<MySqlModelConfiguration>(OrmProviderType.MySql);
    return builder.Build();
});
var serviceProvider = services.BuildServiceProvider();
dbFactory = serviceProvider.GetService<IOrmDbFactory>();

var repository = this.dbFactory.CreateRepository("dbKey1");
访问的就是SqlServer数据库

var repository = this.dbFactory.CreateRepository("dbKey2");
访问的就是MySql数据库
```

#### 指定分库规则，`Trolley`会根据分库规则来获取`dbKey`
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
        .Register(OrmProviderType.MySql, "fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", true)
        .Register(OrmProviderType.MySql, "fengling_tenant1", "Server=localhost;Database=fengling_tenant1;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", false)
        .Register(OrmProviderType.MySql, "fengling_tenant2", "Server=localhost;Database=fengling_tenant2;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", false)
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
        });
	return builder.Build();
});
var serviceProvider = services.BuildServiceProvider();
dbFactory = serviceProvider.GetService<IOrmDbFactory>();
var repository = this.dbFactory.CreateRepository();
根据分库规则，当前租户ID是200,就会使用`fengling_tenant1`的`dbKey`
如果当前租户ID是300,就会使用`fengling_tenant2`的`dbKey`，否则就是默认`dbKey`: `fengling`
使用分库规则的好处是，当原有的分库规则无法满足业务要求时，只需要增加新规则逻辑，兼容原有规则或根本就不需要更改原有逻辑，并且也不需要迁移数据，就可以完成分库规则的更改。
```


### `dbKey`的选取逻辑：
按照以下步骤选取`dbKey`  
  1. 如果有指定`dbKey`，优先使用指定的`dbKey`，创建`IRepository`对象  
  2. 如果有指定分库规则，则使用分库规则获取`dbKey`，创建`IRepository`对象  
  3. 都没有指定，就选取配置的默认数据库的`dbKey`，创建`IRepository`对象
```csharp
var repository = this.dbFactory.CreateRepository("fengling");
var repository = this.dbFactory.CreateRepository();
```




各种操作命令
------------------------------------------------------------
`Trolley`的所有插入、更新操作，都可以使用命名、匿名对象、字典类型(Dictionary<string, object>)等参数完成，
插入和更新、删除接口支持得比较丰富，基本大多数场景都支持，对`enum`,`null`都做了支持，还支持特殊的本地化数据库操作，比如：mySql的Insert Ignore, OnDuplicateKeyUpdate等
所有插入、更新、删除操作都支持批量操作，插入和更新还支持`BulkCopy`操作



### 新增
支持匿名对象、实体对象、字典参数插入
支持单条、批量操作，也支持`BulkCopy`操作
对`enum`,`null`做了特殊支持
实体对象，原值插入，字段值是`0`，插入数据库后也是`0`，字段值是`null`，插入数据库后也是`NULL`
如果不想插入类型默认值，可以使用匿名对象不插入字段，或是把字段设置为可为null类型，带`?`类型


#### 简单基本操作方法，不支持分库分表
```csharp
Create<User>(object insertObjs, int bulkCount = 500);
CreateAsync<TEntity>(object insertObjs, int bulkCount = 500, CancellationToken cancellationToken = default);
```

#### 支持单插入，也支持多条批量插入
```csharp
var repository = this.dbFactory.CreateRepository();

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

#### 批量插入
采用的是多表值方式，就是`INSERT TABLE(...) VALUES(..),(...),(...)...`
这种方式相对于普通插入方式性能要高，但不适合大批量，适合小批量的
可设置单次入库的数据条数，可根据表插入字段个数和条数，设置一个性能最高的条数
通常会有一个插入性能最高的阈值，高于或低于这个阈值，批量插入的性能都会有所下降
这个阈值和数据库类型、插入字段的个数,参数个数，都有关系。
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

#### 自增长列，无需为自增长列赋值，并返回增长的ID值
在字段映射的时候需要设置为自增长列`AutoIncrement()`

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

#### 使用`Create&lt;T&gt;()`方法，支持更复杂的场景
WithBy方法可以多次调用

支持带条件的插入字段数据，可以使用命名、匿名对象、字典类型(Dictionary<string, object>)等参数完成。使用匿名对象的好处，没有的字段将不会插入值。
这样可避免基础类型插入了默认值，如：整型插入`0`,字符串类型插入了`''`等，或者插入的列数据赋值`null`或不赋值，也不会插入空字符串数据

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

#### 对`json`的支持
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

#### 使用`WithBulk`方法，支持批量操作
`Trolley`的批量新增采用的是多表值方式，就是`INSERT TABLE(...) VALUES(..),(...),(...)...`  
`WithBulk`之后，可以继续使用`WithBy`,`OnlyFields`,`IgnoreFields`方法  
这种方式相对于普通插入方式性能要高，但不适合大批量，适合小批量的，可设置单次入库的数据条数，可根据表插入字段个数和条数，设置一个性能最高的条数  
通常会有一个插入性能最高的阈值，高于或低于这个阈值，批量插入的性能都会有所下降，这个阈值和数据库类型、插入字段的个数,参数个数，都有关系。  

效果同简单基本操作方法是一样的。
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
    })
    .Execute();
```

#### `BulkCopy`支持
大批量的插入，推荐使用`WithBulkCopy`方法，性能更优
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

#### 部分字段插入`OnlyFields`
单条、批量，都可以使用

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

#### 忽略字段插入`IgnoreFields`
单条、批量插入，都可以使用

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
        SourceType = UserSourceType.Douyin,
        IsEnabled = true,
        CreatedAt = now,
        CreatedBy = 1,
        UpdatedAt = now,
        UpdatedBy = 1
   })
   .IgnoreFields(f => new { f.Gender, f.CompanyId })
   .Execute();
```

#### 对枚举`enum`的支持
`Trolley`允许在代码中使用`enum`类型方便开发，在数据库端可以是数字类型或是字符串类型
只需要在映射的时候设置为对应的`DbType`或是对应的`Int`类型值即可
在实际项目中，通常会在项目中使用`enum`类型，数据库端使用字符串类型字段，这样可读性比较强，字符串类型时使用枚举名字保存

```csharp
builder.Entity<User>(f =>
{
    f.ToTable("sys_user").Key(t => t.Id);
    ... ...
    //设置数字类型，MySqlDbType.Byte,Int16,Int32,UByte,UInt16,UInt32,.. 等数字类型都可以，不要设置为浮点数
    f.Member(t => t.Gender).Field("Gender").DbColumnType("tinyint(4)").NativeDbType(MySqlDbType.Byte).Position(4);// 或是下面的整型数字也可以
    f.Member(t => t.Gender).Field("Gender").DbColumnType("tinyint(4)").NativeDbType(1).Position(4);
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
    f.Member(t => t.Nature).Field("Nature").DbColumnType("varchar(50)").NativeDbType(MySqlDbType.VarChar).Position(3).Length(50);
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

 
#### `Insert Select From`联表插入
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
	
#### `Insert From`关联多张表插入
可以直接使用`From`多张表或是一张表一张表的`Join`

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

#### `Insert From`支持CTE子句
与子查询`From`用法一样，把`CTE`表当作一个子查询处理，不同的的数据库生成的`SQL`会有些不同
```csharp
var ordersQuery = repository.From<OrderDetail>()
    .GroupBy(f => f.OrderId)
    .Select((x, f) => new
    {
	Id = f.OrderId,
	TenantId = "1",
	OrderNo = $"ON-{f.OrderId}",
	BuyerId = 1,
	SellerId = 1,
	BuyerSource = UserSourceType.Taobao.ToString(),
	ProductCount = 2,
	TotalAmount = x.Sum(f.Amount),
	IsEnabled = true,
	CreatedAt = DateTime.Now,
	CreatedBy = 1,
	UpdatedAt = DateTime.Now,
	UpdatedBy = 1
    })
    .AsCteTable("orders");
var result = await repository.Create<Order>()
    .From(ordersQuery)
    .ExecuteAsync();
//INSERT INTO `sys_order` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`ProductCount`,`TotalAmount`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) WITH 
`orders`(`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`ProductCount`,`TotalAmount`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) AS 
(
SELECT a.`OrderId`,'1',CONCAT('ON-',a.`OrderId`),1,1,'Taobao',2,SUM(a.`Amount`),1,NOW(),1,NOW(),1 FROM `sys_order_detail` a GROUP BY a.`OrderId`
)
SELECT b.`Id`,b.`TenantId`,b.`OrderNo`,b.`BuyerId`,b.`SellerId`,b.`BuyerSource`,b.`ProductCount`,b.`TotalAmount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `orders` b
```



#### 本地化`mysql`/`mariadb`数据库支持
支持`IgnoreInto`,`OnDuplicateKeyUpdate`操作，单条插入、批量插入，都可以使用

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

#### `OnDuplicateKeyUpdate`支持三种方式
直接使用参数Set，或者使用`VALUES`带别名，或是不带别名进行Set都可以，却决于`mysql`/`mariadb`的版本，推荐使用`VALUES`方式

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

#### `mariadb`数据库还支持`Returning`操作，也依赖`mariadb`数据库的版本，有的版本支持，有的版本不支持
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

#### `SqlServer`数据库支持`Output`操作,也依赖`SqlServer`数据库的版本，有的版本支持，有的版本不支持
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

其他数据库本地化支持，有待后续丰富...



### 更新
支持命名、匿名对象、字典参数更新，支持单条、批量操作，也支持`BulkCopy`  
对`enum`,`null`做了特殊支持  
批量更新，是`By`主键更新的


#### 基本简单操作
支持单条、批量操作   
命名、匿名对象、字典参数都可以  
使用匿名对象，存在的字段将会参与更新 


#### 直接使用参数，By主键更新
```csharp
//直接使用参数，By主键更新
var result = repository.Update<User>(new { Id = 1, Name = "leafkevin11" });
//UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=@kId
```

#### 用表达式，Where条件更新
```csharp
//使用表达式，Where条件更新
var result = repository.Update<User>(f => new
    {
        parameter.TotalAmount, //直接赋值，使用同名变量
        Products = this.GetProducts(), //直接赋值，使用本地函数
        BuyerId = DBNull.Value, //直接赋值 NULL
        Disputes = new Dispute { ... } //实体对象由JsonTypeHandler序列化后再变成参数
    }, x => x.Id == 1);
//UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes WHERE `Id`=1
```

#### 对`null`的支持
直接使用匿名对象，对应字段设置为DBNull.Value，或是可为空列直接设置`null`也可以，如上例
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

#### 对`enum`的支持
更新枚举列字段，会根据数据字段类型，自动转换为对应的类型
```csharp
repository.Update<User>(new { Id = 1, Gender = Gender.Male });
//UPDATE `sys_user` SET `Gender`=@Gender WHERE `Id`=@kId
//根据主键Id=1条件更新，Gender字段数据库的类型是varchar(50)，字段赋值时给的时枚举值，更新数据库时会把对应的枚举值名称保存到数据库中。
```


#### 批量参数更新
By主键更新，一定要包含主键列，只能使用参数方式

```csharp
//批量参数更新，`Where`条件是主键，除主键外其他的字段将参与更新
var parameters = await repository.From<OrderDetail>()
    .GroupBy(f => f.OrderId)
    .OrderBy((x, f) => f.OrderId)
    .Select((x, f) => new
    {
        Id = x.Grouping,
        TotalAmount = x.Sum(f.Amount) + 50
    })
    .ToListAsync();
var result = repository.Update<OrderDetail>(parameters);
//UPDATE `sys_order` SET `TotalAmount`=@TotalAmount0 WHERE `Id`=@kId0;UPDATE `sys_order` SET `TotalAmount`=@TotalAmount1 WHERE `Id`=@kId1;UPDATE `sys_order` SET `TotalAmount`=@TotalAmount2 WHERE `Id`=@kId2 ...
```




#### 使用Update&lt;T&gt;，支持各种复杂更新操作

#### Set单条更新
一个或多个字段更新，直接使用参数，也可以是表达式，都需要有`Where`子句  
`Set`字句后，可以继续使用`OnlyFields`和`IgnoreFields`方法，也可以多次调用`Set`方法
参数可以是命名对象、匿名对象或是字典对象


#### Set参数 单字段
```csharp
result = repository.Update<Order>()
    .Set(new { parameter.TotalAmount })
    .Where(x => x.Id == "1")
    .Execute();
//UPDATE `sys_order` SET `TotalAmount`=@TotalAmount WHERE `Id`='1'
```

#### Set参数 多字段
```csharp
result = repository.Update<Order>()
    .Set(new
    {
        parameter.TotalAmount,
        Products = new List<int> { 1, 2, 3 },
        Disputes = new Dispute
        {
            Id = 1,
            Content = "43dss",
            Users = "1,2",
            Result = "OK",
            CreatedAt = DateTime.Now
        }
    })
    .Where(x => x.Id == "1")
    .Execute();
//UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`Disputes`=@Disputes WHERE `Id`='1'
//Products和Disputes都是实体类型属性，有指定`JsonTypeHandler`类型处理器，会调用`JsonTypeHandler`.`ToFieldValue`方法序列化成字符串，再变成参数完成后续的更新操作
```

#### Set参数 字典参数
```csharp
var updateObj = new Dictionary<string, object>();
updateObj.Add("ProductCount", result2.ProductCount + 1);
updateObj.Add("TotalAmount", result2.TotalAmount + 100);
result = repository.Update<Order>()
    .Set(updateObj)
    .Where(new { Id = "1" })
    .Execute();
//UPDATE `sys_order` SET `ProductCount`=@ProductCount,`TotalAmount`=@TotalAmount WHERE `Id`=@kId
```
#### Set参数后，可以继续调用`OnlyFields`和`IgnoreFields`方法
```csharp
OnlyFields方法
repository.Update<User>()
    .Set(new
    {
        Age = 30,
        Name = "leafkevinabc",
        CompanyId = 1
    })
    .OnlyFields(f => f.Name)
    .Where(f => f.Id == 1)
    .Execute();
//UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=1

IgnoreFields方法
repository.Update<User>()
    .Set(new
    {
        Age = 25,
        Name = "leafkevin22",
        CompanyId = DBNull.Value
    })
    .IgnoreFields(f => f.Name)
    .Where(f => f.Id == 1)
    .Execute();
//UPDATE `sys_user` SET `Age`=@Age,`CompanyId`=@CompanyId WHERE `Id`=1
```

#### Set表达式 单字段
除了可以使用参数外，也可以引用原值，实现自增、自减...等运算操作

```csharp
//多个字段更新，表达式
var result = repository.Update<Order>()
    .Set(f => f.Age, 20)
    .Where(x => x.Id == "1")
    .Execute();
//UPDATE `sys_order` SET `Age`=@p0 WHERE `Id`='1'
```

#### Set表达式 多字段

```csharp
var result = repository.Update<Order>()
    .Set(f => new
    {
        parameter.TotalAmount,
        Products = new List<int> { 1, 2, 3 },
        Disputes = new Dispute
        {
            Id = 1,
            Content = "43dss",
            Users = "1,2",
            Result = "OK",
            CreatedAt = DateTime.Now
        }
    })
    .Where(x => x.Id == "1")
    .Execute();
//UPDATE `sys_order` SET `TotalAmount`=@p0,`Products`=@p1,`Disputes`=@p2 WHERE `Id`='1'
```

#### Set表达式 原值引用，自增、自减...等 运算操作
除了可以使用参数外，也可以引用原值，实现自增、自减...等运算操作

```csharp
//多个字段更新，表达式
var result = repository.Update<Order>()
    .Set(f => new
    {
        TotalAmount = f.TotalAmount + 50,
        Products = new List<int> { 1, 2, 3 }
    })
    .Where(x => x.Id == "1")
    .Execute();
//UPDATE `sys_order` SET `TotalAmount`=`TotalAmount`+50,`Products`=@p0 WHERE `Id`='1'
```

#### Set方法可多次调用
参数、表达式交替使用
```csharp
var count = await repository.Update<Order>()
    .Set(new { TotalAmount = 300d })
    .Set(x => x.OrderNo, "ON_111")
    .Set(f => new { BuyerId = DBNull.Value })
    .Where(a => a.Id == "1")
    .ExecuteAsync();
//UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`OrderNo`=@OrderNo,`BuyerId`=NULL WHERE `Id`='1'
```

#### SetFrom 单字段联表更新
某个字段值是从一个或多个表联合查询出来的，支持以下三种方式
```csharp
SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment);
SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector);
```

```csharp
await repository.Update<Order>()
    .SetFrom((x, y) => new
    {
        TotalAmount = x.From<OrderDetail>('b')
            .Where(f => f.OrderId == y.Id)
            .Select(t => Sql.Sum(t.Amount))
    })
    .Set(x => x.OrderNo, "ON_111")
    .Set(f => new { BuyerId = DBNull.Value })
    .Where(a => a.BuyerId == 1)
    .ExecuteAsync();
//UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`BuyerId`=1

result = repository.Update<Order>()
    .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
    .SetFrom(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
        .Where(f => f.OrderId == y.Id)
        .Select(t => Sql.Sum(t.Amount)))
    .Set((a, b) => new { OrderNo = a.OrderNo + " - " + b.Id.ToString() })
    .Set((x, y) => new { BuyerId = DBNull.Value })
    .Where((x, y) => x.Id == "2")
    .Execute();
```

#### Set、SetFrom方法可多次调用，也可结合`OnlyFields`和`IgnoreFields`方法使用
```csharp
await repository.Update<Order>()
    .SetFrom((x, y) => new
    {
        TotalAmount = x.From<OrderDetail>('b')
            .Where(f => f.OrderId == y.Id)
            .Select(t => Sql.Sum(t.Amount))
    })
    .Set(x => x.OrderNo, "ON_111")
    .Set(f => new { BuyerId = DBNull.Value })
    .Where(a => a.BuyerId == 1)
    .ExecuteAsync();
//UPDATE `sys_order` a SET a.`TotalAmount`=(SELECT SUM(b.`Amount`) FROM `sys_order_detail` b WHERE b.`OrderId`=a.`Id`),a.`OrderNo`=@OrderNo,a.`BuyerId`=NULL WHERE a.`BuyerId`=1
```


	
#### SetBulk 批量更新
By主键更新，`SetBulk`之后，可以继续使用`Set`,`SetFrom`,`OnlyFields`,`IgnoreFields`方法  
生成的SQL是多个UPDATE语句拼接在一起后的SQL，这种方式相对于普通更新方式性能要高，但不适合大批量，适合小批量的，可设置单次更新的数据条数，可根据表更新字段个数和条数，设置一个性能最高的条数  
通常会有一个更新性能最高的阈值，高于或低于这个阈值，批量更新的性能都会有所下降，这个阈值和数据库类型、更新字段的个数，参数个数，都有关系。

```csharp
var parameters = await repository.From<OrderDetail>()
    .Where(f => new int[] { 1, 2, 3, 4, 5, 6 }.Contains(f.Id))
    .Select(f => new { f.Id, Price = f.Price + 80, Quantity = f.Quantity + 2, Amount = f.Amount + 100 })
    .ToListAsync();
var result = repository.Update<OrderDetail>()
    .WithBulk(parameters)
    .Execute();
//UPDATE `sys_order_detail` SET `Price`=@Price0,`Quantity`=@Quantity0,`Amount`=@Amount0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Price`=@Price1,`Quantity`=@Quantity1,`Amount`=@Amount1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Price`=@Price2,`Quantity`=@Quantity2,`Amount`=@Amount2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Price`=@Price3,`Quantity`=@Quantity3,`Amount`=@Amount3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Price`=@Price4,`Quantity`=@Quantity4,`Amount`=@Amount4 WHERE `Id`=@kId4;UPDATE `sys_order_detail` SET `Price`=@Price5,`Quantity`=@Quantity5,`Amount`=@Amount5 WHERE `Id`=@kId5
```

#### SetBulk 批量更新 结合`Set`,`SetFrom`,`OnlyFields`,`IgnoreFields`方法

```csharp
var orderDetails = await repository.From<OrderDetail>()
    .Select(f => new
    {
        f.Id,
        Amount = f.Amount + 50,
        UpdatedAt = f.UpdatedAt.AddDays(1)
    })
    .OrderBy(f => f.Id)
    .Take(5)
    .ToListAsync();

var result = repository.Update<OrderDetail>()
    .SetBulk(parameters)
    .Set(f => f.ProductId, 3)
    .Set(new { Price = 200, Quantity = 5 })
    .IgnoreFields(f => f.Price)
    .Execute();
//UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `ProductId`=@ProductId,`Quantity`=@Quantity,`Price`=`Price`+10,`Amount`=@Amount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4


var result = repository.Update<OrderDetail>()
    .SetBulk(parameters)
    .Set(f => f.ProductId, 3)
    .Set(new { Quantity = 5 })
    .Set(f => new { Price = f.Price + 10 })
    .Execute();
//UPDATE [sys_order_detail] SET [ProductId]=@ProductId,[Quantity]=@Quantity,[Price]=[Price]+10,[Amount]=@Amount0,[UpdatedAt]=@UpdatedAt0 WHERE [Id]=@kId0;UPDATE [sys_order_detail] SET [ProductId]=@ProductId,[Quantity]=@Quantity,[Price]=[Price]+10,[Amount]=@Amount1,[UpdatedAt]=@UpdatedAt1 WHERE [Id]=@kId1;UPDATE [sys_order_detail] SET [ProductId]=@ProductId,[Quantity]=@Quantity,[Price]=[Price]+10,[Amount]=@Amount2,[UpdatedAt]=@UpdatedAt2 WHERE [Id]=@kId2;UPDATE [sys_order_detail] SET [ProductId]=@ProductId,[Quantity]=@Quantity,[Price]=[Price]+10,[Amount]=@Amount3,[UpdatedAt]=@UpdatedAt3 WHERE [Id]=@kId3;UPDATE [sys_order_detail] SET [ProductId]=@ProductId,[Quantity]=@Quantity,[Price]=[Price]+10,[Amount]=@Amount4,[UpdatedAt]=@UpdatedAt4 WHERE [Id]=@kId4
```

`SetBulk`方法只能使用一次，`Set`,`SetFrom`可以使用多次


#### Update&lt;T&gt; Join 联表更新
使用`Set`,`SetFrom`方法完成字段更新，只支持`InnerJoin`和`LeftJoin`两种关联方式
支持的数据库：`MySql`，`Mariadb`，`SqlServer`，`PostgreSql`，生成的`SQL`会有一些差别

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
//UPDATE "sys_order" AS a SET "TotalAmount"=@TotalAmount,"OrderNo"=CONCAT(a."OrderNo",'_111'),"BuyerId"=NULL FROM "sys_order_detail" b WHERE a."Id"=b."OrderId" AND a."BuyerId"=1
//UPDATE a SET a.[TotalAmount]=@TotalAmount,a.[OrderNo]=a.[OrderNo]+'_111',a.[BuyerId]=NULL FROM [sys_order] a INNER JOIN [sys_order_detail] b ON a.[Id]=b.[OrderId] WHERE a.[BuyerId]=1

//包含SetFrom
var sql = repository.Update<Order>()
    .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
    .SetFrom(f => f.TotalAmount, (x, y) => x.From<OrderDetail>('c')
	.Where(f => f.OrderId == y.Id)
	.Select(t => Sql.Sum(t.Amount)))
    .Set((a, b) => new { OrderNo = a.OrderNo + b.ProductId.ToString() })
    .Set((x, y) => new { BuyerId = DBNull.Value })
    .Where((a, b) => a.BuyerId == 1)
    .ToSql(out _);
//UPDATE `sys_order` a INNER JOIN `sys_order_detail` b ON a.`Id`=b.`OrderId`SET a.`TotalAmount`=(SELECT SUM(c.`Amount`) FROM `sys_order_detail` c WHERE c.`OrderId`=a.`Id`),a.`OrderNo`=CONCAT(a.`OrderNo`,CAST(b.`ProductId` AS CHAR)),a.`BuyerId`=NULL WHERE a.`BuyerId`=1
```


#### Set Null 用DBNull.Value或是null
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

#### Set 对枚举的支持
设置时，可以直接设置枚举值，更新时会根据数据库字段的类型进行转成对应的值，如果是字符串会转换成枚举名称
```csharp
var result = repository.Update<Company>()
    .SetBulk(new[] { new { Id = 1, company.Nature }, new { Id = 2, Nature = nature } })
    .Set(f => new { company.Name })
    .OnlyFields(f => f.Nature)
    .Execute();
//UPDATE [sys_company] SET [Name]=@p0,[Nature]=@Nature0 WHERE [Id]=@kId0;UPDATE [sys_company] SET [Name]=@p0,[Nature]=@Nature1 WHERE [Id]=@kId1
```

#### Set 对Json的支持
只要在字段映射的设置的时候，有配置`JsonTypeHandler`类型处理器，在`Set`时候，直接使用对象值，在更新的时候，自动调用`JsonTypeHandler`的`ToFieldValue`序列化字符串再完成更新
```csharp
var result = repository.Update<Order>()
    .Set(f => new
    {
        Products = new List<int> { 1, 2, 3 },
        Disputes = new Dispute
        {
            Id = 1,
            Content = "43dss",
            Users = "1,2",
            Result = "OK",
            CreatedAt = DateTime.Now
        }
    })
    .Where(x => x.Id == "1")
    .Execute();
//UPDATE `sys_order` SET `Products`=@p0,`Disputes`=@p1 WHERE `Id`='1'
```

#### 支持本地方法调用
直接调用本地函数，得到值后再执行数据库操作

```csharp
var updateObj = repository.Get<Order>("1");
updateObj.Disputes = new Dispute
{
    Id = 2,
    Content = "无良商家",
    Result = "同意退款",
    Users = "Buyer2,Seller2",
    CreatedAt = DateTime.Now
};
updateObj.UpdatedAt = DateTime.Now;
int increasedAmount = 50;
var result = repository.Update<Order>()
    .Set(f => new
    {
        TotalAmount = this.CalcAmount(updateObj.TotalAmount + increasedAmount, 3),
        Products = this.GetProducts(),
        updateObj.Disputes,
        UpdatedAt = DateTime.Now
    })
    .Where(new { updateObj.Id })
    .Execute();
private double CalcAmount(double price, double amount) => price * amount - 150;
private int[] GetProducts() => new int[] { 1, 2, 3 };

//UPDATE [sys_order] SET [TotalAmount]=@p0,[Products]=@p1,[Disputes]=@p2,[UpdatedAt]=GETDATE() WHERE [Id]=@kId
```

#### BulkCopy 更新支持
`BulkCopy`支持大量数据更新，先将数据`BulkCopy`到数据库中，再使用联表更新，当数据量很大时，性能比`SetBulk`要强很多

```csharp
var orders = new List<Order>();
for (int i = 0; i < 5000; i++)
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

 

#### 删除
支持单条、批量删除，使用参数、值，表达式都可以完成

#### 单条删除 表达式
```csharp
var count = await repository.DeleteAsync<User>(f => f.Id == 1);	
//DELETE FROM [sys_user] WHERE [Id]=1
```

#### 批量删除 只能使用参数方式
可以使用主键值的数组或是列表，或是包含主键值的对象数组或是列表都可以
`Trolley`会根据删除表的主键个数多少决定是用`IN`操作还是`OR`操作，多个主键就是`OR`操作，一个主键就是`IN`

#### 包含主键值的对象列表
```csharp
var count = await repository.DeleteAsync<User>(new[] { new { Id = 1 }, new { Id = 2 } });
//DELETE FROM `sys_user` WHERE `Id` IN (@Id0,@Id1)
```

#### 主键值列表
```csharp
var count = await repository.DeleteAsync<User>(new int[] { 1, 2 });
//DELETE FROM `sys_user` WHERE `Id` IN (@Id0,@Id1)
```
上面两种方式生成的`SQL`完全一样的

#### 多主键值的对象列表
多主键的时候，就会使用`OR`操作
```csharp
var count = repository.Delete<Function>()
    .Where(new[] { new { MenuId = 1, PageId = 1 }, new { MenuId = 2, PageId = 2 } })
    .Execute();
//DELETE FROM `sys_function` WHERE `MenuId`=@MenuId0 AND `PageId`=@PageId0 OR `MenuId`=@MenuId1 AND `PageId`=@PageId1
```
#### 表达式条件删除
```csharp
repository.Delete<User>()
    .Where(f => f.Id == 1)
    .Execute();
//DELETE FROM `sys_user` WHERE `Id`=1
```

#### 同样Delete&lt;T&gt; 支持更多的删除操作
可以使用`Where`、多次`And`操作
```csharp
repository.Delete<User>()
    .Where(f => f.Id == 1)
    .Execute();
repository.Delete<User>()
    .Where(new int[] { 1, 2 })
    .Execute()
	
bool? isMale = true;
var sql = repository.Delete<User>()
    .Where(f => f.Name.Contains("kevin"))
    .And(isMale.HasValue, f => f.Age > 25)
    .Execute();
//DELETE FROM [sys_user] WHERE [Name] LIKE '%kevin%' AND [Age]>25

var orderNos = new string[] { "ON_001", "ON_002", "ON_003" };
count = repository.Delete<Order>()
    .Where(f => f.BuyerId == 1 && orderNos.Contains(f.OrderNo))
    .Execute();
//DELETE FROM `sys_order` WHERE `BuyerId`=1 AND `OrderNo` IN (@p0,@p1,@p2)
```

#### 也支持对NULL和枚举类型
```csharp
var count = repository.Delete<Company>()
     .Where(f => f.Nature == nature)
     .Execute();
//DELETE FROM `sys_company` WHERE `Nature`=@p0
```



### 事务处理
`Trolley`的每个`IRepository`对象包含了一个连接，所有的操作都是在这个连接上操作的，所以，在`IRepository`对象上直接使用以下方法完成事务操作
```csharp
void BeginTransaction();
Task BeginTransactionAsync(CancellationToken cancellationToken = default);
void Commit();
Task CommitAsync(CancellationToken cancellationToken = default);
void Rollback();
Task RollbackAsync(CancellationToken cancellationToken = default);
```


示例
```csharp
var repository = this.dbFactory.CreateRepository();
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

### 多命令查询
就是多个增删改语句，在一个command中执行，将所有命令的SQL拼接在一起去执行，适用于命令多查询少并发量大的消息队列消费者场景，可以通过下面两个方法完成
```csharp
int MultipleExecute(List<MultipleCommand> commands);
Task<int> MultipleExecuteAsync(List<MultipleCommand> commands, CancellationToken cancellationToken = default);
```

示例：
```csharp
var repository = dbFactory.CreateRepository();
int[] productIds = new int[] { 2, 4, 5, 6 };
int category = 1;
var commands = new List<MultipleCommand>();
var deleteCommand = repository.Delete<Product>()
    .Where(f => productIds.Contains(f.Id))
    .ToMultipleCommand();

var insertCommand = repository.Create<Product>()
   .WithBy(new
   {
       Id = 2,
       ProductNo = "PN_111",
       Name = "PName_111",
       BrandId = 1,
       CategoryId = category,
       CompanyId = 1,
       IsEnabled = true,
       CreatedBy = 1,
       CreatedAt = DateTime.Now,
       UpdatedBy = 1,
       UpdatedAt = DateTime.Now
   })
   .ToMultipleCommand();

var insertCommand2 = repository.Create<Product>()
    .WithBulk(new[]
    {
        new
        {
            Id = 4,
            ProductNo="PN-004",
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
            Id = 5,
            ProductNo="PN-005",
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
            Id = 6,
            ProductNo="PN-006",
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
    .ToMultipleCommand();

var updateCommand = repository.Update<Order>()
   .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
   .Set(true, (x, y) => new
   {
       TotalAmount = 200.56,
       OrderNo = x.OrderNo + "-111",
       BuyerSource = y.SourceType
   })
   .Set(x => x.Products, new List<int> { 1, 2, 3 })
   .Where((a, b) => a.Id == "1")
   .ToMultipleCommand();

var orderDetails = await repository.From<OrderDetail>().ToListAsync();
var parameters = orderDetails.Select(f => new
{
    f.Id,
    Amount = f.Amount + 50,
    UpdatedAt = f.UpdatedAt.AddDays(1)
})
.ToList();
var bulkUpdateCommand = repository.Update<OrderDetail>()
    .SetBulk(parameters)
    .Set(f => f.ProductId, 3)
    .Set(new { Quantity = 5 })
    .Set(f => new { Price = f.Price + 10 })
    .ToMultipleCommand();

commands.AddRange(new[] { deleteCommand, insertCommand, insertCommand2, updateCommand, bulkUpdateCommand });
var count = repository.MultipleExecute(commands);
生成的SQL:
DELETE FROM `sys_product` WHERE `Id` IN (@p0_m0,@p1_m0,@p2_m0,@p3_m0);INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) VALUES (@Id_m1,@ProductNo_m1,@Name_m1,@BrandId_m1,@CategoryId_m1,@CompanyId_m1,@IsEnabled_m1,@CreatedBy_m1,@CreatedAt_m1,@UpdatedBy_m1,@UpdatedAt_m1);INSERT INTO `sys_product` (`Id`,`ProductNo`,`Name`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id0,@ProductNo0,@Name0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@Id1,@ProductNo1,@Name1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@Id2,@ProductNo2,@Name2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2);UPDATE `sys_order` a INNER JOIN `sys_user` b ON a.`BuyerId`=b.`Id` SET a.`TotalAmount`=@p39_m3,a.`OrderNo`=CONCAT(a.`OrderNo`,'-111'),a.`BuyerSource`=b.`SourceType`,a.`Products`=@Products_m3 WHERE a.`Id`='1';UPDATE `sys_order_detail` SET `ProductId`=@ProductId_m4,`Quantity`=@Quantity_m4,`Price`=`Price`+10,`Amount`=@Amount_m40,`UpdatedAt`=@UpdatedAt_m40 WHERE `Id`=@kId_m40;UPDATE `sys_order_detail` SET `ProductId`=@ProductId_m4,`Quantity`=@Quantity_m4,`Price`=`Price`+10,`Amount`=@Amount_m41,`UpdatedAt`=@UpdatedAt_m41 WHERE `Id`=@kId_m41;UPDATE `sys_order_detail` SET `ProductId`=@ProductId_m4,`Quantity`=@Quantity_m4,`Price`=`Price`+10,`Amount`=@Amount_m42,`UpdatedAt`=@UpdatedAt_m42 WHERE `Id`=@kId_m42;UPDATE `sys_order_detail` SET `ProductId`=@ProductId_m4,`Quantity`=@Quantity_m4,`Price`=`Price`+10,`Amount`=@Amount_m43,`UpdatedAt`=@UpdatedAt_m43 WHERE `Id`=@kId_m43;UPDATE `sys_order_detail` SET `ProductId`=@ProductId_m4,`Quantity`=@Quantity_m4,`Price`=`Price`+10,`Amount`=@Amount_m44,`UpdatedAt`=@UpdatedAt_m44 WHERE `Id`=@kId_m44;UPDATE `sys_order_detail` SET `ProductId`=@ProductId_m4,`Quantity`=@Quantity_m4,`Price`=`Price`+10,`Amount`=@Amount_m45,`UpdatedAt`=@UpdatedAt_m45 WHERE `Id`=@kId_m45
```





### 命令超时时间
`Timeout`方法设置超时时间，单位秒
```csharp
repository.Timeout(60);
```
### 参数化设置
表达式解析中，所有变量都会参数化，常量不会参数化。如果设置为true，所有常量也将都会参数化
```csharp
repository.WithParameterized(true);
```



### 分库分表支持
`Trolley`对分库分表的支持，非常灵活，完全依赖规则，可以按租户、时间、租户+时间、任何自定义规则来分库分表。  
在配置数据库时，调用`UseSharding`方法，实现分库分表规则的配置

使用`UseDatabase`方法实现分库规则配置，使用`UseTable`方法实现分表规则配置
下面的示例，就是使用租户来做分库，根据当前登录用户的租户ID，生成dbKey

```csharp
var services = new ServiceCollection();
services.AddSingleton(f =>
{
    var builder = new OrmDbFactoryBuilder()
        .Register(OrmProviderType.MySql, "fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", true)
        .Register(OrmProviderType.MySql, "fengling_tenant1", "Server=localhost;Database=fengling_tenant1;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", false)
        .Register(OrmProviderType.MySql, "fengling_tenant2", "Server=localhost;Database=fengling_tenant2;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", false)
        .UseSharding(s =>
        {
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
                    var current = beginTime.AddDays(1 - beginTime.Day);
                    while (current <= endTime)
                    {
                        var tableName = $"{origName}_{tenantId}_{current:yyyyMM}";
                        if (tableNames.Contains(tableName))
                        {
                            current = current.AddMonths(1);
                            continue;
                        }
                        tableNames.Add(tableName);
                        current = current.AddMonths(1);
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
                    var current = beginTime.AddDays(1 - beginTime.Day);
                    while (current <= endTime)
                    {
                        var tableName = $"{origName}_{tenantId}_{current:yyyyMM}";
                        if (tableNames.Contains(tableName))
                        {
                            current = current.AddMonths(1);
                            continue;
                        }
                        tableNames.Add(tableName);
                        current = current.AddMonths(1);
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
        .Configure<ModelConfiguration>(OrmProviderType.MySql);
    return builder.Build();
});
services.AddTransient<IPassport>(f => new Passport { TenantId = "104", UserId = "1" });
var serviceProvider = services.BuildServiceProvider();
this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
```

配置了分库规则后，在执行查询时，会优先根据分库规则获取dbKey，确定连接串。

### 分表支持
`Trolley`的分表功能非常强大，支持`Join`操作，多分表Join操作，子查询分表，子查询多分表Join等操作。  
提供以下几个方法实现分表选择，大多数操作都支持分表操作，不同的操作会稍有些不同，分表后，可以继续`Join`操作，如果有2个以上多分表的情况，第二个以后的分表需要指定分表名映射
```csharp
UseTable(params string[] tableNames);
UseTable(Func<string, bool> tableNamePredicate);
UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
UseTableBy(object field1Value, object field2Value = null);
UseTableByRange(object beginFieldValue, object endFieldValue);
UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
```

#### UseTable方法
下面两个方法，在一个查询中，只能出现一次，执行后将确定多个分表，至少1个分表
```csharp
UseTable(params string[] tableNames);
UseTable(Func<string, bool> tableNamePredicate);
```
示例：
```csharp
await repository.Delete<User>()
    .UseTableBy("104")
    .Where(101)
    .ExecuteAsync();
//DELETE FROM `sys_user_104` WHERE `Id`=@Id
repository.Create<User>()
    .UseTable("sys_user_104")
    .WithBy(new
    {
        Id = 101,
        TenantId = "104",
        Name = "leafkevin",
        Age = 25,
        CompanyId = 1,
        Gender = Gender.Male,
        GuidField = Guid.NewGuid(),
        SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
        SourceType = UserSourceType.Douyin,
        IsEnabled = true,
        CreatedAt = DateTime.Parse("2024-05-10 06:07:08"),
        CreatedBy = 1,
        UpdatedAt = DateTime.Parse("2024-05-15 16:27:38"),
        UpdatedBy = 1
    })
    .Execute();
//INSERT INTO `sys_user_104` (`Id`,`TenantId`,`Name`,`Age`,`CompanyId`,`Gender`,`GuidField`,`SomeTimes`,`SourceType`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES (@Id,@TenantId,@Name,@Age,@CompanyId,@Gender,@GuidField,@SomeTimes,@SourceType,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)
var result = repository.From<User>()
    .UseTableBy("104")
    .Where(f => f.Id == 101)
    .First();
//SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user_104` a WHERE a.`Id`=101
```
多分表情况
`UseTable(Func<string, bool> tableNamePredicate) `

```csharp
await repository.Delete<User>()
    .UseTable(f => f.Contains("104") || f.Contains("105"))
    .Where(userIds)
    .ExecuteAsync();
DELETE FROM `sys_user_105` WHERE `Id` IN (@Id0,@Id1,@Id2);DELETE FROM `sys_user_104` WHERE `Id` IN (@Id0,@Id1,@Id2)

var result = repository.From<Order>()
    .UseTable("sys_order_104_202405", "sys_order_105_202405")
    .Where(f => f.ProductCount > productCount)
    .ToList();
//SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_105_202405` a WHERE a.`ProductCount`>@p0
```
#### 多个多分表关联Join查询
当已存在一个或多个多分表的情况下，再Join一个与前面多分表的表有关联关系的表时，并且当前这个分表也是多分表，就需要此方法来指定与前面分表的表名映射关系。
```csharp
UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
```
下面的子查询中使用了多分表，还有`Join`操作  
子查询中`OrderDetail`表，是个多份表，`Order`表也是多分表，使用了上面的方法与`OrderDetail`表做了表名映射来捞取与`OrderDetail`关联的分表
最外层查询中，`User`表也是多分表，又与主分表做了表名映射，`InnserJoin`关联起来
```csharp
var result = repository
    .From(f => f.From<OrderDetail>()
        .UseTable("sys_order_detail_104_202405", "sys_order_detail_105_202405")
        .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
        .UseTable<OrderDetail>((dbKey, orderOrigName, userOrigName, orderTableName) => orderTableName.Replace(orderOrigName, userOrigName))
        .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
        .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
    .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
    .UseTable<OrderDetail>((dbKey, orderOrigName, userOrigName, orderTableName) =>
    {
        var tableName = orderTableName.Replace(orderOrigName, userOrigName);
        return tableName.Substring(0, tableName.Length - 7);
    })
    .Where((a, b) => a.ProductCount > 1)
    .Select((x, y) => new
    {
        x.Group,
        y.TenantId,
        Buyer = y,
        x.ProductCount
    })
    .ToList();
//SELECT a.`OrderId`,a.`BuyerId`,b.`TenantId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail_104_202405` a INNER JOIN `sys_order_104_202405` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1 UNION ALL SELECT a.`OrderId`,a.`BuyerId`,b.`TenantId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail_105_202405` a INNER JOIN `sys_order_105_202405` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1
```
又一个例子：

```csharp
var result = repository.From<Order>()
    .UseTable("sys_order_104_202405", "sys_order_105_202405")
    .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
    .UseTable<Order>((dbKey, orderOrigName, userOrigName, orderTableName) =>
    {
        var tableName = orderTableName.Replace(orderOrigName, userOrigName);
        return tableName.Substring(0, tableName.Length - 7);
    })
    .Where((a, b) => a.ProductCount > productCount)
    .Select((x, y) => new
    {
        Order = x,
        Buyer = y
    })
    .ToList();
//SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0
```

表达式条件筛选的例子：
```csharp
var result = repository.From<Order>()
    .UseTable(f => (f.Contains("_104_") || f.Contains("_105_")) && int.Parse(f.Substring(f.Length - 6)) > 202001)
    .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
    .UseTable<Order>((dbKey, orderOrigName, orderDetailOrigName, orderTableName) => orderTableName.Replace(orderOrigName, orderDetailOrigName))
    .Where((a, b) => a.ProductCount > productCount)
    .Select((x, y) => new
    {
        Order = x,
        Detail = y
    })
    .ToList();
//SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderId`,b.`ProductId`,b.`Price`,b.`Quantity`,b.`Amount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_order_detail_104_202405` b ON a.`Id`=b.`OrderId` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderId`,b.`ProductId`,b.`Price`,b.`Quantity`,b.`Amount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_order_detail_105_202405` b ON a.`Id`=b.`OrderId` WHERE a.`ProductCount`>@p0
```


#### UseTableBy方法
根据分表配置的字段值，捞取数据库中所有当前表的分表明，再进行匹配分表，再执行后面操作，本方法可以多次调用

#### 租户Id分表
用户表，是根据租户ID来进行分表的，UseTableBy方法的参数就是租户ID，匹配到2个分表
```csharp
var repository = this.dbFactory.Create();
await repository.Delete<User>()
    .UseTableBy("104")
    .UseTableBy("105")
    .Where(new[] { 101, 102, 103 })
    .ExecuteAsync();
//DELETE FROM `sys_user_104` WHERE `Id` IN (@Id0,@Id1,@Id2);DELETE FROM `sys_user_105` WHERE `Id` IN (@Id0,@Id1,@Id2)
```


```csharp
var result = await repository.From<User>()
    .UseTableBy("104")
    .Where(f => f.Id == 101)
    .FirstAsync();
//SELECT a.`Id`,a.`TenantId`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`GuidField`,a.`SomeTimes`,a.`SourceType`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_user_104` a WHERE a.`Id`=101
```
#### 租户Id+日期分表
订单表，是根据租户ID+日期yyyyMM分表的，捞取到1个分表，并关联用户表，用户表是租户ID分表
```csharp
var sql = repository.From<Order>()
    .UseTableBy("104", DateTime.Parse("2024-05-01"))
    .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
    .UseTableBy("104")
    .Where((x, y) => x.Id == orderId)
    .Select((x, y) => new { x.Id, x.OrderNo, x.TenantId, x.BuyerId, BuyerName = y.Name })
    .ToSql(out _);
//SELECT a.`Id`,a.`OrderNo`,a.`TenantId`,a.`BuyerId`,b.`Name` AS `BuyerName` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`Id`=@p0
```

#### UseTableByRange 指定范围区间筛选分表
`Trolley`根据分表配置字段的顺序，进行筛选范围区间，通常是时间规则分表，如果指定了1个时间字段，就可以使用第一个方法，如果指定了2个字段，比如：租户ID+时间分表，就可以使用第二个方法来筛选，字段值的顺序与分表规则配置的字段顺序相同
```csharp
UseTableByRange(object beginFieldValue, object endFieldValue);
UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
```

订单表，是按照租户ID+日期yyyyMM来进行分表的，第1个字段是租户ID，第2个字段是时间，如下：
```csharp
var orders = repository.From<Order>()
    .UseTableByRange("104", beginTime, endTime)
    .Select(f => new
    {
        f.Id,
        f.TenantId,
        f.OrderNo,
        f.TotalAmount
    })
    .OrderByDescending(f => f.Id)
    .ToList();
生成的SQL:
SELECT * FROM (SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`TotalAmount` FROM `sys_order_104_202305` a ORDER BY a.`Id` DESC) a UNION ALL SELECT * FROM (SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`TotalAmount` FROM `sys_order_104_202205` a ORDER BY a.`Id` DESC) a UNION ALL SELECT * FROM (SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`TotalAmount` FROM `sys_order_104_202105` a ORDER BY a.`Id` DESC) a UNION ALL SELECT * FROM (SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`TotalAmount` FROM `sys_order_104_202005` a ORDER BY a.`Id` DESC) a UNION ALL SELECT * FROM (SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`TotalAmount` FROM `sys_order_105_202405` a ORDER BY a.`Id` DESC) a UNION ALL SELECT * FROM (SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`TotalAmount` FROM `sys_order_104_202405` a ORDER BY a.`Id` DESC) a
```

#### UseTableByRange 指定范围区间筛选分表 再Join关联表
```csharp
var orderInfos = repository.From<Order>()
    .UseTableByRange("104", beginTime, endTime)
    .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
    .UseTable<Order>((dbKey, orderOrigName, userOrigName, orderTableName) =>
    {
        var tableName = orderTableName.Replace(orderOrigName, userOrigName);
        return tableName.Substring(0, tableName.Length - 7);
    })
    .Select((x, y) => new
    {
        x.Id,
        x.TenantId,
        BuyerName = y.Name,
        x.TotalAmount
    })
    .OrderByDescending(f => f.Id)
    .ToList();
生成的SQL:
SELECT * FROM (SELECT a.`Id`,a.`TenantId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order_104_202005` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ORDER BY a.`Id` DESC) a UNION ALL SELECT * FROM (SELECT a.`Id`,a.`TenantId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order_104_202105` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ORDER BY a.`Id` DESC) a UNION ALL SELECT * FROM (SELECT a.`Id`,a.`TenantId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order_104_202205` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ORDER BY a.`Id` DESC) a UNION ALL SELECT * FROM (SELECT a.`Id`,a.`TenantId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order_104_202305` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ORDER BY a.`Id` DESC) a UNION ALL SELECT * FROM (SELECT a.`Id`,a.`TenantId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ORDER BY a.`Id` DESC) a
```


#### TableSchema支持
有些数据库，是有`TableSchema`概念的，如：`PostgreSql`和`SqlServer`数据库，这些数据库有默认的`Schema`，如：`PostgreSql`的默认`TableSchema`是`public`,`SqlServer`数据库的默认`TableSchema`是`dbo`，`MySql`数据库没有默认`TableSchema`，也可以认为默认`TableSchema`就是数据库名称。
`Trolley`支持非默认`TableSchema`的场景，会在表名前面增加`TableSchema`名称，如果指定了默认的`TableSchema`，`Trolley`将丢弃`TableSchema`名称。
子查询、分表、Include导航属性都支持`TableSchema`  

```csharp
var result = repository
    .From(f => f.From<OrderDetail>()
        .UseTableSchema("myschema")
        .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
        .UseTableSchema("myschema")
        .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
        .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
    .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
    .UseTableSchema("myschema")
    .Where((a, b) => a.ProductCount > 1)
    .Select((x, y) => new
    {
        x.Group,
        Buyer = y,
        x.ProductCount
    })
    .ToList();
//SELECT a."OrderId",a."BuyerId",b."Id",b."TenantId",b."Name",b."Gender",b."Age",b."CompanyId",b."GuidField",b."SomeTimes",b."SourceType",b."IsEnabled",b."CreatedAt",b."CreatedBy",b."UpdatedAt",b."UpdatedBy",a."ProductCount" FROM (SELECT b."Id" AS "OrderId",b."BuyerId",COUNT(DISTINCT a."ProductId") AS "ProductCount" FROM "myschema"."sys_order_detail" a INNER JOIN "myschema"."sys_order" b ON a."OrderId"=b."Id" GROUP BY b."Id",b."BuyerId") a INNER JOIN "myschema"."sys_user" b ON a."BuyerId"=b."Id" WHERE a."ProductCount">1
```

默认的`TableSchema`，`Trolley`将丢弃`TableSchema`名称
```csharp
var result = repository
    .From(f => f.From<OrderDetail>()
        .UseTableSchema("public")
        .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
        .UseTableSchema("public")
        .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
        .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
    .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
    .UseTableSchema("public")
    .Where((a, b) => a.ProductCount > 1)
    .Select((x, y) => new
    {
        x.Group,
        Buyer = y,
        x.ProductCount
    })
    .ToList();
//SELECT a."OrderId",a."BuyerId",b."Id",b."TenantId",b."Name",b."Gender",b."Age",b."CompanyId",b."GuidField",b."SomeTimes",b."SourceType",b."IsEnabled",b."CreatedAt",b."CreatedBy",b."UpdatedAt",b."UpdatedBy",a."ProductCount" FROM (SELECT b."Id" AS "OrderId",b."BuyerId",COUNT(DISTINCT a."ProductId") AS "ProductCount" FROM "sys_order_detail" a INNER JOIN "sys_order" b ON a."OrderId"=b."Id" GROUP BY b."Id",b."BuyerId") a INNER JOIN "sys_user" b ON a."BuyerId"=b."Id" WHERE a."ProductCount">1
```


#### 读写分离
在`Trolley`中，读写分离，只需要在配置从库字符串就可以了，默认所有的查询都会走从库，事务操作和增删改都会走主库，多个从库之间是使用轮询方式访问，这样负载更均衡一点。
```csharp
//主库的连接串
var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
var builder = new OrmDbFactoryBuilder()
    .Register(OrmProviderType.MySql, "fengling", connectionString, f =>
    {
        //两个读库连接串
        var connectionString1 = "Server=localhost;Database=fengling1;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
        var connectionString2 = "Server=localhost;Database=fengling2;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
        f.UseSlave(connectionString1, connectionString2)
        .AsDefaultDatabase();
    })
    .Configure<ModelConfiguration>(OrmProviderType.MySql)
var dbFactory = builder.Build();
```
通过上面的配置后，除被事务包围的查询外，所有的读操作都会走从库，多个从库之间是使用轮询方式访问。
可以通过拦截器，打印出来每个命令执行的连接串。



#### 拦截器
`Trolley`支持以下几个拦截器
```csharp
public Action<ConectionEventArgs> OnConnectionCreated { get; set; }
public Action<ConectionEventArgs> OnConnectionOpening { get; set; }
public Action<ConectionEventArgs> OnConnectionOpened { get; set; }
public Action<ConectionEventArgs> OnConnectionClosing { get; set; }
public Action<ConectionEventArgs> OnConnectionClosed { get; set; }
public Action<CommandEventArgs> OnCommandExecuting { get; set; }
public Action<CommandCompletedEventArgs> OnCommandExecuted { get; set; }
public Action<CommandCompletedEventArgs> OnCommandFailed { get; set; }
```
可以根据需要设置拦截处理程序，事件参数中的`ConnectionId`、`CommandId`是每次创建对象的的唯一ID，ConnectionId不一定是ADO.NET中真实的链接ID，可以跟踪到创建的每个链接、打开关闭状态



欢迎大家使用
---------------------
QQ：39253425
Mail：leafkevin@outlook.com
