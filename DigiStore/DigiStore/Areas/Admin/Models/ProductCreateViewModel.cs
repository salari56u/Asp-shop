public class ProductCreateViewModel
{
    public int? Id { get; set; }

    public string Title { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }

    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int Stock { get; set; }

    public IFormFile? MainImage { get; set; }

    public List<int> SelectedCategories { get; set; } = new();
    public List<Category> Categories { get; set; } = new();

    public List<IFormFile>? GalleryImages { get; set; }
    public List<ProductImage> ExistingImages { get; set; } = new();

    public List<string>? SpecKeys { get; set; } = new();
    public List<string>? SpecValues { get; set; } = new();

}
