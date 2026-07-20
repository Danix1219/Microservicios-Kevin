namespace Catalog.API.Models.Products.UpdateProduct
{
    public record UpdateProductRequest(Guid Id, string Name, string Description, List<string> Category, string ImageFiles, decimal Price);
    public record UpdateProductResponse(bool IsSuccess);
    public class UpdateProductEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/products/{id:guid}", async (Guid id, UpdateProductRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateProductCommand>() with { Id = id };
                var result = await sender.Send(command);
                if (!result.IsSuccess)
                {
                    return Results.NotFound();
                }
                return Results.Ok(result.Adapt<UpdateProductResponse>());
            })
                .WithName("ActualizarProducto")
                .Produces<UpdateProductResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Actualizar Producto")
                .WithDescription("Edita el Producto");
        }
    }
}
