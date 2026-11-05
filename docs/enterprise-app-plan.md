# Study Project: Hospital Management System (Microservices)

## 1. Overview

A medium-size study project: a hospital management system built as 5 microservices, a web app, and an optional mobile app. It practices enterprise patterns (microservices, role-based authentication, AI integration, data analysis, and deployment) at zero cost.

### Important note on data
This is a study project. Use only synthetic (fake) patient data. Never use real patient information. The AI assists with administrative work and documentation only. It does not diagnose or recommend treatment.

### Goals
- Learn microservices with .NET (C#) and Python
- Role-based access control for hospital staff roles
- Local login first, then Google, Microsoft, GitHub, and Facebook
- AI documentation assistant using a free LLM tier
- Dashboard and data analysis for hospital operations
- Deploy to free cloud tiers

### Non-Goals
- Real clinical use or compliance certification (HIPAA, etc.)
- Medical device or lab integration (HL7, FHIR), which can be a future idea
- Kubernetes and message brokers (services talk over HTTP)

---

## 2. Database Strategy

This is the key decision for the project, so it is stated first.

| Environment | Setup |
|---|---|
| Local development | One SQL Server instance with a separate database per service (`IdentityDb`, `ClinicalDb`, `PharmacyDb`, `AiDb`, `AnalyticsDb`) |
| Cloud deployment | One Azure SQL database (free offer) with a separate schema per service (`identity`, `clinical`, `pharmacy`, `ai`, `analytics`) |

Rules that make the switch from separate databases to one database easy:
- Each service only reads and writes its own database or schema.
- No cross-database queries and no cross-service joins. Services get data from each other through APIs.
- Each service has its own EF Core or SQLAlchemy migrations.
- Connection strings are set per service through configuration, so the cloud setup only changes the connection string and the schema name.

---

## 3. Technology Stack

| Layer | Technology | Why |
|---|---|---|
| Database | SQL Server (Docker locally, Azure SQL in cloud) | Your requirement |
| Web app | Angular or React (TypeScript) | Free hosting on Azure Static Web Apps or Cloudflare Pages |
| Mobile app | .NET MAUI (optional) | Reuse C# models; run on emulator or sideloaded APK |
| API gateway | YARP (ASP.NET Core) | Free, routes and validates tokens |
| .NET services | ASP.NET Core Web API, EF Core | Your existing strength |
| Python services | FastAPI, SQLAlchemy (pyodbc), pandas, scikit-learn | Learning target |
| Authentication | ASP.NET Core Identity + JWT; OAuth with Google, Microsoft, GitHub, Facebook | All free |
| AI | Google Gemini or Groq free API; Ollama for offline testing | Zero cost |
| Logging | Serilog + Azure Application Insights (free allowance) | Free monitoring |
| Containers | Docker, Docker Compose | Same images locally and in the cloud |
| CI/CD | GitHub Actions | Free minutes |
| Secrets | dotnet user-secrets and .env locally; GitHub Secrets and container app secrets in cloud | No secrets in code |

---

## 4. Architecture

```
  Web App (SPA)                  Mobile App (MAUI, optional)
        |                                  |
        +----------------+-----------------+
                         |
                         v
        +-----------------------------------------+
        |  API Gateway (YARP)                      |
        |  Routing, JWT validation, CORS           |
        +--+--------+--------+--------+--------+---+
           |        |        |        |        |
           v        v        v        v        v
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
```

Services 1 to 4 are business services. Service 5 (Analytics) reads data through the other services' APIs and does not write to their data.

---

## 5. Microservices

### 5.1 Identity Service (.NET)
Purpose: staff accounts, login, tokens, roles.
- Local registration and login (ASP.NET Core Identity)
- JWT access and refresh tokens
- External logins: Google, Microsoft, GitHub, Facebook
- Roles: Admin, Doctor, Nurse, Receptionist, Pharmacist, Billing
- Database: `IdentityDb` (cloud schema: `identity`)

### 5.2 Patient and Appointment Service (.NET)
Purpose: the core clinical-administrative domain.
- Patient records (synthetic data): demographics, contact, medical history summary
- Doctor schedules and departments
- Appointment booking, rescheduling, cancellation, and no-show marking
- Check-in and visit history
- Visit notes (the AI assists with summarizing these)
- Database: `ClinicalDb` (cloud schema: `clinical`)

### 5.3 Pharmacy and Billing Service (.NET)
Purpose: medicines, stock, prescriptions, and money. These are merged into one service because they share the same flow: a prescription creates stock movement and a charge.
- Medicine catalog with batch numbers and expiry dates
- Stock in and out, low-stock and near-expiry alerts
- Prescriptions linked to visits (data fetched from the Patient and Appointment service API)
- Invoices for consultations, prescriptions, and bed stays
- Payment recording (cash, card as a simulated status)
- Outstanding balance reports
- Database: `PharmacyDb` (cloud schema: `pharmacy` for stock and prescriptions, `billing` for invoices and payments)

### 5.4 AI Service (Python, FastAPI)
Purpose: an administrative and documentation assistant.
- Summarize visit notes for a doctor to review
- Draft discharge summary text from visit data (doctor must approve)
- Explain an invoice to a patient in plain language
- Q&A chatbot over operational data, such as "how many follow-ups are pending this week?"
- Provider abstraction so you can switch between Gemini, Groq, and Ollama
- Every AI output is marked as a draft that needs human review
- Database: `AiDb` (cloud schema: `ai`; chat history and usage logs)

### 5.5 Analytics Service (Python, FastAPI + pandas)
Purpose: hospital operations analysis and dashboards.
- Bed occupancy rate and trend
- Average waiting time per department
- No-show rate by doctor and day of week
- Patient volume forecast for the next 14 days (scikit-learn or statsmodels)
- Anomaly flags, such as a sudden spike in emergency visits
- Medicine usage trends and expiry risk
- CSV upload with automatic data profiling
- Database: `AnalyticsDb` (cloud schema: `analytics`; snapshots and reports)

---

## 6. Roles and Access

| Role | Can access |
|---|---|
| Admin | Everything, user management |
| Doctor | Own schedule, assigned patients, visit notes, AI summaries |
| Nurse | Check-in, vitals entry, patient lookup |
| Receptionist | Patient registration, appointment booking, check-in |
| Pharmacist | Medicine stock, prescriptions |
| Billing | Invoices, payments, balances |

Enforce roles in the gateway for coarse access and in each service for fine-grained rules (for example, a doctor sees only their own patients).

---

## 7. Authentication Plan

### Phase A: Local (start here)
- ASP.NET Core Identity with SQL Server
- Email and password sign-up and sign-in
- Access token (15 min), refresh token (7 days, rotated)
- Account lockout after failed attempts
- Staff accounts are created by Admin, not open sign-up (realistic for hospitals)

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
- Rate limiting on login
- Audit log for viewing and changing patient records
- Validate tokens at the gateway and in each service

---

## 8. Web and Mobile

### Web app
- Login and role-based menu
- Pages: Dashboard, Patients, Appointments, Doctor Schedule, Pharmacy Stock, Billing, AI Assistant, Admin
- Hosted on Azure Static Web Apps or Cloudflare Pages (free)

### Mobile app (optional, last phase)
- .NET MAUI for doctors: today's schedule, patient lookup, AI summary view
- Run on emulator or sideload APK; no store publishing

---

## 9. Dashboard and Data Analysis

Dashboard widgets:
- KPI cards: patients today, waiting now, beds occupied, pending invoices
- Line chart: daily patient volume
- Bar chart: no-shows by department
- Gauge or bar: bed occupancy
- Table: low-stock and near-expiry medicines
- AI insight card: weekly summary from the AI Service

Charts: Apache ECharts or Chart.js (free).

---

## 10. Free Deployment Plan

Free tiers and their limits change, so check current terms before deploying.

| Component | Where | Free option |
|---|---|---|
| Web app | Azure Static Web Apps or Cloudflare Pages | Free tier |
| Gateway and 5 services | Azure Container Apps | Monthly free grant; scales to zero |
| Database | Azure SQL Database | Azure free offer (one free database per subscription) |
| Monitoring | Application Insights | Free monthly allowance |
| Images | GitHub Container Registry | Free for public images |
| CI/CD | GitHub Actions | Free minutes |
| LLM | Gemini or Groq free API | Free rate-limited tier |

Cloud database: all five logical databases are moved into one Azure SQL database as separate schemas. Only the connection string and the schema setting change per service.

### Local development (free)
- Docker Compose runs SQL Server (Developer Edition, free for development), the gateway, and all 5 services
- An init script creates the five databases on first start
- Give Docker at least 5 GB of RAM for this setup
- Optional: Ollama for a local LLM

---

## 11. Project Structure

```
hospital-platform/
├── docs/                            # This plan, decision notes
├── src/
│   ├── gateway/                     # YARP (.NET)
│   ├── identity-service/            # .NET
│   ├── patient-appointment-service/ # .NET
│   ├── pharmacy-billing-service/    # .NET
│   ├── ai-service/                  # Python (FastAPI)
│   ├── analytics-service/           # Python (FastAPI + pandas)
│   ├── web/                         # Angular or React
│   └── mobile/                      # .NET MAUI (optional)
├── data/
│   └── seed/                        # Synthetic patient, staff, and medicine data
├── infra/
│   ├── docker-compose.yml
│   ├── sql/init-databases.sql       # Creates the 5 local databases
│   └── azure/                       # Bicep or Azure CLI scripts (single DB, schemas)
├── tests/                           # Integration tests
└── .github/workflows/               # CI/CD
```

---

## 12. Roadmap (about 13 weeks, part-time)

| Week | Milestone |
|---|---|
| 1 | Repo, Docker Compose with SQL Server and the 5 local databases, hello-world services in .NET and Python |
| 2 | Identity Service: local login, JWT, roles |
| 3 | Gateway with token validation and routing |
| 4 | Patient and Appointment Service: patients, doctors, booking (EF Core migrations) |
| 5 | Web app: login, role menu, patients and appointments pages |
| 6 | Pharmacy and Billing Service: medicines, stock, prescriptions, invoices, payments |
| 7 | Analytics Service: occupancy, wait times, no-show rates, dashboard page |
| 8 | AI Service: visit note summaries and invoice explanations (draft-only) |
| 9 | AI chatbot over operational data |
| 10 | External logins: Google, GitHub, Microsoft, Facebook |
| 11 | CI/CD with GitHub Actions; containerize all services |
| 12 | Deploy to Azure Container Apps, single Azure SQL database with schemas, Static Web Apps |
| 13 | Audit logs, tests, documentation; optional mobile app |

---

## 13. Python Learning Path

| C# | Python |
|---|---|
| ASP.NET Core Web API | FastAPI |
| Entity Framework Core | SQLAlchemy (with pyodbc) |
| LINQ | List comprehensions, pandas |
| NuGet | pip or uv |
| xUnit | pytest |
| Built-in DI | FastAPI `Depends()` |
| appsettings.json | `.env` with pydantic-settings |

Suggested order: Python basics, FastAPI and Pydantic, SQLAlchemy with SQL Server, pandas, basic scikit-learn, then LLM APIs.

---

## 14. Testing

- .NET: xUnit with Testcontainers for SQL Server
- Python: pytest with a test SQL Server container
- Web: Playwright for key flows (login, book appointment, view dashboard)
- Every service has a `/health` endpoint

---

## 15. Risks

| Risk | Mitigation |
|---|---|
| Separate databases drift into cross-DB queries | Code rule: services use APIs only; review for cross-database SQL |
| Healthcare rules and sensitive data | Synthetic data only; document this in the README |
| AI giving medical advice | AI output is draft-only, with a clear label and no diagnosis features |
| Free tier limits | Scale to zero, cache AI responses, cap tokens per request |
| Local resource use | 5 GB RAM for Docker |

---

## 16. Next Steps

1. Create the repo and the Docker Compose file with SQL Server.
2. Add the init script that creates the 5 local databases.
3. Generate the synthetic seed data.
4. Build the Identity Service with local login and roles.
