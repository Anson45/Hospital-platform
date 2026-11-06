using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Domain.Entities;

/// <summary>
/// Owns a working schedule. Name and login live in the Identity service, so only the staff reference is stored here.
/// </summary>
public class Doctor
{
    private readonly List<DoctorSchedule> _schedules = [];

    // EF Core materializes rows through this constructor.
    private Doctor()
    {
    }

    public Doctor(Guid staffUserId, Guid departmentId)
    {
        Id = Guid.NewGuid();
        StaffUserId = staffUserId;
        DepartmentId = departmentId;
    }

    public Guid Id { get; }

    /// <summary>
    /// Reference to the user in the Identity service.
    /// </summary>
    public Guid StaffUserId { get; }

    public Guid DepartmentId { get; private set; }

    public IReadOnlyCollection<DoctorSchedule> Schedules => _schedules;

    /// <summary>
    /// Adds a working block. Blocks on the same day must not overlap, so each slot has one owner.
    /// </summary>
    public void AddSchedule(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        var schedule = new DoctorSchedule(Id, dayOfWeek, startTime, endTime);

        if (_schedules.Any(existing => existing.Overlaps(schedule)))
        {
            throw new DomainException("The schedule overlaps an existing working block for this doctor.");
        }

        _schedules.Add(schedule);
    }

    /// <summary>
    /// True when a single working block covers the whole appointment. A slot that crosses midnight is never covered.
    /// </summary>
    public bool IsWorkingAt(DateTimeOffset start, TimeSpan duration)
    {
        var startTime = TimeOnly.FromDateTime(start.DateTime);
        var endTime = startTime.Add(duration);

        if (endTime < startTime)
        {
            return false;
        }

        return _schedules.Any(schedule =>
            schedule.DayOfWeek == start.DayOfWeek
            && schedule.StartTime <= startTime
            && endTime <= schedule.EndTime);
    }

    public void MoveToDepartment(Guid departmentId)
    {
        DepartmentId = departmentId;
    }
}
