public class WalletDetailsViewModel
{
    public decimal Balance { get; set; }
    public List<WalletTransaction> Transactions { get; set; }
    public DateTime LastUpdate { get; set; }
}