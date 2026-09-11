using ImageViewer.Domain.ValueObjects;

namespace ImageViewer.Application.Interfaces;

public interface IThumbnailService
{
    ImageBinaryData CreateThumbnail(ImageBinaryData originalImageData, int maxWidth = 200);
}