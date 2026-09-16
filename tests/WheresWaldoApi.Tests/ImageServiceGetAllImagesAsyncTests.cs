using Xunit;
using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Tests;

public class ImageServiceGetAllImagesAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly ImageService _imageService;

  public ImageServiceGetAllImagesAsyncTests()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new AppDbContext(options);

    _imageService = new ImageService(_context);
  }

  public void Dispose()
  {
    _context.Database.EnsureDeleted();
    _context.Dispose();
    GC.SuppressFinalize(this);
  }

  private async Task<Image> SeedImageAsync(string name, string url)
  {
    var image = new Image
    {
      Id = Guid.NewGuid(),
      Name = name,
      Description = "A test image.",
      ImageUrl = url,
      PublicId = name + "-public-id",
      OriginalWidth = 800,
      OriginalHeight = 600
    };

    _context.Images.Add(image);
    await _context.SaveChangesAsync();
    return image;
  }

  [Fact]
  public async Task GetAllImagesAsync_WithNoImages_ShouldReturnEmptyList()
  {
    var result = await _imageService.GetAllImagesAsync();

    Assert.NotNull(result);
    Assert.Empty(result);
  }

  [Fact]
  public async Task GetAllImagesAsync_WithMultipleImages_ShouldReturnAllImages()
  {
    await SeedImageAsync("Image One", "https://res.cloudinary.com/demo/image/upload/one.jpg");
    await SeedImageAsync("Image Two", "https://res.cloudinary.com/demo/image/upload/two.jpg");
    await SeedImageAsync("Image Three", "https://res.cloudinary.com/demo/image/upload/three.jpg");

    var result = await _imageService.GetAllImagesAsync();

    Assert.Equal(3, result.Count);
  }

  [Fact]
  public async Task GetAllImagesAsync_ShouldMapListFieldsCorrectly()
  {
    var seeded = await SeedImageAsync("Waldo Beach", "https://res.cloudinary.com/demo/image/upload/beach.jpg");

    var result = await _imageService.GetAllImagesAsync();

    var dto = Assert.Single(result);
    Assert.Equal(seeded.Id, dto.Id);
    Assert.Equal(seeded.Name, dto.Name);
    Assert.Equal(seeded.Description, dto.Description);
    Assert.Equal(seeded.ImageUrl, dto.ImageUrl);
  }

  [Fact]
  public async Task GetAllImagesAsync_ListItemDtoShouldNotExposeExtraFields()
  {
    await SeedImageAsync("Waldo Beach", "https://res.cloudinary.com/demo/image/upload/beach.jpg");

    var result = await _imageService.GetAllImagesAsync();

    var dtoProps = typeof(ImageListItemDto).GetProperties().Select(p => p.Name).ToHashSet();
    Assert.Contains("Id", dtoProps);
    Assert.Contains("Name", dtoProps);
    Assert.Contains("Description", dtoProps);
    Assert.Contains("ImageUrl", dtoProps);
    Assert.DoesNotContain("OriginalWidth", dtoProps);
    Assert.DoesNotContain("OriginalHeight", dtoProps);
    Assert.DoesNotContain("PublicId", dtoProps);
  }
}