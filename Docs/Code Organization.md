# Truckero Code Structure and Organization Guide

Last updated: 2026-03-13

This document reflects the current `Truckero.sln` layout and where code should be added today.

---

## 1. Solution Snapshot

| Project | Current Role |
| --- | --- |
| `TruckeroApp` | Primary MAUI Blazor client (mobile-focused flows, onboarding, customer/driver/store UX) |
| `Truckero.API` | ASP.NET Core Web API (auth, onboarding, store, freight/driver bid, media, payments) |
| `Truckero.Core` | Domain contracts and shared models (entities, DTOs, interfaces, enums, constants) |
| `Truckero.Infrastructure` | EF Core data access and business/service implementations |
| `Truckero.Diagnostics` | Development/testing helpers, simulators, seed/test utilities |
| `Truckero.Admin` | Separate server-side Blazor admin web app |
| `TruckeroCorporate` | Separate server-side Blazor corporate web app |
| `Truckero.Store` | Separate MAUI Blazor store app scaffold |
| `Truckero.Contracts` | Shared contracts project (currently placeholder) |
| `Truckero.SharedUI` | Shared UI project (currently placeholder) |
| `Truckero.Diagnostics.Server` | Diagnostics server project (currently minimal scaffold) |
| `TestTemplateBuilder` | Console utility that creates a reusable SQLite test template |
| `Truckero.API.Tests` | API integration/controller tests |
| `Truckero.Infrastructure.Tests` | Infrastructure repository tests |
| `Truckero.Admin.Tests` | Admin UI tests |
| `TruckeroApp.Tests` | App test project scaffold |
| `TruckeroCorporate.Tests` | Corporate app test project scaffold |

---

## 2. Dependency Direction (As Implemented)

Project references in the solution currently follow this direction:

- `Truckero.Infrastructure` -> `Truckero.Core`
- `Truckero.API` -> `Truckero.Core`, `Truckero.Infrastructure`, `Truckero.Diagnostics`
- `TruckeroApp` -> `Truckero.Core`, `Truckero.Diagnostics`
- `Truckero.Diagnostics` -> `Truckero.Core`, `Truckero.Infrastructure`
- `Truckero.Diagnostics.Server` -> `Truckero.Core`, `Truckero.Infrastructure`, `Truckero.Diagnostics`
- Test projects reference their target runtime projects

Practical runtime flow is:

`TruckeroApp` -> `Truckero.API` Controllers -> Infrastructure Services -> Infrastructure Repositories -> `AppDbContext` -> SQL Server

`Truckero.Core` provides shared entities/DTOs/interfaces used across those layers.

---

## 3. Primary Project Conventions

### 3.1 `TruckeroApp` (Primary Client)

Key folders/files:

- `Components/Pages/` with feature modules such as `Authentication`, `Companies`, `CustomerFlow`, `Driver`, `Login`, `Onboarding`, `Settings`, `Shared`
- `ServiceClients/` for typed API clients (`AuthApiClientService.cs`, `TruckApiClientService.cs`, `StoreApiClientService.cs`, etc.)
- `Interfaces/` for client-side abstractions (`IAuthSessionContext.cs`, `ITokenStorageService.cs`)
- `Services/` for local app services (`AuthSessionContextService.cs`, `SecureTokenStorageService.cs`, `ToastService.cs`)
- `ApiClientProxies.cs` for modular DI extension methods that register API client services
- `MauiProgram.cs` for app startup and DI wiring
- `AppRoutes.cs` for route constants

Use this project for production client features first unless the work is explicitly for Admin/Corporate/Store apps.

### 3.2 `Truckero.API` (Web API)

Key folders/files:

- `Controllers/` contains endpoint controllers by domain (`AuthController`, `OnboardingController`, `StoreController`, `FreightBidController`, etc.)
- `Models/` for API-layer request/response models
- `Auth/`, `Filters/`, `TestAuth/`, `Extensions/` for auth and middleware support
- `Program.cs` for environment-specific auth, DbContext, CORS, repositories/services DI, Swagger
- `RepositoryServiceSetup.cs` for additional repository registration extensions

Controller guidance in current codebase:

- Keep controllers thin
- Delegate behavior to services/repositories
- Keep authorization and environment behavior in startup/auth handlers

### 3.3 `Truckero.Core` (Shared Domain Contracts)

Key folders:

