# Evidencias de pruebas — Orders.API

Fecha de ejecución local: 13 de agosto de 2026.

Entorno: Orders.API real, MongoDB 8 local y contratos simulados de Basket/Catalog con el mismo JSON de los servicios publicados.

| Prueba | Resultado observado |
|---|---|
| P1 Crear orden válida | `201 Created`, total `232.00` para subtotal `200.00` + IVA 16%. |
| P2 Consultar orden | `200 OK`, una partida completa. |
| P3 Basket vacío | `400 Bad Request`. |
| P4 Idempotencia | Reintento `200 OK`, mismo ID y una sola orden por cliente. |
| P5 Pending → Confirmed | `200 OK`. |
| P6 Transición inválida | `409 Conflict`. |
| P7 MongoDB no disponible | `500 Internal Server Error`, Problem Details controlado y sin stack trace. |
| P8 Frontend | TypeScript y build de producción correctos; flujo compra → confirmación → historial incluido. |

Para la evidencia final debe repetirse la colección Postman contra la URL pública y capturar MongoDB Atlas mostrando un único documento para P1/P4.
