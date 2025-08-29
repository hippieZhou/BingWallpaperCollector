using BingWallpaperCollector.Services;
using Microsoft.Extensions.Logging;

namespace BingWallpaperCollector;

/// <summary>
/// 必应壁纸信息收集器主应用类
/// </summary>
public sealed class BingWallpaperApp : IDisposable
{
  private readonly IBingWallpaperService _wallpaperService;
  private readonly IImageDownloadService _downloadService;
  private readonly ILogger<BingWallpaperApp> _logger;
  private bool _disposed;

  public BingWallpaperApp(
      IBingWallpaperService wallpaperService,
      IImageDownloadService downloadService,
      ILogger<BingWallpaperApp> logger)
  {
    _wallpaperService = wallpaperService ?? throw new ArgumentNullException(nameof(wallpaperService));
    _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// 运行应用程序
  /// </summary>
  /// <param name="cancellationToken">取消令牌</param>
  public async Task RunAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await _wallpaperService.RunAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "应用程序运行失败: {Message}", ex.Message);
      throw;
    }
  }

  /// <summary>
  /// 获取图片下载服务
  /// </summary>
  /// <remarks>
  /// 此方法提供对图片下载服务的访问，适用于WinUI3等需要图片下载功能的应用程序集成。
  /// </remarks>
  public IImageDownloadService GetImageDownloadService()
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    return _downloadService;
  }

  public void Dispose()
  {
    if (!_disposed)
    {
      if (_downloadService is IDisposable disposableDownloadService)
      {
        disposableDownloadService.Dispose();
      }

      _disposed = true;
    }
  }
}
