using ImageViewer.Application.Models;

namespace ImageViewer.Application.Interfaces;

public interface IImageLoaderService
{
    Task<ImageLoadResult> LoadImagesAsync(IReadOnlyList<string> filePath, CancellationToken cancellationToken);
}