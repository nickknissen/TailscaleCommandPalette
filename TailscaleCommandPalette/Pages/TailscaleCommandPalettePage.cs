using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalette.Models;
using TailscaleCommandPalette.Services;

namespace TailscaleCommandPalette.Pages;

internal partial class TailscaleDevicesPage : ListPage
{
    private readonly TailscaleCliService _service;
    private readonly bool _onlyCurrentUser;

    public TailscaleDevicesPage(TailscaleCliService service, bool onlyCurrentUser)
    {
        _service = service;
        _onlyCurrentUser = onlyCurrentUser;

        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = onlyCurrentUser ? "My devices" : "All devices";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        var result = _service.GetStatus();
        if (result.HasError)
        {
            return [
                new ListItem(new NoOpCommand())
                {
                    Title = result.ErrorTitle,
                    Subtitle = result.ErrorDescription,
                }
            ];
        }

        var status = result.Status!;
        var devices = _onlyCurrentUser
            ? status.Devices.Where(d => d.UserId == status.SelfUserId).ToArray()
            : status.Devices;

        if (devices.Count == 0)
        {
            return [
                new ListItem(new NoOpCommand())
                {
                    Title = _onlyCurrentUser ? "No devices found for the active account" : "No Tailscale devices found"
                }
            ];
        }

        return devices
            .Select(CreateItem)
            .OrderBy(x => x.Section == "This Device" ? 0 : x.Section == "Online" ? 1 : 2)
            .ThenBy(x => x.Title)
            .ToArray();
    }

    private static ListItem CreateItem(TailscaleDevice device)
    {
        var defaultCopyValue = !string.IsNullOrWhiteSpace(device.TailscaleIPv4)
            ? device.TailscaleIPv4
            : device.TailscaleIPv6;

        var command = new CopyTextCommand(defaultCopyValue)
        {
            Name = !string.IsNullOrWhiteSpace(device.TailscaleIPv4) ? "Copy IPv4" : "Copy IPv6",
        };
        var copyIPv6Command = new CopyTextCommand(device.TailscaleIPv6)
        {
            Name = "Copy IPv6",
        };
        var copyDnsCommand = new CopyTextCommand(device.DNSName)
        {
            Name = "Copy MagicDNS",
        };

        var title = string.IsNullOrWhiteSpace(device.HostName) ? device.DNSName : device.HostName;
        var subtitleParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(device.TailscaleIPv4)) subtitleParts.Add(device.TailscaleIPv4);
        if (!string.IsNullOrWhiteSpace(device.OS)) subtitleParts.Add(device.OS);
        if (!string.IsNullOrWhiteSpace(device.UserLoginName)) subtitleParts.Add(device.UserLoginName);
        var subtitle = string.Join(" • ", subtitleParts);
        if (!string.IsNullOrWhiteSpace(device.DNSName))
        {
            subtitle = string.IsNullOrWhiteSpace(subtitle) ? device.DNSName : $"{subtitle} — {device.DNSName}";
        }

        var moreCommands = new List<CommandContextItem>();
        if (!string.IsNullOrWhiteSpace(device.TailscaleIPv6))
        {
            moreCommands.Add(new CommandContextItem(copyIPv6Command));
        }

        moreCommands.Add(new CommandContextItem(copyDnsCommand)
        {
            RequestedShortcut = new KeyChord(Windows.System.VirtualKeyModifiers.Shift, 0, 0),
        });

        if (!string.IsNullOrWhiteSpace(device.DNSName))
        {
            moreCommands.Add(new CommandContextItem(new OpenUrlCommand($"http://{device.DNSName}")
            {
                Name = "Open in browser",
            })
            {
                RequestedShortcut = new KeyChord(Windows.System.VirtualKeyModifiers.Control, 0, 0),
            });
        }

        return new ListItem(command)
        {
            Title = title,
            Subtitle = subtitle,
            Tags = GetTags(device),
            Section = device.IsSelf ? "This Device" : device.Online ? "Online" : "Offline",
            MoreCommands = moreCommands.ToArray(),
        };
    }

    private static ITag[] GetTags(TailscaleDevice device)
    {
        var tags = new List<ITag>();

        if (!string.IsNullOrWhiteSpace(device.OS))
        {
            tags.Add(new Tag(device.OS.ToUpperInvariant())
            {
                ToolTip = $"Operating System: {device.OS}"
            });
        }

        tags.Add(new Tag(device.Online ? "ONLINE" : "OFFLINE")
        {
            Background = device.Online ? ColorHelpers.FromRgb(0, 128, 0) : ColorHelpers.FromRgb(128, 128, 128),
            Foreground = ColorHelpers.FromRgb(255, 255, 255),
            ToolTip = device.Online ? "Device is online" : FormatOfflineTooltip(device),
        });

        if (device.Ssh)
        {
            tags.Add(new Tag("SSH")
            {
                Background = ColorHelpers.FromRgb(34, 139, 34),
                Foreground = ColorHelpers.FromRgb(255, 255, 255),
                ToolTip = "Tailscale SSH is available"
            });
        }

        if (device.ExitNode)
        {
            tags.Add(new Tag("EXIT NODE")
            {
                Background = ColorHelpers.FromRgb(0, 102, 204),
                Foreground = ColorHelpers.FromRgb(255, 255, 255),
                ToolTip = "Active exit node"
            });
        }

        if (device.IsSelf)
        {
            tags.Add(new Tag("SELF")
            {
                Background = ColorHelpers.FromRgb(102, 0, 204),
                Foreground = ColorHelpers.FromRgb(255, 255, 255),
                ToolTip = "This device"
            });
        }

        return tags.ToArray();
    }

    private static string FormatOfflineTooltip(TailscaleDevice device)
    {
        if (string.IsNullOrWhiteSpace(device.LastSeen))
        {
            return "Device is offline";
        }

        if (DateTimeOffset.TryParse(device.LastSeen, out var lastSeen))
        {
            return $"Last seen {lastSeen.LocalDateTime:g}";
        }

        return $"Last seen {device.LastSeen}";
    }
}

internal sealed partial class TailscaleCommandPalettePage : TailscaleDevicesPage
{
    public TailscaleCommandPalettePage(TailscaleCliService service)
        : base(service, false)
    {
    }
}
