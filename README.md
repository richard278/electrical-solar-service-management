# Electrical Solar Service Management / EcoInstalacion

Field service management system for residential electrical and solar installation workflows built with .NET 8, ASP.NET Core (MVC/Razor + REST API), Entity Framework Core, SQL Server, and a React + TypeScript + Vite administrative SPA.

## Overview

EcoInstalacion manages operational processes related to residential electrical (`Convencional`) and solar (`Solar`) installation services. The repository demonstrates a progressive modernization strategy on top of a layered .NET 8 architecture: legacy ASP.NET Core MVC / Razor views coexist with a JWT-protected REST API consumed by a React 19 + Vite 8 + TypeScript 6 single-page application (`EcoInstalacion.Frontend`).

### Implemented Capabilities (Public Baseline `3a0e343e`)

- **Customer Management (`Clientes`)**: Registration and maintenance of customer records with Salvadoran DUI validation, status filtering (`activos`, `inactivos`, `todos`), and server-side search and pagination.
- **Technician Catalog (`Técnicos`)**: Management of technical personnel records, specialties, and contact information with server-side search and pagination.
- **Installation Requests (`SolicitudesInstalacion`)**: Operational tracking of `Solar` and `Convencional` service requests across canonical states (`Pendiente`, `Programada`, `En proceso`, `Cerrada`, `Rechazada`), technician assignment, and server-side search and pagination.
- **Technical Visits (`VisitasTecnicas`)**: Scheduling of technical visits linked to active installation requests, observation logging, irreversible visit approval (`Aprobado`), and server-side search and pagination.
- **Operational Alert Classification (`BLL`)**: Deterministic domain policy ([`OperationalAlertPolicy`](EcoInstalacion.BLL/Implementacion/OperationalAlertPolicy.cs)) evaluating pending request age (24-hour warning / 48-hour overdue thresholds) and scheduled visit dates in the `America/El_Salvador` (`UTC-06:00`) time zone.
- **JWT Authentication & Role-Based Authorization**: Token-based authentication (`POST /api/auth/login`) and server-enforced role policies for the REST API alongside cookie-based authentication for MVC views.
- **Server-Side Search & Pagination**: Paginated endpoints and debounced search (`300 ms`) integrated across the four operational modules in both the REST API and the React SPA.
- **PDF Operational Reporting (MVC)**: Generation and download of the assigned technicians report (`Reporte_Tecnicos_Asignados.pdf`) filtered by date range using QuestPDF in the MVC interface.

## Architecture & Tech Stack

The system follows a **Layered Architecture** where presentation concerns are undergoing progressive modernization without disrupting existing MVC workflows:

```text
React 19 + Vite 8 + TypeScript 6 SPA (EcoInstalacion.Frontend)
                    │  (HTTPS / HTTP JSON + JWT Bearer)
                    ▼
ASP.NET Core 8 Presentation Layer (EcoInstalacion.AppWeb)
  ├── REST API Controllers (/api/*) + DTOs + Swagger (Development)
  └── Legacy MVC Controllers + Razor Views + Cookie Auth + Antiforgery
                    │
                    ▼
Business Logic Layer (EcoInstalacion.BLL)
  ├── Domain Services (Cliente, Tecnico, Solicitud, VisitaTecnica, Rol, Usuario, Reporte)
  ├── Operational Clock & Alert Policy (America/El_Salvador)
  └── QuestPDF Document Generation (ReportePdfService)
                    │
                    ▼
Data Access Layer (EcoInstalacion.DAL)
  └── EF Core 8 (EcoInstaladorContext) + Domain & Generic Repositories ──► SQL Server
```

| Layer / Surface | Technology Stack |
| :--- | :--- |
| **Backend Runtime & Framework** | .NET 8 (`C# 12`), ASP.NET Core 8 (MVC / Razor Views + Web API Controllers) |
| **Frontend Administrative SPA** | React 19, TypeScript 6, Vite 8, native `fetch` HTTP client, `lucide-react`, `oxlint` |
| **Data Access & Persistence** | Entity Framework Core 8 (`EcoInstaladorContext`), Microsoft SQL Server |
| **Security & Auth** | ASP.NET Core Cookie Authentication (MVC) + JWT Bearer Authentication (REST API) |
| **API Documentation** | Swashbuckle / Swagger UI (enabled in `Development` environment) |
| **Reporting** | QuestPDF (`2025.7.4`, Community License, operated from MVC `ReportesController`) |
| **Automated Unit Testing** | xUnit (`2.9.3`), `Microsoft.NET.Test.Sdk` (`tests/EcoInstalacion.BLL.Tests`) |

