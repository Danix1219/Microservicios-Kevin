using Orders.API.Models;

namespace Orders.API.Data;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken);
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken);
    Task<bool> UpdateStatusAsync(Guid id, OrderStatus expectedStatus, OrderStatus nextStatus, CancellationToken cancellationToken);
}
