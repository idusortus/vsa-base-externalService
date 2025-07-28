using Application.Abstractions;
using Infrastructure.Database;
// using Infrastructure.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddDatabase(configuration);

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("localdb");

        services.AddDbContext<AppDbContext>(
            options => options.UseSqlServer(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }

    // private static IServiceCollection AddExternalApi(this IServiceCollection services, IConfiguration configuration)
    // {
    //     services.AddHttpClient<IDogApiService, DogApiService>(client =>
    //     {
    //         client.BaseAddress = new Uri("https://dogapi.dog/docs/api-v2");     
    //     });

    //     return services;
    // }
}