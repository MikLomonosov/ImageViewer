using ImageViewer.Domain.ValueObjects;

namespace ImageViewer.Application.Interfaces;

public interface IThumbnailService
{
    (ImageDimensions Dimensions, ImageBinaryData ThumbnailData)? DecodeAndCreateThumbnail(byte[] originalData, int maxWidth = 200);
}