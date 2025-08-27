using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace BingWallpaperCollector;

/// <summary>
/// 必应壁纸信息类（原始API数据）
/// </summary>
public class BingWallpaperInfo
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("urlbase")]
    public string UrlBase { get; set; } = string.Empty;

    [JsonPropertyName("copyright")]
    public string Copyright { get; set; } = string.Empty;

    [JsonPropertyName("copyrightlink")]
    public string CopyrightLink { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("quiz")]
    public string Quiz { get; set; } = string.Empty;

    [JsonPropertyName("wp")]
    public bool Wp { get; set; }

    [JsonPropertyName("hsh")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("drk")]
    public int Drk { get; set; }

    [JsonPropertyName("top")]
    public int Top { get; set; }

    [JsonPropertyName("bot")]
    public int Bot { get; set; }

    [JsonPropertyName("hs")]
    public object[]? Hs { get; set; }
}

/// <summary>
/// 图片分辨率信息
/// </summary>
public class ImageResolution
{
    public string Resolution { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
}

/// <summary>
/// 完整的壁纸信息存储模型
/// </summary>
public class WallpaperInfoStorage
{
    public string Date { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string MarketCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Copyright { get; set; } = string.Empty;
    public string CopyrightLink { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Quiz { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public List<ImageResolution> ImageResolutions { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string OriginalUrlBase { get; set; } = string.Empty;
}

/// <summary>
/// 必应API响应类
/// </summary>
public class BingApiResponse
{
    [JsonPropertyName("images")]
    public List<BingWallpaperInfo> Images { get; set; } = new();

    [JsonPropertyName("tooltips")]
    public object? Tooltips { get; set; }
}

/// <summary>
/// 支持的国家/地区市场代码
/// </summary>
public enum MarketCode
{
    [System.ComponentModel.Description("zh-CN")]
    China,
    [System.ComponentModel.Description("en-US")]
    UnitedStates,
    [System.ComponentModel.Description("en-GB")]
    UnitedKingdom,
    [System.ComponentModel.Description("ja-JP")]
    Japan,
    [System.ComponentModel.Description("de-DE")]
    Germany,
    [System.ComponentModel.Description("fr-FR")]
    France,
    [System.ComponentModel.Description("es-ES")]
    Spain,
    [System.ComponentModel.Description("it-IT")]
    Italy,
    [System.ComponentModel.Description("ru-RU")]
    Russia,
    [System.ComponentModel.Description("ko-KR")]
    SouthKorea,
    [System.ComponentModel.Description("pt-BR")]
    Brazil,
    [System.ComponentModel.Description("en-AU")]
    Australia,
    [System.ComponentModel.Description("en-CA")]
    Canada,
    [System.ComponentModel.Description("en-IN")]
    India
}

/// <summary>
/// 扩展方法类
/// </summary>
public static class EnumExtensions
{
    public static string GetDescription(this MarketCode marketCode)
    {
        var type = marketCode.GetType();
        var memberInfo = type.GetMember(marketCode.ToString());
        if (memberInfo.Length > 0)
        {
            var attrs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if (attrs.Length > 0)
            {
                return ((System.ComponentModel.DescriptionAttribute)attrs[0]).Description;
            }
        }
        return marketCode.ToString();
    }
}

/// <summary>
/// 收集配置类
/// </summary>
public class CollectionConfig
{
    public MarketCode MarketCode { get; set; } = MarketCode.China;
    public int DaysToCollect { get; set; } = 1;
    public bool CollectAllCountries { get; set; } = false;
    public int MaxConcurrentRequests { get; set; } = 3;
    public bool PrettyJsonFormat { get; set; } = true;
}

/// <summary>
/// 必应壁纸信息收集器主应用类
/// </summary>
public class BingWallpaperApp : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BingWallpaperApp> _logger;
    private readonly string _dataDirectory;

    // 必应壁纸API地址模板
    private const string BingApiUrlTemplate = "https://www.bing.com/HPImageArchive.aspx?format=js&idx={0}&n={1}&mkt={2}";
    private const string BingBaseUrl = "https://www.bing.com";
    private const int MaxHistoryDays = 8; // Bing API支持的最大历史天数

    // 图片下载并发控制信号量
    private static readonly SemaphoreSlim _downloadSemaphore = new(5, 5); // 最多同时下载5张图片

    public BingWallpaperApp(HttpClient httpClient, ILogger<BingWallpaperApp> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _dataDirectory = Path.Combine(Environment.CurrentDirectory, "BingWallpaperData");

        // 确保数据目录存在
        Directory.CreateDirectory(_dataDirectory);
    }

    /// <summary>
    /// 运行应用程序
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            // 获取用户配置
            var config = GetUserConfig();

            _logger.LogInformation("=== 开始收集必应壁纸信息 ===");
            _logger.LogInformation("配置信息:");
            _logger.LogInformation("  - 目标国家: {Country}", config.CollectAllCountries ? "所有支持的国家" : config.MarketCode.ToString());
            _logger.LogInformation("  - 历史天数: {Days} 天", config.DaysToCollect);
            _logger.LogInformation("  - 并发请求: {Concurrent} 个", config.MaxConcurrentRequests);
            _logger.LogInformation("  - JSON格式: {Format}", config.PrettyJsonFormat ? "美化" : "压缩");
            _logger.LogInformation("================================");

            if (config.CollectAllCountries)
            {
                await CollectForAllCountriesAsync(config);
            }
            else
            {
                await CollectForSingleCountryAsync(config);
            }

            _logger.LogInformation("所有壁纸信息收集完成！");
            _logger.LogInformation("数据存储目录: {DataDirectory}", _dataDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "运行过程中发生错误: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// 获取用户配置
    /// </summary>
    private CollectionConfig GetUserConfig()
    {
        var config = new CollectionConfig();

        // 检查是否启用了自动模式（用于GitHub Actions等自动化场景）
        var autoMode = Environment.GetEnvironmentVariable("AUTO_MODE") == "true";

        if (autoMode)
        {
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
            if (int.TryParse(collectDays, out var autoDays) && autoDays >= 1 && autoDays <= MaxHistoryDays)
            {
                config.DaysToCollect = autoDays;
            }

            // 设置并发请求数
            if (int.TryParse(concurrentRequests, out var autoConcurrent) && autoConcurrent >= 1 && autoConcurrent <= 5)
            {
                config.MaxConcurrentRequests = autoConcurrent;
            }

            // 设置JSON格式
            config.PrettyJsonFormat = jsonFormat != "compressed";

            _logger.LogInformation("自动模式配置: 所有国家={AllCountries}, 天数={Days}, 并发={Concurrent}, JSON格式={JsonFormat}",
                config.CollectAllCountries, config.DaysToCollect, config.MaxConcurrentRequests,
                config.PrettyJsonFormat ? "美化" : "压缩");

            return config;
        }

        // 交互模式 - 原有的用户交互逻辑
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
            var countries = Enum.GetValues<MarketCode>().Select((code, index) => new { Index = index + 1, Code = code }).ToList();

            foreach (var country in countries)
            {
                Console.WriteLine($"{country.Index:D2}. {country.Code} ({country.Code.GetDescription()})");
            }

            Console.Write($"请选择国家/地区 (1-{countries.Count}) [默认: 1-中国]: ");
            var countryChoice = Console.ReadLine()?.Trim();

            if (int.TryParse(countryChoice, out var countryIndex) && countryIndex >= 1 && countryIndex <= countries.Count)
            {
                config.MarketCode = countries[countryIndex - 1].Code;
            }
        }

        // 选择历史天数
        Console.Write($"\n请输入要收集的历史天数 (1-{MaxHistoryDays}) [默认: 1]: ");
        var daysInput = Console.ReadLine()?.Trim();
        if (int.TryParse(daysInput, out var days) && days >= 1 && days <= MaxHistoryDays)
        {
            config.DaysToCollect = days;
        }

        // 并发请求数
        Console.Write("请输入并发请求数 (1-5) [默认: 3]: ");
        var concurrentInput = Console.ReadLine()?.Trim();
        if (int.TryParse(concurrentInput, out var concurrent) && concurrent >= 1 && concurrent <= 5)
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

    /// <summary>
    /// 为所有国家收集壁纸信息
    /// </summary>
    private async Task CollectForAllCountriesAsync(CollectionConfig config)
    {
        var countries = Enum.GetValues<MarketCode>();
        var semaphore = new SemaphoreSlim(config.MaxConcurrentRequests, config.MaxConcurrentRequests);
        var tasks = new List<Task>();

        foreach (var country in countries)
        {
            tasks.Add(CollectForCountryWithSemaphore(country, config, semaphore));
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 为单个国家收集壁纸信息
    /// </summary>
    private async Task CollectForSingleCountryAsync(CollectionConfig config)
    {
        await CollectForCountryAsync(config.MarketCode, config.DaysToCollect, config);
    }

    /// <summary>
    /// 使用信号量控制并发的国家信息收集
    /// </summary>
    private async Task CollectForCountryWithSemaphore(MarketCode marketCode, CollectionConfig config, SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        try
        {
            await CollectForCountryAsync(marketCode, config.DaysToCollect, config);
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// 为指定国家收集历史壁纸信息
    /// </summary>
    private async Task CollectForCountryAsync(MarketCode marketCode, int daysToCollect, CollectionConfig config)
    {
        var marketCodeStr = marketCode.GetDescription();
        _logger.LogInformation("开始为 {Country} ({MarketCode}) 收集 {Days} 天的历史壁纸信息...",
            marketCode.ToString(), marketCodeStr, daysToCollect);

        var collectTasks = new List<Task>();
        var semaphore = new SemaphoreSlim(3, 3); // 限制并发数

        for (int dayIndex = 0; dayIndex < daysToCollect; dayIndex++)
        {
            collectTasks.Add(CollectWallpaperInfoForDayAsync(marketCode, dayIndex, config, semaphore));
        }

        await Task.WhenAll(collectTasks);

        // 检查收集结果统计
        var countryDir = Path.Combine(_dataDirectory, marketCode.ToString());
        var fileCount = 0;
        if (Directory.Exists(countryDir))
        {
            fileCount = Directory.GetFiles(countryDir, "*.json", SearchOption.AllDirectories).Length;
        }

        _logger.LogInformation("✅ {Country} 的壁纸信息收集完成 - 共有 {FileCount} 个文件", marketCode.ToString(), fileCount);
    }

    /// <summary>
    /// 收集指定天数的壁纸信息
    /// </summary>
    private async Task CollectWallpaperInfoForDayAsync(MarketCode marketCode, int dayIndex, CollectionConfig config, SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        try
        {
            var wallpaperInfo = await GetWallpaperInfoAsync(marketCode, dayIndex);
            if (wallpaperInfo != null)
            {
                await SaveWallpaperInfoAsync(wallpaperInfo, marketCode, dayIndex, config);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// 获取壁纸信息
    /// </summary>
    private async Task<BingWallpaperInfo?> GetWallpaperInfoAsync(MarketCode marketCode, int dayIndex = 0)
    {
        try
        {
            var marketCodeStr = marketCode.GetDescription();
            var apiUrl = string.Format(BingApiUrlTemplate, dayIndex, 1, marketCodeStr);

            _logger.LogDebug("正在获取 {Country} 第 {Day} 天的壁纸信息...", marketCode.ToString(), dayIndex + 1);

            // 创建带有特定语言头的请求
            using var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

            // 设置Accept-Language头以获取对应语言的内容
            var languageCode = GetLanguageCodeFromMarket(marketCodeStr);
            request.Headers.Add("Accept-Language", $"{languageCode},en;q=0.9");

            // 设置User-Agent以模拟浏览器请求
            request.Headers.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            // 添加其他可能有用的请求头
            request.Headers.Add("Accept", "application/json,text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.Headers.Add("Cache-Control", "no-cache");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<BingApiResponse>(content);

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
    /// 从市场代码获取语言代码
    /// </summary>
    private string GetLanguageCodeFromMarket(string marketCode)
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
    /// 保存壁纸信息为JSON文件
    /// </summary>
    private async Task SaveWallpaperInfoAsync(BingWallpaperInfo wallpaperInfo, MarketCode marketCode, int dayIndex, CollectionConfig config)
    {
        try
        {
            // 计算实际日期
            var actualDate = DateTime.Now.AddDays(-dayIndex);
            var dateStr = actualDate.ToString("yyyy-MM-dd");

            // 创建存储模型
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
                CreatedAt = DateTime.Now
            };

            // 创建目录结构：Country/Date/
            var countryDir = Path.Combine(_dataDirectory, marketCode.ToString());
            var dateDir = Path.Combine(countryDir, dateStr);
            Directory.CreateDirectory(dateDir);

            // 生成文件名
            var fileName = $"wallpaper_info.json";
            var filePath = Path.Combine(dateDir, fileName);

            // 检查文件是否已存在
            if (File.Exists(filePath))
            {
                _logger.LogInformation("📋 JSON文件已存在，跳过保存: {Country} - {Date}", marketCode.ToString(), dateStr);
                return;
            }

            // 序列化JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = config.PrettyJsonFormat,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var jsonContent = JsonSerializer.Serialize(storageInfo, options);
            await File.WriteAllTextAsync(filePath, jsonContent);

            _logger.LogInformation("✓ {Country} - {Date} - {Title}",
                marketCode.ToString(),
                dateStr,
                wallpaperInfo.Title.Length > 20 ? wallpaperInfo.Title.Substring(0, 20) + "..." : wallpaperInfo.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存 {Country} 壁纸信息时发生错误: {Message}", marketCode.ToString(), ex.Message);
        }
    }

    /// <summary>
    /// 从版权信息中提取描述
    /// </summary>
    private string ExtractDescription(string copyright)
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

    /// <summary>
    /// 生成不同分辨率的图片URL
    /// </summary>
    private List<ImageResolution> GenerateImageResolutions(string urlBase, MarketCode marketCode)
    {
        var resolutions = new List<ImageResolution>();

        // 直接使用API返回的真实urlBase
        // API返回的urlBase格式: /th?id=OHR.ImageName_MARKET-CODE123456789
        if (string.IsNullOrEmpty(urlBase))
        {
            _logger.LogWarning("urlBase为空，无法生成图片URL");
            return resolutions;
        }

        // UHD - 最高分辨率（Bing的4K等效格式）
        resolutions.Add(new ImageResolution
        {
            Resolution = "UHD",
            Url = $"{BingBaseUrl}{urlBase}_UHD.jpg",
            Size = "Ultra High Definition (~4K)"
        });

        // HD分辨率 (1920x1200)
        resolutions.Add(new ImageResolution
        {
            Resolution = "HD",
            Url = $"{BingBaseUrl}{urlBase}_1920x1200.jpg",
            Size = "1920x1200"
        });

        // Full HD分辨率 (1920x1080)
        resolutions.Add(new ImageResolution
        {
            Resolution = "Full HD",
            Url = $"{BingBaseUrl}{urlBase}_1920x1080.jpg",
            Size = "1920x1080"
        });

        // 标准分辨率 (1366x768)
        resolutions.Add(new ImageResolution
        {
            Resolution = "Standard",
            Url = $"{BingBaseUrl}{urlBase}_1366x768.jpg",
            Size = "1366x768"
        });

        return resolutions;
    }



    /// <summary>
    /// 获取今日壁纸信息（保持向后兼容）
    /// </summary>
    private async Task<BingWallpaperInfo?> GetTodayWallpaperInfoAsync()
    {
        return await GetWallpaperInfoAsync(MarketCode.China, 0);
    }



    /// <summary>
    /// 获取数据存储目录路径
    /// </summary>
    public string GetDataDirectory() => _dataDirectory;

    /// <summary>
    /// 下载图片到指定目录
    /// </summary>
    /// <param name="imageUrl">图片URL地址</param>
    /// <param name="country">国家代码（用于创建目录结构）</param>
    /// <param name="date">日期（用于创建目录结构）</param>
    /// <param name="resolution">分辨率标识（如UHD、HD等）</param>
    /// <param name="progress">进度汇报回调（可选）</param>
    /// <returns>下载成功后的文件完整路径，失败返回null</returns>
    public async Task<string?> DownloadImageAsync(string imageUrl, string country, string date, string resolution,
        IProgress<FileDownloadProgress>? progress = null)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            _logger.LogWarning("图片URL为空，跳过下载");
            return null;
        }

        // 解析文件扩展名
        var uri = new Uri(imageUrl);
        var fileName = Path.GetFileName(uri.LocalPath);
        if (string.IsNullOrWhiteSpace(fileName) || !Path.HasExtension(fileName))
        {
            // 如果无法从URL获取文件名，使用默认命名
            var extension = imageUrl.ToLower().Contains(".jpg") ? ".jpg" :
                           imageUrl.ToLower().Contains(".png") ? ".png" : ".jpg";
            fileName = $"{resolution}_wallpaper{extension}";
        }

        // 创建进度对象
        var downloadProgress = new FileDownloadProgress
        {
            FileName = fileName,
            Resolution = resolution,
            Status = DownloadStatus.Starting
        };

        // 汇报初始状态
        progress?.Report(downloadProgress);

        await _downloadSemaphore.WaitAsync();
        try
        {
            // 创建目录结构：Country/Date/Images/
            var countryDir = Path.Combine(_dataDirectory, country);
            var dateDir = Path.Combine(countryDir, date);
            var imagesDir = Path.Combine(dateDir, "Images");
            Directory.CreateDirectory(imagesDir);

            // 生成完整的文件路径
            var filePath = Path.Combine(imagesDir, fileName);

            // 检查文件是否已存在
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 0)
                {
                    _logger.LogDebug("📁 图片文件已存在，跳过下载: {FilePath}", filePath);

                    // 汇报跳过状态
                    downloadProgress.Status = DownloadStatus.Skipped;
                    downloadProgress.PercentageComplete = 100.0;
                    downloadProgress.BytesDownloaded = fileInfo.Length;
                    downloadProgress.TotalBytes = fileInfo.Length;
                    progress?.Report(downloadProgress);

                    return filePath;
                }
                else
                {
                    // 如果文件存在但大小为0，删除并重新下载
                    File.Delete(filePath);
                    _logger.LogWarning("🗑️ 删除损坏的图片文件: {FilePath}", filePath);
                }
            }

            _logger.LogInformation("📥 开始下载图片: {Resolution} - {FileName}", resolution, fileName);

            // 更新进度状态为下载中
            downloadProgress.Status = DownloadStatus.Downloading;
            progress?.Report(downloadProgress);

            // 创建HTTP请求
            using var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            request.Headers.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Cache-Control", "no-cache");

            // 发送请求并下载
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"HTTP状态码: {response.StatusCode}";
                _logger.LogError("❌ 下载图片失败，HTTP状态码: {StatusCode}, URL: {ImageUrl}",
                    response.StatusCode, imageUrl);

                // 汇报失败状态
                downloadProgress.Status = DownloadStatus.Failed;
                downloadProgress.ErrorMessage = errorMsg;
                progress?.Report(downloadProgress);

                return null;
            }

            // 检查内容类型
            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (contentType != null && !contentType.StartsWith("image/"))
            {
                _logger.LogWarning("⚠️ 响应内容类型不是图片: {ContentType}, URL: {ImageUrl}",
                    contentType, imageUrl);
                // 但仍然尝试下载，因为有些服务器可能返回错误的Content-Type
            }

            // 获取文件大小
            var contentLength = response.Content.Headers.ContentLength;
            downloadProgress.TotalBytes = contentLength;
            var fileSizeText = contentLength.HasValue ?
                $"{contentLength.Value / 1024.0 / 1024.0:F2} MB" : "未知大小";

            _logger.LogInformation("📊 图片信息: 大小 {FileSize}", fileSizeText);

            // 下载并保存文件（带进度汇报）
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

            await CopyStreamWithProgressAsync(contentStream, fileStream, downloadProgress, progress);
            await fileStream.FlushAsync();

            // 验证下载的文件
            var downloadedFileInfo = new FileInfo(filePath);
            if (downloadedFileInfo.Length == 0)
            {
                File.Delete(filePath);
                _logger.LogError("❌ 下载的文件大小为0，删除文件: {FilePath}", filePath);

                // 汇报失败状态
                downloadProgress.Status = DownloadStatus.Failed;
                downloadProgress.ErrorMessage = "下载的文件大小为0";
                progress?.Report(downloadProgress);

                return null;
            }

            _logger.LogInformation("✅ 图片下载成功: {FilePath} ({FileSize})",
                filePath, $"{downloadedFileInfo.Length / 1024.0 / 1024.0:F2} MB");

            // 汇报完成状态
            downloadProgress.Status = DownloadStatus.Completed;
            downloadProgress.PercentageComplete = 100.0;
            downloadProgress.BytesDownloaded = downloadedFileInfo.Length;
            downloadProgress.TotalBytes = downloadedFileInfo.Length;
            progress?.Report(downloadProgress);

            return filePath;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "🌐 网络请求失败: {Message}, URL: {ImageUrl}", ex.Message, imageUrl);

            // 汇报失败状态
            downloadProgress.Status = DownloadStatus.Failed;
            downloadProgress.ErrorMessage = $"网络请求失败: {ex.Message}";
            progress?.Report(downloadProgress);

            return null;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "⏱️ 下载超时: {Message}, URL: {ImageUrl}", ex.Message, imageUrl);

            // 汇报失败状态
            downloadProgress.Status = DownloadStatus.Failed;
            downloadProgress.ErrorMessage = $"下载超时: {ex.Message}";
            progress?.Report(downloadProgress);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 下载图片时发生未知错误: {Message}, URL: {ImageUrl}", ex.Message, imageUrl);

            // 汇报失败状态
            downloadProgress.Status = DownloadStatus.Failed;
            downloadProgress.ErrorMessage = $"未知错误: {ex.Message}";
            progress?.Report(downloadProgress);

            return null;
        }
        finally
        {
            _downloadSemaphore.Release();
        }
    }

    /// <summary>
    /// 带进度汇报的流复制方法
    /// </summary>
    private async Task CopyStreamWithProgressAsync(Stream source, Stream destination,
        FileDownloadProgress progress, IProgress<FileDownloadProgress>? progressReporter)
    {
        var buffer = new byte[81920]; // 80KB 缓冲区
        var totalBytesRead = 0L;
        var startTime = DateTime.UtcNow;
        var lastReportTime = startTime;

        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;

            var currentTime = DateTime.UtcNow;

            // 更新进度（每100ms汇报一次，避免过于频繁）
            if (currentTime - lastReportTime >= TimeSpan.FromMilliseconds(100))
            {
                progress.BytesDownloaded = totalBytesRead;

                // 计算进度百分比
                if (progress.TotalBytes.HasValue && progress.TotalBytes > 0)
                {
                    progress.PercentageComplete = (double)totalBytesRead / progress.TotalBytes.Value * 100.0;
                }

                // 计算下载速度
                var elapsedTime = currentTime - startTime;
                if (elapsedTime.TotalSeconds > 0)
                {
                    progress.BytesPerSecond = totalBytesRead / elapsedTime.TotalSeconds;

                    // 估算剩余时间
                    if (progress.TotalBytes.HasValue && progress.BytesPerSecond > 0)
                    {
                        var remainingBytes = progress.TotalBytes.Value - totalBytesRead;
                        progress.EstimatedTimeRemaining = TimeSpan.FromSeconds(remainingBytes / progress.BytesPerSecond);
                    }
                }

                progressReporter?.Report(progress);
                lastReportTime = currentTime;
            }
        }

        // 最终状态更新
        progress.BytesDownloaded = totalBytesRead;
        if (progress.TotalBytes.HasValue && progress.TotalBytes > 0)
        {
            progress.PercentageComplete = (double)totalBytesRead / progress.TotalBytes.Value * 100.0;
        }

        var finalElapsedTime = DateTime.UtcNow - startTime;
        if (finalElapsedTime.TotalSeconds > 0)
        {
            progress.BytesPerSecond = totalBytesRead / finalElapsedTime.TotalSeconds;
        }

        progress.EstimatedTimeRemaining = TimeSpan.Zero;
        progressReporter?.Report(progress);
    }

    /// <summary>
    /// 批量下载图片（支持并发）
    /// </summary>
    /// <param name="imageRequests">图片下载请求列表</param>
    /// <param name="batchProgress">批量下载进度汇报（可选）</param>
    /// <returns>下载结果列表（成功下载的文件路径）</returns>
    public async Task<List<string>> DownloadImagesAsync(List<ImageDownloadRequest> imageRequests,
        IProgress<BatchDownloadProgress>? batchProgress = null)
    {
        if (imageRequests == null || !imageRequests.Any())
        {
            _logger.LogWarning("图片下载请求列表为空");
            return new List<string>();
        }

        _logger.LogInformation("🚀 开始批量下载 {Count} 张图片", imageRequests.Count);

        // 创建批量进度对象
        var batchProgressData = new BatchDownloadProgress
        {
            TotalFiles = imageRequests.Count,
            StartTime = DateTime.UtcNow
        };

        batchProgress?.Report(batchProgressData);

        var results = new List<string>();
        var completedCount = 0;
        var failedCount = 0;

        // 创建信号量控制并发数
        var semaphore = new SemaphoreSlim(Math.Min(imageRequests.Count, 5), Math.Min(imageRequests.Count, 5));
        var tasks = new List<Task>();

        foreach (var request in imageRequests)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // 为每个文件创建进度汇报
                    var fileProgressReporter = batchProgress != null ? new Progress<FileDownloadProgress>(fileProgress =>
                    {
                        batchProgressData.CurrentFileProgress = fileProgress;
                        batchProgressData.ElapsedTime = DateTime.UtcNow - batchProgressData.StartTime;
                        batchProgress.Report(batchProgressData);
                    }) : null;

                    var filePath = await DownloadImageAsync(request.ImageUrl, request.Country, request.Date,
                        request.Resolution, fileProgressReporter);

                    lock (results)
                    {
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            results.Add(filePath);
                            completedCount++;
                        }
                        else
                        {
                            failedCount++;
                        }

                        // 更新批量进度
                        batchProgressData.CompletedFiles = completedCount;
                        batchProgressData.FailedFiles = failedCount;
                        batchProgressData.OverallPercentage = (double)(completedCount + failedCount) / imageRequests.Count * 100.0;
                        batchProgressData.ElapsedTime = DateTime.UtcNow - batchProgressData.StartTime;

                        batchProgress?.Report(batchProgressData);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);

        // 最终状态汇报
        batchProgressData.CompletedFiles = completedCount;
        batchProgressData.FailedFiles = failedCount;
        batchProgressData.OverallPercentage = 100.0;
        batchProgressData.ElapsedTime = DateTime.UtcNow - batchProgressData.StartTime;
        batchProgressData.CurrentFileProgress = null; // 清除当前文件进度
        batchProgress?.Report(batchProgressData);

        _logger.LogInformation("📊 批量下载完成: 成功 {SuccessCount} 张，失败 {FailedCount} 张，用时 {ElapsedTime}",
            completedCount, failedCount, batchProgressData.ElapsedTime);

        return results;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _downloadSemaphore?.Dispose();
    }
}

/// <summary>
/// 图片下载请求信息
/// </summary>
public class ImageDownloadRequest
{
    public string ImageUrl { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
}

/// <summary>
/// 单个文件下载进度信息
/// </summary>
public class FileDownloadProgress
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 分辨率标识
    /// </summary>
    public string Resolution { get; set; } = string.Empty;

    /// <summary>
    /// 下载进度百分比 (0-100)
    /// </summary>
    public double PercentageComplete { get; set; }

    /// <summary>
    /// 已下载字节数
    /// </summary>
    public long BytesDownloaded { get; set; }

    /// <summary>
    /// 文件总大小（如果已知）
    /// </summary>
    public long? TotalBytes { get; set; }

    /// <summary>
    /// 下载速度 (字节/秒)
    /// </summary>
    public double BytesPerSecond { get; set; }

    /// <summary>
    /// 剩余时间估算
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// 下载状态
    /// </summary>
    public DownloadStatus Status { get; set; } = DownloadStatus.Starting;

    /// <summary>
    /// 错误信息（如果有）
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 批量下载进度信息
/// </summary>
public class BatchDownloadProgress
{
    /// <summary>
    /// 总文件数
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// 已完成的文件数
    /// </summary>
    public int CompletedFiles { get; set; }

    /// <summary>
    /// 失败的文件数
    /// </summary>
    public int FailedFiles { get; set; }

    /// <summary>
    /// 整体进度百分比 (0-100)
    /// </summary>
    public double OverallPercentage { get; set; }

    /// <summary>
    /// 当前正在下载的文件进度
    /// </summary>
    public FileDownloadProgress? CurrentFileProgress { get; set; }

    /// <summary>
    /// 总下载速度 (字节/秒)
    /// </summary>
    public double TotalBytesPerSecond { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 已用时间
    /// </summary>
    public TimeSpan ElapsedTime { get; set; }
}

/// <summary>
/// 下载状态枚举
/// </summary>
public enum DownloadStatus
{
    /// <summary>
    /// 准备开始
    /// </summary>
    Starting,

    /// <summary>
    /// 正在下载
    /// </summary>
    Downloading,

    /// <summary>
    /// 下载完成
    /// </summary>
    Completed,

    /// <summary>
    /// 下载失败
    /// </summary>
    Failed,

    /// <summary>
    /// 已跳过（文件已存在）
    /// </summary>
    Skipped
}
