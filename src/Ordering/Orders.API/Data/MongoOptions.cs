namespace Orders.API.Data;

public sealed class MongoOptions
{
    public const string SectionName = "MongoDb";
    public string ConnectionString { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = "OrdersDb";
    public string CollectionName { get; init; } = "orders";
}

public sealed class OrderOptions
{
    public const string SectionName = "Order";
    public decimal TaxRate { get; init; } = 0.16m;
}
