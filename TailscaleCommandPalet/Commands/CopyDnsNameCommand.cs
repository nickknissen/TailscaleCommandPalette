using System;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalet.Models;

namespace TailscaleCommandPalet.Commands;

internal sealed partial class CopyDnsNameCommand : InvokableCommand
{
    private readonly TailscaleDevice _device;

    public CopyDnsNameCommand(TailscaleDevice device)
    {
        _device = device;
    }

    public override string Name => $"Copy DNS for {_device.HostName}";

    public override CommandResult Invoke()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -Command \"Set-Clipboard -Value '{_device.DNSName}'\"",
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
                Arguments = $"/c echo Error copying DNS name: {ex.Message} & pause",
                UseShellExecute = true
            };

            Process.Start(errorStartInfo);

            return CommandResult.KeepOpen();
        }
    }
}
