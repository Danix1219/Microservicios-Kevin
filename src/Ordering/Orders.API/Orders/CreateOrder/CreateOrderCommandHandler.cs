using FluentValidation;
using Microsoft.Extensions.Options;
using Orders.API.Data;
using Orders.API.Models;
using Orders.API.Orders.Shared;
using Orders.API.Services;

namespace Orders.API.Orders.CreateOrder;

public record CreateOrderCommand(
    string CustomerId,
    string BasketId,
    string IdempotencyKey) : ICommand<CreateOrderResult>;

public record CreateOrderResult(Order Order, bool IsReplay);

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.CustomerId)
            .NotEmpty().WithMessage("CustomerId es obligatorio.");
        RuleFor(command => command.BasketId)
            .NotEmpty().WithMessage("BasketId es obligatorio.");
        RuleFor(command => command.IdempotencyKey)
            .NotEmpty().WithMessage("El header Idempotency-Key es obligatorio.")
            .MaximumLength(128).WithMessage("Idempotency-Key no puede exceder 128 caracteres.");
    }
}

public class CreateOrderCommandHandler(
    IOrderRepository repository,
    IBasketClient basketClient,
    ICatalogClient catalogClient,
    IOptions<OrderOptions> options,
    ILogger<CreateOrderCommandHandler> logger)
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var customerId = command.CustomerId.Trim();
        var basketId = command.BasketId.Trim();
        var idempotencyKey = command.IdempotencyKey.Trim();

        var existing = await repository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            if (!string.Equals(existing.CustomerId, customerId, StringComparison.Ordinal)
                || !string.Equals(existing.BasketId, basketId, StringComparison.Ordinal))
            {
                throw new IdempotencyConflictException();
            }

            return new CreateOrderResult(existing, true);
        }

        var basket = await basketClient.GetBasketAsync(basketId, cancellationToken);
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
            CustomerId = customerId,
            BasketId = basketId,
            Items = orderItems,
            Subtotal = subtotal,
            Tax = tax,
            Total = subtotal + tax,
            IdempotencyKey = idempotencyKey
        };

        var saved = await repository.CreateAsync(order, cancellationToken);
        if (saved.Id != order.Id
            && (!string.Equals(saved.CustomerId, order.CustomerId, StringComparison.Ordinal)
                || !string.Equals(saved.BasketId, order.BasketId, StringComparison.Ordinal)))
        {
            throw new IdempotencyConflictException();
        }

        logger.LogInformation(
            "Orden {OrderId} creada para el cliente {CustomerId} con {ItemCount} partidas.",
            saved.Id,
            saved.CustomerId,
            saved.Items.Count);

        return new CreateOrderResult(saved, saved.Id != order.Id);
    }
}
