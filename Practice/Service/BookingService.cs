using Practice.Models;

namespace Practice.Service
{
    public class BookingService : IBookingService
    {
        private readonly List<Booking> _bookings = new();

        private readonly IEventService _eventService;

        public BookingService(IEventService eventService)
        {
            _eventService = eventService;
        }

        public Task<Booking> CreateBookingAsync(Guid eventId)
        {
            var evt = _eventService.GetById(eventId);
            if (evt is null)
                throw new InvalidOperationException("Event not found");

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null
            };

            _bookings.Add(booking);

            return Task.FromResult(booking);
        }

        public Task<Booking?> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = _bookings.FirstOrDefault(x => x.Id == bookingId);
            return Task.FromResult(booking);
        }

        public Task<List<Booking>> GetPendingBookingsAsync()
        {
            var pendingBookings = _bookings
                .Where(x => x.Status == BookingStatus.Pending)
                .ToList();

            return Task.FromResult(pendingBookings);
        }

        public Task UpdateBookingAsync(Booking booking)
        {
            var existingBooking = _bookings.FirstOrDefault(x => x.Id == booking.Id);
            if (existingBooking is null)
            {
                return Task.CompletedTask;
            }

            existingBooking.EventId = booking.EventId;
            existingBooking.Status = booking.Status;
            existingBooking.CreatedAt = booking.CreatedAt;
            existingBooking.ProcessedAt = booking.ProcessedAt;

            return Task.CompletedTask;
        }
    }
}
