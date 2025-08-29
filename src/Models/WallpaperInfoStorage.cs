namespace BingWallpaperCollector.Models;

/// <summary>
/// 完整的壁纸信息存储模型
/// </summary>
public sealed class WallpaperInfoStorage
{
  public string Date { get; set; } = string.Empty;
  public string Country { get; set; } = string.Empty;
  public string MarketCode { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Copyright { get; set; } = string.Empty;
  public string CopyrightLink { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Quiz { get; set; } = string.Empty;
  public string Hash { get; set; } = string.Empty;
  public List<ImageResolution> ImageResolutions { get; set; } = [];
  public WallpaperTimeInfo TimeInfo { get; set; } = new();
  public DateTime CreatedAt { get; set; } = DateTime.Now;
  public string OriginalUrlBase { get; set; } = string.Empty;
}
