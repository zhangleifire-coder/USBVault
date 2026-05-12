using System;
using FluentAssertions;
using Xunit;
using USBVault.Client.Services;

namespace USBVault.Tests;

public class AdminAuthServiceTests
{
    [Fact]
    public void SetPassword_StoresHash_IsInitialized_IsTrue()
    {
        // Arrange
        var sut = new AdminAuthService();
        const string password = "MySecretPass123!";
        const string salt = "randomsalt12345678";

        // Act
        sut.SetPassword(password, salt);

        // Assert
        sut.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Correct_ReturnsTrue()
    {
        // Arrange
        var sut = new AdminAuthService();
        const string password = "CorrectHorseBattery";
        const string salt = "fixed-salt-for-test";
        sut.SetPassword(password, salt);

        // Act
        bool result = sut.VerifyPassword(password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Wrong_ReturnsFalse()
    {
        // Arrange
        var sut = new AdminAuthService();
        const string correctPassword = "CorrectPassword!";
        const string wrongPassword = "WrongPassword!";
        const string salt = "another-fixed-salt";
        sut.SetPassword(correctPassword, salt);

        // Act
        bool result = sut.VerifyPassword(wrongPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ChangePassword_WrongOld_Throws()
    {
        // Arrange
        var sut = new AdminAuthService();
        const string correctPassword = "OldPassword123!";
        const string wrongOldPassword = "NotTheOldPassword!";
        const string newPassword = "NewPassword456!";
        const string salt = "salt-for-change-test";
        sut.SetPassword(correctPassword, salt);

        // Act
        Action act = () => sut.ChangePassword(wrongOldPassword, newPassword);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ChangePassword_CorrectOld_UpdatesPassword()
    {
        // Arrange
        var sut = new AdminAuthService();
        const string oldPassword = "OldPass777!";
        const string newPassword = "NewPass999!";
        const string salt = "salt-for-update-test";
        sut.SetPassword(oldPassword, salt);

        // Act
        sut.ChangePassword(oldPassword, newPassword);

        // Assert — old password should no longer verify
        bool oldShouldFail = sut.VerifyPassword(oldPassword);
        oldShouldFail.Should().BeFalse();

        // Assert — new password should verify
        bool newShouldPass = sut.VerifyPassword(newPassword);
        newShouldPass.Should().BeTrue();
    }
}
