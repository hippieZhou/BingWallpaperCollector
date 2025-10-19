// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.Http.Enums;

namespace BingWallpaperGallery.Core.Http.Models;

/// <summary>
/// 图片分辨率信息
/// </summary>
public sealed class ImageResolution
{
    public ResolutionCode Resolution { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
}
