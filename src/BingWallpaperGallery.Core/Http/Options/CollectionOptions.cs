using BingWallpaperGallery.Core.Http.Configuration;
using BingWallpaperGallery.Core.Http.Enums;

namespace BingWallpaperGallery.Core.Http.Options;
public class CollectionOptions
{
    public MarketCode MarketCode { get; set; } = MarketCode.China;
    public ResolutionCode ResolutionCode { get; set; } = ResolutionCode.FullHD;
    public bool CollectAllCountries { get; set; } = true;
    public int CollectDays { get; set; } = HTTPConstants.MaxHistoryDays;
    public bool PrettyJsonFormat { get; set; } = true;
    public int MaxConcurrentRequests { get; set; } = HTTPConstants.DefaultConcurrentRequests;
    public int MaxConcurrentDownloads { get; set; } = HTTPConstants.DefaultConcurrentDownloads;

    public static void Validate(CollectionOptions options)
    {
        // 设置收集天数
        if (options.CollectDays is < 1 or > HTTPConstants.MaxHistoryDays)
        {
            options.CollectDays = HTTPConstants.MaxHistoryDays;
        }

        // 设置并发请求数
        if (options.MaxConcurrentRequests is < 1 or > HTTPConstants.MaxConcurrentRequests)
        {
            options.MaxConcurrentRequests = HTTPConstants.DefaultConcurrentRequests;
        }

        // 设置并发下载数
        if (options.MaxConcurrentDownloads is < 1 or > HTTPConstants.MaxConcurrentDownloads)
        {
            options.MaxConcurrentDownloads = HTTPConstants.DefaultConcurrentDownloads;
        }
    }
}
