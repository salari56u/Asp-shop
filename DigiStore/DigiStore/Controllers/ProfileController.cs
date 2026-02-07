using DigiStore.Data;
using DigiStore.Models;
using DigiStore.Services.CartServices;
using DigiStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;

namespace DigiStore.Controllers
{
    public class ProfileController : Controller
    {
                
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ICartService _cartService;
        private readonly IWalletService _walletService;
        public ProfileController(IHttpContextAccessor httpContextAccessor,AppDbContext appDbContext, ICartService cartService,IWalletService walletService)
        {
            _context=appDbContext;
            _httpContextAccessor=httpContextAccessor;
            _cartService=cartService;
            _walletService=walletService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login", "Auth");

            var user = await _context.Users
                .Include(u => u.Wallet)
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var vm = new ProfileViewModel
            {
                UserName = user.UserName,
                Mobile = user.UserName,
                WalletBalance = user.Wallet?.Balance ?? 0,
                Orders = user.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new UserOrderViewModel
                    {
                        OrderId = o.Id,
                        CreatedAt = o.CreatedAt,
                        FinalAmount = o.FinalAmount,
                        OrderStatus = o.OrderStatus,
                        PaymentStatus = o.PaymentStatus
                    }).ToList()
            };

            return View(vm);
        }

