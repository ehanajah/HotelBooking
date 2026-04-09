using MediatR;

namespace HotelBooking.Application.Bookings.Queries.GetBookingById;

public record GetBookingByIdQuery(
    Guid BookingId
): IRequest<BookingDto>;

public record BookingDto(
    Guid Id,
    Guid RoomId,
    Guid GuestId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    decimal TotalPrice,
    string RoomNumber,
    string GuestName
);