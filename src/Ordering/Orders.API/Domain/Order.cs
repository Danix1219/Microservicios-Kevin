namespace Orders.API.Domain;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public sealed class Order
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; init; } = Guid.NewGuid();
    public string CustomerId { get; init; } = string.Empty;
    public string BasketId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public List<OrderItem> Items { get; init; } = [];
    public decimal Subtotal { get; init; }
    public decimal Tax { get; init; }
    public decimal Total { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;

    public bool CanTransitionTo(OrderStatus nextStatus) =>
        Status == OrderStatus.Pending && nextStatus is OrderStatus.Confirmed or OrderStatus.Cancelled;

    public void ChangeStatus(OrderStatus nextStatus)
    {
        if (!CanTransitionTo(nextStatus))
        {
            throw new InvalidOrderStatusTransitionException(Status, nextStatus);
        }

        Status = nextStatus;
    }
}

public sealed class InvalidOrderStatusTransitionException(OrderStatus current, OrderStatus next)
    : Exception($"No se permite cambiar una orden de {current} a {next}.");
