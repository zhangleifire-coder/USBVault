using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace USBVault.Client.Services;

/// <summary>
/// AES-256-CBC encryption service.
/// </summary>
public class CryptoService
{
    private const int KeySizeBytes = 32; // 256 bits
    private const int IvSizeBytes = 16;  // 128 bits
    private const int Pbkdf2Iterations = 100_000;

    /// <summary>
    /// Encrypts plaintext using AES-256-CBC.
    /// A random 16-byte IV is generated and prepended to the ciphertext.
    /// </summary>
    /// <param name="plaintext">Data to encrypt.</param>
    /// <param name="key">32-byte (256-bit) encryption key.</param>
    /// <returns>IV || ciphertext (IV prepended).</returns>
    public byte[] Encrypt(byte[] plaintext, byte[] key)
    {
        if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (key.Length != KeySizeBytes)
            throw new ArgumentException($"Key must be {KeySizeBytes} bytes.", nameof(key));

        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;

        // Generate cryptographically random IV
        byte[] iv = new byte[IvSizeBytes];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }
        aes.IV = iv;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();

        // Write IV first, then ciphertext
        ms.Write(iv, 0, iv.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(plaintext, 0, plaintext.Length);
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Decrypts ciphertext produced by Encrypt.
    /// Extracts the first 16 bytes as IV and decrypts the rest.
    /// </summary>
    /// <param name="ciphertext">IV || encrypted data.</param>
    /// <param name="key">32-byte (256-bit) decryption key.</param>
    /// <returns>Decrypted plaintext bytes.</returns>
    public byte[] Decrypt(byte[] ciphertext, byte[] key)
    {
        if (ciphertext == null) throw new ArgumentNullException(nameof(ciphertext));
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (key.Length != KeySizeBytes)
            throw new ArgumentException($"Key must be {KeySizeBytes} bytes.", nameof(key));
        if (ciphertext.Length < IvSizeBytes)
            throw new ArgumentException("Ciphertext too short to contain IV.", nameof(ciphertext));

        // Extract IV (first 16 bytes)
        byte[] iv = new byte[IvSizeBytes];
        Buffer.BlockCopy(ciphertext, 0, iv, 0, IvSizeBytes);

        // Remaining bytes are the actual ciphertext
        byte[] encryptedData = new byte[ciphertext.Length - IvSizeBytes];
        Buffer.BlockCopy(ciphertext, IvSizeBytes, encryptedData, 0, encryptedData.Length);

        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(encryptedData);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

        using var result = new MemoryStream();
        cs.CopyTo(result);
        return result.ToArray();
    }

    /// <summary>
    /// Derives a 32-byte key from a password and salt using PBKDF2-SHA256.
    /// </summary>
    /// <param name="password">User password.</param>
    /// <param name="salt">Salt bytes (recommended: 16+ bytes).</param>
    /// <returns>32-byte derived key.</returns>
    public byte[] DeriveKey(string password, byte[] salt)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));
        if (salt == null) throw new ArgumentNullException(nameof(salt));

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password: password,
            salt: salt,
            iterations: Pbkdf2Iterations,
            HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(KeySizeBytes);
    }
}