#nullable enable

using System;
using System.Security.Cryptography;
using System.Text;

namespace USBVault.Client.Services;

/// <summary>
/// PBKDF2-SHA256 password authentication service.
/// </summary>
public class AdminAuthService
{
    private const int Pbkdf2Iterations = 100_000;
    private const int HashSizeBytes = 32; // 256 bits

    private byte[]? _storedHash;
    private byte[]? _salt;

    /// <summary>
    /// True once a password has been set.
    /// </summary>
    public bool IsInitialized => _storedHash != null && _salt != null;

    /// <summary>
    /// Sets the admin password, generating a PBKDF2-SHA256 hash from the given password and salt.
    /// </summary>
    /// <param name="password">The plaintext password.</param>
    /// <param name="saltStr">The salt string (converted to UTF-8 bytes).</param>
    public void SetPassword(string password, string saltStr)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));
        if (saltStr == null) throw new ArgumentNullException(nameof(saltStr));

        _salt = Encoding.UTF8.GetBytes(saltStr);
        _storedHash = ComputeHash(password, _salt);
    }

    /// <summary>
    /// Verifies the given password against the stored hash.
    /// Uses a timing-safe comparison to prevent timing attacks.
    /// </summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <returns>True if the password matches; false otherwise.</returns>
    public bool VerifyPassword(string password)
    {
        if (!IsInitialized || _salt == null || _storedHash == null)
            return false;

        byte[] incomingHash = ComputeHash(password, _salt);
        return CryptographicOperations.FixedTimeEquals(incomingHash, _storedHash);
    }

    /// <summary>
    /// Changes the admin password after verifying the old password.
    /// </summary>
    /// <param name="oldPassword">The current password (must be correct).</param>
    /// <param name="newPassword">The new password to set.</param>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="oldPassword"/> is incorrect.</exception>
    public void ChangePassword(string oldPassword, string newPassword)
    {
        if (!VerifyPassword(oldPassword))
            throw new InvalidOperationException("Old password is incorrect.");

        // Generate a new random salt for the new password
        byte[] newSalt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(newSalt);
        }

        _salt = newSalt;
        _storedHash = ComputeHash(newPassword, _salt);
    }

    private static byte[] ComputeHash(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password: password,
            salt: salt,
            iterations: Pbkdf2Iterations,
            HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(HashSizeBytes);
    }
}
