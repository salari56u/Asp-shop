using DigiStore.Data;
using DigiStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }



        
        public async Task<IActionResult> Index(string search, int? categoryId, long? minPrice, long? maxPrice, string sortBy = "date", int page = 1)
        {
            int pageSize = 12;

            var p1 = _context.Products
                .Include(p => p.Images)
                .Include(p => p.ProductCategories)
                .ThenInclude(p => p.Category)
                .AsQueryable();

            if(!string.IsNullOrWhiteSpace(search))
            {
                p1 = p1.Where(p=> p.Title.Contains(search)|| p.Description.Contains(search));
            }

            if(categoryId.HasValue)
            {
                 p1= p1.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId));
            }
            if (minPrice.HasValue)
            {
                p1 = p1.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                p1 = p1.Where(p => p.Price <= maxPrice.Value);
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

            // 5. پر کردن ویومدل
            var model = new ShopViewModel
            {
                Products = products,
                Categories = await _context.Categories.ToListAsync(),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Search = search,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy
            };


            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Partials/_ProductGrid",model);
            }


            return View(model);

        }







        // اکشن نمایش جزئیات محصول
        // مثال: /Product/Details/1
        [Route("Product/Details/{id}/{slug?}")] // اسلاگ برای سئو اختیاری است
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return NotFound();

            var product = await _context.Products
                .Include(p=>p.Images)
                .Include(p=>p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }
    }
}