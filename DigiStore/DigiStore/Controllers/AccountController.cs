using DigiStore.Data;
using DigiStore.Models;
using DigiStore.Services.CartServices;
using DigiStore.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace DigiStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ISmsService _smsService;
        private readonly IMemoryCache _cache;
        private readonly ICartService _cartService;

        public AccountController(AppDbContext context, ISmsService smsService, IMemoryCache cache, ICartService cartService)
        {
            _cache = cache;
            _context = context;
            _smsService = smsService;
            _cartService = cartService;
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length != 11 || !phoneNumber.StartsWith("09"))
            {
                return Json(new { success = false, message = "شماره موبایل معتبر نیست." });
            }
            //var otpCode = new Random().Next(1000, 9999).ToString();

            var otpCode = "1111";

            _cache.Set(phoneNumber, otpCode, TimeSpan.FromMinutes(2));
            //string message = $"کد ورود شما به موبولند: {otpCode}";

           // await _smsService.SendSmsAsync(phoneNumber, message);

            return Json(new { success = true, message = "کد تایید ارسال شد." });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string phoneNumber, string code)
        {
            if (!_cache.TryGetValue(phoneNumber, out string? cachedCode) || cachedCode != code)
            {
                return Json(new { success = false, message = "کد نامعتبر یا منقضی شده است." });
            }



            var user = await _context.Users
                .Include(u=>u.UserRoles)
                .ThenInclude(u=>u.Role)
                .FirstOrDefaultAsync(u => u.UserName == phoneNumber);

            if (user == null)
            {
                var customerRole=await _context.Roles.FirstOrDefaultAsync(r=>r.Name=="Customer");
                if(customerRole==null)
                {
                    customerRole = new Role { Name = "Customer" };
                    _context.Roles.Add(customerRole);
                    await _context.SaveChangesAsync();
                }
                user = new User
                {
                    UserName = phoneNumber,
                    Email = phoneNumber + "@moboland.ir",
                    PasswordHash = "OTP-USER",
                    UserRoles = new List<UserRole>
                    {
                        new UserRole
                        {
                            RoleId=customerRole.Id
                        }
                    }
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }



            var hasWallet = await _context.Wallets.AnyAsync(w => w.UserId == user.Id);
            if (!hasWallet)
            {
                _context.Wallets.Add(new Wallet
                {
                    UserId = user.Id,
                    Balance = 0
                });
                await _context.SaveChangesAsync();
            }


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.MobilePhone, phoneNumber)
            };

            foreach (var ur in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, ur.Role.Name));
            }

            var identity = new ClaimsIdentity(claims, "UserScheme");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("UserScheme", principal);

            if (Request.Cookies.TryGetValue(CartService.CartCookieName, out string guestIdStr))
            {
                if (Guid.TryParse(guestIdStr, out Guid guestId))
                {
                    await _cartService.MergeCartsAsync(user.Id, guestId);
                    Response.Cookies.Delete(CartService.CartCookieName);
                }
            }

            _cache.Remove(phoneNumber);

            return Json(new { success = true, message = "خوش آمدید!" });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("UserScheme");
            return RedirectToAction("Index", "Home");
        }
    }
}