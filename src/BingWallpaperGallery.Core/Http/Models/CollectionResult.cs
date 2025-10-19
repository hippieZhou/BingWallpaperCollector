// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.Http.Enums;

namespace BingWallpaperGallery.Core.Http.Models;

/// <summary>
/// 壁纸收集结果
/// </summary>
public record CollectionResult(
    int TotalCollected,
    TimeSpan Duration,
    DateTime CollectionTime,
    IEnumerable<CollectedWallpaperInfo> CollectedWallpapers
);

/// <summary>
/// 单个市场的收集结果
/// </summary>
public record MarketCollectionResult(
    MarketCode MarketCode,
    int Collected,
    int Success,
    int Failure,
    TimeSpan Duration,
    string? ErrorMessage,
    IEnumerable<CollectedWallpaperInfo> Wallpapers
);

public record CollectedWallpaperInfo(
    MarketCode MarketCode,
    ResolutionCode ResolutionCode,
    DateTimeOffset CollectionDate,
    BingWallpaperInfo WallpaperInfo);