        private string getUserIdStr()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var userIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                return userIdStr;
            }
            return null;
        }


        [HttpGet]
        public async Task<IActionResult> List(string sortBy = "date", int page = 1)
        {
            var userIdStr = getUserIdStr();
            var vm1 = new ListViewModel();
            var httpContext = _httpContextAccessor.HttpContext;
            if (int.TryParse(userIdStr, out int userId))
            {
                vm1.likings =  _context.likings.Where(p => p.UserId == userId).ToList();
            }
            int pageSize = 12;

            var p1 = _context.Products
                .Include(p => p.Images)
                .Include(p => p.ProductCategories)
                .ThenInclude(p => p.Category)
                .AsQueryable();

            if(vm1.likings !=null)
            {
                var likingIds = vm1.likings.Select(p=>p.productId).ToList();
                p1 = p1.Where(p=> likingIds.Contains(p.Id));
            }
            switch (sortBy)
            {
                case "price":
                    p1 = p1.OrderBy(p => p.Price);
                    break;
                case "price-desc":
                    p1 = p1.OrderByDescending(p => p.Price);
                    break;
                case "date":
                default:
                    p1 = p1.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            var totalItems = await p1.CountAsync();
            var products = await p1
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

                var model = new ShopLikingViewModel
                {
                    Products = products,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                    SortBy = sortBy,
                    IsLoggedIn = httpContext.User.Identity?.IsAuthenticated ?? false
                };


                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("Partials/_ProductGrid", model);
                }


                return View(model);
            }


            [HttpPost]
            public async Task<IActionResult> addToLiking(int productid)
            {
                try
                {

                        var userIdStr =getUserIdStr();
                        if (int.TryParse(userIdStr, out int userId))
                        {
                            Liking liking = new Liking
                            {
                                productId = productid,
                                UserId = userId
                            };
                            _context.likings.Add(liking);
                        }

                    await _context.SaveChangesAsync();
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
            [HttpPost]
            public async Task<IActionResult> RemoveFromList(int productid)
            {
                try
                {
                    var userIdStr = getUserIdStr();
                     if (int.TryParse(userIdStr, out int userId))
                    {
                        var vm1 = await _context.likings.FirstOrDefaultAsync(p => p.productId == productid && p.UserId == userId);
                        _context.likings.Remove(vm1);
                    }
                    await _context.SaveChangesAsync();
                    return Json(new
                    {
                        success = false,
                        message = "این محصول در علاقه مندی ها وجود می باشد ",
                    });
                }
                catch (System.Exception ex)
                {
                    return Json(new { success = false, message = "خطایی رخ داد: " + ex.Message });
                }
            }


            [HttpGet]
            public async Task<IActionResult> checkLikingUser(int productid)
            {
                var userIdStr = getUserIdStr();
                var vm1 = new Liking();
                if (int.TryParse(userIdStr, out int userId))
                {
                    vm1 = await _context.likings.FirstOrDefaultAsync(p => p.productId == productid && p.UserId == userId);
                }
                if (vm1 == null)
                {
                    return Json(new { success = false });
                }
                else
                    return Json(new { success = true });
            }

        [HttpPost]
        public async Task<IActionResult> AddWallet(decimal amount)
        {
            var userIdStr = getUserIdStr();
            if (!int.TryParse(userIdStr, out int userId))
                return Json(new { success = false, message = "کاربر پیدا نشد" });

            await _walletService.AddCreditAsync(userId, amount);

            return Json(new { success = true, message = "موجودی کیف پول با موفقیت افزایش یافت" });
        }


        [HttpGet]
        public async Task<IActionResult> Wallet()
        {
            var userIdStr = getUserIdStr();
            if (!int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login", "Auth");

            var balance = await _walletService.GetBalanceAsync(userId);
            var transactions = await _walletService.GetTransactionsAsync(userId, 15);

            var vm = new WalletDetailsViewModel
            {
                Balance = balance,
                Transactions = transactions,
                LastUpdate = DateTime.Now
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddCredit([FromBody] AddCreditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new
                {
                    success = false,
                    message = "داده‌های ورودی نامعتبر است",
                    errors = errors
                });
            }

            var userIdStr = getUserIdStr();
            if (!int.TryParse(userIdStr, out int userId))
                return Json(new { success = false, message = "کاربر یافت نشد" });

            try
            {
                var trackingCode = await _walletService.AddCreditWithTrackingAsync(
                    userId,
                    model.Amount,
                    model.Description ?? "افزایش اعتبار سریع"
                );

                var newBalance = await _walletService.GetBalanceAsync(userId);

                return Json(new
                {
                    success = true,
                    message = $"مبلغ {model.Amount.ToString("N0")} تومان با موفقیت به کیف پول شما اضافه شد.",
                    trackingCode = trackingCode,
                    newBalance = newBalance
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddCredit: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = $"خطا در افزایش موجودی: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> FakePayment([FromBody] FakePaymentViewModel model)
        {
            if (model.Amount < 1000 || model.Amount > 10000000)
            {
                return Json(new
                {
                    success = false,
                    message = "مبلغ باید بین ۱,۰۰۰ تا ۱۰,۰۰۰,۰۰۰ تومان باشد"
                });
            }

            var userIdStr = getUserIdStr();
            if (!int.TryParse(userIdStr, out int userId))
                return Json(new { success = false, message = "کاربر یافت نشد" });
            await Task.Delay(1500);

            var rnd = new Random();
            var isSuccess = rnd.Next(0, 100) < 90; 

            if (!isSuccess)
            {
                return Json(new
                {
                    success = false,
                    message = "پرداخت ناموفق بود. لطفا مجدداً تلاش کنید.",
                    paymentId = $"FAIL-{DateTime.Now:yyyyMMddHHmmss}"
                });
            }

            try
            {
                await _walletService.AddCreditAsync(userId, model.Amount);
                var newBalance = await _walletService.GetBalanceAsync(userId);

                return Json(new
                {
                    success = true,
                    message = "پرداخت با موفقیت انجام شد.",
                    paymentId = $"PAY-{DateTime.Now:yyyyMMddHHmmss}",
                    amount = model.Amount,
                    newBalance = newBalance
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"خطا در پردازش پرداخت: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWalletBalance()
        {
            var userIdStr = getUserIdStr();
            if (!int.TryParse(userIdStr, out int userId))
                return Json(new { success = false, balance = 0m });

            var balance = await _walletService.GetBalanceAsync(userId);
            return Json(new { success = true, balance = balance });
        }




    }
}
