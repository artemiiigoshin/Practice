using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Practice.Application.Repositories;
using Practice.Domain.Models;

namespace Practice.Infrastructure.BackgroundServices
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
                List<Booking> pendingBookings;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                    pendingBookings = await bookingRepository.GetPendingBookingsAsync(stoppingToken);
                }
                var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));

                await Task.WhenAll(tasks);

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessBookingAsync(
            Booking booking,
            CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            try
            {
                await Task.Delay(2000, stoppingToken);

                var evt = await eventRepository.GetByIdAsync(booking.EventId);

                if (evt is null)
                {
                    booking.Reject();

                    await bookingRepository.SaveChangesAsync(stoppingToken);

                    _logger.LogWarning(
                        "Booking {BookingId} rejected because event {EventId} was not found.",
                        booking.Id,
                        booking.EventId);

                    return;
                }

                booking.Confirm();

                await bookingRepository.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var bookingToReject = await bookingRepository.GetByIdAsync(booking.Id, CancellationToken.None);
                if (bookingToReject is not null)
                {
                    bookingToReject.Reject();

                    var evt = await eventRepository.GetByIdAsync(bookingToReject.EventId);

                    if (evt is not null)
                    {
                        EventSeatManager.ReleaseSeats(evt);
                    }

                    await bookingRepository.SaveChangesAsync(CancellationToken.None);
                }

                _logger.LogError(
                    ex,
                    "Unexpected error while processing booking {BookingId}. Booking was rejected.",
                    booking.Id);
            }
        }
    }
}
