using Practice.Models;

namespace Practice.Repositories;

public interface IEventRepository
{
    IQueryable<Event> GetAll();
    Task<Event?> GetByIdAsync(Guid id);
    void Add(Event evt);
    void Remove(Event evt);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}