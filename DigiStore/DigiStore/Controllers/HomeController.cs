using DigiStore.Data;
using DigiStore.Models;
using DigiStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DigiStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
             var vm=new HomeViewModel();


            //بخش اسلایدر
            vm.Sliders=await _context.Sliders
                .Where(s=>s.IsActive)
                .OrderBy(s=>s.SortOrder)
                .ToListAsync();

            //بخش کتگوری ها
            vm.Categories = await _context.Categories
                .ToListAsync();


            //بخش موبایل --فعلا بر اساس فیلتر میکنم چون هنوز دسته بندی درختی ندارم
            vm.MobileProducts = await _context.Products
                .Where(p => p.Title.Contains("گوشی") || p.Title.Contains("موبایل"))
                .OrderBy(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            //دریافت لوازم جانبی 
            vm.AccessoryProducts=await _context.Products
                .Where(p => p.Title.Contains("هدفون") || p.Title.Contains("ساعت") || p.Title.Contains("پاوربانک") || p.Title.Contains("شارژر") || p.Title.Contains("دسته بازی"))
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();



            // بخش لپ تاپ ها

            vm.LaptopProducts = await _context.Products
                .Where(p => p.Title.Contains("لپ تاپ"))
                .OrderBy(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            //  دریافت پرفروش‌ها (فعلا ۱۰ محصول آخر رو می‌گیرم)
            vm.BestSellingProducts = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();


            //بخش اخرین محصولات
            vm.LatestProducts = await _context.Products
                                       .OrderByDescending(p => p.CreatedAt)
                                       .Take(6)
                                       .ToListAsync();

            //دریافت بنر
            vm.WideBanner = await _context.Banners
                .FirstOrDefaultAsync(b => b.Position == "CenterWide" && b.IsActive);


            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
