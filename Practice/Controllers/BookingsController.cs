using Microsoft.AspNetCore.Mvc;
using Practice.Application.DTO;
using Practice.Application.Service;

namespace Practice.Controllers
{
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public BookingsController(IEventService eventService, IBookingService bookingService)
        {
            _eventService = eventService;
            _bookingService = bookingService;
        }

        [HttpPost("events/{id:guid}/book")]
        public async Task<ActionResult<BookingReadDto>> CreateBooking(Guid id)
        {
            var evt = await _eventService.GetByIdAsync(id);
            if (evt == null)
                return NotFound();

            var booking = await _bookingService.CreateBookingAsync(id);

            var dto = new BookingReadDto(
                booking.Id,
                booking.EventId,
                booking.Status.ToString(),
                booking.CreatedAt,
                booking.ProcessedAt
            );

            return Accepted($"/bookings/{booking.Id}", dto);
        }

        [HttpGet("bookings/{id:guid}")]
        public async Task<ActionResult<BookingReadDto>> GetById(Guid id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
                return NotFound();

            var dto = new BookingReadDto(
                booking.Id,
                booking.EventId,
                booking.Status.ToString(),
                booking.CreatedAt,
                booking.ProcessedAt
            );

            return Ok(dto);
        }
    }
}
