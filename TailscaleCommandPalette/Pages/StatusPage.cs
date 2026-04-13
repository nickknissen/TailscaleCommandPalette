using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalette.Services;

namespace TailscaleCommandPalette.Pages;

internal sealed partial class StatusPage : ListPage
{
    private readonly TailscaleCliService _service;

    public StatusPage(TailscaleCliService service)
    {
        _service = service;
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.scale-200.png");
        Title = "Status";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        var result = _service.GetStatus(requireConnected: false);
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
        var activeExitNode = status.Devices.FirstOrDefault(d => d.ExitNode && !d.IsSelf)?.HostName;
        var portInfo = string.Join(", ", new[]
        {
            !string.IsNullOrWhiteSpace(status.SelfIPv4) ? status.SelfIPv4 : null,
            !string.IsNullOrWhiteSpace(status.SelfIPv6) ? status.SelfIPv6 : null,
        }.Where(x => !string.IsNullOrWhiteSpace(x)));

        var items = new List<IListItem>
        {
            CreateInfoItem("Connection", status.IsConnected ? "Connected" : "Disconnected", status.TailnetName),
            CreateInfoItem("Hostname", status.SelfHostName, status.SelfDnsName),
            CreateInfoItem("Tailscale IPs", string.IsNullOrWhiteSpace(portInfo) ? "Unavailable" : portInfo),
            CreateInfoItem("Tailnet", string.IsNullOrWhiteSpace(status.TailnetName) ? "Unavailable" : status.TailnetName),
            CreateInfoItem("Active Exit Node", string.IsNullOrWhiteSpace(activeExitNode) ? "None" : activeExitNode),
            CreateInfoItem("Visible Devices", status.Devices.Count.ToString(CultureInfo.InvariantCulture)),
        };

        return items.ToArray();
    }

    private static ListItem CreateInfoItem(string title, string value, string? subtitle = null)
    {
        return new ListItem(new CopyTextCommand(value) { Name = $"Copy {title}" })
        {
            Title = title,
            Subtitle = value,
            MoreCommands = string.IsNullOrWhiteSpace(subtitle)
                ? []
                : [new CommandContextItem(new CopyTextCommand(subtitle) { Name = $"Copy {title} Detail" })],
        };
    }
}
