# Hospital Management Platform

A study project: a microservices-based hospital management system built with .NET and Python. It uses synthetic data only and is not intended for real clinical use.

## Architecture

| Folder | Technology | Purpose |
|---|---|---|
| `src/gateway` | .NET (YARP) | Single entry point, JWT validation, routing |
| `src/identity-service` | .NET | Staff accounts, roles, tokens |
| `src/patient-appointment-service` | .NET | Patients, doctors, appointments |
| `src/pharmacy-billing-service` | .NET | Medicines, stock, prescriptions, invoices |
| `src/ai-service` | Python (FastAPI) | Draft-only documentation assistant |
| `src/analytics-service` | Python (FastAPI, pandas) | Operations analytics and dashboards |
| `src/web` | Angular or React | Web application |
| `src/mobile` | .NET MAUI | Optional mobile application |

## Data Storage (polyglot persistence)

Each service owns its data and uses the store that best fits its workload:

| Service | Store | Database |
|---|---|---|
| Identity | SQL Server | `HPIdentityDb` |
| Pharmacy & Billing | SQL Server | `HPPharmacyDb` |
| Patient & Appointment | PostgreSQL | `HPClinicalDb` |
| Analytics | PostgreSQL | `HPAnalyticsDb` |
| AI | MongoDB | `HPAiDb` |
| Shared cache | Redis | — |

- **Local:** SQL Server databases are created by `infra/sql/sqlserver-init.sql`; PostgreSQL
  databases by `infra/sql/postgres-init.sql`; MongoDB and Redis run as containers. See
  [infra/README.md](infra/README.md).
- **Cloud:** managed free tiers (e.g., Azure SQL for SQL Server, Neon/Supabase for
  PostgreSQL, MongoDB Atlas, Upstash Redis).

## Rules

- Services never query another service's database. They communicate only through APIs.
- Use synthetic data only. Never store real patient information.
- Never commit secrets. Use user secrets or environment variables.

## Getting Started

Prerequisites: .NET SDK 10 or later, Python 3.11 or later, Docker Desktop, Git.

Start the local infrastructure (SQL Server, Redis, RabbitMQ) and create the databases:

```powershell
docker compose -f infra/docker-compose.yml up -d
```

See [infra/README.md](infra/README.md) for ports, credentials, and connection strings.
Further per-service setup instructions will be added as each service is built.

## Documentation

- [docs/enterprise-app-plan.md](docs/enterprise-app-plan.md) — the full project plan
  (architecture, microservices, roadmap, deployment).
- [docs/project-structure-and-standards.md](docs/project-structure-and-standards.md) —
  project structure, dependency flow, design patterns, SOLID principles, versions, and the
  coding/naming standards.
- [docs/build-steps.md](docs/build-steps.md) — prerequisites, infrastructure startup, and
  build/test/run commands.

## Coding Standards

Formatting and naming rules are enforced automatically by the root
[`.editorconfig`](.editorconfig), which encodes the conventions documented in
[docs/project-structure-and-standards.md](docs/project-structure-and-standards.md).
