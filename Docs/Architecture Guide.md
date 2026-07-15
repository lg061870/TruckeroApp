# Truckero Architecture Guide (Current)

Last updated: 2026-03-13

This guide documents the architecture that is currently implemented in `Truckero.sln`.

---

## 1. System Topology

### Primary runtime path

- Client: `TruckeroApp` (MAUI Blazor hybrid app)
- API: `Truckero.API` (ASP.NET Core Web API)
- Domain contracts: `Truckero.Core`
- Implementations and persistence: `Truckero.Infrastructure`
- Data store: SQL Server via EF Core (`AppDbContext`)
- External storage: Azure Blob storage (`AzureBlobStorageService`)

Runtime flow:

`TruckeroApp` -> `Truckero.API` -> `Truckero.Infrastructure` -> SQL Server

`Truckero.Core` types are shared across client, API, and infrastructure.

### Additional apps in the solution

- `Truckero.Admin`: server-side Blazor admin UI
- `TruckeroCorporate`: server-side Blazor corporate UI
- `Truckero.Store`: MAUI Blazor store app scaffold
- `Truckero.Diagnostics`: diagnostics/test helpers and simulators

These projects are present and buildable, but `TruckeroApp` + `Truckero.API` are the primary end-to-end runtime path today.

---

## 2. Project Responsibilities

| Project | Responsibility |
| --- | --- |
| `TruckeroApp` | UI pages, local session/state services, typed API clients |
| `Truckero.API` | HTTP endpoints, auth setup, DI composition, request/response orchestration |
| `Truckero.Core` | Entities, DTOs, enums, interfaces, constants, validation attributes |
| `Truckero.Infrastructure` | Repository implementations, business services, EF Core context and migrations |
| `Truckero.Diagnostics` | Development/test tooling (mocks, seeders, simulators, debug helpers) |

Dependency direction (current references):

- `Truckero.Infrastructure` -> `Truckero.Core`
- `Truckero.API` -> `Truckero.Core`, `Truckero.Infrastructure`, `Truckero.Diagnostics`
- `TruckeroApp` -> `Truckero.Core`, `Truckero.Diagnostics`

---

## 3. API Architecture

### 3.1 Environment-specific authentication

In `Truckero.API/Program.cs`:

- `UnitTesting` environment: custom `Test` scheme (`TestAuthHandler`)
- `Development` environment: `AllowAll` scheme (`AllowAllAuthHandler`)
- Non-development environments: Azure AD B2C OpenID Connect via `Microsoft.Identity.Web`

Authorization is enabled globally with `AddAuthorization()`, and per-endpoint behavior is controlled by `[Authorize]` / `[AllowAnonymous]` attributes.

### 3.2 Route style currently in use

Route conventions are mixed and intentionally reflected in clients:

- Prefixless groups: `/auth`, `/tokens`, `/onboarding`
- Controller-token groups: `/api/[controller]`
- Custom groups: for example `/api/driver`

When adding endpoints, follow the existing route style for that specific module instead of introducing a new global style.

### 3.3 DI composition

- Main service/repository wiring is in `Truckero.API/Program.cs`
- Additional repository extension wiring exists in `Truckero.API/RepositoryServiceSetup.cs`

Controller rule of thumb: keep controllers thin and delegate to service/repository layers.

---

## 4. Client Architecture (`TruckeroApp`)

### 4.1 API access model

- Typed API clients live in `TruckeroApp/ServiceClients`
- Client registration lives in `TruckeroApp/ApiClientProxies.cs`
- DI composition lives in `TruckeroApp/MauiProgram.cs`

### 4.2 Platform-specific API base URL

`MauiProgram.cs` sets base URL by platform:

- Android emulator: `http://10.0.2.2:5200`
- iOS: `http://localhost:5200`
- Windows/other: `http://localhost:5200`

### 4.3 Session and token handling

- Session/token abstractions live under `TruckeroApp/Interfaces` and `TruckeroApp/Services`
- `AuthenticatedEnvelope` is used for token-bearing requests in client services that require auth
- Some onboarding calls are intentionally unauthenticated today (aligned with API attributes)

---

## 5. Domain and Persistence Architecture

### 5.1 Core domain contract layer

`Truckero.Core` defines:

- Domain entities (`Entities/`)
- API/app DTOs (`DTOs/` modules such as `Auth`, `Onboarding`, `CustomerFlow`, `Store`, `Trucks`)
- Repository and service interfaces (`Interfaces/Repositories`, `Interfaces/Services`)
- Enums, constants, custom data annotations

This layer is shared and should remain implementation-light.

### 5.2 Infrastructure layer

`Truckero.Infrastructure` contains:

- Repositories (`Repositories/`)
- Business services (`Services/`)
- EF Core context and initializer (`Data/`)
- Migrations (`Migrations/`)
- External integration services (`Storage/` and other integration folders)

### 5.3 Transaction and consistency pattern

Current service implementations (for example onboarding and freight bid flows) use:

- EF execution strategy (`CreateExecutionStrategy()`)
- Explicit transactions (`BeginTransactionAsync()`)
- Controlled `SaveChangesAsync()` boundaries
- Rollback + structured error response paths

This is the preferred pattern for multi-step write operations.

---

## 6. End-to-End Request Flow

Standard path for business operations:

1. UI page triggers a typed API client call.
2. API controller validates/orchestrates request handling.
3. Infrastructure service executes domain behavior.
4. Repository layer applies persistence operations.
5. `AppDbContext` commits to SQL Server.
6. Response DTO returns through API and client service to UI.

For authenticated endpoints, the client sends bearer tokens and the API enforces authorization at endpoint level.

---

## 7. Testing Architecture

- `Truckero.API.Tests`: controller/integration tests using `WebApplicationFactory<Program>`
- API tests commonly replace SQL Server `DbContext` registration with in-memory EF provider
- `Truckero.Infrastructure.Tests`: repository tests
- `Truckero.Admin.Tests`: render/basic UI tests
- `TruckeroApp.Tests` and `TruckeroCorporate.Tests`: present but currently lighter/scaffolded

When adding API or infrastructure behavior, add or extend tests in the matching test project.

---

## 8. Architecture Guardrails

- Keep business logic out of Razor pages and API controllers.
- Keep persistence operations in repositories and service-layer orchestration.
- Reuse existing DTO modules and interface folders before creating new top-level patterns.
- Follow existing route conventions per module to avoid client/API drift.
- For multi-entity writes, use execution strategy + explicit transaction patterns already used in services.

---

## 9. Working Rule

Before implementing a feature, identify the closest existing module path and mirror that structure. In this solution, consistency with the established module pattern is the primary architecture rule.
