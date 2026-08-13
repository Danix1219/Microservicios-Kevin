using Microsoft.Extensions.Options;
using Orders.API.Contracts;
using Orders.API.Domain;
using Orders.API.Infrastructure;
using Orders.API.Infrastructure.Clients;
using Orders.API.Infrastructure.Persistence;

namespace Orders.API.Application;

public interface IOrderService
{
    Task<(Order Order, bool IsReplay)> CreateAsync(CreateOrderRequest request, string idempotencyKey, CancellationToken cancellationToken);
    Task<Order> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Order>> GetByCustomerAsync(string customerId, CancellationToken cancellationToken);
    Task<Order> UpdateStatusAsync(Guid id, OrderStatus nextStatus, CancellationToken cancellationToken);
}

public sealed class OrderService(
    IOrderRepository repository,
    IBasketClient basketClient,
    ICatalogClient catalogClient,
    IOptions<OrderOptions> options,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<(Order Order, bool IsReplay)> CreateAsync(CreateOrderRequest request, string idempotencyKey, CancellationToken cancellationToken)
    {
        ValidateRequest(request, idempotencyKey);

        var existing = await repository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            if (!string.Equals(existing.CustomerId, request.CustomerId.Trim(), StringComparison.Ordinal)
                || !string.Equals(existing.BasketId, request.BasketId.Trim(), StringComparison.Ordinal))
            {
                throw new IdempotencyConflictException();
            }
            return (existing, true);
        }

        var basket = await basketClient.GetBasketAsync(request.BasketId, cancellationToken);
        if (basket.Items.Count == 0)
        {
            throw new OrderValidationException("El carrito está vacío.");
        }

        var orderItems = new List<OrderItem>(basket.Items.Count);
        foreach (var basketItem in basket.Items)
        {
            if (basketItem.ProductId == Guid.Empty || basketItem.Quantity <= 0 || basketItem.Price <= 0)
            {
                throw new OrderValidationException("El carrito contiene cantidades, precios o productos inválidos.");
            }

            var product = await catalogClient.GetProductAsync(basketItem.ProductId, cancellationToken);
            if (product is null)
            {
                throw new OrderValidationException($"El producto {basketItem.ProductId} ya no existe en el catálogo.");
            }

            if (product.Price != basketItem.Price)
            {
                throw new OrderValidationException($"El precio de {product.Name} cambió. Actualiza el carrito antes de comprar.");
            }

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Color = basketItem.Color,
                Quantity = basketItem.Quantity,
                UnitPrice = basketItem.Price,
                LineTotal = decimal.Round(basketItem.Price * basketItem.Quantity, 2)
            });
        }

        var subtotal = decimal.Round(orderItems.Sum(item => item.LineTotal), 2);
        var tax = decimal.Round(subtotal * options.Value.TaxRate, 2, MidpointRounding.AwayFromZero);
        var order = new Order
        {
            CustomerId = request.CustomerId.Trim(),
            BasketId = request.BasketId.Trim(),
            Items = orderItems,
            Subtotal = subtotal,
            Tax = tax,
            Total = subtotal + tax,
            IdempotencyKey = idempotencyKey.Trim()
        };

        var saved = await repository.CreateAsync(order, cancellationToken);
        if (saved.Id != order.Id
            && (!string.Equals(saved.CustomerId, order.CustomerId, StringComparison.Ordinal)
                || !string.Equals(saved.BasketId, order.BasketId, StringComparison.Ordinal)))
        {
            throw new IdempotencyConflictException();
        }
        logger.LogInformation("Orden {OrderId} creada para el cliente {CustomerId} con {ItemCount} partidas.", saved.Id, saved.CustomerId, saved.Items.Count);
        return (saved, saved.Id != order.Id);
    }

    public async Task<Order> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken) ?? throw new OrderNotFoundException(id);

    public Task<IReadOnlyCollection<Order>> GetByCustomerAsync(string customerId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(customerId)) throw new OrderValidationException("CustomerId es obligatorio.");
        return repository.GetByCustomerIdAsync(customerId.Trim(), cancellationToken);
    }

    public async Task<Order> UpdateStatusAsync(Guid id, OrderStatus nextStatus, CancellationToken cancellationToken)
    {
        var order = await GetByIdAsync(id, cancellationToken);
        if (!order.CanTransitionTo(nextStatus))
        {
            throw new InvalidOrderStatusTransitionException(order.Status, nextStatus);
        }

        if (!await repository.UpdateStatusAsync(id, order.Status, nextStatus, cancellationToken))
        {
            throw new InvalidOrderStatusTransitionException(order.Status, nextStatus);
        }

        order.ChangeStatus(nextStatus);
        return order;
    }

    private static void ValidateRequest(CreateOrderRequest request, string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerId)) throw new OrderValidationException("CustomerId es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.BasketId)) throw new OrderValidationException("BasketId es obligatorio.");
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new OrderValidationException("El header Idempotency-Key es obligatorio.");
        if (idempotencyKey.Length > 128) throw new OrderValidationException("Idempotency-Key no puede exceder 128 caracteres.");
    }
}
