using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Infrastructure.Persistence.Configurations;

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable(
            "stock_movements",
            PharmacyBillingDbContext.PharmacySchema,
            table => table.HasCheckConstraint("CK_stock_movements_quantity_positive", "[Quantity] > 0"));

        builder.HasKey(movement => movement.Id);

        builder.Property(movement => movement.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(movement => movement.Reason).HasMaxLength(500);

        builder.HasOne<MedicineBatch>()
            .WithMany()
            .HasForeignKey(movement => movement.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(movement => new { movement.MedicineId, movement.OccurredAt });
    }
}
