# EcoInstalacion Documentation

This folder contains the technical documentation for the **Electrical Solar Service Management / EcoInstalacion** project. It serves as a guide for understanding the project overview, layered architecture, refactoring history, business domain audits, and deployment roadmap.

## Reference Guides

- [01. Project Overview](01-project-overview.md): High-level system purpose, business domain scope, modules, and portfolio positioning.
- [02. Layered Architecture](02-layered-architecture.md): Description of the layered architecture, dependencies, DbContext strategy, repository patterns, and architectural rules.
- [03. Refactoring History](03-refactor-history.md): Technical summary of completed refactoring sprints, including warnings resolution, DbContext consolidation, and repository extraction.
- [04. BLL/DAL Architecture Audit](04-bll-dal-audit.md): Deep-dive audit highlighting legacy architecture issues, DbContext duplication, and generic repository leakage.
- [05. Domain Rules](05-domain-rules.md): Separation of concerns and domain business logic constraints enforced by the BLL layer.
- [06. Deployment Roadmap](06-deployment-roadmap.md): Planned infrastructure, database migrations, configuration handling, and steps for production readiness.
- [07. Database Model](07-database-model.md): Description of the SQL Server database schema, tables, relationships, and data types.
- [08. API Contract](08-api-contract.md): REST API endpoints contract definition, DTO structures, and conventions for frontend integration.
- [09. Target Architecture](architecture/target-architecture.md): Architectural roadmap and progressive modernization strategy toward React + Vite.
- [10. Mobile Readiness](09-mobile-readiness.md): Preparation for React Native / Expo mobile client for field technicians.
- [11. Auth API Design](10-auth-api-design.md): Planning and design of the security, authentication, and authorization layer for API endpoints.
- [12. Technical Debt Register](11-technical-debt-register.md): Systematic register cataloging technical debt items, risks, priorities, and roadmap for reduction.

---

## Current Status

- **Framework**: .NET 8
- **UI & Presentation**: ASP.NET Core MVC / Razor
- **Data Persistence**: Entity Framework Core & SQL Server
- **Architecture**: Layered Architecture (`AppWeb` -> `BLL` -> `DAL` -> `Entity`)
- **Compilation Status**: Clean build with `0 warnings` and `0 errors`
- **DbContext Consolidation**: Complete (single DbContext active in DAL, registered solely in `Program.cs`, legacy context deleted from Entity)
- **Repository Extracted**: Complete (`SolicitudRepository` created and integrated with `SolicitudService`, resolving EF Core leakage in that service)
