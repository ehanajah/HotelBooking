using FluentAssertions;
using HotelBooking.Application.Bookings.Commands.CreateBooking;
using HotelBooking.Application.Common.Interfaces;
using Moq;

namespace HotelBooking.Application.Tests.Bookings;

public class CreateBookingValidatorTests
{
    private readonly Mock<IRoomRepository> _roomRepository = new();
    private readonly Mock<IGuestRepository> _guestRepository = new();
    private readonly CreateBookingValidator _validator;

    public CreateBookingValidatorTests()
    {
        // Default: room dan guest dianggap ada
        _roomRepository
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _guestRepository
            .Setup(g => g.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _validator = new CreateBookingValidator(
            _roomRepository.Object,
            _guestRepository.Object);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenRoomDoesNotExist()
    {
        // Override default — room tidak ditemukan
        _roomRepository
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateBookingCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateBookingCommand.RoomId) &&
            e.ErrorMessage == "Kamar tidak ditemukan.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenGuestDoesNotExist()
    {
        // Override default — guest tidak ditemukan
        _guestRepository
            .Setup(g => g.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateBookingCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateBookingCommand.GuestId) &&
            e.ErrorMessage == "Tamu tidak ditemukan.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenRoomIdIsEmpty()
    {
        // Arrange
        var command = new CreateBookingCommand(
            Guid.Empty,                                          // ← tidak valid
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateBookingCommand.RoomId));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenGuestIdIsEmpty()
    {
        // Arrange
        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            Guid.Empty,                                          // ← tidak valid
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateBookingCommand.GuestId));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenCheckInIsInThePast()
    {
        // Arrange
        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),   // ← kemarin
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateBookingCommand.CheckIn));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenCheckOutIsBeforeCheckIn()
    {
        // Arrange
        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)));   // ← sebelum check-in

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateBookingCommand.CheckOut));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenCheckOutEqualToCheckIn()
    {
        // Arrange
        var checkIn = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            checkIn,
            checkIn);                                            // ← sama dengan check-in

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateBookingCommand.CheckOut));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenDurationExceedsThirtyNights()
    {
        // Arrange
        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(32)));  // ← 31 malam

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(CreateBookingCommand.CheckOut));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenMultipleFieldsAreInvalid()
    {
        // Arrange — semua field tidak valid sekaligus
        var command = new CreateBookingCommand(
            Guid.Empty,                                          // ← RoomId kosong
            Guid.Empty,                                          // ← GuestId kosong
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),   // ← CheckIn masa lalu
            DateOnly.FromDateTime(DateTime.Today.AddDays(-2)));  // ← CheckOut sebelum CheckIn

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
        result.Errors.Select(e => e.PropertyName).Should().Contain([
            nameof(CreateBookingCommand.RoomId),
            nameof(CreateBookingCommand.GuestId),
            nameof(CreateBookingCommand.CheckIn)
        ]);
    }
}