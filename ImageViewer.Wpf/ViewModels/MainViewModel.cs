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
                        SerializeCollectionUseCase serializeCollectionUseCase,
                        DeserializeCollectionUseCase deserializeCollectionUseCase)
    {
        _loadImageUseCase = loadImageUseCase;
        _dialogService = dialogService;
        _serializeCollectionUseCase = serializeCollectionUseCase;
        _deserializeCollectionUseCase = deserializeCollectionUseCase;
        
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
        var result = await _loadImageUseCase.ExecuteAsync(_imageCollection);
            
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
        var result =  await _serializeCollectionUseCase.ExecuteAsync(_imageCollection);
        
        if (!result.Saved)
            return;

        if (result.SkippedOriginalPaths.Count > 0)
        {
            var names = string.Join(Environment.NewLine,
                result.SkippedOriginalPaths.Select(Path.GetFileName));
            
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
        var loadedCollection = await _deserializeCollectionUseCase.ExecuteAsync();

        if (loadedCollection is null)
            return;

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
            "Несохраненные изображения будут потеряны."+
            $"Очистить список изображений?: {Environment.NewLine}",
            "Очистка списка изображений.");

        if (!confirmed)
            return;
        
        _imageCollection.Clear();
        Images.Clear();
    }

    private async Task RunBusyAsync(Func<Task> work, string? errorMessagePrefix = null)
    {
        IsLoading =  true;

        try
        {
            await work();
        }
        catch (Exception exception)
        {
            _dialogService.ShowError($"{errorMessagePrefix}: {exception.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}