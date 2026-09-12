namespace App.Api.Models;

public class OrderList
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public DateTime OrderDate { get; set; }
    public int OrderStatusId { get; set; }
    public decimal TotalAmount { get; set; }
    public int CustomerId { get; set; }
}