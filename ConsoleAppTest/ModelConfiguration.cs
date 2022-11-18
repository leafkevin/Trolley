using Trolley;

namespace ConsoleAppTest;

class ModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(f =>
        {
            f.ToTable("sys_user").Key(t => t.Id);

            f.HasOne(t => t.Company).HasForeignKey(t => t.CompanyId).MapTo<Company>();
            f.HasMany(t => t.Orders).HasForeignKey(t => t.BuyerId).MapTo<Order>();
        });
        builder.Entity<Company>(f =>
        {
            f.ToTable("sys_company").Key(t => t.Id);
            f.HasMany(t => t.Users).HasForeignKey(t => t.CompanyId);
        });
        builder.Entity<Order>(f =>
        {
            f.ToTable("sys_order").Key(t => t.Id);
            f.HasMany(t => t.Details).HasForeignKey(t => t.OrderId);
        });
        builder.Entity<OrderDetail>(f =>
        {
            f.ToTable("sys_order_detail").Key(f => f.Id);
            f.HasOne(t => t.Order).HasForeignKey(t => t.OrderId);
        });
        builder.Entity<LookupValue>(f =>
        {
            f.ToTable("sys_lookup_value").Key("LookupId", "LookupValue");
            f.Member(f => f.Value).Field("lookup_value");
        });
    }
}
