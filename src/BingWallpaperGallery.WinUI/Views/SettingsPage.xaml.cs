// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.WinUI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BingWallpaperGallery.WinUI.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<SettingsViewModel>();
    }
}
