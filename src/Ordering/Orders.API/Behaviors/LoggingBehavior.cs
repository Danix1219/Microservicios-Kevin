using System.Diagnostics;

namespace Orders.API.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("[Inicio] Procesando {RequestName}", requestName);

        var timer = Stopwatch.StartNew();
        try
        {
            return await next();
        }
        finally
        {
            timer.Stop();
            if (timer.Elapsed > TimeSpan.FromSeconds(3))
            {
                logger.LogWarning(
                    "[Rendimiento] {RequestName} tardó {ElapsedMilliseconds} ms",
                    requestName,
                    timer.ElapsedMilliseconds);
            }

            logger.LogInformation(
                "[Fin] {RequestName} procesado en {ElapsedMilliseconds} ms",
                requestName,
                timer.ElapsedMilliseconds);
        }
    }
}
