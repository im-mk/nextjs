using App.Api.Dto.Addresses;

namespace App.Api.Dto.Orders;

public class CreateOrderRequest
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public AddressRequest ShippingAddress { get; set; } = default!;
    public AddressRequest BillingAddress { get; set; } = default!;
    public List<OrderItem> OrderLines { get; set; } = [];
}

public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}