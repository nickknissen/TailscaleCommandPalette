using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TailscaleCommandPalette.Models;

namespace TailscaleCommandPalette.Services;

public sealed class TailscaleCliService
{
    private readonly object _cacheLock = new();
    private TailscaleQueryResult? _cachedResult;
    private DateTime _cacheTime = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(15);

    public TailscaleQueryResult GetStatus(bool requireConnected = true)
    {
        lock (_cacheLock)
        {
            if (_cachedResult is not null && DateTime.UtcNow - _cacheTime < CacheDuration)
            {
                if (!requireConnected && _cachedResult.ErrorKind == TailscaleErrorKind.NotConnected && _cachedResult.Status is not null)
                {
                    return new TailscaleQueryResult
                    {
                        Status = _cachedResult.Status,
                        ErrorKind = TailscaleErrorKind.None,
                    };
                }

                return _cachedResult;
            }
        }

        var result = LoadStatus(requireConnected);

        lock (_cacheLock)
        {
            _cachedResult = result;
            _cacheTime = DateTime.UtcNow;
            return result;
        }
    }

    public IReadOnlyList<TailscaleDevice> GetDevices(bool onlyCurrentUser = false)
    {
        var result = GetStatus();
        if (result.Status is null)
        {
            return Array.Empty<TailscaleDevice>();
        }

        var devices = result.Status.Devices;
        if (!onlyCurrentUser)
        {
            return devices;
        }

        return devices.Where(d => d.UserId == result.Status.SelfUserId).ToArray();
    }

    public (bool Success, string Message) Connect()
    {
        var result = Execute("up");
        InvalidateCache();
        return result.ExitCode == 0
            ? (true, "Connected to Tailscale")
            : (false, string.IsNullOrWhiteSpace(result.Error) ? "Unable to connect" : result.Error);
    }

    public (bool Success, string Message) Disconnect()
    {
        var result = Execute("down");
        InvalidateCache();
        return result.ExitCode == 0
            ? (true, "Disconnected from Tailscale")
            : (false, string.IsNullOrWhiteSpace(result.Error) ? "Unable to disconnect" : result.Error);
    }

    public (bool Success, string Message) ToggleConnection()
    {
        var status = GetStatus();
        if (status.Status?.IsConnected == true)
        {
            return Disconnect();
        }

        return Connect();
    }

    public void InvalidateCache()
    {
        lock (_cacheLock)
        {
            _cachedResult = null;
            _cacheTime = DateTime.MinValue;
        }
    }

    private TailscaleQueryResult LoadStatus(bool requireConnected)
    {
        try
        {
            var result = Execute("status --json");
            if (result.ExitCode != 0)
            {
                return CreateErrorFromProcess(result.Error);
            }

            if (string.IsNullOrWhiteSpace(result.Output))
            {
                return new TailscaleQueryResult
                {
                    ErrorKind = TailscaleErrorKind.CommandFailed,
                    ErrorTitle = "Tailscale returned no data",
                    ErrorDescription = "The Tailscale CLI returned an empty response.",
                };
            }

            var status = ParseStatus(result.Output);
            if (!status.IsConnected)
            {
                return new TailscaleQueryResult
                {
                    Status = status,
                    ErrorKind = requireConnected ? TailscaleErrorKind.NotConnected : TailscaleErrorKind.None,
                    ErrorTitle = requireConnected ? "Not connected to a tailnet" : string.Empty,
                    ErrorDescription = requireConnected ? "Tailscale is running, but this device is not currently connected." : string.Empty,
                };
            }

            return new TailscaleQueryResult
            {
                Status = status,
                ErrorKind = TailscaleErrorKind.None,
            };
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return new TailscaleQueryResult
            {
                ErrorKind = TailscaleErrorKind.CliNotFound,
                ErrorTitle = "Can’t find the Tailscale CLI",
                ErrorDescription = "Make sure the 'tailscale' command is installed and available on PATH.",
            };
        }
        catch (JsonException ex)
        {
            return new TailscaleQueryResult
            {
                ErrorKind = TailscaleErrorKind.CommandFailed,
                ErrorTitle = "Couldn’t parse Tailscale status",
                ErrorDescription = ex.Message,
            };
        }
        catch (Exception ex)
        {
            return new TailscaleQueryResult
            {
                ErrorKind = TailscaleErrorKind.Unknown,
                ErrorTitle = "Couldn’t load Tailscale status",
                ErrorDescription = ex.Message,
            };
        }
    }

    private static TailscaleQueryResult CreateErrorFromProcess(string error)
    {
        if (error.Contains("is Tailscale running?", StringComparison.OrdinalIgnoreCase))
        {
            return new TailscaleQueryResult
            {
                ErrorKind = TailscaleErrorKind.NotRunning,
                ErrorTitle = "Can’t connect to Tailscale",
                ErrorDescription = "Make sure the Tailscale service/app is running and try again.",
            };
        }

        return new TailscaleQueryResult
        {
            ErrorKind = TailscaleErrorKind.CommandFailed,
            ErrorTitle = "Tailscale command failed",
            ErrorDescription = string.IsNullOrWhiteSpace(error) ? "The CLI returned a non-zero exit code." : error,
        };
    }

