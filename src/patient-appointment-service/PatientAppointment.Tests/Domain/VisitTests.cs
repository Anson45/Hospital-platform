using PatientAppointment.Domain.Entities;
using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Tests.Domain;

public sealed class VisitTests
{
    private static readonly DateTimeOffset CheckedInAt = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void End_WhenOpen_SetsEndedAtAndClosesVisit()
    {
        var visit = new Visit(Guid.NewGuid(), Guid.NewGuid(), CheckedInAt);
        var endedAt = CheckedInAt.AddMinutes(25);

        visit.End(endedAt);

        Assert.Equal(endedAt, visit.EndedAt);
        Assert.False(visit.IsOpen);
    }

    [Fact]
    public void End_WhenAlreadyEnded_Throws()
    {
        var visit = new Visit(Guid.NewGuid(), Guid.NewGuid(), CheckedInAt);
        visit.End(CheckedInAt.AddMinutes(25));

        Assert.Throws<DomainException>(() => visit.End(CheckedInAt.AddMinutes(30)));
    }

    [Fact]
    public void End_WhenBeforeCheckIn_Throws()
    {
        var visit = new Visit(Guid.NewGuid(), Guid.NewGuid(), CheckedInAt);

        Assert.Throws<DomainException>(() => visit.End(CheckedInAt.AddMinutes(-5)));
    }
}
