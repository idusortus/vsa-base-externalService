using Api.Extensions;

namespace Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApi();        
        services.AddProblemDetails();
        return services;
    }
}