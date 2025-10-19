// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.DataAccess.Domains;
using BingWallpaperGallery.Core.DataAccess.Repositories;
using BingWallpaperGallery.Core.DTOs;
using BingWallpaperGallery.Core.Http.Enums;
using BingWallpaperGallery.Core.Http.Models;
using BingWallpaperGallery.Core.Http.Services;
using BingWallpaperGallery.Core.Services.Impl;
using Microsoft.Extensions.Logging;

namespace BingWallpaperGallery.Core.Tests.Services;

public class ManagementServiceTests
{
    private readonly Mock<IBingWallpaperService> _mockBingWallpaperService;
    private readonly Mock<IWallpaperRepository> _mockRepository;
    private readonly Mock<ILogger<ManagementService>> _mockLogger;
    private readonly ManagementService _service;

    public ManagementServiceTests()
    {
        _mockBingWallpaperService = new Mock<IBingWallpaperService>();
        _mockRepository = new Mock<IWallpaperRepository>();
        _mockLogger = new Mock<ILogger<ManagementService>>();
        _service = new ManagementService(
            _mockBingWallpaperService.Object,
            _mockRepository.Object,
            _mockLogger.Object);
    }

    #region RunCollectionAsync Tests

    [Fact]
    public async Task RunCollectionAsync_WithSuccessfulCollection_ShouldProcessWallpapers()
    {
        // Arrange
        var wallpaperInfo = CreateValidBingWallpaperInfo();
        var collectedWallpapers = new List<CollectedWallpaperInfo>
        {
            new CollectedWallpaperInfo(
                MarketCode.UnitedStates,
                ResolutionCode.UHD4K,
                DateTimeOffset.Now,
                wallpaperInfo)
        };

        var collectionResult = new CollectionResult(
            TotalCollected: 1,
            Duration: TimeSpan.FromSeconds(1),
            CollectionTime: DateTime.Now,
            CollectedWallpapers: collectedWallpapers);

        _mockBingWallpaperService.Setup(s => s.CollectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(collectionResult);

        _mockRepository.Setup(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _service.RunCollectionAsync();

        // Assert
        _mockBingWallpaperService.Verify(s => s.CollectAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunCollectionAsync_WithEmptyResult_ShouldNotProcessWallpapers()
    {
        // Arrange
        var collectionResult = new CollectionResult(
            TotalCollected: 0,
            Duration: TimeSpan.FromSeconds(1),
            CollectionTime: DateTime.Now,
            CollectedWallpapers: Array.Empty<CollectedWallpaperInfo>());

        _mockBingWallpaperService.Setup(s => s.CollectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(collectionResult);

        // Act
        await _service.RunCollectionAsync();

        // Assert
        _mockBingWallpaperService.Verify(s => s.CollectAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunCollectionAsync_WhenServiceThrowsException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        _mockBingWallpaperService.Setup(s => s.CollectAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Collection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RunCollectionAsync());
    }

    [Fact]
    public async Task RunCollectionAsync_WithCancellationToken_ShouldPassThroughToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var collectionResult = new CollectionResult(
            TotalCollected: 0,
            Duration: TimeSpan.FromSeconds(1),
            CollectionTime: DateTime.Now,
            CollectedWallpapers: Array.Empty<CollectedWallpaperInfo>());

        _mockBingWallpaperService.Setup(s => s.CollectAsync(cts.Token))
            .ReturnsAsync(collectionResult);

        // Act
        await _service.RunCollectionAsync(cts.Token);

        // Assert
        _mockBingWallpaperService.Verify(s => s.CollectAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task RunCollectionAsync_WithInvalidWallpaperInfo_ShouldSkipInvalidItems()
    {
        // Arrange - wallpaper with null UrlBase (invalid)
        var invalidWallpaperInfo = new BingWallpaperInfo
        {
            UrlBase = null!, // Invalid
            Title = "Test",
            StartDate = DateOnly.FromDateTime(DateTime.Now)
        };

        var collectedWallpapers = new List<CollectedWallpaperInfo>
        {
            new CollectedWallpaperInfo(
                MarketCode.UnitedStates,
                ResolutionCode.UHD4K,
                DateTimeOffset.Now,
                invalidWallpaperInfo)
        };

        var collectionResult = new CollectionResult(
            TotalCollected: 1,
            Duration: TimeSpan.FromSeconds(1),
            CollectionTime: DateTime.Now,
            CollectedWallpapers: collectedWallpapers);

        _mockBingWallpaperService.Setup(s => s.CollectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(collectionResult);

        // Act
        await _service.RunCollectionAsync();

        // Assert - Should not attempt to save invalid wallpaper
        _mockRepository.Verify(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunCollectionAsync_WithNullWallpaperInfo_ShouldSkipItem()
    {
        // Arrange
        var collectedWallpapers = new List<CollectedWallpaperInfo>
        {
            new CollectedWallpaperInfo(
                MarketCode.UnitedStates,
                ResolutionCode.UHD4K,
                DateTimeOffset.Now,
                null!)
        };

        var collectionResult = new CollectionResult(
            TotalCollected: 1,
            Duration: TimeSpan.FromSeconds(1),
            CollectionTime: DateTime.Now,
            CollectedWallpapers: collectedWallpapers);

        _mockBingWallpaperService.Setup(s => s.CollectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(collectionResult);

        // Act
        await _service.RunCollectionAsync();

        // Assert
        _mockRepository.Verify(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunCollectionAsync_WithEmptyTitle_ShouldSkipItem()
    {
        // Arrange
        var invalidWallpaperInfo = new BingWallpaperInfo
        {
            UrlBase = "/test/url",
            Title = "", // Invalid
            StartDate = DateOnly.FromDateTime(DateTime.Now)
        };

        var collectedWallpapers = new List<CollectedWallpaperInfo>
        {
            new CollectedWallpaperInfo(
                MarketCode.UnitedStates,
                ResolutionCode.UHD4K,
                DateTimeOffset.Now,
                invalidWallpaperInfo)
        };

        var collectionResult = new CollectionResult(
            TotalCollected: 1,
            Duration: TimeSpan.FromSeconds(1),
            CollectionTime: DateTime.Now,
            CollectedWallpapers: collectedWallpapers);

        _mockBingWallpaperService.Setup(s => s.CollectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(collectionResult);

        // Act
        await _service.RunCollectionAsync();

        // Assert
        _mockRepository.Verify(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunCollectionAsync_WithDefaultStartDate_ShouldSkipItem()
    {
        // Arrange
        var invalidWallpaperInfo = new BingWallpaperInfo
        {
            UrlBase = "/test/url",
            Title = "Test Title",
            StartDate = default // Invalid
        };

        var collectedWallpapers = new List<CollectedWallpaperInfo>
        {
            new CollectedWallpaperInfo(
                MarketCode.UnitedStates,
                ResolutionCode.UHD4K,
                DateTimeOffset.Now,
                invalidWallpaperInfo)
        };

        var collectionResult = new CollectionResult(
            TotalCollected: 1,
            Duration: TimeSpan.FromSeconds(1),
            CollectionTime: DateTime.Now,
            CollectedWallpapers: collectedWallpapers);

        _mockBingWallpaperService.Setup(s => s.CollectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(collectionResult);

        // Act
        await _service.RunCollectionAsync();

        // Assert
        _mockRepository.Verify(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunCollectionAsync_WhenSaveReturnsTrue_ShouldLogSaved()
    {
        // Arrange
        var wallpaperInfo = CreateValidBingWallpaperInfo();
        var collectedWallpapers = new List<CollectedWallpaperInfo>
        {
            new CollectedWallpaperInfo(
                MarketCode.UnitedStates,
                ResolutionCode.UHD4K,
                DateTimeOffset.Now,
                wallpaperInfo)
        };

        var collectionResult = new CollectionResult(
            TotalCollected: 1,
            Duration: TimeSpan.FromSeconds(1),
            CollectionTime: DateTime.Now,
            CollectedWallpapers: collectedWallpapers);

        _mockBingWallpaperService.Setup(s => s.CollectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(collectionResult);

        _mockRepository.Setup(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _service.RunCollectionAsync();

        // Assert
        _mockRepository.Verify(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunCollectionAsync_WhenSaveReturnsFalse_ShouldLogSkipped()
    {
        // Arrange
        var wallpaperInfo = CreateValidBingWallpaperInfo();
        var collectedWallpapers = new List<CollectedWallpaperInfo>
        {
            new CollectedWallpaperInfo(
                MarketCode.UnitedStates,
                ResolutionCode.UHD4K,
                DateTimeOffset.Now,
                wallpaperInfo)
        };

        var collectionResult = new CollectionResult(
            TotalCollected: 1,
            Duration: TimeSpan.FromSeconds(1),
            CollectionTime: DateTime.Now,
            CollectedWallpapers: collectedWallpapers);

        _mockBingWallpaperService.Setup(s => s.CollectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(collectionResult);

        _mockRepository.Setup(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _service.RunCollectionAsync();

        // Assert
        _mockRepository.Verify(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunCollectionAsync_WhenRepositoryThrowsException_ShouldLogError()
    {
        // Arrange
        var wallpaperInfo = CreateValidBingWallpaperInfo();
        var collectedWallpapers = new List<CollectedWallpaperInfo>
        {
            new CollectedWallpaperInfo(
                MarketCode.UnitedStates,
                ResolutionCode.UHD4K,
                DateTimeOffset.Now,
                wallpaperInfo)
        };

        var collectionResult = new CollectionResult(
            TotalCollected: 1,
            Duration: TimeSpan.FromSeconds(1),
            CollectionTime: DateTime.Now,
            CollectedWallpapers: collectedWallpapers);

        _mockBingWallpaperService.Setup(s => s.CollectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(collectionResult);

        _mockRepository.Setup(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        // Act
        await _service.RunCollectionAsync();

        // Assert - Should not throw, just log error
        _mockRepository.Verify(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunCollectionAsync_WithMultipleWallpapers_ShouldProcessAll()
    {
        // Arrange
        var wallpaperInfo1 = CreateValidBingWallpaperInfo();
        var wallpaperInfo2 = CreateValidBingWallpaperInfo();

        var collectedWallpapers = new List<CollectedWallpaperInfo>
        {
            new CollectedWallpaperInfo(MarketCode.UnitedStates, ResolutionCode.UHD4K, DateTimeOffset.Now, wallpaperInfo1),
            new CollectedWallpaperInfo(MarketCode.China, ResolutionCode.UHD4K, DateTimeOffset.Now, wallpaperInfo2)
        };

        var collectionResult = new CollectionResult(
            TotalCollected: 2,
            Duration: TimeSpan.FromSeconds(1),
            CollectionTime: DateTime.Now,
            CollectedWallpapers: collectedWallpapers);

        _mockBingWallpaperService.Setup(s => s.CollectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(collectionResult);

        _mockRepository.Setup(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _service.RunCollectionAsync();

        // Assert
        _mockRepository.Verify(r => r.SaveIfNotExistsAsync(It.IsAny<WallpaperEntity>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    #endregion

    #region GetLatestAsync Tests

    [Fact]
    public async Task GetLatestAsync_WithNullMarket_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetLatestAsync(null!));
    }

    [Fact]
    public async Task GetLatestAsync_WhenWallpaperExists_ShouldReturnWallpaper()
    {
        // Arrange
        var market = CreateTestMarketDto();
        var wallpaperEntity = CreateTestWallpaperEntity();

        _mockRepository.Setup(r => r.GetLatestAsync(market.Code, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { wallpaperEntity });

        // Act
        var result = await _service.GetLatestAsync(market);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(wallpaperEntity.Id);
    }

    [Fact]
    public async Task GetLatestAsync_WhenWallpaperNotFound_ShouldReturnNull()
    {
        // Arrange
        var market = CreateTestMarketDto();

        _mockRepository.Setup(r => r.GetLatestAsync(market.Code, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WallpaperEntity>());

        // Act
        var result = await _service.GetLatestAsync(market);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestAsync_WhenRepositoryThrowsException_ShouldRethrow()
    {
        // Arrange
        var market = CreateTestMarketDto();

        _mockRepository.Setup(r => r.GetLatestAsync(market.Code, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetLatestAsync(market));
    }

    [Fact]
    public async Task GetLatestAsync_WithCancellationToken_ShouldPassThroughToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var market = CreateTestMarketDto();

        _mockRepository.Setup(r => r.GetLatestAsync(market.Code, 1, cts.Token))
            .ReturnsAsync(Array.Empty<WallpaperEntity>());

        // Act
        await _service.GetLatestAsync(market, cts.Token);

        // Assert
        _mockRepository.Verify(r => r.GetLatestAsync(market.Code, 1, cts.Token), Times.Once);
    }

    #endregion

    #region GetByMarketCodeAsync Tests

    [Fact]
    public async Task GetByMarketCodeAsync_WithNegativePageNumber_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var market = CreateTestMarketDto();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.GetByMarketCodeAsync(market, pageNumber: -1));
    }

    [Fact]
    public async Task GetByMarketCodeAsync_WithZeroPageNumber_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var market = CreateTestMarketDto();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.GetByMarketCodeAsync(market, pageNumber: 0));
    }

    [Fact]
    public async Task GetByMarketCodeAsync_WithNegativePageSize_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var market = CreateTestMarketDto();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.GetByMarketCodeAsync(market, pageSize: -1));
    }

    [Fact]
    public async Task GetByMarketCodeAsync_WithZeroPageSize_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var market = CreateTestMarketDto();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.GetByMarketCodeAsync(market, pageSize: 0));
    }

    [Fact]
    public async Task GetByMarketCodeAsync_WithValidParameters_ShouldReturnWallpapers()
    {
        // Arrange
        var market = CreateTestMarketDto();
        var wallpaperEntities = new[] { CreateTestWallpaperEntity(), CreateTestWallpaperEntity() };

        _mockRepository.Setup(r => r.GetByMarketCodeAsync(market.Code, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallpaperEntities);

        // Act
        var result = await _service.GetByMarketCodeAsync(market);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByMarketCodeAsync_WhenRepositoryThrowsException_ShouldRethrow()
    {
        // Arrange
        var market = CreateTestMarketDto();

        _mockRepository.Setup(r => r.GetByMarketCodeAsync(market.Code, 1, 20, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetByMarketCodeAsync(market));
    }

    [Fact]
    public async Task GetByMarketCodeAsync_WithCustomPageParameters_ShouldPassCorrectValues()
    {
        // Arrange
        var market = CreateTestMarketDto();
        var pageNumber = 5;
        var pageSize = 50;

        _mockRepository.Setup(r => r.GetByMarketCodeAsync(market.Code, pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WallpaperEntity>());

        // Act
        await _service.GetByMarketCodeAsync(market, pageNumber, pageSize);

        // Assert
        _mockRepository.Verify(r => r.GetByMarketCodeAsync(market.Code, pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByMarketCodeAsync_WithCancellationToken_ShouldPassThroughToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var market = CreateTestMarketDto();

        _mockRepository.Setup(r => r.GetByMarketCodeAsync(market.Code, 1, 20, cts.Token))
            .ReturnsAsync(Array.Empty<WallpaperEntity>());

        // Act
        await _service.GetByMarketCodeAsync(market, cancellationToken: cts.Token);

        // Assert
        _mockRepository.Verify(r => r.GetByMarketCodeAsync(market.Code, 1, 20, cts.Token), Times.Once);
    }

    [Fact]
    public async Task GetByMarketCodeAsync_WhenNoWallpapersFound_ShouldReturnEmptyCollection()
    {
        // Arrange
        var market = CreateTestMarketDto();

        _mockRepository.Setup(r => r.GetByMarketCodeAsync(market.Code, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WallpaperEntity>());

        // Act
        var result = await _service.GetByMarketCodeAsync(market);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetSupportedMarketCodesAsync Tests

    [Fact]
    public async Task GetSupportedMarketCodesAsync_ShouldReturnAllMarketCodes()
    {
        // Act
        var result = await _service.GetSupportedMarketCodesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.All(m => !string.IsNullOrEmpty(m.Name)).Should().BeTrue();
    }

    [Fact]
    public async Task GetSupportedMarketCodesAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _service.GetSupportedMarketCodesAsync(cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region GetSupportedResolutionsAsync Tests

    [Fact]
    public async Task GetSupportedResolutionsAsync_ShouldReturnAllResolutions()
    {
        // Act
        var result = await _service.GetSupportedResolutionsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.All(r => !string.IsNullOrEmpty(r.Name)).Should().BeTrue();
    }

    [Fact]
    public async Task GetSupportedResolutionsAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _service.GetSupportedResolutionsAsync(cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region GetMoreDetailsAsync Tests

    [Fact]
    public async Task GetMoreDetailsAsync_WhenEntityExists_ShouldReturnInfoJson()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateTestWallpaperEntity();
        entity.InfoJson = "{\"test\":\"data\"}";

        _mockRepository.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _service.GetMoreDetailsAsync(id);

        // Assert
        result.Should().Be("{\"test\":\"data\"}");
    }

    [Fact]
    public async Task GetMoreDetailsAsync_WhenEntityNotFound_ShouldReturnEmptyString()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WallpaperEntity)null!);

        // Act
        var result = await _service.GetMoreDetailsAsync(id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMoreDetailsAsync_WithCancellationToken_ShouldPassThroughToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var id = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetAsync(id, cts.Token))
            .ReturnsAsync((WallpaperEntity)null!);

        // Act
        await _service.GetMoreDetailsAsync(id, cts.Token);

        // Assert
        _mockRepository.Verify(r => r.GetAsync(id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task GetMoreDetailsAsync_WhenEntityHasEmptyInfoJson_ShouldReturnEmptyString()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateTestWallpaperEntity();
        entity.InfoJson = string.Empty;

        _mockRepository.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _service.GetMoreDetailsAsync(id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private static BingWallpaperInfo CreateValidBingWallpaperInfo()
    {
        return new BingWallpaperInfo
        {
            UrlBase = "/test/urlbase",
            Title = "Test Wallpaper",
            Copyright = "Test Copyright",
            CopyrightOnly = "Test",
            CopyrightLink = "https://test.com",
            Caption = "Test Caption",
            Desc = "Test Description",
            Hash = "test-hash",
            StartDate = DateOnly.FromDateTime(DateTime.Now),
            EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            FullStartDate = DateTime.Now,
            Date = DateTime.Now.ToString("yyyyMMdd"),
            Hs = Array.Empty<object>()
        };
    }

    private static MarketInfoDto CreateTestMarketDto()
    {
        return new MarketInfoDto(
            Code: MarketCode.UnitedStates,
            Name: "United States",
            Description: "US",
            Flag: "🇺🇸",
            Note: ""
        );
    }

    private static WallpaperEntity CreateTestWallpaperEntity()
    {
        var now = DateTime.Now;
        var startDate = DateOnly.FromDateTime(now);
        var endDate = startDate.AddDays(1);

        return new WallpaperEntity
        {
            Id = Guid.NewGuid(),
            Hash = "test-hash",
            ActualDate = now,
            MarketCode = MarketCode.UnitedStates,
            ResolutionCode = ResolutionCode.UHD4K,
            InfoJson = $@"{{
                ""date"": ""{now:MMM dd, yyyy}"",
                ""country"": ""UnitedStates"",
                ""marketCode"": ""en-US"",
                ""title"": ""Test Wallpaper"",
                ""bsTitle"": ""Test BsTitle"",
                ""caption"": ""Test Caption"",
                ""copyright"": ""Test Copyright"",
                ""copyrightOnly"": ""Test"",
                ""copyrightLink"": ""https://test.com"",
                ""description"": ""Test Description"",
                ""quiz"": """",
                ""hash"": ""test-hash"",
                ""imageResolutions"": [
                    {{
                        ""resolution"": ""Standard"",
                        ""url"": ""https://bing.com/test_1366x768.jpg"",
                        ""size"": ""1366x768""
                    }}
                ],
                ""timeInfo"": {{
                    ""startDate"": ""{startDate:yyyyMMdd}"",
                    ""endDate"": ""{endDate:yyyyMMdd}"",
                    ""fullStartDateTime"": ""{now:yyyyMMddHHmm}"",
                    ""originalTimeFields"": {{
                        ""startDate"": ""{startDate:yyyyMMdd}"",
                        ""fullStartDate"": ""{now:yyyyMMddHHmm}"",
                        ""endDate"": ""{endDate:yyyyMMdd}""
                    }}
                }},
                ""createdAt"": ""{now:yyyyMMddHHmm}"",
                ""originalUrlBase"": ""/test/urlbase""
            }}"
        };
    }

    #endregion
}

