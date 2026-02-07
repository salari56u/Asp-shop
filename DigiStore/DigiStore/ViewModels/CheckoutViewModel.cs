using DigiStore.Models;
using System.Collections.Generic;

namespace DigiStore.ViewModels
{
    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; }
        public List<Address> UserAddresses { get; set; }
        public int? SelectedAddressId { get; set; }
    }
}