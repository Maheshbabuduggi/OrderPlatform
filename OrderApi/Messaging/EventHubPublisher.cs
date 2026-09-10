using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace OrderApi.Messaging;

public class EventHubPublisher : IEventPublisher
{
    private readonly EventHubProducerClient _producerClient;
    private readonly ILogger<EventHubPublisher> _logger;

    public EventHubPublisher(EventHubProducerClient producerClient, ILogger<EventHubPublisher> logger)
    {
        _producerClient = producerClient;
        _logger = logger;
    }

    public async Task PublishOrderCreatedAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken = default)
    {
        using EventDataBatch eventBatch = await _producerClient.CreateBatchAsync(cancellationToken);

        var json = JsonSerializer.Serialize(orderEvent);
        var eventData = new EventData(Encoding.UTF8.GetBytes(json));
        eventData.Properties["EventType"] = "OrderCreated";

        if (!eventBatch.TryAdd(eventData))
        {
            throw new InvalidOperationException("OrderCreated event is too large for the Event Hub batch.");
        }

        await _producerClient.SendAsync(eventBatch, cancellationToken);

        _logger.LogInformation("Published OrderCreated event for OrderId {OrderId}", orderEvent.OrderId);
    }
}