using DigiStore.Data;
using DigiStore.Models;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DigiStore.Services.CartServices
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public const string CartCookieName = "DigiStore_GuestId";
        public CartService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddToCartAsync(int productId, int quantity)
        {
            var cart = await GetOrCreateCartAsync();
            if (cart == null) return;

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        Price = (long)product.Price,
                        CartId = cart.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
        }


        public async Task AddToListAsync(int productId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var userIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    Liking liking = new Liking
                    {
                        productId = productId,
                        UserId = userId
                    };
                    _context.likings.Add(liking);
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<Cart> GetOrCreateCartAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            Cart cart = null;
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var userIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    cart = await _context.Carts
                        .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                        .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

                    if (cart == null)
                    {
                        cart = new Cart { UserId = userId, IsActive = true };
                        _context.Carts.Add(cart);
                        await _context.SaveChangesAsync();
                    }

                    return cart; 
                }
            }
            var guestId = GetGuestIdFromCookie();

            cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.GuestId == guestId && c.IsActive);

            if (cart == null)
            {
                cart = new Cart { GuestId = guestId, IsActive = true };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task RemoveFromCartAsync(int itemId)
        {
            var item = await _context.CartItems.FindAsync(itemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateQuantityAsync(int itemId, int quantity)
        {
            var item = await _context.CartItems.FindAsync(itemId);
            if (item != null)
            {
                item.Quantity = quantity;
                if (item.Quantity <= 0)
                {
                    _context.CartItems.Remove(item);
                }
                await _context.SaveChangesAsync();
            }
        }

        private Guid GetGuestIdFromCookie()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context.Request.Cookies.ContainsKey(CartCookieName))
            {
                if (Guid.TryParse(context.Request.Cookies[CartCookieName], out Guid result))
                {
                    return result;
                }
            }

            var newGuestId = Guid.NewGuid();
            var options = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                HttpOnly = true,
                IsEssential = true
            };

            context.Response.Cookies.Append(CartCookieName, newGuestId.ToString(), options);

            return newGuestId;
        }
        public async Task MergeCartsAsync(int userId, Guid guestId)
        {
            var guestCart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.GuestId == guestId && c.IsActive);
            if (guestCart == null || !guestCart.Items.Any()) return;
            var userCart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

            if (userCart == null)
            {
                userCart = new Cart { UserId = userId, IsActive = true };
                _context.Carts.Add(userCart);
                await _context.SaveChangesAsync(); 
            }
            foreach (var guestItem in guestCart.Items)
            {
                var existingItem = userCart.Items
                    .FirstOrDefault(i => i.ProductId == guestItem.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity += guestItem.Quantity;
                }
                else
                {
                    var newItem = new CartItem
                    {
                        CartId = userCart.Id,
                        ProductId = guestItem.ProductId,
                        Quantity = guestItem.Quantity,
                        Price = guestItem.Price
                    };
                    _context.CartItems.Add(newItem);
                }
            }

            _context.CartItems.RemoveRange(guestCart.Items);
            _context.Carts.Remove(guestCart);
            await _context.SaveChangesAsync();
        }
    }
}
