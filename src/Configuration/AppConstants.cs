namespace BingWallpaperCollector.Configuration;

/// <summary>
/// 应用程序常量
/// </summary>
public static class AppConstants
{
  /// <summary>
  /// 必应壁纸API地址模板 (支持可配置分辨率)
  /// 参数说明: {0}=dayIndex, {1}=count, {2}=marketCode, {3}=simpleLanguageCode, {4}=width, {5}=height
  /// 特性: 使用global.bing.com端点，支持可配置分辨率，包含完整的API参数
  /// </summary>
  public const string BingApiUrlTemplate = "https://global.bing.com/HPImageArchive.aspx?format=js&idx={0}&n={1}&pid=hp&FORM=BEHPTB&uhd=1&uhdwidth={4}&uhdheight={5}&setmkt={2}&setlang={3}";

  /// <summary>
  /// 必应基础URL
  /// </summary>
  public const string BingBaseUrl = "https://www.bing.com";

  /// <summary>
  /// Bing API支持的最大历史天数
  /// </summary>
  public const int MaxHistoryDays = 8;

  /// <summary>
  /// 最大并发下载数
  /// </summary>
  public const int MaxConcurrentDownloads = 5;

  /// <summary>
  /// 默认并发请求数
  /// </summary>
  public const int DefaultConcurrentRequests = 3;

  /// <summary>
  /// HTTP超时时间（秒）
  /// </summary>
  public const int HttpTimeoutSeconds = 30;

  /// <summary>
  /// 进度报告间隔（毫秒）
  /// </summary>
  public const int ProgressReportIntervalMs = 100;

  /// <summary>
  /// 数据目录名称
  /// </summary>
  public const string DataDirectoryName = "archive";

  /// <summary>
  /// 图片子目录名称
  /// </summary>
  public const string ImagesSubDirectoryName = "Images";

  /// <summary>
  /// 支持的图片分辨率
  /// </summary>
  public static readonly Dictionary<string, (string Suffix, string Description)> ImageResolutions = new()
    {
        { "UHD", ("_UHD.jpg", "Ultra High Definition (~4K)") },
        { "HD", ("_1920x1200.jpg", "1920x1200") },
        { "Full HD", ("_1920x1080.jpg", "1920x1080") },
        { "Standard", ("_1366x768.jpg", "1366x768") }
    };

  /// <summary>
  /// HTTP请求头
  /// </summary>
  public static class HttpHeaders
  {
    public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
    public const string Accept = "application/json,text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
    public const string AcceptImage = "image/webp,image/apng,image/*,*/*;q=0.8";
    public const string AcceptEncoding = "gzip, deflate, br";
    public const string CacheControl = "no-cache";
  }
}
