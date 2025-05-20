# 📁 Truckero Code Structure & Organization Guide

This document outlines the **file and folder structure** conventions used in the Truckero platform. Use this guide to determine where to place new files and how to navigate the codebase efficiently.

---

## 🧱 Projects and Their Roles

| Project                   | Role                                 |
| ------------------------- | ------------------------------------ |
| `TruckeroApp`             | Blazor Hybrid app (UI for all roles) |
| `Truckero.API`            | ASP.NET Core Web API endpoints       |
| `Truckero.Core`           | Entities, DTOs, Interfaces, Enums    |
| `Truckero.Infrastructure` | Repositories, Services, EF, Storage  |

---

## 📦 TruckeroApp (Blazor Client)

### Key Folders

* `Components/Pages/[Module]/` → Razor screens by feature (e.g., Onboarding)
* `ServiceClients/` → API proxy classes (e.g., `DriverApiClientService.cs`)
* `Interfaces/` → Frontend abstractions (`ITokenStorageService.cs`)
* `Services/` → UI logic or wrappers (e.g., `AuthSessionContextService.cs`)
* `Session/` → Auth/session context models
* `AppRoutes.cs` → Central routing file

### File Naming

* Screen: `RegisterDriver.razor`
* API Client: `XyzApiClientService.cs`
* Interface: `IServiceName.cs`

---

## 🌐 Truckero.API (Controllers + Setup)

### Key Folders

* `Controllers/` → REST endpoints (e.g., `DriverController.cs`)
* `Models/` → Request/response models for controller binding
* `Services/` → Optional stateless helpers
* `Filters/` → Middleware logic (auth, validation)
* `RepositoryServiceSetup.cs` → Registers DI from Core/Infrastructure

### Notes

* Each controller corresponds to a feature domain (Auth, Driver, Vehicle)
* Minimal business logic in controllers — delegate to `Services`

---

## 🧠 Truckero.Core (Domain, DTOs, Interfaces)

### Key Folders

* `Entities/` → EF Core models (e.g., `User.cs`, `DriverProfile.cs`)
* `DTOs/[Module]/` → Data transfer objects (e.g., `RegisterCustomerRequest.cs`)
* `Interfaces/Repositories/` → Contracts for data access
* `Interfaces/Services/` → Business logic contracts
* `Enums/` → Centralized enum types
* `DataAnnotations/` → Custom validation attributes
* `Constants/` → Shared constants like keys or tokens

### Notes

* No logic or implementation here
* Used by both `Truckero.API` and `Truckero.Infrastructure`

---

## 🏗️ Truckero.Infrastructure (EF, Services, Storage)

### Key Folders

* `Repositories/` → Concrete implementations (e.g., `DriverRepository.cs`)
* `Services/` → Core business logic (e.g., `OnboardingService.cs`)
* `Storage/` → File, blob, or cache services (e.g., `AzureBlobStorageService.cs`)
* `Data/` → `AppDbContext.cs`, migrations, seeding
* `Extensions/` → Utility methods (e.g., ClaimsPrincipal extensions)

### Implementation Rules

* Class `XyzRepository` must implement `IXyzRepository`
* Same for `XyzService` ↔ `IXyzService`

---

## 🔁 Common File Relationships

```plaintext
DTO (Core) → used in → Blazor Pages, API Controllers
Entity (Core) ↔ DTO → via AutoMapper or manual mapping
Controller (API) → calls → Service (Infrastructure) → calls → Repository
```

---

## 📄 Sample File Path Map

```plaintext
TruckeroApp/
  Components/Pages/Onboarding/RegisterDriver.razor
  ServiceClients/OnboardingApiClientService.cs
  AppRoutes.cs

Truckero.API/
  Controllers/OnboardingController.cs
  RepositoryServiceSetup.cs

Truckero.Core/
  DTOs/Onboarding/DriverProfileRequest.cs
  Entities/DriverProfile.cs
  Interfaces/Repositories/IDriverRepository.cs
  Interfaces/Services/IOnboardingService.cs

Truckero.Infrastructure/
  Repositories/DriverRepository.cs
  Services/OnboardingService.cs
  Data/AppDbContext.cs
```

---

## ✅ Naming Best Practices

| Item           | Convention                            |
| -------------- | ------------------------------------- |
| DTO            | `SomethingRequest.cs` / `Response.cs` |
| Interface      | `INameService`, `INameRepository`     |
| Implementation | `NameService`, `NameRepository`       |
| Razor Page     | `RegisterSomething.razor`             |
| Enum           | PascalCase with singular naming       |

---

## 📌 Summary

* Organize by **feature and responsibility**
* Match interfaces to implementations across Core ↔ Infrastructure
* Keep logic out of Controllers and Razor pages — push down to Services
* Maintain strict naming and placement consistency

---
