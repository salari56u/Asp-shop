public interface IWalletService
{
    Task<decimal> GetBalanceAsync(int userId);
    Task AddCreditAsync(int userId, decimal amount);
    Task<bool> DeductAsync(int userId, decimal amount, string description);


    Task<List<WalletTransaction>> GetTransactionsAsync(int userId, int count = 10);
    Task<string> AddCreditWithTrackingAsync(int userId, decimal amount, string description);
    Task<Wallet> GetWalletAsync(int userId);
}
