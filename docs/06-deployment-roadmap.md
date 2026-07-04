# Deployment Roadmap

This roadmap outlines the recommended steps, requirements, and best practices to safely move the **Electrical Solar Service Management / EcoInstalacion** application from a local development environment to a production environment.

---

## 1. Hosting Infrastructure

### Presentation Layer (`EcoInstalacion.AppWeb`)
- **Option A (IIS / Virtual Machine)**: Deploy as an ASP.NET Core self-hosted application behind IIS on a Windows Server VM, or on a Linux VM using Nginx as a reverse proxy.
- **Option B (Azure App Services)**: Deploy as a platform-as-a-service (PaaS) web app. This simplifies certificate handling and scaling.

### Database Layer (`SQL Server`)
- **Development**: Local SQL Server Express instance (`DESKTOP-KQCB74E\SQLEXPRESS`).
- **Production**: Azure SQL Database or a dedicated SQL Server Standard instance.

---

## 2. Configuration & Secrets Management

To maintain security and prevent credential leakage, configurations must be handled using the ASP.NET Core configuration hierarchy.

### App Configuration Steps:
1. **Remove Local Secrets**: Never commit production connection strings to `appsettings.json`.
2. **Environment Variables**: Use environment variables to override settings at runtime. In Azure or IIS, set `ConnectionStrings__CadenaSql` to the production database connection string.
3. **Template Verification**: Ensure `appsettings.Example.json` contains all the keys required by the application so that deployment engineers can prepare their environment files correctly.

---

## 3. Database Migration Strategy

Currently, database schema structures are managed through SQL Server constraints and direct DB mappings.

### Steps for Schema Setup:
1. **Schema Dumps**: Generate a baseline SQL schema script from the development SQL Server database.
2. **Execution**: Run the schema creation script on the production database server before launching the application.
3. **Audit Constraints**: Double check that CHECK constraints (such as the one restricting `TipoInstalacion` to `'Solar'` or `'Convencional'`) are applied on the production tables to guarantee data consistency.

---

## 4. Pre-Production Checklist

Prior to final release, the following checks must be completed:

- [ ] **Clean Build**: Verify project builds with zero warnings and zero errors (`dotnet build -c Release`).
- [ ] **Config Check**: Ensure `ASPNETCORE_ENVIRONMENT` is set to `Production` on the server so that detailed error pages are disabled for end-users.
- [ ] **HTTPS Enforced**: Enable HTTPS redirect middleware in `Program.cs` and configure SSL certificates.
- [ ] **Cookie Security**: Ensure session and authentication cookies are configured with `SecurePolicy = CookieSecurePolicy.Always` and `HttpOnly = true`.
- [ ] **DI Verification**: Verify that all services register correctly at startup without circular references (resolved during the DbContext DI refactoring).
