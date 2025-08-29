using BingWallpaperCollector.Configuration;
using BingWallpaperCollector.Enums;
using BingWallpaperCollector.Extensions;
using BingWallpaperCollector.Services;
using Microsoft.Extensions.Logging;

namespace BingWallpaperCollector.Services.Impl;

/// <summary>
/// 用户配置服务实现
/// </summary>
public sealed class UserConfigurationService : IUserConfigurationService
{
  private readonly ILogger<UserConfigurationService> _logger;

  public UserConfigurationService(ILogger<UserConfigurationService> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public CollectionConfig GetUserConfig()
  {
    if (IsAutoMode())
    {
      return GetAutomationConfig();
    }

    return GetInteractiveConfig();
  }

  public CollectionConfig GetAutomationConfig()
  {
    var config = new CollectionConfig();

    _logger.LogInformation("检测到自动模式，使用环境变量配置");

    // 从环境变量读取配置
    var collectAllCountries = Environment.GetEnvironmentVariable("COLLECT_ALL_COUNTRIES") == "true";
    var collectDays = Environment.GetEnvironmentVariable("COLLECT_DAYS");
    var concurrentRequests = Environment.GetEnvironmentVariable("CONCURRENT_REQUESTS");
    var jsonFormat = Environment.GetEnvironmentVariable("JSON_FORMAT");
    var targetCountry = Environment.GetEnvironmentVariable("TARGET_COUNTRY");

    config.CollectAllCountries = collectAllCountries;

    // 如果不是收集所有国家，尝试设置目标国家
    if (!collectAllCountries && !string.IsNullOrEmpty(targetCountry))
    {
      if (Enum.TryParse<MarketCode>(targetCountry, true, out var marketCode))
      {
        config.MarketCode = marketCode;
      }
    }

    // 设置收集天数
    if (int.TryParse(collectDays, out var autoDays) &&
        autoDays >= 1 && autoDays <= AppConstants.MaxHistoryDays)
    {
      config.DaysToCollect = autoDays;
    }

    // 设置并发请求数
    if (int.TryParse(concurrentRequests, out var autoConcurrent) &&
        autoConcurrent >= 1 && autoConcurrent <= 5)
    {
      config.MaxConcurrentRequests = autoConcurrent;
    }

    // 设置JSON格式
    config.PrettyJsonFormat = jsonFormat != "compressed";

    _logger.LogInformation(
        "自动模式配置: 所有国家={AllCountries}, 天数={Days}, 并发={Concurrent}, JSON格式={JsonFormat}",
        config.CollectAllCountries,
        config.DaysToCollect,
        config.MaxConcurrentRequests,
        config.PrettyJsonFormat ? "美化" : "压缩");

    return config;
  }

  public bool IsAutoMode()
  {
    var autoMode = Environment.GetEnvironmentVariable("AUTO_MODE") == "true";
    _logger.LogInformation(
        "🔍 检查自动模式: AUTO_MODE={AutoModeValue}, 结果={IsAutoMode}",
        Environment.GetEnvironmentVariable("AUTO_MODE"),
        autoMode);
    return autoMode;
  }

  private CollectionConfig GetInteractiveConfig()
  {
    var config = new CollectionConfig();

    _logger.LogInformation("⚠️ 进入交互模式");
    Console.WriteLine("\n=== 必应壁纸信息收集器配置 ===");

    // 选择国家
    Console.WriteLine("\n请选择收集模式:");
    Console.WriteLine("1. 单个国家/地区");
    Console.WriteLine("2. 所有支持的国家/地区");
    Console.Write("请输入选择 (1-2) [默认: 1]: ");

    var modeChoice = Console.ReadLine()?.Trim();
    if (modeChoice == "2")
    {
      config.CollectAllCountries = true;
    }
    else
    {
      // 显示支持的国家列表
      Console.WriteLine("\n支持的国家/地区:");
      var countries = Enum.GetValues<MarketCode>()
          .Select((code, index) => new { Index = index + 1, Code = code })
          .ToList();

      foreach (var country in countries)
      {
        Console.WriteLine($"{country.Index:D2}. {country.Code} ({country.Code.GetDescription()})");
      }

      Console.Write($"请选择国家/地区 (1-{countries.Count}) [默认: 1-中国]: ");
      var countryChoice = Console.ReadLine()?.Trim();

      if (int.TryParse(countryChoice, out var countryIndex) &&
          countryIndex >= 1 && countryIndex <= countries.Count)
      {
        config.MarketCode = countries[countryIndex - 1].Code;
      }
    }

    // 选择历史天数
    Console.Write($"\n请输入要收集的历史天数 (1-{AppConstants.MaxHistoryDays}) [默认: 1]: ");
    var daysInput = Console.ReadLine()?.Trim();
    if (int.TryParse(daysInput, out var days) &&
        days >= 1 && days <= AppConstants.MaxHistoryDays)
    {
      config.DaysToCollect = days;
    }

    // 并发请求数
    Console.Write("请输入并发请求数 (1-5) [默认: 3]: ");
    var concurrentInput = Console.ReadLine()?.Trim();
    if (int.TryParse(concurrentInput, out var concurrent) &&
        concurrent >= 1 && concurrent <= 5)
    {
      config.MaxConcurrentRequests = concurrent;
    }

    // JSON格式选择
    Console.WriteLine("\n请选择JSON格式:");
    Console.WriteLine("1. 美化格式（易读）");
    Console.WriteLine("2. 压缩格式（占用空间小）");
    Console.Write("请输入选择 (1-2) [默认: 1]: ");
    var formatChoice = Console.ReadLine()?.Trim();
    if (formatChoice == "2")
    {
      config.PrettyJsonFormat = false;
    }

    return config;
  }
}
