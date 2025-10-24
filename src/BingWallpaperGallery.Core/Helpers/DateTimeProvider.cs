// Copyright (c) hippieZhou. All rights reserved.

namespace BingWallpaperGallery.Core.Helpers;

public static class DateTimeProvider
{
    public static DateTimeOffset GetUtcNow()
    {
        return DateTime.UtcNow;
    }
}
