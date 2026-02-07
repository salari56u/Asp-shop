using DigiStore.Data;
using System.ComponentModel.DataAnnotations;

public class Order
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public decimal TotalAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }

    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    public string ReceiverName { get; set; }
    public string ReceiverPhone { get; set; }
    public string Province { get; set; }
    public string City { get; set; }
    public string FullAddress { get; set; }
    public string PostalCode { get; set; }

    public ICollection<OrderItem> Items { get; set; }

    public int? PaymentId { get; set; }
    public Payment Payment { get; set; }

    public int? AddressId { get; set; }
    public Address Address { get; set; }

    public int? CouponId { get; set; }
    public Coupon Coupon { get; set; }
}