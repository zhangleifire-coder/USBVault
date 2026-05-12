using FluentAssertions;
using USBVault.Client.Services;
using Xunit;

namespace USBVault.Tests;

public class FingerprintServiceTests
{
    [Fact]
    public void GenerateFingerprint_ShouldReturn40CharHexString()
    {
        // Arrange
        var service = new FingerprintService();

        // Act
        var result = service.GenerateFingerprint();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(40);
        result.Should().MatchRegex("^[0-9a-f]{40}$");
    }

    [Fact]
    public void GenerateFingerprint_ShouldBeConsistent()
    {
        // Arrange
        var service = new FingerprintService();

        // Act
        var first = service.GenerateFingerprint();
        var second = service.GenerateFingerprint();

        // Assert
        first.Should().Be(second);
    }

    [Fact]
    public void GetShortId_ShouldReturnFirst8CharsUppercased()
    {
        // Arrange
        var service = new FingerprintService();
        var fullFingerprint = "abcdef1234567890abcdef1234567890abcdef12";

        // Act
        var result = service.GetShortId(fullFingerprint);

        // Assert
        result.Should().Be("ABCDEF12");
        result.Should().HaveLength(8);
        result.Should().MatchRegex("^[0-9A-F]{8}$");
    }
}
