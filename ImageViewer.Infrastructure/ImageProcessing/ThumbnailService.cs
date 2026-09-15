using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageViewer.Application.Interfaces;
using ImageViewer.Domain.ValueObjects;

namespace ImageViewer.Infrastructure.ImageProcessing;

public class ThumbnailService : IThumbnailService
{
    public (ImageDimensions Dimensions, ImageBinaryData ThumbnailData)? DecodeAndCreateThumbnail(byte[] originalData, int maxWidth = 200)
    {
        try
        {
            using var stream = new MemoryStream(originalData);

            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            var frame = decoder.Frames[0];
            var dimensions = ImageDimensions.Create(frame.PixelWidth, frame.PixelHeight);
            var scale = Math.Min(1.0, (double)maxWidth / frame.PixelWidth);

            var thumbnail = new TransformedBitmap(frame, new ScaleTransform(scale, scale));
            thumbnail.Freeze();

            var encoder = new JpegBitmapEncoder { QualityLevel = 80 };
            encoder.Frames.Add(BitmapFrame.Create(thumbnail));

            using var outputStream = new MemoryStream();
            encoder.Save(outputStream);

            return (dimensions, ImageBinaryData.CreateFromBytes(outputStream.ToArray()));
        }
        catch
        {
            return null;
        }
    }
}