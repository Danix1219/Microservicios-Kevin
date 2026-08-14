using Orders.API.Orders.Shared;

namespace Orders.API.Orders.GetOrdersByCustomer;

public class GetOrdersByCustomerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/customer/{customerId}", async (
            string customerId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetOrdersByCustomerQuery(customerId),
                cancellationToken);
            return Results.Ok(result.Orders.Select(order => order.ToResponse()));
        })
        .WithName("GetOrdersByCustomer")
        .WithTags("Orders")
        .WithSummary("Lista las órdenes de un cliente")
        .Produces<IReadOnlyCollection<OrderResponse>>();
    }
}
