using Practice.Models;

namespace Practice.Service
{
    public interface IEventService
    {
        IEnumerable<Event> GetAll();
        Event? GetById(Guid id);
        Event Create(Event newEvent);
        bool Update(Event updatedEvent);
        bool Delete(Guid id);
    }
}
