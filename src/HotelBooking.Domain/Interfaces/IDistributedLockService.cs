namespace HotelBooking.Domain.Interfaces;

public interface IDistributedLockService
{
    Task<bool> AcquireAsync(string key, string ownerId, TimeSpan? expiry = null);
    Task ReleaseAsync(string key, string ownerId);
}