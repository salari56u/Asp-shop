using DigiStore.Data;
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

        public AccountController(AppDbContext context,ISmsService smsService,IMemoryCache cache)
        {
            _cache = cache;
            _context = context;
            _smsService = smsService;
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length != 11 || !phoneNumber.StartsWith("09"))
            {
                return Json(new { success = false, message = "شماره موبایل معتبر نیست." });
            }
            var otpCode = new Random().Next(1000, 9999).ToString();

            _cache.Set(phoneNumber, otpCode, TimeSpan.FromMinutes(2));
            string message = $"کد ورود شما به موبولند: {otpCode}";

            // Uncomment line below to send real SMS
             await _smsService.SendSmsAsync(phoneNumber, message);

            //Console.WriteLine($"--- OTP for {phoneNumber}: {otpCode} ---");

            return Json(new { success = true, message = "کد تایید ارسال شد." });
        }
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string phoneNumber, string code)
        {
            if (!_cache.TryGetValue(phoneNumber, out string? cachedCode))
            {
                return Json(new { success = false, message = "کد تایید منقضی شده است. مجددا تلاش کنید." });
            }

            if (cachedCode != code)
            {
                return Json(new { success = false, message = "کد وارد شده اشتباه است." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == phoneNumber); 

            if (user == null)
            {
                user = new User
                {
                    UserName = phoneNumber,
                    Email = phoneNumber + "@moboland.ir", 
                    PasswordHash = "OTP-USER", 
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.MobilePhone, phoneNumber)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            _cache.Remove(phoneNumber);

            return Json(new { success = true, message = "خوش آمدید!" });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
