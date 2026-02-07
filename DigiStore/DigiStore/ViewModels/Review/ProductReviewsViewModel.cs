public class ProductReviewsViewModel
{
    public int ProductId { get; set; }
    public string ProductTitle { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<ReviewViewModel> Reviews { get; set; }
    public bool HasUserReviewed { get; set; }
    public ReviewViewModel UserReview { get; set; }
    public bool IsLoggedIn { get; set; }

    public string GetTimeAgo(DateTime date)
    {
        var timeSpan = DateTime.Now - date;

        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays} روز پیش";
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours} ساعت پیش";

        return $"{(int)timeSpan.TotalMinutes} دقیقه پیش";
    }
}