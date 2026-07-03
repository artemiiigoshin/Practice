using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

[Collection(nameof(RepositoryTestsCollection))]
public sealed class MigrationTests
{
    private readonly PostgresTestFixture _fixture;

    public MigrationTests(PostgresTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Migrations_Should_Create_Tables_Foreign_Key_And_Index()
    {
        await _fixture.ResetDatabaseAsync();

        await using var db = _fixture.CreateDbContext();

        var tables = await db.Database.SqlQueryRaw<string>(
            """
        SELECT table_name AS "Value"
        FROM information_schema.tables
        WHERE table_schema = 'public'
          AND table_name IN ('Events', 'Bookings')
        """)
            .ToListAsync();

        var foreignKeyExists = await db.Database.SqlQueryRaw<bool>(
            """
        SELECT EXISTS (
            SELECT 1
            FROM information_schema.table_constraints
            WHERE table_schema = 'public'
              AND table_name = 'Bookings'
              AND constraint_name = 'FK_Bookings_Events_EventId'
              AND constraint_type = 'FOREIGN KEY'
        ) AS "Value"
        """)
            .SingleAsync();

        var indexExists = await db.Database.SqlQueryRaw<bool>(
            """
        SELECT EXISTS (
            SELECT 1
            FROM pg_indexes
            WHERE schemaname = 'public'
              AND tablename = 'Bookings'
              AND indexname = 'IX_Bookings_EventId'
        ) AS "Value"
        """)
            .SingleAsync();

        Assert.Contains("Events", tables);
        Assert.Contains("Bookings", tables);
        Assert.True(foreignKeyExists);
        Assert.True(indexExists);
    }
}
