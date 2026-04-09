using FluentValidation;

namespace HotelBooking.Application.Bookings.Commands.CreateBooking;

public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("RoomId is required.");

        RuleFor(x => x.GuestId)
            .NotEmpty().WithMessage("GuestId is required.");

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