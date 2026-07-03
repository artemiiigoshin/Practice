using Microsoft.EntityFrameworkCore;
using Practice.DataAccess;
using Practice.Models;
using Practice.Repositories;

namespace Practice.Service
{
    public class BookingService(
    IBookingRepository bookingRepository,
    IEventRepository eventRepository) : IBookingService
    {
        private readonly IBookingRepository _bookingRepository = bookingRepository;
        private readonly IEventRepository _eventRepository = eventRepository;

        private static readonly SemaphoreSlim BookingSemaphore = new(1, 1);

        public async Task<Booking> CreateBookingAsync(Guid eventId)
        {
            await BookingSemaphore.WaitAsync();

            try
            {
                var evt = await _eventRepository.GetByIdAsync(eventId);
                if (evt is null)
                    throw new InvalidOperationException("Event not found");

                var reserved = EventSeatManager.TryReserveSeats(evt);

                if (!reserved)
                    throw new NoAvailableSeatsException();

                var booking = new Booking
                (
                    Guid.NewGuid(),
                    eventId,
                    BookingStatus.Pending,
                    DateTime.UtcNow,
                    null
                );

                _bookingRepository.Add(booking);

                await _bookingRepository.SaveChangesAsync();

                return booking;
            }
            finally
            {
                BookingSemaphore.Release();
            }
        }

        public Task<Booking?> GetBookingByIdAsync(Guid bookingId) => _bookingRepository.GetByIdAsync(bookingId);

        public Task<List<Booking>> GetPendingBookingsAsync()
        {
            return _bookingRepository.GetPendingBookingsAsync();
        }

        public async Task UpdateBookingAsync(Booking booking)
        {
            var existingBooking = await _bookingRepository.GetByIdAsync(booking.Id);
            if (existingBooking is null)
            {
                return;
            }

            existingBooking.EventId = booking.EventId;
            existingBooking.Status = booking.Status;
            existingBooking.CreatedAt = booking.CreatedAt;
            existingBooking.ProcessedAt = booking.ProcessedAt;

            await _bookingRepository.SaveChangesAsync();
        }

        public async Task ProcessBookingAsync(Booking booking, CancellationToken cancellationToken)
        {
            booking.Status = BookingStatus.Confirmed;
            booking.ProcessedAt = DateTime.UtcNow;

            await _bookingRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
