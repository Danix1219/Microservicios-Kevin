using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Orders.API.Domain;

namespace Orders.API.Infrastructure.Persistence;

public sealed class MongoOrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _orders;

    public MongoOrderRepository(IMongoClient mongoClient, IOptions<MongoOptions> options)
    {
        var settings = options.Value;
        _orders = mongoClient.GetDatabase(settings.DatabaseName).GetCollection<Order>(settings.CollectionName);
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        var idempotencyIndex = new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Ascending(order => order.IdempotencyKey),
            new CreateIndexOptions { Unique = true, Name = "ux_orders_idempotency_key" });
        var customerIndex = new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Ascending(order => order.CustomerId).Descending(order => order.CreatedAt),
            new CreateIndexOptions { Name = "ix_orders_customer_created_at" });
        await _orders.Indexes.CreateManyAsync([idempotencyIndex, customerIndex], cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _orders.Find(order => order.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken) =>
        await _orders.Find(order => order.IdempotencyKey == idempotencyKey).FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken) =>
        await _orders.Find(order => order.CustomerId == customerId)
            .SortByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken)
    {
        await EnsureIndexesAsync(cancellationToken);
        try
        {
            await _orders.InsertOneAsync(order, cancellationToken: cancellationToken);
            return order;
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            var existing = await GetByIdempotencyKeyAsync(order.IdempotencyKey, cancellationToken);
            if (existing is not null) return existing;
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(Guid id, OrderStatus expectedStatus, OrderStatus nextStatus, CancellationToken cancellationToken)
    {
        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Eq(order => order.Id, id),
            Builders<Order>.Filter.Eq(order => order.Status, expectedStatus));
        var update = Builders<Order>.Update.Set(order => order.Status, nextStatus);
        var result = await _orders.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount == 1;
    }
}
