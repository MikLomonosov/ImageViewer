using ImageViewer.Application.Interfaces;
using ImageViewer.Application.Models;
using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Entities;

namespace ImageViewer.Application.UseCases;

public sealed class LoadImagesUseCase
{
    private readonly IFileDialogService _fileDialogService;
    private readonly IImageLoaderService _imageLoaderService;
    
    #region constructors

    public LoadImagesUseCase(IFileDialogService fileDialogService, IImageLoaderService imageLoaderService)
    {
        _fileDialogService = fileDialogService;
        _imageLoaderService = imageLoaderService;
    }

    #endregion

    public async Task<ImageLoadResult> ExecuteAsync(ImageCollection collection, CancellationToken cancellationToken=default)
    {
        var filePaths = _fileDialogService.OpenImageFilesDialog();

        if (filePaths.Count == 0)
            return new ImageLoadResult(Array.Empty<Image>(), Array.Empty<ImageLoadError>());

        var result = await _imageLoaderService.LoadImagesAsync(filePaths, cancellationToken);
        collection.AddRange(result.LoadedImages);

        return result;
    }
}