// Copyright (c) hippieZhou. All rights reserved.

namespace BingWallpaperGallery.WinUI.Activation;

public interface IActivationHandler
{
    bool CanHandle(object args);

    Task HandleAsync(object args);
}
