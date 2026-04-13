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
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        var service = new TailscaleCliService();
        var commandIcon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        _commands = [
            new CommandItem(new TailscaleCommandPalettePage(service)) { Title = "All Devices", Icon = commandIcon },
            new CommandItem(new MyDevicesPage(service)) { Title = "My Devices", Icon = commandIcon },
            new CommandItem(new TailscaleCliCommand(service, "Connect", s => s.Connect())) { Title = "Connect", Icon = commandIcon },
            new CommandItem(new TailscaleCliCommand(service, "Disconnect", s => s.Disconnect())) { Title = "Disconnect", Icon = commandIcon },
            new CommandItem(new TailscaleCliCommand(service, "Toggle Connection", s => s.ToggleConnection())) { Title = "Toggle Connection", Icon = commandIcon },
            new CommandItem(new OpenUrlCommand("https://login.tailscale.com/admin/machines") { Name = "Admin Console" }) { Title = "Admin Console", Icon = commandIcon },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
