using Practice.Domain.Models;

namespace Practice.Application.Service;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Guid eventId);
    Task<Booking?> GetBookingByIdAsync(Guid bookingId);
    Task<List<Booking>> GetPendingBookingsAsync();
    Task UpdateBookingAsync(Booking booking);
    Task ProcessBookingAsync(Booking booking, CancellationToken cancellationToken);
}
