using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Infrastructure.Persistence.Configurations;

public sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("invoice_lines", PharmacyBillingDbContext.BillingSchema);

        builder.HasKey(line => line.Id);

        builder.Property(line => line.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(line => line.Description).HasMaxLength(200).IsRequired();
        builder.Property(line => line.Amount).HasPrecision(18, 2);
    }
}
