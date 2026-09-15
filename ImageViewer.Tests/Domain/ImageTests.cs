using ImageViewer.Domain.Entities;
using ImageViewer.Domain.ValueObjects;
using Xunit;

namespace ImageViewer.Tests.Domain;

public class ImageTests
{
    private static readonly DateTimeOffset Created = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidArguments_CreatesImage()
    {
        var image = Image.Create("cat.png", @"C:\images\cat.png", Created,
            FileSize.FromBytes(1024), ImageDimensions.Create(640, 480));

        Assert.NotEqual(Guid.Empty, image.Id);
        Assert.Equal("cat.png", image.Name);
        Assert.Equal(@"C:\images\cat.png", image.Path);
        Assert.Equal(Created, image.CreatedDateUtc);
        Assert.Equal(1024, image.Size.Bytes);
        Assert.Equal(640, image.Dimensions!.Width);
        Assert.Equal(480, image.Dimensions!.Height);
        Assert.Null(image.Thumbnail);
        Assert.Null(image.OriginalData);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_WithEmptyName_Throws(string? name)
    {
        Assert.Throws<ArgumentException>(() =>
            Image.Create(name!, @"C:\images\cat.png", Created, FileSize.FromBytes(1), null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPath_Throws(string? path)
    {
        Assert.Throws<ArgumentException>(() =>
            Image.Create("cat.png", path!, Created, FileSize.FromBytes(1), null));
    }

    [Fact]
    public void Create_WithNonUtcDate_Throws()
    {
        var local = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.FromHours(3));

        Assert.Throws<ArgumentException>(() =>
            Image.Create("cat.png", @"C:\images\cat.png", local, FileSize.FromBytes(1), null));
    }

    [Fact]
    public void Create_WithoutDimensions_AllowsNullDimensions()
    {
        var image = Image.Create("cat.png", @"C:\images\cat.png", Created, FileSize.FromBytes(1), null);

        Assert.Null(image.Dimensions);
    }

    [Fact]
    public void Restore_RestoresAllFields_IncludingBinaryData()
    {
        var id = Guid.NewGuid();
        var thumbnail = ImageBinaryData.CreateFromBytes(new byte[] { 1, 2, 3 });
        var original = ImageBinaryData.CreateFromBytes(new byte[] { 4, 5, 6 });

        var image = Image.Restore(id, "cat.png", @"C:\images\cat.png", Created,
            FileSize.FromBytes(10), ImageDimensions.Create(10, 10), thumbnail, original);

        Assert.Equal(id, image.Id);
        Assert.Equal("cat.png", image.Name);
        Assert.Same(thumbnail, image.Thumbnail);
        Assert.Same(original, image.OriginalData);
    }

    [Fact]
    public void AttachThumbnail_WithNull_Throws()
    {
        var image = NewImage();

        Assert.Throws<ArgumentNullException>(() => image.AttachThumbnail(null!));
    }

    [Fact]
    public void AttachOriginalData_WithNull_Throws()
    {
        var image = NewImage();

        Assert.Throws<ArgumentNullException>(() => image.AttachOriginalData(null!));
    }

    private static Image NewImage() =>
        Image.Create("cat.png", @"C:\images\cat.png", Created, FileSize.FromBytes(1), null);
}
