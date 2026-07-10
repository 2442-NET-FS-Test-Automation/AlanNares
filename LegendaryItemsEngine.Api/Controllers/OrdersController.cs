using Microsoft.AspNetCore.Mvc;
using LegendaryItemsEngine.Data.Models;
using LegendaryItemsEngine.Data.Services;
using Serilog;
using LegendaryItemsEngine.Data.DTOs;

namespace LegendaryItemsEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderFulfillmentService _fulfillmentService;

    // Singleton 
    public OrdersController(IOrderFulfillmentService fulfillmentService)
    {
        _fulfillmentService = fulfillmentService;
    }

    // Create one order
    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return StatusCode(499, "Request cancelled by client.");

        if (dto == null || dto.Quantity <= 0)
        {
            return BadRequest("Invalid order.");
        }

        var order = new ItemOrder
        {
            Id = new Random().Next(1000, 9999), // Le inventamos un ID temporal para la fila
            PlayerId = dto.PlayerId,
            Priority = dto.Priority,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Line = new OrderLine
            {
                ItemId = dto.ItemId,
                Quantity = dto.Quantity
            }
        };

        _fulfillmentService.EnqueueOrder(order);

        return Ok(new { Message = $"Order successfully enqueued.", OrderId = order.Id, Priority = order.Priority.ToString() });
    }

    // burst 50 orders
    [HttpPost("simulate-burst")]
    public IActionResult SimulateBurst([FromQuery] int itemId, [FromQuery] int totalOrders = 50, CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            Log.Warning("Burst simulation aborted midway due to client cancellation.");
            return StatusCode(499, "Burst simulation cancelled by client.");
        }

        Log.Information("=== Starting Burst Simulation: Enqueuing {Total} orders for Item {ItemId} ===", totalOrders, itemId);
        
        var random = new Random();

        for (int i = 1; i <= totalOrders; i++)
        {
            if (ct.IsCancellationRequested)
            {
                Log.Warning("Burst simulation aborted midway due to client cancellation.");
                return StatusCode(499, "Burst simulation cancelled by client.");
            }

            // Random priority
            var priority = random.Next(1, 3) == 1 ? OrderPriority.Expedited : OrderPriority.Normal;

            var order = new ItemOrder
            {
                Id = i,
                PlayerId = random.Next(100, 999), // IDs Random Players
                Priority = priority,
                Status = OrderStatus.Pending,
                Line = new OrderLine
                {
                    Id = i,
                    ItemId = itemId,
                    Quantity = 1 // All orders are 1
                }
            };

            _fulfillmentService.EnqueueOrder(order);
        }

        return Ok(new { 
            Message = $"Successfully simulated and enqueued {totalOrders} concurrent orders.",
            CurrentQueueSize = _fulfillmentService.GetPendingOrdersCount()
        });
    }
}