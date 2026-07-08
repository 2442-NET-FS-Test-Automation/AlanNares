using Microsoft.EntityFrameworkCore;
using LegendaryItemsEngine.Data.Models;
using Serilog;

namespace LegendaryItemsEngine.Data.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly LegendaryItemsDbContext _context;

    // El guion bajo conecta el contexto privado de la clase de manera interna
    public InventoryRepository(LegendaryItemsDbContext context)
    {
        _context = context;
    }

    public async Task<ItemInventory?> GetByItemIdAsync(int itemId)
    {
        return await _context.ItemInventories
            .FirstOrDefaultAsync(inv => inv.ItemId == itemId);
    }

    public async Task<bool> TryDeductStockAsync(int itemId, int quantity)
    {
        // We check inventary of an item
        var inventory = await GetByItemIdAsync(itemId);
        
        if (inventory == null)
        {
            Log.Warning("Inventory check failed: Item {ItemId} does not exist in stock registry.", itemId);
            return false;
        }

        // Check if we have stock
        if (inventory.QuantityOnHand < quantity)
        {
            Log.Warning("Fulfillment rejected: Insufficient stock for Item {ItemId}. Requested: {Requested}, Available: {Available}.", 
                itemId, quantity, inventory.QuantityOnHand);
            return false;
        }

        // Modificamos el stock en memoria temporalmente
        inventory.QuantityOnHand -= quantity;

        try
        {
            // Try to apply changes in SQL Server
            // We check RowVersion, just to be shure nobody else has made changes
            // If row version is different SQL Server reject the operation
            await _context.SaveChangesAsync();
            
            Log.Information("Successfully deducted {Quantity} units from Item {ItemId}. New stock: {NewStock}.", 
                quantity, itemId, inventory.QuantityOnHand);
                
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // We catch de exception when someone tried to change the row in the same time
            Log.Error(ex, "Concurrency conflict detected for Item {ItemId}. Another process updated the row simultaneously.", itemId);
            
            ex.Entries.Single().Reload();
            
            throw; // retry exception
        }
    }
}