- `Entities/` (domain entities such as `Company`, `Store`, `FreightBid`, `DriverProfile`, `Truck`)
- `DTOs/` grouped by module (`Auth`, `Onboarding`, `CustomerFlow`, `Store`, `PaymentAccount`, `PayoutAccount`, `PaymentMethodType`, `Media`, `Shared`, `Common`, `Trucks`)
- `Interfaces/Repositories/` and `Interfaces/Services/` for contracts
- `Enums/`, `Constants/`, `DataAnnotations/`, `Exceptions/`, `Extensions/`

`Truckero.Core` should remain implementation-light and shared across app/api/infrastructure.

### 3.4 `Truckero.Infrastructure` (Data + Business Implementation)

Key folders:

- `Repositories/` concrete repository implementations
- `Services/` business/service implementations
- `Data/` with `AppDbContext`, `AppDbContextFactory`, `DbInitializer`
- `Migrations/` EF migrations
- `Storage/` external storage services (`AzureBlobStorageService.cs`)
- Additional integration folders exist (`Auth`, `MapServices`, `NotificationProviders`, `PaymentGateways`, `Extensions`)

Business rules and persistence behavior should be implemented here, not in UI pages or API controllers.

---

## 4. Additional Apps and Utility Projects

- `Truckero.Admin`: Interactive server-side Blazor app with pages like `Dashboard`, `ManageUsers`, `PricingRules`, `Disputes`
- `TruckeroCorporate`: Interactive server-side Blazor app (currently close to template pages)
- `Truckero.Store`: MAUI Blazor app scaffold (currently template-style pages)
- `Truckero.Diagnostics`: Contains diagnostics helpers and docs (for example `FreightBidSimulator` support)
- `TestTemplateBuilder`: Generates `Truckero.Diagnostics/CloneDBs/TestTemplate.sqlite` for test workflows
- `Truckero.Contracts`, `Truckero.SharedUI`, `Truckero.Diagnostics.Server`: currently minimal/placeholder and should be treated as scaffolding unless expanded by active work

---

## 5. Tests (Current Layout)

- `Truckero.API.Tests`: controller/integration coverage (`AuthControllerTests`, `CompanyControllerTests`, `OnboardingControllerTests`, `StoreControllerTests`)
- `Truckero.Infrastructure.Tests/Repositories`: repository-level tests
- `Truckero.Admin.Tests`: includes `BasicRenderTests.cs`
- `TruckeroApp.Tests`, `TruckeroCorporate.Tests`: present but currently light/scaffolded

When adding a new feature in API or infrastructure, add/extend tests in the corresponding test project.

---

## 6. Where To Add New Code (Current Repo)

| Task | Place It Here |
| --- | --- |
| New mobile/client screen | `TruckeroApp/Components/Pages/<Module>/` |
| New client API proxy | `TruckeroApp/ServiceClients/` + DI extension in `TruckeroApp/ApiClientProxies.cs` |
| New API endpoint | `Truckero.API/Controllers/` |
| API startup/DI/auth changes | `Truckero.API/Program.cs` (and `RepositoryServiceSetup.cs` when appropriate) |
| New DTO | `Truckero.Core/DTOs/<Module>/` |
| New domain entity/enum/constant | `Truckero.Core/Entities`, `Truckero.Core/Enums`, `Truckero.Core/Constants` |
| New repository contract | `Truckero.Core/Interfaces/Repositories/` |
| New service contract | `Truckero.Core/Interfaces/Services/` |
| New repository implementation | `Truckero.Infrastructure/Repositories/` |
| New service implementation | `Truckero.Infrastructure/Services/` |
| EF model/configuration/migrations | `Truckero.Infrastructure/Data/` and `Truckero.Infrastructure/Migrations/` |
| Storage integration | `Truckero.Infrastructure/Storage/` |
| Admin-only UI work | `Truckero.Admin/Components/` |
| Corporate-only UI work | `TruckeroCorporate/Components/` |

---

## 7. Naming and Consistency Notes

- UI pages are typically `*.razor` in feature folders (`RegisterDriver.razor`, `RequestFreight.razor`, etc.)
- API clients in `TruckeroApp/ServiceClients` follow `*ApiClientService.cs`
- Repository and service contracts live in Core; implementations live in Infrastructure
- Some naming is domain-specific rather than strictly symmetric (for example `IDriverProfileRepository` is declared in `IDriverRepository.cs` and implemented by `DriverRepository`)
- Prefer matching existing naming in the target module instead of forcing new naming styles

---

## 8. Practical Rule

Before adding files, find the closest existing feature path and follow that module pattern exactly. In this repo, consistency with current module structure is more important than creating new top-level patterns.
