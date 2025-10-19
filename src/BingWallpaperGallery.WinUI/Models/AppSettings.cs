// Copyright (c) hippieZhou. All rights reserved.

using System.Reflection;
using BingWallpaperGallery.WinUI.Helpers;
using CommunityToolkit.WinUI;
using Microsoft.Windows.ApplicationModel.WindowsAppRuntime;
using Windows.ApplicationModel;
using Windows.Storage;

namespace BingWallpaperGallery.WinUI.Models;

public partial class AppSettings
{
    public readonly string LocalFolder = ApplicationData.Current.LocalFolder.Path;

    public readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

    public readonly string DefaulttLocalLogFolder = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "Log");

    public readonly string DefaultPicturesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "BingWallpaper");
    public readonly string TemporaryPath = ApplicationData.Current.TemporaryFolder.Path;

    public static string AppTitle => "AppDisplayName".GetLocalized();

    public static string AppVersion
    {
        get
        {
            Version version;

            if (RuntimeHelper.IsMSIX)
            {
                var packageVersion = Package.Current.Id.Version;

                version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
            }
            else
            {
                version = Assembly.GetExecutingAssembly().GetName().Version!;
            }

            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }

    public static string OSVersion
    {
        get
        {
            var (major, minor, build, desc) = RuntimeHelper.GetOSVersion();
            return $"Windows {major}.{minor} (Build {build})";
        }
    }

    public static string WinAppSdkDetails => $"Windows App SDK {ReleaseInfo.Major}.{ReleaseInfo.Minor}";

    public static string WinAppSdkRuntimeDetails => WinAppSdkDetails + $", Windows App Runtime {RuntimeInfo.AsString}";
}

public partial class AppSettings
{
    #region Singleton
    static AppSettings()
    {
        Current = new AppSettings();
    }

    private AppSettings()
    {
    }

    public static AppSettings Current { get; }
    #endregion
}
