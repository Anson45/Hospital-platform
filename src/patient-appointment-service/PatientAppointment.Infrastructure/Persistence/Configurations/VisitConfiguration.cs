using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Infrastructure.Persistence.Configurations;

public sealed class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("visits");

        builder.HasKey(visit => visit.Id);

        // One visit per appointment: a second check-in for the same booking is rejected by the database.
        builder.HasIndex(visit => visit.AppointmentId).IsUnique();

        builder.HasOne<Appointment>()
            .WithMany()
            .HasForeignKey(visit => visit.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(visit => visit.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
