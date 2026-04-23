using Practice.Models;

namespace Practice.Service
{
    public class BookingProcessingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public BookingProcessingBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                var pendingBookings = await bookingService.GetPendingBookingsAsync();

                foreach (var booking in pendingBookings)
                {
                    if (booking.ProcessedAt is not null)
                    {
                        continue;
                    }

                    await Task.Delay(2000, stoppingToken);

                    booking.Status = BookingStatus.Confirmed;
                    booking.ProcessedAt = DateTime.UtcNow;

                    await bookingService.UpdateBookingAsync(booking);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
