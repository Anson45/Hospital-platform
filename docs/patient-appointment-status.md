# PatientAppointment Service: Status and Plan

Written for: the project team, to see what is done, what is left, and what comes next.
Status as of 2026-11-06. Synthetic data only.

Reference: [enterprise-app-plan.md](./enterprise-app-plan.md) (roadmap, section 13) and
[project-structure-and-standards.md](./project-structure-and-standards.md).

---

## 1. Completed

### Domain (`PatientAppointment.Domain`)

| Type | Purpose |
|---|---|
| `Patient` | Demographics, contact, medical history summary |
| `Department` | Groups doctors (for example, Cardiology) |
| `Doctor` | Owns its schedule; stores only the Identity service's staff id and department |
| `DoctorSchedule` | One working block per weekday; blocks on the same day cannot overlap |
| `Appointment` | Booking with a status lifecycle; stores `ScheduledEnd` for overlap checks |
| `Visit` | Opened at check-in, closed at completion; one open visit per appointment |
| `VisitNote` | Clinical note written by a doctor during a visit |
| `AppointmentStatus` | Scheduled, CheckedIn, Completed, Cancelled, NoShow |
| `DomainException` | Business-rule violations |

Business rules in place:

- Appointment transitions: Scheduled to CheckedIn, Cancelled, or NoShow; CheckedIn to Completed. Anything else is rejected.
- A booking must fall inside one of the doctor's working blocks, and may not cross midnight.
- A doctor cannot have two live (Scheduled or CheckedIn) appointments that overlap.
- A visit cannot end before it started, and cannot end twice.

### Application (`PatientAppointment.Application`)

- Ports: `IPatientRepository`, `IDepartmentRepository`, `IDoctorRepository`, `IAppointmentRepository`, `IVisitRepository`, `IUnitOfWork`
- Handler interfaces: `ICommandHandler`, `IQueryHandler`
- Use cases:

| Use case | Type |
|---|---|
| Register patient | Command |
| Create department | Command |
| Create doctor | Command |
| Add doctor schedule | Command |
| Book appointment | Command |
| Check in appointment (opens the visit) | Command |
| Complete appointment (closes the visit) | Command |
| Get patient appointments | Query |

- `NotFoundException` for unknown ids.

### Infrastructure (`PatientAppointment.Infrastructure`)

- `PatientAppointmentDbContext` with schema `clinical` (PostgreSQL, Npgsql).
- Seven entity configurations, five repositories, and dependency-injection registration.
- Initial migration `InitialCreate` (six tables, foreign keys, and indexes). Generated but not yet applied to a database.

### Api (`PatientAppointment.Api`)

| Method | Route | Result |
|---|---|---|
| POST | `/api/patients` | 201, patient id |
| GET | `/api/patients/{patientId}/appointments` | 200, list |
| POST | `/api/departments` | 201, department id |
| POST | `/api/doctors` | 201, doctor id |
| POST | `/api/doctors/{doctorId}/schedules` | 204 |
| POST | `/api/appointments` | 201, appointment id |
| POST | `/api/appointments/{appointmentId}/check-in` | 201, visit id |
| POST | `/api/appointments/{appointmentId}/complete` | 204 |
| GET | `/health` | 200 |

- Error mapping: not found returns 404, business-rule violations return 422, invalid arguments return 400. All use ProblemDetails.
- The weather template has been removed.

### Tests (`PatientAppointment.Tests`)

- 20 tests, all passing: 16 domain tests and 4 booking-handler tests.
- The solution builds with 0 warnings.

### Housekeeping

- Empty placeholder `Class1.cs` files removed from Domain, Application, and Infrastructure.
- The connection string has no password in source. The password is set with user-secrets.

---

## 2. Not Done Yet

### Must do before the service is usable

1. **Apply the migration** to a local PostgreSQL database (`dotnet ef database update`). Needs Docker and the connection-string password.
2. **Manual end-to-end run** of the flow: register patient, create department, create doctor, add schedule, book, check in, complete, list appointments.
3. **Integration tests** against a real PostgreSQL container (Testcontainers), as the plan's testing section requires.
4. **Authentication and authorization.** All endpoints are currently open. This is the biggest gap. It depends on the Identity service and the Gateway, which are not built yet.

### Should do soon

5. **Database-level overlap protection.** The overlap check runs in application code, so two concurrent bookings for the same slot could both pass. Options: a serializable transaction, or a database constraint.
6. **Tests for the other handlers.** Register patient, check-in, complete, create doctor, and the list query have no tests yet.
7. **Time-zone rule.** Working hours are compared against the local time in the booking's offset. Decide how clinics in different time zones are handled.
8. **Cancel, reschedule, and no-show endpoints.** The domain methods exist; the use cases and routes do not.
9. **Input validation** beyond the domain rules (for example, date of birth cannot be in the future).
10. **Logging.** Serilog is planned but not added.
11. **Health check** only reports that the app is running; it does not check the database.

### Later (per the roadmap)

12. **VisitNote endpoints.** Create and read notes during a visit. The AI summary comes later and must stay draft-only.
13. **Read endpoints** for patients, doctors, and departments (get, list, update contact details, update medical history summary).
14. **Role-based access.** Doctors see only their own patients; receptionists book; nurses check in. This needs the Identity roles.
15. **Audit log** for viewing and changing patient records (security checklist).
16. **Events:** publish `PatientRegistered` and `AppointmentCompleted` through RabbitMQ and MassTransit (roadmap week 8).
17. **gRPC server** that returns visit details to the Billing service (week 9).
18. **Redis caching** for schedule and department lists (week 5).
19. **SignalR** live updates for new appointments and check-ins (week 11).

---

## 3. Plan

### Immediate (next session)

1. Start the local infrastructure and apply the migration.
2. Run the end-to-end flow by hand and save the requests in a `.http` file.
3. Add Testcontainers integration tests for the booking and check-in flows.
4. Add the database-level overlap protection (item 5).

### Near term

5. Build the Identity service (roadmap week 2): ASP.NET Core Identity, JWT, roles. Its staff ids are what `Doctor.StaffUserId` refers to.
6. Build the Gateway with YARP (week 3) and protect the endpoints with JWT validation.
7. Finish the remaining appointment use cases (cancel, reschedule, no-show) and VisitNote endpoints.

### Later

8. Events with RabbitMQ and MassTransit (week 8), so Billing can create invoices from `AppointmentCompleted`.
9. gRPC for visit details (week 9), Redis caching (week 5 scope, deferred), and SignalR (week 11).

### Open question

The roadmap puts Identity and the Gateway before the Patient service. The Patient service was built first, so its endpoints are open until Identity and the Gateway exist. Decide whether to add temporary protection now or accept open endpoints for local development only.
