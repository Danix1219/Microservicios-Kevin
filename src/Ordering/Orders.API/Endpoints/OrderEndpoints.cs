using Microsoft.AspNetCore.Mvc;
using Orders.API.Application;
using Orders.API.Contracts;

namespace Orders.API.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapPost("/", async (
            [FromBody] CreateOrderRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            IOrderService service,
            CancellationToken cancellationToken) =>
        {
            var (order, isReplay) = await service.CreateAsync(request, idempotencyKey ?? string.Empty, cancellationToken);
            return isReplay
                ? Results.Ok(order.ToResponse())
                : Results.Created($"/api/orders/{order.Id}", order.ToCreateResponse());
        })
        .WithName("CreateOrder")
        .WithSummary("Genera una orden desde el carrito del cliente")
        .WithDescription("Requiere Idempotency-Key. Valida el carrito y los productos antes de conservar una instantánea de precios.")
        .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
        .Produces<OrderResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        group.MapGet("/{id:guid}", async (Guid id, IOrderService service, CancellationToken cancellationToken) =>
            Results.Ok((await service.GetByIdAsync(id, cancellationToken)).ToResponse()))
            .WithName("GetOrder")
            .WithSummary("Obtiene una orden por identificador")
            .Produces<OrderResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/customer/{customerId}", async (string customerId, IOrderService service, CancellationToken cancellationToken) =>
        {
            var orders = await service.GetByCustomerAsync(customerId, cancellationToken);
            return Results.Ok(orders.Select(order => order.ToResponse()));
        })
        .WithName("GetOrdersByCustomer")
        .WithSummary("Lista las órdenes de un cliente")
        .Produces<IReadOnlyCollection<OrderResponse>>();

        group.MapPatch("/{id:guid}/status", async (Guid id, UpdateOrderStatusRequest request, IOrderService service, CancellationToken cancellationToken) =>
            Results.Ok((await service.UpdateStatusAsync(id, request.Status, cancellationToken)).ToResponse()))
            .WithName("UpdateOrderStatus")
            .WithSummary("Cambia el estado de una orden")
            .WithDescription("Transiciones permitidas: Pending → Confirmed y Pending → Cancelled.")
            .Produces<OrderResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
