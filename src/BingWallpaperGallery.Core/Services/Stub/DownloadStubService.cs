using BingWallpaperGallery.Core.DataAccess.Repositories;
using BingWallpaperGallery.Core.DTOs;
using BingWallpaperGallery.Core.Http.Services;
using BingWallpaperGallery.Core.Services.Impl;
using Microsoft.Extensions.Logging;

namespace BingWallpaperGallery.Core.Services.Stub;
public class DownloadStubService(
    IWallpaperRepository wallpaperRepository,
    IImageDownloadService imageDownloadService,
    ILogger<DownloadService> logger) :
    DownloadService(wallpaperRepository, imageDownloadService, logger), IDownloadService
{
    protected async override Task<string> GetImageUrlAsync(WallpaperInfoDto wallpaper, ResolutionInfoDto resolution)
    {
        //https://esahubble.org/images/heic2007a/
        //var wallpaper = new WallpaperInfoDto(
        //    Id: Guid.NewGuid(),
        //    Hash: "test-hash",
        //    ActualDate: DateTime.Now,
        //    Startdate: DateOnly.FromDateTime(DateTime.Now),
        //    Enddate: DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
        //    Fullstartdate: DateTime.Now,
        //    Market: new MarketInfoDto(MarketCode.UnitedStates, "United States", "United States", "🇺🇸", ""),
        //    Title: "Test Wallpaper",
        //    Copyright: "Test Copyright",
        //    CopyrightOnly: "Test",
        //    CopyrightLink: "https://test.com",
        //    Caption: "Test Caption",
        //    Description: "Test Description",
        //    Url: "https://esahubble.org/media/archives/images/original/heic2007a.tif"
        //);

        //var resolution = new ResolutionInfoDto(
        //    ResolutionCode.UHD4K,
        //    ResolutionCode.UHD4K.GetName(),
        //    ResolutionCode.UHD4K.GetSuffix());
        return await Task.FromResult(wallpaper.Url);
    }
}
