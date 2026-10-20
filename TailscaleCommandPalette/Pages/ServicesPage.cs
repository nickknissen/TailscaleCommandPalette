using System.Collections.Generic;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalette.Models;
using TailscaleCommandPalette.Services;

namespace TailscaleCommandPalette.Pages;

internal sealed partial class ServicesPage : ListPage
{
    private readonly TailscaleCliService _service;

    public ServicesPage(TailscaleCliService service)
    {
        _service = service;
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.scale-200.png");
        Title = "Services";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        var status = _service.GetStatus();
        if (status.HasError)
        {
            return [
                new ListItem(new NoOpCommand())
                {
                    Title = status.ErrorTitle,
                    Subtitle = status.ErrorDescription,
                }
            ];
        }

        var (services, error) = _service.GetServices();
        if (!string.IsNullOrWhiteSpace(error))
        {
            return [
                new ListItem(new NoOpCommand())
                {
                    Title = "Couldn’t list Tailscale services",
                    Subtitle = error,
                }
            ];
        }

        if (services.Count == 0)
        {
            return [
                new ListItem(new NoOpCommand())
                {
                    Title = "No Tailscale services found"
                }
            ];
        }

        return services.Select(CreateItem).ToArray();
    }

    private static ListItem CreateItem(TailscaleService service)
    {
        var defaultCopyValue = !string.IsNullOrWhiteSpace(service.TailscaleIPv4)
            ? service.TailscaleIPv4
            : service.TailscaleIPv6;

        var command = new CopyTextCommand(defaultCopyValue)
        {
            Name = !string.IsNullOrWhiteSpace(service.TailscaleIPv4) ? "Copy IPv4" : "Copy IPv6",
        };

        var subtitleParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(service.TailscaleIPv4)) subtitleParts.Add(service.TailscaleIPv4);
        if (service.Ports.Count > 0) subtitleParts.Add(string.Join(", ", service.Ports));
        var subtitle = string.Join(" • ", subtitleParts);
        if (!string.IsNullOrWhiteSpace(service.Hostname))
        {
            subtitle = string.IsNullOrWhiteSpace(subtitle) ? service.Hostname : $"{subtitle} — {service.Hostname}";
        }

        var moreCommands = new List<CommandContextItem>();
        if (!string.IsNullOrWhiteSpace(service.TailscaleIPv6))
        {
            moreCommands.Add(new CommandContextItem(new CopyTextCommand(service.TailscaleIPv6)
            {
                Name = "Copy IPv6",
            }));
        }

        if (!string.IsNullOrWhiteSpace(service.Hostname))
        {
            moreCommands.Add(new CommandContextItem(new CopyTextCommand(service.Hostname)
            {
                Name = "Copy MagicDNS",
            })
            {
                RequestedShortcut = new KeyChord(Windows.System.VirtualKeyModifiers.Shift, 0, 0),
            });

            moreCommands.Add(new CommandContextItem(new OpenUrlCommand(service.BrowserUrl)
            {
                Name = "Open in browser",
            })
            {
                RequestedShortcut = new KeyChord(Windows.System.VirtualKeyModifiers.Control, 0, 0),
            });
        }

        return new ListItem(command)
        {
            Title = service.DisplayName,
            Subtitle = subtitle,
            Tags = service.Ports
                .Select(p => (ITag)new Tag(p.ToUpperInvariant()) { ToolTip = $"Endpoint: {p}" })
                .ToArray(),
            MoreCommands = moreCommands.ToArray(),
        };
    }
}
