# PM Coordination — FoodDelivery (diploma)

Owner: Kirill. PM/supervisor: Claude (Cowork). This file coordinates parallel Claude Code
sessions so tasks never overwrite each other. Update the status table after every session.

## Ground rules
- **Backend is FROZEN.** Do not modify `src/Services/**`, `src/BuildingBlocks/**`,
  `src/ApiGateway/**`, `docker-compose.yml`, or DB schemas unless a bug blocks the frontend.
  If a backend change is truly required, stop and ask Kirill first.
- **Frontend = Blazor WebAssembly** (.NET 9, MudBlazor). No React. Spec: `FRONTEND_INSTRUCTIONS.md`.
- New code lives ONLY in `src/FoodDelivery.Web/`.
- One concern per branch/session. Read files before editing. Never overwrite blindly.
- Token discipline: each session reads only `FRONTEND_INSTRUCTIONS.md` + its own area; it does
  NOT re-read all of CONTEXT.md (103 KB). Endpoint/DTO facts are summarized in FRONTEND_INSTRUCTIONS.

## Why not full parallel
A Blazor project shares `Program.cs`, `.csproj`, `_Imports.razor`, `App.razor`, `MainLayout`,
nav menu. If multiple sessions touch these at once -> merge conflicts. So: **foundation first
(sequential), then parallelize by role area** (separate `Pages/` folders = no overlap).

## Branch / session plan

| Phase | Branch | Scope (files) | Depends on | Status |
|---|---|---|---|---|
| 0 Scaffold | feat/web-scaffold | project authored, MudBlazor wired, Program.cs, index.html, App.razor, MainLayout, empty nav | — | DONE (authored by PM; run `dotnet sln add` + build on Windows) |
| 1 Core/Auth | feat/web-core-auth | Models/ (all DTOs), CustomAuthStateProvider, ApiClient base, AuthService, /login /register / /account | Phase 0 | DONE (authored by PM) |
| 2a Customer | feat/web-customer | Pages/Customer/**, RestaurantApiClient (read), OrderApiClient, NotificationApiClient | Phase 1 | TODO |
| 2b Owner | feat/web-owner | Pages/Owner/**, RestaurantApiClient (write) | Phase 1 | TODO |
| 2c Courier | feat/web-courier | Pages/Courier/**, DeliveryApiClient | Phase 1 | TODO |
| 3 Integrate | feat/web-integration | merge 2a/2b/2c, fill nav links per role, CORS check, end-to-end smoke test | 2a+2b+2c | TODO |

Phases 2a/2b/2c CAN run in parallel (disjoint folders). Shared API-client files: each session
adds only the methods its pages need; if two need the same client, Phase 1 stubs it first.

## Shared-file ownership (to prevent collisions)
- Program.cs, MainLayout.razor, _Imports.razor -> edited in Phase 0 & 3 only. Phase 2 sessions
  must NOT edit these; if a service registration is needed, note it here for Phase 3.
- Models/ -> all created in Phase 1. Phase 2 only reads them.

## Pending registrations / notes for Phase 3
(append here as parallel sessions discover needs)
- **Session 2a (Customer):** Add after `AddScoped<IAuthService, AuthService>()` in Program.cs:
  `builder.Services.AddScoped<RestaurantApiClient>();`
  `builder.Services.AddScoped<OrderApiClient>();`
  `builder.Services.AddScoped<NotificationApiClient>();`
- **Session 2c (Courier):** `builder.Services.AddScoped<DeliveryApiClient>();`

## Session log
- 2026-06-04 — PM analysis complete. Backend verified complete (6 services, 47 tests). Frontend not started.
- 2026-06-04 — Findings: (1) 151 "modified" files are CRLF/LF noise only (git diff --ignore-all-space = empty); do NOT commit — discard instead. (2) Sandbox has no .NET SDK and .git is not writable from it; all branch/build work runs as Claude Code on Windows. (3) See SESSION_BRIEFS.md for per-branch prompts. Branch order: web-scaffold -> web-core-auth -> (web-customer || web-owner || web-courier) -> web-integration.

## Foundation authored (2026-06-04, by PM)
Phases 0 & 1 are written into `src/FoodDelivery.Web/` (19 files). No SDK in the Cowork sandbox,
so they are authored but NOT yet built. First step on Windows:
    dotnet sln add src/FoodDelivery.Web/FoodDelivery.Web.csproj
    dotnet restore && dotnet build src/FoodDelivery.Web
Central Package Management is in effect: versions live in Directory.Packages.props
(added: Components.WebAssembly, WebAssembly.DevServer, Components.Authorization 9.0.0, MudBlazor 7.15.0).
Foundation provides for the role sessions: all Models, CustomAuthStateProvider, ApiClient base
(Bearer + 401 refresh-retry), IAuthService/AuthService, RoleRoutes, auth pages, empty role nav.
Role sessions 2a/2b/2c subclass `ApiClient`, add only their Pages/<Role>/** and their API client,
and log any Program.cs service registration under "Pending registrations" for Phase 3.

## Backend gotcha (verified 2026-06-04) — for all frontend sessions
AuthService uses `AddControllers()` with NO JsonStringEnumConverter. Therefore any enum sent
to the backend must be the NUMERIC value, not the name. `UserRole`: 1=Customer, 2=RestaurantOwner,
3=Courier, 4=Admin. Fix applied: register sends `role` as int (RoleCodes.ToInt). Responses still
return Role as a string name (user.Role.ToString()), and the JWT role claim = the name string,
so [Authorize(Roles="Customer")] / AuthorizeView Roles="..." work with the name. If other services
expect enums in request bodies, send numbers too (or confirm a string-enum converter exists there).

## Milestone (2026-06-04)
Foundation (Phases 0-1) built green and MERGED to master. Register/login verified working
(register sends numeric role; CRLF/sync truncation on 3 files fixed by rewrite). 
NEXT: launch role sessions 2a/2b/2c in parallel, then integration (Phase 3).
