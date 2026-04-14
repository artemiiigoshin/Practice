using Practice.Models;

namespace Practice.Service
{
    public interface IEventService
    {
        IEnumerable<Event> GetAll(string? title, DateTime? from, DateTime? to);
        Event? GetById(Guid id);
        Event Create(Event newEvent);
        bool Update(Event updatedEvent);
        bool Delete(Guid id);
    }
}
