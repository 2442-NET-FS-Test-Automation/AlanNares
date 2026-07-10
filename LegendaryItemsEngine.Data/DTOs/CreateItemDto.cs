namespace LegendaryItemsEngine.Data.DTOs;

public class CreateItemDto
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int InitialStock { get; set; }
}