using System;

public class Payment
{
    public int Id { get; set; }
    public string Provider { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public string TransactionId { get; set; }
}
