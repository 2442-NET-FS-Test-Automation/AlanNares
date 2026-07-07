using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LegendaryItemsEngine.Data.Models;

public class Item
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string SKU { get; set; } = string.Empty; 

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    //public string Name { get; set; } = default!;

    [Required]
    [Precision(10, 2)]
    public decimal Price { get; set; }

    // Relación 1:1 con el Inventario
    public ItemInventory? Inventory { get; set; }
}