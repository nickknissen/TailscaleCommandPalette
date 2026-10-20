using TailscaleCommandPalette.Models;
using TailscaleCommandPalette.Services;
using Xunit;

namespace TailscaleCommandPalette.Tests;

public class TailscaleServiceTests
{
    [Fact]
    public void ParseServices_ReadsServiceListJson()
    {
        const string json = """
            [
              {
                "Name": "svc:web",
                "Addrs": ["100.64.0.1", "fd7a:115c:a1e0::1"],
                "Ports": ["tcp:443"],
                "Hostname": "web.example.ts.net"
              },
              {
                "Name": "svc:grafana",
                "Addrs": ["fd7a:115c:a1e0::2", "100.64.0.2"],
                "Ports": ["tcp:80", "tcp:8080"],
                "Hostname": "grafana.example.ts.net."
              }
            ]
            """;

        var services = TailscaleCliService.ParseServices(json);

        Assert.Equal(2, services.Count);

        var grafana = services[0];
        Assert.Equal("grafana", grafana.DisplayName);
        Assert.Equal("100.64.0.2", grafana.TailscaleIPv4);
        Assert.Equal("fd7a:115c:a1e0::2", grafana.TailscaleIPv6);
        Assert.Equal("grafana.example.ts.net", grafana.Hostname);
        Assert.Equal(["tcp:80", "tcp:8080"], grafana.Ports);

        var web = services[1];
        Assert.Equal("svc:web", web.Name);
        Assert.Equal("web", web.DisplayName);
        Assert.Equal("100.64.0.1", web.TailscaleIPv4);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("[]")]
    public void ParseServices_ReturnsEmptyForNoServices(string json)
    {
        Assert.Empty(TailscaleCliService.ParseServices(json));
    }

    [Fact]
    public void BrowserUrl_UsesHttpsWhenServiceListensOn443()
    {
        var service = new TailscaleService { Hostname = "web.example.ts.net", Ports = ["tcp:443"] };

        Assert.Equal("https://web.example.ts.net", service.BrowserUrl);
    }

    [Fact]
    public void BrowserUrl_UsesHttpOtherwise()
    {
        var service = new TailscaleService { Hostname = "web.example.ts.net", Ports = ["tcp:8080"] };

        Assert.Equal("http://web.example.ts.net", service.BrowserUrl);
    }

    [Fact]
    public void BrowserUrl_IsEmptyWithoutHostname()
    {
        Assert.Equal(string.Empty, new TailscaleService { Ports = ["tcp:443"] }.BrowserUrl);
    }

    [Fact]
    public void DisplayName_KeepsNameWithoutSvcPrefix()
    {
        Assert.Equal("plain", new TailscaleService { Name = "plain" }.DisplayName);
    }
}
