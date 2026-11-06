using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PatientAppointment.Application.Abstractions;
using PatientAppointment.Infrastructure.Persistence;
using PatientAppointment.Infrastructure.Persistence.Repositories;

namespace PatientAppointment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PatientAppointmentDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<PatientAppointmentDbContext>());
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();

        return services;
    }
}
