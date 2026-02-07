using Microsoft.VisualBasic;

public class PaymentViewMode
{
    public int SelectedAddressId { get; set; }

    public string msg { get; set; }

    public decimal finalAmount { get; set; }

    public int? OrderId { get; set; }
}