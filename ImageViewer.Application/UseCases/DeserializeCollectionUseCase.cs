using ImageViewer.Application.Interfaces;
using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Repositories;

namespace ImageViewer.Application.UseCases;

public class DeserializeCollectionUseCase
{
    private readonly IFileDialogService _fileDialogService;
    private readonly IImageCollectionRepository _imageCollectionRepository;

    #region constructors

    public DeserializeCollectionUseCase(IFileDialogService fileDialogService,
                                        IImageCollectionRepository imageCollectionRepository)
    {
        _fileDialogService = fileDialogService;
        _imageCollectionRepository = imageCollectionRepository;
    }

    #endregion

    public async Task<ImageCollection?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var filePath = _fileDialogService.OpenDocumentDialog("Файлы коллекции |*.imgcollection|Все файлы|*.*");
        
        if (filePath is null)
            return null;
        
        return await _imageCollectionRepository.LoadAsync(filePath, cancellationToken);
    }
    
}