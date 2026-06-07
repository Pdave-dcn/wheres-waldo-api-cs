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

        var image = modelBuilder.Entity<Image>();

        image.HasIndex(i => i.Name).IsUnique();
        image.HasIndex(i => i.ImageUrl).IsUnique();
        image.HasIndex(i => i.PublicId).IsUnique();

        var character = modelBuilder.Entity<Character>();

        character
            .Property(c => c.CharacterType)
            .HasConversion<string>();

        character
            .HasIndex(c => new {c.ImageId, c.CharacterType})
            .IsUnique();

        character
            .HasOne(c => c.Image)
            .WithMany(i => i.Characters)
            .HasForeignKey(c => c.ImageId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<Image> Images { get; set; }
    public DbSet<Character> Characters {get; set;}
}