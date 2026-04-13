using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using TailscaleCommandPalette.Models;

namespace TailscaleCommandPalette.Services;

public class TailscaleDeviceService
{
    private readonly object _cacheLock = new();
    private IReadOnlyList<TailscaleDevice> _cache = Array.Empty<TailscaleDevice>();
    private DateTime _cacheTime = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public TailscaleDeviceService()
    {
        Task.Run(() =>
        {
            try
            {
                _ = GetDevices();
            }
            catch
            {
            }
        });
    }

    public IReadOnlyList<TailscaleDevice> GetDevices()
    {
        lock (_cacheLock)
        {
            if (_cache.Count > 0 && DateTime.UtcNow - _cacheTime < CacheDuration)
            {
                return _cache;
            }
        }

        var devices = LoadDevices();

        lock (_cacheLock)
        {
            _cache = devices;
            _cacheTime = DateTime.UtcNow;
            return _cache;
        }
    }

    private static IReadOnlyList<TailscaleDevice> LoadDevices()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "tailscale",
                Arguments = "status --json",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Array.Empty<TailscaleDevice>();
            }

            var json = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(json))
            {
                return Array.Empty<TailscaleDevice>();
            }

            return ParseStatus(json);
        }
        catch
        {
            return Array.Empty<TailscaleDevice>();
        }
    }

    private static List<TailscaleDevice> ParseStatus(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var users = new Dictionary<long, string>();
        if (root.TryGetProperty("User", out var userElement))
        {
            foreach (var userEntry in userElement.EnumerateObject())
            {
                if (long.TryParse(userEntry.Name, out var userId))
                {
                    var displayName = userEntry.Value.TryGetProperty("DisplayName", out var dn)
                        ? dn.GetString() ?? string.Empty
                        : string.Empty;
                    users[userId] = displayName;
                }
            }
        }

        var devices = new List<TailscaleDevice>();

        if (root.TryGetProperty("Self", out var self))
        {
            var device = ParsePeer(self, users);
            device.IsSelf = true;
            devices.Add(device);
        }

        if (root.TryGetProperty("Peer", out var peers))
        {
            foreach (var peer in peers.EnumerateObject())
            {
                devices.Add(ParsePeer(peer.Value, users));
            }
        }

        return devices;
    }

    private static TailscaleDevice ParsePeer(JsonElement peer, Dictionary<long, string> users)
    {
        var device = new TailscaleDevice();

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

        if (peer.TryGetProperty("LastSeen", out var lastSeen))
            device.LastSeen = lastSeen.GetString() ?? string.Empty;

        if (peer.TryGetProperty("TailscaleIPs", out var ips) && ips.GetArrayLength() > 0)
            device.TailscaleIP = ips[0].GetString() ?? string.Empty;

        if (peer.TryGetProperty("UserID", out var userId) && users.TryGetValue(userId.GetInt64(), out var userName))
            device.UserDisplayName = userName;

        return device;
    }
}
