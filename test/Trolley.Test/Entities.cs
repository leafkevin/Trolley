using System;
using System.Collections.Generic;

namespace Trolley.Test;

public enum Gender : byte
{
    Male = 1,
    Female = 2
}
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public int CompanyId { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CompanyInfo Company { get; set; }
    public List<Order> Orders { get; set; }
}
public class UserInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public int CompanyId { get; set; }
}
public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<User> Users { get; set; }
    public List<Brand> Brands { get; set; }
    public List<Product> Products { get; set; }
}
public class CompanyInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
}
public class Order
{
    public int Id { get; set; }
    public string OrderNo { get; set; }
    public double TotalAmount { get; set; }
    public int BuyerId { get; set; }
    public int SellerId { get; set; }
    public int? ProductCount { get; set; }
    public List<int> Products { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User Buyer { get; set; }
    public UserInfo Seller { get; set; }
    public List<OrderDetail> Details { get; set; }
}
public class OrderInfo
{
    public int Id { get; set; }
    public string OrderNo { get; set; }
    public int BuyerId { get; set; }
}
public class OrderDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public double Price { get; set; }
    public int Quantity { get; set; }
    public double Amount { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Order Order { get; set; }
    public Product Product { get; set; }
}
public class Product
{
    public int Id { get; set; }
    public string ProductNo { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
    public int CompanyId { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public BrandInfo Brand { get; set; }
    public CompanyInfo Company { get; set; }
}
public class Brand
{
    public int Id { get; set; }
    public string BrandNo { get; set; }
    public string Name { get; set; }
    public int CompanyId { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Product> Products { get; set; }
    public CompanyInfo Company { get; set; }
}
public class BrandInfo
{
    public int Id { get; set; }
    public string BrandNo { get; set; }
    public string Name { get; set; }
}
public class Menu
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ParentId { get; set; }
    public int PageId { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class Page
{
    public int Id { get; set; }
    public string Url { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
