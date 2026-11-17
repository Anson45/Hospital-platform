using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Infrastructure.Persistence;
using PharmacyBilling.Infrastructure.Persistence.Repositories;

namespace PharmacyBilling.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PharmacyBillingDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<PharmacyBillingDbContext>());
        services.AddScoped<IMedicineRepository, MedicineRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();

        return services;
    }
}
