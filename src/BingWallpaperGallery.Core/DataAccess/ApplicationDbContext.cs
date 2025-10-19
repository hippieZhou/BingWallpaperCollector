// Copyright (c) hippieZhou. All rights reserved.

using System.Reflection;
using BingWallpaperGallery.Core.DataAccess.Domains;
using Microsoft.EntityFrameworkCore;

namespace BingWallpaperGallery.Core.DataAccess;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<WallpaperEntity> Wallpapers { get; set; }

    public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {
        await Database.MigrateAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseCollation("BINARY");
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }
}
