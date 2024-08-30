namespace Trolley.Test;

public class ModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Brand>(f =>
            {
                f.ToTable("sys_brand");
                f.HasMany(t => t.Products).HasForeignKey(t => t.BrandId);
                f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
            })
            .Entity<Company>(f =>
            {
                f.ToTable("sys_company");
                f.HasMany(t => t.Users).HasForeignKey(t => t.CompanyId);
                f.HasMany(t => t.Brands).HasForeignKey(t => t.CompanyId);
                f.HasMany(t => t.Products).HasForeignKey(t => t.CompanyId);
            })
            .Entity<Function>(f => f.ToTable("sys_function"))
            .Entity<Menu>(f => f.ToTable("sys_menu"))
            .Entity<Order>(f =>
            {
                f.ToTable("sys_order");
                f.HasOne(t => t.Buyer).HasForeignKey(t => t.BuyerId);
                f.HasOne(t => t.Seller).HasForeignKey(t => t.SellerId).MapTo<User>();
                f.HasMany(t => t.Details).HasForeignKey(t => t.OrderId);
            })
            .Entity<OrderDetail>(f =>
            {
                f.ToTable("sys_order_detail");
                f.HasOne(t => t.Order).HasForeignKey(t => t.OrderId);
                f.HasOne(t => t.Product).HasForeignKey(t => t.ProductId);
            })
            .Entity<Page>(f => f.ToTable("sys_page"))
            .Entity<Product>(f =>
            {
                f.ToTable("sys_product");
                f.HasOne(t => t.Brand).HasForeignKey(t => t.BrandId).MapTo<Brand>();
                f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
            })
            .Entity<UpdateEntity>(f => f.ToTable("sys_update_entity"))
            .Entity<User>(f =>
            {
                f.ToTable("sys_user");
                f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
                f.HasMany(t => t.Orders).HasForeignKey(t => t.BuyerId);
            })
            .UseAutoMap();
    }
}