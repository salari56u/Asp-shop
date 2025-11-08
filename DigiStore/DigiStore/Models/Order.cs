using System;
using System.Collections.Generic;
using System.Net;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Total { get; set; }
    public ICollection<OrderItem> Items { get; set; }
    public int? PaymentId { get; set; }
    public Payment Payment { get; set; }
    public int? AddressId { get; set; }
    public Address Address { get; set; }

    public int? CouponId { get; set; } 
    public Coupon Coupon { get; set; }
}
