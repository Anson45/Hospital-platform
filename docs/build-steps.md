# Build & Run Steps

Step-by-step guide to build and run the Hospital Platform locally. See
[docs/project-structure-and-standards.md](./project-structure-and-standards.md) for
conventions and [infra/README.md](../infra/README.md) for infrastructure details.

> Study project: use synthetic data only. Never store real patient information.

---

## 1. Prerequisites

| Tool | Version | Check |
|---|---|---|
| .NET SDK | 10.0.401+ | `dotnet --version` |
| Docker Desktop | latest | `docker --version` |
| Git | latest | `git --version` |
| Python (later, for AI/Analytics) | 3.11+ | `python --version` |

Give Docker at least **8 GB of RAM**.

All commands below are run from the repository root in **PowerShell**:
`C:\Users\193369\source\Hospital-platform\`.

---

## 2. Clone the repository

```powershell
git clone https://github.com/Anson45/Hospital-platform.git
cd Hospital-platform
```

---

## 3. Start local infrastructure

Starts SQL Server, PostgreSQL, MongoDB, Redis, and RabbitMQ, and creates the databases on
first run.

```powershell
docker compose -f infra/docker-compose.yml up -d
docker compose -f infra/docker-compose.yml ps        # verify all services are healthy
```

Storage per service (polyglot persistence):

| Service | Store | Database |
|---|---|---|
| Identity | SQL Server | `HPIdentityDb` |
| Pharmacy & Billing | SQL Server | `HPPharmacyDb` |
| Patient & Appointment | PostgreSQL | `HPClinicalDb` |
| Analytics | PostgreSQL | `HPAnalyticsDb` |
| AI | MongoDB | `HPAiDb` |

Stop infrastructure when done:

```powershell
docker compose -f infra/docker-compose.yml down       # keep data
docker compose -f infra/docker-compose.yml down -v    # delete data volumes
```

---

## 4. Restore & build

Each service is an independent solution (`.slnx`). Repo-wide settings come from
`Directory.Build.props`; package versions from `Directory.Packages.props`.

Build a single service:

```powershell
dotnet build src/identity-service/Identity.slnx
dotnet build src/gateway/Gateway.slnx
dotnet build src/patient-appointment-service/PatientAppointment.slnx
dotnet build src/pharmacy-billing-service/PharmacyBilling.slnx
```

Build all .NET services:

```powershell
"gateway/Gateway.slnx",
"identity-service/Identity.slnx",
"patient-appointment-service/PatientAppointment.slnx",
"pharmacy-billing-service/PharmacyBilling.slnx" |
  ForEach-Object { dotnet build "src/$_" }
```

`dotnet build` restores NuGet packages automatically. To restore explicitly, run
`dotnet restore <solution>` first.

---

## 5. Run tests

```powershell
dotnet test src/patient-appointment-service/PatientAppointment.slnx
```

All services:

```powershell
"gateway/Gateway.slnx",
"identity-service/Identity.slnx",
"patient-appointment-service/PatientAppointment.slnx",
"pharmacy-billing-service/PharmacyBilling.slnx" |
  ForEach-Object { dotnet test "src/$_" }
```

---

## 6. Run a service

```powershell
dotnet run --project src/identity-service/Identity.Api
```

The console prints the listening URL (e.g., `http://localhost:5xxx`). Each service exposes
a `/health` endpoint. Configure connection strings via `dotnet user-secrets` or environment
variables (see [infra/README.md](../infra/README.md)) — never commit secrets.

To run behind the gateway, start the gateway and the target services, then call the
gateway URL.

---

## 7. Database migrations (per service, once EF Core is added)

Install the EF Core CLI once:

```powershell
dotnet tool install --global dotnet-ef
```

Add and apply a migration (run from the service's solution folder):

```powershell
# SQL Server services (Identity, Pharmacy/Billing) and PostgreSQL services
dotnet ef migrations add InitialCreate --project <Infrastructure> --startup-project <Api>
dotnet ef database update --project <Infrastructure> --startup-project <Api>
```

Python services (AI/Analytics) use Alembic — covered when those services are built.

---

## 8. Common troubleshooting

| Symptom | Fix |
|---|---|
| Containers unhealthy / won't start | Ensure Docker has 8 GB RAM; check `docker compose ... logs` |
| SQL Server login fails | Wait for the health check; password is `Your_strong_Passw0rd!` |
| Port already in use (1433/5432/27017/6379/5672/15672) | Stop the conflicting local service or change the host port in `infra/docker-compose.yml` |
| Build warnings become errors | `TreatWarningsAsErrors` is on only when `CI=true`; local builds are not blocked |
| Databases missing after re-up | Init scripts run only on an **empty** data volume; run `down -v` then `up -d` to recreate |

---

## 9. Validation checklist (before committing)

- [ ] Solution builds with **0 errors** and no new warnings
- [ ] All affected tests pass
- [ ] No secrets, connection strings, or real data committed
- [ ] Changes follow [the engineering standards](./project-structure-and-standards.md)




## 10. Completed so far
•	✅ Four .NET solutions scaffolded (Clean Architecture + references)
•	✅ Engineering standards, SOLID, .editorconfig
•	✅ Repo-wide build config (Directory.Build.props + central package management)
•	✅ Local infrastructure (SQL Server, PostgreSQL, MongoDB, Redis, RabbitMQ) + DB init
•	✅ Polyglot persistence decided and documented
•	✅ Build steps guide
Recommended next steps (in order)
1. Add NuGet packages per service (quick foundation)
•	Gateway: Yarp.ReverseProxy
•	Identity: EF Core + Microsoft.EntityFrameworkCore.SqlServer, ASP.NET Identity, JWT
•	Patient/Appointment: EF Core + Npgsql.EntityFrameworkCore.PostgreSQL
•	Pharmacy/Billing: EF Core + SqlServer, MassTransit, Polly
•	Shared: Serilog
•	All versions go in Directory.Packages.props.
2. Build the Identity Service (the real first feature — everything depends on auth)
•	ASP.NET Core Identity on SQL Server (HPIdentityDb)
•	Local register/login, JWT access + refresh tokens
•	Roles: Admin, Doctor, Nurse, Receptionist, Pharmacist, Billing
•	/health endpoint + EF Core migration
3. Gateway with YARP — routing + JWT validation so services sit behind one entry point.
My recommendation
Start with the Identity Service (option 2) since it's the authentication foundation for every other service, adding its NuGet packages as part of that work. This follows the plan's roadmap (weeks 2–3).
Want me to start building the Identity Service? I'll set up:
•	EF Core + ASP.NET Identity on SQL Server
•	JWT token generation (access + refresh)
•	Roles seeding
•	A health endpoint and the first migration
Or if you'd prefer, I can do option 1 (add all NuGet packages) across every service first. Which do you want?