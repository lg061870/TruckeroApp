# 🚚 Truckero Code Organization Reference Guide

---

## 🔷 Overview of Major Layers

| Layer              | Project                   | Purpose                                                        |
| ------------------ | ------------------------- | -------------------------------------------------------------- |
| 📱 Frontend (UI)   | `TruckeroApp`             | Razor UI for Blazor Hybrid/Mobile apps                         |
| 🌐 API Gateway     | `Truckero.API`            | ASP.NET Core Web API controllers                               |
| 🧠 Core Domain     | `Truckero.Core`           | Entities, DTOs, Interfaces, Enums                              |
| 🏗️ Infrastructure | `Truckero.Infrastructure` | Service & repository implementations, DbContext, storage logic |

---

## 🧱 Folder Conventions by Layer

### ✅ TruckeroApp (Blazor App UI)

| Folder             | Purpose                                                     |
| ------------------ | ----------------------------------------------------------- |
| `Components/Pages` | Razor screens organized by feature (e.g., `RegisterDriver`) |
| `ServiceClients`   | Typed HTTP clients for API calls                            |
| `Interfaces`       | UI-side service abstractions (e.g., `ISessionContext`)      |
| `Services`         | Local services (e.g., token storage)                        |
| `Session/Routes`   | Session state & route definitions                           |
| `wwwroot`          | Static assets like Tailwind config                          |

**Naming Convention**:

* `Register*.razor` for screens
* `*.ApiClientService.cs` for HTTP clients
* Interfaces prefixed with `I`

---

### ✅ Truckero.API (API Layer)

| Folder                      | Purpose                                                              |
| --------------------------- | -------------------------------------------------------------------- |
| `Controllers`               | REST endpoints like `DriverController.cs`, `OnboardingController.cs` |
| `Models`                    | API-only DTOs (e.g., `BlobUploadRequest.cs`)                         |
| `Services`                  | Stateless helpers used by controllers                                |
| `Filters`                   | Auth and error middleware                                            |
| `Program.cs`                | Web host setup & middleware                                          |
| `RepositoryServiceSetup.cs` | Registers infrastructure dependencies                                |

**Pattern**:
API controllers → Services → Core interfaces

---

### ✅ Truckero.Core (Domain Layer)

| Folder                         | Purpose                                        |
| ------------------------------ | ---------------------------------------------- |
| `Entities`                     | EF Core entities (e.g., `DriverProfile.cs`)    |
| `DTOs/Auth`, `DTOs/Onboarding` | DTOs for API and frontend form mapping         |
| `Interfaces/Repositories`      | Repository interfaces                          |
| `Interfaces/Services`          | Domain service contracts                       |
| `Enums`                        | Domain enums (e.g., `VehicleType`, `RoleType`) |
| `DataAnnotations`              | Custom validators                              |
| `Constants`                    | Static values, keys, roles                     |

> ❗ Core contains no implementation logic.

---

### ✅ Truckero.Infrastructure (Service & DB Layer)

| Folder         | Purpose                                             |
| -------------- | --------------------------------------------------- |
| `Repositories` | EF Core repository implementations                  |
| `Services`     | Business logic implementations                      |
| `Data`         | `AppDbContext`, migrations, seeding                 |
| `Storage`      | External systems (e.g., AzureBlobStorageService.cs) |
| `Extensions`   | Helpers like ClaimsPrincipal extension methods      |

**Naming Rule**:

```csharp
public class DriverRepository : IDriverRepository
```

---

## 🔀 Standard Flow: UI → API → Service → Repo → DB

```plaintext
[Blazor Page]
↓ binds to
[DTO] (in Core)
↓ sent via
[ApiClientService] → [API Controller]
↓ calls
[Service Interface] (in Core)
↓ handled by
[Service Implementation] (Infrastructure)
↓ calls
[Repository Interface] (in Core)
↓ handled by
[Repository Implementation] (Infrastructure)
↓ persists to
[DbContext → SQL DB]
```

---

## 🛠️ Where to Place New Code

| Task                             | Folder / File Location                                       |
| -------------------------------- | ------------------------------------------------------------ |
| Add new screen                   | `TruckeroApp/Components/Pages/Onboarding/RegisterX.razor`    |
| Add API controller               | `Truckero.API/Controllers/[Module]Controller.cs`             |
| Define new DTO                   | `Truckero.Core/DTOs/[Module]/MyDto.cs`                       |
| Add interface for service        | `Truckero.Core/Interfaces/Services/I[Name]Service.cs`        |
| Add interface for repository     | `Truckero.Core/Interfaces/Repositories/I[Name]Repository.cs` |
| Add service logic implementation | `Truckero.Infrastructure/Services/[Name]Service.cs`          |
| Add EF repository implementation | `Truckero.Infrastructure/Repositories/[Name]Repository.cs`   |
| Modify DB mappings               | `Truckero.Infrastructure/Data/AppDbContext.cs`               |
| Register DI                      | `RepositoryServiceSetup.cs` and `MauiProgram.cs`             |

---

## 🧪 Unit Test Integration

| Test Target      | Suggested File                                            |
| ---------------- | --------------------------------------------------------- |
| Razor page logic | `TruckeroApp.Tests/RegisterDriver.razor.test.cs`          |
| API controller   | `Truckero.API.Tests/OnboardingControllerTests.cs`         |
| Service logic    | `Truckero.Infrastructure.Tests/OnboardingServiceTests.cs` |
| Repository logic | `Truckero.Infrastructure.Tests/DriverRepositoryTests.cs`  |

Use `Moq`, `FakeItEasy`, or `TestAuthHandler` for mock setup.

---

## 🔒 Security & Logging Guidelines

* Use `[Authorize]` or `[AllowAnonymous]` on API endpoints
* Log via `ILogger<T>` and optionally `AuditLogRepository`
* Validate on both client (Blazor) and server (ModelState)

---

## ✅ Summary of Best Practices

* 🧱 **Domain-first** design: start from Core definitions
* 📦 **Modular structure**: feature-split with consistent layering
* ♻️ **Bi-directional DTO ↔ Entity mappings**
* 🔐 **Secure APIs** with token validation
* 🧪 **Testable** through clean separation and DI

---
