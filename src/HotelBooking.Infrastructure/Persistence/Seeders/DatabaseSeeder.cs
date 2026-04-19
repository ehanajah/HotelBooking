using Bogus;
using HotelBooking.Domain.Entities;

namespace HotelBooking.Infrastructure.Persistence.Seeders;

public class DatabaseSeeder
{
    private readonly AppDbContext _db;

    public DatabaseSeeder(AppDbContext db) => _db = db;

    public async Task SeedAsync(bool fresh = false)
    {
        if (fresh)
        {
            Console.WriteLine("  Menghapus data lama...");
            _db.Bookings.RemoveRange(_db.Bookings);
            _db.Guests.RemoveRange(_db.Guests);
            _db.Rooms.RemoveRange(_db.Rooms);
            await _db.SaveChangesAsync();
        }

        await SeedRoomsAsync();
        await SeedGuestsAsync();
    }

    private async Task SeedRoomsAsync()
    {
        if (_db.Rooms.Any())
        {
            Console.WriteLine("  Rooms already exist, skip.");
            return;
        }

        var rooms = new[]
        {
            Room.Create("101", "Standard",   500_000),
            Room.Create("102", "Standard",   500_000),
            Room.Create("103", "Standard",   500_000),
            Room.Create("201", "Deluxe",     800_000),
            Room.Create("202", "Deluxe",     800_000),
            Room.Create("301", "Suite",    1_500_000),
            Room.Create("302", "Suite",    1_500_000),
        };

        _db.Rooms.AddRange(rooms);
        await _db.SaveChangesAsync();
        Console.WriteLine($"  {rooms.Length} rooms successfully added.");
    }

    private async Task SeedGuestsAsync()
    {
        if (_db.Guests.Any())
        {
            Console.WriteLine("  Guests already exist, skip.");
            return;
        }

        var faker = new Faker<Guest>("en")
            .CustomInstantiator(f => Guest.Create(
                f.Name.FullName(),
                f.Internet.Email(),
                f.Phone.PhoneNumber("08##########")));

        var guests = faker.Generate(10);
        _db.Guests.AddRange(guests);
        await _db.SaveChangesAsync();
        Console.WriteLine($"  {guests.Count} guests successfully added.");
    }
}