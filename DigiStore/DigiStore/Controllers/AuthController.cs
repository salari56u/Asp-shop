using DigiStore.Data;
using DigiStore.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace DigiStore.Controllers
{
    public class AuthController : Controller
    {
        private readonly ISmsService _msService;

        private readonly IMemoryCache _memoryCache;
        private readonly AppDbContext _appDbContext;

        public AuthController(ISmsService smsService,IMemoryCache memoryCache,AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _msService = smsService;
            _memoryCache = memoryCache;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Login(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || !phoneNumber.StartsWith("09"))
            {
                ViewBag.Error = "شماره موبایل نامعتبر است";
                return View();
            }


            var otpCode=new Random().Next(10000,99999).ToString();

            var messageBody = $"کد تایید شما در دیجی استور: {otpCode}";
            await _msService.SendSmsAsync(phoneNumber, messageBody);

            _memoryCache.Set(phoneNumber,otpCode,TimeSpan.FromMinutes(2));

            return RedirectToAction("Verify", new { mobile = phoneNumber });
        }

        [HttpGet]
        public IActionResult Verify(string mobile)
        {
            ViewBag.Mobile = mobile;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Verify(string mobile,string code)
        {
            if(!_memoryCache.TryGetValue(mobile,out string currectCode))
            {
                ViewBag.Error = "کد منقضی شده است. لطفا مجدد تلاش کنید.";
                return View("Login");
            }


            if(code !=currectCode)
            {
                ViewBag.Mobile = mobile;
                ViewBag.Error = "کد وارد شده اشتباه است.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, mobile),
                new Claim(ClaimTypes.MobilePhone, mobile),
                new Claim(ClaimTypes.Role, "Customer") // فعلا نقش مشتری
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _memoryCache.Remove(mobile);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
