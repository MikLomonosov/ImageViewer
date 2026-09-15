using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using ImageViewer.Application.Interfaces;
using ImageViewer.Application.UseCases;
using ImageViewer.Domain.Aggregates;
using ImageViewer.Wpf.ViewModels.Base;

namespace ImageViewer.Wpf.ViewModels;

public sealed class MainViewModel : BaseViewModel
{
    private readonly LoadImagesUseCase _loadImageUseCase;
    private readonly IDialogService _dialogService;
    private readonly SerializeCollectionUseCase _serializeCollectionUseCase;
    private readonly DeserializeCollectionUseCase _deserializeCollectionUseCase;
    private readonly IFileDialogService _fileDialogService;
    private const string CollectionFileFilter = "Файлы коллекции|*.imgcollection|Все файлы|*.*";
    
    public ObservableCollection<ImageItemViewModel> Images { get; } = new();
    private ImageCollection _imageCollection = ImageCollection.Create();
    
    public ICommand LoadImagesCommand { get; }
    public ICommand SerializeCollectionCommand { get; }
    public ICommand DeserializeCollectionCommand { get; }
    public ICommand ClearCommand { get; }
    
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    
    #region constructors

    public MainViewModel(LoadImagesUseCase loadImageUseCase, 
                        IDialogService dialogService,
                        IFileDialogService fileDialogService,
                        SerializeCollectionUseCase serializeCollectionUseCase,
                        DeserializeCollectionUseCase deserializeCollectionUseCase)
    {
        _loadImageUseCase = loadImageUseCase;
        _dialogService = dialogService;
        _serializeCollectionUseCase = serializeCollectionUseCase;
        _deserializeCollectionUseCase = deserializeCollectionUseCase;
        _fileDialogService = fileDialogService;
        
        LoadImagesCommand = new RelayCommand(() => RunBusyAsync(LoadImagesAsync),
            () => !IsLoading);
        SerializeCollectionCommand = new RelayCommand( 
            () => RunBusyAsync(SerializeAsync, "Не удалось сохранить изображения"),
            () => !IsLoading &&  Images.Count > 0);
        DeserializeCollectionCommand = new RelayCommand( 
            () =>  RunBusyAsync(DeserializeAsync, "Не удалось загрузить изображения"), 
            () => !IsLoading);
        ClearCommand = new RelayCommand(Clear, () => !IsLoading && Images.Count > 0);
    }
    
    #endregion

    private async Task LoadImagesAsync()
    {
        var filePaths = _fileDialogService.OpenImageFilesDialog();

        if (filePaths.Count == 0)
            return;
        
        var result = await _loadImageUseCase.ExecuteAsync(_imageCollection, filePaths);
            
        foreach (var image in result.LoadedImages) 
            Images.Add(new ImageItemViewModel(image));

        if (result.Errors.Count > 0)
        {
            var message = string.Join(Environment.NewLine, 
                result.Errors.Select(e => $"{Path.GetFileName(e.FilePath)}: {e.Reason}"));
            
            _dialogService.ShowWarning($"Не удалось загрузить некоторые файлы: {Environment.NewLine}{message}");
        }
    }

    private async Task SerializeAsync()
    {
        var filePath = _fileDialogService.SaveDocumentDialog(
            fileName: $"collection_{DateTime.UtcNow:ddMMyyyy_HHmmss}.imgcollection",
            filter: CollectionFileFilter);

        if (filePath is null)
            return;
        
        var skippedOriginalPaths =  await _serializeCollectionUseCase.ExecuteAsync(_imageCollection, filePath);

        if (skippedOriginalPaths.Count > 0)
        {
            var names = string.Join(Environment.NewLine,
                skippedOriginalPaths.Select(Path.GetFileName));
            
            _dialogService.ShowWarning($"Изображения сохранены, но у некоторых из них " + 
                $"оригиналы данных не удалось прочитать (был удален или перемещен): {Environment.NewLine}{names}");
        }
        else
        {
            _dialogService.ShowInfo("Изображения успешно сохранены");
        }
    }

    private async Task DeserializeAsync()
    {
        var filePath = _fileDialogService.OpenDocumentDialog(CollectionFileFilter);
        
        if (filePath is null)
            return;
        
        var loadedCollection = await Task.Run(() => _deserializeCollectionUseCase.ExecuteAsync(filePath));

        _imageCollection = loadedCollection;
        
        Images.Clear();

        foreach (var image in _imageCollection.Images)
            Images.Add(new ImageItemViewModel(image));
    }

    private void Clear()
    {
        if (Images.Count == 0)
            return;

        var confirmed = _dialogService.Confirm(
            "Несохраненные изображения будут потеряны. Очистить список изображений?",
            "Очистка списка изображений.");

        if (!confirmed)
            return;
        
        _imageCollection.Clear();
        Images.Clear();
    }

    private async Task RunBusyAsync(Func<Task> work, string? errorMessagePrefix = null)
    {
        IsLoading = true;

        try
        {
            await work();
        }
        catch (Exception exception)
        {
            var message = errorMessagePrefix is null
                ? exception.Message
                : $"{errorMessagePrefix}: {exception.Message}";

            _dialogService.ShowError(message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}