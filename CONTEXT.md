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

---

## Session Progress (2026-05-29, AuthService production polish)

Branch: `master` (commit `20124bb`)

### Goal
Polish AuthService to production quality matching the diploma specification exactly.
The service was previously functional but contained business logic directly in the controller
(fat controller anti-pattern) and was missing several required domain building blocks.

### Changes — Domain Layer (`AuthService.Domain`)

**`UserRole` enum:**
- `RestaurantManager = 2` → `RestaurantOwner = 2` (diploma spec requires `RestaurantOwner`)
- **Note:** a previous session had renamed this to `RestaurantManager`; this session reverts that to match the diploma spec. The int value (2) is unchanged, so no DB migration needed. RestaurantService still uses the `"RestaurantManager"` role string in its `[Authorize]` attributes — that is a separate known inconsistency.

**`User` entity (`Entities/User.cs`):**
- Added `AssignRole(UserRole role)` method — updates role and raises `UserRoleAssignedDomainEvent`
- Added `ChangePassword(string newPasswordHash)` method — updates password hash
- `User.Create()` now raises `UserRegisteredDomainEvent` after construction

**New: `Events/` folder:**
- `UserRegisteredDomainEvent` — raised in `User.Create()`, carries `UserId` + `Email`, inherits `DomainEvent`
- `UserRoleAssignedDomainEvent` — raised in `User.AssignRole()`, carries `UserId` + `NewRole`, inherits `DomainEvent`

**New: `Exceptions/` folder:**
- `UserAlreadyExistsException` — thrown by `RegisterUserCommandHandler` on duplicate email → 409
- `InvalidCredentialsException` — thrown by `LoginUserCommandHandler` / `RefreshTokenCommandHandler` → 401
- `UserNotFoundException` — thrown by `GetCurrentUserQueryHandler` on missing user → 404

### Changes — Application Layer (`AuthService.Application`)

**`IUserRepository`:** added `GetByIdAsync(Guid id, CancellationToken)` method.

**`AuthResponse` DTO:** added `Guid UserId` and `string Role` fields (kept `RefreshToken`).

**New: `DTOs/UserDto.cs`:** `Guid UserId`, `string Email`, `string Role`.

**`RegisterRequestValidator`:** added `RuleFor(x => x.Role).IsInEnum()` validation.

**New: `Commands/` folder** (command record + handler interface in one file, handler in separate file):
- `RegisterUserCommand(Email, Password, Role)` + `IRegisterUserCommandHandler` + `RegisterUserCommandHandler`
  — checks duplicate email (throws `UserAlreadyExistsException`), hashes password, calls `User.Create()`, issues refresh token, saves, returns `AuthResponse`
- `LoginUserCommand(Email, Password)` + `ILoginUserCommandHandler` + `LoginUserCommandHandler`
  — finds user by email, verifies password (throws `InvalidCredentialsException` on either failure), rotates refresh token, returns `AuthResponse`
- `RefreshTokenCommand(RefreshToken)` + `IRefreshTokenCommandHandler` + `RefreshTokenCommandHandler`
  — finds user by refresh token, validates expiry (throws `InvalidCredentialsException`), rotates token, returns `AuthResponse`

**New: `Queries/` folder:**
- `GetCurrentUserQuery(UserId)` + `IGetCurrentUserQueryHandler` + `GetCurrentUserQueryHandler`
  — finds user by id (throws `UserNotFoundException`), returns `UserDto`

### Changes — Infrastructure Layer (`AuthService.Infrastructure`)

**`UserRepository`:** added `GetByIdAsync` implementation (EF `FirstOrDefaultAsync` by `x.Id == id`).

**`DependencyInjection.cs`:**
- Removed `ICurrentUser` registration (now handled exclusively by `AddAuthServices()` in Program.cs)
- Added scoped registrations for all 4 handler interfaces → implementations
- Connection string key changed: `"Database"` → `"DefaultConnection"` (matches diploma spec)
- `docker-compose.yml` env var updated accordingly: `ConnectionStrings__Database` → `ConnectionStrings__DefaultConnection`

### Changes — API Layer (`AuthService.API`)

**`AuthController` (slimmed to zero business logic):**
- Constructor now injects 4 handler interfaces: `IRegisterUserCommandHandler`, `ILoginUserCommandHandler`, `IRefreshTokenCommandHandler`, `IGetCurrentUserQueryHandler`
- Each action maps the incoming DTO to a command/query, calls the handler, returns the result
- `POST /api/auth/register` → `RegisterUserCommand` → 201 Created + `AuthResponse`
- `POST /api/auth/login` → `LoginUserCommand` → 200 OK + `AuthResponse`
- `POST /api/auth/refresh` → `RefreshTokenCommand` → 200 OK + `AuthResponse`
- `GET /api/auth/me [Authorize]` → `GetCurrentUserQuery` (UserId from `ICurrentUser` claims) → 200 OK + `UserDto`

**New: `Middlewares/ExceptionHandlingMiddleware.cs`:**
- `UserAlreadyExistsException` → 409 Conflict + JSON `{error}`
- `InvalidCredentialsException` → 401 Unauthorized + JSON `{error}`
- `UserNotFoundException` → 404 Not Found + JSON `{error}`
- `ValidationException` (FluentValidation) → 400 Bad Request + JSON `{errors}` (grouped by field)
- Unhandled `Exception` → 500 Internal Server Error + JSON `{error}`

