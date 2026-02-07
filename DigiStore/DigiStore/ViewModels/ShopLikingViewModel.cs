public class ShopLikingViewModel
{
    public List<Product> Products { get; set; }



    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string SortBy { get; set; }

   public bool IsLoggedIn { get; set; }
}