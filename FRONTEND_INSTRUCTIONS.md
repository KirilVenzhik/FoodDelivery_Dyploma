# Frontend Implementation Instructions

## Task
Implement a Blazor WebAssembly frontend for the FoodDelivery microservices platform.
The backend is fully complete and running. Your job is to build the web client only.

**Read CONTEXT.md first** — it contains the full backend architecture, all API endpoints,
roles, status enums, and integration event flows. Do not re-implement anything from the backend.

---

## Tech Stack

- **Framework:** Blazor WebAssembly (.NET 9, standalone — no ASP.NET Core host)
- **UI components:** MudBlazor (latest stable) — use for all UI: layout, forms, tables, dialogs, badges, snackbars
- **HTTP:** `HttpClient` with `IHttpClientFactory` — one named client `"gateway"` pointing at `http://localhost:5000`
- **Auth:** `CustomAuthStateProvider` (see below) — JWT stored in `localStorage` via `IJSRuntime`
- **Routing:** Blazor built-in router with `[Authorize(Roles = "...")]` page attributes
- **No external state management library** — component parameters + cascading values are enough

---

## Project Location

Create the project at:
```
src/FoodDelivery.Web/
```

Add it to `FoodDelivery.sln`.

---

## Project Setup

```bash
dotnet new blazorwasm -n FoodDelivery.Web -o src/FoodDelivery.Web --no-https
dotnet sln add src/FoodDelivery.Web/FoodDelivery.Web.csproj
```

Add to `FoodDelivery.Web.csproj`:
```xml
<PackageReference Include="MudBlazor" Version="7.*" />
```

