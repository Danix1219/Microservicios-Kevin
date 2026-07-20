using FluentValidation;

namespace Catalog.API.Models.Products.UpdateProduct
{
    public record UpdateProductCommand(Guid Id, string Name, string Description, List<string> Category, string ImageFiles, decimal Price) : ICommand<UpdateProductResult>;
    public record UpdateProductResult(bool IsSuccess);

    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("El Id es requerido");
            RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es requerido");
            RuleFor(x => x.Category).NotEmpty().WithMessage("La categoría es requerida");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("El precio debe ser mayor a 0");
        }
    }
    public class UpdateProductCommandHandler(IDocumentSession documentSession) : ICommandHandler<UpdateProductCommand, UpdateProductResult>
    {
        public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
        {
            var product = await documentSession.LoadAsync<Product>(command.Id, cancellationToken);
            if (product is null)
            {
                return new UpdateProductResult(false);
            }
            product.Name = command.Name;
            product.Description = command.Description;
            product.Category = command.Category;
            product.ImageFiles = command.ImageFiles;
            product.Price = command.Price;

            documentSession.Update(product);
            await documentSession.SaveChangesAsync(cancellationToken);
            return new UpdateProductResult(true);
        }
    }
}
