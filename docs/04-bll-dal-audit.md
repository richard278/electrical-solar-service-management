# BLL/DAL Architecture Audit

This audit documents the legacy architecture problems found in the BLL (Business Logic Layer) and DAL (Data Access Layer) components prior to the refactoring sprints.

---

## 1. DbContext Duplication & DI Collisions
The solution suffered from a critical DbContext duplication:
- **DAL Context**: `EcoInstalacion.DAL/DbContext/EcoInstaladorContext.cs`
- **Entity Context**: `EcoInstalacion.Entity/EcoInstaladorContext.cs`

### Consequences:
1. **Scaffolding Leftovers**: The Entity project held a legacy scaffolding file containing local connection strings.
2. **Container Overrides**: In `Program.cs`, the DAL context was registered. However, `Dependencia.cs` registered the Entity context immediately after. In ASP.NET Core DI, the last registration of a type wins. This meant the legacy, unconfigured context was silently resolving at runtime, leading to database schema mismatch risks.

*Status: **Resolved** in Sprint 3.*

---

## 2. Generic Repository Abstraction Leakage
The system uses a `GenericRepository<T>` defined as:
```csharp
public interface IGenericRepository<TModel> where TModel : class
{
    // ...
    IQueryable<TModel> Query(Expression<Func<TModel, bool>>? filtro = null);
}
```

### The Problem:
By returning `IQueryable<TModel>`, the repository leaked Entity Framework Core internal features directly into the business services.

Rather than working with clean collections of data, BLL services had to import `Microsoft.EntityFrameworkCore` to execute operations such as:
- `.Include(s => s.NavigationProperty)`
- `.ThenInclude(...)`
- `.AsNoTracking()`
- `.ToListAsync()`
- `.FirstOrDefaultAsync(...)`

This defeated the purpose of the repository pattern, coupling the Business Logic Layer to a specific ORM (EF Core).

*Status: **Partially Resolved** in Sprint 4 by introducing `SolicitudRepository` to handle queries for the Solicitudes module. Other services still require similar refactoring.*

---

## 3. Database Check Constraints & Data Validation
A database check constraint audit on the target instance revealed that specific tables enforce domain logic via SQL CHECK constraints:
- **`SolicitudesInstalacion.TipoInstalacion`**: Enforced by constraint `"CK__Solicitud__TipoI__4222D4EF"`.
  - *Allowed values*: `'Solar'` or `'Convencional'`.
  - *Consequence*: Any insert/update attempting to write other strings (e.g., `"Solar Residencial"`) will fail with a database exception.

### Recommendation:
BLL services must validate these values prior to sending commands to the repository, avoiding database roundtrips and providing clear validation messages to the user.

*Status: **Documented**. Deployed in smoke test verification inputs.*

---

## 4. Remaining BLL Coupling Audit
The following BLL service classes still reference `Microsoft.EntityFrameworkCore` and direct EF Core operations due to generic repository usage:

| Service Class | EF Core Imports | Direct EF Operations Used | Action Recommended |
|---|---|---|---|
| `UsuarioService.cs` | Yes | `Include`, `FirstOrDefaultAsync`, `ToListAsync` | Extract to `UsuarioRepository` |
| `ClienteService.cs` | Yes | `ToListAsync`, `Query()` | Decouple using DAL-level querying |
| `TecnicoService.cs` | Yes | `ToListAsync`, `Query()` | Decouple using DAL-level querying |
| `VisitaTecnicaService.cs` | Yes | `Include`, `ToListAsync`, `FirstOrDefaultAsync` | Extract to `VisitaTecnicaRepository` |
| `ReporteService.cs` | Yes | `Include`, `ToListAsync`, `Where` | Encapsulate reporting queries in DAL |

### Next Step Recommendation:
Create specific repositories for `Usuario` and `VisitaTecnica` in subsequent sprints to complete the separation of EF Core from the BLL layer.
