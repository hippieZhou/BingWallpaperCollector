// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.Http.Enums;
using BingWallpaperGallery.Core.Http.Models;

namespace BingWallpaperGallery.Core.Http.Network;

public interface IBingWallpaperClient
{
    Task<IEnumerable<BingWallpaperInfo>> GetWallpapersAsync(
        int count,
        MarketCode marketCode,
        ResolutionCode resolution,
        CancellationToken cancellationToken = default);
}
