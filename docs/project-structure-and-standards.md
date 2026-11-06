# Hospital Platform — Project Structure & Engineering Standards

This document describes the solution layout, dependency flow, design patterns, tool
versions, and the coding standards every contributor must follow. It complements the
high-level plan in [`enterprise-app-plan.md`](./enterprise-app-plan.md).

> Study project notice: Use only synthetic (fake) data. Never use real patient
> information. AI features assist with administrative/documentation tasks only and never
> provide diagnosis or treatment advice.

---

## 1. Repository Layout

```
Hospital-platform/
├── docs/										# Plan + engineering standards (this file)
└── src/
	├── gateway/								# API gateway (.NET, YARP)
	│   ├── Gateway.Api/						# ASP.NET Core empty web host
	│   ├── Gateway.Tests/						# xUnit tests
	│   └── Gateway.slnx
	├── identity-service/						# .NET — accounts, login, JWT, roles
	│   ├── Identity.Api/						# ASP.NET Core Web API
	│   ├── Identity.Tests/						# xUnit tests
	│   └── Identity.slnx
	├── patient-appointment-service/			# .NET — Clean Architecture + CQRS
	│   ├── PatientAppointment.Domain/			# Entities, value objects, domain rules
	│   ├── PatientAppointment.Application/		# Commands, queries, handlers, interfaces
	│   ├── PatientAppointment.Infrastructure/	# EF Core, Redis, RabbitMQ, gRPC, external IO
	│   ├── PatientAppointment.Api/				# REST controllers, gRPC services, SignalR hub
	│   ├── PatientAppointment.Tests/			# xUnit tests (Domain + Application)
	│   └── PatientAppointment.slnx
	└── pharmacy-billing-service/				# .NET — Clean Architecture + CQRS
		├── PharmacyBilling.Domain/
		├── PharmacyBilling.Application/
		├── PharmacyBilling.Infrastructure/
		├── PharmacyBilling.Api/
		├── PharmacyBilling.Tests/
		└── PharmacyBilling.slnx
```

> Note: The .NET 10 SDK produces `.slnx` (XML) solution files by default instead of the
> legacy `.sln` format. We keep `.slnx` consistently across all services.

Future (not yet scaffolded): `ai-service/` and `analytics-service/` (Python/FastAPI),
`web/`, `mobile/`, `protos/`, `infra/`, and `tests/` — see the plan document.

---

## 2. Service Responsibilities

| Service | Architecture | Patterns | Key responsibilities |
|---|---|---|---|
| Gateway | Single project | Reverse proxy | Routing, JWT validation, rate limiting (YARP) |
| Identity | Single project | Service layer | Local login, JWT access/refresh, roles, external logins |
| PatientAppointment | 4-layer Clean Architecture | CQRS, Repository, Domain Events | Patients, schedules, appointments, visit notes; gRPC server; RabbitMQ publisher; Redis cache; SignalR |
| PharmacyBilling | 4-layer Clean Architecture | CQRS, Repository, Retry/Circuit Breaker | Medicines, stock, prescriptions, invoices, payments; RabbitMQ consumer; gRPC client (Polly) |

Clean Architecture and CQRS are applied **only** to the two complex .NET services. Simpler
services (Gateway, Identity) stay flat to avoid unnecessary ceremony.

---

## 3. Dependency Flow

### 3.1 Clean Architecture (PatientAppointment, PharmacyBilling)

Dependencies point **inward only**. The Domain has no external dependencies.

```
		  ┌───────────────────────────────────────────┐
		  │                  Api                      │  REST / gRPC / SignalR
		  │   (controllers, hubs, DI composition)     │
		  └───────────────┬─────────────┬─────────────┘
						  │             │
						  ▼             ▼
		  ┌──────────────────────────┐    │
		  │    Infrastructure        │    │  EF Core, Redis, RabbitMQ, gRPC clients
		  │  (implements Application │    │
		  │     interfaces)          │    │
		  └───────────┬──────────────┘    │
					  │                   │
					  ▼                   ▼
		  ┌───────────────────────────────────────────┐
		  │               Application                 │  Commands, queries, handlers,
		  │   (use cases, DTOs, port interfaces)      │  port interfaces (IRepository...)
		  └───────────────────┬───────────────────────┘
							  │
							  ▼
		  ┌───────────────────────────────────────────┐
		  │                 Domain                    │  Entities, value objects, rules
		  │          (no dependencies)                │
		  └───────────────────────────────────────────┘
```

