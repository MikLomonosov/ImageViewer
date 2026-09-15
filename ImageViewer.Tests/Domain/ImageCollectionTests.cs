using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Entities;
using ImageViewer.Domain.ValueObjects;
using Xunit;

namespace ImageViewer.Tests.Domain;

public class ImageCollectionTests
{
    private static readonly DateTimeOffset Created = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_InitializesEmptyCollection()
    {
        var collection = ImageCollection.Create();

        Assert.NotEqual(Guid.Empty, collection.Id);
        Assert.Equal(0, collection.Count);
        Assert.Empty(collection.Images);
    }

    [Fact]
    public void Add_NewImage_AddsToCollection()
    {
        var collection = ImageCollection.Create();

        collection.Add(NewImage(@"C:\a.png"));

        Assert.Single(collection.Images);
        Assert.Equal(1, collection.Count);
    }

    [Fact]
    public void Add_WithNull_Throws()
    {
        var collection = ImageCollection.Create();

        Assert.Throws<ArgumentNullException>(() => collection.Add(null!));
    }

    [Fact]
    public void Add_SamePathTwice_Throws()
    {
        var collection = ImageCollection.Create();
        collection.Add(NewImage(@"C:\a.png"));

        Assert.Throws<InvalidOperationException>(() => collection.Add(NewImage(@"C:\a.png")));
    }

    [Fact]
    public void Add_PathComparison_IsCaseInsensitive()
    {
        var collection = ImageCollection.Create();
        collection.Add(NewImage(@"C:\a.png"));

        Assert.Throws<InvalidOperationException>(() => collection.Add(NewImage(@"C:\A.PNG")));
    }

    [Fact]
    public void AddRange_SkipsDuplicates_AndReturnsOnlyAdded()
    {
        var collection = ImageCollection.Create();
        collection.Add(NewImage(@"C:\a.png"));

        var added = collection.AddRange(new[]
        {
            NewImage(@"C:\a.png"),   // duplicate path -> silently skipped
            NewImage(@"C:\b.png")
        });

        Assert.Single(added);
        Assert.Equal(2, collection.Count);
    }

    [Fact]
    public void Clear_RemovesAllImages_AndResetsPathIndex()
    {
        var collection = ImageCollection.Create();
        collection.Add(NewImage(@"C:\a.png"));
        collection.Add(NewImage(@"C:\b.png"));

        collection.Clear();

        Assert.Empty(collection.Images);

        // the same path can be added again after Clear
        collection.Add(NewImage(@"C:\a.png"));
        Assert.Single(collection.Images);
    }

    [Fact]
    public void Restore_AcceptsDuplicatePaths_WithoutThrowing()
    {
        // Rehydration bypasses invariants on purpose.
        var images = new[] { NewImage(@"C:\a.png"), NewImage(@"C:\a.png") };

        var collection = ImageCollection.Restore(Guid.NewGuid(), Created, images);

        Assert.Equal(2, collection.Images.Count);
    }

    [Fact]
    public void Restore_RestoresIdentityAndTimestamp()
    {
        var id = Guid.NewGuid();

        var collection = ImageCollection.Restore(id, Created, new[] { NewImage(@"C:\a.png") });

        Assert.Equal(id, collection.Id);
        Assert.Equal(Created, collection.CratedAtUtc);
        Assert.Single(collection.Images);
    }

    private static Image NewImage(string path) =>
        Image.Create(System.IO.Path.GetFileName(path), path, Created, FileSize.FromBytes(1), null);
}
