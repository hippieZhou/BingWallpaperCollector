namespace BingWallpaperCollector.Models;

/// <summary>
/// 图片下载请求信息
/// </summary>
public sealed class ImageDownloadRequest
{
  public string ImageUrl { get; set; } = string.Empty;
  public string Country { get; set; } = string.Empty;
  public string Date { get; set; } = string.Empty;
  public string Resolution { get; set; } = string.Empty;
}
