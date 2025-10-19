using BingWallpaperGallery.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BingWallpaperGallery.WinUI.UserControls;
public sealed partial class SynchronizationDialog : ContentDialog
{
    #region Singleton
    static SynchronizationDialog()
    {
        Current = new SynchronizationDialog();
    }

    public static SynchronizationDialog Current { get; }
    #endregion

    public SynchronizationDialogViewModel ViewModel { get; } = new();
    private SynchronizationDialog()
    {
        InitializeComponent();
        XamlRoot = App.MainWindow.Content.XamlRoot;
    }

    private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        args.Cancel = ViewModel.Cancel;
    }
}

public partial class SynchronizationDialogViewModel : ObservableObject
{
    private readonly IGitHubStorageService _githubStorageService;
    private readonly ILogger<SynchronizationDialogViewModel> _logger;

    [ObservableProperty]
    public partial string Message { get; set; } = "该操作是全量同步，可能会消耗较长时间，请耐心等待...";

    [ObservableProperty]
    public partial bool Cancel { get; set; } = true;

    public SynchronizationDialogViewModel()
    {
        _githubStorageService = App.GetService<IGitHubStorageService>();
        _logger = App.GetService<ILogger<SynchronizationDialogViewModel>>();
    }

    [RelayCommand(IncludeCancelCommand = true, AllowConcurrentExecutions = false)]
    private async Task OnLoaded(CancellationToken cancellationToken = default)
    {
        await _githubStorageService.RunAsync(
            onLoading: msg => Message = msg,
            onError: ex =>
            {
                _logger.LogError("获取 GitHub 归档文件失败: {message}", ex.Message);
                Message = "获取 GitHub 归档文件失败，请检查网络连接或稍后重试。";
            }, cancellationToken);
    }

    [RelayCommand]
    private void OnCloseButton()
    {
        Cancel = false;
    }
}
