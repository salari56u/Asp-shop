

public class Cart
{

    public Cart()
    {
        Items = new List<CartItem>();
    }

    public int Id { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }


    public Guid? GuestId { get; set; }

    public decimal TotalPrice { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;


    public ICollection<CartItem> Items { get; set; }
}
