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

  private Image BuildImage(string name, DateTime createdAt)
  {
    return new Image
    {
      Id = Guid.NewGuid(),
      Name = name,
      Description = "A test image.",
      ImageUrl = "https://res.cloudinary.com/demo/image/upload/" + name + ".jpg",
      PublicId = name + "-public-id",
      OriginalWidth = 800,
      OriginalHeight = 600,
      CreatedAt = createdAt
    };
  }

  private async Task<Image> SeedImageAsync(string name, DateTime createdAt)
  {
    var image = BuildImage(name, createdAt);
    _context.Images.Add(image);
    await _context.SaveChangesAsync();
    return image;
  }

  [Fact]
  public async Task GetAllImagesAsync_WithNoImages_ShouldReturnEmptyPage()
  {
    var result = await _imageService.GetAllImagesAsync(null, 20);

    Assert.NotNull(result);
    Assert.Empty(result.Items);
    Assert.Null(result.NextCursor);
    Assert.False(result.HasMore);
  }

  [Fact]
  public async Task GetAllImagesAsync_WithMultipleImagesWithinPageSize_ShouldReturnAll()
  {
    var baseTime = DateTime.UtcNow;
    await SeedImageAsync("Image One", baseTime.AddMinutes(2));
    await SeedImageAsync("Image Two", baseTime.AddMinutes(1));
    await SeedImageAsync("Image Three", baseTime.AddMinutes(3));

    var result = await _imageService.GetAllImagesAsync(null, 20);

    Assert.Equal(3, result.Items.Count);
    Assert.False(result.HasMore);
    Assert.Null(result.NextCursor);
  }

  [Fact]
  public async Task GetAllImagesAsync_ShouldMapListFieldsCorrectly()
  {
    var seeded = await SeedImageAsync("Waldo Beach", DateTime.UtcNow);

    var result = await _imageService.GetAllImagesAsync(null, 20);

    var dto = Assert.Single(result.Items);
    Assert.Equal(seeded.Id, dto.Id);
    Assert.Equal(seeded.Name, dto.Name);
    Assert.Equal(seeded.Description, dto.Description);
    Assert.Equal(seeded.ImageUrl, dto.ImageUrl);
  }

  [Fact]
  public async Task GetAllImagesAsync_ListItemDtoShouldNotExposeExtraFields()
  {
    await SeedImageAsync("Waldo Beach", DateTime.UtcNow);

    var result = await _imageService.GetAllImagesAsync(null, 20);

    var dtoProps = typeof(ImageListItemDto).GetProperties().Select(p => p.Name).ToHashSet();
    Assert.Contains("Id", dtoProps);
    Assert.Contains("Name", dtoProps);
    Assert.Contains("Description", dtoProps);
    Assert.Contains("ImageUrl", dtoProps);
    Assert.DoesNotContain("OriginalWidth", dtoProps);
    Assert.DoesNotContain("OriginalHeight", dtoProps);
    Assert.DoesNotContain("PublicId", dtoProps);
  }

  [Fact]
  public async Task GetAllImagesAsync_WithMoreImagesThanPageSize_ShouldReturnPageSizeItemsAndHasMore()
  {
    var baseTime = DateTime.UtcNow;
    for (int i = 1; i <= 5; i++)
    {
      await SeedImageAsync("Image " + i, baseTime.AddMinutes(i));
    }

    var result = await _imageService.GetAllImagesAsync(null, 2);

    Assert.Equal(2, result.Items.Count);
    Assert.True(result.HasMore);
    Assert.NotNull(result.NextCursor);
  }

  [Fact]
  public async Task GetAllImagesAsync_WithCursor_ShouldReturnImagesAfterTheCursor()
  {
    var baseTime = DateTime.UtcNow;
    for (int i = 1; i <= 5; i++)
    {
      await SeedImageAsync("Image " + i, baseTime.AddMinutes(i));
    }

    var firstPage = await _imageService.GetAllImagesAsync(null, 2);
    Assert.Equal(2, firstPage.Items.Count);
    Assert.True(firstPage.HasMore);
    Assert.NotNull(firstPage.NextCursor);

    var secondPage = await _imageService.GetAllImagesAsync(firstPage.NextCursor, 2);
    Assert.Equal(2, secondPage.Items.Count);
    Assert.True(secondPage.HasMore);

    var firstPageIds = firstPage.Items.Select(i => i.Id).ToHashSet();
    Assert.True(secondPage.Items.All(i => !firstPageIds.Contains(i.Id)));

    var lastPage = await _imageService.GetAllImagesAsync(secondPage.NextCursor, 2);
    Assert.Single(lastPage.Items);
    Assert.False(lastPage.HasMore);
    Assert.Null(lastPage.NextCursor);
  }

  [Fact]
  public async Task GetAllImagesAsync_ShouldOrderByCreatedAtAscending()
  {
    var baseTime = DateTime.UtcNow.AddMinutes(3);
    await SeedImageAsync("Image A", baseTime.AddHours(2));
    await SeedImageAsync("Image B", baseTime);
    await SeedImageAsync("Image C", baseTime.AddMinutes(5));

    var result = await _imageService.GetAllImagesAsync(null, 20);

    Assert.Equal(new[] { "Image B", "Image C", "Image A" }, result.Items.Select(i => i.Name).ToArray());
  }

  [Fact]
  public async Task GetAllImagesAsync_PageSizeBelowOne_ShouldClampToOne()
  {
    var baseTime = DateTime.UtcNow;
    await SeedImageAsync("Image One", baseTime.AddMinutes(1));
    await SeedImageAsync("Image Two", baseTime.AddMinutes(2));

    var result = await _imageService.GetAllImagesAsync(null, 0);

    Assert.Single(result.Items);
    Assert.True(result.HasMore);
  }

  [Fact]
  public async Task GetAllImagesAsync_PageSizeAboveOneHundred_ShouldClampToOneHundred()
  {
    var baseTime = DateTime.UtcNow;
    for (int i = 1; i <= 120; i++)
    {
      _context.Images.Add(BuildImage("Image " + i, baseTime.AddMinutes(i)));
    }
    await _context.SaveChangesAsync();

    var result = await _imageService.GetAllImagesAsync(null, 1000);

    Assert.Equal(100, result.Items.Count);
    Assert.True(result.HasMore);
  }
}