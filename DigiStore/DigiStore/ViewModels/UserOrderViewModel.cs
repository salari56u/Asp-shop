using DigiStore.Data;

public class UserOrderViewModel
{
    public int OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal FinalAmount { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
}
