using Microsoft.AspNetCore.Mvc;
using Practice.Application.DTO;
using Practice.Application.Service;
using Practice.Domain.Models;

namespace Practice.Controllers
{
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<EventReadDto>>> GetAll([FromQuery] EventQueryParameters query)
        {
            if (query.Page < 1)
                return BadRequest("Page < 1.");

            if (query.PageSize < 1)
                return BadRequest("PageSize < 1.");

            var result = await _eventService.GetAllAsync(query);

            var response = new PaginatedResult<EventReadDto>
            {
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize,
                ItemsCount = result.ItemsCount,
                Items = result.Items.Select(e => new EventReadDto(
                    e.Id,
                    e.Title,
                    e.Description,
                    e.StartAt,
                    e.EndAt,
                    e.TotalSeats,
                    e.AvailableSeats
                )).ToList()
            };

            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EventReadDto>> GetById(Guid id)
        {
            var evt = await _eventService.GetByIdAsync(id);
            if (evt == null) return NotFound();

            var dto = new EventReadDto(
                evt.Id,
                evt.Title,
                evt.Description,
                evt.StartAt,
                evt.EndAt,
                evt.TotalSeats,
                evt.AvailableSeats
            );

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<EventReadDto>> Create(EventRequestDto CreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!EventValidator.CheckTime(CreateDto.StartAt, CreateDto.EndAt, out var error))
                return BadRequest(error);

            var newEvent = new EventCreateDto
            (
                CreateDto.Title,
                CreateDto.Description,
                CreateDto.StartAt,
                CreateDto.EndAt,
                CreateDto.TotalSeats,
                CreateDto.TotalSeats
            );

            var createdEvent = await _eventService.CreateAsync(newEvent);

            var result = new EventReadDto
            (
                createdEvent.Id,
                createdEvent.Title,
                createdEvent.Description,
                createdEvent.StartAt,
                createdEvent.EndAt,
                createdEvent.TotalSeats,
                createdEvent.AvailableSeats
            );

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, EventRequestDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!EventValidator.CheckTime(updateDto.StartAt, updateDto.EndAt, out var error))
                return BadRequest(error);

            var updatedEvent = new EventUpdateDto
            (
                id,
                updateDto.Title,
                updateDto.Description,
                updateDto.StartAt,
                updateDto.EndAt,
                updateDto.TotalSeats
            );

            var result = await _eventService.UpdateAsync(updatedEvent);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {

            var result = await _eventService.DeleteAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
