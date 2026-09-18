using ImageViewer.Application.Interfaces;
using Microsoft.Win32;

namespace ImageViewer.Wpf.Services;

public class FileDialogService : IFileDialogService
{
    
    public Task<IReadOnlyList<string>> OpenImageFilesDialogAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите изображение",
            Filter = "Изображения |*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы|*.*",
            Multiselect = true
        };
        
        IReadOnlyList<string> result = dialog.ShowDialog() == true 
                                        ? dialog.FileNames 
                                        : Array.Empty<string>();
        
        return Task.FromResult(result);
    }

    public Task<string?> SaveDocumentDialogAsync(string fileName, string filter)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Сохранить коллекцию",
            FileName = fileName,
            Filter = filter
        };
        
        var result = dialog.ShowDialog() == true ? dialog.FileName : null;
        
        return Task.FromResult(result);
    }

    public Task<string?> OpenDocumentDialogAsync(string filter)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Открыть коллекцию",
            Filter = filter,
            Multiselect = true
        };
        
        var result = dialog.ShowDialog() == true ? dialog.FileName : null;
        
        return Task.FromResult(result);
    }
}