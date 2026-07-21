namespace Catalog.API.Models.Products.GetProductByCategory
{
    public record GetProductByCategoryResponse(IEnumerable<Product> Products);

    public class GetProductByCategoryEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products/category/{category}", async (string category, ISender sender) =>
            {
                var result = await sender.Send(new GetProductByCategoryQuery(category));
                return Results.Ok(result.Adapt<GetProductByCategoryResponse>());
            })
            .WithName("BuscarProductoPorCategoria")
            .Produces<GetProductByCategoryResponse>(StatusCodes.Status200OK)
            .WithSummary("Buscar Producto por Categoría")
            .WithDescription("Busca productos que pertenezcan a una categoría dada");
        }
    }
}
