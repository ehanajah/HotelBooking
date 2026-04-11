using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence.Repositories;
using HotelBooking.Integration.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using HotelBooking.Infrastructure.Persistence;

namespace HotelBooking.Integration.Tests.Repositories;

public class BookingRepositoryTests : IAsyncLifetime
{
    private readonly AppDbContext _db;

    public BookingRepositoryTests()
    {
        _db = DbContextFactory.Create();
    }

    // IAsyncLifetime: dijalankan sebelum setiap test
    // Pastikan schema terbaru dan data bersih
    public async Task InitializeAsync()
    {
        await _db.Database.EnsureDeletedAsync();
        await _db.Database.EnsureCreatedAsync();
    }

    // IAsyncLifetime: dijalankan setelah setiap test
    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistBookingToDatabase()
    {
        // Arrange
        var guest = Guest.Create("John Doe", "john@example.com", "08123456789");
        _db.Guests.Add(guest);

        var room = Room.Create("101", "Standard", 500_000);
        _db.Rooms.Add(room);
        
        await _db.SaveChangesAsync();

        var booking = Booking.Create(
            room.Id,
            guest.Id,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            room.PricePerNight);

        var repository = new BookingRepository(_db);

        // Act
        await repository.AddAsync(booking, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var saved = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);
        saved.Should().NotBeNull();
        saved!.RoomId.Should().Be(room.Id);
        saved.TotalPrice.Should().Be(1_000_000);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenConcurrencyConflict_ShouldThrowConcurrencyException()
    {
        // Arrange
        var guest = Guest.Create("John Doe", "john@example.com", "08123456789");
        _db.Guests.Add(guest);

        var room = Room.Create("101", "Standard", 500_000);
        _db.Rooms.Add(room);
        
        await _db.SaveChangesAsync();

        // Dua DbContext berbeda membaca room yang sama
        var db1 = DbContextFactory.CreatePostgres();
        var db2 = DbContextFactory.CreatePostgres();

        var roomFromCtx1 = await db1.Rooms.FirstAsync(r => r.Id == room.Id);
        var roomFromCtx2 = await db2.Rooms.FirstAsync(r => r.Id == room.Id);

        // Context pertama berhasil update — RowVersion naik
        roomFromCtx1.MarkAsBooked();
        await db1.SaveChangesAsync();

        // Context kedua mencoba update dengan RowVersion lama
        roomFromCtx2.MarkAsBooked();
        var repository = new BookingRepository(db2);

        // Act
        var act = async () => await repository.SaveChangesAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyException>();

        await db1.DisposeAsync();
        await db2.DisposeAsync();
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_ShouldNotReturnBookedRooms()
    {
        // Arrange
        // Arrange
        var guest = Guest.Create("John Doe", "john@example.com", "08123456789");
        _db.Guests.Add(guest);
        
        var room1 = Room.Create("201", "Standard", 500_000);
        var room2 = Room.Create("202", "Standard", 500_000);
        _db.Rooms.AddRange(room1, room2);
        
        await _db.SaveChangesAsync();

        // Booking aktif untuk room1
        var booking = Booking.Create(
            room1.Id,
            guest.Id,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            room1.PricePerNight);
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        var repository = new RoomRepository(_db);

        // Act
        var available = await repository.GetAvailableRoomsAsync(
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            null,
            CancellationToken.None);

        // Assert — hanya room2 yang tersedia
        available.Should().HaveCount(1);
        available.First().Id.Should().Be(room2.Id);
    }
}