using Microsoft.EntityFrameworkCore;
using LegendaryItemsEngine.Data.Models;
using LegendaryItemsEngine.Data.Repositories;
using Serilog;

namespace LegendaryItemsEngine.Data.Services;

public class OrderFulfillmentService : IOrderFulfillmentService
{
    // La estructura de datos que ordenará todo automáticamente por el enum numérico
    private readonly PriorityQueue<ItemOrder, int> _orderQueue = new();
    private readonly IInventoryRepository _inventoryRepository;

    // Inyectamos el repositorio que creamos en el paso anterior
    public OrderFulfillmentService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public void EnqueueOrder(ItemOrder order)
    {
        // Insertamos la orden. El segundo parámetro es la prioridad (1 es más rápido que 2)
        _orderQueue.Enqueue(order, (int)order.Priority);
        Log.Information("Order {OrderId} added to queue. Priority: {Priority}. Queue size: {QueueSize}", 
            order.Id, order.Priority, _orderQueue.Count);
    }

    public int GetPendingOrdersCount() => _orderQueue.Count;

    public async Task<bool> ProcessNextOrderAsync()
    {
        if (_orderQueue.Count == 0) return false;

        // Extraemos la orden con mayor prioridad de la fila
        var order = _orderQueue.Dequeue();
        
        // Asumimos que de momento procesamos el MVP con una línea de detalle
        if (order.Line == null) return false;

        int maxRetries = 3;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                Log.Information("Processing Order {OrderId} (Attempt {Attempt}/{MaxRetries}) for Item {ItemId}", 
                    order.Id, attempt, maxRetries, order.Line.ItemId);

                // Intentamos restar el stock usando nuestro repositorio
                bool success = await _inventoryRepository.TryDeductStockAsync(order.Line.ItemId, order.Line.Quantity);

                if (success)
                {
                    order.Status = OrderStatus.Fulfilled; // ¡Éxito total!
                    Log.Information("Order {OrderId} fulfilled successfully.", order.Id);
                    return true;
                }
                
                // Si regresó false es porque de plano no había stock suficiente
                order.Status = OrderStatus.Backordered;
                return false;
            }
            catch (DbUpdateConcurrencyException)
            {
                // El repositorio lanzó la excepción del escudo RowVersion
                Log.Warning("Conflict hit on Order {OrderId}. RowVersion changed. Retrying...", order.Id);
                
                if (attempt == maxRetries)
                {
                    // Si ya agotamos los 3 intentos, la orden se marca como fallida por colisión extrema
                    order.Status = OrderStatus.Backordered;
                    Log.Error("Order {OrderId} failed after {MaxRetries} attempts due to heavy concurrency lock.", order.Id, maxRetries);
                    throw;
                }

                // Esperamos un breve parpadeo (50ms) antes de volver a intentar para dejar respirar a la base de datos
                await Task.Delay(50);
            }
        }

        return false;
    }
    
}