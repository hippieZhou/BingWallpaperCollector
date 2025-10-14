using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BingWallpaperCollector;
using BingWallpaperCollector.Configuration;
using BingWallpaperCollector.Services;
using BingWallpaperCollector.Services.Impl;

// 创建服务容器
var services = new ServiceCollection();

// 配置HttpClient
services.AddHttpClient<IBingWallpaperService, BingWallpaperService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(AppConstants.HttpTimeoutSeconds);
    client.DefaultRequestHeaders.Add("User-Agent", AppConstants.HttpHeaders.UserAgent);
});

// 配置日志
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// 注册服务
services.AddSingleton<IUserConfigurationService, UserConfigurationService>();
services.AddSingleton<IWallpaperStorageService, WallpaperStorageService>();
services.AddTransient<IBingWallpaperService, BingWallpaperService>();

// 注册应用服务
services.AddTransient<BingWallpaperApp>();

// 构建服务提供者
using var serviceProvider = services.BuildServiceProvider();

// 获取日志器
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("=== 必应壁纸信息收集器启动 ===");
    logger.LogInformation("程序版本: 2.2.0");
    logger.LogInformation("目标框架: .NET 9.0");
    logger.LogInformation("启动时间: {StartTime}", DateTime.Now);
    logger.LogInformation("=============================");

    // 获取应用实例并运行
    var app = serviceProvider.GetRequiredService<BingWallpaperApp>();
    await app.RunAsync();

    logger.LogInformation("程序执行完成，按任意键退出...");

    // 等待用户输入以便查看结果（检测是否为重定向输入）
    if (Console.IsInputRedirected)
    {
        logger.LogInformation("检测到输入重定向，自动退出");
    }
    else
    {
        Console.ReadKey();
    }
}
catch (Exception ex)
{
    logger.LogCritical(ex, "程序发生致命错误: {Message}", ex.Message);
    Console.WriteLine("\n程序异常退出，按任意键关闭...");
    if (!Console.IsInputRedirected)
    {
        Console.ReadKey();
    }
    Environment.Exit(1);
}
finally
{
    logger.LogInformation("程序退出");
}
