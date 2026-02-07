namespace DigiStore.Services.CartServices
{
    public interface ICartService
    {
        Task<Cart> GetOrCreateCartAsync();

        Task AddToCartAsync(int productId, int quantity);
        Task AddToListAsync(int productId);

        Task RemoveFromCartAsync(int itemId);

        Task UpdateQuantityAsync(int itemId, int quantity);

        Task MergeCartsAsync(int userId, Guid guestId);
    }
}
