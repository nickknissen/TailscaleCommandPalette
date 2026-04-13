using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalette.Helpers;
using TailscaleCommandPalette.Models;

namespace TailscaleCommandPalette.Commands;

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
            ProcessClipboardHelper.SetText(_device.TailscaleIP);
            return CommandResult.Dismiss();
        }
        catch (Exception)
        {
            return CommandResult.KeepOpen();
        }
    }
}
