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

        _commands = [
            new CommandItem(new TailscaleCommandPalettePage(service)) { Title = "All Devices", Subtitle = "Tailscale", Icon = commandIcon },
            new CommandItem(new MyDevicesPage(service)) { Title = "My Devices", Subtitle = "Tailscale", Icon = commandIcon },
            new CommandItem(new ServicesPage(service)) { Title = "Services", Subtitle = "Tailscale", Icon = commandIcon },
            new CommandItem(new StatusPage(service)) { Title = "Status", Subtitle = "Tailscale", Icon = commandIcon },
            new CommandItem(new ToggleConnectionCommand(service))
            {
                Title = "Toggle Connection",
                Subtitle = "Tailscale",
                Icon = commandIcon,
            },
            new CommandItem(adminCommand) { Title = "Admin Console", Subtitle = "Tailscale", Icon = commandIcon },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
