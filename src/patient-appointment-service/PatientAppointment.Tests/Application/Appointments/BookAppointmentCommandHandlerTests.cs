using NSubstitute;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Appointments;
using PatientAppointment.Application.Exceptions;
using PatientAppointment.Domain.Entities;
using PatientAppointment.Domain.Enums;
using PatientAppointment.Domain.Exceptions;

namespace PatientAppointment.Tests.Application.Appointments;

public sealed class BookAppointmentCommandHandlerTests
{
    // 2026-11-09 is a Monday.
    private static readonly DateTimeOffset MondayAtNine = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan HalfHour = TimeSpan.FromMinutes(30);

    private readonly IPatientRepository _patients = Substitute.For<IPatientRepository>();
    private readonly IDoctorRepository _doctors = Substitute.For<IDoctorRepository>();
    private readonly IAppointmentRepository _appointments = Substitute.For<IAppointmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly BookAppointmentCommandHandler _handler;

    public BookAppointmentCommandHandlerTests()
    {
        _handler = new BookAppointmentCommandHandler(_patients, _doctors, _appointments, _unitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenSlotIsFree_AddsScheduledAppointment()
    {
        var patient = CreatePatient();
        var doctor = CreateDoctorWorkingMondayMorning();
        SetupPatient(patient);
        SetupDoctor(doctor);
        _appointments.HasOverlapAsync(doctor.Id, MondayAtNine, MondayAtNine + HalfHour, Arg.Any<CancellationToken>())
            .Returns(false);

        var appointmentId = await _handler.HandleAsync(
            new BookAppointmentCommand(patient.Id, doctor.Id, MondayAtNine, HalfHour));

        Assert.NotEqual(Guid.Empty, appointmentId);
        _appointments.Received(1).Add(Arg.Is<Appointment>(
            appointment => appointment.Id == appointmentId && appointment.Status == AppointmentStatus.Scheduled));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPatientDoesNotExist_ThrowsNotFound()
    {
        var patientId = Guid.NewGuid();
        _patients.GetByIdAsync(patientId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Patient?>(null));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(new BookAppointmentCommand(patientId, Guid.NewGuid(), MondayAtNine, HalfHour)));
    }

    [Fact]
    public async Task HandleAsync_WhenDoctorIsNotWorkingAtRequestedTime_ThrowsDomainException()
    {
        var patient = CreatePatient();
        var doctor = CreateDoctorWorkingMondayMorning();
        SetupPatient(patient);
        SetupDoctor(doctor);
        var thirteenHundred = MondayAtNine.AddHours(4);

        await Assert.ThrowsAsync<DomainException>(() =>
            _handler.HandleAsync(new BookAppointmentCommand(patient.Id, doctor.Id, thirteenHundred, HalfHour)));
        _appointments.DidNotReceive().Add(Arg.Any<Appointment>());
    }

    [Fact]
    public async Task HandleAsync_WhenSlotOverlapsExistingAppointment_ThrowsDomainException()
    {
        var patient = CreatePatient();
        var doctor = CreateDoctorWorkingMondayMorning();
        SetupPatient(patient);
        SetupDoctor(doctor);
        _appointments.HasOverlapAsync(doctor.Id, MondayAtNine, MondayAtNine + HalfHour, Arg.Any<CancellationToken>())
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() =>
            _handler.HandleAsync(new BookAppointmentCommand(patient.Id, doctor.Id, MondayAtNine, HalfHour)));
        _appointments.DidNotReceive().Add(Arg.Any<Appointment>());
    }

    private void SetupPatient(Patient patient)
    {
        _patients.GetByIdAsync(patient.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Patient?>(patient));
    }

    private void SetupDoctor(Doctor doctor)
    {
        _doctors.GetByIdAsync(doctor.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Doctor?>(doctor));
    }

    private static Patient CreatePatient()
    {
        return new Patient("Ada", "Lovelace", new DateOnly(1990, 1, 1), DateTimeOffset.UtcNow);
    }

    private static Doctor CreateDoctorWorkingMondayMorning()
    {
        var doctor = new Doctor(Guid.NewGuid(), Guid.NewGuid());
        doctor.AddSchedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        return doctor;
    }
}
