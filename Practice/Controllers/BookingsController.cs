using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Practice.Application.DTO;
using Practice.Application.Service;
using Practice.Domain.Models;
using System.Security.Claims;

namespace Practice.Controllers
{
    [ApiController]
    [Authorize]
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

            var userId = GetCurrentUserId();

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

        [HttpDelete("bookings/{id:guid}")]
        public async Task<IActionResult> Cancel(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await _bookingService.CancelBookingAsync(id, GetCurrentUserId(), GetCurrentUserRole(), cancellationToken);

            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(value, out var userId))
                throw new UnauthorizedAccessException("Invalid user identifier.");

            return userId;
        }

        private UserRole GetCurrentUserRole()
        {
            var value = User.FindFirstValue(ClaimTypes.Role);

            if (!Enum.TryParse<UserRole>(value, out var role))
                throw new UnauthorizedAccessException("Invalid user role.");

            return role;
        }
    }
}
