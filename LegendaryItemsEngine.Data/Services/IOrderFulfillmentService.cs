using LegendaryItemsEngine.Data.Models;

namespace LegendaryItemsEngine.Data.Services;

public interface IOrderFulfillmentService
{
    // Coloca una orden en la cola de prioridad en memoria
    void EnqueueOrder(ItemOrder order);

    // Procesa la orden que esté al frente de la cola (atendiendo prioridades primero)
    Task<bool> ProcessNextOrderAsync();

    // Devuelve cuántas órdenes quedan pendientes en la fila
    int GetPendingOrdersCount();
}