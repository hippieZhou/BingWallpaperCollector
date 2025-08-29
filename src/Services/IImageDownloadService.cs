using BingWallpaperCollector.Models;

namespace BingWallpaperCollector.Services;

/// <summary>
/// 图片下载服务接口
/// </summary>
public interface IImageDownloadService
{
  /// <summary>
  /// 下载单张图片
  /// </summary>
  /// <param name="imageUrl">图片URL</param>
  /// <param name="country">国家名称</param>
  /// <param name="date">日期</param>
  /// <param name="resolution">分辨率</param>
  /// <param name="progress">进度报告</param>
  /// <param name="cancellationToken">取消令牌</param>
  /// <returns>下载的图片文件路径，失败则返回null</returns>
  Task<string?> DownloadImageAsync(
      string imageUrl,
      string country,
      string date,
      string resolution,
      IProgress<FileDownloadProgress>? progress = null,
      CancellationToken cancellationToken = default);

  /// <summary>
  /// 批量下载图片
  /// </summary>
  /// <param name="imageRequests">图片下载请求列表</param>
  /// <param name="batchProgress">批量下载进度报告</param>
  /// <param name="cancellationToken">取消令牌</param>
  /// <returns>成功下载的图片文件路径列表</returns>
  Task<List<string>> DownloadImagesAsync(
      List<ImageDownloadRequest> imageRequests,
      IProgress<BatchDownloadProgress>? batchProgress = null,
      CancellationToken cancellationToken = default);
}
