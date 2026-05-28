# Food Delivery Platform — Project Context for Claude Code

## Project Overview

**Solution name:** FoodDelivery  
**Type:** Restaurant food delivery web platform  
**Architecture:** Microservices + Domain-Driven Design + Clean Architecture  
**Stack:** C# ASP.NET Core, Entity Framework Core, PostgreSQL, RabbitMQ, Docker, JWT, YARP

---

## Solution Structure

```
food-delivery-dyploma/
└── src/
    ├── ApiGateway/
    │   └── ApiGateway/
    ├── BuildingBlocks/
    │   ├── Auth/
    │   ├── Contracts/
    │   ├── EventBus/
    │   ├── Observability/
    │   └── Shared/
    └── Services/
        ├── AuthService/          — partially implemented, use as primary pattern reference
        ├── RestaurantService/    — partially implemented, use as pattern reference
        ├── OrderService/         — partially implemented, needs full rework
        ├── PaymentService/       — partially implemented, needs full rework
        ├── NotificationService/  — partially implemented, needs full implementation
        └── DeliveryService/      — partially implemented, needs full implementation
```

**IMPORTANT:** Every service in this solution is partially implemented to varying degrees.
Before writing any code in a service — always read ALL existing files in that service first.
Never assume anything is complete. Never overwrite existing code without reading it first.

---

## Each Microservice — Layer Structure (Clean Architecture)

Every service follows this exact 4-layer structure:

```
ServiceName/
├── ServiceName.API/
│   ├── Controllers/
│   ├── Contracts/          # Request/Response DTOs for this service's API
│   ├── Extensions/
│   ├── Middlewares/
│   └── Properties/
├── ServiceName.Application/
│   ├── Behaviors/
│   ├── Commands/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Queries/
│   ├── Services/
│   └── Validators/
├── ServiceName.Domain/
│   ├── Entities/
│   ├── Enums/
│   ├── Events/
│   ├── Exceptions/
│   └── ValueObjects/
└── ServiceName.Infrastructure/
    ├── Authentication/
    ├── BackgroundJobs/
    ├── Configurations/
    ├── Messaging/
    ├── Persistence/
    └── Repositories/
```

---

## BuildingBlocks Libraries

These are shared libraries used by all services. Read them carefully before implementing anything.

### Auth
Shared JWT validation middleware and abstractions used by all services.

### Contracts
Integration event contracts shared between services. Read existing contracts before creating new ones.
Known events:
- `PaymentCompleted` — published by PaymentService on success
- `PaymentFailed` — published by PaymentService on failure
- `NotificationRequested` — published by OrderService to trigger notifications

### EventBus
Abstractions and RabbitMQ implementation for publishing/consuming integration events.

### Shared
Base abstractions: base entity, repository interfaces, value objects, primitives, errors.

### Observability
Centralized logging and diagnostics components.

---

## Services — Responsibilities & Requirements

### AuthService
**Responsibility:** User authentication and authorization

**Roles (UserRole enum):** Customer, RestaurantManager, Courier, Administrator

**Key operations:** Register, Login, RefreshToken, GetCurrentUser

**JWT token contains:** userId, role

---

### RestaurantService
**Responsibility:** Restaurant and menu management

**Access:**
- RestaurantManager: CRUD own restaurants and menu items
- Customer: read restaurants and menu

---

### OrderService — CENTRAL SERVICE, implement first
**Responsibility:** Order lifecycle management and workflow coordination

**Order flow:**
1. Customer POST /orders → API Gateway → OrderService
2. OrderService validates via sync REST call to RestaurantService (restaurant exists, menu items available)
3. Creates Order aggregate with status `PendingPayment`
4. Saves `PaymentRequested` event to outbox_messages table (same DB transaction)
5. Background Outbox worker publishes event to RabbitMQ
6. PaymentService processes and publishes result
7. OrderService consumes result:
   - `PaymentCompleted` → status = `Paid`, publish `NotificationRequested`
   - `PaymentFailed` → status = `PaymentFailed`, publish `NotificationRequested`
8. NotificationService consumes `NotificationRequested`

