using System.ComponentModel.DataAnnotations;

public class Address
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverPhone { get; set; }
    public string Province { get; set; }
    public string City { get; set; }
    public string FullAddress { get; set; }
    public string PostalCode { get; set; }
}
