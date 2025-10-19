namespace BingWallpaperGallery.Core.Services;
public interface IGitHubStorageService
{
    Task RunAsync(
        Action<string> onLoading = null,
        Action<Exception> onError = null,
        CancellationToken cancellationToken = default);
}
