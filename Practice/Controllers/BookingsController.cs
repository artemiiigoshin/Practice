using Microsoft.AspNetCore.Mvc;
using Practice.Controllers.DTO;
using Practice.Models;
using Practice.Service;

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
        public ActionResult<BookingReadDto> CreateBooking(Guid id)
        {
            var evt = _eventService.GetById(id);
            if (evt == null)
                return NotFound();

            var booking = _bookingService.CreateBookingAsync(id).Result;

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
        public ActionResult<BookingReadDto> GetById(Guid id)
        {
            var booking = _bookingService.GetBookingByIdAsync(id).Result;
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
