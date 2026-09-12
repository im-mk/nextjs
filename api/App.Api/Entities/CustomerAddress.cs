public class CustomerAddress
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int AddressId { get; set; }
    public string AddressType { get; set; } = default!; // e.g., "Billing", "Shipping"
}