# Project Overview

## Description
**Electrical Solar Service Management / EcoInstalacion** is a field service management system for residential electrical and solar installation services built with .NET 8, EF Core, and SQL Server.

The system is designed to streamline operations for technician assignments, customer intake, and field installation requests, moving away from legacy clinical terminology toward a professional field service domain.

---

## Purpose
The primary objective of the system is to manage the lifecycle of residential electrical and solar installation services. It coordinates:
1. **Customer Intake**: Keeping track of clients requiring technical installations.
2. **Service Requests**: Handling installation applications for both solar and conventional electrical grids.
3. **Technician Scheduling**: Assigning qualified technicians to open requests.
4. **Technical Visits**: Tracking site evaluations, field work progress, and manager approvals.

---

## Business Domain Mapping
To maintain high credibility and domain precision, the project uses the following domain boundaries:
- **Customers (`Clientes`)**: Homeowners requesting electrical or solar installations.
- **Service Requests (`SolicitudesInstalacion`)**: Applications specifying the type of installation (Solar vs. Conventional) and description of the requirements.
- **Technicians (`Tecnicos`)**: Specialized field engineers assigned to carry out work.
- **Technical Visits (`VisitasTecnica`)**: Logs of site evaluations, diagnostics, progress checks, and approvals.

---

## Main Modules
The application is structured into the following operational modules:
1. **Users & Roles**: Role-based access control supporting *Administrador* and *Técnico*.
2. **Customers Module**: Management of residential clients, contact information, and state (active/inactive).
3. **Technicians Module**: Management of field personnel, contact details, and technical specialties.
4. **Service Requests**: Intake, technician assignment, status tracking, and details.
5. **Technical Visits**: Logging site visits, specifying remarks, and tracking administrator approval status.
6. **Reports**: Basic PDF/Excel export modules for technical assignments and operational metrics.

---

## Current Technical Status
- **Build Quality**: 100% clean compilation, `0 warnings`, `0 errors`.
- **Repository Hygiene**: Aligned `.gitignore` preventing build artifacts (`bin/`, `obj/`) and local configs from committing.
- **Example Config**: `appsettings.Example.json` provided to safely document configuration templates.
- **Consolidation**: Legacy DbContext elements removed from the Entity project, centralizing persistence setup.
- **Decoupling**: `SolicitudService` completely decoupled from direct Entity Framework Core dependencies using a dedicated repository implementation.

---

## Portfolio Positioning
This project is configured as a **professional portfolio MVP** highlighting clean code principles, architectural layering, decoupling of database concerns, and precise business domain modelling. It demonstrates:
- Practical database structure with Entity Framework Core.
- Service-based business logic orchestration.
- Modern C# .NET 8 practices.
