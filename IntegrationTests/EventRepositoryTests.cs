using Microsoft.EntityFrameworkCore;
using Practice.Models;
using Practice.Repositories;

namespace IntegrationTests;

[Collection(nameof(RepositoryTestsCollection))]
public sealed class EventRepositoryTests
{
    private readonly PostgresTestFixture _fixture;

    public EventRepositoryTests(PostgresTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Add_GetByIdAsync_Save_And_Return_Event()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = new EventRepository(db);

        var evt = new Event(
            Guid.NewGuid(),
            "Test event",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            10,
            10);

        repository.Add(evt);
        await repository.SaveChangesAsync();

        var result = await repository.GetByIdAsync(evt.Id);

        Assert.NotNull(result);
        Assert.Equal(evt.Id, result.Id);
        Assert.Equal("Test event", result.Title);
    }

    [Fact]
    public async Task GetAll_Should_Return_Events_As_Queryable()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = new EventRepository(db);

        repository.Add(new Event(Guid.NewGuid(), "First", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, 10));
        repository.Add(new Event(Guid.NewGuid(), "Second", null, DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(4), 20, 20));
        await repository.SaveChangesAsync();

        var events = await repository.GetAll().ToListAsync();

        Assert.Equal(2, events.Count);
    }

    [Fact]
    public async Task Remove_Should_Delete_Event()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = new EventRepository(db);

        var evt = new Event(Guid.NewGuid(), "Delete me", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, 10);

        repository.Add(evt);
        await repository.SaveChangesAsync();

        repository.Remove(evt);
        await repository.SaveChangesAsync();

        var result = await repository.GetByIdAsync(evt.Id);

        Assert.Null(result);
    }
}