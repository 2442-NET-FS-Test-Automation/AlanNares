using System.ComponentModel.DataAnnotations;

namespace LegendaryItemsEngine.Data.Models;

public class ItemInventory
{
    public int Id { get; set; }

    [Required]
    public int ItemId { get; set; }
    
    // Propiedad de navegación a ITEM
    public Item? Item { get; set; }

    [Required]
    public int QuantityOnHand { get; set; } // Available stock

    // Shield who prevents overselling and Null data. Version Control of the data
    //Timestamp don't refer to time in SQLServer... its a unique binary identifier
    //Optimistic Concurrency Control
    //Thats why whe specifie that RowVersion must be Timestamp
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    //public byte[] RowVersion2 { get; set; } = default!;
}