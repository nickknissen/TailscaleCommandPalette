// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalette.Commands;
using TailscaleCommandPalette.Pages;
using TailscaleCommandPalette.Services;

namespace TailscaleCommandPalette;

public partial class TailscaleCommandPaletteCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public TailscaleCommandPaletteCommandsProvider()
    {
        DisplayName = "Tailscale";
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.scale-200.png");

        var service = new TailscaleCliService();
        var commandIcon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.scale-200.png");
        var adminCommand = new OpenUrlCommand("https://login.tailscale.com/admin/machines")
        {
            Name = "Admin Console",
            Icon = commandIcon,
        };

        var connectionStatus = service.GetStatus(requireConnected: false);
        var isConnected = connectionStatus.Status?.IsConnected == true;
        var connectionSubtitle = connectionStatus.HasError
            ? connectionStatus.ErrorTitle
            : isConnected
                ? string.IsNullOrWhiteSpace(connectionStatus.Status?.TailnetName)
                    ? "Connected"
                    : $"Connected on {connectionStatus.Status.TailnetName}"
                : "Disconnected";
        var connectionCommand = isConnected
            ? new TailscaleCliCommand(service, "Down", s => s.Disconnect())
            : new TailscaleCliCommand(service, "Up", s => s.Connect());

        _commands = [
            new CommandItem(new TailscaleCommandPalettePage(service)) { Title = "All Devices", Icon = commandIcon },
            new CommandItem(new MyDevicesPage(service)) { Title = "My Devices", Icon = commandIcon },
            new CommandItem(new StatusPage(service)) { Title = "Status", Icon = commandIcon },
            new CommandItem(connectionCommand)
            {
                Title = "Connection",
                Subtitle = connectionSubtitle,
                Icon = commandIcon,
            },
            new CommandItem(adminCommand) { Title = "Admin Console", Icon = commandIcon },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
