using TailscaleCommandPalette.Services;

namespace TailscaleCommandPalette.Pages;

internal sealed partial class MyDevicesPage : TailscaleDevicesPage
{
    public MyDevicesPage(TailscaleCliService service)
        : base(service, true)
    {
    }
}
