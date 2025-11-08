using System.Collections.Generic;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public ICollection<CartItem> Items { get; set; }
}
