using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Integration.Tests.TestHelpers;

public static class DbContextFactory
{
    // Untuk unit test ringan yang tidak butuh PostgreSQL
    public static AppDbContext CreateInMemory() =>
        new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

    // Untuk integration test dengan PostgreSQL sungguhan
    public static AppDbContext CreatePostgres()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
            ?? "Host=localhost;Port=5433;Database=hotel_booking_test;Username=postgres;Password=postgres";

        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options);
    }

    // Pake yang postgres untuk test hadlle DbUpdateConcurrencyException
    public static AppDbContext Create() => CreatePostgres();
}