// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.DTOs;

namespace BingWallpaperGallery.WinUI.Selectors;

public interface IMarketSelectorService
{
    IReadOnlyList<MarketInfoDto> SupportedMarkets { get; }
    MarketInfoDto Market { get; }

    Task InitializeAsync();
    Task SetMarketAsync(MarketInfoDto market);
}
