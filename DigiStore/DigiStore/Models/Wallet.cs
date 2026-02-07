public class Wallet
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public decimal Balance { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<WalletTransaction> Transactions { get; set; }
}