**Order statuses:** PendingPayment → Paid → InPreparation → ReadyForDelivery → InDelivery → Delivered
Failed: PendingPayment → PaymentFailed

**DB schema (OrderDB):**

Table `orders`:
- id (Guid PK), correlation_id (Guid), client_id (Guid), restaurant_id (Guid)
- total_amount (decimal), status (OrderStatus)
- payment_requested (bool), payment_completed (bool), payment_failed (bool)
- created_at (DateTime UTC)

Table `order_items`:
- id (Guid PK), order_id (Guid FK), menu_item_id (Guid)
- item_name (string), unit_price (decimal), quantity (int), line_total (decimal)

Table `outbox_messages`:
- id (Guid PK), type (string), payload (string JSON)
- occurred_at (DateTime UTC), processed (bool), processed_at (DateTime UTC?)

Table `processed_events`:
- event_id (Guid PK), processed_at (DateTime UTC)

**Required patterns:** Outbox Pattern, Domain Events, Repository Pattern, Idempotent event consumption

---

### PaymentService
**Responsibility:** Payment processing (no external provider for MVP — simulate internally)

**Flow:**
1. Consumes `PaymentRequested` from RabbitMQ
2. Creates Payment record, simulates processing
3. Publishes `PaymentCompleted` or `PaymentFailed`

**DB schema (PaymentDB):**

Table `payments`:
- id (Guid PK), order_id (Guid), correlation_id (Guid), amount (decimal)
- status (PaymentStatus: Pending/Completed/Failed)
- created_at (DateTime UTC), processed_at (DateTime UTC?)

---

### NotificationService
**Responsibility:** Async notification processing

**Flow:**
1. Consumes `NotificationRequested` from RabbitMQ
2. Persists notification record
3. For MVP: log only, no actual email/push delivery

**DB schema (NotificationDB):**

Table `notifications`:
- id (Guid PK), user_id (Guid), order_id (Guid)
- message (string), type (NotificationType)
- sent_at (DateTime UTC), created_at (DateTime UTC)

---

### DeliveryService
**Responsibility:** Delivery coordination and courier management

**Flow:**
1. Consumes `PaymentCompleted` or a `DeliveryRequested` event
2. Creates Delivery with status `Pending`
3. Courier assigns → `Assigned`, picks up → `InProgress`, delivers → `Delivered`
4. Publishes `DeliveryCompleted` → OrderService updates order to `Delivered`

**DB schema (DeliveryDB):**

Table `deliveries`:
- id (Guid PK), order_id (Guid), courier_id (Guid?)
- status (DeliveryStatus: Pending/Assigned/InProgress/Delivered/Failed)
- created_at (DateTime UTC), updated_at (DateTime UTC)

---

## Integration Events

| Event | Published by | Consumed by |
|---|---|---|
| `PaymentRequested` | OrderService (Outbox) | PaymentService |
| `PaymentCompleted` | PaymentService | OrderService, NotificationService, DeliveryService |
| `PaymentFailed` | PaymentService | OrderService, NotificationService |
| `NotificationRequested` | OrderService | NotificationService |
| `DeliveryRequested` | OrderService | DeliveryService |
| `DeliveryCompleted` | DeliveryService | OrderService |

---

## User Roles & Access

| Role | Services |
|---|---|
| Customer | AuthService, RestaurantService (read), OrderService (create/view own) |
| RestaurantManager | RestaurantService (CRUD), OrderService (view/update restaurant orders) |
| Courier | DeliveryService (view assigned, update status) |
| Administrator | All services |

---

## Infrastructure

- **Databases:** PostgreSQL, one DB per service (database-per-service pattern)
- **ORM:** Entity Framework Core with migrations
- **Message broker:** RabbitMQ (AMQP)
- **Containerization:** Docker + Docker Compose (partial, needs completion)
- **API docs:** Swagger / OpenAPI per service
- **Auth:** JWT Bearer, stateless, validated at Gateway and per-service
- **Logging:** Centralized via Observability BuildingBlock

---

## Architectural Rules (never violate)