Project references (as wired in the solutions):

| Project | References |
|---|---|
| `*.Domain` | *(none)* |
| `*.Application` | `*.Domain` |
| `*.Infrastructure` | `*.Application` (+ `*.Domain` transitively) |
| `*.Api` | `*.Infrastructure`, `*.Application` |
| `*.Tests` | `*.Domain`, `*.Application` |

Rules:
- Domain must never reference Application, Infrastructure, or any NuGet infrastructure
  package (no EF Core, no ASP.NET types).
- Application defines **interfaces (ports)**; Infrastructure provides the
  **implementations (adapters)**. The Api wires them together via dependency injection.
- Never let the Api talk to Infrastructure's concrete types directly — depend on
  Application abstractions.

### 3.2 Service-to-service communication

No cross-database access. Services exchange data only through:

| Mechanism | Example |
|---|---|
| gRPC | `PharmacyBilling → PatientAppointment` for visit details; Analytics → Pharmacy for stock |
| Events (RabbitMQ / MassTransit) | `PatientAppointment` publishes `AppointmentCompleted` → `PharmacyBilling` consumes and creates an invoice |
| Redis cache | Shared caching for dashboards, rate-limit counters, refresh-token revocation |

Each service reads/writes **only its own** database (local) or schema (cloud).

---

## 4. Design Patterns In Use

| Pattern | Where | Purpose |
|---|---|---|
| Clean Architecture (Ports & Adapters) | PatientAppointment, PharmacyBilling | Isolate domain from frameworks and IO |
| CQRS (hand-written handlers) | Application layers | Separate write (commands) from read (queries) |
| Repository / Unit of Work | Infrastructure (EF Core) | Abstract persistence behind Application interfaces |
| Domain Events | Domain → RabbitMQ | Decouple side effects (e.g., `AppointmentCompleted`) |
| Mediator-style dispatch (custom) | Application | Route commands/queries to handlers without a paid library |
| Reverse Proxy / Gateway | Gateway (YARP) | Single entry point, auth, rate limiting |
| Retry + Circuit Breaker | PharmacyBilling (Polly) | Resilient gRPC calls to PatientAppointment |
| Publish/Subscribe | RabbitMQ (MassTransit) | Event-driven integration |
| Cache-Aside | Redis | Read-heavy dashboard and lookup data |
| Options pattern | All .NET services | Strongly-typed configuration binding |

> MediatR is intentionally avoided — newer versions require a commercial license. Write
> simple command/query handlers to learn the pattern. Verify licenses before adopting any
> library in a commercial product.

---

## 5. SOLID Principles

All .NET code must follow SOLID. These principles are the reasoning behind the Clean
Architecture and CQRS choices in this project.

### 5.1 Single Responsibility Principle (SRP)
A class/module has one reason to change.
- A CQRS handler does exactly one use case: `CreateAppointmentCommandHandler` only creates
  an appointment — it does not also send emails or build invoices.
- Controllers only translate HTTP ↔ Application; they contain no business logic.
- Keep persistence, validation, and domain rules in separate types (repository, validator,
  entity) rather than one "service" god-class.

### 5.2 Open/Closed Principle (OCP)
Open for extension, closed for modification.
- Add behavior by adding a new command/query handler, not by editing a large switch.
- Use the provider abstraction for the AI service so adding Gemini/Groq/Ollama does not
  change existing callers.
- Prefer polymorphism and strategy over `if/else`/`switch` on type codes.

### 5.3 Liskov Substitution Principle (LSP)
Subtypes must be usable wherever the base type is expected.
- Any `IAppointmentRepository` implementation (EF Core, in-memory for tests) must honor the
  same contract and not throw unexpected exceptions or weaken guarantees.
- Do not override a method to do nothing or to require stricter preconditions than the base.

### 5.4 Interface Segregation Principle (ISP)
Prefer small, focused interfaces over large ones.
- Split ports by need: `IAppointmentReader` (queries) vs `IAppointmentWriter` (commands)
  instead of one fat `IAppointmentRepository` with 20 members.
