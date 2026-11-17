using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Infrastructure.Persistence.Configurations;

public sealed class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("prescriptions", PharmacyBillingDbContext.PharmacySchema);

        builder.HasKey(prescription => prescription.Id);

        // A visit can have several prescriptions, so this index is not unique.
        builder.HasIndex(prescription => prescription.VisitId);

        builder.Ignore(prescription => prescription.TotalAmount);

        // The line's foreign key is a shadow property: the domain model does not need to know about it.
        builder.HasMany(prescription => prescription.Lines)
            .WithOne()
            .HasForeignKey("PrescriptionId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(prescription => prescription.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
