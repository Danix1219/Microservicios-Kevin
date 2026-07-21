using Catalog.API.Models.Products.GetProducts;

namespace Catalog.API.Models.Products.GetProductPag
{
    public record GetProductsPagResponse(IEnumerable<Product> Products, int TotalCount, int PageNumber, int PageSize);
    public class GetProductPagEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products/page", async (ISender sender, int pageNumber = 1, int pageSize = 10) =>
            {
                var result = await sender.Send(new GetProductsPagQuery(pageNumber, pageSize));
                return Results.Ok(result.Adapt<GetProductsPagResponse>());
            })
            .WithName("ObtenerProductos")
            .Produces<GetProductsPagResponse>(StatusCodes.Status200OK)
            .WithSummary("Obtener Productos Paginados")
            .WithDescription("Obtiene todos los productos con paginación");
        }
    }
}
