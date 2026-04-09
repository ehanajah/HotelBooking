using MediatR;

namespace HotelBooking.Application.Bookings.Queries.GetAvailableRooms;

public record GetAvailableRoomsQuery(
    DateOnly CheckIn,
    DateOnly CheckOut,
    string? RoomType = null
) : IRequest<List<RoomDto>>;

public record RoomDto(
    Guid Id,
    string RoomNumber,
    string RoomType,
    decimal PricePerNight,
    decimal TotalPrice
);