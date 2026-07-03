using Microsoft.Extensions.DependencyInjection;
using Practice.Application.Service;

namespace Practice.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
}