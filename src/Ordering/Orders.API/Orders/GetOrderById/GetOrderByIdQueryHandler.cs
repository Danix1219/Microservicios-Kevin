using Orders.API.Data;
using Orders.API.Models;
using Orders.API.Orders.Shared;

namespace Orders.API.Orders.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IQuery<GetOrderByIdResult>;
public record GetOrderByIdResult(Order Order);

public class GetOrderByIdQueryHandler(IOrderRepository repository)
    : IQueryHandler<GetOrderByIdQuery, GetOrderByIdResult>
{
    public async Task<GetOrderByIdResult> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new OrderNotFoundException(query.Id);
        return new GetOrderByIdResult(order);
    }
}
