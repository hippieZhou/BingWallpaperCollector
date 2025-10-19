// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.WinUI.Notifications;
using BingWallpaperGallery.WinUI.Services;
using BingWallpaperGallery.WinUI.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;

namespace BingWallpaperGallery.WinUI.Activation;

public class AppNotificationActivationHandler(INavigationService navigationService, IAppNotificationService notificationService) : ActivationHandler<LaunchActivatedEventArgs>
{
    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        return AppInstance.GetCurrent().GetActivatedEventArgs()?.Kind == ExtendedActivationKind.AppNotification;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        // TODO: Handle notification activations.

        // Access the AppNotificationActivatedEventArgs.
        var activatedEventArgs = (AppNotificationActivatedEventArgs)AppInstance.GetCurrent().GetActivatedEventArgs().Data;

        // Navigate to a specific page based on the notification arguments.
        if (notificationService.ParseArguments(activatedEventArgs.Argument)["action"] == "Settings")
        {
            // Queue navigation with low priority to allow the UI to initialize.
            App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () => navigationService.NavigateTo(typeof(SettingsViewModel).FullName!));
        }

        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            App.MainWindow.ShowMessageDialogAsync("TODO: Handle notification activations.", "Notification Activation");
        });

        await Task.CompletedTask;
    }
}
