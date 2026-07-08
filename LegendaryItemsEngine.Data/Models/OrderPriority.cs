using System.ComponentModel.DataAnnotations;

namespace LegendaryItemsEngine.Data.Models;

//Instructor Jonathan did a New Folder in Data, something like Data.Enums
//and there was where he put this two enums.
//OrderPriority who tell us if a "Order" was completed - Number 1 max prio.
//Jhon uses Normal = 0 instead of our Normal = 2
public enum OrderPriority 
{
    Normal = 2,
    Expedited = 1
}

//Same comments we saw in class with Jhon
    // In my application if an order is yet to be processed it is pending.
    // Fulfilled means the sale completed
    // Backorder happens when someone places a buy request we don't have stock for 
public enum OrderStatus
{
    Pending,
    Fulfilled,
    Backordered //No stock available
}

public class ItemOrder
{
    public int Id { get; set; }

    [Required]
    public int PlayerId { get; set; }
    
    // Propiedad de navegación al jugador que compró
    public Player? Player { get; set; }

    [Required]
    public OrderPriority Priority { get; set; }

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    // Relacion a "uno" a OrderLine / Line
    public OrderLine? Line { get; set; }

    // Relacion a "muchos" Events o FullfillmentEvents
    public ICollection<FulfillmentEvent> Events { get; set; } = new List<FulfillmentEvent>();
}