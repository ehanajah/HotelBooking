using MediatR;

namespace HotelBooking.Application.Bookings.Commands.CancelBooking;

public record CancelBookingCommand(
    Guid BookingId
) : IRequest<CancelBookingResult>;

public record CancelBookingResult(
    Guid BookingId,
    bool IsCancelled
);