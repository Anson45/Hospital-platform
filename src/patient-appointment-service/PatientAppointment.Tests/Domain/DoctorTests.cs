using PatientAppointment.Domain.Entities;
using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Tests.Domain;

public sealed class DoctorTests
{
    // 2026-11-09 is a Monday.
    private static readonly DateTimeOffset MondayAtTen = new(2026, 11, 9, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddSchedule_WhenBlocksOverlap_Throws()
    {
        var doctor = new Doctor(Guid.NewGuid(), Guid.NewGuid());
        doctor.AddSchedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));

        Assert.Throws<DomainException>(
            () => doctor.AddSchedule(DayOfWeek.Monday, new TimeOnly(11, 0), new TimeOnly(13, 0)));
    }

    [Fact]
    public void AddSchedule_WhenBlocksAreAdjacent_Succeeds()
    {
        var doctor = new Doctor(Guid.NewGuid(), Guid.NewGuid());
        doctor.AddSchedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));

        doctor.AddSchedule(DayOfWeek.Monday, new TimeOnly(12, 0), new TimeOnly(17, 0));

        Assert.Equal(2, doctor.Schedules.Count);
    }

    [Fact]
    public void IsWorkingAt_WhenInsideBlock_ReturnsTrue()
    {
        var doctor = CreateDoctorWorkingMondayMorning();

        Assert.True(doctor.IsWorkingAt(MondayAtTen, TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void IsWorkingAt_WhenRunsPastBlockEnd_ReturnsFalse()
    {
        var doctor = CreateDoctorWorkingMondayMorning();
        var elevenFortyFive = MondayAtTen.AddHours(1).AddMinutes(45);

        Assert.False(doctor.IsWorkingAt(elevenFortyFive, TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void IsWorkingAt_WhenDifferentDay_ReturnsFalse()
    {
        var doctor = CreateDoctorWorkingMondayMorning();

        Assert.False(doctor.IsWorkingAt(MondayAtTen.AddDays(1), TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void IsWorkingAt_WhenSlotCrossesMidnight_ReturnsFalse()
    {
        var doctor = new Doctor(Guid.NewGuid(), Guid.NewGuid());
        doctor.AddSchedule(DayOfWeek.Monday, new TimeOnly(23, 0), TimeOnly.MaxValue);
        var justBeforeMidnight = new DateTimeOffset(2026, 11, 9, 23, 50, 0, TimeSpan.Zero);

        Assert.False(doctor.IsWorkingAt(justBeforeMidnight, TimeSpan.FromMinutes(30)));
    }

    private static Doctor CreateDoctorWorkingMondayMorning()
    {
        var doctor = new Doctor(Guid.NewGuid(), Guid.NewGuid());
        doctor.AddSchedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        return doctor;
    }
}
