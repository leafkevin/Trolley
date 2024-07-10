using Npgsql;
using NpgsqlTypes;
using System;
using System.Data;

namespace Trolley.Test.PostgreSql;

class ModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(f =>
        {
            f.ToTable("sys_user").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(User.Id)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(1).Required();
            f.Member(t => t.TenantId).Field(nameof(User.TenantId)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(2).Length(255);
            f.Member(t => t.Name).Field(nameof(User.Name)).DbColumnType("varchar(50").NativeDbType(NpgsqlDbType.Varchar).Position(3).Length(255);
            f.Member(t => t.Gender).Field(nameof(User.Gender)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(4);
            f.Member(t => t.Age).Field(nameof(User.Age)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(5).Required();
            f.Member(t => t.CompanyId).Field(nameof(User.CompanyId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(6);
            f.Member(t => t.GuidField).Field(nameof(User.GuidField)).DbColumnType("uuid").NativeDbType(NpgsqlDbType.Uuid).Position(7);
            f.Member(t => t.SomeTimes).Field(nameof(User.SomeTimes)).DbColumnType("time").NativeDbType(NpgsqlDbType.Time).Position(8);
            f.Member(t => t.SourceType).Field(nameof(User.SourceType)).DbColumnType("varchar(50").NativeDbType(NpgsqlDbType.Varchar).Position(9);
            f.Member(t => t.IsEnabled).Field(nameof(User.IsEnabled)).DbColumnType("bool").NativeDbType(NpgsqlDbType.Boolean).Position(10).Required();
            f.Member(t => t.CreatedAt).Field(nameof(User.CreatedAt)).DbColumnType("timestamp").NativeDbType(NpgsqlDbType.Timestamp).Position(11).Required();
            f.Member(t => t.CreatedBy).Field(nameof(User.CreatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(12).Required();
            f.Member(t => t.UpdatedAt).Field(nameof(User.UpdatedAt)).DbColumnType("timestamp").NativeDbType(NpgsqlDbType.Timestamp).Position(13).Required();
            f.Member(t => t.UpdatedBy).Field(nameof(User.UpdatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(14).Required(); f.Member(t => t.Id).Field(nameof(User.Id)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(1).Required();

            f.Member(t => t.Id).Field(nameof(User.Id)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(1).Required();
            f.Member(t => t.TenantId).Field(nameof(User.TenantId)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(2).Length(50);
            f.Member(t => t.Name).Field(nameof(User.Name)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(3).Length(50);
            f.Member(t => t.Gender).Field(nameof(User.Gender)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(4).Length(50);
            f.Member(t => t.Age).Field(nameof(User.Age)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(5);
            f.Member(t => t.CompanyId).Field(nameof(User.CompanyId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(6);
            f.Member(t => t.SomeTimes).Field(nameof(User.SomeTimes)).DbColumnType("time").NativeDbType(NpgsqlDbType.Time).Position(7);
            f.Member(t => t.GuidField).Field(nameof(User.GuidField)).DbColumnType("uuid").NativeDbType(NpgsqlDbType.Uuid).Position(8);
            f.Member(t => t.SourceType).Field(nameof(User.SourceType)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(9).Length(50);
            f.Member(t => t.IsEnabled).Field(nameof(User.IsEnabled)).DbColumnType("bool").NativeDbType(NpgsqlDbType.Boolean).Position(10);
            f.Member(t => t.CreatedBy).Field(nameof(User.CreatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(11);
            f.Member(t => t.CreatedAt).Field(nameof(User.CreatedAt)).DbColumnType("timestamp").NativeDbType(NpgsqlDbType.Timestamp).Position(12);
            f.Member(t => t.UpdatedBy).Field(nameof(User.UpdatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(13);
            f.Member(t => t.UpdatedAt).Field(nameof(User.UpdatedAt)).DbColumnType("timestamp").NativeDbType(NpgsqlDbType.Timestamp).Position(14);



            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
            f.HasMany(t => t.Orders).HasForeignKey(t => t.BuyerId);
        });
        builder.Entity<Company>(f =>
        {
            f.ToTable("sys_company").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Company.Id)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(1).Required();
            f.Member(t => t.Name).Field(nameof(Company.Name)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(2).Length(50);
            f.Member(t => t.Nature).Field(nameof(Company.Nature)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(3).Length(50);
            f.Member(t => t.IsEnabled).Field(nameof(Company.IsEnabled)).DbColumnType("bool").NativeDbType(NpgsqlDbType.Boolean).Position(4);
            f.Member(t => t.CreatedAt).Field(nameof(Company.CreatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(5);
            f.Member(t => t.CreatedBy).Field(nameof(Company.CreatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(6);
            f.Member(t => t.UpdatedAt).Field(nameof(Company.UpdatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(7);
            f.Member(t => t.UpdatedBy).Field(nameof(Company.UpdatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(8);

            f.HasMany(t => t.Users).HasForeignKey(t => t.CompanyId);
            f.HasMany(t => t.Brands).HasForeignKey(t => t.CompanyId);
            f.HasMany(t => t.Products).HasForeignKey(t => t.CompanyId);
        });
        builder.Entity<Order>(f =>
        {
            f.ToTable("sys_order").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Order.Id)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(1).Length(50).Required();
            f.Member(t => t.TenantId).Field(nameof(Order.TenantId)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(2).Length(50).Required();
            f.Member(t => t.OrderNo).Field(nameof(Order.OrderNo)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(3).Length(50);
            f.Member(t => t.ProductCount).Field(nameof(Order.ProductCount)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(4);
            f.Member(t => t.TotalAmount).Field(nameof(Order.TotalAmount)).DbColumnType("double").NativeDbType(NpgsqlDbType.Double).Position(5);
            f.Member(t => t.BuyerId).Field(nameof(Order.BuyerId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(6);
            f.Member(t => t.BuyerSource).Field(nameof(Order.BuyerSource)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(7).Length(50);
            f.Member(t => t.SellerId).Field(nameof(Order.SellerId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(8);
            //特殊类型JSON
            f.Member(t => t.Products).Field(nameof(Order.Products)).DbColumnType("longtext").NativeDbType(NpgsqlDbType.Jsonb).Position(9).TypeHandler<JsonTypeHandler>();
            f.Member(t => t.Disputes).Field(nameof(Order.Disputes)).DbColumnType("longtext").NativeDbType(NpgsqlDbType.Jsonb).Position(10).TypeHandler<JsonTypeHandler>();
            f.Member(t => t.IsEnabled).Field(nameof(Order.IsEnabled)).DbColumnType("bool").NativeDbType(NpgsqlDbType.Boolean).Position(11);
            f.Member(t => t.CreatedAt).Field(nameof(Order.CreatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(12);
            f.Member(t => t.CreatedBy).Field(nameof(Order.CreatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(13);
            f.Member(t => t.UpdatedAt).Field(nameof(Order.UpdatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(14);
            f.Member(t => t.UpdatedBy).Field(nameof(Order.UpdatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(15);

            f.HasOne(t => t.Buyer).HasForeignKey(t => t.BuyerId);
            f.HasOne(t => t.Seller).HasForeignKey(t => t.SellerId).MapTo<User>();
            f.HasMany(t => t.Details).HasForeignKey(t => t.OrderId);
        });
        builder.Entity<OrderDetail>(f =>
        {
            f.ToTable("sys_order_detail").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(OrderDetail.Id)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(1).Length(50).Required();
            f.Member(t => t.TenantId).Field(nameof(OrderDetail.TenantId)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(2).Length(50).Required();
            f.Member(t => t.OrderId).Field(nameof(OrderDetail.OrderId)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(3).Length(50);
            f.Member(t => t.ProductId).Field(nameof(OrderDetail.ProductId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(4);
            f.Member(t => t.Price).Field(nameof(OrderDetail.Price)).DbColumnType("double(10,2)").NativeDbType(NpgsqlDbType.Double).Position(5);
            f.Member(t => t.Quantity).Field(nameof(OrderDetail.Quantity)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(6);
            f.Member(t => t.Amount).Field(nameof(OrderDetail.Amount)).DbColumnType("double(10,2)").NativeDbType(NpgsqlDbType.Double).Position(7);
            f.Member(t => t.IsEnabled).Field(nameof(OrderDetail.IsEnabled)).DbColumnType("bool").NativeDbType(NpgsqlDbType.Boolean).Position(8);
            f.Member(t => t.CreatedAt).Field(nameof(OrderDetail.CreatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(9);
            f.Member(t => t.CreatedBy).Field(nameof(OrderDetail.CreatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(10);
            f.Member(t => t.UpdatedAt).Field(nameof(OrderDetail.UpdatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(11);
            f.Member(t => t.UpdatedBy).Field(nameof(OrderDetail.UpdatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(12);

            f.HasOne(t => t.Order).HasForeignKey(t => t.OrderId);
            f.HasOne(t => t.Product).HasForeignKey(t => t.ProductId);
        });
        builder.Entity<Product>(f =>
        {
            f.ToTable("sys_product").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Product.Id)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(1).Required();
            f.Member(t => t.ProductNo).Field(nameof(Product.ProductNo)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(2).Length(50);
            f.Member(t => t.Name).Field(nameof(Product.Name)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(3).Length(50);
            f.Member(t => t.BrandId).Field(nameof(Product.BrandId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(4);
            f.Member(t => t.CategoryId).Field(nameof(Product.CategoryId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(5);
            f.Member(t => t.Price).Field(nameof(Product.Price)).DbColumnType("double").NativeDbType(NpgsqlDbType.Double).Position(6);
            f.Member(t => t.CompanyId).Field(nameof(Product.CompanyId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(7);
            f.Member(t => t.IsEnabled).Field(nameof(Product.IsEnabled)).DbColumnType("bool").NativeDbType(NpgsqlDbType.Boolean).Position(8);
            f.Member(t => t.CreatedAt).Field(nameof(Product.CreatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(9);
            f.Member(t => t.CreatedBy).Field(nameof(Product.CreatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(10);
            f.Member(t => t.UpdatedAt).Field(nameof(Product.UpdatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(11);
            f.Member(t => t.UpdatedBy).Field(nameof(Product.UpdatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(12);

            f.HasOne(t => t.Brand).HasForeignKey(t => t.BrandId).MapTo<Brand>();
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
        });
        builder.Entity<Brand>(f =>
        {
            f.ToTable("sys_brand").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Brand.Id)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(1).Required();
            f.Member(t => t.BrandNo).Field(nameof(Brand.BrandNo)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(2).Length(50);
            f.Member(t => t.Name).Field(nameof(Brand.Name)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(3).Length(50);
            f.Member(t => t.CompanyId).Field(nameof(Brand.CompanyId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(4);
            f.Member(t => t.IsEnabled).Field(nameof(Brand.IsEnabled)).DbColumnType("bool").NativeDbType(NpgsqlDbType.Boolean).Position(5);
            f.Member(t => t.CreatedAt).Field(nameof(Brand.CreatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(6);
            f.Member(t => t.CreatedBy).Field(nameof(Brand.CreatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(7);
            f.Member(t => t.UpdatedAt).Field(nameof(Brand.UpdatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(8);
            f.Member(t => t.UpdatedBy).Field(nameof(Brand.UpdatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(9);

            f.HasMany(t => t.Products).HasForeignKey(t => t.BrandId);
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
        });
        builder.Entity<Menu>(f =>
        {
            f.ToTable("sys_menu").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Menu.Id)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(1).Required();
            f.Member(t => t.Name).Field(nameof(Menu.Name)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(2).Length(50);
            f.Member(t => t.ParentId).Field(nameof(Menu.ParentId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(3);
            f.Member(t => t.PageId).Field(nameof(Menu.PageId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(4);
            f.Member(t => t.IsEnabled).Field(nameof(Menu.IsEnabled)).DbColumnType("bool").NativeDbType(NpgsqlDbType.Boolean).Position(5);
            f.Member(t => t.CreatedAt).Field(nameof(Menu.CreatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(6);
            f.Member(t => t.CreatedBy).Field(nameof(Menu.CreatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(7);
            f.Member(t => t.UpdatedAt).Field(nameof(Menu.UpdatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(8);
            f.Member(t => t.UpdatedBy).Field(nameof(Menu.UpdatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(9);
        });
        builder.Entity<Page>(f =>
        {
            f.ToTable("sys_page").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Page.Id)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(1).Required();
            f.Member(t => t.Url).Field(nameof(Page.Url)).DbColumnType("varchar(200)").NativeDbType(NpgsqlDbType.Varchar).Position(2).Length(200);
            f.Member(t => t.IsEnabled).Field(nameof(Page.IsEnabled)).DbColumnType("bool").NativeDbType(NpgsqlDbType.Boolean).Position(3);
            f.Member(t => t.CreatedAt).Field(nameof(Page.CreatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(4);
            f.Member(t => t.CreatedBy).Field(nameof(Page.CreatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(5);
            f.Member(t => t.UpdatedAt).Field(nameof(Page.UpdatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(6);
            f.Member(t => t.UpdatedBy).Field(nameof(Page.UpdatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(7);
        });
        builder.Entity<Function>(f =>
        {
            f.ToTable("sys_function").Key(t => new { t.MenuId, t.PageId });
            f.Member(t => t.MenuId).Field(nameof(Function.MenuId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(1).Required();
            f.Member(t => t.PageId).Field(nameof(Function.PageId)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(2).Required();
            f.Member(t => t.FunctionName).Field(nameof(Function.FunctionName)).DbColumnType("varchar(50)").NativeDbType(NpgsqlDbType.Varchar).Position(3).Length(50);
            f.Member(t => t.Description).Field(nameof(Function.Description)).DbColumnType("varchar(500)").NativeDbType(NpgsqlDbType.Varchar).Position(4).Length(500);
            f.Member(t => t.IsEnabled).Field(nameof(Function.IsEnabled)).DbColumnType("bool").NativeDbType(NpgsqlDbType.Boolean).Position(5);
            f.Member(t => t.CreatedAt).Field(nameof(Function.CreatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(6);
            f.Member(t => t.CreatedBy).Field(nameof(Function.CreatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(7);
            f.Member(t => t.UpdatedAt).Field(nameof(Function.UpdatedAt)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(8);
            f.Member(t => t.UpdatedBy).Field(nameof(Function.UpdatedBy)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(9);
        });
        builder.Entity<UpdateEntity>(f =>
        {
            f.ToTable("sys_update_entity").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(UpdateEntity.Id)).DbColumnType("int4").NativeDbType(NpgsqlDbType.Integer).Position(1).Required();
            f.Member(t => t.BooleanField).Field(nameof(UpdateEntity.BooleanField)).DbColumnType("bool").NativeDbType(NpgsqlDbType.Boolean).Position(2);
            f.Member(t => t.EnumField).Field(nameof(UpdateEntity.EnumField)).DbColumnType("tinyint(4)").NativeDbType(NpgsqlDbType.Smallint).Position(3);
            f.Member(t => t.GuidField).Field(nameof(UpdateEntity.GuidField)).DbColumnType("varchar(36)").NativeDbType(NpgsqlDbType.Varchar).Position(4).Length(36);
            f.Member(t => t.DateTimeField).Field(nameof(UpdateEntity.DateTimeField)).DbColumnType("datetime").NativeDbType(NpgsqlDbType.Timestamp).Position(5);
            f.Member(t => t.DateOnlyField).Field(nameof(UpdateEntity.DateOnlyField)).DbColumnType("date").NativeDbType(NpgsqlDbType.Date).Position(6);
            f.Member(t => t.DateTimeOffsetField).Field(nameof(UpdateEntity.DateTimeOffsetField)).DbColumnType("timestamp").NativeDbType(NpgsqlDbType.Timestamp).Position(7);
            f.Member(t => t.TimeSpanField).Field(nameof(UpdateEntity.TimeSpanField)).DbColumnType("time").NativeDbType(NpgsqlDbType.Time).Position(8);
            f.Member(t => t.TimeOnlyField).Field(nameof(UpdateEntity.TimeOnlyField)).DbColumnType("time").NativeDbType(NpgsqlDbType.Time).Position(9);
        });
    }
}
