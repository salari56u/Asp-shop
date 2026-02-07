// در WalletService.cs
using DigiStore.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

public class WalletService : IWalletService
{
    private readonly AppDbContext _context;
    private readonly ILogger<WalletService> _logger;

    public WalletService(AppDbContext context, ILogger<WalletService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<decimal> GetBalanceAsync(int userId)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);
        return wallet?.Balance ?? 0;
    }

    public async Task AddCreditAsync(int userId, decimal amount)
    {
        var wallet = await GetOrCreateWalletAsync(userId);
        wallet.Balance += amount;

        var transaction = new WalletTransaction
        {
            Wallet = wallet,
            Amount = amount,
            Type = WalletTransactionType.Deposit,
            Description = "افزایش موجودی فیک",
            CreatedAt = DateTime.Now
        };

        _context.WalletTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"اعتبار کاربر {userId} به مبلغ {amount} افزایش یافت");
    }

    public async Task<bool> DeductAsync(int userId, decimal amount, string description)
    {
        var wallet = await GetOrCreateWalletAsync(userId);

        if (wallet.Balance < amount)
            return false;

        wallet.Balance -= amount;

        var transaction = new WalletTransaction
        {
            Wallet = wallet,
            Amount = -amount, 
            Type = WalletTransactionType.Withdraw,
            Description = description,
            CreatedAt = DateTime.Now
        };

        _context.WalletTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<WalletTransaction>> GetTransactionsAsync(int userId, int count = 10)
    {
        var wallet = await GetOrCreateWalletAsync(userId);

        return await _context.WalletTransactions
            .Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<string> AddCreditWithTrackingAsync(int userId, decimal amount, string description)
    {
        var trackingCode = GenerateTrackingCode();

        var wallet = await GetOrCreateWalletAsync(userId);
        wallet.Balance += amount;

        var transaction = new WalletTransaction
        {
            Wallet = wallet,
            Amount = amount,
            Type = WalletTransactionType.Deposit,
            Description = $"{description} - کد رهگیری: {trackingCode}",
            CreatedAt = DateTime.Now
        };

        _context.WalletTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"تراکنش {trackingCode}: اعتبار کاربر {userId} به مبلغ {amount} افزایش یافت");

        return trackingCode;
    }

    public async Task<Wallet> GetWalletAsync(int userId)
    {
        return await GetOrCreateWalletAsync(userId);
    }
    private async Task<Wallet> GetOrCreateWalletAsync(int userId)
    {
        var wallet = await _context.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet == null)
        {
            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0,
                CreatedAt = DateTime.Now
            };
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
        }

        return wallet;
    }
    private string GenerateTrackingCode()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"TRK-{timestamp}-{random}";
    }
}