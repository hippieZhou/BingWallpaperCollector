// Copyright (c) hippieZhou. All rights reserved.

using Microsoft.UI.Xaml;

namespace BingWallpaperGallery.WinUI.Selectors;

public interface IThemeSelectorService
{
    ElementTheme Theme
    {
        get;
    }

    Task InitializeAsync();

    Task SetThemeAsync(ElementTheme theme);

    Task SetRequestedThemeAsync();
}
