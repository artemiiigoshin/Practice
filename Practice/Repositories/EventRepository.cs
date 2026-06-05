using Microsoft.EntityFrameworkCore;
using Practice.DataAccess;
using Practice.Models;

namespace Practice.Repositories;

public class EventRepository(AppDbContext context) : IEventRepository
{
    private readonly AppDbContext _context = context;

    public IQueryable<Event> GetAll()
    {
        return _context.Events.AsQueryable();
    }

    public Task<Event?> GetByIdAsync(Guid id)
    {
        return _context.Events.FirstOrDefaultAsync(e => e.Id == id);
    }

    public void Add(Event evt)
    {
        _context.Add(evt);
    }

    public void Remove(Event evt)
    {
        _context.Remove(evt);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}