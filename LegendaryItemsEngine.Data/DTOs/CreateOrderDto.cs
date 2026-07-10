using  LegendaryItemsEngine.Data.Models;

namespace LegendaryItemsEngine.Data.DTOs;

//DTO para poder simplificar los datos despues en Swagger
public class CreateOrderDto
{
    public int PlayerId { get; set; }
    public OrderPriority Priority { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}