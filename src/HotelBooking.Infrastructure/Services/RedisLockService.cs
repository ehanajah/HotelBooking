using HotelBooking.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HotelBooking.Infrastructure.Services;

public class RedisLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisLockService> _logger;

    // Lua script untuk atomic check-and-delete
    // Hanya bisa release lock yang kita sendiri yang pegang
    private const string ReleaseScript = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end";

    public RedisLockService(
        IConnectionMultiplexer redis,
        ILogger<RedisLockService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> AcquireAsync(
        string key,
        string ownerId,
        TimeSpan? expiry = null)
    {
        var db = _redis.GetDatabase();
        var lockExpiry = expiry ?? TimeSpan.FromSeconds(10);

        // SET key ownerId NX EX {seconds}
        // NX = hanya set jika key belum ada (Not eXists)
        var acquired = await db.StringSetAsync(
            key,
            ownerId,
            lockExpiry,
            When.NotExists);

        if (!acquired)
            _logger.LogDebug("Lock '{Key}' tidak bisa diambil — sedang dipakai.", key);

        return acquired;
    }

    public async Task ReleaseAsync(string key, string ownerId)
    {
        var db = _redis.GetDatabase();

        try
        {
            await db.ScriptEvaluateAsync(
                ReleaseScript,
                new RedisKey[] { key },
                new RedisValue[] { ownerId });
        }
        catch (Exception ex)
        {
            // Log tapi jangan rethrow — lock akan expire sendiri
            _logger.LogWarning(ex, "Gagal release lock '{Key}'.", key);
        }
    }
}