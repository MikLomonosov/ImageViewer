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
}