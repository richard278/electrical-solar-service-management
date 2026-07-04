# Refactoring History

This document logs the step-by-step refactoring history of the **Electrical Solar Service Management / EcoInstalacion** project, detailing changes made to improve codebase hygiene, architecture, and decoupling.

---

## Commit Log Reference
The following commits form the baseline of the refactoring process (retrieved via `git log --oneline -5`):
- `13e0a01` — `refactor: extract SolicitudRepository from SolicitudService`
- `eab4c1d` — `refactor: consolidate dbcontext registration`
- `82e616e` — `chore: update repository hygiene and documentation`
- `002a24d` — `refactor: resolve compiler warnings`

---

## Sprint 1: Repository Hygiene & Warning Resolution
**Objective**: Fix baseline compilation issues, clean up the workspace, and align project presentation.

### Key Work Done:
1. **Compilation Cleanup**: Resolved active C# compiler warnings to achieve a completely clean build status (`0 warnings`, `0 errors`).
2. **Repository Hygiene**: Refined `.gitignore` to prevent tracking of build artifacts (`/bin`, `/obj`, `/dist`, local IDE properties, and databases).
3. **Environment Security**: Created `appsettings.Example.json` to act as a template for connection strings, and removed sensitive credentials from version control.
4. **Documentation Alignment**: Re-wrote the root `README.md` to represent the project as a field service management system for residential electrical/solar services instead of legacy medical software.

---

## Sprint 2 & 3: DbContext Consolidation
**Objective**: Eliminate DbContext duplication and consolidate the dependency injection container.

### Problem Identified:
The codebase contained **two separate DbContext definitions**:
- `EcoInstalacion.DAL/DbContext/EcoInstaladorContext.cs` (Active version containing entity mapping configurations).
- `EcoInstalacion.Entity/EcoInstaladorContext.cs` (Legacy scaffolding version with local database connection paths).

These two contexts collided in the dependency container. `Program.cs` registered the DAL context, but `IOC/Dependencia.cs` registered the Entity context immediately after, overriding it and causing silent DI failures.

### Changes Applied:
1. **Legacy Cleanup**: Deleted `EcoInstalacion.Entity/EcoInstaladorContext.cs`.
2. **DI Cleanup**: Removed `AddDbContext` from `EcoInstalacion.IOC/Dependencia.cs` and cleaned up unused `using` statements.
3. **Single Source of Truth**: Centralized context setup in `Program.cs`, targeting exclusively `EcoInstalacion.DAL/DbContext/EcoInstaladorContext.cs`.

---

## Sprint 4: Decoupling BLL from Entity Framework Core
**Objective**: Isolate `SolicitudService` from direct Entity Framework dependency by introducing a specific repository.

### Problem Identified:
`SolicitudService` in the BLL layer imported `Microsoft.EntityFrameworkCore` and directly executed eager-loading queries (e.g., `.Include(s => s.Cliente).ToListAsync()`) against the generic repository's `Query()` method. This leaked database concerns into the business layer, breaking architecture boundaries.

### Changes Applied:
1. **Repository Interface**: Created `ISolicitudRepository` inside the `DAL` layer defining clear, asynchronous query methods:
   - `ListaConRelaciones()`
   - `ObtenerConRelaciones(int id)`
   - `ObtenerConVisitas(int id)`
2. **Repository Implementation**: Implemented `SolicitudRepository` inside the `DAL` layer, encapsulating all EF Core-specific statements.
3. **DI Registration**: Registered `ISolicitudRepository` in `IOC/Dependencia.cs` as `AddScoped`.
4. **BLL Decoupling**: Updated `SolicitudService` to use `ISolicitudRepository` for data retrieval, removing `using Microsoft.EntityFrameworkCore` completely. Business rules, default values, and validations remained intact in the service.
5. **Manual Smoke Testing**: Successfully validated all 10 Solicitud flows via HTTP requests, verifying list load, creation, duplicate protection, edit, delete with visits blocker, and technician assignment.
