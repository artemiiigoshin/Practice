using Practice.Controllers.DTO;
using Practice.Models;

namespace Practice.Service
{
    public interface IEventService
    {
        PaginatedResult<Event> GetAll(EventQueryParameters query);
        Event? GetById(Guid id);
        Event Create(Event newEvent);
        bool Update(Event updatedEvent);
        bool Delete(Guid id);
        bool TryReserveSeats(Guid eventId, int count = 1);
        bool ReleaseSeats(Guid eventId, int count = 1);
    }
}
