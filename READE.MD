# CropShield AI — Backend

> **ASP.NET Core Web API (.NET 10)** | Crop Insurance & Farmer Risk Intelligence Platform

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core Web API (.NET 10) |
| ORM | Entity Framework Core 10 + SQL Server |
| Auth | JWT Bearer + Refresh Tokens (BCrypt) |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Logging | Serilog (Console + File sinks) |
| Rate Limiting | AspNetCoreRateLimit |
| AI Integration | Google Gemini API (`gemini-2.0-flash`) |
| Testing | Postman |
| Containerization | Docker (Linux target) |

---

## Architecture

Clean Architecture with Repository Pattern and Service Layer.

```
Backend_Crop_Insurrance/
├── Controllers/          # API endpoints, route definitions, role authorization
├── Core/
│   ├── Entities/         # Domain models (EF Core entities)
│   ├── Interfaces/       # Repository & service contracts
│   ├── DTOs/             # Request/Response data transfer objects
│   ├── Validators/       # FluentValidation rule classes
│   └── Common/           # Result<T>, enums, constants
├── Infrastructure/
│   ├── Data/             # AppDbContext, EF Core configurations
│   ├── Repositories/     # IRepository implementations
│   └── Services/         # Business logic service implementations
├── Extensions/           # Service registration, middleware extensions
├── Middleware/           # Exception handler, request logging
└── Program.cs            # App entry point, DI composition root
```

---

## Modules

### ✅ Auth
- Register, Login, Logout
- JWT access token + HTTP-only refresh token cookie
- BCrypt password hashing
- Role-based claims: `Farmer`, `Agent`, `Admin`

### ✅ Farmer
- Profile CRUD (land area, crop type, location, bank details)
- AI Crop Risk Summary — `POST /api/farmer/ai-risk-summary`
  - Gemini `gemini-2.0-flash` integration
  - Returns structured risk analysis with actionable recommendations

### ✅ Agent
- Profile management
- Farmer assignment handling

### ✅ Admin
- User management
- System-wide oversight

### 🔄 Insurance Agent Module *(In Progress)*
**Phase 1 Complete — 12 entities designed & configured:**

| Entity | Purpose |
|---|---|
| `PolicyType` | Insurance product definitions |
| `InsurancePolicy` | Active farmer policies |
| `PremiumCalculation` | Audit-transparent premium records |
| `RiskAssessment` | Per-farmer risk scoring |
| `Claim` | Claim lifecycle management |
| `ClaimDocument` | Uploaded claim evidence |
| `InspectionReport` | Field inspection outcomes |
| `AgentFarmerAssignment` | Agent ↔ Farmer mappings |
| `ClaimStatusHistory` | Append-only status audit trail |
| `AgentActivityLog` | Append-only agent action log |
| `Notification` | In-app notification system |
| `BaseEntity` | Shared audit fields (CreatedAt, UpdatedAt, IsDeleted) |

**Phase 2 Planned:**
- Repository interfaces & implementations
- Service layer with full business logic
- Controllers with role-based authorization
- DTOs, validators, AutoMapper profiles
- Full API surface: agent dashboard stats, policy creation/approval, claim handling, premium calculation, risk assessment, document upload, activity history, reports, search/filtering

---

## Key Design Decisions

**Enum persistence** — stored as strings in SQL for readability in raw queries and reporting.

**Financial integrity** — `OnDelete(DeleteBehavior.Restrict)` throughout; no cascading deletes. `PremiumCalculation` is a separate table for full audit transparency.

**Audit tables** — `ClaimStatusHistory` and `AgentActivityLog` are append-only and do **not** inherit soft-delete from `BaseEntity`.

**Naming** — Custom claim entity named `AppClaim` (not `Claim`) to avoid ambiguous reference with `System.Security.Claims`.

**Result pattern** — `Result<T>` used across service layer for explicit success/failure without exceptions for expected cases.

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server (local or Docker)
- Docker Desktop *(optional)*

### Configuration

`appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CropShieldDb;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "CropShieldAPI",
    "Audience": "CropShieldClient",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "GeminiApi": {
    "ApiKey": "your-gemini-api-key",
    "Model": "gemini-2.0-flash"
  }
}
```

### Run Locally

```bash
# Restore packages
dotnet restore

# Apply migrations
dotnet ef database update

# Run
dotnet run
```

### Docker

```bash
docker-compose up --build
```

---

## Database Migrations

```bash
# Add new migration
dotnet ef migrations add <MigrationName>

# Apply to database
dotnet ef database update

# Revert last migration
dotnet ef migrations remove
```

---

## API Testing

All endpoints tested via **Postman**. No Swagger UI (removed from production build).

### Auth Flow
```
POST /api/auth/register
POST /api/auth/login        → returns access token + sets refresh cookie
POST /api/auth/refresh      → requires refresh cookie
POST /api/auth/logout
```

### AI Risk Summary
```
POST /api/farmer/ai-risk-summary
Authorization: Bearer <token>
Content-Type: application/json

{
  "cropType": "Cotton",
  "landAreaAcres": 5.5,
  "location": "Vidarbha, Maharashtra",
  "soilType": "Black Cotton Soil",
  "irrigationType": "Rainfed"
}
```

---

## Logging

Serilog writes to:
- **Console** — structured output during development
- **File** — rolling daily logs at `logs/cropshield-.log`

Log level configured per environment in `appsettings.json`.

---

## NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.8 | SQL Server provider |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.8 | Migrations CLI |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.8 | JWT auth middleware |
| `System.IdentityModel.Tokens.Jwt` | 8.18.0 | Token generation/validation |
| `BCrypt.Net-Next` | 4.2.0 | Password hashing |
| `AutoMapper` | 13.0.1 | DTO ↔ Entity mapping |
| `FluentValidation.AspNetCore` | 11.3.1 | Request validation |
| `FluentValidation.DependencyInjectionExtensions` | 12.1.1 | Validator DI registration |
| `Serilog.AspNetCore` | 10.0.0 | Structured logging |
| `Serilog.Sinks.Console` | 6.1.1 | Console log sink |
| `Serilog.Sinks.File` | 7.0.0 | File log sink |
| `AspNetCoreRateLimit` | 5.0.0 | IP/client rate limiting |

---

## Role Authorization

| Role | Access |
|---|---|
| `Farmer` | Own profile, AI risk summary, policy view, claim submission |
| `Agent` | Assigned farmers, policy creation/approval, claim inspection, reports |
| `Admin` | Full system access, user management, settings |

---

## Notes

- Date serialization: use `dayjs().format("YYYY-MM-DDTHH:mm:ss")` on the frontend — **not** `toISOString()` — to avoid UTC/IST shift issues.
- Gemini API: set `maxOutputTokens` ≥ 2048; use start/end index JSON extraction to handle partial responses.
- `withCredentials: true` required on all Axios requests for refresh token cookie flow.