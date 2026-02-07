using DigiStore.Data;
using Microsoft.EntityFrameworkCore;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    private const int MAX_HOURS_FOR_DELETE = 24;
    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Review>> GetProductReviewsAsync(int productId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review> AddReviewAsync(int productId, int userId, int rating, string comment)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("امتیاز باید بین 1 تا 5 باشد");
        var existing = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

        if (existing != null)
            throw new InvalidOperationException("شما قبلا برای این محصول نظر داده‌اید");

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.Now
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return review;
    }

    public async Task<bool> HasUserReviewedAsync(int productId, int userId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId);
    }

    public async Task<double> GetProductAverageRatingAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        if (!reviews.Any())
            return 0;

        return Math.Round(reviews.Average(r => r.Rating), 1);
    }

    public async Task<int> GetProductReviewCountAsync(int productId)
    {
        return await _context.Reviews
            .CountAsync(r => r.ProductId == productId);
    }

    public async Task<Review> GetReviewByIdAsync(int reviewId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == reviewId);
    }

    public async Task<Review> UpdateReviewAsync(int reviewId, int userId,
                                                int? newRating = null,
                                                string newComment = null)
    {
        var review = await _context.Reviews.FindAsync(reviewId);

        if (review == null)
            throw new ArgumentException("نظر یافت نشد");

        if (review.UserId != userId)
            throw new UnauthorizedAccessException("شما مجاز به ویرایش این نظر نیستید");
        if (newRating.HasValue)
        {
            if (newRating.Value < 1 || newRating.Value > 5)
                throw new ArgumentException("امتیاز باید بین 1 تا 5 باشد");
            review.Rating = newRating.Value;
        }
        if (!string.IsNullOrEmpty(newComment))
        {
            if (newComment.Length < 10)
                throw new ArgumentException("نظر باید حداقل ۱۰ کاراکتر باشد");
            if (newComment.Length > 1000)
                throw new ArgumentException("نظر نمی‌تواند بیش از ۱۰۰۰ کاراکتر باشد");
            review.Comment = newComment;
        }

        review.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return review;
    }
    public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);

        if (review == null) return false;

        if (review.UserId != userId) return false;
        if (!await CanUserDeleteReviewAsync(reviewId, userId))
            return false;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<bool> CanUserDeleteReviewAsync(int reviewId, int userId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);

        if (review == null || review.UserId != userId)
            return false;
        var timeDifference = DateTime.Now - review.CreatedAt;
        return timeDifference.TotalHours <= MAX_HOURS_FOR_DELETE;
    }
}