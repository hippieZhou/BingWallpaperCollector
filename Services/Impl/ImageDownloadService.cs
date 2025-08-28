using BingWallpaperCollector.Configuration;
using BingWallpaperCollector.Enums;
using BingWallpaperCollector.Models;
using BingWallpaperCollector.Services;
using Microsoft.Extensions.Logging;

namespace BingWallpaperCollector.Services.Impl;

/// <summary>
/// 图片下载服务实现
/// </summary>
public sealed class ImageDownloadService : IImageDownloadService, IDisposable
{
  private readonly HttpClient _httpClient;
  private readonly ILogger<ImageDownloadService> _logger;
  private readonly string _dataDirectory;

  // 图片下载并发控制信号量
  private readonly SemaphoreSlim _downloadSemaphore = new(AppConstants.MaxConcurrentDownloads, AppConstants.MaxConcurrentDownloads);

  public ImageDownloadService(HttpClient httpClient, ILogger<ImageDownloadService> logger)
  {
    _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _dataDirectory = Path.Combine(Environment.CurrentDirectory, AppConstants.DataDirectoryName);
  }

  public async Task<string?> DownloadImageAsync(
      string imageUrl,
      string country,
      string date,
      string resolution,
      IProgress<FileDownloadProgress>? progress = null,
      CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(imageUrl))
    {
      _logger.LogWarning("图片URL为空，跳过下载");
      return null;
    }

    ArgumentException.ThrowIfNullOrEmpty(country);
    ArgumentException.ThrowIfNullOrEmpty(date);
    ArgumentException.ThrowIfNullOrEmpty(resolution);

    // 解析文件扩展名
    var uri = new Uri(imageUrl);
    var fileName = Path.GetFileName(uri.LocalPath);
    if (string.IsNullOrWhiteSpace(fileName) || !Path.HasExtension(fileName))
    {
      // 如果无法从URL获取文件名，使用默认命名
      var extension = imageUrl.ToLowerInvariant().Contains(".jpg") ? ".jpg" :
                     imageUrl.ToLowerInvariant().Contains(".png") ? ".png" : ".jpg";
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

    await _downloadSemaphore.WaitAsync(cancellationToken);
    try
    {
      // 创建目录结构：Country/Date/Images/
      var countryDir = Path.Combine(_dataDirectory, country);
      var dateDir = Path.Combine(countryDir, date);
      var imagesDir = Path.Combine(dateDir, AppConstants.ImagesSubDirectoryName);
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
      request.Headers.Add("User-Agent", AppConstants.HttpHeaders.UserAgent);
      request.Headers.Add("Accept", AppConstants.HttpHeaders.AcceptImage);
      request.Headers.Add("Accept-Encoding", AppConstants.HttpHeaders.AcceptEncoding);
      request.Headers.Add("Cache-Control", AppConstants.HttpHeaders.CacheControl);

      // 发送请求并下载
      using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

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
      using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
      using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

      await CopyStreamWithProgressAsync(contentStream, fileStream, downloadProgress, progress, cancellationToken);
      await fileStream.FlushAsync(cancellationToken);

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

  public async Task<List<string>> DownloadImagesAsync(
      List<ImageDownloadRequest> imageRequests,
      IProgress<BatchDownloadProgress>? batchProgress = null,
      CancellationToken cancellationToken = default)
  {
    if (imageRequests == null || imageRequests.Count == 0)
    {
      _logger.LogWarning("图片下载请求列表为空");
      return [];
    }

    _logger.LogInformation("🚀 开始批量下载 {Count} 张图片", imageRequests.Count);

    var results = new List<string>();
    var completedCount = 0;
    var failedCount = 0;
    var startTime = DateTime.UtcNow;

    // 创建批量进度对象
    var batchProgressData = new BatchDownloadProgress
    {
      TotalFiles = imageRequests.Count,
      StartTime = startTime
    };

    var tasks = imageRequests.Select(async request =>
    {
      // 为每个文件创建进度汇报器
      var fileProgressReporter = batchProgress != null ? new Progress<FileDownloadProgress>(fileProgress =>
          {
          lock (batchProgressData)
          {
            batchProgressData.CurrentFileProgress = fileProgress;
            batchProgressData.ElapsedTime = DateTime.UtcNow - startTime;
            batchProgress.Report(batchProgressData);
          }
        }) : null;

      // 下载单个文件
      var filePath = await DownloadImageAsync(request.ImageUrl, request.Country, request.Date,
              request.Resolution, fileProgressReporter, cancellationToken);

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
        if (batchProgress != null)
        {
          batchProgressData.CompletedFiles = completedCount;
          batchProgressData.FailedFiles = failedCount;
          batchProgressData.OverallPercentage = (double)(completedCount + failedCount) / imageRequests.Count * 100.0;
          batchProgressData.ElapsedTime = DateTime.UtcNow - startTime;
          batchProgress.Report(batchProgressData);
        }
      }
    });

    await Task.WhenAll(tasks);

    _logger.LogInformation("📊 批量下载完成: 成功 {Success} 张，失败 {Failed} 张，总用时 {ElapsedTime}",
        completedCount, failedCount, DateTime.UtcNow - startTime);

    return results;
  }

  /// <summary>
  /// 带进度汇报的流复制方法
  /// </summary>
  private static async Task CopyStreamWithProgressAsync(
      Stream source,
      Stream destination,
      FileDownloadProgress progress,
      IProgress<FileDownloadProgress>? progressReporter,
      CancellationToken cancellationToken = default)
  {
    var buffer = new byte[81920]; // 80KB 缓冲区
    var totalBytesRead = 0L;
    var startTime = DateTime.UtcNow;
    var lastReportTime = startTime;

    int bytesRead;
    while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
    {
      await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
      totalBytesRead += bytesRead;

      var currentTime = DateTime.UtcNow;

      // 更新进度（每100ms汇报一次，避免过于频繁）
      if (currentTime - lastReportTime >= TimeSpan.FromMilliseconds(AppConstants.ProgressReportIntervalMs))
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

  public void Dispose()
  {
    _downloadSemaphore?.Dispose();
  }
}
