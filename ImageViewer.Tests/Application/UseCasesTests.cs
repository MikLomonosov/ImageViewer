using ImageViewer.Application.Interfaces;
using ImageViewer.Application.Models;
using ImageViewer.Application.UseCases;
using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Entities;
using ImageViewer.Domain.Repositories;
using ImageViewer.Domain.ValueObjects;
using Xunit;

namespace ImageViewer.Tests.Application;

public class LoadImagesUseCaseTests
{
    private static readonly DateTimeOffset Created = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private sealed class FakeImageLoader : IImageLoaderService
    {
        public IReadOnlyList<string>? RequestedFilePaths { get; private set; }

        public ImageLoadResult Result { get; set; } =
            new(Array.Empty<Image>(), Array.Empty<ImageLoadError>());

        public Task<ImageLoadResult> LoadImagesAsync(IReadOnlyList<string> filePaths, CancellationToken cancellationToken)
        {
            RequestedFilePaths = filePaths;
            return Task.FromResult(Result);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithoutPaths_ReturnsEmptyResult_AndSkipsLoading()
    {
        var loader = new FakeImageLoader();
        var useCase = new LoadImagesUseCase(loader);
        var collection = ImageCollection.Create();

        var result = await useCase.ExecuteAsync(collection, Array.Empty<string>());

        Assert.Empty(result.LoadedImages);
        Assert.Empty(result.Errors);
        Assert.Null(loader.RequestedFilePaths);
        Assert.Empty(collection.Images);
    }

    [Fact]
    public async Task ExecuteAsync_AddsLoadedImages_ToCollection()
    {
        var images = new[] { NewImage(@"C:\a.png"), NewImage(@"C:\b.png") };
        var loader = new FakeImageLoader
        {
            Result = new ImageLoadResult(images, Array.Empty<ImageLoadError>())
        };
        var useCase = new LoadImagesUseCase(loader);
        var collection = ImageCollection.Create();

        var result = await useCase.ExecuteAsync(collection, new[] { @"C:\a.png", @"C:\b.png" });

        Assert.Equal(2, result.LoadedImages.Count);
        Assert.Empty(result.Errors);
        Assert.Equal(2, collection.Count);
        Assert.Equal(new[] { @"C:\a.png", @"C:\b.png" }, loader.RequestedFilePaths);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicatePaths_AreSkippedInsideCollection()
    {
        var loader = new FakeImageLoader
        {
            Result = new ImageLoadResult(new[] { NewImage(@"C:\a.png") }, Array.Empty<ImageLoadError>())
        };
        var useCase = new LoadImagesUseCase(loader);
        var collection = ImageCollection.Create();
        collection.Add(NewImage(@"C:\a.png")); // already present

        await useCase.ExecuteAsync(collection, new[] { @"C:\a.png" });

        Assert.Single(collection.Images);
    }

    private static Image NewImage(string path) =>
        Image.Create(System.IO.Path.GetFileName(path), path, Created, FileSize.FromBytes(1), null);
}

public class SerializeAndDeserializeUseCaseTests
{
    private sealed class FakeRepository : IImageCollectionRepository
    {
        public ImageCollection? SavedCollection { get; private set; }
        public string? SavedFilePath { get; private set; }
        public string? LoadedFilePath { get; private set; }

        public IReadOnlyList<string> SaveResult { get; set; } = Array.Empty<string>();

        public ImageCollection LoadResult { get; set; } = ImageCollection.Create();

        public Task<IReadOnlyList<string>> SaveAsync(ImageCollection imageCollection,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            SavedCollection = imageCollection;
            SavedFilePath = filePath;
            return Task.FromResult(SaveResult);
        }

        public Task<ImageCollection> LoadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            LoadedFilePath = filePath;
            return Task.FromResult(LoadResult);
        }
    }

    [Fact]
    public async Task Serialize_ForwardsCollectionAndPath_AndReturnsSkippedPaths()
    {
        var repository = new FakeRepository { SaveResult = new[] { @"C:\gone.png" } };
        var useCase = new SerializeCollectionUseCase(repository);
        var collection = ImageCollection.Create();

        var skipped = await useCase.ExecuteAsync(collection, @"C:\out.imgcollection");

        Assert.Same(collection, repository.SavedCollection);
        Assert.Equal(@"C:\out.imgcollection", repository.SavedFilePath);
        Assert.Equal(new[] { @"C:\gone.png" }, skipped);
    }

    [Fact]
    public async Task Deserialize_ForwardsPath_AndReturnsLoadedCollection()
    {
        var repository = new FakeRepository();
        var useCase = new DeserializeCollectionUseCase(repository);

        var loaded = await useCase.ExecuteAsync(@"C:\in.imgcollection");

        Assert.Equal(@"C:\in.imgcollection", repository.LoadedFilePath);
        Assert.Same(repository.LoadResult, loaded);
    }
}
