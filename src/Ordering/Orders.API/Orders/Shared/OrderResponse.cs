using Orders.API.Models;

namespace Orders.API.Orders.Shared;

public record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    string Color,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record OrderResponse(
    Guid Id,
    string CustomerId,
    string BasketId,
    DateTime CreatedAt,
    OrderStatus Status,
    IReadOnlyCollection<OrderItemResponse> Items,
    decimal Subtotal,
    decimal Tax,
    decimal Total);

public static class OrderMappings
{
    public static OrderResponse ToResponse(this Order order) => new(
        order.Id,
        order.CustomerId,
        order.BasketId,
        order.CreatedAt,
        order.Status,
        order.Items.Select(item => new OrderItemResponse(
            item.ProductId,
            item.ProductName,
            item.Color,
            item.Quantity,
            item.UnitPrice,
            item.LineTotal)).ToArray(),
        order.Subtotal,
        order.Tax,
        order.Total);
}
