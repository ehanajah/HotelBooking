using HotelBooking.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Persistence.Repositories;

public class GuestRepository : IGuestRepository
{
    private readonly AppDbContext _db;

    public GuestRepository(AppDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
        => await _db.Guests.AnyAsync(g => g.Id == id, ct);
}