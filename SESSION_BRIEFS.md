# Claude Code Session Briefs — FoodDelivery frontend

Run each on your Windows machine in its OWN Claude Code session, on its OWN branch.
Paste the brief as the first message. Branch order matters: 0 -> 1 -> (2a||2b||2c) -> 3.

## Before anything (one-time, clean base)
The working tree shows ~151 "changed" files that are only CRLF/LF noise — no real changes.
Close Visual Studio (it holds `.git\index.lock`), then:
    git stash --include-untracked   # or: git checkout -- .   to drop the EOL noise
    git config core.autocrlf true   # prevent it recurring
Keep PM_COORDINATION.md, SESSION_BRIEFS.md, FRONTEND_INSTRUCTIONS.md (re-add after).

---

## Session 0 — branch: feat/web-scaffold  (SEQUENTIAL, do first)
You are building only the Blazor WASM frontend. BACKEND IS FROZEN — do not touch
src/Services, src/BuildingBlocks, src/ApiGateway, docker-compose.yml.
Read FRONTEND_INSTRUCTIONS.md. Then:
1. `dotnet new blazorwasm -n FoodDelivery.Web -o src/FoodDelivery.Web --no-https`
2. `dotnet sln add src/FoodDelivery.Web/FoodDelivery.Web.csproj`
3. Add MudBlazor 7.* PackageReference; wire MudBlazor CSS/JS in wwwroot/index.html.
4. Program.cs: AddMudServices, AddAuthorizationCore, named HttpClient "gateway" -> http://localhost:5000.
5. App.razor: CascadingAuthenticationState + AuthorizeRouteView. Empty MainLayout with MudLayout/AppBar/Drawer and an EMPTY nav (role links added in Session 3).
6. `dotnet build` must succeed. Commit. Open PR to master.
DO NOT create any Pages beyond the default. DO NOT write Models or API clients.

## Session 1 — branch: feat/web-core-auth  (SEQUENTIAL, after 0 merges)
Branch from updated master. Backend frozen. Read FRONTEND_INSTRUCTIONS.md (Auth + DTO sections).
Create:
- Models/ : ALL record DTOs listed in FRONTEND_INSTRUCTIONS (Auth, Restaurant, Order, Delivery, Notification, Payment, Requests).
- CustomAuthStateProvider (JWT in localStorage, ParseClaimsFromJwt mapping nameid/role).
- ApiClient base (Bearer attach, 401 -> refresh+retry once).
- AuthService (register/login/refresh/me) + register in Program.cs.
- Pages: /, /login, /register, /account. Role-based post-login redirect.
`dotnet build` green. Commit, PR. Append any shared-file notes to PM_COORDINATION.md.
DO NOT create Customer/Owner/Courier pages or their API clients.

## Session 2a — branch: feat/web-customer  (PARALLEL, after 1 merges)
Branch from updated master. Backend frozen. Only touch Pages/Customer/** and the API clients you need.
Read FRONTEND_INSTRUCTIONS "Customer" section. Create RestaurantApiClient(read), OrderApiClient,
NotificationApiClient and pages: /customer/restaurants, /customer/restaurants/{id} (cart+place order),
/customer/orders, /customer/orders/{id} (5s polling), /customer/notifications.
DO NOT edit Program.cs, MainLayout, _Imports.razor — if you need a service registered, add a line
under "Pending registrations" in PM_COORDINATION.md instead. `dotnet build` green. Commit, PR.

## Session 2b — branch: feat/web-owner  (PARALLEL, after 1 merges)
Same rules as 2a. Only Pages/Owner/** + RestaurantApiClient(write methods).
Pages: /owner/restaurants (create dialog), /owner/restaurants/{id} (edit, open/close, menu CRUD dialogs),
/owner/orders (status transitions InPreparation, ReadyForDelivery). Role string is exactly "RestaurantOwner".
DO NOT edit Program.cs/MainLayout/_Imports — note registrations in PM_COORDINATION.md. Build green. PR.

## Session 2c — branch: feat/web-courier  (PARALLEL, after 1 merges)
Same rules as 2a. Only Pages/Courier/** + DeliveryApiClient.
Pages: /courier/pending (assign), /courier/deliveries (pickup/complete), /courier/deliveries/{id}.
No "get delivery by order id" endpoint exists — show delivery info from order status.
DO NOT edit Program.cs/MainLayout/_Imports — note registrations in PM_COORDINATION.md. Build green. PR.

## Session 3 — branch: feat/web-integration  (SEQUENTIAL, after 2a+2b+2c merge)
Branch from updated master. Apply all "Pending registrations" from PM_COORDINATION.md into Program.cs.
Fill MainLayout nav with role-scoped links (AuthorizeView per role). Verify CORS on gateway allows
http://localhost:5000 origin for localhost:* (add to gateway ONLY if it blocks — this is the one
allowed backend touch; if so, flag it to Kirill first). Run full stack via docker-compose, smoke-test
each role end-to-end. `dotnet build` green. PR.
