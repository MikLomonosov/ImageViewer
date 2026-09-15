using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Repositories;

namespace ImageViewer.Application.UseCases;

public sealed class DeserializeCollectionUseCase
{
    private readonly IImageCollectionRepository _imageCollectionRepository;

    #region constructors

    public DeserializeCollectionUseCase(IImageCollectionRepository imageCollectionRepository)
    {
        _imageCollectionRepository = imageCollectionRepository;
    }

    #endregion

    public Task<ImageCollection> ExecuteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        
        return _imageCollectionRepository.LoadAsync(filePath, cancellationToken);
    }
    
}