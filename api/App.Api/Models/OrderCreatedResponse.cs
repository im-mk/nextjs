namespace App.Api.Models;

public class OrderCreatedResponse
{
    public OrderCreatedResponse(int orderId, string orderNumber)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
    }

    public int OrderId { get; private set; }
    public string OrderNumber { get; private set; }
}
