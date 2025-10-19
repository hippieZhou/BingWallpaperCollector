// Copyright (c) hippieZhou. All rights reserved.

namespace BingWallpaperGallery.WinUI.Services;

public interface INavigationAware
{
    void OnNavigatedTo(object parameter);

    void OnNavigatedFrom();
}