## Authentication & Authorization

Access control is governed by a closed canonical catalog of two operational roles defined in [`RolesSistema.cs`](EcoInstalacion.Entity/Seguridad/RolesSistema.cs):

- **`Administrador`** (`IdRol = 1`)
- **`Técnico`** (`IdRol = 2`)

### Security Model

1. **MVC / Razor Surface**: Uses ASP.NET Core Cookie Authentication (`/Home/Index` login path, 60-minute sliding expiration) and Antiforgery token validation (`RequestVerificationToken`).
2. **REST API Surface (`/api/*`)**: Uses JWT Bearer tokens issued by `POST /api/auth/login`. API authorization policies (`ApiAuthenticated` and `ApiAdminOnly`) explicitly require `JwtBearerDefaults.AuthenticationScheme` and do not accept MVC cookies.
3. **React SPA Session**: Stores the JWT access token strictly in React memory (`AuthContext`), attaches `Authorization: Bearer <token>` via the centralized HTTP client, and automatically closes the session when `expiresAtUtc` is reached.
4. **Effective Role Permissions (API & React SPA)**:
   - **`Administrador`**: Full read and write permissions (`GET`, `POST`, `PUT`, `DELETE`) across `Clientes` (including `activos`, `inactivos`, and `todos`), `Técnicos`, `Solicitudes`, `Visitas Técnicas`, and `Roles`. In the MVC surface, exclusive access to `Usuarios`, `Roles`, `Tecnicos`, `VisitasTecnicas`, and `Reportes` (including PDF export).
   - **`Técnico`**: Read access (`GET`) to active `Clientes`, `Técnicos`, `Solicitudes`, and `Visitas Técnicas`, plus permission to register new active customers (`POST /api/clientes`). Cannot modify or delete customers, technicians, requests, or visits through administrative endpoints, nor access inactive customer records.

For detailed endpoint rules and security design, see [08. API Contract](docs/08-api-contract.md) and [10. Auth API Design](docs/10-auth-api-design.md).

## Solution Structure

- [`EcoInstalacion.AppWeb`](EcoInstalacion.AppWeb): ASP.NET Core 8 host containing MVC controllers, Razor views, REST API controllers (`Controllers/Api`), DTOs, JWT token services, and application startup ([`Program.cs`](EcoInstalacion.AppWeb/Program.cs)).
- [`EcoInstalacion.BLL`](EcoInstalacion.BLL): Business Logic Layer containing domain services, [`OperationalAlertPolicy.cs`](EcoInstalacion.BLL/Implementacion/OperationalAlertPolicy.cs), [`ElSalvadorOperationalClock.cs`](EcoInstalacion.BLL/Implementacion/ElSalvadorOperationalClock.cs), and [`ReportePdfService.cs`](EcoInstalacion.BLL/Implementacion/ReportePdfService.cs).
- [`EcoInstalacion.DAL`](EcoInstalacion.DAL): Data Access Layer encapsulating [`EcoInstaladorContext.cs`](EcoInstalacion.DAL/DbContext/EcoInstaladorContext.cs) and repository implementations.
- [`EcoInstalacion.Entity`](EcoInstalacion.Entity): Domain entities, canonical state definitions ([`EstadosSolicitudSistema.cs`](EcoInstalacion.Entity/EstadosSolicitudSistema.cs)), role definitions ([`RolesSistema.cs`](EcoInstalacion.Entity/Seguridad/RolesSistema.cs)), and pagination models.
- [`EcoInstalacion.IOC`](EcoInstalacion.IOC): Inversion of Control container registration ([`Dependencia.cs`](EcoInstalacion.IOC/Dependencia.cs)).
- [`EcoInstalacion.Frontend`](EcoInstalacion.Frontend): React 19 + Vite 8 + TypeScript 6 administrative SPA (see [Frontend README](EcoInstalacion.Frontend/README.md)).
- [`tests/EcoInstalacion.BLL.Tests`](tests/EcoInstalacion.BLL.Tests): xUnit unit test project for operational clock and alert classification policies.
- [`docs`](docs/README.md): Technical documentation index, architecture guides, API contract, and technical debt register.

