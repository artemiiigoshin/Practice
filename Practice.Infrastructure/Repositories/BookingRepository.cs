using Microsoft.EntityFrameworkCore;
using Practice.Application.Repositories;
using Practice.Domain.Models;
using Practice.Infrastructure.DataContext;

namespace Practice.Infrastructure.Repositories;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    private readonly AppDbContext _context = context;

    public Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return _context.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, cancellationToken);
    }

    public Task<List<Booking>> GetPendingBookingsAsync(CancellationToken cancellationToken = default)
    {
        return _context.Bookings
            .Where(x => x.Status == BookingStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public void Add(Booking booking)
    {
        _context.Bookings.Add(booking);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _context.Bookings
            .CountAsync(x => x.UserId == userId &&
                (x.Status == BookingStatus.Pending || x.Status == BookingStatus.Confirmed),
                cancellationToken);
    }
}