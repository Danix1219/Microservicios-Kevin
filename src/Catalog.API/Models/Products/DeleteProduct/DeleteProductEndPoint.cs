namespace Catalog.API.Products.DeleteProduct
{
    public record DeleteProductResponse(bool IsSuccess);

    public class DeleteProductEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/products/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteProductCommand(id));

                if (!result.IsSuccess)
                {
                    return Results.NotFound();
                }

                return Results.Ok(result.Adapt<DeleteProductResponse>());
            })
            .WithName("EliminarProducto")
            .Produces<DeleteProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Eliminar Producto")
            .WithDescription("Elimina el Producto");
        }
    }
}