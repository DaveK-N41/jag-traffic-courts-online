﻿using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrafficCourts.Staff.Service.Models;

namespace TrafficCourts.Staff.Service.Services;

public class RedisDisputeLockService : IDisputeLockService
{
    private readonly IConnectionMultiplexer _connection;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<RedisDisputeLockService> _logger;

    public RedisDisputeLockService(
        IConnectionMultiplexer connection, 
        TimeProvider timeProvider,
        ILogger<RedisDisputeLockService> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /*
     * Redis Key and Values
     * 
     *                   
     *                   Lock Id                              Lock Value
     *   +-------------------------------------+       +---------------------------+
     *   | v0:staff:lock-ticket-<TicketNumber> |       | v0:staff:lock-id-<LockId> |
     *   |-------------------------------------| <---> |---------------------------|
     *   |                              LockId |       | LockId                    |
     *   +-------------------------------------+       | TicketNumber              |
     *                                                 | Username                  |
     *                                                 | ExpiryTimeUtc             |
     *                                                 | CreatedAtUtc              |
     *                                                 +---------------------------+  
     * 
     */

    /// <summary>
    /// Get the redis key for the "Lock Id" entry for a given ticket number.
    /// </summary>
    private string LockIdRedisKey(string ticketNumber) => $"v0:staff:lock-ticket-{ticketNumber}"; // holds the lock id

    /// <summary>
    /// Get the redis key for the "Lock Value" entry for a given lock id.
    /// </summary>
    private string LockValueRedisKey(string lockId) => $"v0:staff:lock-id-{lockId}"; // holds the Lock object

    /// <summary>
    /// The lock duration. 5 minutes.
    /// </summary>
    private readonly TimeSpan LockDuration = TimeSpan.FromMinutes(5);

    private IDatabase Database => _connection.GetDatabase(0);

    public Lock? GetLock(string ticketNumber, string username)
    {
        Lock? ticketLock = GetLock(ticketNumber);

        if (ticketLock is null)
        {
            // lock does not exist
            return AquireLock(ticketNumber, username);
        }
        else
        {
            // lock exists already in redis
            if (ticketLock.IsExpired)
            {
                // expired steal the lock
                return AquireLock(ticketNumber, username);
            }

            // not expired yet
            if (ticketLock.Username == username)
            {
                return ticketLock;
            }

            throw new LockIsInUseException(ticketLock.Username, ticketLock);
        }
    }

    public DateTimeOffset? RefreshLock(string lockId, string username)
    {
        Lock? lockToUpdate = GetLockById(lockId);

        if (lockToUpdate is null) return null;

        if (lockToUpdate.Username != username) throw new LockIsInUseException(lockToUpdate.Username, lockToUpdate);

        lockToUpdate.ExpiryTimeUtc = _timeProvider.GetUtcNow().Add(LockDuration);

        // TODO: think how about how all these expiry times work together
        string lockIdKey = LockIdRedisKey(lockToUpdate.TicketNumber);

        Debug.Assert(lockId == lockToUpdate.LockId);

        // Returns 1 on success, 0 on failure setting expiry or key not existing, -1 if the key value didn't match
        long result = (long)Database.ScriptEvaluate(Extend, [lockIdKey], [lockId, (long)LockDuration.TotalMilliseconds], CommandFlags.DemandMaster);

        if (result == 1)
        {
            // save the lock value because the lock was extended
            string lockValueKey = LockValueRedisKey(lockId);
            string lockValueJson = Serialize(lockToUpdate);
            Database.StringSet(lockValueKey, lockValueJson, LockDuration, When.Always, CommandFlags.DemandMaster);

            return lockToUpdate.ExpiryTimeUtc;
        }
        else if (result == 0)
        {
            // failure setting expiry or key not existing
        }
        else if (result == -1)
        {
            // key value didn't match
        }

        return null;

    }

    public void ReleaseLock(string lockId)
    {
        Lock? lockToRelease = GetLockById(lockId);
        if (lockToRelease is null) return;

        string lockIdKey = LockIdRedisKey(lockToRelease.TicketNumber);

        // delete the lock if the lock id matches
        RedisResult result = Database.ScriptEvaluate(Unlock, [lockIdKey], [lockToRelease.LockId], CommandFlags.DemandMaster);

        // remove the value regardless
        string lockValueKey = LockValueRedisKey(lockToRelease.LockId);
        Database.KeyDelete(lockValueKey);
    }


    private Lock CreateLock(string ticketNumber, string username) => new()
    {
        LockId = Guid.NewGuid().ToString("n"),
        TicketNumber = ticketNumber,
        Username = username,
        ExpiryTimeUtc = _timeProvider.GetUtcNow().Add(LockDuration)
    };

    private static string Serialize(Lock ticketLock) =>
        JsonSerializer.Serialize(ticketLock, LockSerializerContext.Default.Lock);

    private static Lock? Deserialize(string value) =>
        JsonSerializer.Deserialize(value!, LockSerializerContext.Default.Lock);



    /// <summary>
    /// Attempts to aquire the lock. Tries to set the lock and if that fails, 
    /// to 
    /// </summary>
    /// <param name="ticketNumber"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    /// <exception cref="LockIsInUseException"></exception>
    private Lock AquireLock(string ticketNumber, string username)
    {
        Lock? ticketLock = CreateLock(ticketNumber, username);
        if (SetLock(ticketLock))
        {
            return ticketLock; // got the lock
        }

        // someone else got the lock already?
        ticketLock = GetLock(ticketNumber);
        if (ticketLock is not null)
        {
            // do we hold the lock?
            if (!ticketLock.IsExpired && ticketLock.Username == username)
            {
                return ticketLock;
            }

            // expired?
            // held by someone else

            throw new LockIsInUseException(ticketLock.Username, ticketLock);
        }

        // problem, why couldn't we get the lock

        throw new LockIsInUseException(ticketLock.Username, ticketLock);
    }

    /// <summary>
    /// Gets the lock for the ticket number.
    /// </summary>
    /// <param name="ticketNumber"></param>
    /// <returns></returns>
    private Lock? GetLock(string ticketNumber)
    {
        // get the lock-id for the ticket number
        string lockIdKey = LockIdRedisKey(ticketNumber);

        RedisValue lockId = Database.StringGet(lockIdKey);
        if (lockId.IsNull)
        {
            return null;
        }

        // now grab the lock object
        Lock? ticketLock = GetLockById(lockId!);
        return ticketLock;
    }

    private Lock? GetLockById(string lockId)
    {
        string lockValueKey = LockValueRedisKey(lockId);
         
        RedisValue lockValueJson = Database.StringGet(lockValueKey);
        if (lockValueJson.IsNull)
        {
            return null;
        }

        Lock? ticketLock = Deserialize(lockValueJson!);
        return ticketLock;
    }

    private bool SetLock(Lock ticketLock)
    {
        // save the lock id in redis
        var lockIdKey = LockIdRedisKey(ticketLock.TicketNumber);
        var redisResult = Database
            .StringSet(lockIdKey, ticketLock.LockId, LockDuration, When.NotExists, CommandFlags.DemandMaster);

        if (!redisResult)
        {
            return false; // couldnt create the lock id key
        }

        // save the lock object in redis
        string lockValueKey = LockValueRedisKey(ticketLock.LockId);

        string lockValueJson = Serialize(ticketLock);
        redisResult = Database
            .StringSet(lockValueKey, lockValueJson, LockDuration, When.Always, CommandFlags.DemandMaster);

        return redisResult;
    }

    private const string Extend = """
        ﻿local currentVal = redis.call('get', KEYS[1])
        if (currentVal == false) then
            return redis.call('set', KEYS[1], ARGV[1], 'PX', ARGV[2]) and 1 or 0
        elseif (currentVal == ARGV[1]) then
            return redis.call('pexpire', KEYS[1], ARGV[2])
        else
            return -1
        end
        """;

    private const string Unlock = """
        ﻿if redis.call('get', KEYS[1]) == ARGV[1] then
        	return redis.call('del', KEYS[1])
        else
        	return 0
        end
        """;
}

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(Lock))]
internal partial class LockSerializerContext : JsonSerializerContext
{
}