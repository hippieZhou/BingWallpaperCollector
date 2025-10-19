// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.DataAccess.Domains.Common;
using BingWallpaperGallery.Core.Helpers;
using BingWallpaperGallery.Core.Http.Enums;
using BingWallpaperGallery.Core.Http.Models;

namespace BingWallpaperGallery.Core.DataAccess.Domains;

public sealed class WallpaperEntity : BaseAuditableEntity
{
    public string Hash { get; set; }

    public DateTime ActualDate { get; set; }

    public MarketCode MarketCode { get; set; }

    public ResolutionCode ResolutionCode { get; set; }

    public string InfoJson { get; set; } = string.Empty;

    /// <summary>
    /// 壁纸信息对象，从 InfoJson 反序列化得到
    /// </summary>
    public WallpaperInfoStorage Info
    {
        get => string.IsNullOrEmpty(InfoJson) ? new WallpaperInfoStorage() :
            Json.ToObject<WallpaperInfoStorage>(InfoJson) ?? new WallpaperInfoStorage();
        set => InfoJson = value == null ? string.Empty : Json.Stringify(value);
    }
}