1. Services NEVER share databases — no cross-DB queries ever
2. Services communicate only via REST API (sync) or RabbitMQ events (async)
3. All external requests go through API Gateway
4. Outbox Pattern is mandatory in OrderService for guaranteed event delivery
5. Idempotency via `processed_events` — never process the same event twice
6. Each service has its own EF Core DbContext and migrations
7. JWT claims (userId, role) come from token — no extra auth service calls

---

## Old Demo Reference (external path)

Location: `C:\Users\Kirill\Desktop\Diploma Final Project\food-delivery\services\`
Contains: OrderService and PaymentService demo implementations

**Use ONLY for:** understanding existing business logic, domain model fields, status enums
**Do NOT use for:** architecture, structure, patterns — those are wrong in the demo
**Always prefer:** patterns from AuthService and RestaurantService in the current solution

---

## Implementation Priority (MVP)

1. **OrderService** — central service, most complex, implement first
2. **PaymentService** — required to complete the order flow
3. **NotificationService** — simplest, consumes events only
4. **DeliveryService** — delivery coordination
5. **API Gateway routes** — add routes for all new services
6. **docker-compose.yml** — complete for all services

---

## Session Progress (2026-05-28, updated)

Branch: `feature/implement-services`

### Completed

**BuildingBlocks/Contracts/Events/** — 6 new integration event classes:
`PaymentRequestedEvent`, `PaymentCompletedEvent`, `PaymentFailedEvent`,
`NotificationRequestedEvent`, `DeliveryRequestedEvent`, `DeliveryCompletedEvent`
All inherit `Contracts.Abstractions.IntegrationEvent`. All in namespace `Contracts.Events`.

**BuildingBlocks/Shared/Primitives/OutboxMessage.cs** — added `bool Processed` property.

**Directory.Packages.props** — added `Microsoft.Extensions.Http 9.0.0`.

**OrderService** — complete (all 4 layers + EF migration):
- Domain: `Order` aggregate, `OrderItem` entity, `OrderStatus` enum
- Application: `IOrderRepository` (includes `AddOutboxMessageAsync`), `IRestaurantServiceClient`, 6 DTOs, `CreateOrderRequestValidator`
- Infrastructure: `OrderDbContext`, 4 EF configs, `OrderRepository`, `RestaurantServiceHttpClient` (typed HttpClient), `OutboxPublisherWorker` (polls every 2s), `PaymentCompletedConsumer`, `PaymentFailedConsumer`, `DependencyInjection`
- API: `OrdersController` (POST /api/orders [Customer], GET /api/orders/{id}, GET /api/orders), full `Program.cs`, `appsettings.json`
- Migration: `20260528145126_InitialCreate` — tables: orders, order_items, outbox_messages, processed_events

**PaymentService** — complete (all 4 layers + EF migration):
- Domain: `Payment` entity, `PaymentStatus` enum (Pending/Completed/Failed)
- Application: `IPaymentRepository`
- Infrastructure: `PaymentDbContext`, `PaymentConfiguration`, `ProcessedEventConfiguration`, `PaymentRepository`, `PaymentRequestedConsumer` (simulates with Random), `DependencyInjection`
- API: `Program.cs` (event-driven only, no controllers), `appsettings.json`
- Migration: `20260528145659_InitialCreate` — tables: payments, processed_events

### Decisions / Deviations
- No MediatR used — consistent with AuthService/RestaurantService pattern (direct controller → repository)
- `ProcessedEvent` is a service-local class in each service's `Infrastructure/Persistence/` (not from Shared) because PK column is `event_id` not `id`
- `OutboxMessage.Processed` (bool) added to Shared — needed for Outbox worker query
- PaymentService publishes events **directly** via IEventBus (no Outbox) — Outbox is only mandatory for OrderService per CONTEXT.md
- `IOrderRepository.AddOutboxMessageAsync` added to allow atomic order+outbox save in one transaction from the controller
- `Microsoft.Extensions.Hosting.Abstractions` must be explicitly referenced in each Infrastructure csproj for `BackgroundService`
- Each API csproj needs `Microsoft.EntityFrameworkCore.Design` (design-time only) for `dotnet ef migrations` CLI

### RabbitMQ routing keys (canonical)
| Event | Routing Key | Publisher | Consumer queue pattern |
|---|---|---|---|
| PaymentRequestedEvent | `payment.requested` | OrderService Outbox | `payment-service.payment.requested` |
| PaymentCompletedEvent | `payment.completed` | PaymentService | `order-service.payment.completed`, `notification-service.payment.completed`, `delivery-service.payment.completed` |
| PaymentFailedEvent | `payment.failed` | PaymentService | `order-service.payment.failed`, `notification-service.payment.failed` |
| NotificationRequestedEvent | `notification.requested` | OrderService Outbox | `notification-service.notification.requested` |
| DeliveryRequestedEvent | `delivery.requested` | OrderService Outbox | `delivery-service.delivery.requested` |
| DeliveryCompletedEvent | `delivery.completed` | DeliveryService | `order-service.delivery.completed` |

**NotificationService** — complete (all 4 layers + EF migration):
- Domain: `Notification` entity (id, user_id, order_id, message, type, sent_at, created_at)
- Application: `INotificationRepository`
- Infrastructure: `NotificationDbContext`, `NotificationConfiguration`, `ProcessedEventConfiguration`, `NotificationRepository`, `NotificationRequestedConsumer` (queue: `notification-service.notification.requested`), `DependencyInjection`
- API: `Program.cs` (event-driven only, no controllers), `appsettings.json` (DB port 5438)
- Migration: tables: notifications, processed_events

**DeliveryService** — complete (all 4 layers + EF migration):
- Domain: `Delivery` entity, `DeliveryStatus` enum (Pending/Assigned/InProgress/Delivered/Failed)
- Application: `IDeliveryRepository` (GetByIdAsync, GetByOrderIdAsync, AddAsync, SaveChangesAsync)
- Infrastructure: `DeliveryDbContext`, `DeliveryConfiguration`, `ProcessedEventConfiguration`, `DeliveryRepository`, `DeliveryRequestedConsumer` (queue: `delivery-service.delivery.requested`), `PaymentCompletedConsumer` (queue: `delivery-service.payment.completed`, idempotent — skips if delivery for OrderId already exists), `DependencyInjection`
- API: `DeliveriesController` (GET /{id}, PATCH /{id}/assign [Courier], PATCH /{id}/pickup [Courier], PATCH /{id}/deliver [Courier] — publishes DeliveryCompletedEvent directly via IEventBus), `Program.cs`, `appsettings.json` (DB port 5440)
- Migration: tables: deliveries, processed_events

**OrderService** — `DeliveryCompletedConsumer` added (queue: `order-service.delivery.completed`, calls `order.MarkDelivered()`). Registered in DependencyInjection.

### csproj fixes applied
- Both new Domain csproj files needed explicit `Shared` ProjectReference
- Both new Infrastructure csproj files needed `Microsoft.Extensions.Hosting.Abstractions` and `RabbitMQ.Client` (matching PaymentService.Infrastructure pattern)
- Both new API csproj files needed `Microsoft.EntityFrameworkCore.Design` (matching PaymentService.API pattern, required for `dotnet ef migrations`)

**RestaurantService** — GET endpoints added:
- Added `GetAllAsync` to `IRestaurantRepository` + implementation (eager-loads MenuItems)
- Added `GET /api/restaurants` [Authorize] — returns list with `{Id, Name, Status(string), MenuItems:[{Id,Name,Price,IsAvailable}]}`
- Added `GET /api/restaurants/{restaurantId}` [Authorize] — returns single restaurant same shape
- Both endpoints return Status as `.ToString()` so OrderService can compare with "Open"

**ApiGateway** — complete (YARP 2.3.0):
- `Directory.Packages.props`: added `Yarp.ReverseProxy 2.3.0`
- `ApiGateway.csproj`: only `Yarp.ReverseProxy` package reference
- `Program.cs`: `AddReverseProxy().LoadFromConfig(...)` + `MapReverseProxy()`
- `appsettings.json`: 4 routes + 4 clusters (auth_cluster, restaurant_cluster, order_cluster, delivery_cluster) — underscore IDs for env var override compatibility
- Local addresses: auth→:5262, restaurant→:5265, order→:5223, delivery→:5085

**docker-compose.yml** — complete and verified running:
- All 6 postgres DBs with `pg_isready` healthchecks
- 6 service containers + api-gateway (build context: `.` repo root)
- Env var overrides for DB connection strings (container hostnames) and YARP cluster addresses
- Services wait for DB `service_healthy` + rabbitmq `service_healthy`
- `Database.Migrate()` added to all 6 service `Program.cs` files — migrations run automatically on first `docker-compose up`
- `.dockerignore` at repo root; 7 Dockerfiles in `src/Services/{Name}/Dockerfile` + `src/ApiGateway/Dockerfile`
- DeliveryService appsettings port fixed: 5440→5437 to match docker-compose

### Verified running
`docker-compose up --build` completed successfully:
- All 7 images built (sdk:9.0 → aspnet:9.0 multi-stage)
- All 6 DB migrations applied on first boot
- All services: `Application started. Now listening on: http://[::]:8080`
- RabbitMQ: all consumer connections established
- OrderService Outbox worker polling every 2s (expected steady-state)

