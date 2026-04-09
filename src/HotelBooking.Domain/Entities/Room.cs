using HotelBooking.Domain.Exceptions;

namespace HotelBooking.Domain.Entities;

public class Room
{
    public Guid Id { get; private set; }
    public string RoomNumber { get; private set; } = string.Empty;
    public string RoomType { get; private set; } = string.Empty;
    public decimal PricePerNight { get; private set; }
    public bool IsAvailable { get; private set; }

    // EF Core menggunakan RowVersion untuk optimistic concurrency.
    // Setiap UPDATE otomatis increment nilai ini.
    // Jika dua transaksi membaca lalu update row yang sama,
    // yang kedua akan mendapat DbUpdateConcurrencyException.
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // Private constructor — hanya bisa dibuat lewat factory method
    private Room() { }

    public static Room Create(
        string roomNumber,
        string roomType,
        decimal pricePerNight)
    {
        return new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = roomNumber,
            RoomType = roomType,
            PricePerNight = pricePerNight,
            IsAvailable = true
        };
    }

    public void MarkAsBooked()
    {
        if (!IsAvailable)
            throw new RoomUnavailableException(Id, RoomNumber);

        IsAvailable = false;
    }

    public void MarkAsAvailable()
    {
        IsAvailable = true;
    }
}