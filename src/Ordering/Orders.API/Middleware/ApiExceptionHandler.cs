using Microsoft.AspNetCore.Diagnostics;
using MongoDB.Driver;
using Orders.API.Application;
using Orders.API.Domain;
using Orders.API.Infrastructure.Clients;

namespace Orders.API.Middleware;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            OrderValidationException => (StatusCodes.Status400BadRequest, "Solicitud inválida", exception.Message),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Solicitud inválida", "El cuerpo de la solicitud no tiene el formato esperado."),
            OrderNotFoundException => (StatusCodes.Status404NotFound, "Orden no encontrada", exception.Message),
            InvalidOrderStatusTransitionException => (StatusCodes.Status409Conflict, "Transición de estado inválida", exception.Message),
            IdempotencyConflictException => (StatusCodes.Status409Conflict, "Conflicto de idempotencia", exception.Message),
            ExternalServiceException => (StatusCodes.Status503ServiceUnavailable, "Servicio externo no disponible", "No fue posible validar el carrito o el catálogo."),
            HttpRequestException => (StatusCodes.Status503ServiceUnavailable, "Servicio externo no disponible", "No fue posible validar el carrito o el catálogo."),
            MongoException => (StatusCodes.Status500InternalServerError, "Error de persistencia", "No fue posible acceder al almacén de órdenes."),
            TimeoutException => (StatusCodes.Status500InternalServerError, "Error de persistencia", "No fue posible acceder al almacén de órdenes."),
            _ => (StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado al procesar la solicitud.")
        };

        logger.LogError(exception, "Solicitud de órdenes fallida. StatusCode: {StatusCode}; TraceId: {TraceId}", status, context.TraceIdentifier);
        context.Response.StatusCode = status;
        await Results.Problem(statusCode: status, title: title, detail: detail, instance: context.Request.Path)
            .ExecuteAsync(context);
        return true;
    }
}
