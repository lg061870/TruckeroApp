# How To Add A New Entity API (End?to?End)

This document defines the standard pattern for adding a new entity to Truckero:

- Domain entity (EF Core)
- `AppDbContext` mapping
- Repository interface + implementation
- Service interface + implementation
- API controller (CRUD + common queries)
- DTOs (`<Entity>Request`, `<Entity>Response`, list/metadata responses)
- Client?side API client (for Blazor/MAUI)

The intent is that an AI (or a human) can follow this as a **single to?do list** and implement all pieces consistently.

> Throughout this doc, replace `Xxx` with the entity name (e.g. `Company`, `Store`, `DriverBid`, etc.).

---

## 1. Design The Domain Entity

**Goal:** Define the EF Core entity in `Truckero.Core/Entities`.

1. Create `Truckero.Core/Entities/Xxx.cs` with:

   - `public class Xxx { Guid Id { get; set; } }` as the PK.
   - Scalar properties (names, codes, timestamps) with appropriate types and nullability.
   - Navigation properties for relationships (FKs to existing entities and collections where needed).
   - Data annotations only where needed (e.g., `[MaxLength]`, `[Required]` for constraints your DB should enforce).

   Example:

   ```csharp
   namespace Truckero.Core.Entities;

   public class Xxx
   {
       public Guid Id { get; set; }

       public string Name { get; set; } = null!;
       public string? Description { get; set; }

       // Foreign key to Company (example)
       public Guid CompanyId { get; set; }
       public Company Company { get; set; } = null!;

       public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
       public DateTime? UpdatedAt { get; set; }
   }
   ```

2. For many?to?many relationships, prefer explicit join entities (like `CompanyIndustry`) and configure them in `AppDbContext`.

---

## 2. Wire Up `AppDbContext`

**Goal:** Add `DbSet<Xxx>` and model configuration in `Truckero.Infrastructure/Data/AppDbContext.cs`.

1. Add a `DbSet`:

   ```csharp
   public DbSet<Xxx> Xxxs => Set<Xxx>();
   ```

2. In `OnModelCreating`, configure:

   - Primary key:

     ```csharp
     modelBuilder.Entity<Xxx>().HasKey(x => x.Id);
     ```

   - Property constraints:

     ```csharp
     modelBuilder.Entity<Xxx>(entity =>
     {
         entity.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(100);

         entity.Property(x => x.Description)
               .HasMaxLength(512);
     });
     ```

   - Relationships and delete behaviors:

     ```csharp
     modelBuilder.Entity<Xxx>()
         .HasOne(x => x.Company)
         .WithMany(c => c.Xxxs)
         .HasForeignKey(x => x.CompanyId)
         .OnDelete(DeleteBehavior.Restrict);
     ```

3. If needed, seed data with `HasData` and fixed GUIDs (follow existing examples for roles, business types, etc.).

---

## 3. Create DTOs (`XxxRequest`, `XxxResponse`)

**Goal:** Define API/Blazor?friendly DTOs in `Truckero.Core/DTOs`.

1. Add `Truckero.Core/DTOs/XxxRequest.cs`:

   ```csharp
   namespace Truckero.Core.DTOs;

   public class XxxRequest
   {
       public Guid Id { get; set; }           // Guid.Empty for create
       public Guid CompanyId { get; set; }    // or other FK

       public string Name { get; set; } = null!;
       public string? Description { get; set; }
   }
   ```

2. Add `Truckero.Core/DTOs/XxxResponse.cs` (single entity):

   ```csharp
   namespace Truckero.Core.DTOs;

   public class XxxResponse : BaseResponse
   {
       public XxxRequest? Xxx { get; set; }
   }
   ```

3. Add `Truckero.Core/DTOs/XxxListResponse.cs` (list):

   ```csharp
   namespace Truckero.Core.DTOs;

   public class XxxListResponse : BaseResponse
   {
       public List<XxxRequest> Xxxs { get; set; } = new();
   }
   ```

4. For metadata, either:
   - Extend an existing response (e.g., add lists to `XxxResponse`), or
   - Define `XxxMetadataResponse` with lists of reference DTOs.

---

## 4. Define Repository Interfaces

**Goal:** Add small, focused repo interfaces in `Truckero.Core/Interfaces/Repositories`.

1. Create `IXxxRepository`:

   ```csharp
   using Truckero.Core.Entities;

   namespace Truckero.Core.Interfaces.Repositories;

   public interface IXxxRepository
   {
       Task<Xxx?> GetByIdAsync(Guid id);

       // Typical FK query (adapt as needed)
       Task<List<Xxx>> GetByCompanyIdAsync(Guid companyId);

       Task AddAsync(Xxx entity);
       Task UpdateAsync(Xxx entity);
       Task RemoveAsync(Xxx entity);

       Task SaveChangesAsync();
   }
   ```

2. Add extra methods for common queries as needed:

   ```csharp
   Task<bool> ExistsAsync(Guid id);
   Task<List<Xxx>> SearchAsync(string term);
   ```

Keep interfaces slim and domain?focused.

---

## 5. Implement Repositories

**Goal:** Implement `IXxxRepository` in `Truckero.Infrastructure/Repositories`.

