using BingWallpaperCollector.Enums;

namespace BingWallpaperCollector.Configuration;

/// <summary>
/// 收集配置类
/// </summary>
public sealed class CollectionConfig
{
  public MarketCode MarketCode { get; set; } = MarketCode.China;
  public int DaysToCollect { get; set; } = 1;
  public bool CollectAllCountries { get; set; } = false;
  public int MaxConcurrentRequests { get; set; } = 3;
  public bool PrettyJsonFormat { get; set; } = true;

  /// <summary>
  /// 默认请求分辨率
  /// </summary>
  public ApiResolution DefaultResolution { get; set; } = ApiResolution.UHD4K;
}
