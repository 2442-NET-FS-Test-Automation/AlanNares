using Microsoft.AspNetCore.Mvc;
using LegendaryItemsEngine.Data;
using LegendaryItemsEngine.Data.Models;
using LegendaryItemsEngine.Data.DTOs;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace LegendaryItemsEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly LegendaryItemsDbContext _context;

    public InventoryController(LegendaryItemsDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// We can create a new Item 
    /// </summary>
    /// <response code="200">New Item added successfully</response>
    // Endpoint: Create new item 
    [HttpPost("create-item")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto dto, CancellationToken ct)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.SKU) || string.IsNullOrWhiteSpace(dto.Name) 
            || dto.InitialStock < 0 || dto.Price < 0)
        {
            return BadRequest("Invalid Fields.");
        }

        // Creamos la entidad del Ítem
        var newItem = new Item
        {
            SKU = dto.SKU,
            Name = dto.Name,
            Price = dto.Price
        };

        // Creamos su inventario asociado (Relación 1 a 1)
        var newInventory = new ItemInventory
        {
            Item = newItem,
            QuantityOnHand = dto.InitialStock
        };

        // Agregamos al contexto de Entity Framework
        _context.Items.Add(newItem);
        _context.ItemInventories.Add(newInventory);

        await _context.SaveChangesAsync(ct);

        Log.Information("=== CREATE-ITEM: New Legendary Item created: {Name} with {Stock} units ===", dto.Name, dto.InitialStock);

        return Ok(new { Message = $"Ítem '{dto.Name}' created with ID {newItem.Id}." });
    }

    /// <summary>
    /// Here we can update our Stock from inventory with an ItemId and setting newStock
    /// </summary>
    /// <response code="200">We updated our stock from the ItemId you give.</response>
    [HttpPut("update-stock")]
    public async Task<IActionResult> UpdateStock([FromBody] UpdateStockDto dto, CancellationToken ct)
    {
        if (dto == null || dto.NewStock < 0)
        {
            return BadRequest("Datos inválidos o stock menor a cero.");
        }

        var inventory = await _context.ItemInventories
            .FirstOrDefaultAsync(i => i.ItemId == dto.ItemId, ct);

        if (inventory == null)
        {
            return NotFound($"Couldn't find item with ID: {dto.ItemId}.");
        }

        // Update stock
        inventory.QuantityOnHand = dto.NewStock;
        
        await _context.SaveChangesAsync(ct);

        Log.Information("=== UPDATE STOCK: Stock for Item {ItemId} manually updated to {NewStock} ===", dto.ItemId, dto.NewStock);

        return Ok(new { Message = $"Stock del Ítem {dto.ItemId} actualizado a {dto.NewStock} unidades." });
    }

    /// <summary>
    /// Performs a live audit of the inventory system to ensure data integrity.
    /// </summary>
    /// <response code="200">Returns the full inventory status and confirms no items have negative stock.</response>
    [HttpGet("verify-no-oversell")]
    public async Task<IActionResult> VerifyNoOversell(CancellationToken ct)
    {
        //Check all inventory registries including items
        var inventories = await _context.ItemInventories
            .Include(i => i.Item)
            .ToListAsync(ct);

        //We check with LINQ if we have Quantity in stock that is less than 0
        var negativeStockItems = inventories
            .Where(i => i.QuantityOnHand < 0)
            .Select(i => new { i.ItemId, ItemName = i.Item?.Name, i.QuantityOnHand })
            .ToList();

        return Ok(new
        {
            AnyNegativeStock = negativeStockItems.Any(),
            NegativeItemsCount = negativeStockItems.Count,
            CurrentInventoryStatus = inventories.Select(i => new 
            {
                i.ItemId,
                ItemName = i.Item?.Name,
                StockAvailable = i.QuantityOnHand
            }),
            AuditTimestamp = DateTime.UtcNow
        });
    }
}