### Branch status (2026-05-28)
`feature/implement-services` merged to `master`.

---

## Session Progress (2026-05-29)

Branch: `master` (all changes committed directly)

### Completed

**RestaurantService — CQRS refactor + missing endpoints (Step 1):**
- Application layer: 6 Command handlers (`CreateRestaurant`, `AddMenuItem`, `UpdateMenuItem`, `DeleteMenuItem`, `OpenRestaurant`, `CloseRestaurant`) + 3 Query handlers (`GetAll`, `GetById`, `GetMenuItems`) — each command + handler in one file under `Commands/` / `Queries/`
- `CommandResult` (sealed, `Common/`) with `Success/NotFound/Forbidden/Conflict` statuses + optional `CreatedId`
- Response DTOs: `RestaurantResponse`, `MenuItemResponse` with `FromDomain()` factory, `UpdateMenuItemRequest`
- Domain: `MenuItem.Update(name, price)`, `Restaurant.RemoveMenuItem(menuItemId) → bool`
- **New endpoints:** `PUT /{id}/menu-items/{itemId}`, `DELETE /{id}/menu-items/{itemId}`, `GET /{id}/menu-items`
- Controller slimmed to pure HTTP dispatch — zero business logic
- **Role fixed:** all `[Authorize(Roles = "RestaurantOwner")]` → `"RestaurantManager"` throughout
- `UserRole` enum: `RestaurantOwner = 2` → `RestaurantManager = 2` (int value unchanged — no DB impact)

