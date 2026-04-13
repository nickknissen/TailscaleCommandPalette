using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalette.Services;

namespace TailscaleCommandPalette;

internal sealed partial class TailscaleCommandPalettePage : ListPage
{
    private readonly TailscaleDeviceService _deviceService;

    public TailscaleCommandPalettePage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "All devices";
        Name = "Open";
        _deviceService = new TailscaleDeviceService();
    }

    public override IListItem[] GetItems()
    {
        var devices = _deviceService.GetDevices();

        if (devices.Count == 0)
        {
            return [
                new ListItem(new NoOpCommand()) { Title = "No Tailscale devices found" }
            ];
        }

        return devices
            .Select(device =>
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
                var openInBrowserCommand = new OpenUrlCommand($"http://{device.DNSName}")
                {
                    Name = "Open in browser",
                };

                var title = device.HostName;
                var addressParts = new[] { device.TailscaleIPv4, device.TailscaleIPv6 }
                    .Where(x => !string.IsNullOrWhiteSpace(x));
                var subtitle = $"{string.Join(" • ", addressParts)} — {device.DNSName}";

                var moreCommands = new System.Collections.Generic.List<CommandContextItem>();
                if (!string.IsNullOrWhiteSpace(device.TailscaleIPv6))
                {
                    moreCommands.Add(new CommandContextItem(copyIPv6Command));
                }

                moreCommands.Add(new CommandContextItem(copyDnsCommand)
                {
                    RequestedShortcut = new KeyChord(Windows.System.VirtualKeyModifiers.Shift, 0, 0),
                });
                moreCommands.Add(new CommandContextItem(openInBrowserCommand)
                {
                    RequestedShortcut = new KeyChord(Windows.System.VirtualKeyModifiers.Control, 0, 0),
                });

                return new ListItem(command)
                {
                    Title = title,
                    Subtitle = subtitle,
                    Tags = GetTags(device),
                    Section = device.IsSelf ? "This Device" : device.Online ? "Online" : "Offline",
                    MoreCommands = moreCommands.ToArray(),
                };
            })
            .OrderBy(x => x.Section == "This Device" ? 0 : x.Section == "Online" ? 1 : 2)
            .ThenBy(x => x.Title)
            .ToArray();
    }

    private static ITag[] GetTags(Models.TailscaleDevice device)
    {
        var tags = new System.Collections.Generic.List<ITag>();

        tags.Add(new Tag(device.OS.ToUpperInvariant())
        {
            ToolTip = $"Operating System: {device.OS}"
        });

        if (device.Online)
        {
            tags.Add(new Tag("ONLINE")
            {
                Background = ColorHelpers.FromRgb(0, 128, 0),
                Foreground = ColorHelpers.FromRgb(255, 255, 255),
                ToolTip = "Device is online"
            });
        }
        else
        {
            tags.Add(new Tag("OFFLINE")
            {
                Background = ColorHelpers.FromRgb(128, 128, 128),
                Foreground = ColorHelpers.FromRgb(255, 255, 255),
                ToolTip = "Device is offline"
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
}
