namespace Trolley.Test;

class NpgsqlModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(f =>
        {
            f.ToTable("sys_user").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(User.Id)).NativeDbType(9);
            f.Member(t => t.Name).Field(nameof(User.Name)).NativeDbType(22);
            f.Member(t => t.Gender).Field(nameof(User.Gender)).NativeDbType(18);
            f.Member(t => t.Age).Field(nameof(User.Age)).NativeDbType(9);
            f.Member(t => t.CompanyId).Field(nameof(User.CompanyId)).NativeDbType(9);
            f.Member(t => t.IsEnabled).Field(nameof(User.IsEnabled)).NativeDbType(2);
            f.Member(t => t.CreatedBy).Field(nameof(User.CreatedBy)).NativeDbType(9);
            f.Member(t => t.CreatedAt).Field(nameof(User.CreatedAt)).NativeDbType(21);
            f.Member(t => t.UpdatedBy).Field(nameof(User.UpdatedBy)).NativeDbType(9);
            f.Member(t => t.UpdatedAt).Field(nameof(User.UpdatedAt)).NativeDbType(21);

            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
            f.HasMany(t => t.Orders).HasForeignKey(t => t.BuyerId);
        });
        builder.Entity<Company>(f =>
        {
            f.ToTable("sys_company").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Company.Id)).NativeDbType(9);
            f.Member(t => t.Name).Field(nameof(Company.Name)).NativeDbType(22);
            f.Member(t => t.IsEnabled).Field(nameof(Company.IsEnabled)).NativeDbType(2);
            f.Member(t => t.CreatedBy).Field(nameof(Company.CreatedBy)).NativeDbType(9);
            f.Member(t => t.CreatedAt).Field(nameof(Company.CreatedAt)).NativeDbType(21);
            f.Member(t => t.UpdatedBy).Field(nameof(Company.UpdatedBy)).NativeDbType(9);
            f.Member(t => t.UpdatedAt).Field(nameof(Company.UpdatedAt)).NativeDbType(21);

            f.HasMany(t => t.Users).HasForeignKey(t => t.CompanyId);
            f.HasMany(t => t.Brands).HasForeignKey(t => t.CompanyId);
            f.HasMany(t => t.Products).HasForeignKey(t => t.CompanyId);
        });
        builder.Entity<Order>(f =>
        {
            f.ToTable("sys_order").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Order.Id)).NativeDbType(9);
            f.Member(t => t.OrderNo).Field(nameof(Order.OrderNo)).NativeDbType(22);
            f.Member(t => t.ProductCount).Field(nameof(Order.ProductCount)).NativeDbType(9);
            f.Member(t => t.TotalAmount).Field(nameof(Order.TotalAmount)).NativeDbType(8);
            f.Member(t => t.BuyerId).Field(nameof(Order.BuyerId)).NativeDbType(9);
            f.Member(t => t.SellerId).Field(nameof(Order.SellerId)).NativeDbType(9);
            //特殊类型JSON
            f.Member(t => t.Products).Field(nameof(Order.Products)).NativeDbType(36).SetTypeHandler<JsonTypeHandler>();
            f.Member(t => t.IsEnabled).Field(nameof(Order.IsEnabled)).NativeDbType(2);
            f.Member(t => t.CreatedBy).Field(nameof(Order.CreatedBy)).NativeDbType(9);
            f.Member(t => t.CreatedAt).Field(nameof(Order.CreatedAt)).NativeDbType(21);
            f.Member(t => t.UpdatedBy).Field(nameof(Order.UpdatedBy)).NativeDbType(9);
            f.Member(t => t.UpdatedAt).Field(nameof(Order.UpdatedAt)).NativeDbType(21);

            f.HasOne(t => t.Buyer).HasForeignKey(t => t.BuyerId);
            f.HasOne(t => t.Seller).HasForeignKey(t => t.SellerId).MapTo<User>();
            f.HasMany(t => t.Details).HasForeignKey(t => t.OrderId);
        });
        builder.Entity<OrderDetail>(f =>
        {
            f.ToTable("sys_order_detail").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(OrderDetail.Id)).NativeDbType(9);
            f.Member(t => t.OrderId).Field(nameof(OrderDetail.OrderId)).NativeDbType(9);
            f.Member(t => t.ProductId).Field(nameof(OrderDetail.ProductId)).NativeDbType(9);
            f.Member(t => t.Price).Field(nameof(OrderDetail.Price)).NativeDbType(8);
            f.Member(t => t.Quantity).Field(nameof(OrderDetail.Quantity)).NativeDbType(9);
            f.Member(t => t.Amount).Field(nameof(OrderDetail.Amount)).NativeDbType(8);
            f.Member(t => t.IsEnabled).Field(nameof(OrderDetail.IsEnabled)).NativeDbType(2);
            f.Member(t => t.CreatedBy).Field(nameof(OrderDetail.CreatedBy)).NativeDbType(9);
            f.Member(t => t.CreatedAt).Field(nameof(OrderDetail.CreatedAt)).NativeDbType(21);
            f.Member(t => t.UpdatedBy).Field(nameof(OrderDetail.UpdatedBy)).NativeDbType(9);
            f.Member(t => t.UpdatedAt).Field(nameof(OrderDetail.UpdatedAt)).NativeDbType(21);

            f.HasOne(t => t.Order).HasForeignKey(t => t.OrderId);
            f.HasOne(t => t.Product).HasForeignKey(t => t.ProductId);
        });
        builder.Entity<Product>(f =>
        {
            f.ToTable("sys_product").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Product.Id)).NativeDbType(9);
            f.Member(t => t.ProductNo).Field(nameof(Product.ProductNo)).NativeDbType(22);
            f.Member(t => t.Name).Field(nameof(Product.Name)).NativeDbType(22);
            f.Member(t => t.BrandId).Field(nameof(Product.BrandId)).NativeDbType(9);
            f.Member(t => t.CategoryId).Field(nameof(Product.CategoryId)).NativeDbType(9);
            f.Member(t => t.Price).Field(nameof(Product.Price)).NativeDbType(8);
            f.Member(t => t.CompanyId).Field(nameof(Product.CompanyId)).NativeDbType(9);
            f.Member(t => t.IsEnabled).Field(nameof(Product.IsEnabled)).NativeDbType(2);
            f.Member(t => t.CreatedBy).Field(nameof(Product.CreatedBy)).NativeDbType(9);
            f.Member(t => t.CreatedAt).Field(nameof(Product.CreatedAt)).NativeDbType(21);
            f.Member(t => t.UpdatedBy).Field(nameof(Product.UpdatedBy)).NativeDbType(9);
            f.Member(t => t.UpdatedAt).Field(nameof(Product.UpdatedAt)).NativeDbType(21);

            f.HasOne(t => t.Brand).HasForeignKey(t => t.BrandId).MapTo<Brand>();
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
        });
        builder.Entity<Brand>(f =>
        {
            f.ToTable("sys_brand").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(Brand.Id)).NativeDbType(9);
            f.Member(t => t.BrandNo).Field(nameof(Brand.BrandNo)).NativeDbType(22);
            f.Member(t => t.Name).Field(nameof(Brand.Name)).NativeDbType(22);
            f.Member(t => t.CompanyId).Field(nameof(Brand.CompanyId)).NativeDbType(9);
            f.Member(t => t.IsEnabled).Field(nameof(Brand.IsEnabled)).NativeDbType(2);
            f.Member(t => t.CreatedBy).Field(nameof(Brand.CreatedBy)).NativeDbType(9);
            f.Member(t => t.CreatedAt).Field(nameof(Brand.CreatedAt)).NativeDbType(21);
            f.Member(t => t.UpdatedBy).Field(nameof(Brand.UpdatedBy)).NativeDbType(9);
            f.Member(t => t.UpdatedAt).Field(nameof(Brand.UpdatedAt)).NativeDbType(21);

            f.HasMany(t => t.Products).HasForeignKey(t => t.BrandId);
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
        });
    }
}
