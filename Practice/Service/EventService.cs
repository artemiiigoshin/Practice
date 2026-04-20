using Practice.Controllers.DTO;
using Practice.Models;

namespace Practice.Service
{
    public class EventService : IEventService
    {
        private readonly List<Event> _events = new();

        public PaginatedResult<Event> GetAll(EventQueryParameters query)
        {
            var eventsQuery = _events.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Title))
            {
                eventsQuery = eventsQuery.Where(e =>
                    e.Title.Contains(query.Title, StringComparison.OrdinalIgnoreCase));
            }

            if (query.From.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.StartAt >= query.From.Value);
            }

            if (query.To.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.EndAt <= query.To.Value);
            }

            var totalCount = eventsQuery.Count();

            var items = eventsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            return new PaginatedResult<Event>
            {
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                ItemsCount = items.Count,
                Items = items
            };
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
