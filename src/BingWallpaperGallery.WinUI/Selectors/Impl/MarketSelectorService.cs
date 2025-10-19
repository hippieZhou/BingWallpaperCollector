// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.DTOs;
using BingWallpaperGallery.Core.Services;
using BingWallpaperGallery.WinUI.Services;

namespace BingWallpaperGallery.WinUI.Selectors.Impl;

public class MarketSelectorService(
    IManagementService managementService,
    ILocalSettingsService localSettingsService) :
    SelectorService(localSettingsService), IMarketSelectorService
{
    public IReadOnlyList<MarketInfoDto> SupportedMarkets { get; private set; } = [];

    public MarketInfoDto Market { get; private set; }

    protected override string SettingsKey => "AppMarket";

    public async Task InitializeAsync()
    {
        var markets = await managementService.GetSupportedMarketCodesAsync();
        SupportedMarkets = [.. markets];
        Market = await ReadFromSettingsAsync(SupportedMarkets[0]);
    }

    public async Task SetMarketAsync(MarketInfoDto market)
    {
        Market = market;
        await SaveInSettingsAsync(Market);
    }
}
