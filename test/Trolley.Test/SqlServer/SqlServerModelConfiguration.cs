namespace Trolley.Test;

class SqlServerModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(f =>
        {
            f.ToTable("sys_user").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(User.Id)).NativeDbType(8);
            f.Member(t => t.Name).Field(nameof(User.Name)).NativeDbType(22);
            f.Member(t => t.Gender).Field(nameof(User.Gender)).NativeDbType(20);
            f.Member(t => t.Age).Field(nameof(User.Age)).NativeDbType(8);
            f.Member(t => t.CompanyId).Field(nameof(User.CompanyId)).NativeDbType(8);
            f.Member(t => t.IsEnabled).Field(nameof(User.IsEnabled)).NativeDbType(2);
            f.Member(t => t.CreatedAt).Field(nameof(User.CreatedAt)).NativeDbType(4);
            f.Member(t => t.CreatedBy).Field(nameof(User.CreatedBy)).NativeDbType(8);
            f.Member(t => t.UpdatedAt).Field(nameof(User.UpdatedAt)).NativeDbType(4);
            f.Member(t => t.UpdatedBy).Field(nameof(User.UpdatedBy)).NativeDbType(8);

            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
            f.HasMany(t => t.Orders).HasForeignKey(t => t.BuyerId);
        });
        builder.Entity<Company>(f =>
        {
            f.ToTable("sys_company").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Company.Id)).NativeDbType(8).AutoIncrement();
            f.Member(t => t.Name).Field(nameof(Company.Name)).NativeDbType(12);
            f.Member(t => t.IsEnabled).Field(nameof(Company.IsEnabled)).NativeDbType(2);
            f.Member(t => t.CreatedAt).Field(nameof(Company.CreatedAt)).NativeDbType(4);
            f.Member(t => t.CreatedBy).Field(nameof(Company.CreatedBy)).NativeDbType(8);
            f.Member(t => t.UpdatedAt).Field(nameof(Company.UpdatedAt)).NativeDbType(4);
            f.Member(t => t.UpdatedBy).Field(nameof(Company.UpdatedBy)).NativeDbType(8);

            f.HasMany(t => t.Users).HasForeignKey(t => t.CompanyId);
            f.HasMany(t => t.Brands).HasForeignKey(t => t.CompanyId);
            f.HasMany(t => t.Products).HasForeignKey(t => t.CompanyId);
        });
        builder.Entity<Order>(f =>
        {
            f.ToTable("sys_order").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Order.Id)).NativeDbType(8);
            f.Member(t => t.OrderNo).Field(nameof(Order.OrderNo)).NativeDbType(12);
            f.Member(t => t.ProductCount).Field(nameof(Order.ProductCount)).NativeDbType(8);
            f.Member(t => t.TotalAmount).Field(nameof(Order.TotalAmount)).NativeDbType(6);
            f.Member(t => t.BuyerId).Field(nameof(Order.BuyerId)).NativeDbType(8);
            f.Member(t => t.SellerId).Field(nameof(Order.SellerId)).NativeDbType(8);
            //特殊类型JSON
            f.Member(t => t.Products).Field(nameof(Order.Products)).NativeDbType(12).SetTypeHandler<JsonTypeHandler>();
            f.Member(t => t.IsEnabled).Field(nameof(Order.IsEnabled)).NativeDbType(20);
            f.Member(t => t.CreatedBy).Field(nameof(Order.CreatedBy)).NativeDbType(8);
            f.Member(t => t.CreatedAt).Field(nameof(Order.CreatedAt)).NativeDbType(33);
            f.Member(t => t.UpdatedBy).Field(nameof(Order.UpdatedBy)).NativeDbType(8);
            f.Member(t => t.UpdatedAt).Field(nameof(Order.UpdatedAt)).NativeDbType(33);

            f.HasOne(t => t.Buyer).HasForeignKey(t => t.BuyerId);
            f.HasOne(t => t.Seller).HasForeignKey(t => t.SellerId).MapTo<User>();
            f.HasMany(t => t.Details).HasForeignKey(t => t.OrderId);
        });
        builder.Entity<OrderDetail>(f =>
        {
            f.ToTable("sys_order_detail").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(OrderDetail.Id)).NativeDbType(8);
            f.Member(t => t.OrderId).Field(nameof(OrderDetail.OrderId)).NativeDbType(8);
            f.Member(t => t.ProductId).Field(nameof(OrderDetail.ProductId)).NativeDbType(8);
            f.Member(t => t.Price).Field(nameof(OrderDetail.Price)).NativeDbType(5);
            f.Member(t => t.Quantity).Field(nameof(OrderDetail.Quantity)).NativeDbType(8);
            f.Member(t => t.Amount).Field(nameof(OrderDetail.Amount)).NativeDbType(5);
            f.Member(t => t.IsEnabled).Field(nameof(OrderDetail.IsEnabled)).NativeDbType(2);
            f.Member(t => t.CreatedAt).Field(nameof(OrderDetail.CreatedAt)).NativeDbType(4);
            f.Member(t => t.CreatedBy).Field(nameof(OrderDetail.CreatedBy)).NativeDbType(8);
            f.Member(t => t.UpdatedAt).Field(nameof(OrderDetail.UpdatedAt)).NativeDbType(4);
            f.Member(t => t.UpdatedBy).Field(nameof(OrderDetail.UpdatedBy)).NativeDbType(8);

            f.HasOne(t => t.Order).HasForeignKey(t => t.OrderId);
            f.HasOne(t => t.Product).HasForeignKey(t => t.ProductId);
        });
        builder.Entity<Product>(f =>
        {
            f.ToTable("sys_product").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Product.Id)).NativeDbType(8);
            f.Member(t => t.ProductNo).Field(nameof(Product.ProductNo)).NativeDbType(12);
            f.Member(t => t.Name).Field(nameof(Product.Name)).NativeDbType(12);
            f.Member(t => t.BrandId).Field(nameof(Product.BrandId)).NativeDbType(8);
            f.Member(t => t.CategoryId).Field(nameof(Product.CategoryId)).NativeDbType(8);
            f.Member(t => t.Price).Field(nameof(Product.Price)).NativeDbType(6);
            f.Member(t => t.CompanyId).Field(nameof(Product.CompanyId)).NativeDbType(8);
            f.Member(t => t.IsEnabled).Field(nameof(Product.IsEnabled)).NativeDbType(2);
            f.Member(t => t.CreatedAt).Field(nameof(Product.CreatedAt)).NativeDbType(4);
            f.Member(t => t.CreatedBy).Field(nameof(Product.CreatedBy)).NativeDbType(8);
            f.Member(t => t.UpdatedAt).Field(nameof(Product.UpdatedAt)).NativeDbType(4);
            f.Member(t => t.UpdatedBy).Field(nameof(Product.UpdatedBy)).NativeDbType(8);

            f.HasOne(t => t.Brand).HasForeignKey(t => t.BrandId).MapTo<Brand>();
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
        });
        builder.Entity<Brand>(f =>
        {
            f.ToTable("sys_brand").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Brand.Id)).NativeDbType(8);
            f.Member(t => t.BrandNo).Field(nameof(Brand.BrandNo)).NativeDbType(12);
            f.Member(t => t.Name).Field(nameof(Brand.Name)).NativeDbType(12);
            f.Member(t => t.CompanyId).Field(nameof(Brand.CompanyId)).NativeDbType(8);
            f.Member(t => t.IsEnabled).Field(nameof(Brand.IsEnabled)).NativeDbType(2);
            f.Member(t => t.CreatedAt).Field(nameof(Brand.CreatedAt)).NativeDbType(4);
            f.Member(t => t.CreatedBy).Field(nameof(Brand.CreatedBy)).NativeDbType(8);
            f.Member(t => t.UpdatedAt).Field(nameof(Brand.UpdatedAt)).NativeDbType(4);
            f.Member(t => t.UpdatedBy).Field(nameof(Brand.UpdatedBy)).NativeDbType(8);

            f.HasMany(t => t.Products).HasForeignKey(t => t.BrandId);
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
        });
        builder.Entity<Menu>(f =>
        {
            f.ToTable("sys_menu").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Menu.Id)).NativeDbType(8);
            f.Member(t => t.Name).Field(nameof(Menu.Name)).NativeDbType(12);
            f.Member(t => t.ParentId).Field(nameof(Menu.ParentId)).NativeDbType(8);
            f.Member(t => t.PageId).Field(nameof(Menu.PageId)).NativeDbType(8);
            f.Member(t => t.IsEnabled).Field(nameof(Menu.IsEnabled)).NativeDbType(20);
            f.Member(t => t.CreatedBy).Field(nameof(Menu.CreatedBy)).NativeDbType(8);
            f.Member(t => t.CreatedAt).Field(nameof(Menu.CreatedAt)).NativeDbType(33);
            f.Member(t => t.UpdatedBy).Field(nameof(Menu.UpdatedBy)).NativeDbType(8);
            f.Member(t => t.UpdatedAt).Field(nameof(Menu.UpdatedAt)).NativeDbType(33);
        });
        builder.Entity<Page>(f =>
        {
            f.ToTable("sys_page").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Page.Id)).NativeDbType(8);
            f.Member(t => t.Url).Field(nameof(Page.Url)).NativeDbType(12);
            f.Member(t => t.IsEnabled).Field(nameof(Page.IsEnabled)).NativeDbType(20);
            f.Member(t => t.CreatedBy).Field(nameof(Page.CreatedBy)).NativeDbType(8);
            f.Member(t => t.CreatedAt).Field(nameof(Page.CreatedAt)).NativeDbType(33);
            f.Member(t => t.UpdatedBy).Field(nameof(Page.UpdatedBy)).NativeDbType(8);
            f.Member(t => t.UpdatedAt).Field(nameof(Page.UpdatedAt)).NativeDbType(33);
        });
    }
}
