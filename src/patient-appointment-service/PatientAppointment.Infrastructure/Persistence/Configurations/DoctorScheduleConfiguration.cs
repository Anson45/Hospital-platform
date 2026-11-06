using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientAppointment.Domain.Entities;

namespace PatientAppointment.Infrastructure.Persistence.Configurations;

public sealed class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
{
    public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
    {
        builder.ToTable("doctor_schedules");

        builder.HasKey(schedule => schedule.Id);

        builder.HasIndex(schedule => new { schedule.DoctorId, schedule.DayOfWeek });
    }
}
