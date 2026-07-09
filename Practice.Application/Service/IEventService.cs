using Practice.Application.DTO;
using Practice.Domain.Models;

namespace Practice.Application.Service;

public interface IEventService
{
    Task<PaginatedResult<Event>> GetAllAsync(EventQueryParameters query);
    Task<Event?> GetByIdAsync(Guid id);
    Task<Event> CreateAsync(EventCreateDto newEvent);
    Task<bool> UpdateAsync(EventUpdateDto updatedEvent);
    Task<bool> DeleteAsync(Guid id);

    //До востребования 
    //Task<bool> TryReserveSeatsAsync(Guid eventId, int count = 1);
    //Task<bool> ReleaseSeatsAsync(Guid eventId, int count = 1);
}