## Local Execution & Reproducibility Requirements

### 1. Requirements by Activity

| Activity | Requirements | SQL Server Needed? |
| :--- | :--- | :---: |
| **Compile Backend & Frontend** | .NET 8 SDK (`8.0.x`), Node.js & npm | No |
| **Run Public Baseline xUnit Tests** | .NET 8 SDK (`tests/EcoInstalacion.BLL.Tests`) | No |
| **Start Backend API / MVC Host** | Configured `Jwt:SecretKey` ($\ge 32$ chars) and `ConnectionStrings:CadenaSql` | Yes (for DB queries) |
| **Full Operational Runtime** | Provisioned `EcoInstalador` SQL Server database (tables, trigger, stored procedure) | Yes |

> **Database Reproducibility Notice**: At public baseline `3a0e343e94ef4712fb114d58907f358d386e1a65`, `EcoInstalacion.DAL` maps to an existing SQL Server database schema (`EcoInstalador`). **EF Core migrations and a standalone SQL bootstrap script are not versioned in the public repository.** Full runtime execution requires a local SQL Server instance containing the tables documented in [07. Database Model](docs/07-database-model.md), the trigger `dbo.tr_ActualizarEstadoSolicitud` on `VisitasTecnicas`, and the reporting stored procedure consumed by `ReporteRepository`.

### 2. Backend Setup (`EcoInstalacion.AppWeb`)

1. Copy the example configuration template:
   ```bash
   cp EcoInstalacion.AppWeb/appsettings.Example.json EcoInstalacion.AppWeb/appsettings.json
   ```
2. Configure `ConnectionStrings:CadenaSql` and a valid `Jwt:SecretKey` (at least 32 characters) using .NET User Secrets or environment variables (`Jwt__SecretKey`). Startup validation in `Program.cs` will throw an `InvalidOperationException` if `Jwt:SecretKey` is missing or left with the placeholder value:
   ```bash
   dotnet user-secrets set "ConnectionStrings:CadenaSql" "Server=YOUR_SQL_SERVER;Database=EcoInstalador;Trusted_Connection=True;TrustServerCertificate=True" --project EcoInstalacion.AppWeb
   dotnet user-secrets set "Jwt:SecretKey" "REPLACE_WITH_A_SECURE_SIGNING_KEY_AT_LEAST_32_CHARS" --project EcoInstalacion.AppWeb
   ```
3. Restore, build, and run the backend (defaults to `http://localhost:5240`):
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project EcoInstalacion.AppWeb
   ```

### 3. Frontend Setup (`EcoInstalacion.Frontend`)

1. Configure the local environment file and install packages:
   ```bash
   cd EcoInstalacion.Frontend
   cp .env.example .env.local
   npm install
   ```
   By default, `VITE_API_BASE_URL` targets `http://localhost:5240`.
2. Run static validation or start the Vite development server (`http://localhost:5173`, permitted by backend CORS in development):
   ```bash
   npm run lint
   npm run build
   npm run dev
   ```

## Test Evidence (Baseline `3a0e343e94ef4712fb114d58907f358d386e1a65`)

The public baseline unit test suite was verified in an isolated checkout (`git archive` extraction outside the workspace) during audit **ECO-PORT-01-REV1**:

- **Verified Baseline SHA**: `3a0e343e94ef4712fb114d58907f358d386e1a65`
- **Verification Date**: November 11, 2026 (`2026-11-11`)
- **Test Project**: [`tests/EcoInstalacion.BLL.Tests/EcoInstalacion.BLL.Tests.csproj`](tests/EcoInstalacion.BLL.Tests/EcoInstalacion.BLL.Tests.csproj)
- **Configuration & Runner**: `Release` (`.NET SDK 8.0.316`, `xUnit.net VSTest Adapter v2.8.2`)
- **Command**:
  ```bash
  dotnet test tests/EcoInstalacion.BLL.Tests/EcoInstalacion.BLL.Tests.csproj --configuration Release
  ```
