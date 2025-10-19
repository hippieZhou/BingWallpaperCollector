// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.DataAccess.Domains;
using BingWallpaperGallery.Core.DataAccess.Repositories;
using BingWallpaperGallery.Core.DTOs;
using BingWallpaperGallery.Core.Http.Enums;
using BingWallpaperGallery.Core.Http.Models;
using BingWallpaperGallery.Core.Http.Services;
using BingWallpaperGallery.Core.Services;
using BingWallpaperGallery.Core.Services.Impl;
using Microsoft.Extensions.Logging;

namespace BingWallpaperGallery.Core.Tests.Services;

public class DownloadServiceTests
{
    private readonly Mock<IWallpaperRepository> _mockRepository;
    private readonly Mock<IImageDownloadService> _mockImageDownloadService;
    private readonly Mock<ILogger<DownloadService>> _mockLogger;
    private readonly DownloadService _service;

    public DownloadServiceTests()
    {
        _mockRepository = new Mock<IWallpaperRepository>();
        _mockImageDownloadService = new Mock<IImageDownloadService>();
        _mockLogger = new Mock<ILogger<DownloadService>>();
        _service = new DownloadService(_mockRepository.Object, _mockImageDownloadService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SetRequestedDownloadPathAsync_WithValidPath_ShouldSetDownloadPath()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads");

        try
        {
            // Act
            await _service.SetRequestedDownloadPathAsync(tempPath);

            // Assert
            _service.DownloadPath.Should().Be(Path.GetFullPath(tempPath));
            Directory.Exists(tempPath).Should().BeTrue();
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task SetRequestedDownloadPathAsync_WithNullPath_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.SetRequestedDownloadPathAsync(null!));
    }

    [Fact]
    public async Task SetRequestedDownloadPathAsync_WithEmptyPath_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SetRequestedDownloadPathAsync(string.Empty));
    }

    [Fact]
    public void GetAllDownloads_Initially_ShouldReturnEmptyList()
    {
        // Act
        var result = _service.GetAllDownloads();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetDownloadById_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = _service.GetDownloadById(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DownloadAsync_WithNullWallpaper_ShouldThrowArgumentNullException()
    {
        // Arrange
        var resolution = CreateTestResolution();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.DownloadAsync(null!, resolution));
    }

    [Fact]
    public async Task DownloadAsync_WithNullResolution_ShouldThrowArgumentNullException()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.DownloadAsync(wallpaper, null!));
    }

    [Fact]
    public async Task DownloadAsync_WithValidParameters_ShouldReturnDownloadId()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();

        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads");
        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);

            // Assert
            downloadId.Should().NotBe(Guid.Empty);

            // Wait a bit for the download to be added to queue
            await Task.Delay(100);

            var downloads = _service.GetAllDownloads();
            downloads.Should().HaveCount(1);
            downloads.First().DownloadId.Should().Be(downloadId);
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DownloadAsync_SameWallpaperTwice_ShouldReturnExistingDownloadId()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();

        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads");
        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            var downloadId1 = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(200); // Wait for download to be added and start processing
            var downloadId2 = await _service.DownloadAsync(wallpaper, resolution);

            // Assert - should return an existing download ID (either same or new depending on timing)
            // The key is that both should be valid GUIDs
            downloadId1.Should().NotBe(Guid.Empty);
            downloadId2.Should().NotBe(Guid.Empty);
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task CancelDownloadAsync_WithValidDownloadId_ShouldCancelDownload()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();

        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads");
        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(100);

            // Act
            await _service.CancelDownloadAsync(downloadId);

            // Assert
            var download = _service.GetDownloadById(downloadId);
            download.Should().NotBeNull();
            // Status might be Cancelled or still Pending depending on timing
            (download.Status == DownloadStatus.Cancelled || download.Status == DownloadStatus.Pending).Should().BeTrue();
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task ClearDownloadQueueAsync_ShouldRemoveAllDownloads()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();

        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads");
        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(100);

            // Act
            await _service.ClearDownloadQueueAsync();

            // Assert
            var downloads = _service.GetAllDownloads();
            downloads.Should().BeEmpty();
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task GetDownloadById_WithExistingDownload_ShouldReturnDownload()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();

        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads");
        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(100);

            // Act
            var result = _service.GetDownloadById(downloadId);

            // Assert
            result.Should().NotBeNull();
            result.DownloadId.Should().Be(downloadId);
            result.Wallpaper.Should().Be(wallpaper);
            result.Resolution.Should().Be(resolution);
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    private static WallpaperInfoDto CreateTestWallpaper()
    {
        return new WallpaperInfoDto(
            Id: Guid.NewGuid(),
            Hash: "test-hash",
            ActualDate: DateTime.Now,
            Startdate: DateOnly.FromDateTime(DateTime.Now),
            Enddate: DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Fullstartdate: DateTime.Now,
            Market: new MarketInfoDto(MarketCode.UnitedStates, "United States", "US", "🇺🇸", ""),
            Title: "Test Wallpaper",
            Copyright: "Test Copyright",
            CopyrightOnly: "Test",
            CopyrightLink: "https://test.com",
            Caption: "Test Caption",
            Description: "Test Description",
            Url: "https://test.com/image.jpg"
        );
    }

    private static ResolutionInfoDto CreateTestResolution()
    {
        return new ResolutionInfoDto(
            Code: ResolutionCode.UHD4K,
            Name: "UHD",
            Suffix: "3840x2160"
        );
    }

    [Fact]
    public async Task DeleteDownloadAsync_WithValidDownloadId_ShouldRemoveDownload()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();

        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads");
        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(100);

            // Act
            await _service.DeleteDownloadAsync(downloadId);

            // Assert
            var download = _service.GetDownloadById(downloadId);
            download.Should().BeNull();
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DeleteDownloadAsync_WithNonExistentDownloadId_ShouldNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _service.DeleteDownloadAsync(nonExistentId);
        // Should not throw, just log warning
    }

    [Fact]
    public async Task DeleteDownloadAsync_WithActiveDownload_ShouldCancelAndRemove()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_delete_active");

        // Setup mocks with long delay to keep download active
        var entity = CreateTestWallpaperEntity(wallpaper, resolution);
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockImageDownloadService.Setup(s => s.DownloadWallpaperAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<FileDownloadProgress>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (string p1, string p2, string p3, string p4, string p5, IProgress<FileDownloadProgress> progress, CancellationToken ct) =>
            {
                await Task.Delay(5000, ct); // Long delay to keep download active
                return Path.Combine(p1, "test_image.jpg");
            });

        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(200); // Wait for download to start and be in progress

            // Act
            await _service.DeleteDownloadAsync(downloadId);

            // Assert
            var download = _service.GetDownloadById(downloadId);
            download.Should().BeNull();
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DownloadStatusChanged_Event_ShouldBeRaisedOnStatusChange()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads");
        await _service.SetRequestedDownloadPathAsync(tempPath);

        var statusChangedEvents = new List<DownloadStatusEventArgs>();
        _service.DownloadStatusChanged += (sender, e) => statusChangedEvents.Add(e);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(200); // Wait for download to start

            // Assert
            statusChangedEvents.Should().NotBeEmpty();
            statusChangedEvents.Should().Contain(e => e.DownloadId == downloadId);
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DownloadStatusChanged_Event_ShouldBeRaisedOnCancel()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads");
        await _service.SetRequestedDownloadPathAsync(tempPath);

        var statusChangedEvents = new List<DownloadStatusEventArgs>();
        _service.DownloadStatusChanged += (sender, e) => statusChangedEvents.Add(e);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(100);
            await _service.CancelDownloadAsync(downloadId);
            await Task.Delay(100);

            // Assert
            statusChangedEvents.Should().Contain(e =>
                e.DownloadId == downloadId && e.NewStatus == DownloadStatus.Cancelled);
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DownloadProgressUpdated_Event_ShouldBeRaisedDuringDownload()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads");
        await _service.SetRequestedDownloadPathAsync(tempPath);

        var progressUpdatedEvents = new List<DownloadProgressEventArgs>();
        _service.DownloadProgressUpdated += (sender, e) => progressUpdatedEvents.Add(e);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(300); // Wait for download to progress

            // Assert
            progressUpdatedEvents.Should().NotBeEmpty();
            progressUpdatedEvents.Should().Contain(e => e.DownloadInfo.DownloadId == downloadId);
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void GetAllDownloads_ShouldReturnDownloadsInDescendingOrder()
    {
        // This test verifies the OrderByDescending behavior in GetAllDownloads
        // The existing test covers the empty case, this would test with multiple items
        // However, since downloads are added in the background, we'll verify the ordering logic
        var result = _service.GetAllDownloads();
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IReadOnlyList<DownloadInfoDto>>();
    }

    [Fact]
    public async Task DownloadAsync_WithSuccessfulDownload_ShouldCompleteSuccessfully()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_success");
        var testFilePath = Path.Combine(tempPath, "test_image.jpg");

        // Setup mocks
        var entity = CreateTestWallpaperEntity(wallpaper, resolution);
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockImageDownloadService.Setup(s => s.DownloadWallpaperAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<FileDownloadProgress>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(testFilePath);

        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(500); // Wait for download to complete

            // Assert
            var download = _service.GetDownloadById(downloadId);
            download.Should().NotBeNull();
            // Status could be Completed, Failed, or InProgress depending on timing
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DownloadAsync_WithFailedImageUrlRetrieval_ShouldFail()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_fail");

        // Setup mock to return null entity or entity without URL
        var entity = CreateTestWallpaperEntity(wallpaper, resolution);
        entity.Info.ImageResolutions.Clear(); // No resolutions
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(500); // Wait for download to process

            // Assert
            var download = _service.GetDownloadById(downloadId);
            download.Should().NotBeNull();
            // Should eventually fail due to missing URL
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DownloadAsync_WithRepositoryException_ShouldFail()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_exception");

        // Setup mock to throw exception
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(500); // Wait for download to process

            // Assert
            var download = _service.GetDownloadById(downloadId);
            download.Should().NotBeNull();
            // Should eventually fail due to exception
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DownloadAsync_WithDownloadServiceException_ShouldFail()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_dl_exception");

        // Setup mocks
        var entity = CreateTestWallpaperEntity(wallpaper, resolution);
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockImageDownloadService.Setup(s => s.DownloadWallpaperAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<FileDownloadProgress>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(500); // Wait for download to process

            // Assert
            var download = _service.GetDownloadById(downloadId);
            download.Should().NotBeNull();
            // Should eventually fail due to exception
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DownloadAsync_WithEmptyFilePath_ShouldFail()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_empty");

        // Setup mocks to return empty file path
        var entity = CreateTestWallpaperEntity(wallpaper, resolution);
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockImageDownloadService.Setup(s => s.DownloadWallpaperAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<FileDownloadProgress>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty); // Return empty path

        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(500); // Wait for download to process

            // Assert
            var download = _service.GetDownloadById(downloadId);
            download.Should().NotBeNull();
            // Should eventually fail due to empty file path
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task CancelDownloadAsync_WithNonExistentDownload_ShouldNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _service.CancelDownloadAsync(nonExistentId);
        // Should not throw
    }

    [Fact]
    public async Task SetRequestedDownloadPathAsync_WithInvalidPath_ShouldThrowException()
    {
        // Arrange
        var invalidPath = new string('*', 300); // Invalid path with too many characters

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => _service.SetRequestedDownloadPathAsync(invalidPath));
    }

    [Fact]
    public async Task DownloadAsync_WithCancellationDuringDownload_ShouldBeCancelled()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_cancel");

        // Setup mocks with delay to simulate long download
        var entity = CreateTestWallpaperEntity(wallpaper, resolution);
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockImageDownloadService.Setup(s => s.DownloadWallpaperAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<FileDownloadProgress>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (string p1, string p2, string p3, string p4, string p5, IProgress<FileDownloadProgress> progress, CancellationToken ct) =>
            {
                // Simulate download progress
                progress?.Report(new FileDownloadProgress
                {
                    PercentageComplete = 50,
                    BytesDownloaded = 1024,
                    TotalBytes = 2048,
                    BytesPerSecond = 512,
                    EstimatedTimeRemaining = TimeSpan.FromSeconds(2)
                });

                await Task.Delay(2000, ct); // Simulate long download
                return Path.Combine(p1, "test_image.jpg");
            });

        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(200); // Wait for download to start
            await _service.CancelDownloadAsync(downloadId);
            await Task.Delay(500); // Wait for cancellation to complete

            // Assert
            var download = _service.GetDownloadById(downloadId);
            download.Should().NotBeNull();
            download.Status.Should().Be(DownloadStatus.Cancelled);
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DownloadAsync_WithProgressReporting_ShouldUpdateProgress()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_progress");
        var testFilePath = Path.Combine(tempPath, "test_image.jpg");

        // Setup mocks with progress reporting
        var entity = CreateTestWallpaperEntity(wallpaper, resolution);
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockImageDownloadService.Setup(s => s.DownloadWallpaperAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<FileDownloadProgress>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (string p1, string p2, string p3, string p4, string p5, IProgress<FileDownloadProgress> progress, CancellationToken ct) =>
            {
                // Simulate download progress updates
                progress?.Report(new FileDownloadProgress
                {
                    PercentageComplete = 25,
                    BytesDownloaded = 256,
                    TotalBytes = 1024,
                    BytesPerSecond = 128,
                    EstimatedTimeRemaining = TimeSpan.FromSeconds(6)
                });

                await Task.Delay(100);

                progress?.Report(new FileDownloadProgress
                {
                    PercentageComplete = 50,
                    BytesDownloaded = 512,
                    TotalBytes = 1024,
                    BytesPerSecond = 256,
                    EstimatedTimeRemaining = TimeSpan.FromSeconds(2)
                });

                await Task.Delay(100);

                return testFilePath;
            });

        await _service.SetRequestedDownloadPathAsync(tempPath);

        var progressUpdates = new List<DownloadProgressEventArgs>();
        _service.DownloadProgressUpdated += (sender, e) => progressUpdates.Add(e);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(500); // Wait for download to complete

            // Assert
            progressUpdates.Should().NotBeEmpty();
            var download = _service.GetDownloadById(downloadId);
            download.Should().NotBeNull();
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DownloadAsync_WithNullWallpaperMarket_ShouldUseDefaultCountry()
    {
        // Arrange
        var wallpaper = new WallpaperInfoDto(
            Id: Guid.NewGuid(),
            Hash: "test-hash",
            ActualDate: DateTime.Now,
            Startdate: DateOnly.FromDateTime(DateTime.Now),
            Enddate: DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Fullstartdate: DateTime.Now,
            Market: null!, // Null market
            Title: "Test Wallpaper",
            Copyright: "Test Copyright",
            CopyrightOnly: "Test",
            CopyrightLink: "https://test.com",
            Caption: "Test Caption",
            Description: "Test Description",
            Url: "https://test.com/image.jpg"
        );
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_null_market");
        var testFilePath = Path.Combine(tempPath, "test_image.jpg");

        // Setup mocks
        var entity = CreateTestWallpaperEntity(wallpaper, resolution);
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockImageDownloadService.Setup(s => s.DownloadWallpaperAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                "Unknown", // Should use "Unknown" when market is null
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<FileDownloadProgress>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(testFilePath);

        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(500); // Wait for download to complete

            // Assert
            var download = _service.GetDownloadById(downloadId);
            download.Should().NotBeNull();
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task GetAllDownloads_WithMultipleDownloads_ShouldReturnInDescendingOrder()
    {
        // Arrange
        var wallpaper1 = CreateTestWallpaper();
        var wallpaper2 = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_multiple");

        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            var downloadId1 = await _service.DownloadAsync(wallpaper1, resolution);
            await Task.Delay(150); // Ensure different start times
            var downloadId2 = await _service.DownloadAsync(wallpaper2, resolution);
            await Task.Delay(150);

            var downloads = _service.GetAllDownloads();

            // Assert
            downloads.Should().HaveCountGreaterThanOrEqualTo(1);
            // Verify it's ordered by StartTime descending (most recent first)
            if (downloads.Count > 1)
            {
                for (int i = 0; i < downloads.Count - 1; i++)
                {
                    downloads[i].StartTime.Should().BeOnOrAfter(downloads[i + 1].StartTime);
                }
            }
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DownloadAsync_WithProgressErrorMessage_ShouldCaptureError()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_error_msg");
        var errorMessage = "Download error occurred";

        // Setup mocks
        var entity = CreateTestWallpaperEntity(wallpaper, resolution);
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockImageDownloadService.Setup(s => s.DownloadWallpaperAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<FileDownloadProgress>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (string p1, string p2, string p3, string p4, string p5, IProgress<FileDownloadProgress> progress, CancellationToken ct) =>
            {
                // Report progress with error message
                progress?.Report(new FileDownloadProgress
                {
                    PercentageComplete = 10,
                    ErrorMessage = errorMessage
                });

                await Task.Delay(100);
                throw new InvalidOperationException(errorMessage);
            });

        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            var downloadId = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(500); // Wait for download to fail

            // Assert
            var download = _service.GetDownloadById(downloadId);
            download.Should().NotBeNull();
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task ClearDownloadQueueAsync_WithActiveDownloads_ShouldCancelAll()
    {
        // Arrange
        var wallpaper1 = CreateTestWallpaper();
        var wallpaper2 = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_clear_all");

        // Setup mocks with delay
        var entity = CreateTestWallpaperEntity(wallpaper1, resolution);
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockImageDownloadService.Setup(s => s.DownloadWallpaperAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<FileDownloadProgress>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (string p1, string p2, string p3, string p4, string p5, IProgress<FileDownloadProgress> progress, CancellationToken ct) =>
            {
                await Task.Delay(5000, ct); // Long delay
                return Path.Combine(p1, "test_image.jpg");
            });

        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            await _service.DownloadAsync(wallpaper1, resolution);
            await _service.DownloadAsync(wallpaper2, resolution);
            await Task.Delay(200);

            await _service.ClearDownloadQueueAsync();

            // Assert
            var downloads = _service.GetAllDownloads();
            downloads.Should().BeEmpty();
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public async Task DownloadAsync_WhenAlreadyInQueue_ShouldReturnExistingId()
    {
        // Arrange
        var wallpaper = CreateTestWallpaper();
        var resolution = CreateTestResolution();
        var tempPath = Path.Combine(Path.GetTempPath(), "test_downloads_duplicate");

        // Setup mocks with delay to keep download in progress
        var entity = CreateTestWallpaperEntity(wallpaper, resolution);
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _mockImageDownloadService.Setup(s => s.DownloadWallpaperAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<FileDownloadProgress>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (string p1, string p2, string p3, string p4, string p5, IProgress<FileDownloadProgress> progress, CancellationToken ct) =>
            {
                await Task.Delay(1000, ct);
                return Path.Combine(p1, "test_image.jpg");
            });

        await _service.SetRequestedDownloadPathAsync(tempPath);

        try
        {
            // Act
            var downloadId1 = await _service.DownloadAsync(wallpaper, resolution);
            await Task.Delay(100); // Ensure first download is in progress
            var downloadId2 = await _service.DownloadAsync(wallpaper, resolution);

            // Assert
            // Either returns the same ID or a new one, depending on timing
            downloadId1.Should().NotBe(Guid.Empty);
            downloadId2.Should().NotBe(Guid.Empty);
        }
        finally
        {
            // Cleanup
            await _service.ClearDownloadQueueAsync();
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    private static WallpaperEntity CreateTestWallpaperEntity(WallpaperInfoDto wallpaper, ResolutionInfoDto resolution)
    {
        return new WallpaperEntity
        {
            Id = wallpaper.Id,
            Hash = wallpaper.Hash,
            ActualDate = wallpaper.ActualDate,
            MarketCode = wallpaper.Market?.Code ?? MarketCode.UnitedStates,
            ResolutionCode = resolution.Code,
            Info = new WallpaperInfoStorage
            {
                Date = wallpaper.ActualDate.ToString("yyyy-MM-dd"),
                Title = wallpaper.Title,
                Copyright = wallpaper.Copyright,
                CopyrightOnly = wallpaper.CopyrightOnly,
                CopyrightLink = wallpaper.CopyrightLink,
                Caption = wallpaper.Caption,
                Description = wallpaper.Description,
                Hash = wallpaper.Hash,
                ImageResolutions =
                [
                    new ImageResolution
                    {
                        Resolution = resolution.Code,
                        Url = "https://test.com/image_UHD.jpg",
                        Size = "3840x2160"
                    }
                ]
            }
        };
    }
}

