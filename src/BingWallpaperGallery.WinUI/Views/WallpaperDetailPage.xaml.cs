// Copyright (c) hippieZhou. All rights reserved.

using System.Globalization;
using BingWallpaperGallery.WinUI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BingWallpaperGallery.WinUI.Views;

public sealed partial class WallpaperDetailPage : Page
{
    public CultureInfo Culture { get; } 
    public WallpaperDetailViewModel ViewModel { get; }

    public WallpaperDetailPage()
    {
        InitializeComponent();
        Culture = CultureInfo.CurrentCulture;
        ViewModel = App.GetService<WallpaperDetailViewModel>();
    }

    private void ToggleEditState()
    {
        WallpaperView.IsPaneOpen = !WallpaperView.IsPaneOpen;
    }
}
