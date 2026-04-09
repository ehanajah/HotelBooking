using MediatR;

namespace HotelBooking.Application.Bookings.Commands.CreateBooking;

// Command adalah "niat" user — tidak mengandung logic
public record CreateBookingCommand(
    Guid RoomId,
    Guid GuestId,
    DateOnly CheckIn,
    DateOnly CheckOut
) : IRequest<CreateBookingResult>;

public record CreateBookingResult(
    Guid BookingId,
    decimal TotalPrice,
    string RoomNumber
);