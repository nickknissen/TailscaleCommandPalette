using System;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalet.Models;

namespace TailscaleCommandPalet.Commands;

internal sealed partial class CopyTailscaleIpCommand : InvokableCommand
{
    private readonly TailscaleDevice _device;

    public CopyTailscaleIpCommand(TailscaleDevice device)
    {
        _device = device;
    }

    public override string Name => $"Copy IP for {_device.HostName}";

    public override CommandResult Invoke()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -Command \"Set-Clipboard -Value '{_device.TailscaleIP}'\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo);

            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            var errorStartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c echo Error copying Tailscale IP: {ex.Message} & pause",
                UseShellExecute = true
            };

            Process.Start(errorStartInfo);

            return CommandResult.KeepOpen();
        }
    }
}