1. Create `Truckero.Infrastructure/Repositories/XxxRepository.cs`:

   ```csharp
   using Microsoft.EntityFrameworkCore;
   using Truckero.Core.Entities;
   using Truckero.Core.Interfaces.Repositories;
   using Truckero.Infrastructure.Data;

   namespace Truckero.Infrastructure.Repositories;

   public class XxxRepository : IXxxRepository
   {
       private readonly AppDbContext _context;

       public XxxRepository(AppDbContext context)
       {
           _context = context;
       }

       public async Task<Xxx?> GetByIdAsync(Guid id) =>
           await _context.Xxxs.FirstOrDefaultAsync(x => x.Id == id);

       public async Task<List<Xxx>> GetByCompanyIdAsync(Guid companyId) =>
           await _context.Xxxs
               .AsNoTracking()
               .Where(x => x.CompanyId == companyId)
               .ToListAsync();

       public async Task AddAsync(Xxx entity) =>
           await _context.Xxxs.AddAsync(entity);

       public Task UpdateAsync(Xxx entity)
       {
           _context.Xxxs.Update(entity);
           return Task.CompletedTask;
       }

       public Task RemoveAsync(Xxx entity)
       {
           _context.Xxxs.Remove(entity);
           return Task.CompletedTask;
       }

       public async Task SaveChangesAsync() =>
           await _context.SaveChangesAsync();
   }
   ```

2. Register in `Truckero.API/Program.cs`:

   ```csharp
   builder.Services.AddScoped<IXxxRepository, XxxRepository>();
   ```

---

## 6. Define Service Interfaces

**Goal:** Add a domain?level service in `Truckero.Core/Interfaces/Services`.

1. Create `IXxxService`:

   ```csharp
   using Truckero.Core.DTOs;

   namespace Truckero.Core.Interfaces.Services;

   public interface IXxxService
   {
       Task<XxxResponse> CreateAsync(XxxRequest request);
       Task<XxxResponse> UpdateAsync(Guid id, XxxRequest request);
       Task<BaseResponse> DeleteAsync(Guid id);

       Task<XxxResponse> GetByIdAsync(Guid id);
       Task<XxxListResponse> GetByCompanyIdAsync(Guid companyId);
   }
   ```

---

## 7. Implement Services

**Goal:** Implement `IXxxService` in `Truckero.Infrastructure/Services` using the repository and DTO mapping.

1. Create `Truckero.Infrastructure/Services/XxxService.cs` (follow the pattern used in `StoreService`), mapping `Xxx` ? `XxxRequest`, catching `DbUpdateException`, and returning `XxxResponse` / `XxxListResponse` / `BaseResponse`.

2. Register the service in `Truckero.API/Program.cs`:

   ```csharp
   builder.Services.AddScoped<IXxxService, XxxService>();
   ```

---

## 8. Create The API Controller

**Goal:** Expose the usual CRUD + query endpoints in `Truckero.API/Controllers/XxxController.cs`.

1. Add `XxxController` using `IXxxService` and follow the pattern from `StoreController` / `CompanyController`:

   - `GET /api/Xxx/{id}` ? `GetById`
   - `GET /api/Xxx/company/{companyId}` (or other FK route)
   - `POST /api/Xxx` ? `Create`
   - `PUT /api/Xxx/{id}` ? `Update`
   - `DELETE /api/Xxx/{id}` ? `Delete`

2. Use `XxxResponse`, `XxxListResponse`, and `BaseResponse` as return types.

---

## 9. Client?Side API Client (Blazor/MAUI)

**Goal:** Provide a typed client in `TruckeroApp/ServiceClients` (and DI registration).

1. Create `TruckeroApp/ServiceClients/XxxApiClientService.cs` that:

   - Calls `/api/Xxx` for create/update.
   - Calls `/api/Xxx/{id}` to get by id.
   - Calls `/api/Xxx/company/{companyId}` for FK queries.
   - Calls `/api/Xxx/{id}` with `DELETE`.

   Follow the `CompanyApiClientService` pattern.

2. Register the client in `TruckeroApp/ApiClientProxies.cs` with an `AddXxxApiClientService` extension.

---

## 10. Integration Tests (Recommended)

**Goal:** Verify controller + service + repository + EF mapping.

1. In `Truckero.API.Tests`, add `XxxControllerTests`:

   - Use `WebApplicationFactory<Program>`.
   - Override `AppDbContext` to in?memory DB.
   - Test:
     - `Create` + `GetById` round trip.
     - `GetByCompanyId`.
     - `Delete`.
     - Edge cases (404s, validation errors).

Use existing `CompanyControllerTests` and `StoreControllerTests` as templates.

---

## 11. Checklist Summary

When adding a new entity `Xxx`, perform **all** of these:

1. [ ] Add `Xxx` entity to `Truckero.Core/Entities`.
2. [ ] Add `DbSet<Xxx>` and mapping in `AppDbContext`.
3. [ ] Add `XxxRequest`, `XxxResponse`, `XxxListResponse` DTOs.
4. [ ] Add `IXxxRepository` in `Truckero.Core/Interfaces/Repositories`.
5. [ ] Add `XxxRepository` in `Truckero.Infrastructure/Repositories`.
6. [ ] Register `IXxxRepository` in `Truckero.API/Program.cs`.
7. [ ] Add `IXxxService` in `Truckero.Core/Interfaces/Services`.
8. [ ] Add `XxxService` in `Truckero.Infrastructure/Services`.
9. [ ] Register `IXxxService` in `Truckero.API/Program.cs`.
10. [ ] Add `XxxController` in `Truckero.API/Controllers`.
11. [ ] Add `XxxApiClientService` in `TruckeroApp/ServiceClients`.
12. [ ] Register the API client in `TruckeroApp/ApiClientProxies.cs`.
13. [ ] (Optional) Add `XxxControllerTests` to `Truckero.API.Tests`.

Following all these steps ensures a complete, consistent end?to?end implementation that matches the existing Truckero architecture and patterns.