**ApiGateway — full routing (Step 2):**
- All 6 services routed: auth→5001, restaurant→5002, order→5003, payment→5004, notification→5005, delivery→5006
- JWT validation wired via `BuildingBlocks/Auth` (`AddJwtAuthentication`) + `UseAuthentication/UseAuthorization`
- `X-Correlation-Id` middleware: generates a new GUID if header absent; YARP forwards it automatically to all downstream services
- Swagger UI aggregates all 6 services (payment + notification added)
- Auth BuildingBlock added as ProjectReference to ApiGateway.csproj

**OrderService — CQRS + status endpoint (Step 3):**
- Commands: `PlaceOrderCommand + PlaceOrderCommandHandler` (full validation + outbox), `UpdateOrderStatusCommand + UpdateOrderStatusCommandHandler`
- Queries: `GetOrderByIdQuery + GetOrderByIdQueryHandler` (ownership check), `GetOrdersByClientQuery + GetOrdersByClientQueryHandler`
- `CommandResult` with `UnprocessableEntity` status + `Error` message; generic `QueryResult<T>` (Success/NotFound/Forbidden)
- Domain: `OrderDomainException`; `StartPreparation()` and `MarkReadyForDelivery()` now throw on invalid state
- **New endpoint:** `PATCH /api/orders/{id}/status [RestaurantManager]` — allows `InPreparation`, `ReadyForDelivery` transitions
- Controller fully slimmed

