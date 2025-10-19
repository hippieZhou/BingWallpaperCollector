using BingWallpaperGallery.Core.DTOs;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace BingWallpaperGallery.WinUI.UserControls;

public sealed partial class CustomCard : UserControl
{
    public CustomCard()
    {
        InitializeComponent();
    }

    [GeneratedDependencyProperty]
    public partial WallpaperInfoDto? Model { get; set; }
}
