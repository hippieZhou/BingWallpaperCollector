// Copyright (c) hippieZhou. All rights reserved.

using System.Numerics;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;

namespace BingWallpaperGallery.WinUI.Models;

/// <summary>
/// https://deviceshots.com
/// </summary>
public partial class MockupImage : IDisposable
{
    private readonly CanvasBitmap _device;
    private readonly CanvasBitmap _wallpaper;

    // Surface Book 屏幕区域的相对坐标（相对于整个 mockup 图片）
    // 这些值需要根据实际的 mockup 图片进行调整
    public Vector2 ScreenTopLeft { get; } = new Vector2(0.125f, 0.15f);  // 屏幕左上角相对位置
    public Vector2 ScreenBottomRight { get; } = new Vector2(0.875f, 0.75f); // 屏幕右下角相对位置

    // 屏幕的透视变换参数（模拟屏幕的角度）
    public Matrix3x2 ScreenTransform { get; private set; } = Matrix3x2.Identity;

    public MockupImage(CanvasBitmap device, CanvasBitmap wallpaper)
    {
        _device = device;
        _wallpaper = wallpaper;
        CalculateScreenTransform();
    }

    private void CalculateScreenTransform()
    {
        // 为了模拟 Surface Book 屏幕的透视效果
        // 这里可以根据实际需要调整变换参数
        var scaleX = 1.0f;
        var scaleY = 0.95f; // 略微压缩高度以模拟透视
        var skewX = 0.02f;  // 轻微倾斜

        ScreenTransform = new Matrix3x2(
            scaleX, skewX,
            0, scaleY,
            0, 0
        );
    }

    /// <summary>
    /// 计算 mockup 图片的显示区域，保持宽高比
    /// </summary>
    /// <param name="canvasSize"></param>
    /// <returns></returns>
    private Rect CalculateMockupRect(Size canvasSize)
    {
        var mockupSize = _device.Size;
        var scale = Math.Min(canvasSize.Width / mockupSize.Width, canvasSize.Height / mockupSize.Height);
        var scaledWidth = mockupSize.Width * scale;
        var scaledHeight = mockupSize.Height * scale;
        var offsetX = (canvasSize.Width - scaledWidth) / 2;
        var offsetY = (canvasSize.Height - scaledHeight) / 2;

        return new Rect(offsetX, offsetY, scaledWidth, scaledHeight);
    }

    public void DrawImage(CanvasDrawingSession session, Size canvasSize)
    {
        var mockupRect = CalculateMockupRect(canvasSize);
        session.DrawImage(_device, mockupRect);

        // 如果有用户图片，将其绘制到屏幕区域
        if (_wallpaper != null)
        {
        }
    }

    public void Dispose()
    {
        _device.Dispose();
        _wallpaper.Dispose();
    }
}
