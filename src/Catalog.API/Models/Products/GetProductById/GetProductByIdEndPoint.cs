using Catalog.API.Models;

namespace Catalog.API.Models.Products.GetProductById;

public sealed class GetProductByIdEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:guid}", async (Guid id, IQuerySession session, CancellationToken cancellationToken) =>
        {
            var product = await session.LoadAsync<Product>(id, cancellationToken);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName("GetProductById")
        .Produces<Product>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Obtener producto por identificador")
        .WithDescription("Devuelve el producto que Orders.API utiliza para validar nombre y precio.");
    }
}
