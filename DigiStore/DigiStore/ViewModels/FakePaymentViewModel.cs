using System.ComponentModel.DataAnnotations;

public class FakePaymentViewModel
{
    [Required]
    [Range(1000, 5000000)]
    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = "FakeGateway";
}