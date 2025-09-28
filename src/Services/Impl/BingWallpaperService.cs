using System.Text.Json;
using BingWallpaperCollector.Configuration;
using BingWallpaperCollector.Enums;
using BingWallpaperCollector.Extensions;
using BingWallpaperCollector.Models;
using BingWallpaperCollector.Services;
using Microsoft.Extensions.Logging;

namespace BingWallpaperCollector.Services.Impl;

/// <summary>
/// 必应壁纸信息收集服务实现
/// </summary>
public sealed class BingWallpaperService : IBingWallpaperService
{
  private readonly HttpClient _httpClient;
  private readonly ILogger<BingWallpaperService> _logger;
  private readonly IUserConfigurationService _configService;
  private readonly IWallpaperStorageService _storageService;
  private readonly string _dataDirectory;

  // JSON序列化配置 - 用于API响应解析
  private static readonly JsonSerializerOptions _jsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  public BingWallpaperService(
      HttpClient httpClient,
      ILogger<BingWallpaperService> logger,
      IUserConfigurationService configService,
      IWallpaperStorageService storageService)
  {
    _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    _dataDirectory = Path.Combine(Environment.CurrentDirectory, AppConstants.DataDirectoryName);
  }

  public async Task RunAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogInformation("🚀 应用程序启动，开始初始化...");

      // 获取用户配置
      var config = _configService.GetUserConfig();

      _logger.LogInformation("=== 开始收集必应壁纸信息 ===");
      _logger.LogInformation("配置信息:");
      _logger.LogInformation("  - 目标国家: {Country}", config.CollectAllCountries ? "所有支持的国家" : config.MarketCode.ToString());
      _logger.LogInformation("  - 历史天数: {Days} 天", config.DaysToCollect);
      _logger.LogInformation("  - 并发请求: {Concurrent} 个", config.MaxConcurrentRequests);
      _logger.LogInformation("  - JSON格式: {Format}", config.PrettyJsonFormat ? "美化" : "压缩");
      _logger.LogInformation("================================");

      if (config.CollectAllCountries)
      {
        await CollectForAllCountriesAsync(config, cancellationToken);
      }
      else
      {
        await CollectForCountryAsync(config.MarketCode, config.DaysToCollect, config, cancellationToken);
      }

