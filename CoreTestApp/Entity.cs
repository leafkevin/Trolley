using System;
using System.Collections.Generic;
using Trolley.Attributes;

namespace CoreTestApp
{
    public class UserInfo
    {
        public int UniqueId { get; set; }
        public string UserName { get; set; }
        public Sex Sex { get; set; }
    }
    [Table("Coin_User")]
    public class User
    {
        [PrimaryKey("Id")]
        public int UniqueId { get; set; }
        public string UserName { get; set; }
        [Column(typeof(string))]
        public Sex? Sex { get; set; }
        public int Age { get; set; }
        public int DeptId { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    [Table("Coin_Dept")]
    public class Dept
    {
        [PrimaryKey("Id")]
        public int UniqueId { get; set; }
        public string DeptName { get; set; }
        public int PersonTotal { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    public class DeptInfo
    {
        public int DeptId { get; set; }
        public int PersonTotal { get; set; }
    }
    [Table("Coin_Order")]
    public class Order
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Number { get; set; }
        public int BuyerId { get; set; }
        public List<OrderLine> Lines { get; set; }
    }
    [Table("Coin_OrderLine")]
    public class OrderLine
    {
        [PrimaryKey]
        public int LineId { get; set; }
        public int ProductId { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Amount { get; set; }
    }
    public enum Sex : byte
    {
        Male = 1,
        Female = 2
    }
}
