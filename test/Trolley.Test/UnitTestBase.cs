using System;
using System.Collections.Generic;

namespace Trolley.Test;

public class UnitTestBase
{
    public IOrmDbFactory dbFactory;
    public static int connTotal = 0;
    public static int connOpenTotal = 0;
    public static int tranTotal = 0;

    public void Initialize()
    {
        var repository = this.dbFactory.CreateRepository();
        repository.BeginTransaction();
        repository.Delete<User>(new[] { 1, 2 });
        repository.Create<User>(new[]
        {
            new User
            {
                Id = 1,
                TenantId = "1",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                UpdatedBy = 1
            },
            new User
            {
                Id = 2,
                TenantId = "2",
                Name = "cindy",
                Age = 21,
                CompanyId = 2,
                Gender = Gender.Male,
                GuidField= Guid.NewGuid(),
                SomeTimes= TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
                SourceType = UserSourceType.Taobao,
                IsEnabled = true,
                CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });
        if (!repository.Exists<Company>())
        {
            repository.Create<Company>(new[]
            {
                new Company
                {
                    Name = "微软",
                    Nature = CompanyNature.Internet,
                    IsEnabled = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = 1,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = 1
                },
                new Company
                {
                    Name = "谷歌",
                    Nature = CompanyNature.Internet,
                    IsEnabled = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = 1,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = 1
                }
            });
        }
        repository.Delete<Brand>(new[] { 1, 2, 3 });
        repository.Create<Brand>(new[]
        {
            new Brand
            {
                Id = 1,
                BrandNo = "BN-001",
                Name = "波司登",
                CompanyId= 1,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Brand
            {
                Id = 2,
                BrandNo = "BN-002",
                Name = "雪中飞",
                CompanyId= 2,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Brand
            {
                Id = 3,
                BrandNo = "BN-003",
                Name = "优衣库",
                CompanyId = 1,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<Product>(new[] { 1, 2, 3 });
        repository.Create<Product>(new[]
        {
            new Product
            {
                Id = 1,
                ProductNo="PN-001",
                Name = "波司登羽绒服",
                Price =550,
                BrandId = 1,
                CategoryId = 1,
                CompanyId = 1,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Product
            {
                Id = 2,
                ProductNo="PN-002",
                Name = "雪中飞羽绒裤",
                Price =350,
                BrandId = 2,
                CategoryId = 2,
                CompanyId = 2,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Product
            {
                Id = 3,
                ProductNo="PN-003",
                Name = "优衣库保暖内衣",
                Price =180,
                BrandId = 3,
                CategoryId = 3,
                CompanyId = 1,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<Order>(new[] { "1", "2", "3" });
        repository.Create<Order>(new[]
        {
            new Order
            {
                Id = "1",
                TenantId = "1",
                OrderNo = "ON-001",
                BuyerId = 1,
                BuyerSource = UserSourceType.Douyin,
                SellerId = 2,
                TotalAmount = 500,
                Products = new List<int>{1, 2},
                ProductCount = 2,
                Disputes = new Dispute
                {
                    Id = 1,
                    Content = "无良商家，投诉，投诉",
                    Result = "同意更换",
                    Users = "Buyer1,Seller1",
                    CreatedAt = DateTime.Now
                },
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Order
            {
                Id = "2",
                TenantId = "2",
                OrderNo = "ON-002",
                BuyerId = 2,
                BuyerSource = UserSourceType.Taobao,
                SellerId = 1,
                TotalAmount = 350,
                Products = new List<int>{1, 3},
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
            },
            new Order
            {
                Id = "3",
                TenantId = "3",
                OrderNo = "ON-003",
                BuyerId = 1,
                BuyerSource = UserSourceType.Douyin,
                SellerId = 2,
                TotalAmount = 199,
                Products = new List<int>{2},
                ProductCount = 1,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<OrderDetail>(new[] { "1", "2", "3", "4", "5", "6" });
        repository.Create<OrderDetail>(new[]
        {
            new OrderDetail
            {
                Id = "1",
                TenantId = "1",
                OrderId = "1",
                ProductId = 1,
                Price = 299,
                Quantity = 1,
                Amount = 299,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = "2",
                TenantId = "1",
                OrderId = "1",
                ProductId = 2,
                Price = 159,
                Quantity = 1,
                Amount = 159,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = "3",
                TenantId = "2",
                OrderId = "1",
                ProductId = 3,
                Price = 69,
                Quantity = 1,
                Amount = 69,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = "4",
                TenantId = "1",
                OrderId = "2",
                ProductId = 1,
                Price = 299,
                Quantity = 1,
                Amount = 299,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = "5",
                TenantId = "2",
                OrderId = "2",
                ProductId = 3,
                Price = 69,
                Quantity = 1,
                Amount = 69,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = "6",
                TenantId = "3",
                OrderId = "3",
                ProductId = 2,
                Price = 199,
                Quantity = 1,
                Amount = 199,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<Menu>(new[] { 1, 2, 3 });
        repository.Create<Menu>(new[]
        {
            new Menu
            {
                Id = 1,
                Name = "系统管理",
                PageId = 0,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Menu
            {
                Id = 2,
                Name = "用户管理",
                PageId = 1,
                ParentId = 1,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Menu
            {
                Id = 3,
                Name = "角色管理",
                PageId = 2,
                ParentId = 1,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<Page>(new[] { 1, 2, 3 });
        repository.Create<Page>(new[]
        {
            new Page
            {
                Id = 1,
                Url = "/user/index",
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Page
            {
                Id = 2,
                Url = "/role/index",
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<UpdateEntity>(new[] { 1, 2 });
        repository.Create<UpdateEntity>(new UpdateEntity[]
        {
            new UpdateEntity
            {
                Id = 1,
                BooleanField = true,
                DateOnlyField = DateOnly.FromDateTime(DateTime.Now),
                DateTimeField = DateTime.Now,
                DateTimeOffsetField = DateTimeOffset.UtcNow,
                EnumField = Gender.Male,
                GuidField = Guid.NewGuid(),
                TimeOnlyField = TimeOnly.FromDateTime(DateTime.Now),
                TimeSpanField = TimeSpan.FromMinutes(350)
            },
            new UpdateEntity
            {
               Id = 2,
                BooleanField = false ,
                DateOnlyField = DateOnly.Parse("2024-07-07"),
                DateTimeField = DateTime.Now,
                DateTimeOffsetField = DateTimeOffset.UtcNow,
                EnumField = Gender.Male,
                GuidField = Guid.NewGuid(),
                TimeOnlyField = TimeOnly.FromDateTime(DateTime.Now),
                TimeSpanField = TimeSpan.FromMinutes(350)
            }
        });
        repository.Commit();
    }    
    public interface IPassport
    {
        //只用于演示，实际使用中要与ASP.NET CORE中间件或是IOC组件相结合，赋值此对象
        string TenantId { get; set; }
        string UserId { get; set; }
    }
    public class Passport : IPassport
    {
        public string TenantId { get; set; }
        public string UserId { get; set; }
    }
}