**PaymentService — controller added (Step 4):**
- `IPaymentRepository.GetByOrderIdAsync` + implementation
- `GetPaymentByOrderIdQueryHandler`, `PaymentResponse` DTO
- `PaymentsController`: `GET /api/payments/{orderId} [Authorize]`
- Swagger enabled in all environments

**NotificationService — controller added (Step 5):**
- `INotificationRepository.GetByUserIdAsync` (ordered by CreatedAt desc) + implementation
- `GetNotificationsByUserQueryHandler`, `NotificationResponse` DTO
- `NotificationsController`: `GET /api/notifications [Authorize]` (scoped to current user)
- Swagger enabled in all environments

**DeliveryService — full CQRS + new endpoint (Step 6):**
- `IDeliveryRepository.GetByCourierIdAsync` + implementation
- Commands: `AssignDelivery`, `PickupDelivery`, `CompleteDelivery` (+ handlers; `CompleteDelivery` publishes `DeliveryCompletedEvent` via IEventBus)
- Queries: `GetDeliveryById`, `GetDeliveriesByCourier` (+ handlers)
- `DeliveryResponse.FromDomain()` DTO factory
- **New endpoint:** `GET /api/deliveries [Courier]` — lists courier's own deliveries
- Controller fully slimmed; EventBus added as ProjectReference to Application csproj

**AuthService — RefreshToken (Step 9):**
- `User` entity: `RefreshToken (string?)`, `RefreshTokenExpiry (DateTime?)`, `SetRefreshToken(token, expiry)` method
- `IJwtProvider.GenerateRefreshToken()` → cryptographically random base64(64 bytes)
- `IUserRepository.GetByRefreshTokenAsync` + implementation
- `AuthResponse`: now returns both `AccessToken` + `RefreshToken`
- `POST /api/auth/refresh` endpoint: validates token, checks expiry, rotates (7-day sliding window)
- Register and Login now issue + persist refresh tokens atomically
- EF migration `20260529000000_AddRefreshToken` — adds `RefreshToken`, `RefreshTokenExpiry` columns to `Users` table

**Unit tests — xUnit/Moq/FluentAssertions (Step 7):**
- `Directory.Packages.props`: added xunit 2.9.2, Moq 4.20.72, FluentAssertions 6.12.0, Microsoft.NET.Test.Sdk 17.12.0, coverlet.collector 6.0.2
- `tests/AuthService.Tests` (21 tests): `UserEntityTests`, `JwtProviderTests`, `AuthControllerTests` (register/login/refresh all paths)
- `tests/OrderService.Tests` (26 tests): `OrderAggregateTests` (all transitions + domain exceptions), `OrderItemTests`, `PlaceOrderCommandHandlerTests`, `UpdateOrderStatusCommandHandlerTests`
- `tests/PaymentService.Tests` (13 tests): `PaymentEntityTests` (all states), `GetPaymentByOrderIdQueryHandlerTests`
- **60 tests, 0 failures** — `dotnet test` confirmed passing

### CQRS handler pattern (used in all services from this session)
- Command/query class + handler class live in the **same file** under `Application/Commands/` or `Application/Queries/`
- Handlers are registered as `Scoped` in Infrastructure `DependencyInjection.cs`
- Controllers inject handlers directly (no MediatR)
- `CommandResult` / `QueryResult<T>` in `Application/Common/` per service — controllers switch on status, return HTTP result

### Service port map (local development)
| Service | Port |
|---|---|
| AuthService | 5001 |
| RestaurantService | 5002 |
| OrderService | 5003 |
| PaymentService | 5004 |
| NotificationService | 5005 |
| DeliveryService | 5006 |
| ApiGateway | (default, proxies all above) |

### Remaining known issues (cosmetic, not blocking)
- DataProtection warning on all services (keys not persisted outside container) — harmless for MVP
- docker-compose service ports may differ from local dev ports above — check docker-compose env var overrides before running locally

### Nothing left to implement for MVP
All services, CQRS application layers, unit tests, and documentation are complete.
Next steps if needed:
1. End-to-end smoke test: `docker-compose up --build`, register user, place order, verify full saga
2. Additional test coverage: RestaurantService and DeliveryService handlers
3. Integration tests with real DB (TestContainers)
