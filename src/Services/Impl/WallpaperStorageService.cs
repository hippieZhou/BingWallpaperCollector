using System.Text.Encodings.Web;
using System.Text.Json;
using BingWallpaperCollector.Configuration;
using BingWallpaperCollector.Converters;
using BingWallpaperCollector.Enums;
using BingWallpaperCollector.Extensions;
using BingWallpaperCollector.Models;
using BingWallpaperCollector.Services;
using Microsoft.Extensions.Logging;

namespace BingWallpaperCollector.Services.Impl;

/// <summary>
/// 壁纸信息存储服务实现
/// </summary>
public sealed class WallpaperStorageService : IWallpaperStorageService
{
  private readonly ILogger<WallpaperStorageService> _logger;
  private readonly string _dataDirectory;

  // JSON序列化配置 - 包含自定义转换器
  private static readonly JsonSerializerOptions _jsonOptions = new()
  {
    Converters = { new WallpaperTimeInfoConverter() },
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
  };

  public WallpaperStorageService(ILogger<WallpaperStorageService> logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _dataDirectory = Path.Combine(Environment.CurrentDirectory, AppConstants.DataDirectoryName);
  }

  public async Task SaveWallpaperInfoAsync(
      BingWallpaperInfo wallpaperInfo,
      MarketCode marketCode,
      string dateStr,
      CollectionConfig config,
      CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(wallpaperInfo);
    ArgumentException.ThrowIfNullOrEmpty(dateStr);
    ArgumentNullException.ThrowIfNull(config);

    try
    {
      // 创建存储信息对象
      var storageInfo = new WallpaperInfoStorage
      {
        Date = dateStr,
        Country = marketCode.ToString(),
        MarketCode = marketCode.GetDescription(),
        Title = wallpaperInfo.Title,
        Copyright = wallpaperInfo.Copyright,
        CopyrightLink = wallpaperInfo.CopyrightLink,
        Description = ExtractDescription(wallpaperInfo.Copyright),
        Quiz = wallpaperInfo.Quiz,
        Hash = wallpaperInfo.Hash,
        OriginalUrlBase = wallpaperInfo.UrlBase,
        ImageResolutions = GenerateImageResolutions(wallpaperInfo.UrlBase, marketCode),
        TimeInfo = WallpaperTimeInfo.FromBingApiFields(
              wallpaperInfo.StartDate,
              wallpaperInfo.FullStartDate,
              wallpaperInfo.EndDate),
        CreatedAt = DateTime.Now
      };

      // 创建目录结构：Country/
      var countryDir = Path.Combine(_dataDirectory, marketCode.ToString());
      Directory.CreateDirectory(countryDir);

      // 生成文件路径：使用日期作为文件名
      var fileName = $"{dateStr}.json";
      var filePath = Path.Combine(countryDir, fileName);

      // 检查文件是否已存在
      if (File.Exists(filePath))
      {
        _logger.LogInformation("📋 JSON文件已存在，跳过保存: {Country} - {Date}", marketCode.ToString(), dateStr);
        return;
      }

      // 序列化JSON - 基于配置创建选项
      var options = new JsonSerializerOptions(_jsonOptions)
      {
        WriteIndented = config.PrettyJsonFormat
      };

      var jsonContent = JsonSerializer.Serialize(storageInfo, options);
      await File.WriteAllTextAsync(filePath, jsonContent, cancellationToken);

      _logger.LogInformation("✓ {Country} - {Date} - {Title}",
          marketCode.ToString(),
          dateStr,
          wallpaperInfo.Title.Length > 20 ? wallpaperInfo.Title[..20] + "..." : wallpaperInfo.Title);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "保存 {Country} 壁纸信息时发生错误: {Message}", marketCode.ToString(), ex.Message);
      throw;
    }
  }

  public bool IsWallpaperInfoExists(MarketCode marketCode, string dateStr)
  {
    ArgumentException.ThrowIfNullOrEmpty(dateStr);

    var countryDir = Path.Combine(_dataDirectory, marketCode.ToString());
    var fileName = $"{dateStr}.json";
    var filePath = Path.Combine(countryDir, fileName);
    return File.Exists(filePath);
  }

  public List<ImageResolution> GenerateImageResolutions(string urlBase, MarketCode marketCode)
  {
    ArgumentException.ThrowIfNullOrEmpty(urlBase);

    var resolutions = new List<ImageResolution>();
    var marketDescription = marketCode.GetDescription();

    foreach (var resolution in AppConstants.ImageResolutions)
    {
      var imageUrl = $"{AppConstants.BingBaseUrl}{urlBase}{resolution.Value.Suffix}";

      resolutions.Add(new ImageResolution
      {
        Resolution = resolution.Key,
        Url = imageUrl,
        Size = resolution.Value.Description
      });
    }

    return resolutions;
  }

  public string ExtractDescription(string copyright)
  {
    if (string.IsNullOrEmpty(copyright))
      return string.Empty;

    // 通常版权信息格式为 "描述 (© 版权方)"
    var parts = copyright.Split('(');
    if (parts.Length > 0)
    {
      return parts[0].Trim();
    }

    return copyright;
  }
}
