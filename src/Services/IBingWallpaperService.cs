using BingWallpaperCollector.Configuration;
using BingWallpaperCollector.Enums;
using BingWallpaperCollector.Models;

namespace BingWallpaperCollector.Services;

/// <summary>
/// 必应壁纸信息收集服务接口
/// </summary>
public interface IBingWallpaperService
{
  /// <summary>
  /// 运行壁纸收集应用
  /// </summary>
  /// <param name="cancellationToken">取消令牌</param>
  Task RunAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// 为指定国家收集壁纸信息
  /// </summary>
  /// <param name="marketCode">市场代码</param>
  /// <param name="daysToCollect">收集天数</param>
  /// <param name="config">收集配置</param>
  /// <param name="cancellationToken">取消令牌</param>
  Task CollectForCountryAsync(MarketCode marketCode, int daysToCollect, CollectionConfig config, CancellationToken cancellationToken = default);

  /// <summary>
  /// 获取指定日期和市场的壁纸信息
  /// </summary>
  /// <param name="marketCode">市场代码</param>
  /// <param name="dayIndex">天数索引 (0=今天, 1=昨天)</param>
  /// <param name="config">收集配置</param>
  /// <param name="cancellationToken">取消令牌</param>
  /// <returns>壁纸信息，如果获取失败则返回null</returns>
  Task<BingWallpaperInfo?> GetWallpaperInfoAsync(MarketCode marketCode, int dayIndex, CollectionConfig config, CancellationToken cancellationToken = default);
}
