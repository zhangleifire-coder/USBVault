using System;
using System.Linq;
using USBVault.Client.Models;

namespace USBVault.Client.Services;

/// <summary>
/// Immutable whitelist management service for authorized machines.
/// </summary>
public class WhitelistService
{
    /// <summary>
    /// Case-insensitive check if shortId is in the machines list.
    /// </summary>
    public bool IsAuthorized(AuthWhitelist whitelist, string shortId)
    {
        return whitelist.Machines.Any(
            m => string.Equals(m.ShortId, shortId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// True if machines.Count &lt; MaxAllowed.
    /// </summary>
    public bool CanAddMore(AuthWhitelist whitelist)
    {
        return whitelist.Machines.Count < whitelist.MaxAllowed;
    }

    /// <summary>
    /// Returns a new AuthWhitelist with the machine added.
    /// Throws InvalidOperationException if already at limit.
    /// </summary>
    public AuthWhitelist AddMachine(AuthWhitelist whitelist, string shortId)
    {
        if (!CanAddMore(whitelist))
            throw new InvalidOperationException("Whitelist has reached its maximum capacity.");

        var newMachines = whitelist.Machines.Append(
            new AuthorizedMachine(shortId, DateTime.UtcNow)).ToList();

        return new AuthWhitelist(newMachines, whitelist.MaxAllowed);
    }

    /// <summary>
    /// Returns a new AuthWhitelist with the machine removed (case-insensitive match).
    /// </summary>
    public AuthWhitelist RemoveMachine(AuthWhitelist whitelist, string shortId)
    {
        var newMachines = whitelist.Machines
            .Where(m => !string.Equals(m.ShortId, shortId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new AuthWhitelist(newMachines, whitelist.MaxAllowed);
    }

    /// <summary>
    /// Returns (currentCount, maxAllowed) tuple.
    /// </summary>
    public (int CurrentCount, int MaxAllowed) GetQuotaStatus(AuthWhitelist whitelist)
    {
        return (whitelist.Machines.Count, whitelist.MaxAllowed);
    }
}