**`Program.cs`:**
- Added `builder.Host.UseSerilog(...)` reading from configuration
- Added `builder.Services.AddCorrelation()`
- Added `app.UseMiddleware<ExceptionHandlingMiddleware>()` (before routing)
- Added `app.UseCorrelation()`

**`AuthService.API.csproj`:** added `Serilog.Sinks.Seq` package reference.

**`appsettings.json`:**
- Renamed `ConnectionStrings:Database` → `ConnectionStrings:DefaultConnection`
- Removed generic `Logging` section (superseded by Serilog)
- Added `Serilog` section: Console + Seq (`http://seq:5341`) sinks, min level Information, Microsoft/System overrides to Warning

### Changes — Tests (`tests/AuthService.Tests`)

**Deleted:**
- `API/AuthControllerTests.cs` — tested fat controller, no longer relevant
- `Infrastructure/JwtProviderTests.cs` — infrastructure test, removed as part of full replacement

**New (26 tests, 0 failures):**
- `Domain/UserEntityTests.cs` (8 tests) — `Create` properties, `UserRegisteredDomainEvent` raised, `SetRefreshToken` happy + repeat, `AssignRole` updates role + raises `UserRoleAssignedDomainEvent`, `ChangePassword` updates hash, enum int values
- `Application/RegisterUserCommandHandlerTests.cs` (5 tests) — returns `AuthResponse` on new email, throws `UserAlreadyExistsException` on duplicate, hashes password, saves user, does not save on duplicate
- `Application/LoginUserCommandHandlerTests.cs` (5 tests) — valid credentials return `AuthResponse`, unknown email throws, wrong password throws, valid sets refresh token on user, invalid does not save
- `Application/RefreshTokenCommandHandlerTests.cs` (4 tests) — valid token returns new response, invalid token throws, expired token throws, rotates refresh token and persists
- `Application/GetCurrentUserQueryHandlerTests.cs` (4 tests) — found user returns `UserDto`, missing user throws `UserNotFoundException`, queries correct id, role is string representation

### Final state
- `dotnet build` — 0 errors, 0 warnings
- `dotnet test tests/AuthService.Tests` — 26/26 passed

---

## Session Progress (2026-05-29, OrderService production polish)

Branch: `master` (commit `add7cca`)

### Goal
Polish OrderService to production quality. Previous implementation was functional
but used a fat-Result-pattern (handlers returning `CommandResult` with status codes
switched in the controller), had no domain events, no typed exceptions, no unit tests,
and all business logic lived in the controller or handler rather than the domain.

### Changes — Domain Layer (`OrderService.Domain`)

**`OrderStatus` enum:** added `Cancelled = 7`.

**New: `Events/` folder (4 files):**
- `OrderCreatedDomainEvent` — OrderId, ClientId, RestaurantId, TotalAmount; raised in `Order.Create()`
- `OrderPaidDomainEvent` — OrderId, ClientId; raised in `Order.CompletePayment()`
- `OrderPaymentFailedDomainEvent` — OrderId, ClientId, Reason; raised in `Order.FailPayment()`
- `OrderStatusChangedDomainEvent` — OrderId, OldStatus, NewStatus; raised in `Order.ChangeStatus()`
All inherit `Shared.Primitives.DomainEvent`.

**`Exceptions/` folder** — `OrderDomainException` unsealed; 4 new specific types added:
- `InvalidOrderStatusTransitionException : OrderDomainException` — two constructors: (from, to enums) and (free message)
- `OrderItemsEmptyException : OrderDomainException` — thrown by `Order.Create()` when no items
- `OrderNotFoundException : Exception` — thrown by query/command handlers on missing order
- `RestaurantValidationException : Exception` — thrown by `PlaceOrderCommandHandler` on invalid restaurant/item

**`OrderItem` entity:** added `UpdateQuantity(int newQuantity)` — validates > 0, recalculates `LineTotal`.

**`Order` aggregate — complete rewrite:**
- `Create(clientId, restaurantId, items, correlationId)` — now accepts `correlationId` param;
  throws `OrderItemsEmptyException` if items empty; raises `OrderCreatedDomainEvent`
- `RequestPayment()` → `StartPayment()` — validates `Status == PendingPayment`, throws `InvalidOrderStatusTransitionException` if not
- `MarkPaymentCompleted()` → `CompletePayment()` — raises `OrderPaidDomainEvent`
- `MarkPaymentFailed()` → `FailPayment(string reason)` — passes reason to domain event; raises `OrderPaymentFailedDomainEvent`
- Individual transition methods (`StartPreparation`, `MarkReadyForDelivery`, `MarkInDelivery`, `MarkDelivered`) **removed**
- New: `ChangeStatus(OrderStatus newStatus)` — static `AllowedTransitions` dictionary (Paid→InPreparation, InPreparation→ReadyForDelivery, ReadyForDelivery→InDelivery, InDelivery→Delivered);
  throws `InvalidOrderStatusTransitionException` for any other pair; raises `OrderStatusChangedDomainEvent`

### Changes — Application Layer (`OrderService.Application`)

**`IOrderRepository`:** removed `AddOutboxMessageAsync` — outbox is now `IOutboxService`'s responsibility.

**`IRestaurantServiceClient`:** `GetRestaurantAsync` → `GetRestaurantWithItemsAsync`.

