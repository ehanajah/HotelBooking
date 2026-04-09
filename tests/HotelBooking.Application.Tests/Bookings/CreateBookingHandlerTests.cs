using FluentAssertions;
using HotelBooking.Application.Bookings.Commands.CreateBooking;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Interfaces;
using Moq;

namespace HotelBooking.Application.Tests.Bookings;

public class CreateBookingHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateBooking_WhenRequestIsValid()
    {
        // Arrange
        var room = Room.Create("101", "Standard", 500_000);
        var guest = Guest.Create("John Doe", "johndoe@test.com", "08123456789");

        var bookingRepository = new Mock<IBookingRepository>();
        var roomRepository = new Mock<IRoomRepository>();
        var lockService = new Mock<IDistributedLockService>();

        roomRepository
            .Setup(r => r.GetByIdAsync(room.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Room.Create("101", "Standard", 500_000));

        roomRepository
            .Setup(r => r.HasConflictAsync(
                room.Id,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        lockService
            .Setup(l => l.AcquireAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>()))
            .ReturnsAsync(true);
        lockService
            .Setup(l => l.ReleaseAsync(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateBookingHandler(
            roomRepository.Object,
            bookingRepository.Object,
            lockService.Object);

        var command = new CreateBookingCommand(
            room.Id,
            guest.Id,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreateBookingResult>();
    }
}