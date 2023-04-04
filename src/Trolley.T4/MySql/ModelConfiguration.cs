using PaymentCenter.Domain.Models;
using Thea.Orm;

namespace PaymentCenter;

class ModelConfiguration : IModelConfiguration
{
    public void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(f =>
        {
            f.ToTable("sys_user").Key(t => t.Id);
            f.Member(t => t.Id).Field(nameof(User.Id)).NativeDbType(3);
            f.Member(t => t.Name).Field(nameof(User.Name)).NativeDbType(253);
            f.Member(t => t.Gender).Field(nameof(User.Gender)).NativeDbType(1);
            f.Member(t => t.Age).Field(nameof(User.Age)).NativeDbType(3);
            f.Member(t => t.CompanyId).Field(nameof(User.CompanyId)).NativeDbType(3);
            f.Member(t => t.GuidField).Field(nameof(User.GuidField)).NativeDbType(253);
            f.Member(t => t.SomeTimes).Field(nameof(User.SomeTimes)).NativeDbType(11);
            f.Member(t => t.IsEnabled).Field(nameof(User.IsEnabled)).NativeDbType(1);
            f.Member(t => t.CreatedAt).Field(nameof(User.CreatedAt)).NativeDbType(12);
            f.Member(t => t.CreatedBy).Field(nameof(User.CreatedBy)).NativeDbType(3);
            f.Member(t => t.UpdatedAt).Field(nameof(User.UpdatedAt)).NativeDbType(12);
            f.Member(t => t.UpdatedBy).Field(nameof(User.UpdatedBy)).NativeDbType(3);
        });
    }
}

