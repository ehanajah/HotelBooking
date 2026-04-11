using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Common.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
    Task<bool> HasConflictAsync(Guid roomId, DateOnly checkIn, DateOnly checkOut, CancellationToken ct);
    Task<List<Room>> GetAvailableRoomsAsync(
        DateOnly checkIn,
        DateOnly checkOut,
        string? roomType,
        CancellationToken ct);
}