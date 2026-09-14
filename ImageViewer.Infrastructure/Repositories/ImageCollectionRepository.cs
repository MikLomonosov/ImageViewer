using System.IO;
using ImageViewer.Application.Interfaces;
using ImageViewer.Application.Mapping;
using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Repositories;

namespace ImageViewer.Infrastructure.Repositories;

public class ImageCollectionRepository : IImageCollectionRepository
{
    private readonly IReadOnlyList<IImageSerializer> _serializers;
    private const int StreamBufferSizeBytes = 81920; // 80 kb
    
    #region constructors

    public ImageCollectionRepository(IEnumerable<IImageSerializer> serializers)
    {
        _serializers = serializers.ToList();
        
        if (_serializers.Count == 0)
            throw new InvalidOperationException("Нет ни одного сериализатора");
    }
    
    #endregion
    
    public async Task<IReadOnlyList<string>> SaveAsync(ImageCollection imageCollection,
                                                        string filePath,
                                                        CancellationToken cancellationToken = default)
    {
        var serializer = ResolveSerializer(filePath);
        var skippedSourcePaths = new List<string>();
        var imageCollectionDto = ImageMapper.ToDto(imageCollection);

        foreach (var imageDto in imageCollectionDto.Images)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (imageDto.OriginalData is not null)
                continue;

            try
            {
                imageDto.OriginalData = await File.ReadAllBytesAsync(imageDto.Path, cancellationToken);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                skippedSourcePaths.Add(imageDto.Path);
            }
        }
        
        await using var stream = OpenStream(filePath, FileAccess.Write);
        
        await serializer.SerializeAsync(imageCollectionDto, stream, cancellationToken);
        
        return skippedSourcePaths;
    }

    public async Task<ImageCollection> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Файл не найден!", filePath);

        var serializer = ResolveSerializer(filePath);

        await using var stream = OpenStream(filePath, FileAccess.Read);

        var imageCollection = await serializer.DeserializeAsync(stream, cancellationToken);
        
        return ImageMapper.ToDomain(imageCollection);
    }

    private IImageSerializer ResolveSerializer(string filePath)
    {
        var extension = Path.GetExtension(filePath).TrimStart('.');
        
        return _serializers.FirstOrDefault(s => 
                                            string.Equals(s.FileExtension, extension, StringComparison.OrdinalIgnoreCase))
                                            ?? _serializers[0]; // fallback
    }

    private static FileStream OpenStream(string filePath, FileAccess access)
    {
        var mode = access == FileAccess.Write ? FileMode.Create : FileMode.Open;
        var share = access == FileAccess.Write ? FileShare.None : FileShare.Read;
        
        return new FileStream(filePath, 
                                mode, 
                                access, 
                                share,
                                bufferSize: StreamBufferSizeBytes,
                                useAsync:true);
    }
}