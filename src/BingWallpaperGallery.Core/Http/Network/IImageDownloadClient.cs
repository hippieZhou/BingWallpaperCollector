// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.Http.Models;

namespace BingWallpaperGallery.Core.Http.Network;

/// <summary>
/// 图片下载客户端接口
/// </summary>
public interface IImageDownloadClient
{
    Task<string> DownloadImageAsync(
        FileDownloadRequest request,
        IProgress<FileDownloadProgress> progress,
        CancellationToken cancellationToken);
}
