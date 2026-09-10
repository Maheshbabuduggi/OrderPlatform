using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderProcessor.Data;
using OrderProcessor.Messaging;

namespace OrderProcessor.Functions;

public class OrderCreatedFunction
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<OrderCreatedFunction> _logger;

    public OrderCreatedFunction(AppDbContext dbContext, ILogger<OrderCreatedFunction> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [Function("OrderCreatedFunction")]
    public async Task RunAsync(
        [EventHubTrigger("orders", Connection = "EventHubConnection", ConsumerGroup = "$Default")]
        string[] events)
    {
        foreach (var rawEvent in events)
        {
            OrderCreatedEvent? orderEvent;
            try
            {
                orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(rawEvent);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize event payload: {Payload}", rawEvent);
                continue;
            }

            if (orderEvent is null)
            {
                _logger.LogWarning("Received null OrderCreatedEvent, skipping.");
                continue;
            }

            _logger.LogInformation("Processing OrderCreated for OrderId {OrderId}", orderEvent.OrderId);

            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderEvent.OrderId);
            if (order is null)
            {
                _logger.LogWarning("Order {OrderId} not found in SQL — skipping update.", orderEvent.OrderId);
                continue;
            }

            // Simulated processing work — replace with real business logic
            // (inventory check, payment capture, notification, etc.)
            await Task.Delay(TimeSpan.FromSeconds(1));

            order.Status = "Processed";
            order.ProcessedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} marked Processed in SQL", orderEvent.OrderId);
        }
    }
}