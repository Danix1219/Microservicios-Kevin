using FluentValidation;
using Orders.API.Data;
using Orders.API.Models;
using Orders.API.Orders.Shared;

namespace Orders.API.Orders.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid Id, OrderStatus Status)
    : ICommand<UpdateOrderStatusResult>;

public record UpdateOrderStatusResult(Order Order);

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("El identificador de la orden es obligatorio.");
        RuleFor(command => command.Status)
            .IsInEnum().WithMessage("El estado de la orden no es válido.");
    }
}

public class UpdateOrderStatusCommandHandler(IOrderRepository repository)
    : ICommandHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    public async Task<UpdateOrderStatusResult> Handle(
        UpdateOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new OrderNotFoundException(command.Id);

        if (!order.CanTransitionTo(command.Status))
        {
            throw new InvalidOrderStatusTransitionException(order.Status, command.Status);
        }

        if (!await repository.UpdateStatusAsync(command.Id, order.Status, command.Status, cancellationToken))
        {
            throw new InvalidOrderStatusTransitionException(order.Status, command.Status);
        }

        order.ChangeStatus(command.Status);
        return new UpdateOrderStatusResult(order);
    }
}
