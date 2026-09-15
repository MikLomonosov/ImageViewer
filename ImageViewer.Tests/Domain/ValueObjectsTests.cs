using ImageViewer.Domain.ValueObjects;
using Xunit;

namespace ImageViewer.Tests.Domain;

public class FileSizeTests
{
    [Fact]
    public void FromBytes_WithNegativeValue_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => FileSize.FromBytes(-1));

    [Fact]
    public void FromBytes_WithZero_IsAllowed()
    {
        Assert.Equal(0, FileSize.FromBytes(0).Bytes);
    }

    [Fact]
    public void Units_AreComputedCorrectly()
    {
        var size = FileSize.FromBytes(2048);

        Assert.Equal(2.0, size.Kilobytes);
        Assert.Equal(2048.0 / (1024.0 * 1024.0), size.Megabytes, 10);
    }

    [Theory]
    [InlineData(512, "Б")]
    [InlineData(2048, "КБ")]
    [InlineData(5 * 1024 * 1024, "МБ")]
    public void ToString_UsesSuitableUnit(long bytes, string expectedUnit)
    {
        Assert.Contains(expectedUnit, FileSize.FromBytes(bytes).ToString());
    }

    [Fact]
    public void FileSizes_HaveValueEquality()
    {
        Assert.Equal(FileSize.FromBytes(100), FileSize.FromBytes(100));
        Assert.NotEqual(FileSize.FromBytes(100), FileSize.FromBytes(200));
    }
}

public class ImageDimensionsTests
{
    [Fact]
    public void Create_WithValidDimensions_Succeeds()
    {
        var dimensions = ImageDimensions.Create(640, 480);

        Assert.Equal(640, dimensions.Width);
        Assert.Equal(480, dimensions.Height);
        Assert.Equal("640x480", dimensions.ToString());
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(100, 0)]
    [InlineData(-1, 100)]
    [InlineData(100, -1)]
    public void Create_WithInvalidDimensions_Throws(int width, int height) =>
        Assert.Throws<ArgumentException>(() => ImageDimensions.Create(width, height));

    [Fact]
    public void Dimensions_HaveValueEquality()
    {
        Assert.Equal(ImageDimensions.Create(10, 20), ImageDimensions.Create(10, 20));
        Assert.NotEqual(ImageDimensions.Create(10, 20), ImageDimensions.Create(20, 10));
    }
}

public class ImageBinaryDataTests
{
    [Fact]
    public void CreateFromBytes_WithNull_Throws() =>
        Assert.Throws<ArgumentNullException>(() => ImageBinaryData.CreateFromBytes(null!));

    [Fact]
    public void CreateFromBytes_WithEmptyArray_Throws() =>
        Assert.Throws<ArgumentNullException>(() => ImageBinaryData.CreateFromBytes(Array.Empty<byte>()));

    [Fact]
    public void AsMemory_ReturnsUnderlyingContent()
    {
        var data = ImageBinaryData.CreateFromBytes(new byte[] { 1, 2, 3 });

        Assert.True(new byte[] { 1, 2, 3 }.AsSpan().SequenceEqual(data.AsMemory().Span));
    }

    [Fact]
    public void ToArray_ReturnsIndependentCopy()
    {
        var data = ImageBinaryData.CreateFromBytes(new byte[] { 1, 2, 3 });

        var copy = data.ToArray();
        copy[0] = 99;

        Assert.Equal(1, data.AsMemory().Span[0]);
    }

    [Fact]
    public void OpenRead_ReturnsReadableStreamWithContent()
    {
        var data = ImageBinaryData.CreateFromBytes(new byte[] { 1, 2, 3 });

        using var stream = data.OpenRead();

        Assert.Equal(3L, stream.Length);
        Assert.Equal(1, stream.ReadByte());
    }

    [Fact]
    public void Equals_ComparesContentNotReference()
    {
        Assert.Equal(
            ImageBinaryData.CreateFromBytes(new byte[] { 1, 2, 3 }),
            ImageBinaryData.CreateFromBytes(new byte[] { 1, 2, 3 }));
        Assert.NotEqual(
            ImageBinaryData.CreateFromBytes(new byte[] { 1, 2, 3 }),
            ImageBinaryData.CreateFromBytes(new byte[] { 3, 2, 1 }));
    }
}
