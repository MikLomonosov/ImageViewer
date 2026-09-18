using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ImageViewer.Application.Interfaces;

namespace ImageViewer.Avalonia.Services;

public class FileDialogService : IFileDialogService
{
    private readonly Window _owner;
    
    #region constructors

    public FileDialogService(Window owner)
    {
        _owner = owner;
    }
    
    #endregion
    
    public async Task<IReadOnlyList<string>> OpenImageFilesDialogAsync()
    {
        var storageProvider = _owner.StorageProvider;

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите изображения",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Изображения")
                {
                    Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" }
                }
            }
        });
        
        return files.Select(f => f.Path.LocalPath).ToList();
    }

    public async Task<string?> SaveDocumentDialogAsync(string fileName, string filter)
    {
        var storageProvider = _owner.StorageProvider;

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить изображения",
            SuggestedFileName = fileName,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Файлы коллекции") { Patterns = new[] { "*.imgcollection" } },
                new FilePickerFileType("Все файлы") { Patterns = new[] { "*.*" } },
            }
        });
        
        return file?.Path.LocalPath;
    }

    public async Task<string?> OpenDocumentDialogAsync(string filter)
    {
        var storageProvider = _owner.StorageProvider;

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Открыть коллекцию",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Файлы коллекции") { Patterns = new[] { "*.imgcollection" } },
                new FilePickerFileType("Все файлы") { Patterns = new[] { "*.*" } },
            }
        });
        
        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }
}