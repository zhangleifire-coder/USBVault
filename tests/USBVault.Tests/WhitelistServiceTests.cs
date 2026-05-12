using System;
using System.Collections.Generic;
using FluentAssertions;
using USBVault.Client.Models;
using USBVault.Client.Services;
using Xunit;

namespace USBVault.Tests;

public class WhitelistServiceTests
{
    private readonly WhitelistService _sut = new();

    [Fact]
    public void IsAuthorized_FingerprintInList_ReturnsTrue()
    {
        var machine = new AuthorizedMachine("ABC123", DateTime.UtcNow);
        var whitelist = new AuthWhitelist(new List<AuthorizedMachine> { machine });

        var result = _sut.IsAuthorized(whitelist, "ABC123");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthorized_FingerprintNotInList_ReturnsFalse()
    {
        var machine = new AuthorizedMachine("ABC123", DateTime.UtcNow);
        var whitelist = new AuthWhitelist(new List<AuthorizedMachine> { machine });

        var result = _sut.IsAuthorized(whitelist, "XYZ789");

        result.Should().BeFalse();
    }

    [Fact]
    public void CanAddMore_UnderLimit_ReturnsTrue()
    {
        var machines = new List<AuthorizedMachine>
        {
            new("M1", DateTime.UtcNow),
            new("M2", DateTime.UtcNow),
        };
        var whitelist = new AuthWhitelist(machines, 5);

        var result = _sut.CanAddMore(whitelist);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanAddMore_AtLimit_ReturnsFalse()
    {
        var machines = new List<AuthorizedMachine>
        {
            new("M1", DateTime.UtcNow),
            new("M2", DateTime.UtcNow),
            new("M3", DateTime.UtcNow),
            new("M4", DateTime.UtcNow),
            new("M5", DateTime.UtcNow),
        };
        var whitelist = new AuthWhitelist(machines, 5);

        var result = _sut.CanAddMore(whitelist);

        result.Should().BeFalse();
    }

    [Fact]
    public void AddMachine_WithinLimit_Succeeds()
    {
        var machines = new List<AuthorizedMachine>
        {
            new("M1", DateTime.UtcNow),
        };
        var whitelist = new AuthWhitelist(machines, 5);

        var result = _sut.AddMachine(whitelist, "M2");

        result.Machines.Should().HaveCount(2);
        result.Machines.Should().Contain(m => m.ShortId == "M2");
        whitelist.Machines.Should().HaveCount(1); // original unchanged
    }

    [Fact]
    public void AddMachine_AtLimit_Throws()
    {
        var machines = new List<AuthorizedMachine>
        {
            new("M1", DateTime.UtcNow),
            new("M2", DateTime.UtcNow),
            new("M3", DateTime.UtcNow),
            new("M4", DateTime.UtcNow),
            new("M5", DateTime.UtcNow),
        };
        var whitelist = new AuthWhitelist(machines, 5);

        Action act = () => _sut.AddMachine(whitelist, "M6");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveMachine_Existing_ReturnsNewWhitelistWithoutIt()
    {
        var machines = new List<AuthorizedMachine>
        {
            new("M1", DateTime.UtcNow),
            new("M2", DateTime.UtcNow),
        };
        var whitelist = new AuthWhitelist(machines, 5);

        var result = _sut.RemoveMachine(whitelist, "M1");

        result.Machines.Should().HaveCount(1);
        result.Machines.Should().NotContain(m => m.ShortId == "M1");
        whitelist.Machines.Should().HaveCount(2); // original unchanged
    }

    [Fact]
    public void GetQuotaStatus_ReturnsCorrectCounts()
    {
        var machines = new List<AuthorizedMachine>
        {
            new("M1", DateTime.UtcNow),
            new("M2", DateTime.UtcNow),
            new("M3", DateTime.UtcNow),
        };
        var whitelist = new AuthWhitelist(machines, 5);

        var (currentCount, maxAllowed) = _sut.GetQuotaStatus(whitelist);

        currentCount.Should().Be(3);
        maxAllowed.Should().Be(5);
    }
}