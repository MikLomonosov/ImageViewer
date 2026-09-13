using ImageViewer.Application.Interfaces;
using Microsoft.Win32;

namespace ImageViewer.Wpf.Services;

public class FileDialogService : IFileDialogService
{
    public IReadOnlyList<string> OpenImageFilesDialog()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите изображение",
            Filter = "Изображения |*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы|*.*",
            Multiselect = true
        };
        
        return dialog.ShowDialog() == true 
            ? dialog.FileNames 
            : Array.Empty<string>();
    }

    public string? SaveDocumentDialog(string fileName, string filter)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Сохранить коллекцию",
            FileName = fileName,
            Filter = filter
        };
        
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? OpenDocumentDialog(string filter)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Открыть коллекцию",
            Filter = filter,
            Multiselect = true
        };
        
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}