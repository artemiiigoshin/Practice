using Practice.Service;

namespace Practice.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IEventService, EventService>();
            services.AddSingleton<IBookingService, BookingService>();

            services.AddHostedService<BookingProcessingBackgroundService>();

            return services;
        }
    }
}
