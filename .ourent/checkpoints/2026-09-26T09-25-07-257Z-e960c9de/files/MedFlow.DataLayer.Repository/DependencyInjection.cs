using MedFlow.Application.Common.Interfaces;
using MedFlow.DataLayer.Repository.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedFlow.DataLayer.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddDataLayerRepository(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default topilmadi.");

        services.AddDbContext<MedFlowDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
