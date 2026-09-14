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
        
        if (serializers.Count() == 0)
            throw new InvalidOperationException("Нет ни одного сериализатора");
    }
    
    #endregion
    
    public async Task SaveAsync(ImageCollection imageCollection,
                                string filePath,
                                CancellationToken cancellationToken = default)
    {
        var serializer = ResolveSerializer(filePath);
        
        var imageCollectionDto = ImageMapper.ToDto(imageCollection);
        
        await using var stream = OpenStream(filePath, FileAccess.Write);
        
        await serializer.SerializeAsync(imageCollectionDto, stream, cancellationToken);
    }

    public async Task<ImageCollection> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Файл не найден!", filePath);

        var serializer = ResolveSerializer(filePath);

        await using var stream = OpenStream(filePath, FileAccess.Read);

        var imageCollection = await serializer.DeserializeAsync(stream, cancellationToken);
        
        var iamges = imageCollection.Images.Select(ImageMapper.ToDomain);
        
        return ImageCollection.Restore(imageCollection.Id, imageCollection.CreatedDateUtc,  iamges);
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