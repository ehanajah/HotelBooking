using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;

    public BookingRepository(AppDbContext db) => _db = db;

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _db.Bookings
            .Include(b => b.Room)
            .Include(b => b.Guest)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task AddAsync(Booking booking, CancellationToken ct)
        => await _db.Bookings.AddAsync(booking, ct);

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Data telah diubah oleh proses lain.");
        }
    }
}