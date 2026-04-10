using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence.Repositories;
using HotelBooking.Integration.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace HotelBooking.Integration.Tests.Repositories;

public class BookingRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldPersistBookingToDatabase()
    {
        // Arrange
        var db = DbContextFactory.Create();
        var room = Room.Create("101", "Standard", 500_000);
        db.Rooms.Add(room);
        await db.SaveChangesAsync();

        var booking = Booking.Create(
            room.Id,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            room.PricePerNight);

        var repository = new BookingRepository(db);

        // Act
        await repository.AddAsync(booking, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var saved = await db.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);
        saved.Should().NotBeNull();
        saved!.RoomId.Should().Be(room.Id);
        saved.TotalPrice.Should().Be(1_000_000); // 2 malam x 500.000
    }
}

// Catatan: test DbUpdateConcurrencyException → ConcurrencyException
// memerlukan PostgreSQL sungguhan karena InMemory database tidak mensimulasikan
// RowVersion conflict secara otomatis. Gunakan database test terpisah
// untuk skenario tersebut.