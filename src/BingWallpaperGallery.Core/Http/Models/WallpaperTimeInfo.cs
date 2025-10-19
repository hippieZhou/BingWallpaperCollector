// Copyright (c) hippieZhou. All rights reserved.

namespace BingWallpaperGallery.Core.Http.Models;

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
    public static WallpaperTimeInfo FromBingApiFields(DateOnly startDate, DateTime fullStartDate, DateOnly endDate)
    {
        return new WallpaperTimeInfo
        {
            StartDate = startDate,
            FullStartDateTime = fullStartDate,
            EndDate = endDate,
            OriginalTimeFields = new BingApiTimeFields
            {
                StartDate = startDate,
                FullStartDate = fullStartDate,
                EndDate = endDate
            }
        };
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
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// 原始完整开始时间 (YYYYMMDDHHMM)
    /// </summary>
    public DateTime FullStartDate { get; set; }

    /// <summary>
    /// 原始结束日期 (YYYYMMDD)
    /// </summary>
    public DateOnly EndDate { get; set; }
}
