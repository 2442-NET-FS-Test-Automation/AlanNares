using System.ComponentModel.DataAnnotations;

namespace LegendaryItemsEngine.Data.Models;

public class FulfillmentEvent
{
    public int Id { get; set; }

    [Required]
    public int ItemOrderId { get; set; }
    
    // Propiedad de navegación a ItemOrder / a uno
    public ItemOrder? Order { get; set; }

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // Ej: "ConcurrencyRetry", "FulfillmentSuccess"

    [Required]
    [MaxLength(250)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}