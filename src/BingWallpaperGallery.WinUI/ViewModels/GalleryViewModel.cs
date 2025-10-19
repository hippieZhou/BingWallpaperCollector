// Copyright (c) hippieZhou. All rights reserved.

using System.Collections.ObjectModel;
using BingWallpaperGallery.Core.DTOs;
using BingWallpaperGallery.Core.Services;
using BingWallpaperGallery.WinUI.Models;
using BingWallpaperGallery.WinUI.Notifications;
using BingWallpaperGallery.WinUI.Selectors;
using BingWallpaperGallery.WinUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Collections;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BingWallpaperGallery.WinUI.ViewModels;

public partial class GalleryViewModel : ObservableRecipient, INavigationAware
{
    private readonly IMarketSelectorService _marketSelectorService;
    private readonly IManagementService _managementService;
    private readonly IInAppNotificationService _inAppNotificationService;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<GalleryViewModel> _logger;

    [ObservableProperty]
    public partial IncrementalLoadingCollection<WallpaperInfoSource, WallpaperInfoDto> Wallpapers { get; set; }

    [ObservableProperty]
    public partial WallpaperInfoDto Today { get; set; }

    public ObservableCollection<MarketInfoDto> Markets { get; private set; }

    [ObservableProperty]
    public partial MarketInfoDto SelectedMarket { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial WallpaperInfoDto SelectedWallpaper { get; set; }

    public GalleryViewModel(
        IMarketSelectorService marketSelectorService,
        IManagementService managementService,
        IInAppNotificationService inAppNotificationService,
        IMemoryCache memoryCache,
        ILogger<GalleryViewModel> logger)
    {
        _marketSelectorService = marketSelectorService;
        _managementService = managementService;
        _inAppNotificationService = inAppNotificationService;
        _memoryCache = memoryCache;
        _logger = logger;

        Markets = new ObservableCollection<MarketInfoDto>(_marketSelectorService.SupportedMarkets);
        SelectedMarket = _marketSelectorService.Market;
    }

    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo(object parameter)
    {
    }

    partial void OnSelectedMarketChanged(MarketInfoDto oldValue, MarketInfoDto newValue)
    {
        if (oldValue is null || newValue is null || oldValue.Code == newValue.Code)
        {
            return;
        }

        this.LoadedCommand.Execute(CancellationToken.None);
    }

    [RelayCommand(IncludeCancelCommand = true, AllowConcurrentExecutions = false)]
    private async Task OnLoaded(CancellationToken cancellationToken = default)
    {
        if (SelectedMarket is null)
        {
            return;
        }

        await _marketSelectorService.SetMarketAsync(SelectedMarket);

        Today = await _managementService.GetLatestAsync(SelectedMarket, cancellationToken);
        Wallpapers = _memoryCache.GetOrCreate(SelectedMarket, entry =>
        {
            return new IncrementalLoadingCollection<WallpaperInfoSource, WallpaperInfoDto>(
                source: new WallpaperInfoSource(_marketSelectorService, _managementService),
                itemsPerPage: 10,
                onStartLoading: () => { IsLoading = true; },
                onEndLoading: () => { IsLoading = false; },
                onError: ex =>
                {
                    _logger.LogError(ex, $"加载壁纸失败: {ex.Message}");

                    IsLoading = false;

                    _inAppNotificationService.ShowError(
                      message: $"加载壁纸失败: {ex.Message}",
                      title: "加载失败",
                      details: ex.ToString(),
                      showRetryButton: true,
                      retryAction: () => LoadedCommand.Execute(null));
                });
        });
    }

    [RelayCommand]
    private void OnWallpaperSelected(WallpaperInfoDto wallpaper)
    {
        SelectedWallpaper = wallpaper;

        // 导航到详情页
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo<WallpaperDetailViewModel>(wallpaper);
    }
}
