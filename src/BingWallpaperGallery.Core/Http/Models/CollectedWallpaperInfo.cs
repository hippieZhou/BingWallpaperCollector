// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.Http.Enums;

namespace BingWallpaperGallery.Core.Http.Models;

public record CollectedWallpaperInfo(
    MarketCode MarketCode,
    ResolutionCode ResolutionCode,
    DateTimeOffset CollectionDate,
    BingWallpaperInfo WallpaperInfo);
