using DigiStore.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DigiStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private const string AdminScheme = "AdminScheme";
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var result = await HttpContext.AuthenticateAsync(AdminScheme);

            if (result.Succeeded && result.Principal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null || user.PasswordHash != password)
            {
                ViewBag.Error = "نام کاربری یا کلمه عبور اشتباه است.";
                return View();
            }

            var isAdmin = user.UserRoles.Any(ur => ur.Role.Name == "Admin" || ur.Role.Name == "SuperAdmin");
            if (!isAdmin)
            {
                ViewBag.Error = "شما دسترسی ورود به پنل مدیریت را ندارید.";
                return View();
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.MobilePhone, user.UserName) 
            };


            foreach (var ur in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, ur.Role.Name));
            }

            var identity = new ClaimsIdentity(claims, AdminScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(AdminScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(AdminScheme);
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}