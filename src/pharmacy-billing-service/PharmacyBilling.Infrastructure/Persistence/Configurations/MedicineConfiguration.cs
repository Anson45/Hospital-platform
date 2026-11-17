using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Infrastructure.Persistence.Configurations;

public sealed class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable("medicines", PharmacyBillingDbContext.PharmacySchema);

        builder.HasKey(medicine => medicine.Id);

        builder.Property(medicine => medicine.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(medicine => medicine.Code).IsUnique();

        builder.Property(medicine => medicine.Name).HasMaxLength(200).IsRequired();
        builder.Property(medicine => medicine.UnitPrice).HasPrecision(18, 2);

        // Stock is changed by several requests at once, so concurrent edits to the same medicine must be detected.
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.Ignore(medicine => medicine.TotalQuantityOnHand);

        builder.HasMany(medicine => medicine.Batches)
            .WithOne()
            .HasForeignKey(batch => batch.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(medicine => medicine.Batches)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(medicine => medicine.Movements)
            .WithOne()
            .HasForeignKey(movement => movement.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(medicine => medicine.Movements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
