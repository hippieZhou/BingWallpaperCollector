// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core;
using BingWallpaperGallery.WinUI.Activation;
using BingWallpaperGallery.WinUI.Helpers;
using BingWallpaperGallery.WinUI.Models;
using BingWallpaperGallery.WinUI.Notifications;
using BingWallpaperGallery.WinUI.Notifications.Impl;
using BingWallpaperGallery.WinUI.Options;
using BingWallpaperGallery.WinUI.Selectors;
using BingWallpaperGallery.WinUI.Selectors.Impl;
using BingWallpaperGallery.WinUI.Services;
using BingWallpaperGallery.WinUI.Services.Impl;
using BingWallpaperGallery.WinUI.ViewModels;
using BingWallpaperGallery.WinUI.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Serilog;

namespace BingWallpaperGallery.WinUI;

internal class Bootstrapper
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    private readonly IHost _host;

    public Bootstrapper()
    {
        _host = Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
             {
                 // 使用 SerilogConfigurationHelper 来初始化 Serilog
                 var logger = SerilogConfigurationHelper.ConfigureLogger(
                     AppSettings.Current.DefaulttLocalLogFolder,
                     context.Configuration);
                 services.AddLogging(x =>
                 {
                     x.ClearProviders();
                     x.AddSerilog(logger);
                 });

                 // Default Activation Handler
                 services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                 // Other Activation Handlers
                 services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

                 #region Selectors
                 services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                 services.AddSingleton<IMarketSelectorService, MarketSelectorService>();
                 services.AddSingleton<ILanguageSelectorService, LanguageSelectorService>();
                 services.AddSingleton<IDownloadSelectorService, DownloadSelectorService>();
                 services.AddSingleton<ILoggingSelectorService, LoggingSelectorService>();
                 #endregion

                 #region Notifications
                 services.AddActivatedSingleton<IAppNotificationService, AppNotificationService>();
                 services.AddActivatedSingleton<IInAppNotificationService, InAppNotificationService>();
                 services.AddSingleton<IMessenger, WeakReferenceMessenger>();
                 #endregion

                 #region Services
                 services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                 services.AddSingleton<IActivationService, ActivationService>();
                 services.AddSingleton<IPageService, PageService>();
                 services.AddSingleton<INavigationService, NavigationService>();
                 services.AddSingleton<INavigationViewService, NavigationViewService>();
                 services.AddSingleton<IImageRenderService, ImageRenderService>();
                 services.AddSingleton<IImageExportService, ImageExportService>();
                 #endregion

                 #region Views and ViewModels
                 services.AddTransient<SettingsViewModel>();
                 services.AddTransient<SettingsPage>();
                 services.AddTransient<DownloadViewModel>();
                 services.AddTransient<DownloadPage>();
                 services.AddTransient<GalleryViewModel>();
                 services.AddTransient<GalleryPage>();
                 services.AddTransient<WallpaperDetailViewModel>();
                 services.AddTransient<WallpaperDetailPage>();
                 services.AddTransient<ShellViewModel>();
                 #endregion

                 // Configuration
                 services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
                 services.Configure<LoggingOptions>(context.Configuration.GetSection(nameof(LoggingOptions)));

                 // Core Services
                 services.AddCorelayer(context.Configuration, AppSettings.Current.LocalFolder);
             }).Build();
    }

    public T GetService<T>() where T : class
    {
        if (_host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    internal async Task StartAsync(LaunchActivatedEventArgs args)
    {
        _host.Start();

        var activation = App.GetService<IActivationService>();
        await activation.ActivateAsync(args);

        //var notification = GetService<IAppNotificationService>();
        //notification.Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        App.GetService<ILogger<App>>().LogInformation("The app has been launched successfully.");
    }

    internal void HandleException(Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        GetService<ILogger<App>>().LogError(e.Exception, e.Message);
    }
}
