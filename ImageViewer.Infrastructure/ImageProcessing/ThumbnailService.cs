using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageViewer.Application.Interfaces;
using ImageViewer.Domain.ValueObjects;

namespace ImageViewer.Infrastructure.ImageProcessing;

public class ThumbnailService : IThumbnailService
{
    public ImageBinaryData CreateThumbnail(ImageBinaryData originalImageData, int maxWidth = 200)
    {
        var bytes = originalImageData.ToArray();
        
        using var inputStream = new MemoryStream(bytes);

        var decoder = BitmapDecoder.Create(inputStream,
                                            BitmapCreateOptions.None,
                                            BitmapCacheOption.OnLoad);
        
        var frame = decoder.Frames[0];
        
        var scale = (double)maxWidth / frame.PixelWidth;
        
        var thumbnail = new TransformedBitmap(frame, new ScaleTransform(scale, scale));
        thumbnail.Freeze();

        var encoder = new JpegBitmapEncoder { QualityLevel = 80 };
        encoder.Frames.Add(BitmapFrame.Create(thumbnail));

        using var outputStream = new MemoryStream();
        encoder.Save(outputStream);
        
        return ImageBinaryData.CreateFromBytes(outputStream.ToArray());
    }
}