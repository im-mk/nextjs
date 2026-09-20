namespace App.Api.Dto.Orders;

public class OrderCreatedResponse(int orderId, string orderNumber)
{
    public int OrderId { get; private set; } = orderId;
    public string OrderNumber { get; private set; } = orderNumber;
}