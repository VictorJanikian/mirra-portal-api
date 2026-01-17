# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run Commands

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the API (development mode)
dotnet run --project Mirra.Portal.API

# Publish for deployment (Linux)
dotnet publish -c Release -r linux-x64
```

## Architecture Overview

This is a .NET 9 ASP.NET Core Web API for the Mirra Portal, using a layered architecture.

### Project Structure

- **Controllers/** - API endpoints (`AccountController`, `ConfigurationController`)
- **Services/** - Business logic layer with interfaces in `Services/Interfaces/`
- **Database/** - Data access layer
  - `DatabaseContext.cs` - EF Core context using SQL Server
  - `DBEntities/` - Database table row entities (suffixed with `TableRow`)
  - `Repositories/` - Repository pattern implementation with interfaces
  - `Mapper/` - AutoMapper profiles for entity-to-model mapping
- **Model/** - Domain models and DTOs
  - `Requests/` - API request DTOs
  - `Responses/` - API response DTOs
- **Middleware/** - Custom middleware (`IdentityMiddleware`, `LoggingMiddleware`)
- **Security/** - JWT authentication configuration
- **Helper/** - Utility classes (`IdentityHelper`, `SymmetricEncryptionHelper`)
- **Exceptions/** - Custom exceptions (`BadRequestException`, `NotFoundException`, `UnauthorizedException`)
- **Integration/** - External service integrations (Azure Email)

### Key Patterns

- **Repository Pattern**: All database access goes through repositories with interfaces
- **AutoMapper**: Used for mapping between TableRow entities, domain models, and request/response DTOs
- **JWT Bearer Authentication**: Protected endpoints use `[Authorize("Bearer")]` attribute
- **Custom Exception Handling**: Controllers catch custom exceptions and return appropriate HTTP status codes

### Database

- Uses Entity Framework Core with SQL Server
- Connection string configured in `appsettings.json` under `ConnectionStrings:DefaultConnection`
- Entity classes use `TableRow` suffix (e.g., `CustomerTableRow`, `SchedulingTableRow`)

### Authentication Flow

1. User registers via `/api/Account/register`
2. Email activation via `/api/Account/activate`
3. Login via `/api/Account/login` returns JWT token
4. Protected endpoints require `Authorization: Bearer <token>` header

### Configuration Management

The `ConfigurationController` handles platform configurations and schedulings:
- Schedulings use cron expressions (5-field standard format) for intervals
- Configurations are scoped to the authenticated customer via `IdentityHelper.UserId()`