- **Verified Results**:
  - **Declared Test Methods**: `26` (`18` `[Fact]` methods + `8` `[Theory]` methods)
  - **Discovered & Executed xUnit Test Cases**: `49` (`18` `[Fact]` + `31` `[InlineData]` cases)
    - [`OperationalAlertPolicySolicitudesTests`](tests/EcoInstalacion.BLL.Tests/OperationalAlertPolicySolicitudesTests.cs): `9` methods $\rightarrow$ `20` cases (`20` passed)
    - [`OperationalAlertPolicyVisitasTests`](tests/EcoInstalacion.BLL.Tests/OperationalAlertPolicyVisitasTests.cs): `8` methods $\rightarrow$ `9` cases (`9` passed)
    - [`OperationalClockAndConfigurationTests`](tests/EcoInstalacion.BLL.Tests/OperationalClockAndConfigurationTests.cs): `9` methods $\rightarrow$ `20` cases (`20` passed)
  - **Outcome**: `49` Passed, `0` Failed, `0` Skipped (`Exit Code 0`)

> **Scope of Test Evidence**: This result accredits exclusively the isolated execution of `tests/EcoInstalacion.BLL.Tests` from public commit `3a0e343e94ef4712fb114d58907f358d386e1a65`. It does not represent end-to-end system testing, SQL Server DAL integration testing, HTTP API controller testing, PDF rendering verification, or automated React UI testing.

## Deployment Status & Known Limitations

- **Local Development Lab**: Verified locally with synthetic test data across the .NET 8 backend and React + Vite SPA.
- **No Public SQL Bootstrap Confirmed**: Database creation scripts, triggers, and stored procedures are not yet versioned as public migrations or SQL initialization scripts.
- **Containerization & CI/CD Pending**: Docker (`Dockerfile` / `docker-compose.yml`) is not implemented, and GitHub Actions workflows (`.github/workflows`) are pending.
- **Public Live Demo Pending**: The system is not currently deployed to a public cloud URL; hosting options are outlined in [06. Deployment Roadmap](docs/06-deployment-roadmap.md).
- **PDF Reporting Surface**: Operational PDF export (`QuestPDF`) is active in the legacy MVC module (`ReportesController`) for `Administrador` users and is not yet integrated as a verified React SPA workflow.
- **SPA Session & Navigation Constraints**:
  - JWT access tokens are held in memory only; reloading the browser clears the session and requires re-authentication (refresh tokens and token revocation are not implemented).
  - Section navigation in `EcoInstalacion.Frontend` is managed via React state ([`AppShell.tsx`](EcoInstalacion.Frontend/src/app/AppShell.tsx)) without URL deep-linking (`react-router` is pending).
- **Endpoint-Level Technician Scope**: While `Técnico` users are restricted from mutating requests, visits, and technicians and cannot access inactive customers, read endpoints do not yet filter installation requests or technical visits by individual assigned technician.
- **Mobile Client & Offline Sync Planned**: React Native / Expo field client and offline synchronization described in [09. Mobile Readiness](docs/09-mobile-readiness.md) remain planned roadmap items.

## Technical Documentation

Full architectural references, audits, and design documents are indexed in [`docs/README.md`](docs/README.md):

- [01. Project Overview](docs/01-project-overview.md)
- [02. Layered Architecture](docs/02-layered-architecture.md)
- [03. Refactoring History](docs/03-refactor-history.md)
- [04. BLL/DAL Architecture Audit](docs/04-bll-dal-audit.md)
- [05. Domain Rules](docs/05-domain-rules.md)
- [06. Deployment Roadmap](docs/06-deployment-roadmap.md)
- [07. Database Model](docs/07-database-model.md)
- [08. API Contract](docs/08-api-contract.md)
- [09. Mobile Readiness](docs/09-mobile-readiness.md)
- [10. Auth API Design](docs/10-auth-api-design.md)
- [11. Technical Debt Register](docs/11-technical-debt-register.md)
- [Target Architecture (React + Vite Modernization)](docs/architecture/target-architecture.md)
- [Frontend SPA Documentation](EcoInstalacion.Frontend/README.md)
