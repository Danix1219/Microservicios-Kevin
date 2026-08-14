using System.Net;

namespace Orders.API.Services;

public record BasketItemDto(int Quantity, string Color, decimal Price, Guid ProductId, string ProductName);
public record BasketDto(string UserName, IReadOnlyCollection<BasketItemDto> Items, decimal TotalPrice);
internal record BasketEnvelope(BasketDto Cart);

public interface IBasketClient
{
    Task<BasketDto> GetBasketAsync(string basketId, CancellationToken cancellationToken);
    Task DeleteBasketAsync(string basketId, CancellationToken cancellationToken);
}

public sealed class BasketClient(HttpClient httpClient) : IBasketClient
{
    public async Task<BasketDto> GetBasketAsync(string basketId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"/basket/{Uri.EscapeDataString(basketId)}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new BasketDto(basketId, [], 0);
        }
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<BasketEnvelope>(cancellationToken)
            ?? throw new ExternalServiceException("Basket.API devolvió una respuesta inválida.");
        return envelope.Cart;
    }

    public async Task DeleteBasketAsync(string basketId, CancellationToken cancellationToken)
    {
        var response = await httpClient.DeleteAsync($"/basket/{Uri.EscapeDataString(basketId)}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