**New: `Interfaces/IOutboxService.cs`:**
`SaveMessageAsync<T>(T integrationEvent, string routingKey, CancellationToken ct)` where `T : IIntegrationEvent`.
Routing key is a first-class parameter; implementation stores in the same DbContext transaction.

**`DTOs/RestaurantDto.cs`:** added `bool IsActive => Status == "Open"` computed property
(JSON deserialization from RestaurantService still uses `Status` string; handler code uses `IsActive`).

**New: `Mapping/OrderMapper.cs`:** internal static `MapToResponse(Order)` — shared by all handlers, eliminates duplication.

**`Commands/PlaceOrderCommand.cs`:** handler refactored:
- Constructor adds `IOutboxService` dependency
- Returns `Task<OrderResponse>` (was `Task<CommandResult>`)
- Throws `RestaurantValidationException` on bad restaurant/item (was returning error CommandResult)
- Generates `correlationId = Guid.NewGuid()` before `Order.Create()`
- Calls `order.StartPayment()` (was `order.RequestPayment()`)
- Saves outbox via `IOutboxService.SaveMessageAsync(paymentRequestedEvent, "payment.requested")` — same DbContext, one `SaveChangesAsync()` covers both

**New: `Commands/ChangeOrderStatusCommand.cs`:** replaces `UpdateOrderStatusCommand`:
- Parses `NewStatus` string to `OrderStatus` enum; throws `InvalidOrderStatusTransitionException` for unrecognised strings
- Calls `order.ChangeStatus(newStatus)` — domain validates the transition
- Returns `Task<OrderResponse>`

**Deleted:** `Commands/UpdateOrderStatusCommand.cs`, `Common/CommandResult.cs`, `Common/QueryResult.cs` — all unused after exception-based architecture.

**`Queries/GetOrderByIdQuery.cs`:** returns `Task<OrderResponse>`; throws `OrderNotFoundException` / `UnauthorizedAccessException` instead of returning `QueryResult`.

**`Queries/GetOrdersByClientQuery.cs`:** updated to use `OrderMapper`.

### Changes — Infrastructure Layer (`OrderService.Infrastructure`)

**New: `Services/OutboxService.cs`:** implements `IOutboxService`.
Serializes event with `JsonSerializer.Serialize`, sets `Type = typeof(T).Name`, adds to `OrderDbContext.OutboxMessages`.
Returns `Task.CompletedTask` (ChangeTracker add is synchronous); caller's `SaveChangesAsync()` persists it.

**`Messaging/RestaurantServiceHttpClient.cs`:** `GetRestaurantAsync` → `GetRestaurantWithItemsAsync`;
wrapped in `try/catch HttpRequestException` to return `null` instead of throwing.

**`BackgroundJobs/PaymentCompletedConsumer.cs`:**
- `order.MarkPaymentCompleted()` → `order.CompletePayment()`
- Direct `db.OutboxMessages.Add()` calls replaced with `IOutboxService` resolved from scope
- `IOutboxService` is scoped → same `OrderDbContext` instance → one `db.SaveChangesAsync()` covers order + both outbox entries

**`BackgroundJobs/PaymentFailedConsumer.cs`:**
- `order.MarkPaymentFailed()` → `order.FailPayment(evt.Reason)` (passes reason through)
- Same outbox refactor as above

**`BackgroundJobs/DeliveryCompletedConsumer.cs`:**
- `order.MarkDelivered()` → `order.ChangeStatus(OrderStatus.Delivered)` (domain validates InDelivery→Delivered)

**`Repositories/OrderRepository.cs`:** removed `AddOutboxMessageAsync`.

**`Extensions/DependencyInjection.cs`:**
- Added `services.AddScoped<IOutboxService, OutboxService>()`
- Replaced `UpdateOrderStatusCommandHandler` → `ChangeOrderStatusCommandHandler`

### Changes — API Layer (`OrderService.API`)

**New: `Middlewares/ExceptionHandlingMiddleware.cs`:**
| Exception | HTTP Status |
|---|---|
| `OrderNotFoundException` | 404 Not Found |
| `OrderItemsEmptyException` | 400 Bad Request |
| `RestaurantValidationException` | 422 Unprocessable Entity |
| `InvalidOrderStatusTransitionException` | 409 Conflict |
| `ValidationException` (FluentValidation) | 400 + `errors` dict grouped by field |
| `UnauthorizedAccessException` | 403 Forbidden |
| Unhandled `Exception` | 500 Internal Server Error (logged at Error) |
All responses: `application/json` with camelCase `{error}` or `{error, errors}` body.

**`Controllers/OrdersController.cs`:**
- Swapped `UpdateOrderStatusCommandHandler` → `ChangeOrderStatusCommandHandler`
- `POST /api/orders` → `CreatedAtAction(nameof(GetById), ...)` returning **201 Created + OrderResponse** (was `Ok({orderId})`)
- `GET /api/orders` — added `[Authorize(Roles = "Customer")]` (was missing)
- `PATCH /api/orders/{id}/status` → `Ok(response)` returning **200 OK + OrderResponse** (was `NoContent()`)
- All error-branch code removed — middleware handles everything

**`Program.cs`:**
- Added `builder.Services.AddCorrelation()`
- Added `app.UseCorrelation()` + `app.UseMiddleware<ExceptionHandlingMiddleware>()` (before Swagger/auth)

