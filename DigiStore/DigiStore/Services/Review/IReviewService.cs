public interface IReviewService
{
    Task<List<Review>> GetProductReviewsAsync(int productId);
    Task<Review> AddReviewAsync(int productId, int userId, int rating, string comment);
    Task<bool> HasUserReviewedAsync(int productId, int userId);
    Task<double> GetProductAverageRatingAsync(int productId);
    Task<int> GetProductReviewCountAsync(int productId);

    Task<Review> UpdateReviewAsync(int reviewId, int userId, int? newRating = null, string newComment = null);
    Task<bool> DeleteReviewAsync(int reviewId, int userId);
    Task<bool> CanUserDeleteReviewAsync(int reviewId, int userId);
    Task<Review> GetReviewByIdAsync(int reviewId);
}