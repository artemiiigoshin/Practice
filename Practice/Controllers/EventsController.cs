using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Practice.Controllers.DTO;
using Practice.Models;
using Practice.Service;

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
        public ActionResult<PaginatedResult<EventReadDto>> GetAll([FromQuery] EventQueryParameters query)
        {
            if (query.Page < 1)
                return BadRequest("Page < 1.");

            if (query.PageSize < 1)
                return BadRequest("PageSize < 1.");

            var result = _eventService.GetAll(query);

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
                    e.EndAt
                )).ToList()
            };

            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public ActionResult<EventReadDto> GetById(Guid id)
        {
            var evt = _eventService.GetById(id);
            if (evt == null) return NotFound();

            var dto = new EventReadDto(
                evt.Id,
                evt.Title,
                evt.Description,
                evt.StartAt,
                evt.EndAt
            );

            return Ok(dto);
        }

        [HttpPost]
        public ActionResult<EventReadDto> Create(EventCreateAndUpdateDto CreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!EventValidator.CheckTime(CreateDto, out var error))
                return BadRequest(error);

            var newEvent = new Event
            {
                Title = CreateDto.Title,
                Description = CreateDto.Description,
                StartAt = CreateDto.StartAt,
                EndAt = CreateDto.EndAt
            };

            var createdEvent = _eventService.Create(newEvent);

            var result = new EventReadDto
            (
                createdEvent.Id,
                createdEvent.Title,
                createdEvent.Description,
                createdEvent.StartAt,
                createdEvent.EndAt
            );

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        public IActionResult Update(Guid id, EventCreateAndUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!EventValidator.CheckTime(updateDto, out var error))
                return BadRequest(error);

            var updatedEvent = new Event
            {
                Id = id,
                Title = updateDto.Title,
                Description = updateDto.Description,
                StartAt = updateDto.StartAt,
                EndAt = updateDto.EndAt
            };

            var result = _eventService.Update(updatedEvent);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {

            var result = _eventService.Delete(id);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
