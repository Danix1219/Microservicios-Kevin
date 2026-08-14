using Orders.API.Orders.Shared;

namespace Orders.API.Orders.GetOrderById;

public class GetOrderByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetOrderByIdQuery(id), cancellationToken);
            return Results.Ok(result.Order.ToResponse());
        })
        .WithName("GetOrder")
        .WithTags("Orders")
        .WithSummary("Obtiene una orden por identificador")
        .Produces<OrderResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
