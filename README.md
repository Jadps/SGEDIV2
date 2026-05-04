# SGEDI v2 — Dual Education Management System (ITTLA)

> [!IMPORTANT]
> **Project Overview**
> SGEDI v2 is a full rewrite of the Dual Education management platform used at Instituto Tecnológico de Tlalnepantla (ITTLA). The system coordinates the relationship between students, academic coordinators, department heads, and company advisors within the Dual Education modality.
>
> This version was redesigned from the ground up to improve performance, strengthen security, and reduce the technical debt accumulated in legacy implementations.

## Technical Stack
![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=flat&logo=dotnet&logoColor=white)
![FastEndpoints](https://img.shields.io/badge/FastEndpoints-5C2D91?style=flat)
![Minimal APIs](https://img.shields.io/badge/Minimal_APIs-6C757D?style=flat)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-336791?style=flat&logo=postgresql&logoColor=white)
![Entity Framework](https://img.shields.io/badge/EF_Core-68217A?style=flat)
![JWT](https://img.shields.io/badge/JWT-000000?style=flat&logo=jsonwebtokens&logoColor=white)

![Angular](https://img.shields.io/badge/Angular_21-DD0031?style=flat&logo=angular&logoColor=white)
![PrimeNG](https://img.shields.io/badge/PrimeNG_21-009688?style=flat)
![TailwindCSS](https://img.shields.io/badge/TailwindCSS_4-38B2AC?style=flat&logo=tailwindcss&logoColor=white)
![Signals](https://img.shields.io/badge/Angular_Signals-FF6F00?style=flat)

![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)
![Dokploy](https://img.shields.io/badge/Dokploy-000000?style=flat)
![Vercel](https://img.shields.io/badge/Vercel-000000?style=flat&logo=vercel&logoColor=white)
![Serilog](https://img.shields.io/badge/Serilog-FF6F00?style=flat)

### Backend (.NET 10)
- **API Framework:** FastEndpoints and Minimal APIs
- **Architecture:** Vertical Slice Architecture (VSA)
- **Data Access:** Entity Framework Core + PostgreSQL
- **Authentication & Security:**
  - Custom JWT strategy with access, refresh rotation, and secondary auth tokens
  - BCrypt password hashing
- **Patterns & Design:**
  - Result Pattern for standardized error handling
  - Records for immutable DTOs
  - EF projections instead of AutoMapper
- **Cross-cutting Concerns:**
  - Scalar for API documentation
  - Serilog for structured logging
  - Custom audit middleware

### Frontend (Angular 21)
- **State Management:** Angular Signals and `rxResource` for high-performance reactivity
- **UI Library:** PrimeNG 21 with Aura Theme (Glassmorphism aesthetic)
- **Styling:** Tailwind CSS 4
- **Patterns:**
  - Siloed Service Architecture (Decomposed domain-specific services)
  - Interface-driven models
  - Reactive Forms with real-time validation
  - Custom interceptors for JWT rotation

---

## Architecture

### Vertical Slice Architecture (VSA)
SGEDI v2 is organized by feature instead of by technical layer. Each slice contains its own endpoints, business logic, validation, and data access. This reduces coupling and makes the system easier to maintain and extend.

### Service Decomposition (Frontend)
To ensure scalability, the frontend utilizes a siloed service pattern where responsibilities are split into domain-specific providers:
- **ExpedienteService:** Lifecycle of the student file and deadlines.
- **DocumentUploadService:** Secure, role-based file ingestion.
- **CargaAcademicaService:** Management of subjects and academic load.
- **CatalogService:** Centralized source for typed system catalogs.

### Security and Role Model
The platform uses a role-based permission matrix with career-scoped access rules. Coordinators and department heads are limited to their assigned academic areas, while administrators have global access. All entities implement `IHasStatus` for standardized state management (Activo, SinActivar, Borrado).

**Supported roles:**
- Admin
- Alumno
- Profesor
- Coordinador
- Asesor Interno
- Asesor Externo
- Jefe de Departamento

### Document Security Model
The system manages three document silos with jurisdictional enforcement:

- **Templates:** Official Word/PDF templates managed by administrators.
- **Student Files:** Mandatory academic documents (Schedule, Kardex, etc.).
- **Annexes I–VIII:** Role-specific documents governed by deadlines and strict upload permissions.

---

## Features

### Completed
- **Multi-role Authentication:** Secure login and registration with JWT rotation.
- **Expediente Management:** Specialized tracking of student documentation and status.
- **Academic Load (Carga Académica):**
    - Subject and professor assignment module.
    - Business Rule Enforcement: Max 8 subjects and 36 total credits per semester.
    - Duplicate prevention logic (Backend & Frontend).
    - Automatic placeholder generation for Anexos III and VII.
- **Siloed File Management:** Role-specific secure upload endpoints with 5MB limits.
- **Jurisdictional Filters:** Automatic career-scoped data filtering for all administrative roles.
- **Status Management:** Unified entity lifecycle management via `IHasStatus`.
- **Evaluation Management:** Rubric-based evaluation management for advisors.
- **Digital agreements:** Digital agreements with versioning and rejection handling

### Planned

- PDF generation with embedded signatures
---

## Local Development

### Prerequisites
- .NET 10 SDK
- Node.js LTS
- Docker Desktop

### Setup

Start the infrastructure:

```bash
docker-compose up -d
```

### Environment Variables

Create a .env file in the backend root with:

```bash
ConnectionStrings__DefaultConnection=
JwtSettings__Secret=
Frontend_URL=http://localhost:4201
```

### Run the Backend

```bash
dotnet run --project BACKSGEDI.Api
```

### Run the Frontend

```bash
npm install
ng serve 
```

### Deployment

- **Backend and Database:** Managed with Dokploy for CI/CD and container orchestration
- **Frontend:** Hosted on Vercel for edge delivery and availability