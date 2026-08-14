namespace Orders.API.Data;

public sealed class MongoIndexInitializer(
    IServiceScopeFactory scopeFactory,
    ILogger<MongoIndexInitializer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<MongoOrderRepository>().EnsureIndexesAsync(stoppingToken);
            logger.LogInformation("Índices de órdenes verificados correctamente.");
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "MongoDB no está disponible al iniciar. La API seguirá activa y responderá errores controlados.");
        }
    }
}
