using Practice.Models;

namespace Practice.Service
{
    public class BookingProcessingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingProcessingBackgroundService> _logger;
        private readonly SemaphoreSlim _processSemaphore = new(1, 1);

        public BookingProcessingBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BookingProcessingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                var pendingBookings = await bookingService.GetPendingBookingsAsync();

                var tasks = pendingBookings
                    .Where(booking => booking.ProcessedAt is null)
                    .Select(booking => ProcessBookingAsync(booking, bookingService, eventService, stoppingToken));

                await Task.WhenAll(tasks);

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessBookingAsync(
            Booking booking,
            IBookingService bookingService,
            IEventService eventService,
            CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(2000, stoppingToken);

                await _processSemaphore.WaitAsync(stoppingToken);

                try
                {
                    var evt = eventService.GetById(booking.EventId);

                    if (evt is null)
                    {
                        booking.Reject();
                        await bookingService.UpdateBookingAsync(booking);

                        _logger.LogWarning(
                            "Booking {BookingId} rejected because event {EventId} was not found.",
                            booking.Id,
                            booking.EventId);

                        return;
                    }

                    booking.Confirm();
                    await bookingService.UpdateBookingAsync(booking);
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
            catch (Exception ex)
            {
                var evt = eventService.GetById(booking.EventId);

                booking.Reject();

                if (evt is not null)
                {
                    eventService.ReleaseSeats(evt.Id);
                    eventService.Update(evt);
                }

                await bookingService.UpdateBookingAsync(booking);

                _logger.LogError(
                    ex,
                    "Unexpected error while processing booking {BookingId}. Booking was rejected.",
                    booking.Id);
            }
        }
    }
}
