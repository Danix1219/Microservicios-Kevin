using Orders.API.Domain;

namespace Orders.API.Contracts;

public sealed record CreateOrderRequest(string CustomerId, string BasketId);
public sealed record CreateOrderResponse(Guid Id, string CustomerId, DateTime CreatedAt, OrderStatus Status, decimal Subtotal, decimal Tax, decimal Total);
public sealed record UpdateOrderStatusRequest(OrderStatus Status);
public sealed record OrderItemResponse(Guid ProductId, string ProductName, string Color, int Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record OrderResponse(Guid Id, string CustomerId, string BasketId, DateTime CreatedAt, OrderStatus Status, IReadOnlyCollection<OrderItemResponse> Items, decimal Subtotal, decimal Tax, decimal Total);

public static class OrderMappings
{
    public static OrderResponse ToResponse(this Order order) => new(
        order.Id,
        order.CustomerId,
        order.BasketId,
        order.CreatedAt,
        order.Status,
        order.Items.Select(item => new OrderItemResponse(item.ProductId, item.ProductName, item.Color, item.Quantity, item.UnitPrice, item.LineTotal)).ToArray(),
        order.Subtotal,
        order.Tax,
        order.Total);

    public static CreateOrderResponse ToCreateResponse(this Order order) => new(
        order.Id, order.CustomerId, order.CreatedAt, order.Status, order.Subtotal, order.Tax, order.Total);
}
