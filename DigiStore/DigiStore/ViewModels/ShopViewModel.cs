public class ShopViewModel
{
    public  List<Product> Products { get; set; }

    public List<Category> Categories { get; set; }


    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    // پارامترهای فیلتر که از سمت کاربر می‌آید
    public string Search { get; set; }
    public int? CategoryId { get; set; }
    public long? MinPrice { get; set; }
    public long? MaxPrice { get; set; }
    public string SortBy { get; set; }
}
