using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients");

        builder.HasKey(patient => patient.Id);

        builder.Property(patient => patient.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(patient => patient.LastName).HasMaxLength(100).IsRequired();
        builder.Property(patient => patient.Email).HasMaxLength(256);
        builder.Property(patient => patient.PhoneNumber).HasMaxLength(32);
        builder.Property(patient => patient.MedicalHistorySummary).HasMaxLength(4000);
    }
}
