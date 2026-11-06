using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");

        builder.HasKey(appointment => appointment.Id);

        builder.Property(appointment => appointment.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Serves the doctor overlap check and the patient appointment list.
        builder.HasIndex(appointment => new { appointment.DoctorId, appointment.ScheduledStart });
        builder.HasIndex(appointment => appointment.PatientId);

        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(appointment => appointment.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Doctor>()
            .WithMany()
            .HasForeignKey(appointment => appointment.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
