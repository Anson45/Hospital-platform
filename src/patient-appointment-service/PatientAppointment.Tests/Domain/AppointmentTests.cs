using PatientAppointment.Domain.Entities;
using PatientAppointment.Domain.Enums;
using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Tests.Domain;

public sealed class AppointmentTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Duration = TimeSpan.FromMinutes(30);

    [Fact]
    public void Constructor_SetsScheduledEndFromStartAndDuration()
    {
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Start, Duration);

        Assert.Equal(Start + Duration, appointment.ScheduledEnd);
    }

    [Fact]
    public void Constructor_WhenDurationIsZero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Appointment(Guid.NewGuid(), Guid.NewGuid(), Start, TimeSpan.Zero));
    }

    [Fact]
    public void Reschedule_WhenScheduled_MovesStartAndEnd()
    {
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Start, Duration);
        var newStart = Start.AddDays(1);

        appointment.Reschedule(newStart);

        Assert.Equal(newStart, appointment.ScheduledStart);
        Assert.Equal(newStart + Duration, appointment.ScheduledEnd);
    }

    [Fact]
    public void CheckIn_WhenAlreadyCheckedIn_Throws()
    {
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Start, Duration);
        appointment.CheckIn();

        Assert.Throws<DomainException>(() => appointment.CheckIn());
    }

    [Fact]
    public void Complete_WhenStillScheduled_Throws()
    {
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Start, Duration);

        Assert.Throws<DomainException>(() => appointment.Complete());
    }

    [Fact]
    public void Complete_WhenCheckedIn_SetsCompleted()
    {
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Start, Duration);
        appointment.CheckIn();

        appointment.Complete();

        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
    }

    [Fact]
    public void Cancel_WhenCheckedIn_Throws()
    {
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), Start, Duration);
        appointment.CheckIn();

        Assert.Throws<DomainException>(() => appointment.Cancel());
    }
}
