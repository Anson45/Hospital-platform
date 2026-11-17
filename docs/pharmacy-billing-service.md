# Pharmacy and Billing Service: Scope and Plan

Written for: the project team, to see what the service must do, how it fits the platform, and what is still to build.
Status as of 2026-11-17. Synthetic data only.

Reference: [enterprise-app-plan.md](./enterprise-app-plan.md) (section 5.3, roadmap step 7 to 9) and
[project-structure-and-standards.md](./project-structure-and-standards.md).

---

## 1. Purpose

The service manages medicines, stock, prescriptions, and money. It is merged into one service because a prescription creates both a stock movement and a charge. It uses Clean Architecture and CQRS.

Database: `HPPharmacyDb` (SQL Server). Cloud schemas:

- `pharmacy` for the medicine catalog, stock, and prescriptions
- `billing` for invoices and payments

## 2. Features

### Pharmacy

- **Medicine catalog** with batch numbers and expiry dates
- **Stock** with stock in and stock out movements
- **Alerts** for low stock and near-expiry batches, pushed through SignalR
- **Prescriptions** linked to visits. Visit details come from the Patient service over gRPC.

### Billing

- **Event consumer:** consumes `AppointmentCompleted` from RabbitMQ and creates an invoice automatically
- **Invoices** for consultations, prescriptions, and bed stays
- **Payment recording** for cash, and for card as a simulated status

### Resilience

- **Polly retry and circuit breaker** around the gRPC call to the Patient service

### Commands and queries (CQRS)

| Kind | Operation |
|---|---|
| Command | Create prescription |
| Command | Adjust stock (stock in and out) |
| Command | Record payment |
| Query | Stock levels |
| Query | Outstanding balances |
| Query | Expiry reports |

## 3. Integration with Other Services

| Direction | Mechanism | Purpose |
|---|---|---|
| Billing and Pharmacy to Patient | gRPC (client) | Fetch visit details for prescriptions and invoices. Wrapped in Polly. |
| Patient to Billing and Pharmacy | RabbitMQ (`AppointmentCompleted`) | Trigger invoice creation without a direct call |
| Pharmacy and Billing to web app | SignalR | Live low-stock, near-expiry, and invoice-paid updates |
| Pharmacy to Analytics | gRPC (server) | Stock data for medicine usage and expiry-risk trends |

The service never reads another service's database. All data from other services arrives through these APIs.

### Event flow: appointment completed

1. A doctor marks an appointment as completed in the Patient service.
2. The Patient service saves the change and publishes `AppointmentCompleted` to RabbitMQ.
3. The Pharmacy and Billing service consumes the event.
4. It calls the Patient service over gRPC to get visit details, then creates an invoice.
5. It sends a SignalR notification so the billing dashboard updates live.

Event messages can be delivered more than once. The consumer must be idempotent: check whether an invoice already exists for the visit before creating one.

## 4. Project Layers

The service follows the four-layer structure from the plan:

| Project | Responsibility |
|---|---|
| `PharmacyBilling.Domain` | Entities (medicine, batch, stock movement, prescription, invoice, payment) and business rules |
| `PharmacyBilling.Application` | Commands, queries, handlers, and ports (repositories, gRPC client, event handling) |
| `PharmacyBilling.Infrastructure` | EF Core with SQL Server, repositories, gRPC client, RabbitMQ consumer, Polly policies |
| `PharmacyBilling.Api` | HTTP endpoints, SignalR hub, and the host |
| `PharmacyBilling.Tests` | Unit tests for domain and handlers; integration tests for the event flow |

## 5. Current Status

- Solution and five projects exist (`PharmacyBilling.slnx`). The placeholder `Class1.cs` and `UnitTest1.cs` files are removed.
- **Domain: done.** `Medicine` (aggregate root, owns batches and stock movements), `MedicineBatch`, `StockMovement`, `Prescription`, `PrescriptionLine`, `Invoice`, `InvoiceLine`, `Payment`, and their enums. Rules: no stock below zero, no expired stock received or dispensed, earliest-expiry batches dispensed first, batch expiry fixed per batch number, payments cannot exceed the balance.
- **Application: done.** Commands: register medicine, receive stock, remove stock, create prescription, record payment. Queries: stock levels, expiry report, outstanding balances. Ports: `IMedicineRepository`, `IInvoiceRepository`, `IPrescriptionRepository`, `IUnitOfWork`.
- **Tests: 30 passing** (`PharmacyBilling.slnx`): domain rules and command handlers.
- **Infrastructure: done (migration not applied).** `PharmacyBillingDbContext` with the `pharmacy` and `billing` schemas, eight entity configurations, three repositories, and DI registration. The `InitialCreate` migration creates eight tables and generates cleanly. It has not been applied to a database, because Docker is not installed and SQL Server Express (`localhost\SQLEXPRESS`) refuses connections over named pipes, shared memory, and TCP.
- **API: done.** Controllers for medicines, inventory, prescriptions, and invoices, plus ProblemDetails error mapping (including 409 for concurrent updates). Swagger UI is served at `/swagger` in Development. The health check is at `/health`. Startup works without a database.
- **Not started:** the RabbitMQ consumer, the gRPC client, and SignalR.

## 6. Build Order

1. **Done.** Domain: medicine, batch, stock movement, prescription, invoice, and payment, with their rules
2. **Done.** Application: create prescription, adjust stock, record payment, and the stock, balance, and expiry queries
3. **Done (migration not applied).** Infrastructure: `PharmacyBillingDbContext` with the `pharmacy` and `billing` schemas, repositories, and the `InitialCreate` migration
4. **Partly done.** API: endpoints for the commands and queries, and Swagger, are done. The SignalR hub for alerts is not started.
5. **Not started.** Messaging: RabbitMQ consumer for `AppointmentCompleted` with an idempotency check
6. **Not started.** gRPC client to the Patient service, wrapped in Polly retry and circuit breaker
7. **Partly done.** Unit tests for the domain rules and command handlers are passing. The end-to-end test of the event flow is not started.

Steps 5 and 6 depend on the Patient service's RabbitMQ publisher and gRPC server, which are on its roadmap.

## 7. Open Questions

- **Alert thresholds:** defaults are set in code: low stock below 20 available units, near expiry within 30 days (`InventoryAlertDefaults`). Queries can override them. Moving them to configuration is part of the API step. Confirm the defaults.
- **Invoice pricing:** medicine prices come from this service's catalog, and each prescription line keeps the price from when it was issued. Consultation and bed-stay prices are still open: a price table here, or a value carried in the event.
- **Simulated payments:** decided. Card payments are recorded with the `SimulatedApproved` status. Cash payments are recorded as `Succeeded`. No payment gateway is involved.
- **Visit and patient checks:** not yet validated. The service does not check that a visit or patient exists until the gRPC client is built.
- **Bed stays:** every invoice requires a `VisitId`. Confirm whether a bed stay belongs to a visit before the event consumer is built.
