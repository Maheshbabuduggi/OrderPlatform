namespace OrderApi.Messaging;

public interface IEventPublisher
{
    Task PublishOrderCreatedAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken = default);
}