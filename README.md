# Electrical Solar Service Management / EcoInstalacion

Field service management system for residential electrical and solar installation services built with .NET 8, EF Core and SQL Server.

## Overview

EcoInstalacion is a comprehensive field service management platform designed to streamline operations for residential electrical and solar installation providers. The system coordinates service requests, schedules onsite technical visits, manages technician assignments, and tracks operational workflows to bridge the gap between customers and field engineering teams.

## Domain

- **Residential Electrical Installation Services**: Management of residential wiring, service panel upgrades, and basic electrical systems.
- **Solar Installation Services**: Workflows for residential photovoltaic (PV) panel setup, inverter configurations, and battery storage system installations.
- **Customer Management**: Profile and contract tracking for homeowners and residents.
- **Service Requests**: Intake, classification, and priority handling for client installation requests.
- **Technical Visits**: Detailed scheduling, dispatch, tracking, and onsite service reporting.
- **Operational Service Tracking**: Lifecycle tracking of field tickets from creation to final quality validation.

## Main Features

- **Customer Management**: Register and manage customer profiles.
- **User and Role Management**: Roles for Administrators, Coordinators, and Field Technicians.
- **Service Request Management**: Lifecycle tracking of incoming residential installation requests.
- **Technician Management**: Tracking technical skills, availability, and assignments.
- **Technical Visit Scheduling & Tracking**: Dynamic scheduling of onsite visits and status tracking.
- **Operational Reporting**: Basic metrics on completed visits and pending services.

## Tech Stack

- **Framework**: .NET 8
- **UI Engine**: ASP.NET Core MVC / Razor Views
- **Language**: C# 12
- **ORM**: Entity Framework Core
- **Database**: SQL Server
- **Styling**: Bootstrap & Custom CSS

## Solution Structure

The project is structured following a standard Layered Architecture pattern:

- **[EcoInstalacion.AppWeb](file:///d:/UTEC/2025/ProyectoEcoInstalacion/EcoInstalacionWebApp/EcoInstalacion.AppWeb)**: Presentation layer containing MVC controllers, Razor views, and web assets.
- **[EcoInstalacion.BLL](file:///d:/UTEC/2025/ProyectoEcoInstalacion/EcoInstalacionWebApp/EcoInstalacion.BLL)**: Business Logic Layer containing application services and core business rules.
- **[EcoInstalacion.DAL](file:///d:/UTEC/2025/ProyectoEcoInstalacion/EcoInstalacionWebApp/EcoInstalacion.DAL)**: Data Access Layer encapsulating repository implementations, EF Core DbContext, and database migrations.
- **[EcoInstalacion.Entity](file:///d:/UTEC/2025/ProyectoEcoInstalacion/EcoInstalacionWebApp/EcoInstalacion.Entity)**: Shared domain models, data transfer objects, and entities.
- **[EcoInstalacion.IOC](file:///d:/UTEC/2025/ProyectoEcoInstalacion/EcoInstalacionWebApp/EcoInstalacion.IOC)**: Inversion of Control layer responsible for dependency injection registration.

## Technical Documentation

Additional technical documentation is available in the [`docs/`](docs/) folder.

Recommended reading:

- [Project Overview](docs/01-project-overview.md)
- [Layered Architecture](docs/02-layered-architecture.md)
- [Refactor History](docs/03-refactor-history.md)
- [BLL / DAL Architecture Audit](docs/04-bll-dal-audit.md)
- [Domain Rules](docs/05-domain-rules.md)
- [Deployment Roadmap](docs/06-deployment-roadmap.md)

## Local Setup

1. Copy the example configuration file:
   ```bash
   cp EcoInstalacion.AppWeb/appsettings.Example.json EcoInstalacion.AppWeb/appsettings.json
   ```
2. Configure the database connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "CadenaSql": "Server=YOUR_SQL_SERVER;Database=EcoInstalador;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
3. Update database schemas using EF Core migrations or execute the SQL initialization script.

## Build and Run

Restore dependencies, build the solution, and run the web application locally:

```bash
dotnet restore
dotnet build
dotnet run --project EcoInstalacion.AppWeb
```

## Current Technical Status

- **Build Status**: Clean compile (0 warnings, 0 errors).
- **Target SDK**: .NET 8.0 SDK.

## Portfolio Notes

This repository is maintained and modernized as a professional .NET 8 portfolio project demonstrating structured layered architecture, enterprise-grade data handling using Entity Framework Core, and clean presentation using ASP.NET Core MVC.
