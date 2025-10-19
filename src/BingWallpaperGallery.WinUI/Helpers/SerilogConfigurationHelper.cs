// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.WinUI.Options;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace BingWallpaperGallery.WinUI.Helpers;

/// <summary>
/// Serilog 日志配置辅助类
/// </summary>
public static class SerilogConfigurationHelper
{
    /// <summary>
    /// 配置 Serilog 日志记录器
    /// </summary>
    /// <param name="logBaseDir">日志基础目录</param>
    /// <param name="configuration">配置对象</param>
    /// <param name="applicationName">应用程序名称</param>
    /// <returns>配置好的日志记录器</returns>
    public static ILogger ConfigureLogger(
        string logBaseDir,
        IConfiguration configuration = null,
        string applicationName = "BingWallpaperGallery")
    {
        // 从配置文件读取日志选项
        var options = new LoggingOptions();
        configuration?.GetSection(nameof(LoggingOptions)).Bind(options);

        // 确保日志目录存在
        Directory.CreateDirectory(logBaseDir);

        // 解析最小日志级别
        var minimumLevel = Enum.TryParse<LogEventLevel>(options.MinimumLevel, true, out var level)
            ? level
            : LogEventLevel.Verbose;

        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .MinimumLevel.Is(minimumLevel)
            // 过滤第三方库的日志
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning);

        // 调试输出（可选）
        if (options.EnableDebugOutput)
        {
            loggerConfig.WriteTo.Debug(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: minimumLevel
            );
        }

        // 主日志文件（包含所有级别）
        loggerConfig.WriteTo.Map(
            keyPropertyName: "UtcDateTime",
            defaultKey: DateTime.UtcNow.ToString("yyyyMMdd"),
            configure: (date, writeTo) =>
            {
                var dir = Path.Combine(logBaseDir, date);
                Directory.CreateDirectory(dir);

                writeTo.File(
                    Path.Combine(dir, "app.log"),
                    restrictedToMinimumLevel: minimumLevel,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Infinite,
                    fileSizeLimitBytes: options.FileSizeLimitBytes,
                    retainedFileCountLimit: options.RetainedFileCountLimit,
                    rollOnFileSizeLimit: true
                );
            });

        // 错误日志文件（仅 Error 和 Fatal）
        loggerConfig.WriteTo.Map(
            keyPropertyName: "UtcDateTime",
            defaultKey: DateTime.UtcNow.ToString("yyyyMMdd"),
            configure: (date, writeTo) =>
            {
                var dir = Path.Combine(logBaseDir, date);
                Directory.CreateDirectory(dir);

                writeTo.File(
                    Path.Combine(dir, "error.log"),
                    restrictedToMinimumLevel: LogEventLevel.Error,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Infinite,
                    fileSizeLimitBytes: options.FileSizeLimitBytes,
                    retainedFileCountLimit: options.RetainedFileCountLimit,
                    rollOnFileSizeLimit: true
                );
            });

        // 结构化日志（JSON格式，可选）
        if (options.EnableStructuredLogging)
        {
            loggerConfig.WriteTo.Map(
                keyPropertyName: "UtcDateTime",
                defaultKey: DateTime.UtcNow.ToString("yyyyMMdd"),
                configure: (date, writeTo) =>
                {
                    var dir = Path.Combine(logBaseDir, date);
                    Directory.CreateDirectory(dir);

                    writeTo.File(
                        new CompactJsonFormatter(),
                        Path.Combine(dir, "structured.json"),
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        rollingInterval: RollingInterval.Infinite,
                        fileSizeLimitBytes: options.FileSizeLimitBytes,
                        retainedFileCountLimit: options.RetainedFileCountLimit,
                        rollOnFileSizeLimit: true
                    );
                });
        }

        Log.Logger = loggerConfig.CreateLogger();

        return Log.Logger;
    }
}
