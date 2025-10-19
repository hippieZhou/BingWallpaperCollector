using BingWallpaperGallery.Core.Http.Models;

namespace BingWallpaperGallery.Core.Http.Services;
public interface IGithubRepositoryService
{
    Task<IEnumerable<ArchiveItem>> GetArchiveListAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<WallpaperInfoStorage>> GetArchiveDetailsAsync(ArchiveItem archiveItem, CancellationToken cancellationToken = default);
}
