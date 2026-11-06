using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Infrastructure.Persistence.Configurations;

public sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("doctors");

        builder.HasKey(doctor => doctor.Id);

        builder.Property(doctor => doctor.StaffUserId).IsRequired();
        builder.HasIndex(doctor => doctor.StaffUserId).IsUnique();

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(doctor => doctor.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // The aggregate exposes schedules read-only, so EF writes through the backing field.
        builder.HasMany(doctor => doctor.Schedules)
            .WithOne()
            .HasForeignKey(schedule => schedule.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(doctor => doctor.Schedules)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
