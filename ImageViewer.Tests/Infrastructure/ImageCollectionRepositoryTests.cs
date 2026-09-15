using System.IO;
using ImageViewer.Application.Interfaces;
using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Entities;
using ImageViewer.Domain.ValueObjects;
using ImageViewer.Infrastructure.Repositories;
using ImageViewer.Infrastructure.Serialization;
using Xunit;

namespace ImageViewer.Tests.Infrastructure;

public class ImageCollectionRepositoryTests : IDisposable
{
    private static readonly DateTimeOffset FixedDate = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), "imageviewer_tests_" + Guid.NewGuid().ToString("N"));

    private readonly ImageCollectionRepository _repository =
        new(new IImageSerializer[] { new BinaryImageSerializer() });

    public ImageCollectionRepositoryTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch (IOException) { /* best effort cleanup */ }
    }

    [Fact]
    public async Task SaveThenLoad_LazyLoadsOriginalData_FromSourceFile()
    {
        var sourcePath = Path.Combine(_tempDir, "cat.png");
        var sourceBytes = new byte[] { 1, 2, 3, 4, 5 };
        File.WriteAllBytes(sourcePath, sourceBytes);

        var image = NewImage(sourcePath);
        image.AttachThumbnail(ImageBinaryData.CreateFromBytes(new byte[] { 9, 9 }));
        var collection = ImageCollection.Restore(Guid.NewGuid(), FixedDate, new[] { image });

        var collectionPath = Path.Combine(_tempDir, "collection.imgcollection");
        var skipped = await _repository.SaveAsync(collection, collectionPath);

        Assert.Empty(skipped);

        var loaded = await _repository.LoadAsync(collectionPath);

        Assert.Equal(collection.Id, loaded.Id);
        var loadedImage = Assert.Single(loaded.Images);
        Assert.Equal(sourceBytes, loadedImage.OriginalData!.ToArray());
        Assert.True(new byte[] { 9, 9 }.AsSpan().SequenceEqual(loadedImage.Thumbnail!.AsMemory().Span));
    }

    [Fact]
    public async Task SaveAsync_WithAlreadyAttachedOriginalData_StillSavesTheImage()
    {
        // Regression: an early version skipped Images.Add for images whose
        // OriginalData was already present, silently dropping them from the file.
        var original = ImageBinaryData.CreateFromBytes(new byte[] { 7, 7 });
        var image = Image.Restore(Guid.NewGuid(), "x.png", @"C:\gone\x.png", FixedDate,
            FileSize.FromBytes(7), null, null, original);
        var collection = ImageCollection.Restore(Guid.NewGuid(), FixedDate, new[] { image });

        var collectionPath = Path.Combine(_tempDir, "collection.imgcollection");
        var skipped = await _repository.SaveAsync(collection, collectionPath);

        Assert.Empty(skipped);

        var loaded = await _repository.LoadAsync(collectionPath);
        var loadedImage = Assert.Single(loaded.Images);
        Assert.True(new byte[] { 7, 7 }.AsSpan().SequenceEqual(loadedImage.OriginalData!.AsMemory().Span));
    }

    [Fact]
    public async Task SaveAsync_WithMissingSourceFile_ReportsSkippedPath_AndStillSavesImage()
    {
        var missingPath = Path.Combine(_tempDir, "missing.png");
        var image = NewImage(missingPath);
        var collection = ImageCollection.Restore(Guid.NewGuid(), FixedDate, new[] { image });

        var collectionPath = Path.Combine(_tempDir, "collection.imgcollection");
        var skipped = await _repository.SaveAsync(collection, collectionPath);

        var skippedPath = Assert.Single(skipped);
        Assert.Equal(missingPath, skippedPath);

        // metadata-only entry survives
        var loaded = await _repository.LoadAsync(collectionPath);
        var loadedImage = Assert.Single(loaded.Images);
        Assert.Null(loadedImage.OriginalData);
    }

    [Fact]
    public async Task LoadAsync_WithMissingFile_Throws()
    {
        var missing = Path.Combine(_tempDir, "nope.imgcollection");

        await Assert.ThrowsAsync<FileNotFoundException>(() => _repository.LoadAsync(missing));
    }

    [Fact]
    public void Constructor_WithoutSerializers_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () => new ImageCollectionRepository(Array.Empty<IImageSerializer>()));
    }

    private static Image NewImage(string path) =>
        Image.Create(Path.GetFileName(path), path, FixedDate, FileSize.FromBytes(10), null);
}
