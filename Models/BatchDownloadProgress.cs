namespace BingWallpaperCollector.Models;

/// <summary>
/// 批量下载进度信息
/// </summary>
public sealed class BatchDownloadProgress
{
  /// <summary>
  /// 总文件数
  /// </summary>
  public int TotalFiles { get; set; }

  /// <summary>
  /// 已完成的文件数
  /// </summary>
  public int CompletedFiles { get; set; }

  /// <summary>
  /// 失败的文件数
  /// </summary>
  public int FailedFiles { get; set; }

  /// <summary>
  /// 整体进度百分比 (0-100)
  /// </summary>
  public double OverallPercentage { get; set; }

  /// <summary>
  /// 当前正在下载的文件进度
  /// </summary>
  public FileDownloadProgress? CurrentFileProgress { get; set; }

  /// <summary>
  /// 总下载速度 (字节/秒)
  /// </summary>
  public double TotalBytesPerSecond { get; set; }

  /// <summary>
  /// 开始时间
  /// </summary>
  public DateTime StartTime { get; set; }

  /// <summary>
  /// 已用时间
  /// </summary>
  public TimeSpan ElapsedTime { get; set; }
}
