using System.ComponentModel.DataAnnotations;

public class Payment
{
    [Key]
    public int Id { get; set; }
    public string Provider { get; set; }
    public decimal Amount { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.Now;
    public string? TransactionId { get; set; }
    public string? RefId { get; set; }
    public string? Description { get; set; }
}