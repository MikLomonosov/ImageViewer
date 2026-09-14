using ImageViewer.Application.Interfaces;
using ImageViewer.Application.Models;
using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Repositories;

namespace ImageViewer.Application.UseCases;

public class SerializeCollectionUseCase
{
    private readonly IFileDialogService _fileDialogService;
    private readonly IImageCollectionRepository _imageCollectionRepository;
    
    #region constructors

    public SerializeCollectionUseCase(IFileDialogService fileDialogService,
                                        IImageCollectionRepository imageCollectionRepository)
    {
        _fileDialogService = fileDialogService;
        _imageCollectionRepository = imageCollectionRepository;
    }
    
    #endregion

    public async Task<SerializeResult> ExecuteAsync(ImageCollection imageCollection, CancellationToken cancellationToken = default)
    {
        var filePath = _fileDialogService.SaveDocumentDialog(
            fileName: $"collection_{DateTime.UtcNow:ddMMyyyy_HHmmss}.imgcollection",
            filter: "Файлы коллекции |*.imgcollection|Все файлы|*.*");

        if (filePath is null)
            return new SerializeResult(false, Array.Empty<string>());

        var skipped = await _imageCollectionRepository.SaveAsync(imageCollection, filePath, cancellationToken);
        
        return new SerializeResult(true, skipped);
    }
}