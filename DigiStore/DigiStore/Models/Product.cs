using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Collections.Generic;

public class Product
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public ICollection<ProductImage> Images { get; set; }
    public ICollection<ProductSpecification> Specifications { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; }
    public ICollection<Review> Reviews { get; set; }
}
