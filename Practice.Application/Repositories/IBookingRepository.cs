using Practice.Domain.Models;

namespace Practice.Application.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<List<Booking>> GetPendingBookingsAsync(CancellationToken cancellationToken = default);
    void Add(Booking booking);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}