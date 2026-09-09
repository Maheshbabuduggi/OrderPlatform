using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Data.Entities;
using OrderApi.DTOs;
using OrderApi.Messaging;

namespace OrderApi.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext dbContext, IEventPublisher eventPublisher, ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = dto.CustomerName,
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            Price = dto.Price,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} saved to SQL with status Pending", order.Id);

        var orderEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerName = order.CustomerName,
            ProductName = order.ProductName,
            Quantity = order.Quantity,
            Price = order.Price,
            CreatedAt = order.CreatedAt
        };

        // Publish after the DB commit succeeds — never publish before the write is durable.
        await _eventPublisher.PublishOrderCreatedAsync(orderEvent, cancellationToken);

        return ToDto(order);
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order is null ? null : ToDto(order);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _dbContext.Orders.AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(ToDto);
    }

    private static OrderResponseDto ToDto(Order order) => new()
    {
        Id = order.Id,
        CustomerName = order.CustomerName,
        ProductName = order.ProductName,
        Quantity = order.Quantity,
        Price = order.Price,
        Status = order.Status,
        CreatedAt = order.CreatedAt,
        ProcessedAt = order.ProcessedAt
    };
}