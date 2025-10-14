namespace BingWallpaperCollector.Enums;

/// <summary>
/// 下载状态枚举
/// </summary>
public enum DownloadStatus
{
  /// <summary>
  /// 准备开始
  /// </summary>
  Starting,

  /// <summary>
  /// 正在下载
  /// </summary>
  Downloading,

  /// <summary>
  /// 下载完成
  /// </summary>
  Completed,

  /// <summary>
  /// 下载失败
  /// </summary>
  Failed,

  /// <summary>
  /// 已跳过（文件已存在）
  /// </summary>
  Skipped
}
