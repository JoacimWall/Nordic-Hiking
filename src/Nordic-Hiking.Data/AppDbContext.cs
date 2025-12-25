using Microsoft.EntityFrameworkCore;
using NordicHiking.Core.Models;

namespace NordicHiking.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Channel> Channels { get; set; }
    public DbSet<Video> Videos { get; set; }
    public DbSet<HikeLocation> HikeLocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.YoutubeChannelId).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Url).IsRequired();
            entity.HasMany(e => e.Videos)
                .WithOne(e => e.Channel)
                .HasForeignKey(e => e.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.YoutubeVideoId).IsRequired();
            entity.Property(e => e.Title).IsRequired();
            entity.HasMany(e => e.Locations)
                .WithOne(e => e.Video)
                .HasForeignKey(e => e.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HikeLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlaceName).IsRequired();
        });
    }
}
