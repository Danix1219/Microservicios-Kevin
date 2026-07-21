namespace Catalog.API.Models.Products.GetProductsBy
{
    public record GetProductByNameResponse(IEnumerable<Product> Products);
    public class GetProductsByNameEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products/search/{name}", async (string name, ISender sender) =>
            {
                var result = await sender.Send(new GetProductByNameQuery(name));
                return Results.Ok(result.Adapt<GetProductByNameResponse>());
            })
                .WithName("BuscarProductoPorNombre")
                .Produces<GetProductByNameResponse>(StatusCodes.Status200OK)
                .WithSummary("Buscar Producto por Nombre")
                .WithDescription("Busca productos cuyo nombre contenga el texto dado");
        }
    }
}
