using HotelBooking.Domain.Enums;

namespace HotelBooking.Domain.Entities;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid GuestId { get; private set; }
    public DateOnly CheckIn { get; private set; }
    public DateOnly CheckOut { get; private set; }
    public decimal TotalPrice { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public Room Room { get; private set; } = null!;
    public Guest Guest { get; private set; } = null!;

    private Booking() { }

    public static Booking Create(
        Guid roomId,
        Guid guestId,
        DateOnly checkIn,
        DateOnly checkOut,
        decimal pricePerNight)
    {
        if (checkOut <= checkIn)
            throw new ArgumentException("Check-out must be after check-in.");

        var nights = checkOut.DayNumber - checkIn.DayNumber;

        return new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            GuestId = guestId,
            CheckIn = checkIn,
            CheckOut = checkOut,
            TotalPrice = pricePerNight * nights,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled.");

        Status = BookingStatus.Cancelled;
    }
}