- Clients depend only on the methods they actually use — a read handler should not see
  write methods.

### 5.5 Dependency Inversion Principle (DIP)
High-level policy depends on abstractions, not on details.
- Application defines interfaces (ports); Infrastructure implements them (adapters). This is
  why Domain/Application never reference EF Core, Redis, or RabbitMQ directly.
- The Api composes concrete implementations through dependency injection at startup.
- Always inject abstractions (`ILogger<T>`, `IPublishEndpoint`, repository interfaces), never
  `new` up infrastructure inside domain/application code.

| Principle | Primary enforcement in this project |
|---|---|
| SRP | One handler per use case; thin controllers |
| OCP | New handlers/providers instead of editing existing ones |
| LSP | Repository and provider implementations honor their contracts |
| ISP | Segregated read/write ports; role-specific interfaces |
| DIP | Ports in Application, adapters in Infrastructure, DI in Api |

---

## 6. Tooling & Versions

| Tool / Library | Version / Tier | Notes |
|---|---|---|
| .NET SDK | 10.0.401 | Target framework `net10.0` for all projects |
| Solution format | `.slnx` | Default for .NET 10 SDK |
| Test framework | xUnit | One `*.Tests` project per service |
| API gateway | YARP | Gateway service |
| ORM | EF Core | Per-service migrations; no cross-DB queries |
| Messaging | RabbitMQ + MassTransit | Free/OSS for this project |
| Cache | Redis | Local Docker / Upstash free tier in cloud |
| Resilience | Polly | Retries + circuit breakers |
| Real-time | SignalR | Built into ASP.NET Core |
| RPC | gRPC | HTTP/2 between backend services |
| Logging | Serilog (+ App Insights) | Free monitoring allowance |
| Auth | ASP.NET Core Identity + JWT; OAuth (Google, Microsoft, GitHub, Facebook) | All free tiers |

> When adding packages, pin versions explicitly in each `.csproj` and keep a single
> version per package across a solution (consider `Directory.Packages.props` for central
> package management).

---

## 7. Coding Standards (C#)

### 7.1 General
- Follow the default .NET / Roslyn analyzer conventions and the `.editorconfig` (add one
  at the repo root if missing).
- Enable nullable reference types (`<Nullable>enable</Nullable>`) and implicit usings.
- Treat warnings seriously; prefer `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
  on CI builds.
- One public type per file; file name matches the type name.
- Keep methods small and single-purpose; avoid classes with mixed responsibilities.

### 7.2 Naming conventions

| Element | Style | Example |
|---|---|---|
| Namespace | PascalCase, matches folder path | `PatientAppointment.Application.Appointments` |
| Class / struct / record / enum | PascalCase | `AppointmentService`, `PatientId` |
| Interface | PascalCase with `I` prefix | `IAppointmentRepository` |
| Method | PascalCase, verb phrase | `CreateInvoice`, `GetScheduleAsync` |
| Async method | PascalCase + `Async` suffix | `SaveChangesAsync` |
| Public property | PascalCase | `FirstName`, `IsActive` |
| Local variable / parameter | camelCase | `patientId`, `appointmentDate` |
| Private field | `_camelCase` (leading underscore) | `_repository`, `_logger` |
| Constant | PascalCase | `MaxRetryCount` |
| `static readonly` | PascalCase | `DefaultTimeout` |
| Generic type parameter | `T` prefix | `TEntity`, `TKey` |
| Boolean members | affirmative, prefixed `Is/Has/Can` | `IsCompleted`, `HasBalance` |
| Enum members | PascalCase, singular | `AppointmentStatus.Completed` |
| CQRS command | `<Action><Entity>Command` | `CreateAppointmentCommand` |
| CQRS query | `<Get><Entity>Query` | `GetPatientScheduleQuery` |
| Handler | `<Command/Query>Handler` | `CreateAppointmentCommandHandler` |
| DTO | `<Name>Dto` or `<Name>Response/Request` | `InvoiceDto`, `BookAppointmentRequest` |
| Event (integration) | past-tense fact | `AppointmentCompleted`, `PatientRegistered` |

### 7.3 Variable & member writing rules
- Prefer `var` when the type is obvious from the right-hand side; use explicit types when
  it improves clarity.
- Use meaningful, intention-revealing names — no abbreviations beyond well-known ones
  (`id`, `db`, `dto`).
- Declare variables as close to first use as possible; keep scope tight.
- Prefer `readonly` fields and immutable `record` types for DTOs and value objects.
- No magic numbers or strings — promote to named constants or configuration.
- Always use `async`/`await`; never block with `.Result` or `.Wait()`. Pass and honor
  `CancellationToken` through async call chains.
- Dispose `IDisposable` with `using`/`await using`.

### 7.4 Layout & style
- 4-space indentation, no tabs. UTF-8, LF or CRLF consistently (set in `.editorconfig`).
- One statement per line; always use braces `{ }` even for single-line blocks.
- `using` directives outside the namespace, sorted with `System.*` first.
- Order members: constants, fields, constructors, properties, public methods, private
  methods.
- Keep lines readable (soft limit ~120 chars).

### 7.5 Comments & documentation
- Write self-documenting code; comment the **why**, not the **what**.
- Use XML doc comments (`///`) on public APIs of Domain and Application layers.
- Avoid redundant comments that restate the code.

