# Study Project: Hospital Management System (Microservices)

## 1. Overview

A medium-size study project: a hospital management system built as 5 microservices, a web app, and an optional mobile app. It practices enterprise patterns (microservices, API gateway, event-driven messaging, caching, real-time updates, role-based authentication, AI integration, data analysis, and deployment) at zero cost.

### Important note on data
This is a study project. Use only synthetic (fake) patient data. Never use real patient information. The AI assists with administrative work and documentation only. It does not diagnose or recommend treatment.

### Goals
- Learn microservices with .NET (C#) and Python
- Learn the API gateway, gRPC, messaging, and caching patterns used in enterprise systems
- Apply Clean Architecture and CQRS to the most complex services
- Role-based access control for hospital staff roles
- Local login first, then Google, Microsoft, GitHub, and Facebook
- AI documentation assistant using a free LLM tier
- Real-time dashboard and data analysis for hospital operations
- Deploy to free cloud tiers

### Non-Goals
- Real clinical use or compliance certification (HIPAA, etc.)
- Medical device or lab integration (HL7, FHIR), which can be a future idea
- Kubernetes (use managed container hosting instead)
- Full event sourcing (study CQRS first; event sourcing is a later idea)

---

## 2. Database Strategy

| Environment | Setup |
|---|---|
| Local development | One SQL Server instance with a separate database per service (`IdentityDb`, `ClinicalDb`, `PharmacyDb`, `AiDb`, `AnalyticsDb`) |
| Cloud deployment | One Azure SQL database (free offer) with a separate schema per service (`identity`, `clinical`, `pharmacy`, `ai`, `analytics`) |

Rules that make the switch from separate databases to one database easy:
- Each service only reads and writes its own database or schema.
- No cross-database queries. Services get data from each other through gRPC, REST, or events.
- Each service has its own migrations (EF Core for .NET services, Alembic for Python services).
- Connection strings are set per service through configuration, so the cloud setup only changes the connection string and the schema setting.

---

## 3. Technology Stack

### 3.1 Core stack

| Layer | Technology | Why |
|---|---|---|
| Database | SQL Server (Docker locally, Azure SQL in cloud) | Your requirement |
| Web app | Angular or React (TypeScript) | Free hosting on Azure Static Web Apps or Cloudflare Pages |
| Mobile app | .NET MAUI (optional) | Reuse C# models; run on emulator or sideloaded APK |
| .NET services | ASP.NET Core Web API, EF Core | Your existing strength |
| Python services | FastAPI, SQLAlchemy (pyodbc), pandas, scikit-learn | Learning target |
| Authentication | ASP.NET Core Identity + JWT; OAuth with Google, Microsoft, GitHub, Facebook | All free |
| AI | Google Gemini or Groq free API; Ollama for offline testing | Zero cost |
| Logging | Serilog + Azure Application Insights (free allowance) | Free monitoring |
| Containers | Docker, Docker Compose | Same images locally and in the cloud |
| CI/CD | GitHub Actions | Free minutes |
| Secrets | dotnet user-secrets and .env locally; GitHub Secrets and container app secrets in cloud | No secrets in code |

### 3.2 Added technologies

| Technology | Used for | Where it is used | Local tool | Free cloud option |
|---|---|---|---|---|
| **YARP** | API gateway: routing, load balancing, JWT validation, rate limiting | Gateway service | Docker | Runs as a container on Azure Container Apps |
| **gRPC** | Fast, typed calls between backend services | Billing asks Patient service for visit details; Analytics asks Pharmacy service for stock data | Docker | Container Apps (HTTP/2 ingress; verify setup) |
| **RabbitMQ + MassTransit** | Event-driven messaging. Example: `AppointmentCompleted` event makes Billing create an invoice without a direct call | Patient service (publisher), Billing service (consumer) | Docker | CloudAMQP free plan |
| **Redis** | Caching dashboard data, rate-limit counters, refresh token blacklist | Analytics, Patient, Gateway | Docker | Upstash Redis free tier |
| **SignalR** | Real-time dashboard updates: new appointment, low-stock alert, invoice paid | Analytics or a notification endpoint in Patient service, pushed to the web app | Built into ASP.NET Core | Azure SignalR Service free tier (limited connections) |
| **Clean Architecture** | Separates the domain, application, infrastructure, and API layers inside a service | Patient and Appointment service, Pharmacy and Billing service | Code structure | Not applicable |
| **CQRS** | Separates write operations (commands) from read operations (queries) | Pharmacy and Billing service; read-heavy queries in Patient service | Code structure | Not applicable |
| **Alembic** | Database migrations for SQLAlchemy | AI service, Analytics service | Python | Not applicable |

### 3.3 Where each technology applies

| Service | Clean Architecture | CQRS | Other technologies |
|---|---|---|---|
| Identity (.NET) | No (simple service) | No | ASP.NET Core Identity, JWT |
| Patient and Appointment (.NET) | Yes | Yes (queries for schedules and patient lists) | gRPC server, RabbitMQ publisher, Redis cache, SignalR notifications |
| Pharmacy and Billing (.NET) | Yes | Yes (commands for stock and invoices; queries for reports) | RabbitMQ consumer, gRPC client, Polly for retries |
| AI (Python) | No | No | Alembic, Redis cache for repeated answers |
| Analytics (Python) | No | No | Alembic, Redis cache, gRPC client, SignalR or WebSocket push |
| Gateway (.NET) | No | No | YARP, JWT validation, rate limiting |

Clean Architecture and CQRS are applied only to the two complex .NET services. Applying them to every service would add unnecessary code for a study project.

### 3.4 Library notes
- **MediatR** is a popular CQRS library, but its newer versions require a paid license for commercial use. For this study project, write simple command and query handlers yourself to learn the pattern. Check the license before using MediatR in any commercial project.
- **MassTransit** is free and open source for the scenarios in this project. Verify its license terms if you use it in a commercial product.
- **Polly** is free and used for retries and circuit breakers in Pharmacy and Billing.

---

## 4. Architecture

```
  Web App (SPA)                        Mobile App (MAUI, optional)
        |                                        |
        |  REST + SignalR (live updates)         |
        +------------------+---------------------+
                           |
                           v
        +----------------------------------------------+
        |  API Gateway (YARP)                           |
        |  Routing, JWT validation, rate limiting       |
        +--+-------+--------+--------+--------+---------+
           |       |        |        |        |
           v       v        v        v        v
        +-------+-------+-------+-------+-------+
        | 1     | 2     | 3     | 4     | 5     |
        | Auth  | Pati- | Pharm-| AI    | Ana-  |
        |       | ent & | acy & |       | lytics|
        |       | Appt  | Bill. |       |       |
        +---+---+---+---+---+---+---+---+---+---+
            |       |       |       |       |
            v       v       v       v       v
        IdentityDb ClinicalDb PharmacyDb AiDb AnalyticsDb
        (local: separate databases; cloud: one DB with schemas)

    Service-to-service:
      gRPC   : Billing -> Patient (visit details), Analytics -> Pharmacy (stock data)
      Events : Patient -> RabbitMQ -> Billing (AppointmentCompleted, PrescriptionIssued)
      Cache  : Redis used by Patient, Analytics, and Gateway
```

Services 1 to 4 are business services. Service 5 (Analytics) reads data through APIs and does not write to other services' data.

---

## 5. Microservices

### 5.1 Identity Service (.NET)
Purpose: staff accounts, login, tokens, roles.
- Local registration and login (ASP.NET Core Identity)
- JWT access and refresh tokens
- External logins: Google, Microsoft, GitHub, Facebook
- Roles: Admin, Doctor, Nurse, Receptionist, Pharmacist, Billing
- Refresh token revocation stored in Redis
- Database: `IdentityDb` (cloud schema: `identity`)

### 5.2 Patient and Appointment Service (.NET)
Purpose: the core clinical-administrative domain. Uses Clean Architecture and CQRS.
- Patient records (synthetic data): demographics, contact, medical history summary
- Doctor schedules and departments
- Appointment booking, rescheduling, cancellation, no-show marking, and completion
- Check-in and visit history
- Visit notes (the AI assists with summarizing these)
- gRPC server: returns visit details to Billing
- Publishes events to RabbitMQ: `AppointmentCompleted`, `PatientRegistered`
- Caches schedule and department lists in Redis
- Pushes live updates (new appointment, check-in) through SignalR
- Database: `ClinicalDb` (cloud schema: `clinical`)

Project layers:
```
PatientAppointment.Domain          (entities, business rules; no dependencies)
PatientAppointment.Application     (commands, queries, handlers, interfaces)
PatientAppointment.Infrastructure  (EF Core, Redis, RabbitMQ, external services)
PatientAppointment.Api             (REST controllers, gRPC services, SignalR hub)
PatientAppointment.Tests           (unit tests for Domain and Application)
```

### 5.3 Pharmacy and Billing Service (.NET)
Purpose: medicines, stock, prescriptions, and money. Merged because a prescription creates a stock movement and a charge. Uses Clean Architecture and CQRS.
- Medicine catalog with batch numbers and expiry dates
- Stock in and out, low-stock and near-expiry alerts (pushed through SignalR)
- Prescriptions linked to visits (visit details fetched from Patient service over gRPC)
- Consumes `AppointmentCompleted` from RabbitMQ and creates an invoice automatically
- Invoices for consultations, prescriptions, and bed stays
- Payment recording (cash, card as a simulated status)
- Polly retry and circuit breaker around the gRPC call to Patient service
- Commands: create prescription, adjust stock, record payment
- Queries: stock levels, outstanding balances, expiry reports
- Database: `PharmacyDb` (cloud schema: `pharmacy` for stock and prescriptions, `billing` for invoices and payments)

Project layers follow the same four-layer structure as section 5.2.

### 5.4 AI Service (Python, FastAPI)
Purpose: an administrative and documentation assistant.
- Summarize visit notes for a doctor to review
- Draft discharge summary text from visit data (doctor must approve)
- Explain an invoice to a patient in plain language
- Q&A chatbot over operational data, such as "how many follow-ups are pending this week?"
- Provider abstraction so you can switch between Gemini, Groq, and Ollama
- Redis cache for repeated questions
- Every AI output is marked as a draft that needs human review
- Migrations with Alembic
- Database: `AiDb` (cloud schema: `ai`; chat history and usage logs)

### 5.5 Analytics Service (Python, FastAPI + pandas)
Purpose: hospital operations analysis and dashboards.
- Bed occupancy rate and trend
- Average waiting time per department
- No-show rate by doctor and day of week
- Patient volume forecast for the next 14 days (scikit-learn or statsmodels)
- Anomaly flags, such as a sudden spike in emergency visits
- Medicine usage trends and expiry risk (data from Pharmacy service over gRPC)
- Redis cache for dashboard aggregates
- Live KPI updates pushed to the web app through SignalR-compatible WebSocket endpoint
- CSV upload with automatic data profiling
- Migrations with Alembic
- Database: `AnalyticsDb` (cloud schema: `analytics`; snapshots and reports)

---

## 6. Event Flow Example

1. A doctor marks an appointment as completed in the Patient service.
2. The Patient service saves the change and publishes `AppointmentCompleted` to RabbitMQ.
3. The Pharmacy and Billing service consumes the event.
4. It calls the Patient service over gRPC to get visit details, then creates an invoice.
5. It sends a SignalR notification so the billing dashboard updates live.
6. The Analytics service updates its cached KPIs in Redis.

This flow teaches event-driven design, gRPC calls, caching, and real-time updates in one feature.

---

## 7. Roles and Access

| Role | Can access |
|---|---|
| Admin | Everything, user management |
| Doctor | Own schedule, assigned patients, visit notes, AI summaries |
| Nurse | Check-in, vitals entry, patient lookup |
| Receptionist | Patient registration, appointment booking, check-in |
| Pharmacist | Medicine stock, prescriptions |
| Billing | Invoices, payments, balances |

Enforce roles in YARP for coarse access and in each service for fine-grained rules (for example, a doctor sees only their own patients).

---

## 8. Authentication Plan

### Phase A: Local (start here)
- ASP.NET Core Identity with SQL Server
- Email and password sign-up and sign-in
- Access token (15 min), refresh token (7 days, rotated and stored in Redis)
- Account lockout after failed attempts
- Staff accounts are created by Admin, not open sign-up

### Phase B: External providers (free)
| Provider | Setup |
|---|---|
| Google | Google Cloud Console OAuth client |
| Microsoft | Entra ID app registration (free tier) |
| GitHub | GitHub OAuth App |
| Facebook | Meta developer app in development mode |

External logins apply to staff accounts only and must be linked to an account that an Admin has approved.

### Security checklist
- HTTPS everywhere
- No secrets in source code
- Rate limiting at YARP and on login endpoints
- Audit log for viewing and changing patient records
- Validate tokens at the gateway and in each service
- gRPC calls use TLS in cloud deployment

---

## 9. Web and Mobile

### Web app
- Login and role-based menu
- Pages: Dashboard (live updates), Patients, Appointments, Doctor Schedule, Pharmacy Stock, Billing, AI Assistant, Admin
- Uses REST for data and SignalR for live updates
- Hosted on Azure Static Web Apps or Cloudflare Pages (free)

### Mobile app (optional, last phase)
- .NET MAUI for doctors: today's schedule, patient lookup, AI summary view
- Run on emulator or sideload APK; no store publishing

---

## 10. Dashboard and Data Analysis

Dashboard widgets:
- KPI cards: patients today, waiting now, beds occupied, pending invoices (live through SignalR)
- Line chart: daily patient volume
- Bar chart: no-shows by department
- Gauge or bar: bed occupancy
- Table: low-stock and near-expiry medicines
- AI insight card: weekly summary from the AI Service

Charts: Apache ECharts or Chart.js (free).

---

## 11. Free Deployment Plan

Free tiers and their limits change, so check current terms before deploying.

| Component | Where | Free option |
|---|---|---|
| Web app | Azure Static Web Apps or Cloudflare Pages | Free tier |
| Gateway and 5 services | Azure Container Apps | Monthly free grant; scales to zero |
| Database | Azure SQL Database | Azure free offer (one free database per subscription) |
| Redis | Upstash | Free tier with request limits |
| RabbitMQ | CloudAMQP | Free plan (limited connections and messages) |
| Real-time | Azure SignalR Service | Free tier (limited concurrent connections) |
| Monitoring | Application Insights | Free monthly allowance |
| Images | GitHub Container Registry | Free for public images |
| CI/CD | GitHub Actions | Free minutes |
| LLM | Gemini or Groq free API | Free rate-limited tier |

Cloud database: all five logical databases are moved into one Azure SQL database as separate schemas. Only the connection string and the schema setting change per service.

### Local development (free)
- Docker Compose runs SQL Server (Developer Edition), Redis, RabbitMQ (with management UI), the gateway, and all 5 services
- An init script creates the five databases on first start
- Give Docker at least 8 GB of RAM for this setup
- Optional: Ollama for a local LLM

---

## 12. Project Structure

```
hospital-platform/
├── docs/                              # This plan, decision notes
├── src/
│   ├── gateway/                       # YARP (.NET)
│   │   ├── Gateway.Api/
│   │   ├── Gateway.Tests/
│   │   └── Gateway.sln
│   ├── identity-service/              # .NET
│   │   ├── Identity.Api/
│   │   ├── Identity.Tests/
│   │   └── Identity.sln
│   ├── patient-appointment-service/   # .NET, Clean Architecture + CQRS
│   │   ├── PatientAppointment.Domain/
│   │   ├── PatientAppointment.Application/
│   │   ├── PatientAppointment.Infrastructure/
│   │   ├── PatientAppointment.Api/
│   │   ├── PatientAppointment.Tests/
│   │   └── PatientAppointment.sln
│   ├── pharmacy-billing-service/      # .NET, Clean Architecture + CQRS
│   │   ├── PharmacyBilling.Domain/
│   │   ├── PharmacyBilling.Application/
│   │   ├── PharmacyBilling.Infrastructure/
│   │   ├── PharmacyBilling.Api/
│   │   ├── PharmacyBilling.Tests/
│   │   └── PharmacyBilling.sln
│   ├── ai-service/                    # Python (FastAPI), Alembic
│   │   ├── app/
│   │   ├── migrations/
│   │   ├── tests/
│   │   ├── requirements.txt
│   │   └── Dockerfile
│   ├── analytics-service/             # Python (FastAPI + pandas), Alembic
│   │   ├── app/
│   │   ├── migrations/
│   │   ├── tests/
│   │   ├── requirements.txt
│   │   └── Dockerfile
│   ├── web/                           # Angular or React
│   └── mobile/                        # .NET MAUI (optional)
├── protos/                            # Shared gRPC .proto contracts
├── data/
│   └── seed/                          # Synthetic patients, staff, medicines
├── infra/
│   ├── docker-compose.yml             # SQL Server, Redis, RabbitMQ, all services
│   ├── sql/init-databases.sql         # Creates the 5 local databases
│   └── azure/                         # Cloud scripts (single DB with schemas)
├── tests/
│   ├── integration/                   # Tests across services (gRPC, events)
│   └── e2e/                           # Playwright tests for the web app
├── .github/workflows/                 # CI/CD
├── .gitignore
└── README.md
```

---

## 13. Roadmap (about 16 weeks, part-time)

| Week | Milestone |
|---|---|
| 1 | Repo, Docker Compose with SQL Server, Redis, RabbitMQ; hello-world services |
| 2 | Identity Service: local login, JWT, roles |
| 3 | Gateway with YARP: routing and token validation |
| 4 | Patient and Appointment Service: Clean Architecture layers, EF Core migrations, basic CRUD |
| 5 | Patient and Appointment: CQRS commands and queries; Redis caching |
| 6 | Web app: login, role menu, patients and appointments pages |
| 7 | Pharmacy and Billing Service: Clean Architecture, medicines, stock, prescriptions |
| 8 | RabbitMQ with MassTransit: `AppointmentCompleted` event creates invoices |
| 9 | gRPC: Patient service server, Billing client, Polly retries |
| 10 | Analytics Service (Python): dashboard endpoints, Alembic migrations, Redis cache |
| 11 | SignalR: live dashboard updates and low-stock alerts |
| 12 | AI Service: visit note summaries and invoice explanations (draft-only), Alembic |
| 13 | AI chatbot over operational data |
| 14 | External logins: Google, GitHub, Microsoft, Facebook |
| 15 | CI/CD with GitHub Actions; containerize all services; deploy to Azure |
| 16 | Hardening: tests, audit logs, documentation; optional mobile app |

---

## 14. Python Learning Path

| C# | Python |
|---|---|
| ASP.NET Core Web API | FastAPI |
| Entity Framework Core | SQLAlchemy (with pyodbc) |
| EF Core migrations | Alembic |
| LINQ | List comprehensions, pandas |
| NuGet | pip or uv |
| xUnit | pytest |
| Built-in DI | FastAPI `Depends()` |
| appsettings.json | `.env` with pydantic-settings |

Suggested order: Python basics, FastAPI and Pydantic, SQLAlchemy with SQL Server, Alembic migrations, pandas, basic scikit-learn, then LLM APIs.

---

## 15. Testing

- .NET: xUnit with Testcontainers for SQL Server, Redis, and RabbitMQ
- Domain and Application layers tested without infrastructure (Clean Architecture makes this possible)
- gRPC: test the contract with the `.proto` files and an in-memory test server
- Python: pytest with a test SQL Server container
- Integration: test the `AppointmentCompleted` flow end to end (publish, consume, invoice created)
- Web: Playwright for key flows (login, book appointment, view live dashboard)
- Every service has a `/health` endpoint

---

## 16. Risks

| Risk | Mitigation |
|---|---|
| Too many new technologies at once | Follow the roadmap order; add each technology in its own week |
| Separate databases drift into cross-DB queries | Code rule: services use gRPC, REST, or events only |
| Event messages processed twice | Make consumers idempotent (check if the invoice already exists) |
| Clean Architecture adds extra code | Apply it only to the two complex .NET services |
| Free tier limits (Redis, RabbitMQ, SignalR) | Keep local Docker setup as the main environment; cloud is a deployment target |
| Healthcare rules and sensitive data | Synthetic data only; document this in the README |
| AI giving medical advice | AI output is draft-only, with a clear label and no diagnosis features |
| Local resource use | 8 GB RAM for Docker |

---

## 17. Next Steps

1. Copy this updated plan into the repository's `docs/` folder and commit it.
2. Add Redis and RabbitMQ to `infra/docker-compose.yml` alongside SQL Server.
3. Build the Identity Service with local login and roles.
4. Create the gateway with YARP.
