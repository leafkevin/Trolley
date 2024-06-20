using System;
using System.Data;

namespace Trolley.Test.SqlServer;

class ModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(f =>
        {
            f.ToTable("sys_user").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(User.Id)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(1).Required();
            f.Member(t => t.TenantId).Field(nameof(User.TenantId)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(2).Length(50).Required();
            f.Member(t => t.Name).Field(nameof(User.Name)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(3).Length(50);
            f.Member(t => t.Gender).Field(nameof(User.Gender)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(4).Length(50);
            f.Member(t => t.Age).Field(nameof(User.Age)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(5);
            f.Member(t => t.CompanyId).Field(nameof(User.CompanyId)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(6);
            f.Member(t => t.GuidField).Field(nameof(User.GuidField)).DbColumnType("uniqueidentifier").NativeDbType(SqlDbType.UniqueIdentifier).Position(7);
            f.Member(t => t.SomeTimes).Field(nameof(User.SomeTimes)).DbColumnType("time").NativeDbType(SqlDbType.Time).Position(8);
            f.Member(t => t.SourceType).Field(nameof(User.SourceType)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(9).Length(50);
            f.Member(t => t.IsEnabled).Field(nameof(User.IsEnabled)).DbColumnType("bit").NativeDbType(SqlDbType.Bit).Position(10);
            f.Member(t => t.CreatedAt).Field(nameof(User.CreatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(11);
            f.Member(t => t.CreatedBy).Field(nameof(User.CreatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(12);
            f.Member(t => t.UpdatedAt).Field(nameof(User.UpdatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(13);
            f.Member(t => t.UpdatedBy).Field(nameof(User.UpdatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(14);

            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
            f.HasMany(t => t.Orders).HasForeignKey(t => t.BuyerId);
        });
        builder.Entity<Company>(f =>
        {
            f.ToTable("sys_company").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Company.Id)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(1).Required();
            f.Member(t => t.Name).Field(nameof(Company.Name)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(2).Length(50);
            f.Member(t => t.Nature).Field(nameof(Company.Nature)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(3).Length(50);
            f.Member(t => t.IsEnabled).Field(nameof(Company.IsEnabled)).DbColumnType("bit").NativeDbType(SqlDbType.Bit).Position(4);
            f.Member(t => t.CreatedAt).Field(nameof(Company.CreatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(5);
            f.Member(t => t.CreatedBy).Field(nameof(Company.CreatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(6);
            f.Member(t => t.UpdatedAt).Field(nameof(Company.UpdatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(7);
            f.Member(t => t.UpdatedBy).Field(nameof(Company.UpdatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(8);

            f.HasMany(t => t.Users).HasForeignKey(t => t.CompanyId);
            f.HasMany(t => t.Brands).HasForeignKey(t => t.CompanyId);
            f.HasMany(t => t.Products).HasForeignKey(t => t.CompanyId);
        });
        builder.Entity<Order>(f =>
        {
            f.ToTable("sys_order").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Order.Id)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(1).Length(50).Required();
            f.Member(t => t.TenantId).Field(nameof(Order.TenantId)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(2).Length(50).Required();
            f.Member(t => t.OrderNo).Field(nameof(Order.OrderNo)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(3).Length(50);
            f.Member(t => t.ProductCount).Field(nameof(Order.ProductCount)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(4);
            f.Member(t => t.TotalAmount).Field(nameof(Order.TotalAmount)).DbColumnType("float").NativeDbType(SqlDbType.Float).Position(5);
            f.Member(t => t.BuyerId).Field(nameof(Order.BuyerId)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(6);
            f.Member(t => t.BuyerSource).Field(nameof(Order.BuyerSource)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(7).Length(50);
            f.Member(t => t.SellerId).Field(nameof(Order.SellerId)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(8);
            //特殊类型JSON
            f.Member(t => t.Products).Field(nameof(Order.Products)).DbColumnType("ntext").NativeDbType(SqlDbType.NText).Position(9).TypeHandler<JsonTypeHandler>();
            f.Member(t => t.Disputes).Field(nameof(Order.Disputes)).DbColumnType("ntext").NativeDbType(SqlDbType.NText).Position(10).TypeHandler<JsonTypeHandler>();
            f.Member(t => t.IsEnabled).Field(nameof(Order.IsEnabled)).DbColumnType("bit").NativeDbType(SqlDbType.Bit).Position(11);
            f.Member(t => t.CreatedAt).Field(nameof(Order.CreatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(12);
            f.Member(t => t.CreatedBy).Field(nameof(Order.CreatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(13);
            f.Member(t => t.UpdatedAt).Field(nameof(Order.UpdatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(14);
            f.Member(t => t.UpdatedBy).Field(nameof(Order.UpdatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(15);

            f.HasOne(t => t.Buyer).HasForeignKey(t => t.BuyerId);
            f.HasOne(t => t.Seller).HasForeignKey(t => t.SellerId).MapTo<User>();
            f.HasMany(t => t.Details).HasForeignKey(t => t.OrderId);
        });
        builder.Entity<OrderDetail>(f =>
        {
            f.ToTable("sys_order_detail").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(OrderDetail.Id)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(1).Length(50).Required();
            f.Member(t => t.TenantId).Field(nameof(OrderDetail.TenantId)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(2).Length(50).Required();
            f.Member(t => t.OrderId).Field(nameof(OrderDetail.OrderId)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(3).Length(50);
            f.Member(t => t.ProductId).Field(nameof(OrderDetail.ProductId)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(4);
            f.Member(t => t.Price).Field(nameof(OrderDetail.Price)).DbColumnType("float").NativeDbType(SqlDbType.Float).Position(5);
            f.Member(t => t.Quantity).Field(nameof(OrderDetail.Quantity)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(6);
            f.Member(t => t.Amount).Field(nameof(OrderDetail.Amount)).DbColumnType("float").NativeDbType(SqlDbType.Float).Position(7);
            f.Member(t => t.IsEnabled).Field(nameof(OrderDetail.IsEnabled)).DbColumnType("bit").NativeDbType(SqlDbType.Bit).Position(8);
            f.Member(t => t.CreatedAt).Field(nameof(OrderDetail.CreatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(9);
            f.Member(t => t.CreatedBy).Field(nameof(OrderDetail.CreatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(10);
            f.Member(t => t.UpdatedAt).Field(nameof(OrderDetail.UpdatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(11);
            f.Member(t => t.UpdatedBy).Field(nameof(OrderDetail.UpdatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(12);

            f.HasOne(t => t.Order).HasForeignKey(t => t.OrderId);
            f.HasOne(t => t.Product).HasForeignKey(t => t.ProductId);
        });
        builder.Entity<Product>(f =>
        {
            f.ToTable("sys_product").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Product.Id)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(1).Required();
            f.Member(t => t.ProductNo).Field(nameof(Product.ProductNo)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(2).Length(50);
            f.Member(t => t.Name).Field(nameof(Product.Name)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(3).Length(50);
            f.Member(t => t.BrandId).Field(nameof(Product.BrandId)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(4);
            f.Member(t => t.CategoryId).Field(nameof(Product.CategoryId)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(5);
            f.Member(t => t.Price).Field(nameof(Product.Price)).DbColumnType("float").NativeDbType(SqlDbType.Float).Position(6);
            f.Member(t => t.CompanyId).Field(nameof(Product.CompanyId)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(7);
            f.Member(t => t.IsEnabled).Field(nameof(Product.IsEnabled)).DbColumnType("bit").NativeDbType(SqlDbType.Bit).Position(8);
            f.Member(t => t.CreatedAt).Field(nameof(Product.CreatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(9);
            f.Member(t => t.CreatedBy).Field(nameof(Product.CreatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(10);
            f.Member(t => t.UpdatedAt).Field(nameof(Product.UpdatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(11);
            f.Member(t => t.UpdatedBy).Field(nameof(Product.UpdatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(12);

            f.HasOne(t => t.Brand).HasForeignKey(t => t.BrandId).MapTo<Brand>();
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
        });
        builder.Entity<Brand>(f =>
        {
            f.ToTable("sys_brand").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Brand.Id)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(1).Required();
            f.Member(t => t.BrandNo).Field(nameof(Brand.BrandNo)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(2).Length(50);
            f.Member(t => t.Name).Field(nameof(Brand.Name)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(3).Length(50);
            f.Member(t => t.CompanyId).Field(nameof(Brand.CompanyId)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(4);
            f.Member(t => t.IsEnabled).Field(nameof(Brand.IsEnabled)).DbColumnType("bit").NativeDbType(SqlDbType.Bit).Position(5);
            f.Member(t => t.CreatedAt).Field(nameof(Brand.CreatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(6);
            f.Member(t => t.CreatedBy).Field(nameof(Brand.CreatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(7);
            f.Member(t => t.UpdatedAt).Field(nameof(Brand.UpdatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(8);
            f.Member(t => t.UpdatedBy).Field(nameof(Brand.UpdatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(9);

            f.HasMany(t => t.Products).HasForeignKey(t => t.BrandId);
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
        });
        builder.Entity<Menu>(f =>
        {
            f.ToTable("sys_menu").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Menu.Id)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(1).Required();
            f.Member(t => t.Name).Field(nameof(Menu.Name)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(2).Length(50);
            f.Member(t => t.ParentId).Field(nameof(Menu.ParentId)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(3);
            f.Member(t => t.PageId).Field(nameof(Menu.PageId)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(4);
            f.Member(t => t.IsEnabled).Field(nameof(Menu.IsEnabled)).DbColumnType("bit").NativeDbType(SqlDbType.Bit).Position(5);
            f.Member(t => t.CreatedAt).Field(nameof(Menu.CreatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(6);
            f.Member(t => t.CreatedBy).Field(nameof(Menu.CreatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(7);
            f.Member(t => t.UpdatedAt).Field(nameof(Menu.UpdatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(8);
            f.Member(t => t.UpdatedBy).Field(nameof(Menu.UpdatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(9);
        });
        builder.Entity<Page>(f =>
        {
            f.ToTable("sys_page").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Page.Id)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(1).Required();
            f.Member(t => t.Url).Field(nameof(Page.Url)).DbColumnType("nvarchar(200)").NativeDbType(SqlDbType.NVarChar).Position(2).Length(200);
            f.Member(t => t.IsEnabled).Field(nameof(Page.IsEnabled)).DbColumnType("bit").NativeDbType(SqlDbType.Bit).Position(3);
            f.Member(t => t.CreatedAt).Field(nameof(Page.CreatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(4);
            f.Member(t => t.CreatedBy).Field(nameof(Page.CreatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(5);
            f.Member(t => t.UpdatedAt).Field(nameof(Page.UpdatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(6);
            f.Member(t => t.UpdatedBy).Field(nameof(Page.UpdatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(7);
        });
        builder.Entity<Function>(f =>
        {
            f.ToTable("sys_function").Key(t => t.MenuId);
            f.Member(t => t.MenuId).Field(nameof(Function.MenuId)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(1).Required();
            f.Member(t => t.PageId).Field(nameof(Function.PageId)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(2).Required();
            f.Member(t => t.FunctionName).Field(nameof(Function.FunctionName)).DbColumnType("nvarchar(50)").NativeDbType(SqlDbType.NVarChar).Position(3).Length(50);
            f.Member(t => t.Description).Field(nameof(Function.Description)).DbColumnType("nvarchar(500)").NativeDbType(SqlDbType.NVarChar).Position(4).Length(500);
            f.Member(t => t.IsEnabled).Field(nameof(Function.IsEnabled)).DbColumnType("bit").NativeDbType(SqlDbType.Bit).Position(5);
            f.Member(t => t.CreatedAt).Field(nameof(Function.CreatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(6);
            f.Member(t => t.CreatedBy).Field(nameof(Function.CreatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(7);
            f.Member(t => t.UpdatedAt).Field(nameof(Function.UpdatedAt)).DbColumnType("datetime").NativeDbType(SqlDbType.DateTime).Position(8);
            f.Member(t => t.UpdatedBy).Field(nameof(Function.UpdatedBy)).DbColumnType("int").NativeDbType(SqlDbType.Int).Position(9);
        });
    }
}
