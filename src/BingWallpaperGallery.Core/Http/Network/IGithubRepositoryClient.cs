using BingWallpaperGallery.Core.Http.Models;

namespace BingWallpaperGallery.Core.Http.Network;
public interface IGithubRepositoryClient
{
    Task<IEnumerable<ArchiveItem>> GetArchiveAsync(string path, CancellationToken cancellationToken = default);
    Task<WallpaperInfoStorage> GetArchiveFileAsync(string downloadUrl, CancellationToken cancellationToken = default);
}
