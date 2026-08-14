using Orders.API.Data;
using Orders.API.Models;
using Orders.API.Orders.Shared;

namespace Orders.API.Orders.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(string CustomerId) : IQuery<GetOrdersByCustomerResult>;
public record GetOrdersByCustomerResult(IReadOnlyCollection<Order> Orders);

public class GetOrdersByCustomerQueryHandler(IOrderRepository repository)
    : IQueryHandler<GetOrdersByCustomerQuery, GetOrdersByCustomerResult>
{
    public async Task<GetOrdersByCustomerResult> Handle(
        GetOrdersByCustomerQuery query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.CustomerId))
        {
            throw new OrderValidationException("CustomerId es obligatorio.");
        }

        var orders = await repository.GetByCustomerIdAsync(query.CustomerId.Trim(), cancellationToken);
        return new GetOrdersByCustomerResult(orders);
    }
}
