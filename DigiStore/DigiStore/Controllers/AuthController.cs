using DigiStore.Data;
using DigiStore.Models;
using DigiStore.Services.CartServices;
using DigiStore.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DigiStore.Controllers
{
    public class AuthController : Controller
    {
        private readonly ISmsService _smsService;
        private readonly IMemoryCache _memoryCache;
        private readonly AppDbContext _context;
        private readonly ICartService _cartService;

        public AuthController(ISmsService smsService, IMemoryCache memoryCache, AppDbContext context, ICartService cartService)
        {
            _context = context;
            _smsService = smsService;
            _memoryCache = memoryCache;
            _cartService = cartService;
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

            //var otpCode = new Random().Next(10000, 99999).ToString();

            var otpCode = "1111";

            // var messageBody = $"کد تایید شما در دیجی استور: {otpCode}";
            //  await _smsService.SendSmsAsync(phoneNumber, messageBody);

            _memoryCache.Set(phoneNumber, otpCode, TimeSpan.FromMinutes(2));

            return RedirectToAction("Verify", new { mobile = phoneNumber });
        }

        [HttpGet]
        public IActionResult Verify(string mobile)
        {
            ViewBag.Mobile = mobile;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Verify(string mobile, string code)
        {
            // 1. بررسی کش
            if (!_memoryCache.TryGetValue(mobile, out string currectCode))
            {
                ViewBag.Error = "کد منقضی شده است.";
                return View("Login");
            }

            if (code != currectCode)
            {
                ViewBag.Mobile = mobile;
                ViewBag.Error = "کد اشتباه است.";
                return View();
            }
            Guid? guestIdToMerge = null;
            if (Request.Cookies.TryGetValue("DigiStore_GuestId", out string guestIdStr))
            {
                if (Guid.TryParse(guestIdStr, out Guid parsedId))
                {
                    guestIdToMerge = parsedId;
                }
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == mobile);
            if (user == null)
            {
                user = new User
                {
                    UserName = mobile,
                    Email = mobile + "@moboland.ir",
                    PasswordHash = "OTP-USER",
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync(); 
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, mobile),
                new Claim(ClaimTypes.MobilePhone, mobile),
                new Claim(ClaimTypes.Role, "Customer")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "UserScheme");
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(
                 "UserScheme",
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (guestIdToMerge.HasValue)
            {
                try
                {
                    await _cartService.MergeCartsAsync(user.Id, guestIdToMerge.Value);

                    Response.Cookies.Delete("DigiStore_GuestId");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Merge Error: " + ex.Message);
                }
            }

            _memoryCache.Remove(mobile);
            return RedirectToAction("Index", "home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}