      _logger.LogInformation("所有壁纸信息收集完成！");
      _logger.LogInformation("数据存储目录: {DataDirectory}", _dataDirectory);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "运行过程中发生错误: {Message}", ex.Message);
      throw;
    }
  }

  public async Task CollectForCountryAsync(
      MarketCode marketCode,
      int daysToCollect,
      CollectionConfig config,
      CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(config);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(daysToCollect);

    var marketCodeStr = marketCode.GetDescription();
    _logger.LogInformation("开始为 {Country} ({MarketCode}) 收集 {Days} 天的历史壁纸信息...",
        marketCode.ToString(), marketCodeStr, daysToCollect);

    var collectTasks = new List<Task>();
    var semaphore = new SemaphoreSlim(AppConstants.DefaultConcurrentRequests, AppConstants.DefaultConcurrentRequests);

    try
    {
      for (int dayIndex = 0; dayIndex < daysToCollect; dayIndex++)
      {
        collectTasks.Add(CollectWallpaperInfoForDayAsync(marketCode, dayIndex, config, semaphore, cancellationToken));
      }

      await Task.WhenAll(collectTasks);

      // 检查收集结果统计
      var countryDir = Path.Combine(_dataDirectory, marketCode.ToString());
      var fileCount = 0;
      if (Directory.Exists(countryDir))
      {
        fileCount = Directory.GetFiles(countryDir, "*.json", SearchOption.TopDirectoryOnly).Length;
      }

      _logger.LogInformation("✅ {Country} 的壁纸信息收集完成 - 共有 {FileCount} 个文件", marketCode.ToString(), fileCount);
    }
    finally
    {
      semaphore.Dispose();
    }
  }

  public async Task<BingWallpaperInfo?> GetWallpaperInfoAsync(
      MarketCode marketCode,
      int dayIndex,
      CollectionConfig config,
      CancellationToken cancellationToken = default)
  {
    try
    {
      var marketCodeStr = marketCode.GetDescription();
      var languageCode = GetLanguageCodeFromMarket(marketCodeStr);
      var simpleLanguageCode = GetSimpleLanguageCode(marketCodeStr);

      // 构建支持可配置分辨率的API请求URL
      // 使用 global.bing.com 端点以获取更高质量的图片数据
      var apiUrl = string.Format(AppConstants.BingApiUrlTemplate, dayIndex, 1, marketCodeStr, simpleLanguageCode, config.DefaultResolution.GetWidth(), config.DefaultResolution.GetHeight());

      _logger.LogDebug("正在获取 {Country} 第 {Day} 天的壁纸信息 ({ResolutionName} {Width}x{Height})...", marketCode.ToString(), dayIndex + 1, config.DefaultResolution.GetDescription(), config.DefaultResolution.GetWidth(), config.DefaultResolution.GetHeight());

      // 创建带有特定语言头的请求
      using var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

      // 设置Accept-Language头以获取对应语言的内容  
      request.Headers.Add("Accept-Language", $"{languageCode},en;q=0.9");

      // 设置User-Agent以模拟浏览器请求
      request.Headers.Add("User-Agent", AppConstants.HttpHeaders.UserAgent);

      // 添加其他可能有用的请求头
      request.Headers.Add("Accept", AppConstants.HttpHeaders.Accept);
      request.Headers.Add("Cache-Control", AppConstants.HttpHeaders.CacheControl);

      _logger.LogInformation("🌐 正在发送请求到 Bing API: {ApiUrl}", apiUrl);
      var response = await _httpClient.SendAsync(request, cancellationToken);
      _logger.LogInformation("✅ 收到 Bing API 响应，状态码: {StatusCode}", response.StatusCode);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync(cancellationToken);
      var apiResponse = JsonSerializer.Deserialize<BingApiResponse>(content, _jsonOptions);

      if (apiResponse?.Images?.Count > 0)
      {
        return apiResponse.Images[0];
      }

      return null;
    }
    catch (HttpRequestException ex)
    {
      _logger.LogError(ex, "获取 {Country} 第 {Day} 天壁纸信息失败: {Message}", marketCode.ToString(), dayIndex + 1, ex.Message);
      return null;
    }
    catch (JsonException ex)
    {
      _logger.LogError(ex, "解析 {Country} 壁纸JSON数据失败: {Message}", marketCode.ToString(), ex.Message);
      return null;
    }
  }

  /// <summary>
  /// 为所有国家收集壁纸信息
  /// </summary>
  private async Task CollectForAllCountriesAsync(CollectionConfig config, CancellationToken cancellationToken)
  {
    var countries = Enum.GetValues<MarketCode>();
    var semaphore = new SemaphoreSlim(config.MaxConcurrentRequests, config.MaxConcurrentRequests);
    var tasks = new List<Task>();

    try
    {
      foreach (var country in countries)
      {
        tasks.Add(CollectForCountryWithSemaphore(country, config, semaphore, cancellationToken));
      }

      await Task.WhenAll(tasks);
    }
    finally
    {
      semaphore.Dispose();
    }
  }

  /// <summary>
  /// 使用信号量控制并发的国家信息收集
  /// </summary>
  private async Task CollectForCountryWithSemaphore(
      MarketCode marketCode,
      CollectionConfig config,
      SemaphoreSlim semaphore,
      CancellationToken cancellationToken)
  {
    await semaphore.WaitAsync(cancellationToken);
    try
    {
      await CollectForCountryAsync(marketCode, config.DaysToCollect, config, cancellationToken);
    }
    finally
    {
      semaphore.Release();
    }
  }

  /// <summary>
  /// 收集指定天数的壁纸信息
  /// </summary>
  private async Task CollectWallpaperInfoForDayAsync(
      MarketCode marketCode,
      int dayIndex,
      CollectionConfig config,
      SemaphoreSlim semaphore,
      CancellationToken cancellationToken)
  {
    await semaphore.WaitAsync(cancellationToken);
    try
    {
      var wallpaperInfo = await GetWallpaperInfoAsync(marketCode, dayIndex, config, cancellationToken);
      if (wallpaperInfo != null)
      {
        // 计算实际日期
        var actualDate = DateTime.Now.AddDays(-dayIndex);
        var dateStr = actualDate.ToString("yyyy-MM-dd");

        await _storageService.SaveWallpaperInfoAsync(wallpaperInfo, marketCode, dateStr, config, cancellationToken);
      }
    }
    finally
    {
      semaphore.Release();
    }
  }

  /// <summary>
  /// 从市场代码获取完整语言代码 (用于Accept-Language头)
  /// </summary>
  private static string GetLanguageCodeFromMarket(string marketCode)
  {
    return marketCode switch
    {
      "zh-CN" => "zh-CN",
      "en-US" => "en-US",
      "en-GB" => "en-GB",
      "ja-JP" => "ja-JP",
      "de-DE" => "de-DE",
      "fr-FR" => "fr-FR",
      "es-ES" => "es-ES",
      "it-IT" => "it-IT",
      "ru-RU" => "ru-RU",
      "ko-KR" => "ko-KR",
      "pt-BR" => "pt-BR",
      "en-AU" => "en-AU",
      "en-CA" => "en-CA",
      "en-IN" => "en-IN",
      _ => "en-US" // 默认使用英语
    };
  }

  /// <summary>
  /// 从市场代码获取简化语言代码 (用于API的setlang参数)
  /// </summary>
  private static string GetSimpleLanguageCode(string marketCode)
  {
    return marketCode switch
    {
      "zh-CN" => "zh",
      "en-US" or "en-GB" or "en-AU" or "en-CA" or "en-IN" => "en",
      "ja-JP" => "ja",
      "de-DE" => "de",
      "fr-FR" => "fr",
      "es-ES" => "es",
      "it-IT" => "it",
      "ru-RU" => "ru",
      "ko-KR" => "ko",
      "pt-BR" => "pt",
      _ => "en" // 默认使用英语
    };
  }
}
