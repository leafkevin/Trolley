using System;
using System.Collections.Generic;

namespace ConsoleAppTest;

public class Order
{
    public int Id { get; set; }
    public string OrderNo { get; set; }
    public double TotalAmount { get; set; }
    public int BuyerId { get; set; }
    public int SellerId { get; set; }
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