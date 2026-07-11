using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Practice.Application.Service;
using Practice.Application.DTO;
using Practice.Domain.Models;
using Practice.Infrastructure.DataContext;
using Practice.Application.Repositories;
using Practice.Infrastructure.Repositories;
using Practice.Domain.Exceptions;

namespace Tests
{
    public class BookingServiceTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        private readonly Guid _userId = Guid.NewGuid();

        public BookingServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();

            _serviceProvider = services.BuildServiceProvider();
            _eventService = _serviceProvider.GetRequiredService<IEventService>();
            _bookingService = _serviceProvider.GetRequiredService<IBookingService>();
        }

        private async Task<Event> CreateEventAsync()
        {
            return await _eventService.CreateAsync(new EventCreateDto(
                "Test event",
                "Test description",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(2),
                5,
                5));
        }

        private async Task<Event> CreateEventAsync(int totalSeats)
        {
            return await _eventService.CreateAsync(new EventCreateDto(
                "Test event",
                "Test description",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(2),
                totalSeats,
                totalSeats));
        }

        [Fact]
        public async Task Create_EventExisting_Returns_PendingStatus()
        {
            var evt = await CreateEventAsync();

            var booking = await _bookingService.CreateBookingAsync(evt.Id, _userId);

            Assert.NotNull(booking);
            Assert.Equal(evt.Id, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.NotEqual(Guid.Empty, booking.Id);
            Assert.True(booking.CreatedAt <= DateTime.UtcNow);
            Assert.Null(booking.ProcessedAt);
            Assert.Equal(_userId, booking.UserId);
        }

        [Fact]
        public async Task Create_MultipleBookings_ReturnsUniqueIds()
        {
            var evt = await CreateEventAsync();

            var booking1 = await _bookingService.CreateBookingAsync(evt.Id, _userId);
            var booking2 = await _bookingService.CreateBookingAsync(evt.Id, _userId);
            var booking3 = await _bookingService.CreateBookingAsync(evt.Id, _userId);

            Assert.NotEqual(booking1.Id, booking2.Id);
            Assert.NotEqual(booking1.Id, booking3.Id);
            Assert.NotEqual(booking2.Id, booking3.Id);

            Assert.Equal(evt.TotalSeats - 3, evt.AvailableSeats);
        }

        [Fact]
        public async Task Get_BookingExisting_ReturnsCorrectBooking()
        {
            var evt = await CreateEventAsync();

            var createdBooking = await _bookingService.CreateBookingAsync(evt.Id, _userId);

            var result = await _bookingService.GetBookingByIdAsync(createdBooking.Id);

            Assert.NotNull(result);
            Assert.Equal(createdBooking.Id, result!.Id);
            Assert.Equal(createdBooking.EventId, result.EventId);
            Assert.Equal(BookingStatus.Pending, result.Status);
            Assert.Equal(createdBooking.CreatedAt, result.CreatedAt);
            Assert.Equal(createdBooking.ProcessedAt, result.ProcessedAt);
        }

        [Fact]
        public async Task Reject_SetsStatusRejected()
        {
            var evt = await CreateEventAsync();

            var booking = await _bookingService.CreateBookingAsync(evt.Id, _userId);

            booking.Reject();

            Assert.Equal(BookingStatus.Rejected, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
        }

        [Fact]
        public async Task Get_ReturnsStatusReject()
        {
            var evt = await CreateEventAsync();

            var booking = await _bookingService.CreateBookingAsync(evt.Id, _userId);

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
                await _bookingService.CreateBookingAsync(nonExistingEventId, _userId));
        }

        [Fact]
        public async Task Create_EventDeleted_Exception()
        {
            var evt = await CreateEventAsync();

            await _eventService.DeleteAsync(evt.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _bookingService.CreateBookingAsync(evt.Id, _userId));
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
            var evt = await CreateEventAsync();

            var booking = await _bookingService.CreateBookingAsync(evt.Id, _userId);

            await _bookingService.ProcessBookingAsync(booking, CancellationToken.None);

            Assert.Equal(BookingStatus.Confirmed, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
        }

        [Fact]
        public async Task CreateBooking_AvailableSeatsByOne()
        {
            var evt = await CreateEventAsync();

            await _bookingService.CreateBookingAsync(evt.Id, _userId);

            Assert.Equal(evt.TotalSeats - 1, evt.AvailableSeats);
        }

        [Fact]
        public async Task CreateBooking_WhenSeatsAreExhausted_Exception()
        {
            var evt = await CreateEventAsync();

            for (var i = 0; i < evt.TotalSeats; i++)
            {
                await _bookingService.CreateBookingAsync(evt.Id, Guid.NewGuid());
            }

            await Assert.ThrowsAsync<ExtensionException>(() =>
                 _bookingService.CreateBookingAsync(evt.Id, Guid.NewGuid()));
        }

        [Fact]
        public async Task RejectReleaseSeats_RestoresAvailableSeats()
        {
            var evt = await CreateEventAsync();

            var booking = await _bookingService.CreateBookingAsync(evt.Id, _userId);
            booking.Reject();

            var updatedEvent = await _eventService.GetByIdAsync(evt.Id);

            Assert.NotNull(updatedEvent);

            EventSeatManager.ReleaseSeats(updatedEvent);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await context.SaveChangesAsync();

            var resultEvent = await _eventService.GetByIdAsync(evt.Id);

            Assert.NotNull(resultEvent);
            Assert.Equal(resultEvent!.TotalSeats, resultEvent.AvailableSeats);
        }

        [Fact]
        public async Task RejectReleaseSeats_AllowsNewBooking()
        {
            var evt = await CreateEventAsync();

            var firstBooking = await _bookingService.CreateBookingAsync(evt.Id, _userId);
            firstBooking.Reject();

            var updatedEvent = await _eventService.GetByIdAsync(evt.Id);

            Assert.NotNull(updatedEvent);

            EventSeatManager.ReleaseSeats(updatedEvent);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await context.SaveChangesAsync();

            var secondBooking = await _bookingService.CreateBookingAsync(evt.Id, _userId);

            Assert.NotEqual(firstBooking.Id, secondBooking.Id);
        }

        [Fact]
        public async Task ConcurrentBookings_NoOverbooking()
        {
            var evt = await CreateEventAsync();

            var tasks = Enumerable.Range(0, 20)
                .Select(async _ =>
                {
                    try
                    {
                        await _bookingService.CreateBookingAsync(evt.Id, Guid.NewGuid());
                        return true;
                    }
                    catch (ExtensionException)
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
            var evt = await CreateEventAsync();

            var tasks = Enumerable.Range(0, 5)
                .Select(_ => _bookingService.CreateBookingAsync(evt.Id, _userId));

            var bookings = await Task.WhenAll(tasks);

            Assert.Equal(5, bookings.Select(b => b.Id).Distinct().Count());
        }

        [Fact]
        public async Task CreateBooking_PastEvent_ThrowsPastEventBookingException()
        {
            using var scope = _serviceProvider.CreateScope();

            var eventRepository =
                scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var pastEvent = new Event(
                Guid.NewGuid(),
                "Past event",
                "Past event description",
                DateTime.UtcNow.AddDays(-2),
                DateTime.UtcNow.AddDays(-1),
                5,
                5);

            eventRepository.Add(pastEvent);
            await eventRepository.SaveChangesAsync();

            await Assert.ThrowsAsync<PastEventBookingException>(() =>
                _bookingService.CreateBookingAsync(
                    pastEvent.Id,
                    Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateBooking_WhenUserHasTenActiveBookings_ThrowsLimitException()
        {
            var evt = await CreateEventAsync(20);
            var userId = Guid.NewGuid();

            for (var i = 0; i < 10; i++)
            {
                await _bookingService.CreateBookingAsync(
                    evt.Id,
                    userId);
            }

            var ex = await Assert.ThrowsAsync<ActiveBookingLimitExceededException>(() =>
                _bookingService.CreateBookingAsync(
                    evt.Id,
                    userId));

            Assert.Equal(10, evt.AvailableSeats);
            Assert.Contains("10", ex.Message);
            Assert.Equal(10, evt.AvailableSeats);
        }

        [Fact]
        public async Task CreateBooking_DifferentUsersHaveIndependentLimits()
        {
            var evt = await CreateEventAsync(20);

            var firstUserId = Guid.NewGuid();
            var secondUserId = Guid.NewGuid();

            for (var i = 0; i < 10; i++)
            {
                await _bookingService.CreateBookingAsync(
                    evt.Id,
                    firstUserId);
            }

            var booking = await _bookingService.CreateBookingAsync(
                evt.Id,
                secondUserId);

            Assert.NotNull(booking);
            Assert.Equal(secondUserId, booking.UserId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.Equal(9, evt.AvailableSeats);
        }
    }
}
