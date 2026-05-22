using System.ComponentModel.DataAnnotations;

namespace Practice.Models
{
    public class Event
    {
        private Event()
        {
            Title = null!;
        }

        public Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt, int totalSeats, int availableSeats)
        {
            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = availableSeats;
        }


        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
