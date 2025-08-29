namespace BingWallpaperCollector.Models;

/// <summary>
/// 时间信息类
/// </summary>
public sealed class WallpaperTimeInfo
{
  /// <summary>
  /// 开始日期
  /// </summary>
  public DateOnly? StartDate { get; set; }

  /// <summary>
  /// 结束日期
  /// </summary>
  public DateOnly? EndDate { get; set; }

  /// <summary>
  /// 完整开始时间 (ISO 8601 格式)
  /// </summary>
  public DateTime? FullStartDateTime { get; set; }

  /// <summary>
  /// 原始Bing API时间字段
  /// </summary>
  public BingApiTimeFields OriginalTimeFields { get; set; } = new();

  /// <summary>
  /// 从Bing API时间字段创建WallpaperTimeInfo
  /// </summary>
  public static WallpaperTimeInfo FromBingApiFields(string? startDate, string? fullStartDate, string? endDate)
  {
    var timeInfo = new WallpaperTimeInfo
    {
      OriginalTimeFields = new BingApiTimeFields
      {
        StartDate = startDate ?? string.Empty,
        FullStartDate = fullStartDate ?? string.Empty,
        EndDate = endDate ?? string.Empty
      }
    };

    try
    {
      // 解析开始日期 (YYYYMMDD -> DateOnly)
      if (!string.IsNullOrEmpty(startDate) && startDate.Length == 8)
      {
        if (TryParseDate(startDate, out var parsedStartDate))
        {
          timeInfo.StartDate = parsedStartDate;
        }
      }

      // 解析结束日期 (YYYYMMDD -> DateOnly)
      if (!string.IsNullOrEmpty(endDate) && endDate.Length == 8)
      {
        if (TryParseDate(endDate, out var parsedEndDate))
        {
          timeInfo.EndDate = parsedEndDate;
        }
      }

      // 解析完整开始时间 (YYYYMMDDHHMM -> DateTime)
      if (!string.IsNullOrEmpty(fullStartDate) && fullStartDate.Length == 12)
      {
        if (TryParseFullDateTime(fullStartDate, out var parsedDateTime))
        {
          timeInfo.FullStartDateTime = parsedDateTime;
        }
      }
    }
    catch (Exception)
    {
      // 忽略解析错误，保留原始字段值
    }

    return timeInfo;
  }

  private static bool TryParseDate(string dateString, out DateOnly date)
  {
    date = default;
    if (dateString.Length != 8) return false;

    try
    {
      var year = int.Parse(dateString.AsSpan(0, 4));
      var month = int.Parse(dateString.AsSpan(4, 2));
      var day = int.Parse(dateString.AsSpan(6, 2));
      date = new DateOnly(year, month, day);
      return true;
    }
    catch
    {
      return false;
    }
  }

  private static bool TryParseFullDateTime(string dateTimeString, out DateTime dateTime)
  {
    dateTime = default;
    if (dateTimeString.Length != 12) return false;

    try
    {
      var year = int.Parse(dateTimeString.AsSpan(0, 4));
      var month = int.Parse(dateTimeString.AsSpan(4, 2));
      var day = int.Parse(dateTimeString.AsSpan(6, 2));
      var hour = int.Parse(dateTimeString.AsSpan(8, 2));
      var minute = int.Parse(dateTimeString.AsSpan(10, 2));

      dateTime = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
      return true;
    }
    catch
    {
      return false;
    }
  }
}

/// <summary>
/// Bing API原始时间字段
/// </summary>
public sealed class BingApiTimeFields
{
  /// <summary>
  /// 原始开始日期 (YYYYMMDD)
  /// </summary>
  public string StartDate { get; set; } = string.Empty;

  /// <summary>
  /// 原始完整开始时间 (YYYYMMDDHHMM)
  /// </summary>
  public string FullStartDate { get; set; } = string.Empty;

  /// <summary>
  /// 原始结束日期 (YYYYMMDD)
  /// </summary>
  public string EndDate { get; set; } = string.Empty;
}
