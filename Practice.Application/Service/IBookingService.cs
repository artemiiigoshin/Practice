using Practice.Domain.Models;

namespace Practice.Application.Service;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Guid eventId, Guid userId);
    Task<Booking?> GetBookingByIdAsync(Guid bookingId);
    Task<List<Booking>> GetPendingBookingsAsync();
    Task UpdateBookingAsync(Booking booking);
    Task ProcessBookingAsync(Booking booking, CancellationToken cancellationToken);
    Task CancelBookingAsync(
        Guid bookingId,
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default);
}
