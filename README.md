# Basket, Catalog & Orders

Solución de comercio electrónico basada en microservicios:

- `Catalog.API`: catálogo y administración de productos con PostgreSQL.
- `Basket.API`: carrito por usuario con PostgreSQL y Redis.
- `Orders.API`: órdenes de compra con MongoDB Atlas, idempotencia e integración HTTP.

La documentación técnica del nuevo servicio se encuentra en [src/Ordering/Orders.API/README.md](src/Ordering/Orders.API/README.md).

> Los secretos de PostgreSQL, Redis y MongoDB deben configurarse mediante variables de entorno. No deben guardarse en Git.

Para Docker Compose local, copia `.env.example` como `.env`, configura las conexiones de Neon, Redis y MongoDB Atlas, y ejecuta `docker compose up --build`.

`Orders.API` consume por defecto las APIs ya publicadas:

- `https://catalogapi-gchz.onrender.com`
- `https://basketapi.onrender.com`

Por ello puede desplegarse de manera independiente sin levantar `catalog.api`, `basket.api`, PostgreSQL o Redis desde Compose.
