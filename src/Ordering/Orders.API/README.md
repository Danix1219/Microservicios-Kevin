# Orders.API

Microservicio ASP.NET Core Minimal API para crear y administrar órdenes de compra. Consume `Basket.API` y `Catalog.API`, conserva la instantánea de precios y persiste las órdenes en MongoDB Atlas.

## Arquitectura

```text
Orders/
  CreateOrder/           Command + Validator + Handler + Carter Endpoint
  UpdateOrderStatus/     Command + Validator + Handler + Carter Endpoint
  GetOrderById/          Query + Handler + Carter Endpoint
  GetOrdersByCustomer/   Query + Handler + Carter Endpoint
  Shared/                Responses, mappings y excepciones compartidas
Models/                  Order, OrderItem, estados y transiciones
Data/                    MongoDB, repositorio, índices y opciones
Services/                Clientes HTTP para Basket y Catalog
Exceptions/              Respuestas de error controladas
```

El servicio aplica CQRS con los mismos BuildingBlocks del resto de la solución:

```text
HTTP → Carter Endpoint → MediatR/ISender → Command o Query → Handler → Repository
                                ↓
                   ValidationBehavior + LoggingBehavior
```

Los endpoints solo traducen HTTP a mensajes CQRS. Los comandos modifican estado y las consultas únicamente leen; la infraestructura de MongoDB permanece detrás de `IOrderRepository`.

## Configuración local

No existen secretos en `appsettings.json`. Usa User Secrets desde la carpeta del proyecto:

```powershell
dotnet user-secrets init
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb+srv://USUARIO:PASSWORD@CLUSTER.mongodb.net/?retryWrites=true&w=majority"
```

O configura la variable de entorno:

```text
MongoDb__ConnectionString=mongodb+srv://...
```

Variables disponibles:

```text
MongoDb__ConnectionString
MongoDb__DatabaseName=OrdersDb
MongoDb__CollectionName=orders
Services__BasketUrl=https://basketapi.onrender.com
Services__CatalogUrl=https://catalogapi-gchz.onrender.com
Order__TaxRate=0.16
Cors__AllowedOrigins__0=http://localhost:5173
```

## Ejecutar

```powershell
dotnet restore
dotnet run
```

- API: `http://localhost:6002`
- Swagger: `http://localhost:6002/swagger`
- Health: `http://localhost:6002/health`

## Endpoints

| Método | Ruta | Resultado |
|---|---|---|
| POST | `/api/orders` | Crea una orden desde el Basket. Requiere `Idempotency-Key`. |
| GET | `/api/orders/{id}` | Recupera el detalle completo. |
| GET | `/api/orders/customer/{customerId}` | Lista órdenes del cliente. |
| PATCH | `/api/orders/{id}/status` | Permite `Pending → Confirmed` o `Pending → Cancelled`. |

Al repetir una creación con la misma clave y el mismo request se devuelve `200 OK` con la orden existente. Reutilizar la clave con otro cliente o Basket produce `409 Conflict`.

## MongoDB Atlas

1. Crea un cluster y un usuario con acceso de lectura/escritura.
2. Autoriza la IP del servicio de Render en **Network Access**. Para una demostración puede usarse temporalmente `0.0.0.0/0` con contraseña robusta.
3. Configura `MongoDb__ConnectionString` como variable secreta en Render.
4. La base `OrdersDb`, colección `orders` e índices se crean automáticamente al insertar la primera orden.

## Publicación en Render

- Tipo: **Web Service / Docker**.
- Root Directory: `Microservicios_Basket/Microservicios`.
- Dockerfile: `src/Ordering/Orders.API/Dockerfile`.
- Health Check Path: `/health`.
- Agrega todas las variables anteriores como Environment Variables.

Para tu despliegue actual basta con estas variables en el servicio de Orders:

```text
MongoDb__ConnectionString=mongodb+srv://...
MongoDb__DatabaseName=OrdersDb
MongoDb__CollectionName=orders
Services__CatalogUrl=https://catalogapi-gchz.onrender.com
Services__BasketUrl=https://basketapi.onrender.com
Order__TaxRate=0.16
Cors__AllowedOrigins__0=https://URL-DE-TU-FRONTEND
```

Render no lee `docker-compose.override.yml` cuando publicas un único Web Service desde Dockerfile; estas claves deben registrarse en la sección **Environment** de Orders.API.

Después reemplaza `VITE_ORDERS_API_URL` del frontend con la URL pública real y vuelve a desplegarlo.

## Pruebas obligatorias

Usa [Orders.API.http](Orders.API.http) o importa las solicitudes manualmente en Postman:

- P1: Basket con productos + clave nueva → `201 Created`.
- P2: `GET` por el identificador recibido → `200 OK`.
- P3: cliente con Basket vacío → `400 Bad Request`.
- P4: repetir exactamente la clave de P1 → `200 OK`, mismo `id`, sin documento duplicado.
- P5: `PATCH` a `Confirmed` desde `Pending` → `200 OK`.
- P6: intentar cambiar una orden `Cancelled` a `Confirmed` → `409 Conflict`.
- P7: cadena MongoDB inaccesible → error controlado `500`, sin stack trace en el response.
- P8: desde Vue, agregar al carrito, pulsar **Realizar compra** y observar la confirmación.

## Decisiones técnicas

- Carter descubre los módulos HTTP y MediatR resuelve cada handler por caso de uso.
- `CreateOrder` y `UpdateOrderStatus` son comandos; `GetOrderById` y `GetOrdersByCustomer` son consultas.
- FluentValidation se ejecuta en el pipeline antes de los handlers de comandos.
- El precio del Basket debe coincidir con Catalog al comprar; Orders consulta `GET /products`, que ya existe en el Catalog publicado, por lo que no requiere volver a desplegar Catalog. La orden guarda ese valor y no lo recalcula posteriormente.
- Un índice único en `IdempotencyKey` protege contra concurrencia y reintentos.
- El cambio de estado usa un filtro atómico que exige el estado `Pending`.
- Los logs solo contienen identificadores; nunca se registra la cadena de MongoDB.
