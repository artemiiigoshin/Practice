namespace Practice.Models
{
    public class Booking
    {
        private Booking()
        {
            Event = null!;
        }

        public Booking(Guid id, Guid eventId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
        {
            Id = id;
            EventId = eventId;
            Status = status;
            CreatedAt = createdAt;
            ProcessedAt = processedAt;
        }

        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

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

        public Event Event { get; set; } = null!;
    }
}

