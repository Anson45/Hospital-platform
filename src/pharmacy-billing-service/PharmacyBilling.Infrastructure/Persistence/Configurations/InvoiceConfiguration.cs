using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices", PharmacyBillingDbContext.BillingSchema);

        builder.HasKey(invoice => invoice.Id);

        // One invoice per visit. The prescription flow and the future event consumer both rely on this.
        builder.HasIndex(invoice => invoice.VisitId).IsUnique();
        builder.HasIndex(invoice => invoice.PatientId);

        // Payments can arrive together, so concurrent payments must not both pass the balance check.
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.Ignore(invoice => invoice.TotalAmount);
        builder.Ignore(invoice => invoice.PaidAmount);
        builder.Ignore(invoice => invoice.BalanceDue);
        builder.Ignore(invoice => invoice.Status);

        builder.HasMany(invoice => invoice.Lines)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(invoice => invoice.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(invoice => invoice.Payments)
            .WithOne()
            .HasForeignKey(payment => payment.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(invoice => invoice.Payments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
