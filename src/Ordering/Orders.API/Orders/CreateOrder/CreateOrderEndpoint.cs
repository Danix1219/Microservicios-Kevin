using Microsoft.AspNetCore.Mvc;
using Orders.API.Models;
using Orders.API.Orders.Shared;

namespace Orders.API.Orders.CreateOrder;

public record CreateOrderRequest(string CustomerId, string BasketId);
public record CreateOrderResponse(
    Guid Id,
    string CustomerId,
    DateTime CreatedAt,
    OrderStatus Status,
    decimal Subtotal,
    decimal Tax,
    decimal Total);

public class CreateOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (
            [FromBody] CreateOrderRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateOrderCommand(
                request.CustomerId,
                request.BasketId,
                idempotencyKey ?? string.Empty);
            var result = await sender.Send(command, cancellationToken);

            if (result.IsReplay)
            {
                return Results.Ok(result.Order.ToResponse());
            }

            var order = result.Order;
            var response = new CreateOrderResponse(
                order.Id,
                order.CustomerId,
                order.CreatedAt,
                order.Status,
                order.Subtotal,
                order.Tax,
                order.Total);
            return Results.Created($"/api/orders/{order.Id}", response);
        })
        .WithName("CreateOrder")
        .WithTags("Orders")
        .WithSummary("Genera una orden desde el carrito del cliente")
        .WithDescription("Requiere Idempotency-Key. Valida el carrito y los productos antes de conservar una instantánea de precios.")
        .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
        .Produces<OrderResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
    }
}
