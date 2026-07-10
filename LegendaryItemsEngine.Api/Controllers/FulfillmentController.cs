using Microsoft.AspNetCore.Mvc;
using LegendaryItemsEngine.Data.Services;
using Serilog;

namespace LegendaryItemsEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FulfillmentController : ControllerBase
{
    private readonly IOrderFulfillmentService _fulfillmentService;

    public FulfillmentController(IOrderFulfillmentService fulfillmentService)
    {
        _fulfillmentService = fulfillmentService;
    }

    // Endpoint que procesa de golpe todas las órdenes pendientes en la cola usando concurrencia
    [HttpPost("process-all")]
    public async Task<IActionResult> ProcessAllOrders(CancellationToken ct)
    {
        int pendingCount = _fulfillmentService.GetPendingOrdersCount();
        
        if (pendingCount == 0)
        {
            return BadRequest("The order queue is currently empty.");
        }

        Log.Information("=== Starting Fulfillment Engine: Processing {Count} pending orders in parallel ===", pendingCount);

        var processingTasks = new List<Task<bool>>();

        while (_fulfillmentService.GetPendingOrdersCount() > 0)
        {
            if (ct.IsCancellationRequested)
            {
                Log.Warning("Fulfillment loop interrupted because the client cancelled the HTTP request.");
                return StatusCode(499, "Fulfillment processing was aborted by the client.");
            }
            processingTasks.Add(_fulfillmentService.ProcessNextOrderAsync());
        }

        try
        {
            var results = await Task.WhenAll(processingTasks);
            
            int successfulOrders = results.Count(r => r == true);
            int failedOrders = results.Count(r => r == false);

            Log.Information("=== Fulfillment Run Completed. Success: {Success}, Failed: {Failed} ===", successfulOrders, failedOrders);

            return Ok(new {
                Message = "Fulfillment processing completed.",
                TotalProcessed = results.Length,
                Successful = successfulOrders,
                Failed = failedOrders
            });
        }
        catch (OperationCanceledException)
        {
            Log.Warning("Fulfillment tasks execution cancelled during asynchronous wait.");
            return StatusCode(499, "Tasks execution cancelled by client.");
        }
    }
}