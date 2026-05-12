using System;
using System.Collections.Generic;

namespace USBVault.Client.Models;

public record AuthorizedMachine(string ShortId, DateTime RegisteredAt);

public record AuthWhitelist(List<AuthorizedMachine> Machines, int MaxAllowed = 5);

public record WhitelistData(List<MachineEntry> Machines);

public record MachineEntry(string ShortId, DateTime RegisteredAt);