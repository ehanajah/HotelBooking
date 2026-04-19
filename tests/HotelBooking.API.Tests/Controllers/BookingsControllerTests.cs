using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HotelBooking.Application.Bookings.Commands.CreateBooking;
using HotelBooking.Application.Bookings.Queries.GetAvailableRooms;
using HotelBooking.API.Tests.Fixtures;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.API.Tests.Controllers;

public class BookingsControllerTests : ApiTestBase
{
    public BookingsControllerTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetAvailableRooms_ShouldReturn200_WithSeededRooms()
    {
        // Act
        var response = await Client.GetAsync(
            "/api/rooms/available?checkIn=2026-06-01&checkOut=2026-06-03");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var rooms = await response.Content.ReadFromJsonAsync<List<RoomDto>>();
        rooms.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateBooking_ShouldReturn201_WhenRequestIsValid()
    {
        // Ambil room dan guest dari database test
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var room = db.Rooms.First();
        var guest = db.Guests.First();

        var command = new CreateBookingCommand(
            room.Id,
            guest.Id,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

        // Act
        var response = await Client.PostAsJsonAsync("/api/bookings", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateBookingResult>();
        result.Should().NotBeNull();
        result.RoomNumber.Should().Be(room.RoomNumber);
    }

    [Fact]
    public async Task CreateBooking_ShouldReturn422_WhenRoomIdIsEmpty()
    {
        // Arrange
        var command = new CreateBookingCommand(
            Guid.Empty,       // ← tidak valid
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

        // Act
        var response = await Client.PostAsJsonAsync("/api/bookings", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateBooking_ShouldReturn409_WhenRoomIsAlreadyBooked()
    {
        // Arrange — booking pertama
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Pilih room yang berbeda dari test sebelumnya agar tidak konflik
        var room = db.Rooms.Skip(1).First();
        var guest = db.Guests.First();

        var checkIn = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var checkOut = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        var firstBooking = new CreateBookingCommand(room.Id, guest.Id, checkIn, checkOut);
        await Client.PostAsJsonAsync("/api/bookings", firstBooking);

        // Act — booking kedua untuk kamar dan tanggal yang sama
        var secondBooking = new CreateBookingCommand(room.Id, guest.Id, checkIn, checkOut);
        var response = await Client.PostAsJsonAsync("/api/bookings", secondBooking);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}