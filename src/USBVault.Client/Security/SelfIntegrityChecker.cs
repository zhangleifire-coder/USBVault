#nullable enable
using System;
using System.IO;
using System.Security.Cryptography;

namespace USBVault.Client.Security
{
    /// <summary>
    /// Computes and verifies SHA-256 hashes of files for integrity self-checks.
    /// </summary>
    public class SelfIntegrityChecker
    {
        public SelfIntegrityChecker() { }

        /// <summary>
        /// Computes the SHA-256 hash of the entire file at the given path.
        /// </summary>
        public byte[] ComputeFileHash(string path)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(path);
            return sha256.ComputeHash(stream);
        }

        /// <summary>
        /// Compares the current EXE hash to the provided baseline hash.
        /// Uses Environment.ProcessPath, falling back to exePath if provided.
        /// </summary>
        public bool CheckIntegrity(byte[] baselineHash, string? exePath = null)
        {
            string? targetPath = Environment.ProcessPath ?? exePath;
            if (string.IsNullOrEmpty(targetPath))
                throw new InvalidOperationException("Cannot determine executable path.");

            byte[] currentHash = ComputeFileHash(targetPath);

            if (currentHash.Length != baselineHash.Length)
                return false;

            // Constant-time comparison to avoid timing attacks
            int diff = 0;
            for (int i = 0; i < currentHash.Length; i++)
                diff |= currentHash[i] ^ baselineHash[i];

            return diff == 0;
        }

        /// <summary>
        /// Saves the given hash as a hex string to the specified file path.
        /// Intended for one-time baseline generation.
        /// </summary>
        public void SaveBaselineHash(byte[] hash, string path)
        {
            string hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            File.WriteAllText(path, hex);
        }
    }
}