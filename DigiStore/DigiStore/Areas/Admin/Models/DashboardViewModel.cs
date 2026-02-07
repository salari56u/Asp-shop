using DigiStore.Models; 

namespace DigiStore.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {

        public int UsersCount { get; set; }
        public int ProductsCount { get; set; }
        public int TotalOrders { get; set; } 
        public decimal TotalRevenue { get; set; }


        public List<Product> LowStockProducts { get; set; } 


        public List<User> RecentUsers { get; set; }
    }
}