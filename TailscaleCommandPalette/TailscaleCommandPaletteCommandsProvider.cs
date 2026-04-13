// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace TailscaleCommandPalette;

public partial class TailscaleCommandPaletteCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public TailscaleCommandPaletteCommandsProvider()
    {
        DisplayName = "Tailscale Command Palette";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _commands = [
            new CommandItem(new TailscaleCommandPalettePage()) { Title = DisplayName },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}
