using System.Collections.Generic;
using System.Linq;

namespace DigiStore.ViewModels
{

    public class CartItemViewModel
    {
        public int Id { get; set; }       
        public int ProductId { get; set; }
        public string Title { get; set; }
        public string ImageName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal TotalPrice => Price * Quantity;
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        public decimal GrandTotal => Items.Sum(i => i.TotalPrice);

        public int TotalCount => Items.Sum(i => i.Quantity);
    }
}