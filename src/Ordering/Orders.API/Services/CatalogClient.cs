namespace Orders.API.Services;

public record CatalogProductDto(Guid Id, string Name, decimal Price);
internal record CatalogProductsEnvelope(IReadOnlyCollection<CatalogProductDto> Products);

public interface ICatalogClient
{
    Task<CatalogProductDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken);
}

public sealed class CatalogClient(HttpClient httpClient) : ICatalogClient
{
    public async Task<CatalogProductDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync("/products", cancellationToken);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<CatalogProductsEnvelope>(cancellationToken)
            ?? throw new ExternalServiceException("Catalog.API devolvió una respuesta inválida.");
        return envelope.Products.FirstOrDefault(product => product.Id == productId);
    }
}

public sealed class ExternalServiceException(string message, Exception? innerException = null)
    : Exception(message, innerException);
