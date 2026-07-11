using Microsoft.EntityFrameworkCore;
using Practice.Domain.Models;
using Practice.Infrastructure.Repositories;

namespace IntegrationTests;

[Collection(nameof(RepositoryTestsCollection))]
public sealed class UserRepositoryTests
{
    private readonly PostgresTestFixture _fixture;

    public UserRepositoryTests(PostgresTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Add_And_GetByLoginAsync_ShouldReturnUser()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = new UserRepository(db);

        var user = new User(
            Guid.NewGuid(),
            "test-user",
            "hashed-password",
            UserRole.User);

        repository.Add(user);
        await repository.SaveChangesAsync();

        var result = await repository.GetByLoginAsync("test-user");

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("test-user", result.Login);
        Assert.Equal("hashed-password", result.PasswordHash);
        Assert.Equal(UserRole.User, result.Role);
    }

    [Fact]
    public async Task GetByLoginAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = new UserRepository(db);

        var result = await repository.GetByLoginAsync("missing-user");

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenLoginIsDuplicated_ShouldThrowDbUpdateException()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();
        var repository = new UserRepository(db);

        var firstUser = new User(
            Guid.NewGuid(),
            "duplicate-login",
            "first-password-hash",
            UserRole.User);

        var secondUser = new User(
            Guid.NewGuid(),
            "duplicate-login",
            "second-password-hash",
            UserRole.Admin);

        repository.Add(firstUser);
        await repository.SaveChangesAsync();

        repository.Add(secondUser);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repository.SaveChangesAsync());
    }
}