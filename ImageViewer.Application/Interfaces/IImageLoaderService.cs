using ImageViewer.Application.Models;

namespace ImageViewer.Application.Interfaces;

public interface IImageLoaderService
{
    Task<ImageLoadResult> LoadImagesAsync(IReadOnlyCollection<string> filePath, CancellationToken cancellationToken);
}