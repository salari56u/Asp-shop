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
            var vm = new HomeViewModel();

            vm.Sliders = await _context.Sliders
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            
            vm.Categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();

            vm.MobileProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.ProductCategories.Any(pc => pc.CategoryId == 12))
                .OrderByDescending(p => p.CreatedAt) 
                .Take(8)
                .ToListAsync();

            
            vm.LaptopProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.ProductCategories.Any(pc => pc.CategoryId == 11))
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            
            var accessoryIds = new List<int> { 14, 15, 17, 18, 20 };

            vm.AccessoryProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.ProductCategories.Any(pc => accessoryIds.Contains(pc.CategoryId)))
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            vm.BestSellingProducts = await _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Take(12)
                .ToListAsync();


            vm.LatestProducts = await _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .ToListAsync();

            vm.WideBanner = await _context.Banners
                .AsNoTracking()
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
