using System.ComponentModel;

namespace BingWallpaperCollector.Enums;

/// <summary>
/// API请求支持的分辨率类型
/// </summary>
public enum ApiResolution
{
  /// <summary>
  /// 4K Ultra HD - 3840x2160
  /// </summary>
  [Description("4K Ultra HD")]
  UHD4K,

  /// <summary>
  /// 2K QHD - 2560x1440
  /// </summary>
  [Description("2K QHD")]
  QHD2K,

  /// <summary>
  /// Full HD - 1920x1080
  /// </summary>
  [Description("Full HD")]
  FullHD,

  /// <summary>
  /// HD - 1280x720
  /// </summary>
  [Description("HD")]
  HD,

  /// <summary>
  /// 标准分辨率 - 1366x768
  /// </summary>
  [Description("Standard")]
  Standard
}
