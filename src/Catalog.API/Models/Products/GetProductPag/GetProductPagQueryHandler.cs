using Catalog.API.Models.Products.GetProducts;
using Marten.Pagination;

namespace Catalog.API.Models.Products.GetProductPag
{
    public record GetProductsPagQuery(int PageNumber = 1, int PageSize = 10) : IQuery<GetProductsPagResult>;
    public record GetProductsPagResult(IEnumerable<Product> Products, int TotalCount, int PageNumber, int PageSize);
    public class GetProductPagQueryHandler(IDocumentSession session) : IQueryHandler<GetProductsPagQuery, GetProductsPagResult>
    {
        public async Task<GetProductsPagResult> Handle(GetProductsPagQuery query, CancellationToken cancellationToken)
        {
            var totalCount = await session.Query<Product>().CountAsync(cancellationToken);

            var products = await session.Query<Product>()
                .ToPagedListAsync(query.PageNumber, query.PageSize, cancellationToken);

            return new GetProductsPagResult(products, totalCount, query.PageNumber, query.PageSize);
        }
    }
}