    private static TailscaleStatus ParseStatus(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var users = new Dictionary<long, (string DisplayName, string LoginName)>();
        if (root.TryGetProperty("User", out var userElement))
        {
            foreach (var userEntry in userElement.EnumerateObject())
            {
                if (!long.TryParse(userEntry.Name, out var userId))
                {
                    continue;
                }

                var displayName = userEntry.Value.TryGetProperty("DisplayName", out var dn)
                    ? dn.GetString() ?? string.Empty
                    : string.Empty;
                var loginName = userEntry.Value.TryGetProperty("LoginName", out var ln)
                    ? ln.GetString() ?? string.Empty
                    : string.Empty;
                users[userId] = (displayName, loginName);
            }
        }

        var status = new TailscaleStatus();
        var devices = new List<TailscaleDevice>();

        if (root.TryGetProperty("CurrentTailnet", out var currentTailnet) &&
            currentTailnet.TryGetProperty("Name", out var tailnetName))
        {
            status.TailnetName = tailnetName.GetString() ?? string.Empty;
        }

        if (root.TryGetProperty("Self", out var self))
        {
            var selfDevice = ParsePeer(self, users);
            selfDevice.IsSelf = true;
            devices.Add(selfDevice);

            status.SelfHostName = selfDevice.HostName;
            status.SelfDnsName = selfDevice.DNSName;
            status.SelfIPv4 = selfDevice.TailscaleIPv4;
            status.SelfIPv6 = selfDevice.TailscaleIPv6;
            status.SelfUserId = selfDevice.UserId;
            status.IsConnected = selfDevice.Online;
        }

        if (root.TryGetProperty("Peer", out var peers))
        {
            foreach (var peer in peers.EnumerateObject())
            {
                devices.Add(ParsePeer(peer.Value, users));
            }
        }

        status.Devices = devices
            .OrderByDescending(d => d.IsSelf)
            .ThenByDescending(d => d.Online)
            .ThenBy(d => d.HostName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return status;
    }

    private static TailscaleDevice ParsePeer(JsonElement peer, Dictionary<long, (string DisplayName, string LoginName)> users)
    {
        var device = new TailscaleDevice();

        if (peer.TryGetProperty("ID", out var id))
            device.Id = id.GetString() ?? string.Empty;

        if (peer.TryGetProperty("HostName", out var hostName))
            device.HostName = hostName.GetString() ?? string.Empty;

        if (peer.TryGetProperty("DNSName", out var dnsName))
            device.DNSName = (dnsName.GetString() ?? string.Empty).TrimEnd('.');

        if (peer.TryGetProperty("OS", out var os))
            device.OS = os.GetString() ?? string.Empty;

        if (peer.TryGetProperty("Online", out var online))
            device.Online = online.GetBoolean();

        if (peer.TryGetProperty("ExitNode", out var exitNode))
            device.ExitNode = exitNode.GetBoolean();

        if (peer.TryGetProperty("ExitNodeOption", out var exitNodeOption))
            device.ExitNodeOption = exitNodeOption.GetBoolean();

        if (peer.TryGetProperty("LastSeen", out var lastSeen))
            device.LastSeen = lastSeen.GetString() ?? string.Empty;

        if (peer.TryGetProperty("UserID", out var userId))
        {
            device.UserId = userId.GetInt64();
            if (users.TryGetValue(device.UserId, out var user))
            {
                device.UserDisplayName = user.DisplayName;
                device.UserLoginName = user.LoginName;
            }
        }

        if (peer.TryGetProperty("sshHostKeys", out var sshHostKeys) && sshHostKeys.ValueKind == JsonValueKind.Array)
            device.Ssh = sshHostKeys.GetArrayLength() > 0;

        if (peer.TryGetProperty("TailscaleIPs", out var ips))
        {
            foreach (var ipElement in ips.EnumerateArray())
            {
                var ipString = ipElement.GetString();
                if (string.IsNullOrWhiteSpace(ipString) || !IPAddress.TryParse(ipString, out var ipAddress))
                {
                    continue;
                }

                if (ipAddress.AddressFamily == AddressFamily.InterNetwork && string.IsNullOrEmpty(device.TailscaleIPv4))
                {
                    device.TailscaleIPv4 = ipString;
                }
                else if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6 && string.IsNullOrEmpty(device.TailscaleIPv6))
                {
                    device.TailscaleIPv6 = ipString;
                }
            }
        }

        return device;
    }

    private static (int ExitCode, string Output, string Error) Execute(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "tailscale",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return (-1, string.Empty, "Failed to start tailscale process.");
        }

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output, error);
    }
}