`wwwroot/index.html` — add MudBlazor CSS in `<head>`:
```html
<link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
```
Add before `</body>`:
```html
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

`Program.cs` — wire everything:
```csharp
builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient("gateway", c => c.BaseAddress = new Uri("http://localhost:5000"));
// register all API client services as scoped
```

`App.razor` — use `<CascadingAuthenticationState>` + `<AuthorizeRouteView>`.

---

## Authentication

### JWT structure
Token claims: `nameid` = userId (Guid), `role` = one of `Customer`, `RestaurantOwner`, `Courier`, `Administrator`.
Backend also returns `refreshToken` (string). Access token expiry: read from JWT `exp` claim.

### CustomAuthStateProvider
```csharp
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    // Store access token in localStorage via IJSRuntime
    // ParseClaimsFromJwt() — decode base64 JWT payload manually (no library needed)
    // GetAuthenticationStateAsync() — read token from localStorage, return ClaimsPrincipal
    // MarkUserAsAuthenticated(token) — notify subscribers
    // MarkUserAsLoggedOut() — clear storage, notify
}
```

Parse JWT payload without a library:
```csharp
private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
{
    var payload = jwt.Split('.')[1];
    var jsonBytes = Convert.FromBase64String(PadBase64(payload));
    var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
    // map "nameid" -> ClaimTypes.NameIdentifier, "role" -> ClaimTypes.Role
}
```

### AuthService (API client)
```
POST /api/auth/register   → { email, password, role }
POST /api/auth/login      → { email, password }
POST /api/auth/refresh    → { refreshToken }
GET  /api/auth/me
```
Response: `{ accessToken, refreshToken, userId, role }`

On login success: save both tokens to localStorage, call `MarkUserAsAuthenticated`.
On 401 response (anywhere in the app): attempt refresh, retry once, then log out.

---

## Pages & Features by Role

### Shared pages (no auth required)
- `/` — landing: brief description + Login/Register buttons
- `/login` — email + password form → POST /api/auth/login
- `/register` — email + password + role selector (Customer / RestaurantOwner / Courier) → POST /api/auth/register

### Customer (`/customer/*`)
All protected with `[Authorize(Roles = "Customer")]`.

**`/customer/restaurants`** (home after login)
- `GET /api/restaurants` → list of restaurants as MudCards
- Each card: name, address, status badge (Open/Closed), "View Menu" button

**`/customer/restaurants/{id}`**
- `GET /api/restaurants/{id}` → restaurant detail + menu items
- Menu items as MudTable: name, description, price, availability badge
- "Add to cart" button per item → cart stored in component state (Dictionary<Guid, int> itemId→qty)
- Cart summary panel: items, quantities, total
- "Place Order" button → POST /api/orders with `{ restaurantId, items: [{menuItemId, quantity}] }`
- On success: navigate to `/customer/orders/{id}`

**`/customer/orders`**
- `GET /api/orders` → list of own orders as MudTable
- Columns: date, restaurant, total, status badge, "View" link

**`/customer/orders/{id}`**
- `GET /api/orders/{id}` → order detail
- Show: items table, total, status badge, payment status
- Poll every 5s with `@implements IDisposable` + `Timer` → refresh order status automatically
- If delivery exists: show delivery status (fetch `GET /api/deliveries/{deliveryId}` if orderId→deliveryId mapping available; otherwise show from order status)

**`/customer/notifications`**
- `GET /api/notifications` → list as MudList
- Each item: message, type chip, read/unread state, timestamp
- Click → PATCH /api/notifications/{id}/read

---

### Restaurant Owner (`/owner/*`)
All protected with `[Authorize(Roles = "RestaurantOwner")]`.

**`/owner/restaurants`** (home after login)
- `GET /api/restaurants/my` → list own restaurants
- "Create Restaurant" → opens MudDialog with form (name, description, address)
  - POST /api/restaurants

**`/owner/restaurants/{id}`**
- Restaurant detail + edit form (PUT /api/restaurants/{id})
- Open/Close toggle → POST /api/restaurants/{id}/open or /close
- Menu items MudTable with inline "Edit" and "Delete" actions
  - Edit → MudDialog: name, description, price, available toggle → PUT /api/restaurants/{id}/menu-items/{itemId}
  - Delete → confirm dialog → DELETE /api/restaurants/{id}/menu-items/{itemId}
  - "Add Menu Item" button → MudDialog → POST /api/restaurants/{id}/menu-items

**`/owner/orders`**
- `GET /api/orders` → shows orders for owner's restaurants (backend filters by role)
- MudTable: order id, client, items count, total, status badge
- Status badge is clickable for `Paid` orders → opens dialog to set `InPreparation`
- For `InPreparation` orders → button "Ready for Delivery" → PATCH /api/orders/{id}/status `{ newStatus: "ReadyForDelivery" }`

---

### Courier (`/courier/*`)
All protected with `[Authorize(Roles = "Courier")]`.

**`/courier/pending`** (home after login)
- `GET /api/deliveries/pending` → list of unassigned deliveries
- MudTable: order id, restaurant id, created at, "Assign to me" button
  - PATCH /api/deliveries/{id}/assign

**`/courier/deliveries`**
- `GET /api/deliveries/my` → own deliveries
- MudTable with status badge + action button per row:
  - `Assigned` → "Pick Up" → PATCH /api/deliveries/{id}/pickup
  - `InProgress` → "Complete" → PATCH /api/deliveries/{id}/complete
  - `Delivered` → no action

**`/courier/deliveries/{id}`**
- `GET /api/deliveries/{id}` → delivery detail
- Show all fields + current status + action button

---

### Shared (all authenticated roles)

**`/account`**
- `GET /api/auth/me` → show userId, email, role
- Logout button → clear localStorage, redirect to `/login`

---

## Navigation Layout

Use `MudLayout` with `MudAppBar` + `MudDrawer`. Show nav links based on role:

```razor
<AuthorizeView Roles="Customer"> ... </AuthorizeView>
<AuthorizeView Roles="RestaurantOwner"> ... </AuthorizeView>
<AuthorizeView Roles="Courier"> ... </AuthorizeView>
```

After login, redirect based on role:
- Customer → `/customer/restaurants`
- RestaurantOwner → `/owner/restaurants`
- Courier → `/courier/pending`

---

## HTTP Client Pattern

Create a base `ApiClient` that all service classes extend:

```csharp
public abstract class ApiClient
{
    protected readonly HttpClient _http;
    protected readonly CustomAuthStateProvider _auth;

    protected async Task<T?> GetAsync<T>(string url) { ... }
    protected async Task<T?> PostAsync<T>(string url, object body) { ... }
    protected async Task<T?> PatchAsync<T>(string url, object? body = null) { ... }
    protected async Task DeleteAsync(string url) { ... }

    // Attach Bearer token from localStorage on every request
    // On 401: attempt token refresh via AuthService, retry once
}
```

Service classes: `RestaurantApiClient`, `OrderApiClient`, `DeliveryApiClient`,
`NotificationApiClient`, `PaymentApiClient` — each wraps the relevant endpoints.

---

## Error Handling

- Show `MudSnackbar` on API errors (use `ISnackbar` injected via MudBlazor)
- 401 after refresh failure → redirect to `/login`
- 403 → show "Access denied" snackbar
- 404 → show "Not found" snackbar
- 422/409 → show the error message from response body `{ "error": "..." }`

---

## DTO Models

Define C# record types in `Models/` matching backend responses exactly.
Key models to create (match backend field names, use camelCase JSON):

```csharp
// Auth
record AuthResponse(string AccessToken, string RefreshToken, Guid UserId, string Role);
record UserDto(Guid UserId, string Email, string Role);

// Restaurant
record RestaurantSummaryResponse(Guid Id, string Name, string Description, string Address, bool IsActive, string Status);
record RestaurantResponse(Guid Id, string Name, string Description, string Address, bool IsActive, string Status, List<MenuItemResponse> MenuItems);
record MenuItemResponse(Guid Id, string Name, string Description, decimal Price, bool Available);

// Order
record OrderResponse(Guid Id, Guid RestaurantId, decimal TotalAmount, string Status, DateTime CreatedAt, List<OrderItemResponse> Items);
record OrderItemResponse(Guid Id, Guid MenuItemId, string ItemName, decimal UnitPrice, int Quantity, decimal LineTotal);

// Delivery
record DeliveryResponse(Guid Id, Guid OrderId, Guid? CourierId, string Status, DateTime CreatedAt, DateTime UpdatedAt);

// Notification
record NotificationResponse(Guid Id, Guid OrderId, string Message, string Type, bool IsRead, DateTime CreatedAt, DateTime? SentAt);

// Payment
record PaymentResponse(Guid Id, Guid OrderId, decimal Amount, string Status, DateTime CreatedAt);

// Requests
record CreateOrderRequest(Guid RestaurantId, List<OrderItemRequest> Items);
record OrderItemRequest(Guid MenuItemId, int Quantity);
record CreateRestaurantRequest(string Name, string Description, string Address);
record AddMenuItemRequest(string Name, string Description, decimal Price);
record UpdateMenuItemRequest(string? Name, string? Description, decimal? Price, bool? Available);
record ChangeOrderStatusRequest(string NewStatus);
```

---

## Status Badge Colors (MudBlazor)

Use `Color` enum on `MudChip` or `MudBadge`:

| Status | Color |
|---|---|
| Open / Paid / Completed / Delivered | `Color.Success` |
| Closed / PaymentFailed / Failed | `Color.Error` |
| PendingPayment / Pending / Assigned | `Color.Warning` |
| InPreparation / InProgress | `Color.Info` |
| ReadyForDelivery / InDelivery | `Color.Primary` |

---

## Important Notes

- The API Gateway runs at `http://localhost:5000` — all requests go there
- JWT role claim key is `role` (not the standard `ClaimTypes.Role` URL) — handle in `ParseClaimsFromJwt`
- `RestaurantOwner` is the role string in JWT; use exactly this string in `[Authorize(Roles = "RestaurantOwner")]`
- Order status transitions the frontend can trigger: `InPreparation`, `ReadyForDelivery` (RestaurantOwner only)
- DeliveryService does not expose a "get delivery by order id" endpoint — show delivery info from order status
- Blazor WASM runs in the browser; CORS must be enabled on the gateway for `http://localhost:*` (add if needed)
- Do not share any C# projects/assemblies with backend services — the Web project is fully standalone
