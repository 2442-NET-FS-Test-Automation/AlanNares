using LegendaryItemsEngine.Data.Models;

namespace LegendaryItemsEngine.Data.Repositories;

public interface IInventoryRepository
{
    // Obtiene el inventario de un ítem específico para revisar su stock y RowVersion
    //We get the Inventary from an specific item to see the stock and RowVersion
    Task<ItemInventory?> GetByItemIdAsync(int itemId);
    
    // Intenta reducir el stock de un ítem. Devolverá true si lo logra, o false si no hay suficiente stock
    Task<bool> TryDeductStockAsync(int itemId, int quantity);
}