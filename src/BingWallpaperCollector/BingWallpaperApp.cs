using BingWallpaperCollector.Services;
using Microsoft.Extensions.Logging;

namespace BingWallpaperCollector;

/// <summary>
/// 必应壁纸信息收集器主应用类
/// </summary>
public sealed class BingWallpaperApp : IDisposable
{
  private readonly IBingWallpaperService _wallpaperService;
  private readonly ILogger<BingWallpaperApp> _logger;
  private bool _disposed;

  public BingWallpaperApp(
      IBingWallpaperService wallpaperService,
      ILogger<BingWallpaperApp> logger)
  {
    _wallpaperService = wallpaperService ?? throw new ArgumentNullException(nameof(wallpaperService));
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

  public void Dispose()
  {
    if (!_disposed)
    {
      _disposed = true;
    }
  }
}
