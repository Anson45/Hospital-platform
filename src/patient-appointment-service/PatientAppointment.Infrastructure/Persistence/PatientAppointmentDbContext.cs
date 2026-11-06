using Microsoft.EntityFrameworkCore;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Infrastructure.Persistence;

/// <summary>
/// Owns the clinical schema. Other services never read this database; they integrate through APIs and events.
/// </summary>
public class PatientAppointmentDbContext : DbContext, IUnitOfWork
{
    public const string SchemaName = "clinical";

    public PatientAppointmentDbContext(DbContextOptions<PatientAppointmentDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Doctor> Doctors => Set<Doctor>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<Visit> Visits => Set<Visit>();

    public DbSet<VisitNote> VisitNotes => Set<VisitNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PatientAppointmentDbContext).Assembly);
    }
}
