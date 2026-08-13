namespace Orders.API.Application;

public sealed class OrderValidationException(string message) : Exception(message);
public sealed class OrderNotFoundException(Guid id) : Exception($"No se encontró la orden {id}.");
public sealed class IdempotencyConflictException() : Exception("Idempotency-Key ya fue utilizada para otra solicitud.");
