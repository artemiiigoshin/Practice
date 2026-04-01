using Practice.Models;

namespace Practice.Service
{
    public class EventService : IEventService
    {
        private readonly List<Event> _events = new();

        public IEnumerable<Event> GetAll() => _events;

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
