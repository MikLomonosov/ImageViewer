using System.IO;
using ImageViewer.Application.DTO;
using ImageViewer.Application.Interfaces;
using ImageViewer.Application.Mapping;
using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Repositories;

namespace ImageViewer.Infrastructure.Repositories;

public class ImageCollectionRepository : IImageCollectionRepository
{
    private readonly IReadOnlyList<IImageSerializer> _serializers;
    
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

        var imageCollectionDto = new ImageCollectionDto()
        {
            Id = imageCollection.Id,
            CreatedDateUtc = imageCollection.CratedAtUtc,
            Images = imageCollection.Images.Select(ImageMapper.ToDto).ToList()
        };
        
        await using var stream = new FileStream(filePath, FileMode.Create,
                                                FileAccess.Write, FileShare.None,
                                                bufferSize: 81920, useAsync:true);
        
        await serializer.SerializeAsync(imageCollectionDto, stream, cancellationToken);
    }

    public async Task<ImageCollection> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Файл не найден!", filePath);

        var serializer = ResolveSerializer(filePath);

        await using var stream = new FileStream(filePath, FileMode.Open,
                                                FileAccess.Read, FileShare.Read,
                                                bufferSize: 81920, useAsync: true);

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
}