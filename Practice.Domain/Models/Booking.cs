using Practice.Domain.Exceptions;

namespace Practice.Domain.Models;

public class Booking
{
    private Booking()
    {
        Event = null!;
    }

    public Booking(Guid id, Guid eventId, Guid userId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
    {
        Id = id;
        EventId = eventId;
        UserId = userId;
        Status = status;
        CreatedAt = createdAt;
        ProcessedAt = processedAt;
    }

    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public BookingStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
    public Guid UserId { get; set; }

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking has already been cancelled.");

        if (Status == BookingStatus.Rejected)
            throw new InvalidOperationException("Rejected booking cannot be cancelled.");

        Status = BookingStatus.Cancelled;
        ProcessedAt = DateTime.UtcNow;
    }

    public void EnsureCanBeManagedBy(Guid userId, UserRole role)
    {
        if (role != UserRole.Admin && UserId != userId)
            throw new OperationForbiddenException();
    }

    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
}

