using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Infrastructure.Persistence.Configurations;

public sealed class PrescriptionLineConfiguration : IEntityTypeConfiguration<PrescriptionLine>
{
    public void Configure(EntityTypeBuilder<PrescriptionLine> builder)
    {
        builder.ToTable(
            "prescription_lines",
            PharmacyBillingDbContext.PharmacySchema,
            table => table.HasCheckConstraint("CK_prescription_lines_quantity_positive", "[Quantity] > 0"));

        builder.HasKey(line => line.Id);

        builder.Property(line => line.MedicineName).HasMaxLength(200).IsRequired();
        builder.Property(line => line.UnitPrice).HasPrecision(18, 2);

        builder.Ignore(line => line.LineTotal);
    }
}
