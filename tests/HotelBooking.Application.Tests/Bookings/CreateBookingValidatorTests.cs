using FluentAssertions;
using HotelBooking.Application.Bookings.Commands.CreateBooking;

namespace HotelBooking.Application.Tests.Bookings;

public class CreateBookingValidatorTests
{
    private readonly CreateBookingValidator _validator = new();

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