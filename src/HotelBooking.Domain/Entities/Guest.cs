namespace HotelBooking.Domain.Entities;

public class Guest
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();
    private Guest() {}
    public static Guest Create(string name, string email, string phoneNumber)
    {
        return new Guest
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PhoneNumber = phoneNumber
        };
    }
}