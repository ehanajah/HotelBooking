using FluentValidation;
using HotelBooking.Application.Common.Interfaces;

namespace HotelBooking.Application.Bookings.Commands.CreateBooking;

public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator(
        IRoomRepository roomRepository,
        IGuestRepository guestRepository)
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("RoomId tidak boleh kosong.")
            .MustAsync(async (roomId, ct) =>
                await roomRepository.ExistsAsync(roomId, ct))
            .WithMessage("Kamar tidak ditemukan.");

        RuleFor(x => x.GuestId)
            .NotEmpty().WithMessage("GuestId tidak boleh kosong.")
            .MustAsync(async (guestId, ct) =>
                await guestRepository.ExistsAsync(guestId, ct))
            .WithMessage("Tamu tidak ditemukan.");

        RuleFor(x => x.CheckIn)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Check-in cannot be in the past.");

        RuleFor(x => x.CheckOut)
            .GreaterThan(x => x.CheckIn)
            .WithMessage("Check-out must be after check-in.");

        RuleFor(x => x.CheckOut)
            .LessThanOrEqualTo(x => x.CheckIn.AddDays(30))
            .WithMessage("Duration of stay cannot exceed 30 nights.");
    }
}