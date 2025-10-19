// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.WinUI.Helpers;
using BingWallpaperGallery.WinUI.ViewModels;

namespace BingWallpaperGallery.WinUI;

public sealed partial class ShellWindow : WindowEx
{
    public ShellViewModel ViewModel { get; } = App.GetService<ShellViewModel>();

    public ShellWindow()
    {
        InitializeComponent();
        this.CenterOnScreen();
        this.SetAppTitleBar(AppTitleBar, "Assets/WindowIcon.ico");
        ViewModel.Initialize(
            NavView,
            NavFrame,
            InAppNotification.NotificationQueue);
    }
}
