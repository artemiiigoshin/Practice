using Practice.Models;
using Practice.Service;

namespace Tests
{
    public class BookingServiceTests
    {
        private readonly EventService _eventService;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _eventService = new EventService();
            _bookingService = new BookingService(_eventService);
        }

        private Event CreateEvent()
        {
            return _eventService.Create(new Event
            {
                Title = "Test event",
                Description = "Test description",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
                TotalSeats = 5,
                AvailableSeats = 5
            });
        }

        [Fact]
        public async Task Create_EventExisting_Returns_PendingStatus()
        {
            var evt = CreateEvent();

            var booking = await _bookingService.CreateBookingAsync(evt.Id);

            Assert.NotNull(booking);
            Assert.Equal(evt.Id, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.NotEqual(Guid.Empty, booking.Id);
            Assert.True(booking.CreatedAt <= DateTime.UtcNow);
            Assert.Null(booking.ProcessedAt);
        }

        [Fact]
        public async Task Create_MultipleBookings_ReturnsUniqueIds()
        {
            var evt = CreateEvent();

            var booking1 = await _bookingService.CreateBookingAsync(evt.Id);
            var booking2 = await _bookingService.CreateBookingAsync(evt.Id);
            var booking3 = await _bookingService.CreateBookingAsync(evt.Id);

            Assert.NotEqual(booking1.Id, booking2.Id);
            Assert.NotEqual(booking1.Id, booking3.Id);
            Assert.NotEqual(booking2.Id, booking3.Id);

            Assert.Equal(evt.TotalSeats - 3, evt.AvailableSeats);
        }

        [Fact]
        public async Task Get_BookingExisting_ReturnsCorrectBooking()
        {
            var evt = CreateEvent();
            var createdBooking = await _bookingService.CreateBookingAsync(evt.Id);

            var result = await _bookingService.GetBookingByIdAsync(createdBooking.Id);

            Assert.NotNull(result);
            Assert.Equal(createdBooking.Id, result!.Id);
            Assert.Equal(createdBooking.EventId, result.EventId);
            Assert.Equal(BookingStatus.Pending, result.Status);
            Assert.Equal(createdBooking.CreatedAt, result.CreatedAt);
            Assert.Equal(createdBooking.ProcessedAt, result.ProcessedAt);
        }

        [Fact]
        public async Task Get_ReturnsStatusConfirm()
        {
            var evt = CreateEvent();
            var booking = await _bookingService.CreateBookingAsync(evt.Id);

            booking.Status = BookingStatus.Confirmed;
            booking.ProcessedAt = DateTime.UtcNow;

            var result = await _bookingService.GetBookingByIdAsync(booking.Id);

            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Confirmed, result!.Status);
            Assert.NotNull(result.ProcessedAt);
        }

        [Fact]
        public async Task Get_ReturnsStatusReject()
        {
            var evt = CreateEvent();
            var booking = await _bookingService.CreateBookingAsync(evt.Id);

            booking.Status = BookingStatus.Rejected;
            booking.ProcessedAt = DateTime.UtcNow;

            var result = await _bookingService.GetBookingByIdAsync(booking.Id);

            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Rejected, result!.Status);
            Assert.NotNull(result.ProcessedAt);
        }

        [Fact]
        public async Task Create_EventNonExisting_Exception()
        {
            var nonExistingEventId = Guid.NewGuid();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _bookingService.CreateBookingAsync(nonExistingEventId));
        }

        [Fact]
        public async Task Create_EventDeleted_Exception()
        {
            var evt = CreateEvent();
            _eventService.Delete(evt.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _bookingService.CreateBookingAsync(evt.Id));
        }

        [Fact]
        public async Task Get_BookingNonExisting_ReturnsNull()
        {
            var result = await _bookingService.GetBookingByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task ProcessBooking_SetsConfirmedStatus()
        {
            var evt = CreateEvent();
            var booking = await _bookingService.CreateBookingAsync(evt.Id);

            await _bookingService.ProcessBookingAsync(booking, CancellationToken.None);

            Assert.Equal(BookingStatus.Confirmed, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
        }

        [Fact]
        public async Task CreateBooking_AvailableSeatsByOne()
        {
            var evt = CreateEvent();

            await _bookingService.CreateBookingAsync(evt.Id);

            Assert.Equal(evt.TotalSeats - 1, evt.AvailableSeats);
        }

        [Fact]
        public async Task CreateBooking_WhenSeatsAreExhausted_Exception()
        {
            var evt = CreateEvent();

            for (var i = 0; i < evt.TotalSeats; i++)
            {
                await _bookingService.CreateBookingAsync(evt.Id);
            }

            await Assert.ThrowsAsync<NoAvailableSeatsException>(() =>
                _bookingService.CreateBookingAsync(evt.Id));
        }

        [Fact]
        public async Task RejectReleaseSeats_RestoresAvailableSeats()
        {
            var evt = CreateEvent();

            var booking = await _bookingService.CreateBookingAsync(evt.Id);

            booking.Status = BookingStatus.Rejected;
            booking.ProcessedAt = DateTime.UtcNow;

            _eventService.ReleaseSeats(evt.Id);

            Assert.Equal(evt.TotalSeats, evt.AvailableSeats);
        }

        [Fact]
        public async Task RejectReleaseSeats_AllowsNewBooking()
        {
            var evt = CreateEvent();

            var firstBooking = await _bookingService.CreateBookingAsync(evt.Id);

            firstBooking.Status = BookingStatus.Rejected;
            firstBooking.ProcessedAt = DateTime.UtcNow;

            _eventService.ReleaseSeats(evt.Id);

            var secondBooking = await _bookingService.CreateBookingAsync(evt.Id);

            Assert.NotEqual(firstBooking.Id, secondBooking.Id);
        }

        [Fact]
        public async Task ConcurrentBookings_NoOverbooking()
        {
            var evt = CreateEvent();

            var tasks = Enumerable.Range(0, 20)
                .Select(async _ =>
                {
                    try
                    {
                        await _bookingService.CreateBookingAsync(evt.Id);
                        return true;
                    }
                    catch (NoAvailableSeatsException)
                    {
                        return false;
                    }
                });

            var results = await Task.WhenAll(tasks);

            Assert.Equal(5, results.Count(x => x));
            Assert.Equal(15, results.Count(x => !x));
            Assert.Equal(0, evt.AvailableSeats);
        }

        [Fact]
        public async Task ConcurrentBookings_UniqueIds()
        {
            var evt = CreateEvent();

            var tasks = Enumerable.Range(0, 5)
                .Select(_ => _bookingService.CreateBookingAsync(evt.Id));

            var bookings = await Task.WhenAll(tasks);

            Assert.Equal(5, bookings.Select(b => b.Id).Distinct().Count());
        }
    }
}
