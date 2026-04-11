namespace HotelBooking.Application.Common.Interfaces;

public interface IGuestRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
}