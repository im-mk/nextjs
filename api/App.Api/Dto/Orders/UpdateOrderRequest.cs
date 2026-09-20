namespace App.Api.Dto.Orders;

public class UpdateOrderRequest
{
    public int OrderStatusId { get; set; }
    public decimal TotalAmount { get; set; }
}