namespace BingWallpaperGallery.WinUI.Selectors;

public interface ILoggingSelectorService
{
    long FolderSizeInBytes { get; }
    Task InitializeAsync();
    void CleanUpOldLogs();
}
