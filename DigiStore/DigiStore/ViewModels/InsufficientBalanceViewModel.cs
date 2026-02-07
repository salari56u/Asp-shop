using DigiStore.ViewModels;

public class InsufficientBalanceViewModel
{
    public decimal WalletBalance { get; set; }
    public decimal OrderAmount { get; set; }
    public decimal Shortage { get; set; }
    public CartViewModel Cart { get; set; }
}
