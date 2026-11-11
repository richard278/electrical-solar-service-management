# EcoInstalacion Technical Documentation

This directory contains the technical documentation for **Electrical Solar Service Management / EcoInstalacion**. It organizes the current system reference guides, architectural modernization decisions, and historical refactoring audits for the public baseline (`3a0e343e94ef4712fb114d58907f358d386e1a65`).

---

## 1. Current Technical Reference

- [01. Project Overview](01-project-overview.md): High-level system purpose, operational domain scope, core modules, and portfolio positioning.
- [02. Layered Architecture](02-layered-architecture.md): Layered dependency structure (`AppWeb` $\rightarrow$ `BLL` $\rightarrow$ `DAL` $\rightarrow$ `Entity`, wired via `IOC`), `DbContext` strategy, and repository boundaries.
- [05. Domain Rules](05-domain-rules.md): Business logic constraints, state transition rules, and validation responsibilities enforced by the `BLL` layer.
- [07. Database Model](07-database-model.md): SQL Server relational schema (`EcoInstalador`), core entities, and foreign-key relationships.
- [08. API Contract](08-api-contract.md): REST API endpoint contracts (`/api/*`), DTO conventions, and HTTP status semantics.
- [10. Auth API Design](10-auth-api-design.md): Dual authentication architecture (MVC Cookies + REST JWT Bearer), `ApiAuthenticated` / `ApiAdminOnly` policies, and canonical role rules (`Administrador`, `Técnico`).
- [Frontend SPA Documentation (`EcoInstalacion.Frontend`)](../EcoInstalacion.Frontend/README.md): Setup, role permission matrix, and component architecture for the React 19 + Vite 8 + TypeScript 6 administrative client ([`../EcoInstalacion.Frontend`](../EcoInstalacion.Frontend)).

## 2. Architecture Decisions & Roadmaps

- [Target Architecture — Progressive Modernization (`architecture/target-architecture.md`)](architecture/target-architecture.md): Architectural strategy for coexisting ASP.NET Core MVC/Razor views and the React + Vite SPA over a shared REST API and layered backend.
- [06. Deployment Roadmap](06-deployment-roadmap.md): Target hosting options, configuration hierarchy, secret management, and pre-production checklist.
- [09. Mobile Readiness](09-mobile-readiness.md): Architectural preparation for a planned React Native / Expo field technician client.
- [11. Technical Debt Register](11-technical-debt-register.md): Prioritized register of resolved, partial, and pending technical debt items across security, API, frontend, testing, and deployment.

## 3. Historical Audits & Refactoring Records

- [03. Refactoring History](03-refactor-history.md): Record of completed refactoring sprints, compiler warning elimination, `DbContext` consolidation, and repository extraction.
- [04. BLL/DAL Architecture Audit](04-bll-dal-audit.md): Historical audit documenting legacy `DbContext` duplication and generic repository leakage resolved during earlier sprints.

---

## Current Baseline Status (`3a0e343e94ef4712fb114d58907f358d386e1a65`)

- **Backend Framework**: .NET 8 (`C# 12`)
- **Dual Presentation Architecture**:
  - **ASP.NET Core MVC / Razor (`EcoInstalacion.AppWeb`)**: Legacy server-rendered views with Cookie authentication, Antiforgery validation, user administration (`Usuarios`), and QuestPDF report export (`Reportes`).
  - **React Administrative SPA (`EcoInstalacion.Frontend`)**: React 19 + TypeScript 6 + Vite 8 client consuming `/api/*` endpoints with in-memory JWT Bearer authentication.
- **REST API & Security**: JWT Bearer authentication (`POST /api/auth/login`), role-based authorization policies (`ApiAuthenticated` and `ApiAdminOnly`) enforcing the closed role catalog (`Administrador`, `Técnico`), and Swagger/OpenAPI enabled in `Development`.
- **Server-Side Search & Pagination**: Published and active across all four operational modules (`/api/clientes/paginados`, `/api/clientes/administracion`, `/api/tecnicos/paginados`, `/api/solicitudes/paginadas`, `/api/visitas-tecnicas/paginadas`) and integrated into the React SPA with `300 ms` debounced search.
- **Data Persistence**: Entity Framework Core 8 & SQL Server (`EcoInstaladorContext` consolidated in `EcoInstalacion.DAL` with dedicated domain repositories). Database DDL, triggers (`dbo.tr_ActualizarEstadoSolicitud`), and reporting stored procedures are not yet versioned as public migrations or SQL bootstrap scripts.
- **Verified xUnit Test Suite ([`tests/EcoInstalacion.BLL.Tests`](../tests/EcoInstalacion.BLL.Tests))**:
  - **Isolated Baseline Verification (`ECO-PORT-01-REV1`, executed on `2026-11-11`)**: **26 declared test methods** (`18` `[Fact]` + `8` `[Theory]`) expanding into **49 discovered xUnit test cases** (`49` passed, `0` failed, `0` skipped, `Exit Code 0`, `Release` configuration) covering [`OperationalAlertPolicy`](../EcoInstalacion.BLL/Implementacion/OperationalAlertPolicy.cs) (`20` request cases + `9` visit cases) and [`ElSalvadorOperationalClock`](../EcoInstalacion.BLL/Implementacion/ElSalvadorOperationalClock.cs) / `OperationalTemporalOptions` (`20` cases).

---

## Documentation Alignment & Version Notice (`ECO-PORT-02` Scope)

> **Baseline Authority Notice**: Earlier documents in this folder preserve the historical progression of previous engineering increments and may describe intermediate states of the project. Whenever a historical document differs from the current implementation, **the source code at public baseline commit `3a0e343e94ef4712fb114d58907f358d386e1a65` is the authoritative reference**.
>
> Specifically, the following documents contain sections drafted prior to the completion of server-side pagination, `Solicitudes` / `Visitas Técnicas` REST endpoints, and the `EcoInstalacion.BLL.Tests` suite, and are scheduled for targeted revision in **`ECO-PORT-02`**:
> - [`07-database-model.md`](07-database-model.md) (lists `SolicitudesInstalacion` REST exposure as a next step, whereas `/api/solicitudes` and `/api/visitas-tecnicas` are now implemented).
> - [`08-api-contract.md`](08-api-contract.md) (documents the base CRUD endpoints but does not yet list the `/paginados` and `/paginadas` query endpoints).
> - [`11-technical-debt-register.md`](11-technical-debt-register.md) and [`../EcoInstalacion.Frontend/README.md`](../EcoInstalacion.Frontend/README.md) (still list server-side pagination and search `DT-08` / `DT-09` as pending, and do not yet reference the BLL xUnit test suite).
