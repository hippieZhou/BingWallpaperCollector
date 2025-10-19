// Copyright (c) hippieZhou. All rights reserved.

using System.Diagnostics;
using BingWallpaperGallery.Core.DataAccess.Repositories;
using BingWallpaperGallery.Core.DTOs;
using BingWallpaperGallery.Core.Http.Configuration;
using BingWallpaperGallery.Core.Http.Models;
using BingWallpaperGallery.Core.Http.Services;
using BingWallpaperGallery.Core.Mappers;
using Microsoft.Extensions.Logging;

namespace BingWallpaperGallery.Core.Services.Impl;

/// <summary>
/// 必应壁纸管理服务实现
/// 提供壁纸收集、查询和统计功能
/// </summary>
public class ManagementService(
    IBingWallpaperService bingWallpaperService,
    IWallpaperRepository wallpaperRepository,
    ILogger<ManagementService> logger) : IManagementService
{
    private readonly IBingWallpaperService _wallpaperService = bingWallpaperService;
    private readonly IWallpaperRepository _wallpaperRepository = wallpaperRepository;
    private readonly ILogger<ManagementService> _logger = logger;

    public async Task RunCollectionAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("开始执行壁纸收集命令...");

            // 运行壁纸收集服务
            var result = await _wallpaperService.CollectAsync(cancellationToken);

            // 处理收集到的壁纸信息，存储到数据库
            if (result.CollectedWallpapers.Any())
            {
                await ProcessCollectedWallpapersAsync(result.CollectedWallpapers, cancellationToken);
            }

            stopwatch.Stop();

            _logger.LogInformation("壁纸收集命令执行完成: 总计 {Total}, 耗时 {Duration}ms", result.TotalCollected, stopwatch.ElapsedMilliseconds);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "应用程序运行失败: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<WallpaperInfoDto> GetLatestAsync(MarketInfoDto market, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(market);
        var marketCode = market.Code;
        try
        {
            _logger.LogInformation("正在获取 {MarketCode} 的最新壁纸信息", marketCode);
            var wallpaperEntities = await _wallpaperRepository.GetLatestAsync(
                marketCode, 1, cancellationToken);
            if (wallpaperEntities.Any() == false)
            {
                _logger.LogWarning("未找到 {MarketCode} 的最新壁纸信息", marketCode);
                return null;
            }

            _logger.LogInformation("成功获取 {MarketCode} 的最新壁纸信息", marketCode);
            return WallpaperMapper.MapToDto(wallpaperEntities.First());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 {MarketCode} 最新壁纸信息时发生错误: {Message}",
                marketCode, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<WallpaperInfoDto>> GetByMarketCodeAsync(
        MarketInfoDto market,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber, nameof(pageNumber));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var marketCode = market.Code;

        try
        {
            _logger.LogInformation("正在获取 {MarketCode} 的壁纸信息，页码: {PageNumber}，页大小: {PageSize}",
                marketCode, pageNumber, pageSize);

            var wallpaperEntities = await _wallpaperRepository.GetByMarketCodeAsync(
                marketCode, pageNumber, pageSize, cancellationToken);

            _logger.LogInformation("成功获取 {MarketCode} 的 {Count} 条壁纸信息", marketCode, wallpaperEntities.Count());

            return wallpaperEntities.Select(WallpaperMapper.MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 {MarketCode} 壁纸信息时发生错误: {Message}", marketCode, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<MarketInfoDto>> GetSupportedMarketCodesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("获取所有支持的市场代码");

            // 获取所有MarketCode枚举值
            var marketCodes = HTTPConstants.GetSupportedMarketCodes();

            _logger.LogInformation("成功获取 {Count} 个市场代码", marketCodes.Length);

            return await Task.FromResult(marketCodes.Select(WallpaperMapper.MapToMarketDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取市场代码时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<ResolutionInfoDto>> GetSupportedResolutionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("获取所有支持的壁纸分辨率");

            // 获取所有ResolutionCode枚举值
            var resolutionCodes = HTTPConstants.GetSupportedResolutions();

            _logger.LogInformation("成功获取 {Count} 个分辨率代码", resolutionCodes.Length);

            return await Task.FromResult(resolutionCodes.Select(WallpaperMapper.ResolutionInfoDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取壁纸分辨率时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<string> GetMoreDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _wallpaperRepository.GetAsync(id, cancellationToken);
        return entity is not null ? entity.InfoJson : string.Empty;
    }

    /// <summary>
    /// 处理收集到的壁纸信息，存储到数据库
    /// </summary>
    /// <param name="collectedWallpapers">收集到的壁纸信息列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    private async Task ProcessCollectedWallpapersAsync(
        IEnumerable<CollectedWallpaperInfo> collectedWallpapers,
        CancellationToken cancellationToken = default)
    {
        if (collectedWallpapers.Any() == false)
        {
            _logger.LogInformation("没有成功的壁纸需要存储");
            return;
        }

        _logger.LogInformation("开始处理 {Count} 个成功收集的壁纸信息", collectedWallpapers.Count());

        var savedCount = 0;
        var skippedCount = 0;
        var errorCount = 0;

        foreach (var collectedWallpaper in collectedWallpapers)
        {
            try
            {
                // 验证壁纸信息
                var validationResult = TryValidateWallpaperInfo(collectedWallpaper.WallpaperInfo!, out var errorMessage);
                if (!validationResult)
                {
                    _logger.LogWarning("壁纸信息验证失败: {MarketCode} - {Date} - {Error}",
                        collectedWallpaper.MarketCode, collectedWallpaper.CollectionDate.Date, errorMessage);
                    errorCount++;
                    continue;
                }

                // 保存到数据库（使用原子性保存，避免重复）
                var wallpaperEntity = WallpaperMapper.MapToEntity(
                    collectedWallpaper.WallpaperInfo,
                    collectedWallpaper.MarketCode,
                    collectedWallpaper.CollectionDate.DateTime);

                var saved = await _wallpaperRepository.SaveIfNotExistsAsync(wallpaperEntity, cancellationToken);

                if (saved)
                {
                    savedCount++;
                    _logger.LogDebug("成功保存壁纸: {MarketCode} - {Title} - {Date}",
                        collectedWallpaper.MarketCode, wallpaperEntity.Info.Title, collectedWallpaper.CollectionDate.Date);
                }
                else
                {
                    skippedCount++;
                    _logger.LogDebug("壁纸已存在，跳过保存: {MarketCode} - {Title} - {Date}",
                        collectedWallpaper.MarketCode, wallpaperEntity.Info.Title, collectedWallpaper.CollectionDate.Date);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理壁纸信息时发生错误: {MarketCode} - {Date}",
                    collectedWallpaper.MarketCode, collectedWallpaper.CollectionDate.Date);
                errorCount++;
            }
        }

        _logger.LogInformation("壁纸处理完成: 保存 {Saved} 个, 跳过 {Skipped} 个, 错误 {Error} 个", savedCount, skippedCount, errorCount);
    }

    private static bool TryValidateWallpaperInfo(BingWallpaperInfo wallpaperInfo, out string errorMessage)
    {
        if (wallpaperInfo == null)
        {
            errorMessage = "壁纸信息不能为空";
            return false;
        }

        if (string.IsNullOrWhiteSpace(wallpaperInfo.UrlBase))
        {
            errorMessage = "壁纸URL基础路径不能为空";
            return false;
        }

        if (string.IsNullOrWhiteSpace(wallpaperInfo.Title))
        {
            errorMessage = "壁纸标题不能为空";
            return false;
        }

        if (wallpaperInfo.StartDate == default)
        {
            errorMessage = "壁纸开始日期无效";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
