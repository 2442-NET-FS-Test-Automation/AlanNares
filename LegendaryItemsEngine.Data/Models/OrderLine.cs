using System.ComponentModel.DataAnnotations;

namespace LegendaryItemsEngine.Data.Models;

public class OrderLine
{
    public int Id {get; set;}

    [Required]
    public int ItemId {get; set;}
    // Propiedad de navegación al ítem legendario comprado
    public Item? Item { get; set; }

    [Required]
    public int ItemOrderId {get; set;}

    public ItemOrder? Order { get; set; }    // Propiedad de navegación inversa

    [Required]
    public int Quantity {get; set;}
}

