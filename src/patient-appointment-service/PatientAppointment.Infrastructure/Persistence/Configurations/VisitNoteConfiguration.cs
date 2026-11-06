using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Infrastructure.Persistence.Configurations;

public sealed class VisitNoteConfiguration : IEntityTypeConfiguration<VisitNote>
{
    public void Configure(EntityTypeBuilder<VisitNote> builder)
    {
        builder.ToTable("visit_notes");

        builder.HasKey(note => note.Id);

        builder.Property(note => note.Content).HasMaxLength(10000).IsRequired();

        builder.HasOne<Visit>()
            .WithMany()
            .HasForeignKey(note => note.VisitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
