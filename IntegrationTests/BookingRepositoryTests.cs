using Practice.Domain.Models;
using Practice.Infrastructure.DataContext;
using Practice.Infrastructure.Repositories;

namespace IntegrationTests;

[Collection(nameof(RepositoryTestsCollection))]
public sealed class BookingRepositoryTests
{
    private readonly PostgresTestFixture _fixture;

    public BookingRepositoryTests(PostgresTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Add_GetByIdAsync_Save_And_Return_Booking()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var eventRepository = new EventRepository(db);
        var bookingRepository = new BookingRepository(db);
        var user = await CreateUserAsync(db);

        var evt = new Event(Guid.NewGuid(), "Event", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, 10);
        eventRepository.Add(evt);
        await eventRepository.SaveChangesAsync();

        var booking = new Booking(Guid.NewGuid(), evt.Id, user.Id, BookingStatus.Pending, DateTime.UtcNow, null);

        bookingRepository.Add(booking);
        await bookingRepository.SaveChangesAsync();

        var result = await bookingRepository.GetByIdAsync(booking.Id);

        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(evt.Id, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
    }

    [Fact]
    public async Task GetPendingBookingsAsync_Should_Return_Only_Pending_Bookings()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var eventRepository = new EventRepository(db);
        var bookingRepository = new BookingRepository(db);
        var user = await CreateUserAsync(db);

        var evt = new Event(Guid.NewGuid(), "Event", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, 10);
        eventRepository.Add(evt);
        await eventRepository.SaveChangesAsync();

        bookingRepository.Add(new Booking(Guid.NewGuid(), evt.Id, user.Id, BookingStatus.Pending, DateTime.UtcNow, null));
        bookingRepository.Add(new Booking(Guid.NewGuid(), evt.Id, user.Id, BookingStatus.Confirmed, DateTime.UtcNow, DateTime.UtcNow));
        bookingRepository.Add(new Booking(Guid.NewGuid(), evt.Id, user.Id, BookingStatus.Rejected, DateTime.UtcNow, DateTime.UtcNow));
        await bookingRepository.SaveChangesAsync();

        var result = await bookingRepository.GetPendingBookingsAsync();

        Assert.Single(result);
        Assert.Equal(BookingStatus.Pending, result[0].Status);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Update_Booking_Status()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var eventRepository = new EventRepository(db);
        var bookingRepository = new BookingRepository(db);
        var user = await CreateUserAsync(db);

        var evt = new Event(
            Guid.NewGuid(),
            "Event",
            null,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            10,
            10);

        eventRepository.Add(evt);
        await eventRepository.SaveChangesAsync();

        var booking = new Booking(
            Guid.NewGuid(),
            evt.Id,
            user.Id,
            BookingStatus.Pending,
            DateTime.UtcNow,
            null);

        bookingRepository.Add(booking);
        await bookingRepository.SaveChangesAsync();

        booking.Confirm();
        await bookingRepository.SaveChangesAsync();

        await using var assertDb = _fixture.CreateDbContext();
        var updatedBooking = await assertDb.Bookings.FindAsync(booking.Id);

        Assert.NotNull(updatedBooking);
        Assert.Equal(BookingStatus.Confirmed, updatedBooking.Status);
        Assert.NotNull(updatedBooking.ProcessedAt);
    }

    private async Task<User> CreateUserAsync(AppDbContext db)
    {
        var user = new User(
            Guid.NewGuid(),
            $"user-{Guid.NewGuid()}",
            "password-hash",
            UserRole.User);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }
}