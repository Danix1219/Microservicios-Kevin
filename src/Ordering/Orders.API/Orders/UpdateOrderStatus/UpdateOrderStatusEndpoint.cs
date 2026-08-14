using Orders.API.Models;
using Orders.API.Orders.Shared;

namespace Orders.API.Orders.UpdateOrderStatus;

public record UpdateOrderStatusRequest(OrderStatus Status);

public class UpdateOrderStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/orders/{id:guid}/status", async (
            Guid id,
            UpdateOrderStatusRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateOrderStatusCommand(id, request.Status),
                cancellationToken);
            return Results.Ok(result.Order.ToResponse());
        })
        .WithName("UpdateOrderStatus")
        .WithTags("Orders")
        .WithSummary("Cambia el estado de una orden")
        .WithDescription("Transiciones permitidas: Pending → Confirmed y Pending → Cancelled.")
        .Produces<OrderResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