---

## 8. Project & Structure Rules

- Each service is an independent solution with its own migrations and configuration.
- A service only reads/writes its own database (local) or schema (cloud). No cross-DB
  joins — integrate via gRPC, REST, or events.
- Connection strings and secrets come from configuration (`dotnet user-secrets` / `.env`
  locally; GitHub Secrets / container secrets in cloud). **Never commit secrets.**
- Every service exposes a `/health` endpoint.
- Keep domain logic out of controllers; controllers only translate HTTP ↔ Application.
- Make event consumers **idempotent** (e.g., check if an invoice already exists before
  creating one).

---

## 9. Testing Standards

- xUnit for all .NET tests; one `*.Tests` project per service.
- Domain and Application layers are tested **without infrastructure** (Clean Architecture
  enables this).
- Use Testcontainers for integration tests needing SQL Server, Redis, or RabbitMQ.
- Test naming: `MethodUnderTest_Scenario_ExpectedResult`
  (e.g., `CreateInvoice_WhenAppointmentCompleted_CreatesPendingInvoice`).
- Arrange–Act–Assert structure; one logical assertion focus per test.
- gRPC contracts tested against `.proto` files with an in-memory server.
- Integration: cover the `AppointmentCompleted` flow end-to-end (publish → consume →
  invoice created).

---

## 10. Build & Validation

### 10.1 Repo-wide build configuration

Two files at the repository root apply to every .NET project automatically:

- [`Directory.Build.props`](../Directory.Build.props) — shared settings: `net10.0`,
  `Nullable` and `ImplicitUsings` enabled, `latest` language version, recommended
  analyzers with `EnforceCodeStyleInBuild`, and `TreatWarningsAsErrors` on CI only
  (when `CI=true`).
- [`Directory.Packages.props`](../Directory.Packages.props) — Central Package Management.
  Package versions are declared once here; projects reference packages **without** a
  `Version` attribute. Add new versions here, not in individual `.csproj` files.

### 10.2 Commands

Build a single service:

```powershell
dotnet build src/patient-appointment-service/PatientAppointment.slnx
```

Build all services:

```powershell
"gateway/Gateway.slnx","identity-service/Identity.slnx","patient-appointment-service/PatientAppointment.slnx","pharmacy-billing-service/PharmacyBilling.slnx" |
  ForEach-Object { dotnet build "src/$_" }
```

Run tests for a service:

```powershell
dotnet test src/patient-appointment-service/PatientAppointment.slnx
```

Validation rules before committing:
- Solution builds with **0 errors** and no new warnings.
- All affected tests pass.
- No secrets, connection strings, or real data committed.

---

## 11. Git & Commit Conventions

- Branch from `main`; use short-lived feature branches (`feature/`, `fix/`, `chore/`).
- Conventional Commits style: `type(scope): summary`
  (e.g., `feat(billing): create invoice on AppointmentCompleted`).
- Keep commits focused and buildable.
- Open a PR for review; CI must be green before merge.
