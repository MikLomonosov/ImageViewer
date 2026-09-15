using System.IO;
using ImageViewer.Application.Configuration;
using ImageViewer.Infrastructure.ImageProcessing;
using Microsoft.Extensions.Options;
using Xunit;

namespace ImageViewer.Tests.Infrastructure;

public class ImageLoaderServiceTests : IDisposable
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), "imageviewer_loader_" + Guid.NewGuid().ToString("N"));

    public ImageLoaderServiceTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch (IOException) { /* best effort cleanup */ }
    }

    private static ImageLoaderService CreateLoader(long maxFileSizeBytes = 50 * 1024 * 1024) =>
        new(Options.Create(new ImageLoadingLimits
        {
            MaxFileSizeBytes = maxFileSizeBytes
        }),
            new ThumbnailService());

    [Fact]
    public async Task LoadImagesAsync_WithValidFile_SetsSizeToFileLength()
    {
        // Regression: an early version stored the thumbnail byte length
        // instead of the original file length.
        var pngPath = Path.Combine(_tempDir, "photo.png");
        var pngBytes = TestPng.Create(300, 200);
        File.WriteAllBytes(pngPath, pngBytes);

        var result = await CreateLoader().LoadImagesAsync(new[] { pngPath }, CancellationToken.None);

        var image = Assert.Single(result.LoadedImages);
        Assert.Empty(result.Errors);
        Assert.Equal(pngBytes.Length, image.Size.Bytes);
        Assert.Equal(300, image.Dimensions!.Width);
        Assert.Equal(200, image.Dimensions.Height);
        Assert.NotNull(image.Thumbnail);
        Assert.Null(image.OriginalData); // originals are lazily loaded only on save
    }

    [Fact]
    public async Task LoadImagesAsync_WithMissingFile_ReturnsError()
    {
        var missing = Path.Combine(_tempDir, "missing.png");

        var result = await CreateLoader().LoadImagesAsync(new[] { missing }, CancellationToken.None);

        Assert.Empty(result.LoadedImages);
        var error = Assert.Single(result.Errors);
        Assert.Equal(missing, error.FilePath);
        Assert.False(string.IsNullOrEmpty(error.Reason));
    }

    [Fact]
    public async Task LoadImagesAsync_WithOversizedFile_ReturnsError()
    {
        var pngPath = Path.Combine(_tempDir, "big.png");
        File.WriteAllBytes(pngPath, TestPng.Create(10, 10));

        var result = await CreateLoader(maxFileSizeBytes: 4).LoadImagesAsync(new[] { pngPath }, CancellationToken.None);

        Assert.Empty(result.LoadedImages);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task LoadImagesAsync_WithCorruptedFile_ReturnsError()
    {
        var path = Path.Combine(_tempDir, "garbage.png");
        File.WriteAllBytes(path, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

        var result = await CreateLoader().LoadImagesAsync(new[] { path }, CancellationToken.None);

        Assert.Empty(result.LoadedImages);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task LoadImagesAsync_WithEmptyList_ReturnsEmptyResult()
    {
        var result = await CreateLoader().LoadImagesAsync(Array.Empty<string>(), CancellationToken.None);

        Assert.Empty(result.LoadedImages);
        Assert.Empty(result.Errors);
    }
}
