using System.Data;

namespace Trolley.Test.SqlServer;

class ModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(f =>
        {
            f.ToTable("sys_user").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(User.Id)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.Name).Field(nameof(User.Name)).NativeDbType(SqlDbType.NVarChar);
            f.Member(t => t.Gender).Field(nameof(User.Gender)).NativeDbType(SqlDbType.TinyInt);
            f.Member(t => t.Age).Field(nameof(User.Age)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.CompanyId).Field(nameof(User.CompanyId)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.GuidField).Field(nameof(User.GuidField)).NativeDbType(SqlDbType.UniqueIdentifier);
            f.Member(t => t.SomeTimes).Field(nameof(User.SomeTimes)).NativeDbType(SqlDbType.Time);
            f.Member(t => t.SourceType).Field(nameof(User.SourceType)).NativeDbType(SqlDbType.NVarChar);
            f.Member(t => t.IsEnabled).Field(nameof(User.IsEnabled)).NativeDbType(SqlDbType.Bit);
            f.Member(t => t.CreatedAt).Field(nameof(User.CreatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.CreatedBy).Field(nameof(User.CreatedBy)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.UpdatedAt).Field(nameof(User.UpdatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.UpdatedBy).Field(nameof(User.UpdatedBy)).NativeDbType(SqlDbType.Int);

            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
            f.HasMany(t => t.Orders).HasForeignKey(t => t.BuyerId);
        });
        builder.Entity<Company>(f =>
        {
            f.ToTable("sys_company").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Company.Id)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.Name).Field(nameof(Company.Name)).NativeDbType(SqlDbType.NVarChar);
            f.Member(t => t.Nature).Field(nameof(Company.Nature)).NativeDbType(SqlDbType.NVarChar);
            f.Member(t => t.IsEnabled).Field(nameof(Company.IsEnabled)).NativeDbType(SqlDbType.Bit);
            f.Member(t => t.CreatedAt).Field(nameof(Company.CreatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.CreatedBy).Field(nameof(Company.CreatedBy)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.UpdatedAt).Field(nameof(Company.UpdatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.UpdatedBy).Field(nameof(Company.UpdatedBy)).NativeDbType(SqlDbType.Int);

            f.HasMany(t => t.Users).HasForeignKey(t => t.CompanyId);
            f.HasMany(t => t.Brands).HasForeignKey(t => t.CompanyId);
            f.HasMany(t => t.Products).HasForeignKey(t => t.CompanyId);
        });
        builder.Entity<Order>(f =>
        {
            f.ToTable("sys_order").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Order.Id)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.OrderNo).Field(nameof(Order.OrderNo)).NativeDbType(SqlDbType.NVarChar);
            f.Member(t => t.ProductCount).Field(nameof(Order.ProductCount)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.TotalAmount).Field(nameof(Order.TotalAmount)).NativeDbType(SqlDbType.Float);
            f.Member(t => t.BuyerId).Field(nameof(Order.BuyerId)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.BuyerSource).Field(nameof(Order.BuyerSource)).NativeDbType(SqlDbType.NVarChar);
            f.Member(t => t.SellerId).Field(nameof(Order.SellerId)).NativeDbType(SqlDbType.Int);
            //特殊类型JSON
            f.Member(t => t.Products).Field(nameof(Order.Products)).NativeDbType(SqlDbType.NText).TypeHandler<JsonTypeHandler>();
            f.Member(t => t.Disputes).Field(nameof(Order.Disputes)).NativeDbType(SqlDbType.NText).TypeHandler<JsonTypeHandler>();
            f.Member(t => t.IsEnabled).Field(nameof(Order.IsEnabled)).NativeDbType(SqlDbType.Bit);
            f.Member(t => t.CreatedAt).Field(nameof(Order.CreatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.CreatedBy).Field(nameof(Order.CreatedBy)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.UpdatedAt).Field(nameof(Order.UpdatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.UpdatedBy).Field(nameof(Order.UpdatedBy)).NativeDbType(SqlDbType.Int);

            f.HasOne(t => t.Buyer).HasForeignKey(t => t.BuyerId);
            f.HasOne(t => t.Seller).HasForeignKey(t => t.SellerId).MapTo<User>();
            f.HasMany(t => t.Details).HasForeignKey(t => t.OrderId);
        });
        builder.Entity<OrderDetail>(f =>
        {
            f.ToTable("sys_order_detail").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(OrderDetail.Id)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.OrderId).Field(nameof(OrderDetail.OrderId)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.ProductId).Field(nameof(OrderDetail.ProductId)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.Price).Field(nameof(OrderDetail.Price)).NativeDbType(SqlDbType.Float);
            f.Member(t => t.Quantity).Field(nameof(OrderDetail.Quantity)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.Amount).Field(nameof(OrderDetail.Amount)).NativeDbType(SqlDbType.Float);
            f.Member(t => t.IsEnabled).Field(nameof(OrderDetail.IsEnabled)).NativeDbType(SqlDbType.Bit);
            f.Member(t => t.CreatedAt).Field(nameof(OrderDetail.CreatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.CreatedBy).Field(nameof(OrderDetail.CreatedBy)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.UpdatedAt).Field(nameof(OrderDetail.UpdatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.UpdatedBy).Field(nameof(OrderDetail.UpdatedBy)).NativeDbType(SqlDbType.Int);

            f.HasOne(t => t.Order).HasForeignKey(t => t.OrderId);
            f.HasOne(t => t.Product).HasForeignKey(t => t.ProductId);
        });
        builder.Entity<Product>(f =>
        {
            f.ToTable("sys_product").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Product.Id)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.ProductNo).Field(nameof(Product.ProductNo)).NativeDbType(SqlDbType.NVarChar);
            f.Member(t => t.Name).Field(nameof(Product.Name)).NativeDbType(SqlDbType.NVarChar);
            f.Member(t => t.BrandId).Field(nameof(Product.BrandId)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.CategoryId).Field(nameof(Product.CategoryId)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.Price).Field(nameof(Product.Price)).NativeDbType(SqlDbType.Float);
            f.Member(t => t.CompanyId).Field(nameof(Product.CompanyId)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.IsEnabled).Field(nameof(Product.IsEnabled)).NativeDbType(SqlDbType.Bit);
            f.Member(t => t.CreatedAt).Field(nameof(Product.CreatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.CreatedBy).Field(nameof(Product.CreatedBy)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.UpdatedAt).Field(nameof(Product.UpdatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.UpdatedBy).Field(nameof(Product.UpdatedBy)).NativeDbType(SqlDbType.Int);

            f.HasOne(t => t.Brand).HasForeignKey(t => t.BrandId).MapTo<Brand>();
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
        });
        builder.Entity<Brand>(f =>
        {
            f.ToTable("sys_brand").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Brand.Id)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.BrandNo).Field(nameof(Brand.BrandNo)).NativeDbType(SqlDbType.NVarChar);
            f.Member(t => t.Name).Field(nameof(Brand.Name)).NativeDbType(SqlDbType.NVarChar);
            f.Member(t => t.CompanyId).Field(nameof(Brand.CompanyId)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.IsEnabled).Field(nameof(Brand.IsEnabled)).NativeDbType(SqlDbType.Bit);
            f.Member(t => t.CreatedAt).Field(nameof(Brand.CreatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.CreatedBy).Field(nameof(Brand.CreatedBy)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.UpdatedAt).Field(nameof(Brand.UpdatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.UpdatedBy).Field(nameof(Brand.UpdatedBy)).NativeDbType(SqlDbType.Int);

            f.HasMany(t => t.Products).HasForeignKey(t => t.BrandId);
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
        });
        builder.Entity<Menu>(f =>
        {
            f.ToTable("sys_menu").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Menu.Id)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.Name).Field(nameof(Menu.Name)).NativeDbType(SqlDbType.NVarChar);
            f.Member(t => t.ParentId).Field(nameof(Menu.ParentId)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.PageId).Field(nameof(Menu.PageId)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.IsEnabled).Field(nameof(Menu.IsEnabled)).NativeDbType(SqlDbType.Bit);
            f.Member(t => t.CreatedAt).Field(nameof(Menu.CreatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.CreatedBy).Field(nameof(Menu.CreatedBy)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.UpdatedAt).Field(nameof(Menu.UpdatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.UpdatedBy).Field(nameof(Menu.UpdatedBy)).NativeDbType(SqlDbType.Int);
        });
        builder.Entity<Page>(f =>
        {
            f.ToTable("sys_page").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Page.Id)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.Url).Field(nameof(Page.Url)).NativeDbType(SqlDbType.NVarChar);
            f.Member(t => t.IsEnabled).Field(nameof(Page.IsEnabled)).NativeDbType(SqlDbType.Bit);
            f.Member(t => t.CreatedAt).Field(nameof(Page.CreatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.CreatedBy).Field(nameof(Page.CreatedBy)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.UpdatedAt).Field(nameof(Page.UpdatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.UpdatedBy).Field(nameof(Page.UpdatedBy)).NativeDbType(SqlDbType.Int);
        });
        builder.Entity<Function>(f =>
        {
            f.ToTable("sys_function").Key(t => new { t.MenuId, t.PageId });
            f.Member(t => t.MenuId).Field(nameof(Function.MenuId)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.PageId).Field(nameof(Function.PageId)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.FunctionName).Field(nameof(Function.FunctionName)).NativeDbType(SqlDbType.VarChar);
            f.Member(t => t.Description).Field(nameof(Function.Description)).NativeDbType(SqlDbType.VarChar);
            f.Member(t => t.IsEnabled).Field(nameof(Function.IsEnabled)).NativeDbType(SqlDbType.Bit);
            f.Member(t => t.CreatedAt).Field(nameof(Function.CreatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.CreatedBy).Field(nameof(Function.CreatedBy)).NativeDbType(SqlDbType.Int);
            f.Member(t => t.UpdatedAt).Field(nameof(Function.UpdatedAt)).NativeDbType(SqlDbType.DateTime);
            f.Member(t => t.UpdatedBy).Field(nameof(Function.UpdatedBy)).NativeDbType(SqlDbType.Int);
        });
    }
}
