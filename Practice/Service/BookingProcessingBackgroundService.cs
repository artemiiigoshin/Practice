using Practice.Models;

namespace Practice.Service
{
    public class BookingProcessingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SemaphoreSlim _processSemaphore = new(1, 1);

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

                var tasks = pendingBookings
                    .Where(booking => booking.ProcessedAt is null)
                    .Select(booking => ProcessBookingAsync(booking, bookingService, stoppingToken));

                await Task.WhenAll(tasks);

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessBookingAsync(
            Booking booking,
            IBookingService bookingService,
            CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(2000, stoppingToken);

                await _processSemaphore.WaitAsync(stoppingToken);

                try
                {
                    await bookingService.ProcessBookingAsync(booking, stoppingToken);
                }
                finally
                {
                    _processSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }
    }
}
