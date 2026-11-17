using Microsoft.EntityFrameworkCore;
using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Infrastructure.Persistence;

/// <summary>
/// Owns the pharmacy and billing schemas in SQL Server. Other services never read this database.
/// </summary>
public class PharmacyBillingDbContext : DbContext, IUnitOfWork
{
    public const string PharmacySchema = "pharmacy";
    public const string BillingSchema = "billing";

    public PharmacyBillingDbContext(DbContextOptions<PharmacyBillingDbContext> options) : base(options)
    {
    }

    public DbSet<Medicine> Medicines => Set<Medicine>();

    public DbSet<Prescription> Prescriptions => Set<Prescription>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PharmacyBillingDbContext).Assembly);
    }
}
