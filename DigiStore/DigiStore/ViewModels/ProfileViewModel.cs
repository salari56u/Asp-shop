public class ProfileViewModel
{
    public string UserName { get; set; }
    public string Mobile { get; set; }
    public decimal WalletBalance { get; set; }

    public List<UserOrderViewModel> Orders { get; set; }
}
