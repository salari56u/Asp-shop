using DigiStore.Data;
using DigiStore.Models;
using DigiStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DigiStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IReviewService _reviewService;

        public ProductController(AppDbContext context , IHttpContextAccessor httpContextAccessor,IReviewService reviewService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _reviewService = reviewService;
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






        [Route("Product/Details/{id}/{slug?}")]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return NotFound();

            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var httpContext = _httpContextAccessor.HttpContext;
            var userIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reviews = await _reviewService.GetProductReviewsAsync(id);
            var averageRating = await _reviewService.GetProductAverageRatingAsync(id);
            var reviewCount = await _reviewService.GetProductReviewCountAsync(id);

            bool hasUserReviewed = false;
            Review userReview = null;
            int userId = 0;

            if (int.TryParse(userIdStr, out userId))
            {
                hasUserReviewed = await _reviewService.HasUserReviewedAsync(id, userId);
                userReview = reviews.FirstOrDefault(r => r.UserId == userId);
            }

            var viewModel = new DetailsViewModel
            {
                product = product,
                AverageRating = averageRating,
                ReviewCount = reviewCount,
                Reviews = reviews.Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User?.UserName ?? "کاربر",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    IsCurrentUserReview = r.UserId == userId
                }).ToList(),
                HasUserReviewed = hasUserReviewed,
                IsLoggedIn = httpContext.User.Identity?.IsAuthenticated ?? false
            };

            ViewData["AverageRating"] = averageRating;
            ViewData["ReviewCount"] = reviewCount;
            ViewData["Reviews"] = reviews.Select(r => new ReviewViewModel
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User?.UserName ?? "کاربر",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                IsCurrentUserReview = r.UserId == userId
            }).ToList();
            ViewData["HasUserReviewed"] = hasUserReviewed;
            ViewData["IsLoggedIn"] = httpContext.User.Identity?.IsAuthenticated ?? false;


            return View(viewModel);
        }


        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview([FromBody] AddReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "داده‌های ورودی نامعتبر است"
                });
            }

            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out int userId))
                    return Json(new { success = false, message = "کاربر یافت نشد" });

                var review = await _reviewService.AddReviewAsync(
                    model.ProductId,
                    userId,
                    model.Rating,
                    model.Comment
                );

                return Json(new
                {
                    success = true,
                    message = "نظر شما با موفقیت ثبت شد",
                    reviewId = review.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReview([FromBody] UpdateReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "داده‌های ورودی نامعتبر است"
                });
            }

            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out int userId))
                    return Json(new { success = false, message = "کاربر یافت نشد" });

                var review = await _reviewService.UpdateReviewAsync(
                    model.ReviewId,
                    userId,
                    model.Rating,
                    model.Comment
                );

                return Json(new
                {
                    success = true,
                    message = "نظر شما با موفقیت ویرایش شد",
                    review = new
                    {
                        id = review.Id,
                        rating = review.Rating,
                        comment = review.Comment,
                        updatedAt = review.UpdatedAt?.ToString("yyyy/MM/dd HH:mm")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview([FromBody] DeleteReviewViewModel model)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out int userId))
                    return Json(new { success = false, message = "کاربر یافت نشد" });

                var result = await _reviewService.DeleteReviewAsync(model.ReviewId, userId);

                if (!result)
                {
                    return Json(new
                    {
                        success = false,
                        message = "امکان حذف این نظر وجود ندارد. ممکن است زمان حذف گذشته باشد یا شما مجوز لازم را نداشته باشید."
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "نظر شما با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}