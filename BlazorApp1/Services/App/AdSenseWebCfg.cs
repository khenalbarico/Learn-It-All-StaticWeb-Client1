namespace BlazorApp1.Services.App;

public class AdSenseWebCfg : IAdSenseCfg
{
    // PLACEHOLDER values — replace after AdSense account/site approval.
    // ClientId must also be updated in wwwroot/index.html's adsbygoogle.js script tag.
    public string ClientId { get; set; } = "ca-pub-0000000000000000";
    public string TopBannerSlotId { get; set; } = "0000000000";
    public string ReaderSlotId { get; set; } = "0000000000";
}
