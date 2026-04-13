using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalette.Commands;
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
                var command = new CopyTailscaleIpCommand(device);
                var copyDnsCommand = new CopyDnsNameCommand(device);
                var openInBrowserCommand = new OpenInBrowserCommand(device);

                var title = device.HostName;
                var subtitle = $"{device.TailscaleIP} — {device.DNSName}";

                return new ListItem(command)
                {
                    Title = title,
                    Subtitle = subtitle,
                    Tags = GetTags(device),
                    Section = device.IsSelf ? "This Device" : device.Online ? "Online" : "Offline",
                    MoreCommands = [
                        new CommandContextItem(copyDnsCommand)
                        {
                            RequestedShortcut = new KeyChord(Windows.System.VirtualKeyModifiers.Shift, 0, 0),
                        },
                        new CommandContextItem(openInBrowserCommand)
                        {
                            RequestedShortcut = new KeyChord(Windows.System.VirtualKeyModifiers.Control, 0, 0),
                        },
                    ],
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
