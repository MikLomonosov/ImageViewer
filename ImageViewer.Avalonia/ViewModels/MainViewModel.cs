using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageViewer.Application.Interfaces;
using ImageViewer.Application.UseCases;
using ImageViewer.Domain.Aggregates;
using ImageViewer.Avalonia.ViewModels.Base;

namespace ImageViewer.Avalonia.ViewModels;

public sealed class MainViewModel : BaseViewModel
{
    private readonly LoadImagesUseCase _loadImageUseCase;
    private readonly IDialogService _dialogService;
    private readonly SerializeCollectionUseCase _serializeCollectionUseCase;
    private readonly DeserializeCollectionUseCase _deserializeCollectionUseCase;
    private readonly IFileDialogService _fileDialogService;
    private const string CollectionFileFilter = "Файлы коллекции|*.imgcollection|Все файлы|*.*";
    
    public ObservableCollection<ImageItemViewModel> Images { get; } = new();
    public bool HasImages => Images.Count > 0;
    private ImageCollection _imageCollection = ImageCollection.Create();

    private readonly RelayCommand _loadImagesCommand;
    public ICommand LoadImagesCommand => _loadImagesCommand;
    private readonly RelayCommand _serializeCollectionCommand;
    public ICommand SerializeCollectionCommand => _serializeCollectionCommand;
    private readonly RelayCommand _deserializeCollectionCommand;
    public ICommand DeserializeCollectionCommand => _deserializeCollectionCommand;
    private readonly RelayCommand _clearCommand;
    public ICommand ClearCommand => _clearCommand;
    
    
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RaiseAllCanExecuteChanged();
            }
        }
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
        
        Images.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasImages));
            RaiseAllCanExecuteChanged();
        };
        
        _loadImagesCommand = new RelayCommand(() => RunBusyAsync(LoadImagesAsync),
            () => !IsLoading);
        _serializeCollectionCommand = new RelayCommand( 
            () => RunBusyAsync(SerializeAsync, "Не удалось сохранить изображения"),
            () => !IsLoading && HasImages);
        _deserializeCollectionCommand = new RelayCommand( 
            () =>  RunBusyAsync(DeserializeAsync, "Не удалось загрузить изображения"), 
            () => !IsLoading);
        _clearCommand = new RelayCommand(Clear, () => !IsLoading && HasImages);
    }
    
    #endregion

    private async Task LoadImagesAsync()
    {
        var filePaths = await _fileDialogService.OpenImageFilesDialogAsync();

        if (filePaths.Count == 0)
            return;
        
        var result = await _loadImageUseCase.ExecuteAsync(_imageCollection, filePaths);
            
        foreach (var image in result.LoadedImages) 
            Images.Add(new ImageItemViewModel(image));

        RaiseAllCanExecuteChanged();

        if (result.Errors.Count > 0)
        {
            var message = string.Join(Environment.NewLine, 
                result.Errors.Select(e => $"{Path.GetFileName(e.FilePath)}: {e.Reason}"));
            
            await _dialogService.ShowErrorAsync($"Не удалось загрузить некоторые файлы: {Environment.NewLine}{message}");
        }
    }

    private async Task SerializeAsync()
    {
        var filePath = await _fileDialogService.SaveDocumentDialogAsync(
            fileName: $"collection_{DateTime.UtcNow:ddMMyyyy_HHmmss}.imgcollection",
            filter: CollectionFileFilter);

        if (filePath is null)
            return;
        
        var skippedOriginalPaths =  await _serializeCollectionUseCase.ExecuteAsync(_imageCollection, filePath);

        if (skippedOriginalPaths.Count > 0)
        {
            var names = string.Join(Environment.NewLine,
                skippedOriginalPaths.Select(Path.GetFileName));
            
            await _dialogService.ShowWarningAsync($"Изображения сохранены, но у некоторых из них " + 
                $"оригиналы данных не удалось прочитать (был удален или перемещен): {Environment.NewLine}{names}");
        }
        else
        {
            await _dialogService.ShowInfoAsync("Изображения успешно сохранены");
        }
    }

    private async Task DeserializeAsync()
    {
        var filePath = await _fileDialogService.OpenDocumentDialogAsync(CollectionFileFilter);
        
        if (filePath is null)
            return;
        
        var loadedCollection = await Task.Run(() => _deserializeCollectionUseCase.ExecuteAsync(filePath));

        _imageCollection = loadedCollection;
        
        Images.Clear();

        foreach (var image in _imageCollection.Images)
            Images.Add(new ImageItemViewModel(image));

        RaiseAllCanExecuteChanged();
    }

    private async Task Clear()
    {
        if (Images.Count == 0)
            return;

        var confirmed = await _dialogService.ConfirmAsync(
            "Несохраненные изображения будут потеряны. Очистить список изображений?",
            "Очистка списка изображений.");

        if (!confirmed)
            return;
        
        _imageCollection.Clear();
        Images.Clear();

        RaiseAllCanExecuteChanged();
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

            await _dialogService.ShowErrorAsync(message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void RaiseAllCanExecuteChanged()
    {
        _loadImagesCommand.RaiseCanExecuteChanged();
        _serializeCollectionCommand.RaiseCanExecuteChanged();
        _deserializeCollectionCommand.RaiseCanExecuteChanged();
        _clearCommand.RaiseCanExecuteChanged();
    }
}