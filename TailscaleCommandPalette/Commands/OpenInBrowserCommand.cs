using System;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalette.Models;

namespace TailscaleCommandPalette.Commands;

internal sealed partial class OpenInBrowserCommand : InvokableCommand
{
    private readonly TailscaleDevice _device;

    public OpenInBrowserCommand(TailscaleDevice device)
    {
        _device = device;
    }

    public override string Name => $"Open {_device.HostName} in browser";

    public override CommandResult Invoke()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $"http://{_device.DNSName}",
                UseShellExecute = true,
            });

            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            var errorStartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c echo Error opening browser: {ex.Message} & pause",
                UseShellExecute = true,
            };

            Process.Start(errorStartInfo);

            return CommandResult.KeepOpen();
        }
    }
}
