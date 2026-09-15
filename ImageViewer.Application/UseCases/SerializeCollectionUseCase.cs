using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Repositories;

namespace ImageViewer.Application.UseCases;

public sealed class SerializeCollectionUseCase
{
    private readonly IImageCollectionRepository _imageCollectionRepository;
    
    #region constructors

    public SerializeCollectionUseCase(IImageCollectionRepository imageCollectionRepository)
    {
        _imageCollectionRepository = imageCollectionRepository;
    }
    
    #endregion

    public Task<IReadOnlyList<string>> ExecuteAsync(ImageCollection imageCollection,
                                                    string filePath,
                                                    CancellationToken cancellationToken = default)
    {
        return _imageCollectionRepository.SaveAsync(imageCollection, filePath, cancellationToken);
    }
}