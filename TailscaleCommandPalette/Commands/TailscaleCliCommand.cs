using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalette.Services;

namespace TailscaleCommandPalette.Commands;

internal sealed partial class TailscaleCliCommand : InvokableCommand
{
    private readonly TailscaleCliService _service;
    private readonly string _name;
    private readonly Func<TailscaleCliService, (bool Success, string Message)> _action;

    public TailscaleCliCommand(
        TailscaleCliService service,
        string name,
        Func<TailscaleCliService, (bool Success, string Message)> action)
    {
        _service = service;
        _name = name;
        _action = action;
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.scale-200.png");
    }

    public override string Name => _name;

    public override CommandResult Invoke()
    {
        try
        {
            var (success, message) = _action(_service);
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
