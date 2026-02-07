namespace DigiStore.Data
{
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        Failed = 2
    }
}