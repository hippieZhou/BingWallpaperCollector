// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.DTOs;
using BingWallpaperGallery.WinUI.Models;
using BingWallpaperGallery.WinUI.ViewModels;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;

namespace BingWallpaperGallery.WinUI.Views;

public sealed partial class GalleryPage : Page
{
    public GalleryViewModel ViewModel { get; }

    public GalleryPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<GalleryViewModel>();
    }

    public static bool IsEmpty(IncrementalLoadingCollection<WallpaperInfoSource, WallpaperInfoDto>? items)
    {
        return items is null || !items.Any();
    }
}
