using Catalog.API.Models;
using FluentValidation;

namespace Catalog.API.Products.DeleteProduct
{
    public record DeleteProductCommand(Guid Id) : ICommand<DeleteProductResult>;
    public record DeleteProductResult(bool IsSuccess);

    public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("El Id es requerido");
        }
    }

    public class DeleteProductCommandHandler(IDocumentSession documentSession)
        : ICommandHandler<DeleteProductCommand, DeleteProductResult>
    {
        public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            var product = await documentSession.LoadAsync<Product>(command.Id, cancellationToken);
            if (product is null)
            {
                return new DeleteProductResult(false);
            }

            documentSession.Delete<Product>(command.Id);
            await documentSession.SaveChangesAsync(cancellationToken);

            return new DeleteProductResult(true);
        }
    }
}