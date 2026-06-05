using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Models;

namespace WheresWaldoApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Image>()
        .HasIndex(i => i.Name)
        .IsUnique();

    modelBuilder.Entity<Image>()
        .HasIndex(i => i.ImageUrl)
        .IsUnique();

    modelBuilder.Entity<Image>()
        .HasIndex(i => i.PublicId)
        .IsUnique();


    // var image = modelBuilder.Entity<Image>();

    // image.HasIndex(i => i.Name).IsUnique();
    // image.HasIndex(i => i.ImageUrl).IsUnique();
    // image.HasIndex(i => i.PublicId).IsUnique();
}

    public DbSet<Image> Images { get; set; }
}