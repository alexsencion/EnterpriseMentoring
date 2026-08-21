using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Api.Data;

namespace WarehouseManagement.Tests;

public class SqliteTestFixture : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    public DbContextOptions<ApplicationDbContext> Options { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        Options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        await using var context = CreateContext();

        await context.Database.EnsureCreatedAsync();
    }

    public ApplicationDbContext CreateContext()
    {
        return new ApplicationDbContext(Options);
    }
    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
