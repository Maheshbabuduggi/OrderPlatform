using OrderApi.DTOs;

namespace OrderApi.Services;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderResponseDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
}