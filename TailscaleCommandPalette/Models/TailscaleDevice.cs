namespace TailscaleCommandPalette.Models;

public class TailscaleDevice
{
    public string HostName { get; set; } = string.Empty;
    public string DNSName { get; set; } = string.Empty;
    public string OS { get; set; } = string.Empty;
    public string TailscaleIP { get; set; } = string.Empty;
    public bool Online { get; set; }
    public bool ExitNode { get; set; }
    public bool IsSelf { get; set; }
    public string UserDisplayName { get; set; } = string.Empty;
    public string LastSeen { get; set; } = string.Empty;
}
