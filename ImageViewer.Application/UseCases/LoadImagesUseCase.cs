using ImageViewer.Application.Interfaces;
using ImageViewer.Application.Models;
using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Entities;

namespace ImageViewer.Application.UseCases;

public sealed class LoadImagesUseCase
{
    private readonly IImageLoaderService _imageLoaderService;
    
    #region constructors

    public LoadImagesUseCase(IImageLoaderService imageLoaderService)
    {
        _imageLoaderService = imageLoaderService;
    }

    #endregion

    public async Task<ImageLoadResult> ExecuteAsync(ImageCollection collection,
                                                    IReadOnlyList<string> filePaths,
                                                    CancellationToken cancellationToken=default)
    {
        if (filePaths.Count == 0)
            return new ImageLoadResult(Array.Empty<Image>(), Array.Empty<ImageLoadError>());

        var result = await _imageLoaderService.LoadImagesAsync(filePaths, cancellationToken);
        collection.AddRange(result.LoadedImages);

        return result;
    }
}