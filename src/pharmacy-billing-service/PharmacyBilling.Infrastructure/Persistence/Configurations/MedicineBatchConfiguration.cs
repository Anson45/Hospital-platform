using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Infrastructure.Persistence.Configurations;

public sealed class MedicineBatchConfiguration : IEntityTypeConfiguration<MedicineBatch>
{
    public void Configure(EntityTypeBuilder<MedicineBatch> builder)
    {
        builder.ToTable(
            "medicine_batches",
            PharmacyBillingDbContext.PharmacySchema,
            table => table.HasCheckConstraint("CK_medicine_batches_quantity_non_negative", "[QuantityOnHand] >= 0"));

        builder.HasKey(batch => batch.Id);

        builder.Property(batch => batch.BatchNumber).HasMaxLength(50).IsRequired();

        // A batch number is unique within one medicine, not across the catalog.
        builder.HasIndex(batch => new { batch.MedicineId, batch.BatchNumber }).IsUnique();

        // Dispensing sorts by expiry, so this index serves that query.
        builder.HasIndex(batch => batch.ExpiryDate);
    }
}
