using ImageViewer.Application.Interfaces;
using ImageViewer.Domain.ValueObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ImageViewer.Infrastructure.ImageProcessing;

public class ThumbnailService : IThumbnailService
{
    public (ImageDimensions Dimensions, ImageBinaryData ThumbnailData)? DecodeAndCreateThumbnail(byte[] originalData, int maxWidth = 200)
    {
        try
        {
            using var image = Image.Load(originalData);
            
            var dimensions = ImageDimensions.Create(image.Width, image.Height);

            var scale = Math.Min(1.0, (double)maxWidth / image.Width);
            var thumbnailHeight = (int)(image.Height * scale);
            
            image.Mutate(x => x.Resize(maxWidth, thumbnailHeight));
            
            using var outputStream = new MemoryStream();
            image.Save(outputStream, new JpegEncoder { Quality = 80 });

            return (dimensions, ImageBinaryData.CreateFromBytes(outputStream.ToArray()));
        }
        catch
        {
            return null;
        }
    }
}