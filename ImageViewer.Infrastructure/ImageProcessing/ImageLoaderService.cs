using System.IO;
using ImageViewer.Application.Configuration;
using ImageViewer.Application.Interfaces;
using ImageViewer.Application.Models;
using ImageViewer.Domain.Entities;
using ImageViewer.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace ImageViewer.Infrastructure.ImageProcessing;

public class ImageLoaderService : IImageLoaderService
{
    private readonly ImageLoadingLimits _limits;
    private readonly IThumbnailService _thumbnailService;
    
    #region constructors

    public ImageLoaderService(IOptions<ImageLoadingLimits> limits, IThumbnailService thumbnailService)
    {
        _limits = limits.Value;
        _thumbnailService = thumbnailService;
    }
    
    #endregion
    
    public async Task<ImageLoadResult> LoadImagesAsync(IReadOnlyList<string> filePaths, 
                                                        CancellationToken cancellationToken = default)
    {
        var loadedImages = new List<Image>();
        var errors = new List<ImageLoadError>();

        using var semaphore = new SemaphoreSlim(_limits.MaxDegreeOfParallelism);

        var tasks = filePaths.Select(async path =>
        {
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                return await LoadSingleImageAsync(path, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);

        foreach (var (image, error) in results)
        {
            if (image is not null)
                loadedImages.Add(image);
            else if (error is not null)
                errors.Add(error);
        }
        
        return new ImageLoadResult(loadedImages, errors);
    }

    private async Task<(Image?, ImageLoadError?)> LoadSingleImageAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            var fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
                return (null, new ImageLoadError(path, "Файл не найден."));

            if (fileInfo.Length > _limits.MaxFileSizeBytes)
            {
                var limitReadable = FileSize.FromBytes(_limits.MaxFileSizeBytes).ToString();

                return (null, new ImageLoadError(path, $"Файл превышает допустимый размер в {limitReadable}"));
            }

            var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);

            var decodedResult = await Task.Run(
                    () => _thumbnailService.DecodeAndCreateThumbnail(bytes),
                    cancellationToken)
                .ConfigureAwait(false);

            if (decodedResult is null)
                return (null, new ImageLoadError(path, "Не удалось распознать формат изображения."));

            var (dimensions, thumbnailBytes) = decodedResult.Value;
            var createdDateUtc = new DateTimeOffset(fileInfo.CreationTimeUtc, TimeSpan.Zero);

            var image = Image.Create(fileInfo.Name,
                                fileInfo.FullName,
                                createdDateUtc,
                                FileSize.FromBytes(fileInfo.Length),
                                dimensions);
            
            image.AttachThumbnail(thumbnailBytes);

            return (image, null);
        }
        catch (Exception exception)
        {
            return (null, new ImageLoadError(path, $"Ошибка загрузки: {exception.Message}"));
        }
    }
}