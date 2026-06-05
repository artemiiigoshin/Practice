using Microsoft.EntityFrameworkCore;
using Practice.Controllers.DTO;
using Practice.Models;
using Practice.Repositories;

namespace Practice.Service
{
    public class EventService(IEventRepository eventRepository) : IEventService
    {
        private readonly IEventRepository _eventRepository = eventRepository;

        public async Task<PaginatedResult<Event>> GetAllAsync(EventQueryParameters query)
        {
            var eventsQuery = _eventRepository.GetAll();

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

            var totalCount = await eventsQuery.CountAsync();

            var items = await eventsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PaginatedResult<Event>
            {
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                ItemsCount = items.Count,
                Items = items
            };
        }

        public Task<Event?> GetByIdAsync(Guid id) => _eventRepository.GetByIdAsync(id);

        public async Task<Event> CreateAsync(EventCreateDto evt)
        {
            var newEvent = new Event
                (
                Guid.NewGuid(),
                evt.Title,
                evt.Description,
                evt.StartAt,
                evt.EndAt,
                evt.TotalSeats,
                evt.AvailableSeats
                );

            _eventRepository.Add(newEvent);
            await _eventRepository.SaveChangesAsync();

            return newEvent;

        }

        public async Task<bool> UpdateAsync(EventUpdateDto updatedEvent)
        {
            var existing = await GetByIdAsync(updatedEvent.Id);
            if (existing == null) return false;

            existing.Title = updatedEvent.Title;
            existing.Description = updatedEvent.Description;
            existing.StartAt = updatedEvent.StartAt;
            existing.EndAt = updatedEvent.EndAt;
            existing.TotalSeats = updatedEvent.TotalSeats;

            await _eventRepository.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null) return false;

            _eventRepository.Remove(existing);

            await _eventRepository.SaveChangesAsync();

            return true;
        }

        //Пока нет необходимости в использовании, перенесены в EventSeatManager
        //public bool TryReserveSeats(Guid eventId, int count = 1)
        //{
        //    var existing = GetById(eventId);
        //    if (existing == null)
        //        return false;
       
        //    if (count <= 0)
        //        throw new ArgumentException("Количество мест должно быть больше нуля.");

        //    if (existing.AvailableSeats < count)
        //        return false;

        //    existing.AvailableSeats -= count;
        //    return true;
        //}

        //public async Task<bool> ReleaseSeatsAsync(Guid eventId, int count = 1)
        //{
        //    var existing = await GetByIdAsync(eventId);
        //    if (existing == null)
        //        return false;

        //    if (count <= 0)
        //        throw new ArgumentException("Количество мест должно быть больше нуля.");

        //    existing.AvailableSeats += count;

        //    if (existing.AvailableSeats > existing.TotalSeats)
        //        throw new InvalidOperationException("AvailableSeats не может быть больше TotalSeats.");

        //    await _context.SaveChangesAsync();

        //    return true;
        //}
    }
}
