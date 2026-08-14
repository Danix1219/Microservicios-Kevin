namespace Orders.API.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public sealed class OrderItem
{
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}
