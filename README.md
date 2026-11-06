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

## Database

- **Local:** one SQL Server instance with five databases (`IdentityDb`, `ClinicalDb`, `PharmacyDb`, `AiDb`, `AnalyticsDb`), created by `infra/sql/init-databases.sql`.
- **Cloud:** one Azure SQL database with a separate schema per service.

## Rules

- Services never query another service's database. They communicate only through APIs.
- Use synthetic data only. Never store real patient information.
- Never commit secrets. Use user secrets or environment variables.

## Getting Started

Prerequisites: .NET SDK 8 or later, Python 3.11 or later, Docker Desktop, Git.

Setup instructions will be added as each service is created.

## Documentation

- [docs/enterprise-app-plan.md](docs/enterprise-app-plan.md) — the full project plan
  (architecture, microservices, roadmap, deployment).
- [docs/project-structure-and-standards.md](docs/project-structure-and-standards.md) —
  project structure, dependency flow, design patterns, SOLID principles, versions, and the
  coding/naming standards.

## Coding Standards

Formatting and naming rules are enforced automatically by the root
[`.editorconfig`](.editorconfig), which encodes the conventions documented in
[docs/project-structure-and-standards.md](docs/project-structure-and-standards.md).
