using DigiStore.Services.CartServices;
using DigiStore.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DigiStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            try 
            {
                await _cartService.AddToCartAsync(productId, quantity);

                return Json(new
                {
                    success = true,
                    message = "محصول با موفقیت به سبد خرید اضافه شد.",
                });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "خطایی رخ داد: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMiniCart()
        {
            var cart = await _cartService.GetOrCreateCartAsync();


            var vm = new CartViewModel
            {
                Items = cart.Items.Select(i => new CartItemViewModel
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Title = i.Product.Title,
                    ImageName = i.Product.MainImageName, 
                    Price = i.Price, 
                    Quantity = i.Quantity
                }).ToList()
            };

            return PartialView("Partials/_MiniCart", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = await _cartService.GetOrCreateCartAsync();

            if (cart == null)
            {
                cart = new Cart();
            }

            var vm = new CartViewModel
            {

                Items = cart.Items?.Select(i => new CartItemViewModel
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Title = i.Product?.Title ?? "بدون نام",
                    ImageName = i.Product?.MainImageName ?? "no-image.png",
                    Price = (long)i.Price,
                    Quantity = i.Quantity
                }).ToList() ?? new List<CartItemViewModel>() 
            };

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> ChangeQuantity(int itemId, int quantity)
        {
            await _cartService.UpdateQuantityAsync(itemId, quantity);


            var cart = await _cartService.GetOrCreateCartAsync();


            var grandTotal = cart.Items.Sum(x => x.Price * x.Quantity);


            var totalCount = cart.Items.Sum(x => x.Quantity);

            return Json(new
            {
                success = true,
                newTotal = grandTotal.ToString("N0"),
                newCount = totalCount 
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            await _cartService.RemoveFromCartAsync(itemId);
            var cart = await _cartService.GetOrCreateCartAsync();
            var grandTotal = cart.Items.Sum(x => x.Price * x.Quantity);

            return Json(new
            {
                success = true,
                newTotal = grandTotal.ToString("N0")
            });
        }
    }
}
