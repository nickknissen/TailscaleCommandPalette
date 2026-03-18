using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace TailscaleCommandPalet;

public partial class TailscaleCommandPaletCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public TailscaleCommandPaletCommandsProvider()
    {
        DisplayName = "Tailscale";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _commands = [
            new CommandItem(new TailscaleCommandPaletPage()) { Title = DisplayName },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
