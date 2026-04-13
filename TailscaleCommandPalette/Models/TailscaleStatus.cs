using System;
using System.Collections.Generic;

namespace TailscaleCommandPalette.Models;

public enum TailscaleErrorKind
{
    None,
    CliNotFound,
    NotRunning,
    NotConnected,
    CommandFailed,
    Unknown,
}

public sealed class TailscaleQueryResult
{
    public TailscaleStatus? Status { get; init; }
    public TailscaleErrorKind ErrorKind { get; init; }
    public string ErrorTitle { get; init; } = string.Empty;
    public string ErrorDescription { get; init; } = string.Empty;

    public bool HasError => ErrorKind != TailscaleErrorKind.None;
}

public sealed class TailscaleStatus
{
    public bool IsConnected { get; set; }
    public string TailnetName { get; set; } = string.Empty;
    public string SelfHostName { get; set; } = string.Empty;
    public string SelfDnsName { get; set; } = string.Empty;
    public string SelfIPv4 { get; set; } = string.Empty;
    public string SelfIPv6 { get; set; } = string.Empty;
    public long SelfUserId { get; set; }
    public IReadOnlyList<TailscaleDevice> Devices { get; set; } = Array.Empty<TailscaleDevice>();
}
