using Microsoft.AspNetCore.Mvc;
using Practice.Application.DTO;
using Practice.Application.Service;
using Practice.Domain.Models;

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
        public async Task<ActionResult<BookingReadDto>> CreateBooking(Guid id, Guid userId)
        {
            var evt = await _eventService.GetByIdAsync(id);
            if (evt == null)
                return NotFound();

            var booking = await _bookingService.CreateBookingAsync(id, userId);

            var dto = new BookingReadDto(
                booking.Id,
                booking.EventId,
                booking.UserId,
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
                booking.UserId,
                booking.Status.ToString(),
                booking.CreatedAt,
                booking.ProcessedAt
            );

            return Ok(dto);
        }

        [HttpPost("bookings/{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            Guid userId,
            UserRole role = UserRole.User,
            CancellationToken cancellationToken = default)
        {
            await _bookingService.CancelBookingAsync(id, userId, role, cancellationToken);

            return NoContent();
        }
    }
}
