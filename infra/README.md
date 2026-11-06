# Local Infrastructure

Docker Compose stack for local development using **polyglot persistence**:
**SQL Server**, **PostgreSQL**, **MongoDB**, **Redis**, and **RabbitMQ**.
On first start, the service databases are created automatically.

> These credentials are for **local development only**. Never use them in the cloud.
> Keep real secrets out of source control (use `dotnet user-secrets` / environment vars).

## Start / stop

From the repository root:

```powershell
docker compose -f infra/docker-compose.yml up -d        # start
docker compose -f infra/docker-compose.yml ps           # status
docker compose -f infra/docker-compose.yml logs -f      # logs
docker compose -f infra/docker-compose.yml down         # stop (keep data)
docker compose -f infra/docker-compose.yml down -v      # stop and delete volumes
```

Give Docker at least **8 GB of RAM** for this setup.

## Services & ports

| Service | Host port | Notes |
|---|---|---|
| SQL Server | `1433` | `sa` / `Your_strong_Passw0rd!` |
| PostgreSQL | `5432` | `hp_admin` / `Your_strong_Passw0rd!` |
| MongoDB | `27017` | `hp_admin` / `Your_strong_Passw0rd!` (AI service) |
| Redis | `6379` | no password locally |
| RabbitMQ (AMQP) | `5672` | `guest` / `guest` |
| RabbitMQ (Management UI) | `15672` | http://localhost:15672 |

## Storage per service (polyglot persistence)

| Service | Store | Database |
|---|---|---|
| Identity | SQL Server | `HPIdentityDb` |
| PharmacyBilling | SQL Server | `HPPharmacyDb` |
| PatientAppointment | PostgreSQL | `HPClinicalDb` |
| Analytics | PostgreSQL | `HPAnalyticsDb` |
| AI | MongoDB | `HPAiDb` |

- SQL Server databases are created by [`sql/sqlserver-init.sql`](./sql/sqlserver-init.sql)
  (via the one-shot `mssql-init` container).
- PostgreSQL databases are created by [`sql/postgres-init.sql`](./sql/postgres-init.sql).
- MongoDB creates `HPAiDb` lazily on first write.

## Connection strings (local)

| Service | Connection string |
|---|---|
| Identity | `Server=localhost,1433;Database=HPIdentityDb;User Id=sa;Password=Your_strong_Passw0rd!;TrustServerCertificate=True` |
| PharmacyBilling | `Server=localhost,1433;Database=HPPharmacyDb;User Id=sa;Password=Your_strong_Passw0rd!;TrustServerCertificate=True` |
| PatientAppointment | `Host=localhost;Port=5432;Database=HPClinicalDb;Username=hp_admin;Password=Your_strong_Passw0rd!` |
| Analytics | `Host=localhost;Port=5432;Database=HPAnalyticsDb;Username=hp_admin;Password=Your_strong_Passw0rd!` |
| AI (MongoDB) | `mongodb://hp_admin:Your_strong_Passw0rd!@localhost:27017/HPAiDb?authSource=admin` |

- Redis: `localhost:6379`
- RabbitMQ: `amqp://guest:guest@localhost:5672`

> Each service reads/writes **only its own** database. No cross-database queries —
> integrate via gRPC, REST, or events.
