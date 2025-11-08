using System.Collections.Generic;

public class Category
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category ParentCategory { get; set; }
    public ICollection<Category> Children { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; }
}
