// Copyright (c) hippieZhou. All rights reserved.

using BingWallpaperGallery.Core.DataAccess;
using BingWallpaperGallery.Core.DataAccess.Interceptors;
using BingWallpaperGallery.Core.DataAccess.Repositories;
using BingWallpaperGallery.Core.DataAccess.Repositories.Impl;
using BingWallpaperGallery.Core.Http.Configuration;
using BingWallpaperGallery.Core.Http.Network;
using BingWallpaperGallery.Core.Http.Network.Impl;
using BingWallpaperGallery.Core.Http.Options;
using BingWallpaperGallery.Core.Http.Services;
using BingWallpaperGallery.Core.Http.Services.Impl;
using BingWallpaperGallery.Core.Services;
using BingWallpaperGallery.Core.Services.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BingWallpaperGallery.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorelayer(
        this IServiceCollection services,
        IConfiguration configuration, string appDataPath)
    {
        services.Configure<CollectionOptions>(configuration.GetSection(nameof(CollectionOptions)))
            .PostConfigure<CollectionOptions>(CollectionOptions.Validate);

        services.AddBingWallpaperCollector();
        services.AddInfrastructure(appDataPath);

        services.AddMemoryCache();

        #region Services
        services.AddSingleton<IManagementService, ManagementService>();
        services.AddSingleton<IDownloadService, DownloadService>();
        services.AddSingleton<ILocalStorageService, LocalStorageService>();
        services.AddSingleton<IGitHubStorageService, GitHubStorageService>();
        #endregion

        return services;
    }

    private static void AddBingWallpaperCollector(this IServiceCollection services)
    {
        // 配置HttpClient with 弹性策略
        services.AddHttpClient<IBingWallpaperClient, BingWallpaperClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(HTTPConstants.HttpTimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", HTTPConstants.HttpHeaders.UserAgent);
        }).AddStandardResilienceHandler(ResilienceConfiguration.ConfigureStandardResilience);

        services.AddHttpClient<IImageDownloadClient, ImageDownloadClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(HTTPConstants.HttpTimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", HTTPConstants.HttpHeaders.UserAgent);
        }).AddStandardResilienceHandler(ResilienceConfiguration.ConfigureStandardResilience);

        services.AddHttpClient<IGithubRepositoryClient, GithubRepositoryClient>(client =>
        {
            client.BaseAddress = new Uri(HTTPConstants.GithubRepositoryBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(HTTPConstants.HttpTimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", HTTPConstants.HttpHeaders.UserAgent);
        }).AddStandardResilienceHandler(ResilienceConfiguration.ConfigureStandardResilience);

        // 注册服务
        services.AddScoped<IBingWallpaperService, BingWallpaperService>();
        services.AddScoped<IImageDownloadService, ImageDownloadService>();
        services.AddScoped<IGithubRepositoryService, GithubRepositoryService>();
    }

    private static void AddInfrastructure(this IServiceCollection services, string appDataPath)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<IDbConnectionInterceptor, SqliteJournalModeSettingInterceptor>();

        services.AddPooledDbContextFactory<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.AddInterceptors(sp.GetServices<IDbConnectionInterceptor>());
            var dbFile = Path.Combine(appDataPath, "database.db");

            var migrationsAssemblyName = typeof(ApplicationDbContext).Assembly.GetName().Name;
            options.UseSqlite($"Data Source={dbFile}", db => db.MigrationsAssembly(migrationsAssemblyName));
        });

        #region Repositories
        services.AddScoped<IWallpaperRepository, WallpaperRepository>();
        #endregion
    }
}
