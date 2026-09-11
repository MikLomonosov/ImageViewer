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
    private readonly ImageCollection _imageCollection = ImageCollection.Create();
    
    public ObservableCollection<ImageItemViewModel> Images { get; } = new();
    
    public ICommand LoadImagesCommand { get; }
    
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    
    #region constructors

    public MainViewModel(LoadImagesUseCase loadImageUseCase, IDialogService dialogService)
    {
        _loadImageUseCase = loadImageUseCase;
        _dialogService = dialogService;
        
        LoadImagesCommand = new RelayCommand(async () => await LoadImagesAsync(), () => !IsLoading);
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
}