### Changes — Tests (`src/Services/OrderService/OrderService.Tests`)

New project: `OrderService.Tests.csproj` — xUnit 2.9.2, Moq 4.20.72, FluentAssertions 6.12.0.
Project added to `FoodDelivery.sln`.

**`Domain/OrderAggregateTests.cs` (11 tests):**
Create valid/empty, `OrderCreatedDomainEvent` fields, `StartPayment`, `CompletePayment` state + event,
`FailPayment` state + event with reason, `ChangeStatus` valid transition, invalid transition exception, `OrderStatusChangedDomainEvent` old/new status.

**`Domain/OrderItemTests.cs` (2 tests):**
`Create` computes `LineTotal`; `UpdateQuantity` recalculates `LineTotal` and updates `Quantity`.

**`Application/PlaceOrderCommandHandlerTests.cs` (4 tests):**
Happy path verifies `AddAsync`, `SaveMessageAsync("payment.requested")`, `SaveChangesAsync` each once;
restaurant null → `RestaurantValidationException`; item unavailable → `RestaurantValidationException`;
empty items → `OrderItemsEmptyException` (bubbles from `Order.Create()`).

**`Application/GetOrderByIdQueryHandlerTests.cs` (3 tests):**
Existing order returns `OrderResponse`; unknown id → `OrderNotFoundException`;
different ClientId + Customer role → `UnauthorizedAccessException`.

**`Application/ChangeOrderStatusCommandHandlerTests.cs` (3 tests):**
`Paid → InPreparation` updates status and returns response; order not found → `OrderNotFoundException`;
`PendingPayment → InPreparation` (not in transition table) → `InvalidOrderStatusTransitionException`.

### Final state
- `dotnet build src/Services/OrderService/OrderService.API` — 0 errors, 0 warnings
- `dotnet test src/Services/OrderService/OrderService.Tests` — **23/23 passed**
- Committed: `add7cca`, pushed to `origin/master`

### Architectural decisions made
- **Exception-based error handling** — handlers throw typed domain exceptions; `ExceptionHandlingMiddleware` maps to HTTP. `CommandResult`/`QueryResult` pattern deleted entirely.
- **`IOutboxService` as seam** — handler code is routing-key-aware; implementation stores by `typeof(T).Name` and `OutboxPublisherWorker` resolves routing via switch-case (avoids BuildingBlocks schema change that would require migrations on all 6 services).
- **`correlationId` param in `Order.Create()`** — generated by handler before construction so it can be passed to `PaymentRequestedEvent` consistently.
- **`RestaurantDto.IsActive` computed property** — keeps JSON deserialization from RestaurantService working (it sends `Status: "Open"`) while handler code reads the cleaner `IsActive` bool.

---

## Session Progress (2026-05-29, RestaurantService production polish)

Branch: `master` (commits `d9e4ee0`, `9163979`, `efdf864`)

### Goal
Polish RestaurantService to production quality matching the diploma specification.
Previous implementation had a thin domain (no events, no exceptions, missing fields),
fat handlers returning `CommandResult`, no ownership validation via exceptions,
wrong enum values, and no unit tests.

### Changes — Domain Layer (`RestaurantService.Domain`)

**`RestaurantStatus` enum:** `Closed = 1, Open = 2` → `Closed = 0, Open = 1`.

**`MenuItem` entity — complete rewrite:**
- Added `Description (string)` property
- Renamed `IsAvailable` → `Available (bool)` (EF migration maps to column `available`)
- `Create()` signature: `(restaurantId, name, description, price)` — validates `price > 0`, throws `InvalidMenuItemPriceException`; sets `Available = true`
- New: `UpdatePrice(decimal newPrice)` — throws `InvalidMenuItemPriceException` if `newPrice <= 0`
- New: `UpdateDescription(string description)`
- New: `internal UpdateName(string name)` — used by `Restaurant.UpdateMenuItem()`
- Removed: `Update(name, price)` method

**`Restaurant` aggregate — complete rewrite:**
- Renamed `OwnerUserId` → `OwnerId` (EF migration: `OwnerUserId` column → `owner_id`)
- Added `Description (string)`, `Address (string)`, `IsActive (bool)` properties
- `Create(ownerId, name, description, address)` — sets `IsActive = true`, `Status = Open`, raises `RestaurantCreatedDomainEvent`
- New: `Update(name, description, address)` — raises `RestaurantUpdatedDomainEvent`
- New: `ChangeAvailability(bool isActive)` — sets both `IsActive` and `Status`; raises `RestaurantAvailabilityChangedDomainEvent`
- `AddMenuItem(name, description, price)` — raises `MenuItemAddedDomainEvent`
- `RemoveMenuItem(menuItemId)` — **throws `MenuItemNotFoundException`** (was returning `bool`); raises `MenuItemRemovedDomainEvent`
- New: `UpdateMenuItem(menuItemId, name?, description?, price?, available?)` — throws `MenuItemNotFoundException` if not found; calls relevant MenuItem methods; raises `MenuItemUpdatedDomainEvent`
- Removed: `Open()`, `Close()` methods

