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
public class BingWallpaperApp
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BingWallpaperApp> _logger;
    private readonly string _dataDirectory;

    // 必应壁纸API地址模板
    private const string BingApiUrlTemplate = "https://www.bing.com/HPImageArchive.aspx?format=js&idx={0}&n={1}&mkt={2}";
    private const string BingBaseUrl = "https://www.bing.com";
    private const int MaxHistoryDays = 8; // Bing API支持的最大历史天数

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
            if (int.TryParse(collectDays, out var days) && days >= 1 && days <= MaxHistoryDays)
            {
                config.DaysToCollect = days;
            }

            // 设置并发请求数
            if (int.TryParse(concurrentRequests, out var concurrent) && concurrent >= 1 && concurrent <= 5)
            {
                config.MaxConcurrentRequests = concurrent;
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
        _logger.LogInformation("{Country} 的壁纸信息收集完成", marketCode.ToString());
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
                _logger.LogDebug("JSON文件已存在，跳过保存: {FilePath}", filePath);
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
        var marketCodeStr = marketCode.GetDescription();

        // 从URL中提取基础信息并重构正确的URL
        var baseUrl = ExtractCorrectImageUrl(urlBase, marketCodeStr);

        // UHD (Ultra High Definition) - 通常是4K
        resolutions.Add(new ImageResolution
        {
            Resolution = "UHD",
            Url = $"{BingBaseUrl}{baseUrl}_UHD.jpg",
            Size = "3840x2160"
        });

        // 4K分辨率
        resolutions.Add(new ImageResolution
        {
            Resolution = "4K",
            Url = $"{BingBaseUrl}{baseUrl}_3840x2160.jpg",
            Size = "3840x2160"
        });

        // 1080p分辨率
        resolutions.Add(new ImageResolution
        {
            Resolution = "1080p",
            Url = $"{BingBaseUrl}{baseUrl}_1920x1080.jpg",
            Size = "1920x1080"
        });

        // 标准分辨率（作为备用）
        resolutions.Add(new ImageResolution
        {
            Resolution = "HD",
            Url = $"{BingBaseUrl}{baseUrl}_1920x1200.jpg",
            Size = "1920x1200"
        });

        return resolutions;
    }

    /// <summary>
    /// 提取并修正图片URL以匹配正确的市场代码
    /// </summary>
    private string ExtractCorrectImageUrl(string urlBase, string marketCode)
    {
        // URL格式通常是：/th?id=OHR.ImageName_ZH-CN1234567890
        // 我们需要将其转换为对应市场的格式

        if (string.IsNullOrEmpty(urlBase))
            return urlBase;

        // 尝试提取图片名称和ID
        var parts = urlBase.Split('_');
        if (parts.Length >= 2)
        {
            // 移除最后一部分的市场代码和数字
            var imageName = parts[0]; // 例如：/th?id=OHR.ImageName

            // 为每个市场代码生成一个随机ID（模拟真实的Bing URL模式）
            var randomId = GenerateRandomId(marketCode);

            // 构建新的URL，使用正确的市场代码
            var upperMarketCode = marketCode.ToUpper().Replace('-', '-');
            return $"{imageName}_{upperMarketCode}{randomId}";
        }

        // 如果无法解析，返回原始URL但尝试替换市场代码
        return urlBase.Replace("ZH-CN", marketCode.ToUpper().Replace('-', '-'));
    }

    /// <summary>
    /// 为指定市场生成一个基于哈希的一致ID
    /// </summary>
    private string GenerateRandomId(string marketCode)
    {
        // 使用市场代码生成一致的ID，确保同一个市场总是得到相同的ID
        var hash = marketCode.GetHashCode();
        var positiveHash = Math.Abs(hash);
        return positiveHash.ToString().PadLeft(10, '0');
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
}
