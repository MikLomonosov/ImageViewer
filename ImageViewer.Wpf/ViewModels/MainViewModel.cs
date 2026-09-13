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
        
        LoadImagesCommand = new RelayCommand(async () => await LoadImagesAsync(), () => !IsLoading);
        SerializeCollectionCommand = new RelayCommand(async () => await SerializeAsync(), () => !IsLoading);
        DeserializeCollectionCommand = new RelayCommand(async () => await DeserializeAsync(), () => !IsLoading);
        ClearCommand = new RelayCommand(Clear, () => !IsLoading && Images.Count > 0);
    }
    
    #endregion

    private async Task LoadImagesAsync()
    {
        IsLoading = true;

        try
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
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SerializeAsync()
    {
        IsLoading = true;

        try
        {
            var saved = await _serializeCollectionUseCase.ExecuteAsync(_imageCollection);

            if (saved)
                _dialogService.ShowInfo("Изображения сохранены");
        }
        catch (Exception exception)
        {
            _dialogService.ShowError($"Иозбражения не удалось сохранить{exception.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DeserializeAsync()
    {
        IsLoading = true;

        try
        {
            var loadedCollection = await _deserializeCollectionUseCase.ExecuteAsync();

            if (loadedCollection is null)
                return;

            _imageCollection = loadedCollection;
            
            
            if (Images.Count != 0)
                Images.Clear();

            foreach (var image in _imageCollection.Images)
            {
                Images.Add(new ImageItemViewModel(image));
            }
        }
        catch (Exception exception)
        {
            _dialogService.ShowError($"Не удалось загрузить изображения {exception.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Clear()
    {
        if (Images.Count == 0)
            return;

        var confirmed = _dialogService.Confirm(
            "Несохраненные изображения будут потеряны. /n Очистить список изображений?",
            "Очистка списка изображений.");

        if (!confirmed)
            return;
        
        _imageCollection.Clear();
        Images.Clear();
    }
}