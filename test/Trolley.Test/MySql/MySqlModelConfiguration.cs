namespace Trolley.Test;

class MySqlModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(f =>
        {
            f.ToTable("sys_user").Key(t => t.Id);
            f.Member(t => t.Id).Field("Id").NativeDbType(3);
            f.Member(t => t.Name).Field("Name").NativeDbType(253);
            f.Member(t => t.Gender).Field("Gender").NativeDbType(1);
            f.Member(t => t.Age).Field("Age").NativeDbType(3);
            f.Member(t => t.CompanyId).Field("CompanyId").NativeDbType(3);
            f.Member(t => t.IsEnabled).Field("IsEnabled").NativeDbType(1);
            f.Member(t => t.CreatedBy).Field("CreatedBy").NativeDbType(3);
            f.Member(t => t.CreatedAt).Field("CreatedAt").NativeDbType(12);
            f.Member(t => t.UpdatedBy).Field("UpdatedBy").NativeDbType(3);
            f.Member(t => t.UpdatedAt).Field("UpdatedAt").NativeDbType(12);

            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
            f.HasMany(t => t.Orders).HasForeignKey(t => t.BuyerId);
        });
        builder.Entity<Company>(f =>
        {
            f.ToTable("sys_company").Key(t => t.Id);
            f.Member(t => t.Id).Field("Id").NativeDbType(3);
            f.Member(t => t.Name).Field("Name").NativeDbType(253);
            f.Member(t => t.IsEnabled).Field("IsEnabled").NativeDbType(1);
            f.Member(t => t.CreatedBy).Field("CreatedBy").NativeDbType(3);
            f.Member(t => t.CreatedAt).Field("CreatedAt").NativeDbType(12);
            f.Member(t => t.UpdatedBy).Field("UpdatedBy").NativeDbType(3);
            f.Member(t => t.UpdatedAt).Field("UpdatedAt").NativeDbType(12);

            f.HasMany(t => t.Users).HasForeignKey(t => t.CompanyId);
            f.HasMany(t => t.Brands).HasForeignKey(t => t.CompanyId);
            f.HasMany(t => t.Products).HasForeignKey(t => t.CompanyId);
        });
        builder.Entity<Order>(f =>
        {
            f.ToTable("sys_order").Key(t => t.Id);
            f.Member(t => t.Id).Field("Id").NativeDbType(3);
            f.Member(t => t.OrderNo).Field("OrderNo").NativeDbType(253);
            f.Member(t => t.ProductCount).Field("ProductCount").NativeDbType(3);
            f.Member(t => t.TotalAmount).Field("TotalAmount").NativeDbType(5);
            f.Member(t => t.BuyerId).Field("BuyerId").NativeDbType(3);
            f.Member(t => t.SellerId).Field("SellerId").NativeDbType(3);
            //特殊类型JSON
            f.Member(t => t.Products).NativeDbType(245).SetTypeHandler<JsonTypeHandler>();
            f.Member(t => t.IsEnabled).Field("IsEnabled").NativeDbType(1);
            f.Member(t => t.CreatedBy).Field("CreatedBy").NativeDbType(3);
            f.Member(t => t.CreatedAt).Field("CreatedAt").NativeDbType(12);
            f.Member(t => t.UpdatedBy).Field("UpdatedBy").NativeDbType(3);
            f.Member(t => t.UpdatedAt).Field("UpdatedAt").NativeDbType(12);

            f.HasOne(t => t.Buyer).HasForeignKey(t => t.BuyerId);
            f.HasOne(t => t.Seller).HasForeignKey(t => t.SellerId).MapTo<User>();
            f.HasMany(t => t.Details).HasForeignKey(t => t.OrderId);
        });
        builder.Entity<OrderDetail>(f =>
        {
            f.ToTable("sys_order_detail").Key(t => t.Id);
            f.Member(t => t.Id).Field("Id").NativeDbType(3);
            f.Member(t => t.OrderId).Field("OrderId").NativeDbType(3);
            f.Member(t => t.ProductId).Field("ProductId").NativeDbType(3);
            f.Member(t => t.Price).Field("Price").NativeDbType(5);
            f.Member(t => t.Quantity).Field("Quantity").NativeDbType(3);
            f.Member(t => t.Amount).Field("Amount").NativeDbType(5);
            f.Member(t => t.IsEnabled).Field("IsEnabled").NativeDbType(1);
            f.Member(t => t.CreatedBy).Field("CreatedBy").NativeDbType(3);
            f.Member(t => t.CreatedAt).Field("CreatedAt").NativeDbType(12);
            f.Member(t => t.UpdatedBy).Field("UpdatedBy").NativeDbType(3);
            f.Member(t => t.UpdatedAt).Field("UpdatedAt").NativeDbType(12);

            f.HasOne(t => t.Order).HasForeignKey(t => t.OrderId);
            f.HasOne(t => t.Product).HasForeignKey(t => t.ProductId);
        });
        builder.Entity<Product>(f =>
        {
            f.ToTable("sys_product").Key(t => t.Id);
            f.Member(t => t.Id).Field("Id").NativeDbType(3);
            f.Member(t => t.ProductNo).Field("ProductNo").NativeDbType(253);
            f.Member(t => t.Name).Field("Name").NativeDbType(253);
            f.Member(t => t.BrandId).Field("BrandId").NativeDbType(3);
            f.Member(t => t.CategoryId).Field("CategoryId").NativeDbType(3);
            f.Member(t => t.Price).Field("Price").NativeDbType(5);
            f.Member(t => t.CompanyId).Field("CompanyId").NativeDbType(3);
            f.Member(t => t.IsEnabled).Field("IsEnabled").NativeDbType(1);
            f.Member(t => t.CreatedBy).Field("CreatedBy").NativeDbType(3);
            f.Member(t => t.CreatedAt).Field("CreatedAt").NativeDbType(12);
            f.Member(t => t.UpdatedBy).Field("UpdatedBy").NativeDbType(3);
            f.Member(t => t.UpdatedAt).Field("UpdatedAt").NativeDbType(12);

            f.HasOne(t => t.Brand).HasForeignKey(t => t.BrandId).MapTo<Brand>();
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
        });
        builder.Entity<Brand>(f =>
        {
            f.ToTable("sys_brand").Key(t => t.Id);
            f.Member(t => t.Id).Field("Id").NativeDbType(3);
            f.Member(t => t.BrandNo).Field("BrandNo").NativeDbType(253);
            f.Member(t => t.Name).Field("Name").NativeDbType(253);
            f.Member(t => t.CompanyId).Field("CompanyId").NativeDbType(3);
            f.Member(t => t.IsEnabled).Field("IsEnabled").NativeDbType(1);
            f.Member(t => t.CreatedBy).Field("CreatedBy").NativeDbType(3);
            f.Member(t => t.CreatedAt).Field("CreatedAt").NativeDbType(12);
            f.Member(t => t.UpdatedBy).Field("UpdatedBy").NativeDbType(3);
            f.Member(t => t.UpdatedAt).Field("UpdatedAt").NativeDbType(12);

            f.HasMany(t => t.Products).HasForeignKey(t => t.BrandId);
            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
        });
    }
}
