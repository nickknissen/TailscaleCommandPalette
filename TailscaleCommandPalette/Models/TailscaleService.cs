using System;
using System.Collections.Generic;
using System.Linq;

namespace TailscaleCommandPalette.Models;

public class TailscaleService
{
    public string Name { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string TailscaleIPv4 { get; set; } = string.Empty;
    public string TailscaleIPv6 { get; set; } = string.Empty;
    public IReadOnlyList<string> Ports { get; set; } = Array.Empty<string>();

    public string DisplayName => Name.StartsWith("svc:", StringComparison.OrdinalIgnoreCase) ? Name[4..] : Name;

    public string BrowserUrl
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Hostname))
            {
                return string.Empty;
            }

            var https = Ports.Any(p => string.Equals(p, "tcp:443", StringComparison.OrdinalIgnoreCase));
            return https ? $"https://{Hostname}" : $"http://{Hostname}";
        }
    }
}
