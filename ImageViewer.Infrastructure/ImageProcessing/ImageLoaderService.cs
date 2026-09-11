using System.IO;
using System.Windows.Media.Imaging;
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

            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);

            var dimensions = TryGetDimensions(bytes);
            if (dimensions is null)
                return (null, new ImageLoadError(path, "Не удалось распознать формат изображения."));

            var createdAtUtc = new DateTimeOffset(fileInfo.CreationTimeUtc, TimeSpan.Zero);

            var image = Image.Create(fileInfo.Name,
                                fileInfo.FullName,
                                    createdAtUtc,
                                    FileSize.FromBytes(fileInfo.Length),
                                    dimensions);
            
            var originalImageData = ImageBinaryData.CreateFromBytes(bytes);
            image.AttachOriginalData(originalImageData);

            var thumbnailData = _thumbnailService.CreateThumbnail(originalImageData);
            image.AttachThumbnail(thumbnailData);

            return (image, null);
        }
        catch (Exception exception)
        {
            return (null, new ImageLoadError(path, $"Ошибка загрузки: {exception.Message}"));
        }
    }

    private static ImageDimensions? TryGetDimensions(byte[] bytes)
    {
        try
        {
            using var stream = new MemoryStream(bytes);

            var decoder = BitmapDecoder.Create(stream,
                                                BitmapCreateOptions.DelayCreation, // decode only header
                                                BitmapCacheOption.None);
            
            var frame = decoder.Frames[0];
            
            return ImageDimensions.Create(frame.PixelWidth, frame.PixelHeight);
        }
        catch
        {
            return null;
        }
    }
}