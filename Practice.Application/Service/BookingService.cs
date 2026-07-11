using Practice.Application.Repositories;
using Practice.Domain.Exceptions;
using Practice.Domain.Models;

namespace Practice.Application.Service;

public class BookingService(
IBookingRepository bookingRepository,
IEventRepository eventRepository) : IBookingService
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IEventRepository _eventRepository = eventRepository;

    private static readonly SemaphoreSlim BookingSemaphore = new(1, 1);

    public async Task<Booking> CreateBookingAsync(Guid eventId, Guid userId)
    {
        await BookingSemaphore.WaitAsync();

        try
        {
            var evt = await _eventRepository.GetByIdAsync(eventId);
            if (evt is null)
                throw new InvalidOperationException("Event not found");

            if (evt.StartAt <= DateTime.UtcNow)
                throw new PastEventBookingException();

            var activeBookings = await _bookingRepository.CountActiveByUserIdAsync(userId);

            if (activeBookings >= 5)
                throw new ActiveBookingLimitExceededException();

            var reserved = EventSeatManager.TryReserveSeats(evt);

            if (!reserved)
                throw new ExtensionException();

            var booking = new Booking
            (
                Guid.NewGuid(),
                eventId,
                userId,
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
        existingBooking.UserId = booking.UserId;
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

    public async Task CancelBookingAsync(
    Guid bookingId,
    Guid userId,
    UserRole role,
    CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(
                bookingId,
                cancellationToken)
            ?? throw new InvalidOperationException("Booking not found");

        booking.EnsureCanBeManagedBy(userId, role);

        var evt = await _eventRepository.GetByIdAsync(booking.EventId)
            ?? throw new InvalidOperationException("Event not found");

        booking.Cancel();
        EventSeatManager.ReleaseSeats(evt);

        await _bookingRepository.SaveChangesAsync(cancellationToken);
    }
}
