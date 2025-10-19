// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.Http.Enums;

namespace BingWallpaperGallery.Core.DTOs;

/// <summary>
/// 市场信息数据传输对象
/// </summary>
/// <param name="Code">市场代码</param>
/// <param name="Name">市场名称</param>
/// <param name="Description">市场描述</param>
/// <param name="Flag">国旗表情符号</param>
/// <param name="Note">备注信息</param>
public record MarketInfoDto(
    MarketCode Code,
    string Name,
    string Description,
    string Flag,
    string Note);
