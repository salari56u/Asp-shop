using DigiStore.Data;
using DigiStore.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : BaseAdminController 
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            var dashboardData = new DashboardViewModel
            {
  
                UsersCount = await _context.Users.CountAsync(),


                ProductsCount = await _context.Products.CountAsync(),


                LowStockProducts = await _context.Products
                                            .Where(p => p.Stock < 5)
                                            .OrderBy(p => p.Stock)
                                            .Take(5)
                                            .ToListAsync(),

     
                RecentUsers = await _context.Users
                                        .OrderByDescending(u => u.Id) 
                                        .Take(5)
                                        .ToListAsync()
            };



            return View(dashboardData);
        }
    }
}