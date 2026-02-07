namespace DigiStore.ViewModels
{
    public class DetailsViewModel
    {
        public Product product { get; set; }

        public bool isLiking { get; set; }


        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<ReviewViewModel> Reviews { get; set; }
        public bool HasUserReviewed { get; set; }
        public ReviewViewModel UserReview { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
