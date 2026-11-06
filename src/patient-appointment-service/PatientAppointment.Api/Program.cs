using PatientAppointment.Api.ExceptionHandling;
using PatientAppointment.Api.Requests;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Application.Appointments;
using PatientAppointment.Application.Departments;
using PatientAppointment.Application.Doctors;
using PatientAppointment.Application.Patients;
using PatientAppointment.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();
builder.Services.AddHealthChecks();

// Handlers read the current time through this abstraction so tests can control it.
builder.Services.AddSingleton(TimeProvider.System);

var connectionString = builder.Configuration.GetConnectionString("PatientAppointment")
    ?? throw new InvalidOperationException("Connection string 'PatientAppointment' is not configured.");
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddScoped<ICommandHandler<RegisterPatientCommand, Guid>, RegisterPatientCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CreateDepartmentCommand, Guid>, CreateDepartmentCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CreateDoctorCommand, Guid>, CreateDoctorCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AddDoctorScheduleCommand>, AddDoctorScheduleCommandHandler>();
builder.Services.AddScoped<ICommandHandler<BookAppointmentCommand, Guid>, BookAppointmentCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CheckInAppointmentCommand, Guid>, CheckInAppointmentCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CompleteAppointmentCommand>, CompleteAppointmentCommandHandler>();
builder.Services.AddScoped<
    IQueryHandler<GetPatientAppointmentsQuery, IReadOnlyList<AppointmentDto>>,
    GetPatientAppointmentsQueryHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
