using DigiStore.Models; 

namespace DigiStore.ViewModels
{
    public class HomeViewModel
    {
 
        public List<Slider> Sliders { get; set; }

 
        public List<Category> Categories { get; set; }

 
        public List<Product> MobileProducts { get; set; }

 
        public List<Product> AccessoryProducts { get; set; }


        public List<Product> LaptopProducts { get; set; }


        public List<Product> BestSellingProducts { get; set; }

        public List<Product> LatestProducts { get; set; }

        public Banner WideBanner { get; set; }
    }
}