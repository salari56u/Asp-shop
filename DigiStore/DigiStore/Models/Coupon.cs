using System.ComponentModel.DataAnnotations;

public class Coupon
{
    [Key]
    public int Id { get; set; }
    public string Code { get; set; }
    public decimal DiscountAmount { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public DateTime? Expiry { get; set; }
    public bool IsActive { get; set; }
}