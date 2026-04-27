using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalette.Services;

namespace TailscaleCommandPalette.Commands;

internal sealed partial class ToggleConnectionCommand : InvokableCommand
{
    private readonly TailscaleCliService _service;

    public ToggleConnectionCommand(TailscaleCliService service)
    {
        _service = service;
        Name = "Toggle";
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.scale-200.png");
    }

    public override CommandResult Invoke()
    {
        try
        {
            var status = _service.GetStatus(requireConnected: false);
            var isConnected = status.Status?.IsConnected == true;
            var (success, message) = isConnected ? _service.Disconnect() : _service.Connect();
            return CommandResult.ShowToast(new ToastArgs
            {
                Message = success ? message : $"Tailscale command failed: {message}",
                Result = success ? CommandResult.Dismiss() : CommandResult.KeepOpen(),
            });
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast(new ToastArgs
            {
                Message = $"Unexpected error: {ex.Message}",
                Result = CommandResult.KeepOpen(),
            });
        }
    }
}
