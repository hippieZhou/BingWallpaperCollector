using BingWallpaperCollector.Configuration;
using BingWallpaperCollector.Enums;
using BingWallpaperCollector.Models;

namespace BingWallpaperCollector.Services;

/// <summary>
/// 壁纸信息存储服务接口
/// </summary>
public interface IWallpaperStorageService
{
  /// <summary>
  /// 保存壁纸信息到JSON文件
  /// </summary>
  /// <param name="wallpaperInfo">壁纸信息</param>
  /// <param name="marketCode">市场代码</param>
  /// <param name="dateStr">日期字符串</param>
  /// <param name="config">收集配置</param>
  /// <param name="cancellationToken">取消令牌</param>
  Task SaveWallpaperInfoAsync(
      BingWallpaperInfo wallpaperInfo,
      MarketCode marketCode,
      string dateStr,
      CollectionConfig config,
      CancellationToken cancellationToken = default);

  /// <summary>
  /// 检查JSON文件是否已存在
  /// </summary>
  /// <param name="marketCode">市场代码</param>
  /// <param name="dateStr">日期字符串</param>
  /// <returns>如果文件存在返回true</returns>
  bool IsWallpaperInfoExists(MarketCode marketCode, string dateStr);

  /// <summary>
  /// 生成图片分辨率信息
  /// </summary>
  /// <param name="urlBase">URL基础路径</param>
  /// <param name="marketCode">市场代码</param>
  /// <returns>图片分辨率信息列表</returns>
  List<ImageResolution> GenerateImageResolutions(string urlBase, MarketCode marketCode);

  /// <summary>
  /// 从版权信息中提取描述
  /// </summary>
  /// <param name="copyright">版权信息</param>
  /// <returns>提取的描述</returns>
  string ExtractDescription(string copyright);
}
