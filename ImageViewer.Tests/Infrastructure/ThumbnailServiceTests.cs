using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageViewer.Infrastructure.ImageProcessing;
using Xunit;

namespace ImageViewer.Tests.Infrastructure;

public class ThumbnailServiceTests
{
    private readonly ThumbnailService _service = new();

    [Fact]
    public void DecodeAndCreateThumbnail_WithValidPng_ReturnsDimensionsAndThumbnail()
    {
        var result = _service.DecodeAndCreateThumbnail(TestPng.Create(64, 32));

        Assert.NotNull(result);
        Assert.Equal(64, result!.Value.Dimensions.Width);
        Assert.Equal(32, result.Value.Dimensions.Height);
        Assert.True(result.Value.ThumbnailData.AsMemory().Length > 0);
    }

    [Fact]
    public void DecodeAndCreateThumbnail_DoesNotUpscaleSmallImages()
    {
        // Regression: scale used to be maxWidth / PixelWidth without a clamp,
        // upscaling images narrower than maxWidth (200 px).
        var result = _service.DecodeAndCreateThumbnail(TestPng.Create(1, 1));

        Assert.NotNull(result);
        Assert.Equal(1, result!.Value.Dimensions.Width);

        var thumbnailFrame = DecodeFrame(result.Value.ThumbnailData.ToArray());

        Assert.Equal(1, thumbnailFrame.PixelWidth);
        Assert.Equal(1, thumbnailFrame.PixelHeight);
    }

    [Fact]
    public void DecodeAndCreateThumbnail_DownscalesWideImages()
    {
        var result = _service.DecodeAndCreateThumbnail(TestPng.Create(400, 200), maxWidth: 200);

        Assert.NotNull(result);

        var thumbnailFrame = DecodeFrame(result!.Value.ThumbnailData.ToArray());

        Assert.Equal(200, thumbnailFrame.PixelWidth);
        Assert.Equal(100, thumbnailFrame.PixelHeight);
    }

    [Fact]
    public void DecodeAndCreateThumbnail_WithGarbageBytes_ReturnsNull()
    {
        Assert.Null(_service.DecodeAndCreateThumbnail(new byte[] { 1, 2, 3, 4, 5 }));
    }

    private static BitmapFrame DecodeFrame(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);

        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

        return decoder.Frames[0];
    }
}

internal static class TestPng
{
    public static byte[] Create(int width, int height)
    {
        var pixels = new byte[width * height * 4];

        var source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, pixels, width * 4);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(source));

        using var stream = new MemoryStream();
        encoder.Save(stream);

        return stream.ToArray();
    }
}
