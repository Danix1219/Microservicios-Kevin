namespace Catalog.API.Models.Products.GetProductsBy
{
    public record GetProductByNameQuery(string Name) : IQuery<GetProductByNameResult>;
    public record GetProductByNameResult(IEnumerable<Product> Products);
    public class GetProductsByNameCommandHandler(IDocumentSession session) : IQueryHandler<GetProductByNameQuery, GetProductByNameResult>
    {
        public async Task<GetProductByNameResult> Handle(GetProductByNameQuery query, CancellationToken cancellationToken)
        {
            var products = await session.Query<Product>()
                .Where(p => p.Name.ToLower().Contains(query.Name.ToLower()))
                .ToListAsync(cancellationToken);

            return new GetProductByNameResult(products);
        }
    }
}
