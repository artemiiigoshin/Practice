using Microsoft.EntityFrameworkCore;
using Practice.DataAccess;
using Practice.Models;

namespace Practice.Service
{
    public class BookingProcessingBackgroundService(IServiceScopeFactory scopeFactory,
            ILogger<BookingProcessingBackgroundService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<BookingProcessingBackgroundService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<Guid> pendingBookings;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    pendingBookings = await context.Bookings
                        .Where(x => x.Status == BookingStatus.Pending)
                        .Select(x => x.Id)
                        .ToListAsync(stoppingToken);
                }
                var tasks = pendingBookings.Select(id => ProcessBookingAsync(id, stoppingToken));

                await Task.WhenAll(tasks);

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessBookingAsync(
            Guid bookingId,
            CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                await Task.Delay(2000, stoppingToken);

                var booking = await context.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, stoppingToken);

                if (booking is null)
                {
                    return;
                }

                var evt = await context.Events.FirstOrDefaultAsync(evt => evt.Id == booking.EventId, stoppingToken);

                if (evt is null)
                {
                    booking.Reject();

                    await context.SaveChangesAsync(stoppingToken);

                    _logger.LogWarning(
                        "Booking {BookingId} rejected because event {EventId} was not found.",
                        booking.Id,
                        booking.EventId);

                    return;
                }

                booking.Confirm();

                await context.SaveChangesAsync(stoppingToken);       
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var booking = await context.Bookings
                                   .FirstOrDefaultAsync(booking => booking.Id == bookingId, CancellationToken.None);

                if (booking is not null)
                {
                    booking.Reject();

                    var evt = await context.Events
                        .FirstOrDefaultAsync(evt => evt.Id == booking.EventId, CancellationToken.None);

                    if (evt is not null)
                    {
                        EventSeatManager.ReleaseSeats(evt);
                    }

                    await context.SaveChangesAsync(CancellationToken.None);
                }

                _logger.LogError(
                    ex,
                    "Unexpected error while processing booking {BookingId}. Booking was rejected.",
                    bookingId);
            }
        }
    }
}
