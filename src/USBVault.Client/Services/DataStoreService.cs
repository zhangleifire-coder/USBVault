using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using USBVault.Client.Models;

namespace USBVault.Client.Services;

/// <summary>
/// Reads and writes the encrypted whitelist to the USB data partition (vault/).
/// </summary>
public class DataStoreService
{
    private const string VaultDir = "vault";
    private const string WhitelistFileName = "whitelist.enc";
    private const string FirmwareDir = ".firmware";
    private const string KeyFileName = "key.bin";
    private const string EnvKeyVar = "USBVAULT_KEY";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false
    };

    private readonly CryptoService _crypto;

    public DataStoreService()
    {
        _crypto = new CryptoService();
    }

    /// <summary>
    /// Reads and decrypts the whitelist from the USB drive's vault partition.
    /// </summary>
    /// <param name="driveLetter">Drive letter or mount point (e.g. "D:" or "/Volumes/USB").</param>
    /// <returns>AuthWhitelist on success; empty whitelist on any error.</returns>
    public AuthWhitelist LoadWhitelist(string driveLetter)
    {
        string path = GetWhitelistPath(driveLetter);

#if NET8_0_WINDOWS
        if (!File.Exists(path))
            return new AuthWhitelist(new List<AuthorizedMachine>(), 5);

        try
        {
            byte[] key = GetFirmwareKey(driveLetter);
            byte[] encrypted = File.ReadAllBytes(path);
            byte[] jsonBytes = _crypto.Decrypt(encrypted, key);

            var data = JsonSerializer.Deserialize<WhitelistData>(jsonBytes, JsonOptions);
            if (data?.Machines == null)
                return new AuthWhitelist(new List<AuthorizedMachine>(), 5);

            var machines = new List<AuthorizedMachine>(data.Machines.Count);
            foreach (var entry in data.Machines)
                machines.Add(new AuthorizedMachine(entry.ShortId, entry.RegisteredAt));

            return new AuthWhitelist(machines, 5);
        }
        catch
        {
            return new AuthWhitelist(new List<AuthorizedMachine>(), 5);
        }
#else
        return new AuthWhitelist(new List<AuthorizedMachine>(), 5);
#endif
    }

    /// <summary>
    /// Serializes and encrypts the whitelist, writing it to the USB drive's vault partition.
    /// </summary>
    /// <param name="driveLetter">Drive letter or mount point.</param>
    /// <param name="whitelist">The whitelist to persist.</param>
    public void SaveWhitelist(string driveLetter, AuthWhitelist whitelist)
    {
#if NET8_0_WINDOWS
        byte[] key = GetFirmwareKey(driveLetter);

        string vaultPath = Path.Combine(driveLetter, VaultDir);
        if (!Directory.Exists(vaultPath))
            Directory.CreateDirectory(vaultPath);

        var data = new WhitelistData(whitelist.Machines);
        byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(data, JsonOptions);
        byte[] encrypted = _crypto.Encrypt(jsonBytes, key);

        string path = GetWhitelistPath(driveLetter);
        File.WriteAllBytes(path, encrypted);
#endif
    }

    /// <summary>
    /// Locates the 32-byte firmware key.
    /// Priority: (1) driveLetter/.firmware/key.bin, (2) USBVAULT_KEY env var.
    /// </summary>
    private byte[] GetFirmwareKey(string driveLetter)
    {
        // Priority 1: key.bin on the drive
        string keyPath = Path.Combine(driveLetter, FirmwareDir, KeyFileName);
#if NET8_0_WINDOWS
        if (File.Exists(keyPath))
        {
            byte[] key = File.ReadAllBytes(keyPath);
            if (key.Length == 32)
                return key;
        }
#endif

        // Priority 2: environment variable
        string envValue = Environment.GetEnvironmentVariable(EnvKeyVar) ?? string.Empty;
        if (!string.IsNullOrEmpty(envValue))
        {
            try
            {
                byte[] key = Convert.FromHexString(envValue);
                if (key.Length == 32)
                    return key;
            }
            catch (FormatException)
            {
                // fall through to throw
            }
        }

        throw new InvalidOperationException("Cannot locate firmware key: " +
            $"neither '{keyPath}' (32 bytes) nor env var '{EnvKeyVar}' found.");
    }

    private static string GetWhitelistPath(string driveLetter)
        => Path.Combine(driveLetter, VaultDir, WhitelistFileName);
}