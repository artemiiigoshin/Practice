using Practice.Application.DTO;
using Practice.Application.Service;
using Practice.Domain.Models;
using Practice.Infrastructure.Repositories;

namespace IntegrationTests;

[Collection(nameof(RepositoryTestsCollection))]
public sealed class EventServiceIntegrationTests
{
    private readonly PostgresTestFixture _fixture;

    public EventServiceIntegrationTests(PostgresTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllAsync_Should_Filter_By_Title()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = new EventRepository(db);
        var service = new EventService(repository);

        repository.Add(new Event(Guid.NewGuid(), "Music Fest", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, 10));
        repository.Add(new Event(Guid.NewGuid(), "Tech Talk", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, 10));
        await repository.SaveChangesAsync();

        var result = await service.GetAllAsync(new EventQueryParameters
        {
            Title = "music",
            Page = 1,
            PageSize = 10
        });

        Assert.Single(result.Items);
        Assert.Equal("Music Fest", result.Items[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_Should_Filter()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = new EventRepository(db);
        var service = new EventService(repository);

        var now = DateTime.UtcNow;

        repository.Add(new Event(Guid.NewGuid(), "Old", null, now.AddDays(-5), now.AddDays(-4), 10, 10));
        repository.Add(new Event(Guid.NewGuid(), "Actual", null, now.AddDays(2), now.AddDays(3), 10, 10));
        repository.Add(new Event(Guid.NewGuid(), "Future", null, now.AddDays(10), now.AddDays(11), 10, 10));
        await repository.SaveChangesAsync();

        var result = await service.GetAllAsync(new EventQueryParameters
        {
            From = now,
            To = now.AddDays(5),
            Page = 1,
            PageSize = 10
        });

        Assert.Single(result.Items);
        Assert.Equal("Actual", result.Items[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_Should_Apply_Pagination()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = new EventRepository(db);
        var service = new EventService(repository);

        repository.Add(new Event(Guid.NewGuid(), "First", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, 10));
        repository.Add(new Event(Guid.NewGuid(), "Second", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, 10));
        repository.Add(new Event(Guid.NewGuid(), "Third", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, 10));
        await repository.SaveChangesAsync();

        var result = await service.GetAllAsync(new EventQueryParameters
        {
            Page = 2,
            PageSize = 2
        });

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Single(result.Items);
    }
}