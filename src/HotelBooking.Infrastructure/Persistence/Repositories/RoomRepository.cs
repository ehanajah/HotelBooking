using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Persistence.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _db;

    public RoomRepository(AppDbContext db) => _db = db;

    public async Task<Room?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _db.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<bool> HasConflictAsync(
        Guid roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken ct)
        => await _db.Bookings.AnyAsync(b =>
            b.RoomId == roomId &&
            b.Status == BookingStatus.Confirmed &&
            b.CheckIn < checkOut &&
            b.CheckOut > checkIn, ct);

    public async Task<List<Room>> GetAvailableRoomsAsync(
        DateOnly checkIn,
        DateOnly checkOut,
        string? roomType,
        CancellationToken ct)
    {
        var bookedRoomIds = await _db.Bookings
        .Where(b =>
            b.Status == BookingStatus.Confirmed &&
            b.CheckIn < checkOut &&
            b.CheckOut > checkIn)
        .Select(b => b.RoomId)
        .Distinct()
        .ToListAsync(ct);

        var query = _db.Rooms
            .Where(r => r.IsAvailable && !bookedRoomIds.Contains(r.Id));

        if (!string.IsNullOrEmpty(roomType))
            query = query.Where(r => r.RoomType == roomType);

        return await query.ToListAsync(ct);
    }
}