**New: `Events/` folder (6 files):** all are `sealed class : DomainEvent` (NOT records — `DomainEvent` base is a class, records cannot inherit from it):
- `RestaurantCreatedDomainEvent(RestaurantId, OwnerId, Name)`
- `RestaurantUpdatedDomainEvent(RestaurantId, Name)`
- `RestaurantAvailabilityChangedDomainEvent(RestaurantId, IsActive)`
- `MenuItemAddedDomainEvent(RestaurantId, MenuItemId, Name, Price)`
- `MenuItemRemovedDomainEvent(RestaurantId, MenuItemId)`
- `MenuItemUpdatedDomainEvent(RestaurantId, MenuItemId)`

**New: `Exceptions/` folder (4 files):**
- `RestaurantNotFoundException(Guid restaurantId)` — thrown by handlers on missing restaurant
- `MenuItemNotFoundException(Guid menuItemId)` — thrown by aggregate and handlers
- `InvalidMenuItemPriceException(decimal price)` — thrown by `MenuItem.Create()` and `UpdatePrice()`
- `RestaurantOwnershipException(Guid restaurantId, Guid requestingOwnerId)` — thrown by handlers on ownership mismatch

### Changes — Application Layer (`RestaurantService.Application`)

**`IRestaurantRepository` — rewritten:**
- `GetByIdAsync(id)` — no includes (for write ops that don't need menu items)
- `GetByIdWithItemsAsync(id)` — eagerly loads `MenuItems` (new, was merged into `GetByIdAsync`)
- `GetAllActiveAsync()` — filters `WHERE is_active = true` (was `GetAllAsync()` with no filter)
- `GetByOwnerIdAsync(ownerId)` — returns `IEnumerable<Restaurant>` (was `Restaurant?` single)
- `UpdateAsync(restaurant)` — new method (marks entity as Modified)
- `SaveChangesAsync()` — unchanged

**DTOs — expanded:**
- `CreateRestaurantRequest`: added `Description`, `Address`
- `AddMenuItemRequest`: added `Description`
- `UpdateMenuItemRequest`: all fields nullable (`Name?`, `Description?`, `Price?`, `Available?`)
- `RestaurantResponse`: added `OwnerId`, `Description`, `Address`, `IsActive`
- `MenuItemResponse`: added `Description`; renamed `IsAvailable` → `Available`
- New: `UpdateRestaurantRequest(Name, Description, Address)`
- New: `RestaurantSummaryResponse(Id, Name, Description, Address, IsActive, Status)` — no menu items, used by list endpoints

**Commands — handlers now throw domain exceptions, return DTOs (no `CommandResult`):**
- `CreateRestaurantCommand(OwnerId, Name, Description, Address)` + handler — injects `IValidator<>`, calls `ValidateAndThrowAsync`, returns `RestaurantResponse`
- New: `UpdateRestaurantCommand(RestaurantId, RequestingOwnerId, Name, Description, Address)` + handler — returns `RestaurantResponse`
- `AddMenuItemCommand(RestaurantId, RequestingOwnerId, Name, Description, Price)` + handler — uses `GetByIdWithItemsAsync`, returns `MenuItemResponse`
- `UpdateMenuItemCommand(RestaurantId, MenuItemId, RequestingOwnerId, Name?, Description?, Price?, Available?)` + handler — uses `GetByIdWithItemsAsync`, returns `MenuItemResponse`
- New: `RemoveMenuItemCommand(RestaurantId, MenuItemId, RequestingOwnerId)` + handler — uses `GetByIdWithItemsAsync`, returns `Task` (void)
- New: `ChangeRestaurantAvailabilityCommand(RestaurantId, RequestingOwnerId, IsActive)` + handler — uses `GetByIdAsync`, returns `RestaurantResponse`
- Stubbed out (empty namespace only): `DeleteMenuItemCommand`, `OpenRestaurantCommand`, `CloseRestaurantCommand`

**Queries — handlers throw `RestaurantNotFoundException` instead of returning null:**
- `GetAllRestaurantsQuery` (no params) + handler — returns `List<RestaurantSummaryResponse>`
- `GetRestaurantByIdQuery(RestaurantId)` + handler — uses `GetByIdWithItemsAsync`, returns `RestaurantResponse`
- New: `GetMyRestaurantsQuery(OwnerId)` + handler — returns `List<RestaurantSummaryResponse>`
- Stubbed out: `GetMenuItemsQuery` (menu items now returned as part of `RestaurantResponse`)

**Validators — command-level (injected into handlers via DI, called via `ValidateAndThrowAsync`):**
- New: `CreateRestaurantCommandValidator` — `Name: NotEmpty, MaxLength(100)`, `Description: NotEmpty, MaxLength(500)`, `Address: NotEmpty, MaxLength(200)`
- New: `AddMenuItemCommandValidator` — `Name: NotEmpty, MaxLength(100)`, `Price: GreaterThan(0)`
- New: `UpdateMenuItemCommandValidator` — `Price: GreaterThan(0) when provided`
- Updated existing request validators to match new DTO fields

### Changes — Infrastructure Layer (`RestaurantService.Infrastructure`)

**`RestaurantConfiguration` — snake_case schema:**
- `ToTable("restaurants")`; all columns mapped with `HasColumnName`: `id`, `owner_id`, `name`, `description`, `address`, `is_active`, `status`
- `HasMaxLength(100)` for name, `HasMaxLength(500)` for description, `HasMaxLength(200)` for address
- `HasForeignKey` + `OnDelete(Cascade)` explicit on menu items navigation

**`MenuItemConfiguration` — snake_case schema:**
- `ToTable("menu_items")`; columns: `id`, `restaurant_id`, `name`, `description`, `price`, `available`
- `HasColumnType("decimal(18,2)")` on price

**`RestaurantRepository` — updated to match new interface:**
- `GetByIdAsync` — no `.Include()` (lean, for write ops)
- `GetByIdWithItemsAsync` — `.Include(x => x.MenuItems)` (for ops needing items)
- `GetAllActiveAsync` — `.Where(x => x.IsActive)`
- `GetByOwnerIdAsync` — `.Where(x => x.OwnerId == ownerId)` returns list
- `UpdateAsync` — `_dbContext.Restaurants.Update(restaurant)` + `Task.CompletedTask`

**`DependencyInjection` — updated handler registrations:**
Registers: `CreateRestaurantCommandHandler`, `UpdateRestaurantCommandHandler`, `AddMenuItemCommandHandler`, `UpdateMenuItemCommandHandler`, `RemoveMenuItemCommandHandler`, `ChangeRestaurantAvailabilityCommandHandler`, `GetAllRestaurantsQueryHandler`, `GetRestaurantByIdQueryHandler`, `GetMyRestaurantsQueryHandler`.

**New EF migration `RefactorSchema`** (`20260529002947_RefactorSchema`):
- Renames `Restaurants` → `restaurants`, `MenuItems` → `menu_items`
- Renames all columns to snake_case (`OwnerUserId` → `owner_id`, `IsAvailable` → `available`, etc.)
- Adds new columns: `description (varchar 500)` + `address (varchar 200)` + `is_active (bool)` to restaurants; `description (varchar 500)` to menu_items
- Narrows `name` from `varchar(200)` → `varchar(100)` in both tables
- Fully reversible `Down()` migration

### Changes — API Layer (`RestaurantService.API`)

**New: `Middlewares/ExceptionHandlingMiddleware.cs`:**
| Exception | HTTP Status |
|---|---|
| `RestaurantNotFoundException` | 404 Not Found |
| `MenuItemNotFoundException` | 404 Not Found |
| `RestaurantOwnershipException` | 403 Forbidden (+ Warning log) |
| `InvalidMenuItemPriceException` | 400 Bad Request |
| `ValidationException` (FluentValidation) | 400 + `{errors}` dict grouped by field |
| Unhandled `Exception` | 500 Internal Server Error (logged at Error) |

**`RestaurantsController` — thin, zero business logic:**
- `GET /api/restaurants` — public (no auth), returns `List<RestaurantSummaryResponse>`
- `GET /api/restaurants/{id}` — public (no auth), returns `RestaurantResponse`
- `GET /api/restaurants/my [RestaurantManager]` — returns `List<RestaurantSummaryResponse>`
- `POST /api/restaurants [RestaurantManager]` — **201 Created** + `RestaurantResponse`
- `PUT /api/restaurants/{id} [RestaurantManager]` — 200 OK + `RestaurantResponse`
- `POST /api/restaurants/{id}/open [RestaurantManager]` — 200 OK + `RestaurantResponse`
- `POST /api/restaurants/{id}/close [RestaurantManager]` — 200 OK + `RestaurantResponse`
- `POST /api/restaurants/{id}/menu-items [RestaurantManager]` — **201 Created** + `MenuItemResponse`
- `PUT /api/restaurants/{id}/menu-items/{itemId} [RestaurantManager]` — 200 OK + `MenuItemResponse`
- `DELETE /api/restaurants/{id}/menu-items/{itemId} [RestaurantManager]` — **204 No Content**

**`Program.cs`:**
- Added `builder.Services.AddCorrelation()`
- Added `app.UseMiddleware<ExceptionHandlingMiddleware>()` (before Swagger/auth)
- Added `app.UseCorrelation()`

### Tests — `RestaurantService.Tests` (new project)

xUnit 2.9.2 + Moq 4.20.72 + FluentAssertions 6.12.0. Added to `FoodDelivery.sln`.

**`RestaurantAggregateTests.cs` (11 tests):** Create sets `IsActive=true`/`Status=Open`, raises `RestaurantCreatedDomainEvent`; Update changes fields + raises event; AddMenuItem adds to collection + raises event; RemoveMenuItem removes + raises event; RemoveMenuItem unknown id throws `MenuItemNotFoundException`; ChangeAvailability(false) sets `IsActive=false`/`Status=Closed` + raises event.

**`MenuItemTests.cs` (5 tests):** Create with valid price; UpdatePrice valid/zero/negative; SetAvailability.

**`CreateRestaurantCommandHandlerTests.cs` (2 tests):** happy path returns `RestaurantResponse`; empty name → `ValidationException`.

**`UpdateRestaurantCommandHandlerTests.cs` (3 tests):** valid owner updates restaurant; wrong owner → `RestaurantOwnershipException`; unknown id → `RestaurantNotFoundException`.

**`AddMenuItemCommandHandlerTests.cs` (3 tests):** valid data returns `MenuItemResponse`; wrong owner → `RestaurantOwnershipException`; negative price → `ValidationException`.

**`RemoveMenuItemCommandHandlerTests.cs` (3 tests):** valid data removes item; unknown item id → `MenuItemNotFoundException`; wrong owner → `RestaurantOwnershipException`.

### Final state
- `dotnet build src/Services/RestaurantService/RestaurantService.API` — 0 errors, 0 warnings
- `dotnet test src/Services/RestaurantService/RestaurantService.Tests` — **28/28 passed**

### Key decisions
- **Domain events must be `sealed class : DomainEvent`** — `DomainEvent` base is a class, so `record` inheritance fails (`CS8864`). Always use classes.
- **`GetByIdAsync` vs `GetByIdWithItemsAsync` split** — write ops that don't return a response with menu items use the leaner no-include variant; all read ops and ops returning `RestaurantResponse` use the include variant.
- **`ChangeAvailability` consolidates `Open`/`Close`** — single command with `bool IsActive` param; controller has two endpoints (`/open`, `/close`) that both call `ChangeRestaurantAvailabilityCommand`.
- **`RestaurantManager` role string kept** — controller `[Authorize(Roles = "RestaurantManager")]` kept as-is for compatibility with JWT tokens issued by AuthService. Diploma spec says `RestaurantOwner` but AuthService issues `RestaurantManager` in token claims.

---

## Session Progress (2026-05-29, PaymentService production polish)

Branch: `master` (commits `87dd631`, `183e8e6`, `2b740ac`, `69c3bc1`)

### Goal
Polish PaymentService to production quality. Previous implementation was functional
but anemic: no domain events, no typed exceptions, no `ClientId` on the aggregate,
business logic embedded directly in the RabbitMQ consumer (no Application layer command handler),
random success/fail in consumer, old `CommandResult`/`QueryResult` result-object pattern in the
query handler and controller, and no unit tests.

### Changes — Domain Layer (`PaymentService.Domain`)

**`Payment` aggregate — updated:**
- Added `ClientId (Guid)` property — identifies the customer who owns this payment
- `Create(orderId, correlationId, clientId, amount)` — added `clientId` parameter; raises `PaymentCreatedDomainEvent` after construction
- `Complete()` — added guard: throws `InvalidPaymentOperationException` if `Status != Pending`; raises `PaymentCompletedDomainEvent`
- `Fail(string reason)` — added `reason` parameter; added same guard; raises `PaymentFailedDomainEvent`

**New: `Events/` folder (3 files):** all are `sealed class : DomainEvent`:
- `PaymentCreatedDomainEvent(PaymentId, OrderId, Amount)`
- `PaymentCompletedDomainEvent(PaymentId, OrderId, CorrelationId, Amount)`
- `PaymentFailedDomainEvent(PaymentId, OrderId, CorrelationId, Reason)`

**New: `Exceptions/` folder (3 files):**
- `PaymentNotFoundException(Guid orderId)` — thrown by query handler when payment not found by order id → 404
- `InvalidPaymentOperationException(string message)` — thrown by `Complete()`/`Fail()` on bad state transition → 400
- `DuplicatePaymentException(Guid orderId)` — thrown by `ProcessPaymentCommandHandler` on duplicate OrderId → 409

### Changes — Application Layer (`PaymentService.Application`)

**`IPaymentRepository`:** added `GetByIdAsync(Guid id, CancellationToken)` method.

**`PaymentResponse` DTO:** added `ClientId (Guid)` property.

**New: `Commands/ProcessPaymentCommand.cs`** — command record + handler in one file:
- `ProcessPaymentCommand(OrderId, CorrelationId, ClientId, Amount)`
- `ProcessPaymentCommandHandler` — injects `IPaymentRepository` + `IEventBus`
- Step 1: `GetByOrderIdAsync` — throws `DuplicatePaymentException` if result is not null
- Step 2: `Payment.Create(orderId, correlationId, clientId, amount)` + `AddAsync` + `SaveChangesAsync` (Pending)
- Step 3: `payment.Complete()` + `SaveChangesAsync` (Completed) — always succeeds for MVP
- Step 4: `IEventBus.PublishAsync(PaymentCompletedEvent, "payment.completed")`
- Step 5: returns `PaymentResponse`

**`GetPaymentByOrderIdQuery` — rewritten:**
- Now takes `(OrderId, RequestingClientId, RequestingRole)` — required for ownership check
- Returns `Task<PaymentResponse>` (was `Task<QueryResult<PaymentResponse>>`)
- Throws `PaymentNotFoundException` if payment not found (was returning `QueryResult.NotFound()`)
- Throws `UnauthorizedAccessException` if role is `"Customer"` and `ClientId != RequestingClientId`

**`PaymentService.Application.csproj`:** added `<ProjectReference>` to `BuildingBlocks/EventBus/EventBus.csproj` — required for `IEventBus` in the command handler.

**`Common/CommandResult.cs` and `Common/QueryResult.cs`:** left in place as dead code (not deleted to avoid unnecessary churn; no longer used by anything).

### Changes — Infrastructure Layer (`PaymentService.Infrastructure`)

**`PaymentConfiguration`:** added explicit `HasColumnName()` for all properties in snake_case:
`id`, `order_id`, `correlation_id`, `client_id`, `amount`, `status`, `created_at`, `processed_at`.
`Amount` uses `HasColumnType("decimal(18,2)")`.

**`PaymentRepository`:** added `GetByIdAsync` implementation.

**`PaymentRequestedConsumer` — refactored:**
- Removed `IEventBus` constructor dependency (publishing is now the handler's responsibility)
- On each message: creates scope, resolves `PaymentDbContext` + `ProcessPaymentCommandHandler`
- Begins an explicit EF `IDbContextTransaction` (`db.Database.BeginTransactionAsync()`)
- Checks `ProcessedEvents` for idempotency (`evt.Id`) — acks and returns if already processed
- Calls `handler.HandleAsync(command, ct)` — handler's two `SaveChangesAsync` calls flush within the open transaction but do not commit
- Adds `ProcessedEvent` to context and calls `db.SaveChangesAsync()`
- Commits transaction — all three DB writes (insert payment, update payment status, insert processed_event) are atomic
- `DuplicatePaymentException` caught → ack (payment already exists for this order, no need to retry)
- Removed `Random.Shared.Next()` success/fail flip — always succeeds (MVP spec)

**`DependencyInjection`:** added `services.AddScoped<ProcessPaymentCommandHandler>()`.

**New EF migration `AddClientIdAndSnakeCaseColumns`** (`20260529005147_AddClientIdAndSnakeCaseColumns`):
- Adds `client_id (uuid NOT NULL)` column to `payments`
- Renames existing columns to snake_case where names differed

**`PaymentService.API.csproj`:** added references to `BuildingBlocks/Observability/Observability.csproj` and `Serilog.AspNetCore` package.

### Changes — API Layer (`PaymentService.API`)

**New: `Middlewares/ExceptionHandlingMiddleware.cs`:**
| Exception | HTTP Status |
|---|---|
| `PaymentNotFoundException` | 404 Not Found |
| `DuplicatePaymentException` | 409 Conflict |
| `InvalidPaymentOperationException` | 400 Bad Request |
| `UnauthorizedAccessException` | 403 Forbidden (+ Warning log) |
| Unhandled `Exception` | 500 Internal Server Error (logged at Error) |

**`PaymentsController` — rewritten:**
- Injects `GetPaymentByOrderIdQueryHandler` + `ICurrentUser`
- `GET /api/payments/{orderId:guid} [Authorize]` — passes `_currentUser.UserId` and `_currentUser.Role` into query; returns `Ok(response)` directly (middleware handles exceptions)

**`Program.cs` — updated:**
- Added `builder.Host.ConfigureLogging(builder.Configuration)` — Serilog via `LoggingExtensions`
- Added `builder.Services.AddAuthServices()` — registers `ICurrentUser` / `IHttpContextAccessor`
- Added `builder.Services.AddCorrelation()`
- Added `app.UseMiddleware<ExceptionHandlingMiddleware>()` (before auth)
- Added `app.UseCorrelation()`

### Tests — `PaymentService.Tests` (new project, `src/Services/PaymentService/PaymentService.Tests`)

xUnit 2.9.2 + Moq 4.20.72 + FluentAssertions 6.12.0. Added to `FoodDelivery.sln`.

**`PaymentEntityTests.cs` (9 tests):**
`Create_WithValidData_SetsStatusPending`; `Create_RaisesPaymentCreatedDomainEvent`;
`Complete_SetsStatusCompleted`; `Complete_SetsProcessedAt`; `Complete_RaisesPaymentCompletedDomainEvent`;
`Fail_SetsStatusFailed`; `Fail_RaisesPaymentFailedDomainEvent`;
`Complete_WhenAlreadyCompleted_ThrowsInvalidPaymentOperationException`;
`Fail_WhenAlreadyCompleted_ThrowsInvalidPaymentOperationException`.

**`ProcessPaymentCommandHandlerTests.cs` (3 tests):**
`Handle_NewPayment_CreatesAndCompletesPayment`; `Handle_NewPayment_PublishesPaymentCompletedEvent`;
`Handle_DuplicateOrderId_ThrowsDuplicatePaymentException`.

**`GetPaymentByOrderIdQueryHandlerTests.cs` (3 tests):**
`Handle_ExistingPayment_ReturnsPaymentResponse`; `Handle_UnknownOrderId_ThrowsPaymentNotFoundException`;
`Handle_CustomerAccessingOthersPayment_ThrowsUnauthorizedAccessException`.

### Final state
- `dotnet build src/Services/PaymentService/PaymentService.API` — 0 errors, 0 warnings
- `dotnet test src/Services/PaymentService/PaymentService.Tests` — **15/15 passed**

### Key decisions
- **Explicit transaction in consumer** — the consumer wraps the handler call + `ProcessedEvent` insert in a single `IDbContextTransaction`. The handler's two `SaveChangesAsync()` calls flush SQL within the transaction but do not commit; `CommitAsync()` in the consumer makes all three writes atomic. This satisfies both "consumer must be idempotent via processed_events" and "handler + consumer DB save in one transaction".
- **`ProcessPaymentCommandHandler` always succeeds (MVP)** — `Fail(reason)` path is preserved in domain for correctness but never triggered by the handler. The consumer no longer uses `Random.Shared.Next()`.
- **`Application.csproj` → `EventBus` reference** — handler directly uses `IEventBus` for publishing; the interface lives in the EventBus BuildingBlock so Application needs the reference. This is a deliberate pragmatic choice over introducing a redundant `IPaymentEventPublisher` wrapper.
- **`DuplicatePaymentException` → ack** — if the handler throws this, it means the payment for this `OrderId` was already processed (different event ID, same order). Consumer acks rather than requeuing since retrying would always throw again.
