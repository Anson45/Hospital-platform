using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Domain.Entities;

/// <summary>
/// One working block for a doctor on a given weekday.
/// </summary>
public class DoctorSchedule
{
    // EF Core materializes rows through this constructor.
    private DoctorSchedule()
    {
    }

    public DoctorSchedule(Guid doctorId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
        {
            throw new DomainException("A schedule block must end after it starts.");
        }

        Id = Guid.NewGuid();
        DoctorId = doctorId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    public Guid Id { get; }

    public Guid DoctorId { get; }

    public DayOfWeek DayOfWeek { get; }

    public TimeOnly StartTime { get; }

    public TimeOnly EndTime { get; }

    public bool Overlaps(DoctorSchedule other)
    {
        return DayOfWeek == other.DayOfWeek
            && StartTime < other.EndTime
            && other.StartTime < EndTime;
    }
}
