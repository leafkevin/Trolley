using System;
using System.Collections.Generic;

namespace ConsoleAppTest;

public class Seller
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Gender Gender { get; set; }
    public int CompanyId { get; set; }
    public bool IsEnabled { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CompanyInfo Company { get; set; }
    public List<Order> Orders { get; set; }
}
