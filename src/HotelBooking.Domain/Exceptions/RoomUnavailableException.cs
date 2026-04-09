namespace HotelBooking.Domain.Exceptions;

public class RoomUnavailableException : Exception
{
    public RoomUnavailableException(Guid roomId, string roomNumber)
        : base($"Room {roomNumber} (ID: {roomId}) unavailable.") { }
}