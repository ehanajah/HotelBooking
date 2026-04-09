using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Domain.Enums;
using MediatR;

namespace HotelBooking.Application.Bookings.Queries.GetAvailableRooms;

public class GetAvailableRoomsHandler
    : IRequestHandler<GetAvailableRoomsQuery, List<RoomDto>>
{
    private readonly IRoomRepository _roomRepository;

    public GetAvailableRoomsHandler(IRoomRepository roomRepository) => _roomRepository = roomRepository;

    public async Task<List<RoomDto>> Handle(
        GetAvailableRoomsQuery query,
        CancellationToken ct)
    {
        var nights = query.CheckOut.DayNumber - query.CheckIn.DayNumber;

        // Cari kamar yang tidak punya booking aktif di rentang tanggal ini
        var rooms = await _roomRepository.GetAvailableRoomsAsync(
            query.CheckIn,
            query.CheckOut,
            query.RoomType,
            ct);

        return rooms.Select(r => new RoomDto(
            r.Id,
            r.RoomNumber,
            r.RoomType,
            r.PricePerNight,
            r.PricePerNight * nights))
        .ToList();
    }
}