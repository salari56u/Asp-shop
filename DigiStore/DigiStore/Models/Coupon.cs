using System;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime Expiry { get; set; }
    public bool IsActive { get; set; }
}
