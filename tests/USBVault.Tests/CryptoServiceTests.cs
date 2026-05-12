using System;
using FluentAssertions;
using Xunit;
using USBVault.Client.Services;

namespace USBVault.Tests;

public class CryptoServiceTests
{
    [Fact]
    public void EncryptAndDecrypt_RoundTrip_PreservesData()
    {
        // Arrange
        var sut = new CryptoService();
        byte[] plaintext = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x21 }; // "Hello!"
        byte[] key = new byte[32]; // 256-bit key
        new Random(42).NextBytes(key);

        // Act
        byte[] ciphertext = sut.Encrypt(plaintext, key);
        byte[] decrypted = sut.Decrypt(ciphertext, key);

        // Assert
        decrypted.Should().Equal(plaintext);
    }

    [Fact]
    public void Encrypt_ProducesDifferentOutputEachTime()
    {
        // Arrange
        var sut = new CryptoService();
        byte[] plaintext = new byte[] { 0x54, 0x65, 0x73, 0x74, 0x44, 0x61, 0x74, 0x61 };
        byte[] key = new byte[32];
        new Random(99).NextBytes(key);

        // Act
        byte[] ciphertext1 = sut.Encrypt(plaintext, key);
        byte[] ciphertext2 = sut.Encrypt(plaintext, key);

        // Assert — same plaintext + key must produce different ciphertexts (random IV)
        ciphertext1.Should().NotEqual(ciphertext2);
    }

    [Fact]
    public void Decrypt_WithWrongKey_Throws()
    {
        // Arrange
        var sut = new CryptoService();
        byte[] plaintext = new byte[] { 0x53, 0x65, 0x63, 0x72, 0x65, 0x74 };
        byte[] correctKey = new byte[32];
        byte[] wrongKey = new byte[32];
        new Random(77).NextBytes(correctKey);
        new Random(88).NextBytes(wrongKey); // different key

        // Act
        byte[] ciphertext = sut.Encrypt(plaintext, correctKey);

        // Assert
        Action act = () => sut.Decrypt(ciphertext, wrongKey);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void DeriveKey_ProducesSameKeyFromSamePasswordAndSalt()
    {
        // Arrange
        var sut = new CryptoService();
        string password = "SuperSecret123!";
        byte[] salt = new byte[16];
        new Random(123).NextBytes(salt);

        // Act
        byte[] key1 = sut.DeriveKey(password, salt);
        byte[] key2 = sut.DeriveKey(password, salt);

        // Assert
        key1.Should().Equal(key2);
        key1.Should().HaveCount(32);
    }

    [Fact]
    public void DeriveKey_DifferentSalts_ProduceDifferentKeys()
    {
        // Arrange
        var sut = new CryptoService();
        string password = "SamePassword";
        byte[] salt1 = new byte[16];
        byte[] salt2 = new byte[16];
        new Random(1).NextBytes(salt1);
        new Random(2).NextBytes(salt2);

        // Act
        byte[] key1 = sut.DeriveKey(password, salt1);
        byte[] key2 = sut.DeriveKey(password, salt2);

        // Assert
        key1.Should().NotEqual(key2);
    }
}