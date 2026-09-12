namespace App.Api.Models;

public class UpdateOrderRequest
{
    public int OrderStatusId { get; set; }
    public decimal TotalAmount { get; set; }
}
