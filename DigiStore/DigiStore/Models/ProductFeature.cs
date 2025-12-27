namespace DigiStore.Models
{
    public class ProductFeature
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }      
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}