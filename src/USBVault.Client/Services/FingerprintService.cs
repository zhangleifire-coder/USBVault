#nullable enable
using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace USBVault.Client.Services;

public class FingerprintService
{
    public string GenerateFingerprint()
    {
        var processorId = GetWmiProperty("Win32_Processor", "ProcessorId") ?? "";
        var baseBoardSerial = GetWmiProperty("Win32_BaseBoard", "SerialNumber") ?? "";
        var biosVersion = GetWmiProperty("Win32_BIOS", "SMBIOSBIOSVersion") ?? "";

        var combined = string.Join("|", processorId, baseBoardSerial, biosVersion);
        var bytes = Encoding.UTF8.GetBytes(combined);
        var hash = SHA256.HashData(bytes);

        // Return first 40 hex chars (lowercase) as specified
        return Convert.ToHexString(hash)[..40].ToLowerInvariant();
    }

    public string GetShortId(string fullFingerprint)
    {
        return fullFingerprint[..8].ToUpperInvariant();
    }

    public string? GetWmiProperty(string wmiClass, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}");
            foreach (var obj in searcher.Get())
            {
                var value = obj[property]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }
        }
        catch
        {
            // Safely return null on any WMI failure
        }
        return null;
    }
}
