# Domain Rules

This document maps out the specific domain rules and validation checks currently enforced within the Business Logic Layer (BLL), focusing on the lifecycle of installation requests.

---

## 1. Service Requests Lifecycle (`SolicitudesInstalacion`)

### A. Inception & Defaults (Crear)
- **Null Safety**: A service request payload must not be null. An `ArgumentNullException` is thrown if null.
- **Auto-date**: If no `FechaSolicitud` is provided by the client, the system automatically assigns the current UTC timestamp:
  ```csharp
  entidad.FechaSolicitud = DateTime.UtcNow;
  ```
- **Default Status**: New installation requests default to the **"Pendiente"** state if not explicitly specified.

### B. Business Uniqueness (Duplicidad)
To prevent customers from opening multiple requests for the same task:
- **Rule**: A customer cannot have more than one open (non-Cerrada) request with the exact same description.
- **Enforcement**: Prior to saving, the service queries the database:
  ```csharp
  var duplicada = await _repositorio.ObtenerDatos(s =>
      s.ClienteId == entidad.ClienteId &&
      s.Descripcion == entidad.Descripcion &&
      s.Estado != "Cerrada");
  ```
  If a matching request is found, creation is aborted with a `TaskCanceledException`.

### C. Technical Constraints (Database Constraints)
- **Installation Type**: `TipoInstalacion` must be either **"Solar"** or **"Convencional"** to satisfy database check constraints.

---

## 2. Request Modification (`Editar`)
- **Existence Check**: The system validates that the target request exists in the database. If not found, a `KeyNotFoundException` is raised.
- **Protected Fields**: The original `FechaSolicitud` is preserved if the update payload sends a default/empty date.
- **Technician Allocation**: Technicians can be assigned or removed (`tecnicoAsignadoId` is nullable).

---

## 3. Request Deletion (`Eliminar`)
- **Existence Check**: If the target request does not exist, the operation returns `false`.
- **Referential Integrity Blocker**: To preserve operational history, a request cannot be deleted if it has associated technical visits:
  ```csharp
  if (existente.VisitasTecnicas != null && existente.VisitasTecnicas.Any())
      throw new TaskCanceledException("No se puede eliminar: existen visitas técnicas asociadas.");
  ```
  This is a critical business integrity guard.

---

## 4. Separation of Concerns Matrix

| Operational Task | BLL Responsibility | DAL Responsibility |
|---|---|---|
| **Request Creation** | Set defaults, check duplicate open requests, throw validation exceptions | Insert into DB, return ID |
| **Request Editing** | Validate ID, preserve original fields if needed, perform field mappings | Commit updates |
| **Request Deletion** | Check child visit associations, block if visits exist | Execute `Remove` database command |
| **Data Querying** | None | Fetch data, apply eager loading (`Include`), sort, apply `AsNoTracking` |
