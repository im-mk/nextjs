namespace App.Api.Models;

public class CreateOrderRequest
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public AddressRequest ShippingAddress { get; set; } = default!;
    public AddressRequest BillingAddress { get; set; } = default!;
    public List<OrderItem> OrderLines { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
