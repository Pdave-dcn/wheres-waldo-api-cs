using Xunit;
using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Tests;

public class ImageServiceAddImageAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly ImageService _imageService;

  public ImageServiceAddImageAsyncTests()
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

  private AddImageDto BuildDto(string name = "Test Image")
  {
    return new AddImageDto
    {
      Name = name,
      Description = "A test image.",
      ImageUrl = "https://res.cloudinary.com/demo/image/upload/" + name + ".jpg",
      PublicId = name + "-public-id",
      OriginalWidth = 800,
      OriginalHeight = 600
    };
  }

  [Fact]
  public async Task AddImageAsync_WithValidDto_ShouldPersistAndReturnImage()
  {
    var dto = BuildDto();

    var result = await _imageService.AddImageAsync(dto);

    Assert.NotNull(result);
    Assert.NotEqual(Guid.Empty, result.Id);
    Assert.Equal(dto.Name, result.Name);
    Assert.Equal(dto.Description, result.Description);
    Assert.Equal(dto.ImageUrl, result.ImageUrl);
    Assert.Equal(dto.PublicId, result.PublicId);
    Assert.Equal(dto.OriginalWidth, result.OriginalWidth);
    Assert.Equal(dto.OriginalHeight, result.OriginalHeight);

    var persisted = await _context.Images.FindAsync(result.Id);
    Assert.NotNull(persisted);
    Assert.Equal(1, await _context.Images.CountAsync());
  }

  [Fact]
  public async Task AddImageAsync_WithDuplicateName_ShouldThrowImageAlreadyExistsException()
  {
    await _imageService.AddImageAsync(BuildDto("Beach Scene"));

    var ex = await Assert.ThrowsAsync<ImageAlreadyExistsException>(
      () => _imageService.AddImageAsync(BuildDto("Beach Scene")));

    Assert.Equal("Image 'Beach Scene' already exists.", ex.Message);
  }

  [Fact]
  public async Task AddImageAsync_WithDuplicateName_ShouldNotPersistSecondImage()
  {
    await _imageService.AddImageAsync(BuildDto("Beach Scene"));

    await Assert.ThrowsAsync<ImageAlreadyExistsException>(
      () => _imageService.AddImageAsync(BuildDto("Beach Scene")));

    Assert.Equal(1, await _context.Images.CountAsync());
  }

  [Fact]
  public async Task AddImageAsync_WithDistinctNames_ShouldSucceedForBoth()
  {
    var first = await _imageService.AddImageAsync(BuildDto("Beach Scene"));
    var second = await _imageService.AddImageAsync(BuildDto("Mountain Scene"));

    Assert.NotEqual(first.Id, second.Id);
    Assert.Equal(2, await _context.Images.CountAsync());
  }
}