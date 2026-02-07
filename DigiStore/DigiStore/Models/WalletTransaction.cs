public class WalletTransaction
{
    public int Id { get; set; }

    public int WalletId { get; set; }
    public Wallet Wallet { get; set; }

    public decimal Amount { get; set; }   
    public string Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public WalletTransactionType Type { get; set; }
}
