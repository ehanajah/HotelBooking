using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Domain.Interfaces;
using MediatR;

namespace HotelBooking.Application.Bookings.Commands.CreateBooking;

public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, CreateBookingResult>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IDistributedLockService _lockService;

    public CreateBookingHandler(
        IRoomRepository roomRepository,
        IBookingRepository bookingRepository,
        IDistributedLockService lockService)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
        _lockService = lockService;
    }
    public async Task<CreateBookingResult> Handle(
        CreateBookingCommand cmd,
        CancellationToken ct)
    {
        // ── STRATEGI: Redis Distributed Lock ─────────────────────────────
        // Kunci resource "kamar X" sebelum kita lakukan apapun.
        // Maksimal 1 request bisa memegang lock ini pada satu waktu.
        // Request lain yang coba booking kamar yang sama akan langsung ditolak.
        //
        // Alternatif: gunakan hanya Optimistic Locking (RowVersion) tanpa Redis
        // untuk beban normal. Tambah Redis untuk flash deal / promo.
        // ─────────────────────────────────────────────────────────────────

        var lockKey = $"room-lock:{cmd.RoomId}";
        var lockOwner = Guid.NewGuid().ToString(); // ID unik untuk request ini

        var acquired = await _lockService.AcquireAsync(
            lockKey,
            lockOwner,
            expiry: TimeSpan.FromSeconds(15));

        if (!acquired)
        {
            // Ada request lain sedang memproses kamar ini
            // Lempar exception yang sama agar response ke user konsisten
            throw new RoomUnavailableException(cmd.RoomId, "this room");
        }

        try
        {
            return await ProcessBookingAsync(cmd, ct);
        }
        finally
        {
            // Selalu release lock — bahkan jika terjadi exception
            await _lockService.ReleaseAsync(lockKey, lockOwner);
        }
    }

    private async Task<CreateBookingResult> ProcessBookingAsync(
        CreateBookingCommand cmd,
        CancellationToken ct)
    {
        // Ambil room — EF Core secara otomatis melacak RowVersion
        var room = await _roomRepository.GetByIdAsync(cmd.RoomId, ct)
            ?? throw new NotFoundException(nameof(Room), cmd.RoomId);

        // Validasi tambahan: cek tidak ada booking aktif untuk tanggal yang sama
        var hasConflict = await _roomRepository.HasConflictAsync(
            cmd.RoomId, cmd.CheckIn, cmd.CheckOut, ct);

        if (hasConflict)
            throw new RoomUnavailableException(cmd.RoomId, room.RoomNumber);

        // Domain logic: tandai kamar sebagai sudah dipesan
        // Ini juga throw RoomUnavailableException jika IsAvailable == false
        room.MarkAsBooked();

        // Buat booking baru
        var booking = Booking.Create(
            cmd.RoomId,
            cmd.GuestId,
            cmd.CheckIn,
            cmd.CheckOut,
            room.PricePerNight);

        await _bookingRepository.AddAsync(booking, ct);
        await _bookingRepository.SaveChangesAsync(ct);

        return new CreateBookingResult(
            booking.Id,
            booking.TotalPrice,
            room.RoomNumber);
    }
}