using Practice.Models;

namespace Practice.Service
{
    public class EventService : IEventService
    {
        private readonly List<Event> _events = new();

        public IEnumerable<Event> GetAll(string? title, DateTime? from, DateTime? to)
        {
            var query = _events.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(e =>
                    e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            }

            if (from.HasValue)
            {
                query = query.Where(e => e.StartAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.EndAt <= to.Value);
            }

            return query.ToList();
        }

        public Event? GetById(Guid id) => _events.FirstOrDefault(e => e.Id == id);

        public Event Create(Event newEvent)
        {
            newEvent.Id = Guid.NewGuid();
            _events.Add(newEvent);
            return newEvent;
        }

        public bool Update(Event updatedEvent)
        {
            var existing = GetById(updatedEvent.Id);
            if (existing == null) return false;

            existing.Title = updatedEvent.Title;
            existing.Description = updatedEvent.Description;
            existing.StartAt = updatedEvent.StartAt;
            existing.EndAt = updatedEvent.EndAt;
            return true;
        }

        public bool Delete(Guid id)
        {
            var existing = GetById(id);
            if (existing == null) return false;

            _events.Remove(existing);
            return true;
        }
    }
}
