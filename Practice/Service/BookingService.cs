using Microsoft.EntityFrameworkCore;
using Practice.DataAccess;
using Practice.Models;

namespace Practice.Service
{
    public class BookingService(AppDbContext context, IEventService eventService) : IBookingService
    {
        private readonly AppDbContext _context = context;

        private static readonly SemaphoreSlim BookingSemaphore = new(1, 1);

        public async Task<Booking> CreateBookingAsync(Guid eventId)
        {
            await BookingSemaphore.WaitAsync();

            try
            {
                var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
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

                _context.Bookings.Add(booking);

                await _context.SaveChangesAsync();

                return booking;
            }
            finally
            {
                BookingSemaphore.Release();
            }
        }

        public Task<Booking?> GetBookingByIdAsync(Guid bookingId)
        {
            return _context.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId);
        }

        public Task<List<Booking>> GetPendingBookingsAsync()
        {
            return _context.Bookings
                .Where(x => x.Status == BookingStatus.Pending)
                .ToListAsync();
        }

        public async Task UpdateBookingAsync(Booking booking)
        {
            var existingBooking = await _context.Bookings.FirstOrDefaultAsync(x => x.Id == booking.Id);
            if (existingBooking is null)
            {
                return;
            }

            existingBooking.EventId = booking.EventId;
            existingBooking.Status = booking.Status;
            existingBooking.CreatedAt = booking.CreatedAt;
            existingBooking.ProcessedAt = booking.ProcessedAt;

            await _context.SaveChangesAsync();
        }

        public async Task ProcessBookingAsync(Booking booking, CancellationToken cancellationToken)
        {
            booking.Status = BookingStatus.Confirmed;
            booking.ProcessedAt = DateTime.UtcNow;

            await UpdateBookingAsync(booking);
        }
    }
}
